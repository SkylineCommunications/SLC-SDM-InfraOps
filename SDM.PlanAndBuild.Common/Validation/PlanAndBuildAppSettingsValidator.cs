namespace Skyline.DataMiner.SDM.PlanAndBuild.Validation
{
    using System;

    using Skyline.DataMiner.SDM.PlanAndBuild.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Public validator service for PlanAndBuildAppSettings validation.
    /// No repository access is required since production defines no business rules for this entity.
    /// </summary>
    public class PlanAndBuildAppSettingsValidator
    {
        private readonly Validator<PlanAndBuildAppSettings> _validationPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanAndBuildAppSettingsValidator"/> class.
        /// </summary>
        public PlanAndBuildAppSettingsValidator()
        {
            _validationPipeline = Validator<PlanAndBuildAppSettings>.Create(ValidateInfo);
        }

        /// <summary>
        /// Validates a PlanAndBuildAppSettings and returns a ValidationResult.
        /// Collects all errors without throwing exceptions.
        /// </summary>
        public ValidationResult Validate(PlanAndBuildAppSettings appSettings)
        {
            if (appSettings == null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            return _validationPipeline.Validate(appSettings);
        }

        /// <summary>
        /// Validates a PlanAndBuildAppSettings and throws a ValidationException if invalid.
        /// Use this when you want fail-fast behavior.
        /// </summary>
        public void ValidateAndThrow(PlanAndBuildAppSettings appSettings)
        {
            _validationPipeline.ValidateAndThrow(appSettings);
        }

        /// <summary>
        /// Validates with a custom error handling callback.
        /// </summary>
        public ValidationResult ValidateWithHandler(PlanAndBuildAppSettings appSettings, Action<ValidationResult> onError)
        {
            return _validationPipeline.ValidateWithHandler(appSettings, onError);
        }

        private ValidationResult ValidateInfo(PlanAndBuildAppSettings appSettings)
        {
            var result = new ValidationResult();

            if (!PlanAndBuildAppSettingsValidationHandler.IsValid(appSettings, out var infoResult))
            {
                result.AddFailuresFrom(infoResult);
            }

            return result;
        }
    }
}
