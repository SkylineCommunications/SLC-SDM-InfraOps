namespace Skyline.DataMiner.SDM.PlanAndBuild.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.InfraOps.Common.Validation;
    using Skyline.DataMiner.SDM.PlanAndBuild.Helpers;
    using Skyline.DataMiner.SDM.PlanAndBuild.Models;
    using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    using static Skyline.DataMiner.SDM.PlanAndBuild.Validation.PlanAndBuildJobValidationHandler;

    /// <summary>
    /// Public validator service for PlanAndBuildJob validation, including data access for JobName uniqueness checks.
    /// </summary>
    public class PlanAndBuildJobValidator
    {
        private readonly IPlanAndBuildApiHelper _helper;
        private readonly Validator<PlanAndBuildJob> _validationPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanAndBuildJobValidator"/> class.
        /// </summary>
        /// <param name="helper">
        /// The Plan &amp; Build API helper used to query existing Jobs for uniqueness checks.
        /// Note: this is captured by reference during <see cref="PlanAndBuildApiHelper"/> construction, before
        /// its repositories are wired up. Only <see cref="Validate"/>/<see cref="ValidateAndThrow"/> (called
        /// after construction completes) access <paramref name="helper"/>'s repositories.
        /// </param>
        public PlanAndBuildJobValidator(IPlanAndBuildApiHelper helper)
        {
            _helper = helper ?? throw new ArgumentNullException(nameof(helper));
            _validationPipeline = BuildValidationPipeline();
        }

        #region PlanAndBuildJob Validation

        /// <summary>
        /// Validates a PlanAndBuildJob and returns a ValidationResult.
        /// Collects all errors without throwing exceptions.
        /// </summary>
        public ValidationResult Validate(PlanAndBuildJob job)
        {
            if (job == null)
            {
                throw new ArgumentNullException(nameof(job));
            }

            return _validationPipeline.Validate(job);
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
        public List<ValidationResult> ValidateBulk(List<PlanAndBuildJob> jobs)
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
            for (int i = 0; i < jobs.Count; i++)
            {
                results[i].AddFailuresFrom(ValidateJobNameUniqueness(jobs[i]));
                results[i].AddFailuresFrom(ValidateJobTypeAndDates(jobs[i]));
                results[i].AddFailuresFrom(ValidatePeopleAndOrganizations(jobs[i]));
            }

            return results;
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

        private Validator<PlanAndBuildJob> BuildValidationPipeline()
        {
            // Critical validations - stop on failure. Uniqueness/other checks are meaningless without a name.
            var criticalValidations = Validator<PlanAndBuildJob>
                .Create(ValidateInfo)
                .StopOnFailure();

            // Standard validations - collect all errors
            var standardValidations = Validator<PlanAndBuildJob>
                .Create(ValidateJobNameUniqueness)
                .AndThen(ValidateJobTypeAndDates)
                .AndThen(ValidatePeopleAndOrganizations);

            // Combine: critical first, then standard
            return criticalValidations.AndThen(standardValidations);
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

            if (job.ShouldValidate(job.JobTypeField) && !IsJobTypeValid(job, out var jobTypeResult))
            {
                result.AddFailuresFrom(jobTypeResult);
            }

            if (job.ShouldValidateAny(job.StartField, job.EndField) && !IsEndTimeValid(job, out var endResult))
            {
                result.AddFailuresFrom(endResult);
            }

            return result;
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

        private bool IsPersonValid(Guid personId)
        {
            return _helper.People.People.Count(PersonExposers.Id.Equal(personId)) > 0;
        }

        private bool IsTeamValid(Guid teamId)
        {
            return _helper.People.Teams.Count(TeamExposers.Id.Equal(teamId)) > 0;
        }

        #endregion
    }
}
