namespace Skyline.DataMiner.SDM.FacilityManagement.Validation
{
    using System;

    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Public validator service for FacilityManagerAppSettings validation.
    /// No repository access is required since production defines no business rules for this entity.
    /// </summary>
    public class FacilityManagerAppSettingsValidator
    {
        private readonly Validator<FacilityManagerAppSettings> _validationPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="FacilityManagerAppSettingsValidator"/> class.
        /// </summary>
        public FacilityManagerAppSettingsValidator()
        {
            _validationPipeline = Validator<FacilityManagerAppSettings>.Create(ValidateInfo);
        }

        /// <summary>
        /// Validates a FacilityManagerAppSettings and returns a ValidationResult.
        /// Collects all errors without throwing exceptions.
        /// </summary>
        public ValidationResult Validate(FacilityManagerAppSettings appSettings)
        {
            if (appSettings == null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            return _validationPipeline.Validate(appSettings);
        }

        /// <summary>
        /// Validates a FacilityManagerAppSettings and throws a ValidationException if invalid.
        /// Use this when you want fail-fast behavior.
        /// </summary>
        public void ValidateAndThrow(FacilityManagerAppSettings appSettings)
        {
            _validationPipeline.ValidateAndThrow(appSettings);
        }

        /// <summary>
        /// Validates with a custom error handling callback.
        /// </summary>
        public ValidationResult ValidateWithHandler(FacilityManagerAppSettings appSettings, Action<ValidationResult> onError)
        {
            return _validationPipeline.ValidateWithHandler(appSettings, onError);
        }

        private ValidationResult ValidateInfo(FacilityManagerAppSettings appSettings)
        {
            var result = new ValidationResult();

            if (!FacilityManagerAppSettingsValidationHandler.IsValid(appSettings, out var infoResult))
            {
                result.AddFailuresFrom(infoResult);
            }

            return result;
        }
    }
}
