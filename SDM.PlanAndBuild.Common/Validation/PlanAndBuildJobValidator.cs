namespace Skyline.DataMiner.SDM.PlanAndBuild.Validation
{
    using System;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.InfraOps.Common.Validation;
    using Skyline.DataMiner.SDM.PlanAndBuild.Helpers;
    using Skyline.DataMiner.SDM.PlanAndBuild.Models;
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
                .AndThen(ValidateJobTypeAndDates);

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

        #endregion
    }
}
