namespace Skyline.DataMiner.SDM.PlanAndBuild.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.InfraOps.Common.Validation;
    using Skyline.DataMiner.SDM.PlanAndBuild.Helpers;
    using Skyline.DataMiner.SDM.PlanAndBuild.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    using static Skyline.DataMiner.SDM.PlanAndBuild.Validation.JobTypeValidationHandler;

    /// <summary>
    /// Public validator service for JobType validation, including data access for Name uniqueness
    /// and "in use" checks.
    /// </summary>
    public class JobTypeValidator
    {
        private readonly IPlanAndBuildApiHelper _helper;
        private readonly Validator<JobType> _validationPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="JobTypeValidator"/> class.
        /// </summary>
        /// <param name="helper">
        /// The Plan &amp; Build API helper used to query existing JobTypes (uniqueness) and Jobs (in-use checks).
        /// Note: this is captured by reference during <see cref="PlanAndBuildApiHelper"/> construction, before
        /// its repositories are wired up. Only <see cref="Validate"/>/<see cref="ValidateAndThrow"/> (called
        /// after construction completes) access <paramref name="helper"/>'s repositories.
        /// </param>
        public JobTypeValidator(IPlanAndBuildApiHelper helper)
        {
            _helper = helper ?? throw new ArgumentNullException(nameof(helper));
            _validationPipeline = BuildValidationPipeline();
        }

        #region JobType Validation

        /// <summary>
        /// Validates a JobType and returns a ValidationResult.
        /// Collects all errors without throwing exceptions.
        /// </summary>
        public ValidationResult Validate(JobType jobType)
        {
            if (jobType == null)
            {
                throw new ArgumentNullException(nameof(jobType));
            }

            return _validationPipeline.Validate(jobType);
        }

        /// <summary>
        /// Validates a JobType and throws a ValidationException if invalid.
        /// Use this when you want fail-fast behavior.
        /// </summary>
        public void ValidateAndThrow(JobType jobType)
        {
            _validationPipeline.ValidateAndThrow(jobType);
        }

        /// <summary>
        /// Validates with a custom error handling callback.
        /// </summary>
        public ValidationResult ValidateWithHandler(JobType jobType, Action<ValidationResult> onError)
        {
            return _validationPipeline.ValidateWithHandler(jobType, onError);
        }

        /// <summary>
        /// Validates that a JobType can be deleted. Mirrors production behavior: deletion is blocked
        /// while the JobType is referenced by existing Jobs.
        /// </summary>
        public ValidationResult ValidateDeletion(JobType jobType)
        {
            if (jobType == null)
            {
                throw new ArgumentNullException(nameof(jobType));
            }

            var result = new ValidationResult();

            if (IsJobTypeInUse(jobType.Identifier))
            {
                result.AddFailReason(JobTypeValidationField.JobType, "Cannot delete a Job Type that is in use by existing Jobs.");
            }

            return result;
        }

        /// <summary>
        /// Validates multiple JobTypes in bulk. Results are returned in the same order as the input JobTypes.
        /// In addition to the per-JobType checks, this also detects Name conflicts <em>within the batch itself</em>
        /// (i.e. two JobTypes being saved together that share the same Name), which a single-JobType DB uniqueness
        /// query cannot catch since none of the batch's entries are persisted yet.
        /// Mirrors InfraOpsShared.DOM_Classes.DOM.Applications.Plan_And_Build.Validation.JobTypeValidationHandler's
        /// OtherChangedEntries check.
        /// </summary>
        public List<ValidationResult> ValidateBulk(List<JobType> jobTypes)
        {
            if (jobTypes == null || !jobTypes.Any())
            {
                return new List<ValidationResult>();
            }

            // Initialize results - same order as input
            var results = jobTypes.Select(j => new ValidationResult()).ToList();

            // ============================================================
            // PHASE 1: NO DATABASE ACCESS CHECKS (BUSINESS RULES)
            // ============================================================
            for (int i = 0; i < jobTypes.Count; i++)
            {
                results[i].AddFailuresFrom(ValidateInfo(jobTypes[i]));
            }

            // Fast-fail if business rules fail
            if (results.AnyInvalid())
            {
                return results;
            }

            // ============================================================
            // PHASE 2: IN-MEMORY BATCH CONFLICT DETECTION (NO DATABASE)
            // ============================================================
            var batchConflicts = ValidateBatchConflicts(jobTypes);
            results.MergeFrom(batchConflicts);

            // Fast-fail if batch conflicts exist
            if (results.AnyInvalid())
            {
                return results;
            }

            // ============================================================
            // PHASE 3: DATABASE ACCESS CHECKS (UNIQUENESS) + REMAINING RULES
            // ============================================================
            // Batch-fetch every Name that needs a uniqueness check, and every renamed JobType's "in use by
            // existing Jobs" state, in two big-OR queries instead of one query per JobType in the loop below.
            var names = jobTypes
                .Where(j => j.ShouldValidate(j.NameField) && !string.IsNullOrWhiteSpace(j.Name))
                .Select(j => j.Name)
                .Distinct()
                .ToList();

            var existingByName = _helper.JobTypes.GetByNames(names).ToLookup(j => j.Name);

            var renamedIdentifiers = jobTypes
                .Where(j => !j.IsNew && j.NameField.Changed)
                .Select(j => j.Identifier)
                .Distinct()
                .ToList();

            var jobTypesInUse = _helper.Jobs.GetByJobTypes(renamedIdentifiers)
                .Select(job => job.Type.Identifier)
                .Where(identifier => identifier != null)
                .ToHashSet();

            for (int i = 0; i < jobTypes.Count; i++)
            {
                results[i].AddFailuresFrom(ValidateNameUniqueness(jobTypes[i], existingByName));
                results[i].AddFailuresFrom(ValidateNotInUseWhenRenamed(jobTypes[i], jobTypesInUse));
            }

            return results;
        }

        /// <summary>
        /// Detects Name conflicts among the JobTypes of a single batch (in-memory only, no database access).
        /// Result at index i corresponds to JobType at index i.
        /// </summary>
        public List<ValidationResult> ValidateBatchConflicts(List<JobType> jobTypes)
        {
            var results = jobTypes.Select(j => new ValidationResult()).ToList();

            var nameGroups = jobTypes
                .Select((jobType, index) => new { jobType, index })
                .Where(x => x.jobType.ShouldValidate(x.jobType.NameField) && !string.IsNullOrWhiteSpace(x.jobType.Name))
                .GroupBy(x => x.jobType.Name, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1);

            foreach (var group in nameGroups)
            {
                foreach (var item in group)
                {
                    results[item.index].AddFailReason(JobTypeValidationField.Name,
                        $"Job Type Name '{item.jobType.Name}' is duplicated within the validation batch.");
                }
            }

            return results;
        }

        #endregion

        #region Pipeline Construction

        private Validator<JobType> BuildValidationPipeline()
        {
            // Critical validations - stop on failure. Uniqueness/in-use checks are meaningless without a name.
            var criticalValidations = Validator<JobType>
                .Create(ValidateInfo)
                .StopOnFailure();

            // Standard validations - collect all errors
            var standardValidations = Validator<JobType>
                .Create(ValidateNameUniqueness)
                .AndThen(ValidateNotInUseWhenRenamed);

            // Combine: critical first, then standard
            return criticalValidations.AndThen(standardValidations);
        }

        #endregion

        #region Validation Methods

        private ValidationResult ValidateInfo(JobType jobType)
        {
            var result = new ValidationResult();

            if (jobType.ShouldValidate(jobType.NameField) && !IsNameValid(jobType, out var nameResult))
            {
                result.AddFailuresFrom(nameResult);
            }

            return result;
        }

        private ValidationResult ValidateNameUniqueness(JobType jobType)
        {
            var result = new ValidationResult();

            if (!jobType.ShouldValidate(jobType.NameField))
            {
                return result;
            }

            if (IsNameInUse(jobType.Name, jobType.Identifier))
            {
                result.AddFailReason(JobTypeValidationField.Name, $"Job Type Name '{jobType.Name}' is already in use.");
            }

            return result;
        }

        /// <summary>
        /// Batch variant of <see cref="ValidateNameUniqueness(JobType)"/>, used by <see cref="ValidateBulk"/>.
        /// Checks a pre-fetched lookup (built once for the whole batch via <c>_helper.JobTypes.GetByNames</c>)
        /// instead of issuing one uniqueness query per JobType.
        /// </summary>
        private ValidationResult ValidateNameUniqueness(JobType jobType, ILookup<string, JobType> existingByName)
        {
            var result = new ValidationResult();

            if (!jobType.ShouldValidate(jobType.NameField))
            {
                return result;
            }

            if (IsNameInUse(jobType.Name, jobType.Identifier, existingByName))
            {
                result.AddFailReason(JobTypeValidationField.Name, $"Job Type Name '{jobType.Name}' is already in use.");
            }

            return result;
        }

        /// <summary>
        /// Mirrors production behavior: renaming an existing JobType is blocked while it is referenced by
        /// existing Jobs. Only relevant when the Name actually changed on an existing (non-new) JobType.
        /// </summary>
        private ValidationResult ValidateNotInUseWhenRenamed(JobType jobType)
        {
            var result = new ValidationResult();

            if (jobType.IsNew || !jobType.NameField.Changed)
            {
                return result;
            }

            if (IsJobTypeInUse(jobType.Identifier))
            {
                result.AddFailReason(JobTypeValidationField.Name, "Cannot edit the name of a Job Type that is in use by existing Jobs.");
            }

            return result;
        }

        /// <summary>
        /// Batch variant of <see cref="ValidateNotInUseWhenRenamed(JobType)"/>, used by <see cref="ValidateBulk"/>.
        /// Checks a pre-fetched set of JobType identifiers that are actually referenced by existing Jobs (built
        /// once for the whole batch via <c>_helper.Jobs.GetByJobTypes</c>, restricted to renamed JobTypes only)
        /// instead of issuing one "in use" query per renamed JobType.
        /// </summary>
        private ValidationResult ValidateNotInUseWhenRenamed(JobType jobType, HashSet<string> jobTypesInUse)
        {
            var result = new ValidationResult();

            if (jobType.IsNew || !jobType.NameField.Changed)
            {
                return result;
            }

            if (jobTypesInUse != null && jobTypesInUse.Contains(jobType.Identifier))
            {
                result.AddFailReason(JobTypeValidationField.Name, "Cannot edit the name of a Job Type that is in use by existing Jobs.");
            }

            return result;
        }

        private bool IsNameInUse(string name, string exceptIdentifier)
        {
            FilterElement<JobType> filter = JobTypeExposers.Name.Equal(name);

            if (!string.IsNullOrEmpty(exceptIdentifier))
            {
                filter = filter.AND(JobTypeExposers.Identifier.NotEqual(exceptIdentifier));
            }

            return _helper.JobTypes.Count(filter) > 0;
        }

        private static bool IsNameInUse(string name, string exceptIdentifier, ILookup<string, JobType> existingByName)
        {
            if (existingByName == null)
            {
                return false;
            }

            return existingByName[name ?? string.Empty]
                .Any(j => string.IsNullOrEmpty(exceptIdentifier) || !string.Equals(j.Identifier, exceptIdentifier, StringComparison.Ordinal));
        }

        private bool IsJobTypeInUse(string jobTypeIdentifier)
        {
            FilterElement<PlanAndBuildJob> filter = PlanAndBuildJobExposers.Type.Equal(new SdmObjectReference<JobType>(jobTypeIdentifier));
            return _helper.Jobs.Count(filter) > 0;
        }

        #endregion
    }
}
