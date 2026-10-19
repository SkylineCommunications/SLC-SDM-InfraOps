namespace Skyline.DataMiner.SDM.PlanAndBuild.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Skyline.DataMiner.SDM.InfraOps.Common.Validation;
    using Skyline.DataMiner.SDM.PlanAndBuild.Helpers;
    using Skyline.DataMiner.SDM.PlanAndBuild.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    using static Skyline.DataMiner.SDM.PlanAndBuild.Validation.JobTypeValidationHandler;

    /// <summary>
    /// Public validator service for JobType validation, including data access for Name uniqueness
    /// and "in use" checks.
    /// </summary>
    public class JobTypeValidator : ValidatorBase<JobType>
    {
        private readonly IPlanAndBuildApiHelper _helper;

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
        }

        protected override List<ValidationResult> ValidateBulkNoDatabaseBusinessRules(List<JobType> entities)
        {
            // ============================================================
            // PHASE 1: NO DATABASE ACCESS CHECKS (BUSINESS RULES)
            // ============================================================
            return entities.Select(ValidateInfo).ToList();
        }

        protected override List<ValidationResult> ValidateBulkNoDatabaseBatchConflicts(List<JobType> entities)
        {
            // ============================================================
            // PHASE 2: IN-MEMORY BATCH CONFLICT DETECTION (NO DATABASE)
            // ============================================================
            return ValidateBatchConflicts(entities);
        }

        protected override List<ValidationResult> ValidateBulkWithDatabase(List<JobType> entities)
        {

            // Initialize results - same order as input
            var results = entities.Select(j => new ValidationResult()).ToList();

            // ============================================================
            // PHASE 3: DATABASE ACCESS CHECKS (UNIQUENESS) + REMAINING RULES
            // ============================================================
            // Batch-fetch every Name that needs a uniqueness check, and every renamed JobType's "in use by
            // existing Jobs" state, in two big-OR queries instead of one query per JobType in the loop below.
            var names = entities
                .Where(j => j.ShouldValidate(j.NameField) && !string.IsNullOrWhiteSpace(j.Name))
                .Select(j => j.Name)
                .Distinct()
                .ToList();

            var existingByName = _helper.JobTypes.GetByNames(names).ToLookup(j => j.Name);

            var renamedIdentifiers = entities
                .Where(j => !j.IsNew && j.NameField.Changed)
                .Select(j => j.Identifier)
                .Distinct()
                .ToList();

            var jobTypesInUse = _helper.Jobs.GetByJobTypes(renamedIdentifiers)
                .Select(job => job.Type.Identifier)
                .Where(identifier => identifier != null)
                .ToHashSet();

            for (int i = 0; i < entities.Count; i++)
            {
                results[i].AddFailuresFrom(ValidateNameUniqueness(entities[i], existingByName));
                results[i].AddFailuresFrom(ValidateNotInUseWhenRenamed(entities[i], jobTypesInUse));
            }

            return results;
        }




        #region JobType Validation

        /// <summary>
        /// Bulk delete guard: blocks deleting JobTypes that are in use by existing Jobs
        /// (batch-fetched in a single query to avoid N+1).
        /// </summary>
        protected override List<ValidationResult> ValidateBulkForDelete(List<JobType> jobTypes)
        {
            if (jobTypes == null || !jobTypes.Any())
            {
                return new List<ValidationResult>();
            }

            return ValidateNotInUseWhenDeleted(jobTypes);
        }

        /// <summary>
        /// Checks whether a JobType is in use by existing Jobs. Deletion is blocked while referenced.
        /// </summary>
        public ValidationResult ValidateNotInUseWhenDeleted(JobType jobType)
        {
            if (jobType == null)
            {
                throw new ArgumentNullException(nameof(jobType));
            }

            return ValidateNotInUseWhenDeleted(new List<JobType> { jobType }).Single();
        }

        /// <summary>
        /// Checks which JobTypes in the batch
        /// are in use by existing Jobs in a single query, then maps the results back to per-JobType <see cref="ValidationResult"/>s.
        /// </summary>
        private List<ValidationResult> ValidateNotInUseWhenDeleted(List<JobType> jobTypes)
        {
            var results = jobTypes.Select(j => new ValidationResult()).ToList();

            var identifiers = jobTypes
                .Select(j => j.Identifier)
                .Where(id => id != null)
                .Distinct()
                .ToList();

            var inUseIdentifiers = _helper.Jobs.GetByJobTypes(identifiers)
                .Select(job => job.Type.Identifier)
                .Where(id => id != null)
                .ToHashSet();

            for (int i = 0; i < jobTypes.Count; i++)
            {
                if (inUseIdentifiers.Contains(jobTypes[i].Identifier))
                {
                    results[i].AddFailReason(JobTypeValidationField.JobType, "Cannot delete a Job Type that is in use by existing Jobs.");
                }
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

        /// <summary>
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

        private static bool IsNameInUse(string name, string exceptIdentifier, ILookup<string, JobType> existingByName)
        {
            if (existingByName == null)
            {
                return false;
            }

            return existingByName[name ?? string.Empty]
                .Any(j => string.IsNullOrEmpty(exceptIdentifier) || !string.Equals(j.Identifier, exceptIdentifier, StringComparison.Ordinal));
        }

        #endregion
    }
}
