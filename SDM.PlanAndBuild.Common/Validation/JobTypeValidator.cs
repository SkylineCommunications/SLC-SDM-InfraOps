namespace Skyline.DataMiner.SDM.PlanAndBuild.Validation
{
    using System;

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

        private bool IsNameInUse(string name, string exceptIdentifier)
        {
            FilterElement<JobType> filter = JobTypeExposers.Name.Equal(name);

            if (!string.IsNullOrEmpty(exceptIdentifier))
            {
                filter = filter.AND(JobTypeExposers.Identifier.NotEqual(exceptIdentifier));
            }

            return _helper.JobTypes.Count(filter) > 0;
        }

        private bool IsJobTypeInUse(string jobTypeIdentifier)
        {
            FilterElement<PlanAndBuildJob> filter = PlanAndBuildJobExposers.JobType.Equal(new SdmObjectReference<JobType>(jobTypeIdentifier));
            return _helper.Jobs.Count(filter) > 0;
        }

        #endregion
    }
}
