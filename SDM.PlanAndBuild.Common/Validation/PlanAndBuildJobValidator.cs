namespace Skyline.DataMiner.SDM.PlanAndBuild.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.InfraOps.Common.Validation;
    using Skyline.DataMiner.SDM.PlanAndBuild.Helpers;
    using Skyline.DataMiner.SDM.PlanAndBuild.Models;
    using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    using static Skyline.DataMiner.SDM.PlanAndBuild.Validation.PlanAndBuildJobValidationHandler;

    /// <summary>
    /// Public validator service for PlanAndBuildJob validation, including data access for JobName uniqueness checks.
    /// </summary>
    public class PlanAndBuildJobValidator : ValidatorBase<PlanAndBuildJob>
    {
        private readonly IPlanAndBuildApiHelper _helper;
        private readonly IPeopleAndOrganizationsApi _peopleApi;
        private readonly IPlanAndBuildExternalReferenceChecker _externalReferenceChecker;
        private readonly Validator<PlanAndBuildJob> _validationPipeline;
        private readonly Validator<PlanAndBuildJob> _creationValidationPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanAndBuildJobValidator"/> class.
        /// </summary>
        /// <param name="helper">
        /// The Plan &amp; Build API helper used to query existing Jobs for uniqueness checks.
        /// Note: this is captured by reference during <see cref="PlanAndBuildApiHelper"/> construction, before
        /// its repositories are wired up. Only <see cref="Validate"/>/<see cref="ValidateAndThrow"/> (called
        /// after construction completes) access <paramref name="helper"/>'s repositories.
        /// </param>
        /// <param name="peopleApi">
        /// The People &amp; Organizations API used to validate the existence of Person/Team references in Jobs.
        /// Kept separate from the helper so validation does not require <see cref="IPeopleAndOrganizationsApi"/>
        /// to be part of the public <see cref="IPlanAndBuildApiHelper"/> contract.
        /// </param>
        public PlanAndBuildJobValidator(
            IPlanAndBuildApiHelper helper,
            IPeopleAndOrganizationsApi peopleApi,
            IPlanAndBuildExternalReferenceChecker externalReferenceChecker = null)
        {
            _helper = helper ?? throw new ArgumentNullException(nameof(helper));
            _peopleApi = peopleApi ?? throw new ArgumentNullException(nameof(peopleApi));
            _externalReferenceChecker = externalReferenceChecker;
            _validationPipeline = BuildValidationPipeline(includeStateGatedChanges: true);
            _creationValidationPipeline = BuildValidationPipeline(includeStateGatedChanges: false);
        }

        #region PlanAndBuildJob Validation

        /// <summary>
        /// Validates a PlanAndBuildJob and returns a ValidationResult.
        /// Collects all errors without throwing exceptions.
        /// </summary>
        protected override ValidationResult Validate(PlanAndBuildJob job)
        {
            return _validationPipeline.Validate(job);
        }

        public override ValidationResult Validate(PlanAndBuildJob job, RepositoryAction action)
        {
            if (job == null)
            {
                throw new ArgumentNullException(nameof(job));
            }

            if (action == RepositoryAction.Delete)
            {
                return new ValidationResult();
            }

            return action == RepositoryAction.Create
                ? _creationValidationPipeline.Validate(job)
                : _validationPipeline.Validate(job);
        }

        /// <summary>
        /// Validates a PlanAndBuildJob and throws a ValidationException if invalid.
        /// Use this when you want fail-fast behavior.
        /// </summary>
        public void ValidateAndThrow(PlanAndBuildJob job)
        {
            _validationPipeline.ValidateAndThrow(job);
        }

        /// <summary>
        /// Validates with a custom error handling callback.
        /// </summary>
        public ValidationResult ValidateWithHandler(PlanAndBuildJob job, Action<ValidationResult> onError)
        {
            return _validationPipeline.ValidateWithHandler(job, onError);
        }

        /// <summary>
        /// Validates multiple Jobs in bulk. Results are returned in the same order as the input jobs.
        /// In addition to the per-job checks, this also detects JobName conflicts <em>within the batch itself</em>
        /// (i.e. two jobs being saved together that share the same JobName), which a single-job DB uniqueness
        /// query cannot catch since none of the batch's entries are persisted yet.
        /// Mirrors InfraOpsShared.DOM_Classes.DOM.Applications.Plan_And_Build.Validation.JobValidationHandler's
        /// OtherChangedEntries check.
        /// </summary>
        protected override List<ValidationResult> ValidateBulk(List<PlanAndBuildJob> jobs)
        {
            return ValidateBulk(jobs, includeStateGatedChanges: true);
        }

        public override List<ValidationResult> ValidateBulk(List<PlanAndBuildJob> jobs, RepositoryAction action)
        {
            if (action == RepositoryAction.Delete)
            {
                return jobs == null
                    ? new List<ValidationResult>()
                    : jobs.Select(_ => new ValidationResult()).ToList();
            }

            return ValidateBulk(jobs, includeStateGatedChanges: action != RepositoryAction.Create);
        }

        private List<ValidationResult> ValidateBulk(List<PlanAndBuildJob> jobs, bool includeStateGatedChanges)
        {
            if (jobs == null || !jobs.Any())
            {
                return new List<ValidationResult>();
            }

            // Initialize results - same order as input
            var results = jobs.Select(j => new ValidationResult()).ToList();

            // ============================================================
            // PHASE 1: NO DATABASE ACCESS CHECKS (BUSINESS RULES)
            // ============================================================
            for (int i = 0; i < jobs.Count; i++)
            {
                results[i].AddFailuresFrom(ValidateInfo(jobs[i]));
            }

            // Fast-fail if business rules fail
            if (results.AnyInvalid())
            {
                return results;
            }

            // ============================================================
            // PHASE 2: IN-MEMORY BATCH CONFLICT DETECTION (NO DATABASE)
            // ============================================================
            var batchConflicts = ValidateBatchConflicts(jobs);
            results.MergeFrom(batchConflicts);

            // Fast-fail if batch conflicts exist
            if (results.AnyInvalid())
            {
                return results;
            }

            // ============================================================
            // PHASE 3: DATABASE ACCESS CHECKS (UNIQUENESS) + REMAINING RULES
            // ============================================================
            ValidateAgainstDatabase(jobs, results, includeStateGatedChanges);

            return results;
        }

        /// <summary>
        /// Runs the database-backed validation phase. Batch-fetches every JobName that needs a uniqueness
        /// check, and every Person/Team id referenced by AssignedTo/AssignmentGroup/Attachments, in a handful
        /// of big-OR queries instead of issuing one query per Job (and, for People/Teams, one remote
        /// People &amp; Organizations call per reference).
        /// </summary>
        private void ValidateAgainstDatabase(List<PlanAndBuildJob> jobs, List<ValidationResult> results, bool includeStateGatedChanges)
        {
            var jobNames = jobs
                .Where(j => j.ShouldValidate(j.JobNameField) && !string.IsNullOrWhiteSpace(j.JobName))
                .Select(j => j.JobName)
                .Distinct()
                .ToList();

            var existingByJobName = _helper.Jobs.GetByJobNames(jobNames).ToLookup(j => j.JobName);

            var personIds = CollectReferencedPersonIds(jobs);
            var teamIds = CollectReferencedTeamIds(jobs);
            var jobTypeIds = CollectReferencedJobTypeIds(jobs);
            var locationIds = CollectReferencedLocationIds(jobs);
            var assetIds = CollectReferencedAssetIds(jobs);
            var connectionIds = CollectReferencedConnectionIds(jobs);
            var cableTypeIds = CollectReferencedCableTypeIds(jobs);

            var existingPersonIds = GetExistingPersonIds(personIds);
            var existingTeamIds = GetExistingTeamIds(teamIds);
            var existingJobTypeIds = GetExistingJobTypeIds(jobTypeIds);
            var existingLocationIds = _externalReferenceChecker?.GetExistingLocationIds(locationIds);
            var existingAssetIds = _externalReferenceChecker?.GetExistingAssetIds(assetIds);
            var existingConnectionIds = _externalReferenceChecker?.GetExistingConnectionIds(connectionIds);
            var existingCableTypeIds = _externalReferenceChecker?.GetExistingCableTypeIds(cableTypeIds);

            for (int i = 0; i < jobs.Count; i++)
            {
                results[i].AddFailuresFrom(ValidateJobTypeAndDates(jobs[i]));
                if (includeStateGatedChanges)
                {
                    results[i].AddFailuresFrom(ValidateStateGatedChanges(jobs[i]));
                }

                results[i].AddFailuresFrom(ValidateJobNameUniqueness(jobs[i], existingByJobName));
                results[i].AddFailuresFrom(ValidatePeopleAndOrganizations(jobs[i], existingPersonIds, existingTeamIds));
                results[i].AddFailuresFrom(ValidateJobTypeReference(jobs[i], existingJobTypeIds));
                results[i].AddFailuresFrom(ValidateExternalReferences(jobs[i], existingLocationIds, existingAssetIds, existingConnectionIds, existingCableTypeIds));
            }
        }

        private static List<string> CollectReferencedJobTypeIds(List<PlanAndBuildJob> jobs)
        {
            return jobs
                .Where(j => j.ShouldValidate(j.TypeField) && IsReferenceSet(j.Type))
                .Select(j => j.Type.Identifier)
                .ToList();
        }

        private static List<Guid> CollectReferencedLocationIds(List<PlanAndBuildJob> jobs)
        {
            return jobs
                .Where(j => j.ShouldValidateAny(j.LocationsField) && j.Locations != null)
                .SelectMany(j => j.Locations)
                .Where(id => id != Guid.Empty)
                .ToList();
        }

        private static List<string> CollectReferencedAssetIds(List<PlanAndBuildJob> jobs)
        {
            return jobs
                .Where(j => j.ShouldValidateAny(j.AssetsUsedField) && j.AssetsUsed != null)
                .SelectMany(j => j.AssetsUsed)
                .Where(asset => asset != null && IsReferenceSet(asset.AssetId))
                .Select(asset => asset.AssetId.Identifier)
                .ToList();
        }

        private static List<string> CollectReferencedConnectionIds(List<PlanAndBuildJob> jobs)
        {
            return jobs
                .Where(j => j.ShouldValidateAny(j.ConnectionsOnJobField) && j.ConnectionsOnJob != null)
                .SelectMany(j => j.ConnectionsOnJob)
                .Where(connection => connection != null && IsReferenceSet(connection.ConnectionId))
                .Select(connection => connection.ConnectionId.Identifier)
                .ToList();
        }

        private static List<string> CollectReferencedCableTypeIds(List<PlanAndBuildJob> jobs)
        {
            return jobs
                .Where(j => j.ShouldValidateAny(j.ConnectionsOnJobField) && j.ConnectionsOnJob != null)
                .SelectMany(j => j.ConnectionsOnJob)
                .Where(connection => connection != null && IsReferenceSet(connection.CableType))
                .Select(connection => connection.CableType.Identifier)
                .ToList();
        }

        /// <summary>
        /// Collects every Person id referenced by AssignedTo or attachment AttachedBy fields that require validation.
        /// </summary>
        private static List<Guid> CollectReferencedPersonIds(List<PlanAndBuildJob> jobs)
        {
            var assignedTo = jobs
                .Where(j => j.ShouldValidate(j.Ownership.AssignedToField) && j.Ownership.AssignedTo.HasValue)
                .Select(j => j.Ownership.AssignedTo.Value);

            var attachedBy = jobs
                .Where(j => j.ShouldValidateAny(j.AttachmentsField) && j.Attachments != null)
                .SelectMany(j => j.Attachments)
                .Where(a => a?.AttachedBy.HasValue == true)
                .Select(a => a.AttachedBy.Value);

            return assignedTo.Concat(attachedBy).ToList();
        }

        /// <summary>
        /// Collects every Team id referenced by AssignmentGroup fields that require validation.
        /// </summary>
        private static List<Guid> CollectReferencedTeamIds(List<PlanAndBuildJob> jobs)
        {
            return jobs
                .Where(j => j.ShouldValidate(j.Ownership.AssignmentGroupField) && j.Ownership.AssignmentGroup.HasValue)
                .Select(j => j.Ownership.AssignmentGroup.Value)
                .ToList();
        }

        /// <summary>
        /// Detects JobName conflicts among the jobs of a single batch (in-memory only, no database access).
        /// Result at index i corresponds to job at index i.
        /// </summary>
        public List<ValidationResult> ValidateBatchConflicts(List<PlanAndBuildJob> jobs)
        {
            var results = jobs.Select(j => new ValidationResult()).ToList();

            var nameGroups = jobs
                .Select((job, index) => new { job, index })
                .Where(x => x.job.ShouldValidate(x.job.JobNameField) && !string.IsNullOrWhiteSpace(x.job.JobName))
                .GroupBy(x => x.job.JobName, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1);

            foreach (var group in nameGroups)
            {
                foreach (var item in group)
                {
                    results[item.index].AddFailReason(PlanAndBuildJobValidationField.JobName,
                        $"Job Name '{item.job.JobName}' is duplicated within the validation batch.");
                }
            }

            return results;
        }

        #endregion

        #region Pipeline Construction

        private Validator<PlanAndBuildJob> BuildValidationPipeline(bool includeStateGatedChanges)
        {
            // Critical validations - stop on failure. Uniqueness/other checks are meaningless without a name.
            var criticalValidations = Validator<PlanAndBuildJob>
                .Create(ValidateInfo)
                .StopOnFailure();

            // No database access checks - fail fast before hitting the database
            var noDatabaseChecks = Validator<PlanAndBuildJob>
                .Create(ValidateJobTypeAndDates);

            if (includeStateGatedChanges)
            {
                noDatabaseChecks = noDatabaseChecks.AndThen(ValidateStateGatedChanges);
            }

            noDatabaseChecks = noDatabaseChecks.StopOnFailure();

            // Database access checks (uniqueness, People/Team existence)
            var databaseChecks = Validator<PlanAndBuildJob>
                .Create(ValidateJobNameUniqueness)
                .AndThen(ValidatePeopleAndOrganizations)
                .AndThen(ValidateJobTypeReference)
                .AndThen(ValidateExternalReferences);

            // Combine: critical first, then no-database checks, then database checks
            return criticalValidations.AndThen(noDatabaseChecks.AndThen(databaseChecks));
        }

        #endregion

        #region Validation Methods

        private ValidationResult ValidateInfo(PlanAndBuildJob job)
        {
            var result = new ValidationResult();

            if (job.ShouldValidate(job.JobNameField) && !IsJobNameValid(job, out var nameResult))
            {
                result.AddFailuresFrom(nameResult);
            }

            return result;
        }

        private ValidationResult ValidateJobTypeAndDates(PlanAndBuildJob job)
        {
            var result = new ValidationResult();

            if (job.ShouldValidate(job.TypeField) && !IsJobTypeValid(job, out var jobTypeResult))
            {
                result.AddFailuresFrom(jobTypeResult);
            }

            if (job.ShouldValidateAny(job.StartField, job.EndField) && !IsEndTimeValid(job, out var endResult))
            {
                result.AddFailuresFrom(endResult);
            }

            return result;
        }

        private ValidationResult ValidateStateGatedChanges(PlanAndBuildJob job)
        {
            return AreStateGatedChangesAllowed(job, out var result)
                ? new ValidationResult()
                : result;
        }

        private ValidationResult ValidateJobNameUniqueness(PlanAndBuildJob job)
        {
            var result = new ValidationResult();

            if (!job.ShouldValidate(job.JobNameField))
            {
                return result;
            }

            if (IsJobNameInUse(job.JobName, job.Identifier))
            {
                result.AddFailReason(PlanAndBuildJobValidationField.JobName, $"Job Name '{job.JobName}' is already in use.");
            }

            return result;
        }

        private bool IsJobNameInUse(string jobName, string exceptIdentifier)
        {
            FilterElement<PlanAndBuildJob> filter = PlanAndBuildJobExposers.JobName.Equal(jobName);

            if (!string.IsNullOrEmpty(exceptIdentifier))
            {
                filter = filter.AND(PlanAndBuildJobExposers.Identifier.NotEqual(exceptIdentifier));
            }

            return _helper.Jobs.Count(filter) > 0;
        }

        /// <summary>
        /// Batch variant of <see cref="ValidateJobNameUniqueness(PlanAndBuildJob)"/>, used by
        /// <see cref="ValidateBulk"/>. Checks the pre-fetched <paramref name="existingByJobName"/> lookup (built
        /// once for the whole batch via <see cref="IPlanAndBuildJobRepository.GetByJobNames"/>) instead of
        /// issuing its own DB query.
        /// </summary>
        private ValidationResult ValidateJobNameUniqueness(PlanAndBuildJob job, ILookup<string, PlanAndBuildJob> existingByJobName)
        {
            var result = new ValidationResult();

            if (!job.ShouldValidate(job.JobNameField))
            {
                return result;
            }

            if (IsJobNameInUse(job.JobName, job.Identifier, existingByJobName))
            {
                result.AddFailReason(PlanAndBuildJobValidationField.JobName, $"Job Name '{job.JobName}' is already in use.");
            }

            return result;
        }

        private static bool IsJobNameInUse(string jobName, string exceptIdentifier, ILookup<string, PlanAndBuildJob> existingByJobName)
        {
            if (existingByJobName == null)
            {
                return false;
            }

            return existingByJobName[jobName ?? string.Empty]
                .Any(j => string.IsNullOrEmpty(exceptIdentifier) || !string.Equals(j.Identifier, exceptIdentifier, StringComparison.Ordinal));
        }

        private ValidationResult ValidateJobTypeReference(PlanAndBuildJob job)
        {
            return ValidateJobTypeReference(job, GetExistingJobTypeIds(new[] { job.Type == null ? null : job.Type.Identifier }));
        }

        private ValidationResult ValidateJobTypeReference(PlanAndBuildJob job, HashSet<string> existingJobTypeIds)
        {
            var result = new ValidationResult();

            if (job.ShouldValidate(job.TypeField) &&
                IsReferenceSet(job.Type) &&
                existingJobTypeIds?.Contains(job.Type.Identifier) != true)
            {
                result.AddFailReason(PlanAndBuildJobValidationField.JobType, $"Referenced JobType '{job.Type.Identifier}' does not exist.");
            }

            return result;
        }

        private HashSet<string> GetExistingJobTypeIds(IEnumerable<string> jobTypeIds)
        {
            var keys = jobTypeIds?.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList() ?? new List<string>();

            return _helper.JobTypes.GetByIdentifiers(keys)
                .Select(jobType => jobType.Identifier)
                .ToHashSet();
        }

        /// <summary>
        /// Validates that <see cref="JobOwnership.AssignedTo"/> and <see cref="JobOwnership.AssignmentGroup"/>
        /// (when set) reference an existing Person/Team in the People &amp; Organizations solution, and that each
        /// <see cref="JobAttachment.AttachedBy"/> (when set) references an existing Person.
        /// Since a literal <c>SdmObjectReference&lt;Person&gt;</c> is not possible (Person is defined in an
        /// external, already-compiled assembly, so the SDM source generator cannot recognize it as an SdmObject),
        /// these fields are plain <see cref="Guid"/>s validated here via lightweight existence (Count) checks.
        /// </summary>
        private ValidationResult ValidatePeopleAndOrganizations(PlanAndBuildJob job)
        {
            var result = new ValidationResult();

            if (job.ShouldValidate(job.Ownership.AssignedToField) &&
                job.Ownership.AssignedTo.HasValue &&
                !IsPersonValid(job.Ownership.AssignedTo.Value))
            {
                result.AddFailReason(PlanAndBuildJobValidationField.AssignedTo, $"AssignedTo Person '{job.Ownership.AssignedTo}' does not exist.");
            }

            if (job.ShouldValidate(job.Ownership.AssignmentGroupField) &&
                job.Ownership.AssignmentGroup.HasValue &&
                !IsTeamValid(job.Ownership.AssignmentGroup.Value))
            {
                result.AddFailReason(PlanAndBuildJobValidationField.AssignmentGroup, $"AssignmentGroup Team '{job.Ownership.AssignmentGroup}' does not exist.");
            }

            if (job.ShouldValidateAny(job.AttachmentsField) && job.Attachments != null)
            {
                foreach (var attachment in job.Attachments)
                {
                    if (attachment?.AttachedBy.HasValue == true && !IsPersonValid(attachment.AttachedBy.Value))
                    {
                        result.AddFailReason(PlanAndBuildJobValidationField.Attachments, $"AttachedBy Person '{attachment.AttachedBy}' does not exist.");
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Batch variant of <see cref="ValidatePeopleAndOrganizations(PlanAndBuildJob)"/>, used by
        /// <see cref="ValidateBulk"/>. Checks the pre-fetched <paramref name="existingPersonIds"/>/
        /// <paramref name="existingTeamIds"/> sets (built once for the whole batch via
        /// <see cref="GetExistingPersonIds"/>/<see cref="GetExistingTeamIds"/>) instead of issuing a People
        /// &amp; Organizations query per Person/Team reference.
        /// </summary>
        private ValidationResult ValidatePeopleAndOrganizations(PlanAndBuildJob job, HashSet<Guid> existingPersonIds, HashSet<Guid> existingTeamIds)
        {
            var result = new ValidationResult();

            if (job.ShouldValidate(job.Ownership.AssignedToField) &&
                job.Ownership.AssignedTo.HasValue &&
                existingPersonIds?.Contains(job.Ownership.AssignedTo.Value) != true)
            {
                result.AddFailReason(PlanAndBuildJobValidationField.AssignedTo, $"AssignedTo Person '{job.Ownership.AssignedTo}' does not exist.");
            }

            if (job.ShouldValidate(job.Ownership.AssignmentGroupField) &&
                job.Ownership.AssignmentGroup.HasValue &&
                existingTeamIds?.Contains(job.Ownership.AssignmentGroup.Value) != true)
            {
                result.AddFailReason(PlanAndBuildJobValidationField.AssignmentGroup, $"AssignmentGroup Team '{job.Ownership.AssignmentGroup}' does not exist.");
            }

            if (job.ShouldValidateAny(job.AttachmentsField) && job.Attachments != null)
            {
                foreach (var attachment in job.Attachments)
                {
                    if (attachment?.AttachedBy.HasValue == true && existingPersonIds?.Contains(attachment.AttachedBy.Value) != true)
                    {
                        result.AddFailReason(PlanAndBuildJobValidationField.Attachments, $"AttachedBy Person '{attachment.AttachedBy}' does not exist.");
                    }
                }
            }

            return result;
        }

        private ValidationResult ValidateExternalReferences(PlanAndBuildJob job)
        {
            if (_externalReferenceChecker == null)
            {
                return new ValidationResult();
            }

            return ValidateExternalReferences(
                job,
                _externalReferenceChecker.GetExistingLocationIds(CollectReferencedLocationIds(new List<PlanAndBuildJob> { job })),
                _externalReferenceChecker.GetExistingAssetIds(CollectReferencedAssetIds(new List<PlanAndBuildJob> { job })),
                _externalReferenceChecker.GetExistingConnectionIds(CollectReferencedConnectionIds(new List<PlanAndBuildJob> { job })),
                _externalReferenceChecker.GetExistingCableTypeIds(CollectReferencedCableTypeIds(new List<PlanAndBuildJob> { job })));
        }

        private ValidationResult ValidateExternalReferences(
            PlanAndBuildJob job,
            IReadOnlyCollection<Guid> existingLocationIds,
            IReadOnlyCollection<string> existingAssetIds,
            IReadOnlyCollection<string> existingConnectionIds,
            IReadOnlyCollection<string> existingCableTypeIds)
        {
            var result = new ValidationResult();

            if (existingLocationIds != null && job.ShouldValidateAny(job.LocationsField) && job.Locations != null)
            {
                var existing = existingLocationIds.ToHashSet();
                foreach (var locationId in job.Locations.Where(id => id != Guid.Empty))
                {
                    if (!existing.Contains(locationId))
                    {
                        result.AddFailReason(PlanAndBuildJobValidationField.Locations, $"Referenced Location '{locationId}' does not exist.");
                    }
                }
            }

            if (existingAssetIds != null && job.ShouldValidateAny(job.AssetsUsedField) && job.AssetsUsed != null)
            {
                var existing = existingAssetIds.ToHashSet();
                foreach (var assetId in job.AssetsUsed.Where(asset => asset != null && IsReferenceSet(asset.AssetId)).Select(asset => asset.AssetId.Identifier))
                {
                    if (!existing.Contains(assetId))
                    {
                        result.AddFailReason(PlanAndBuildJobValidationField.AssetsUsed, $"Referenced Asset '{assetId}' does not exist.");
                    }
                }
            }

            if (existingConnectionIds != null && job.ShouldValidateAny(job.ConnectionsOnJobField) && job.ConnectionsOnJob != null)
            {
                var existing = existingConnectionIds.ToHashSet();
                foreach (var connectionId in job.ConnectionsOnJob.Where(connection => connection != null && IsReferenceSet(connection.ConnectionId)).Select(connection => connection.ConnectionId.Identifier))
                {
                    if (!existing.Contains(connectionId))
                    {
                        result.AddFailReason(PlanAndBuildJobValidationField.Connections, $"Referenced Connection '{connectionId}' does not exist.");
                    }
                }
            }

            if (existingCableTypeIds != null && job.ShouldValidateAny(job.ConnectionsOnJobField) && job.ConnectionsOnJob != null)
            {
                var existing = existingCableTypeIds.ToHashSet();
                foreach (var cableTypeId in job.ConnectionsOnJob.Where(connection => connection != null && IsReferenceSet(connection.CableType)).Select(connection => connection.CableType.Identifier))
                {
                    if (!existing.Contains(cableTypeId))
                    {
                        result.AddFailReason(PlanAndBuildJobValidationField.Connections, $"Referenced CableType '{cableTypeId}' does not exist.");
                    }
                }
            }

            return result;
        }

        private bool IsPersonValid(Guid personId)
        {
            return _peopleApi.People.Count(PersonExposers.Id.Equal(personId)) > 0;
        }

        private bool IsTeamValid(Guid teamId)
        {
            return _peopleApi.Teams.Count(TeamExposers.Id.Equal(teamId)) > 0;
        }

        /// <summary>
        /// Batch-fetches the ids of all existing People matching any of <paramref name="personIds"/> in a single
        /// big-OR query, instead of one <c>Count</c> call per candidate id.
        /// </summary>
        private HashSet<Guid> GetExistingPersonIds(IEnumerable<Guid> personIds)
        {
            var keys = personIds?.Distinct().ToList() ?? new List<Guid>();

            return _peopleApi.People
                .ReadByBigOrFilter(keys, id => PersonExposers.Id.Equal(id))
                .Select(p => p.Id)
                .ToHashSet();
        }

        /// <summary>
        /// Batch-fetches the ids of all existing Teams matching any of <paramref name="teamIds"/> in a single
        /// big-OR query, instead of one <c>Count</c> call per candidate id.
        /// </summary>
        private HashSet<Guid> GetExistingTeamIds(IEnumerable<Guid> teamIds)
        {
            var keys = teamIds?.Distinct().ToList() ?? new List<Guid>();

            return _peopleApi.Teams
                .ReadByBigOrFilter(keys, id => TeamExposers.Id.Equal(id))
                .Select(t => t.Id)
                .ToHashSet();
        }

        private static bool IsReferenceSet<T>(SdmObjectReference<T> reference)
            where T : SdmObject<T>
        {
            return reference != null && !string.IsNullOrWhiteSpace(reference.Identifier);
        }

        #endregion
    }
}
