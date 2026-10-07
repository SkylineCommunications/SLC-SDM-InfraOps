namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using System;

    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Public validator service for AssetManagerAppSettings validation.
    /// No repository access is required since production defines no business rules for this entity.
    /// </summary>
    public class AssetManagerAppSettingsValidator
    {
        private readonly Validator<AssetManagerAppSettings> _validationPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetManagerAppSettingsValidator"/> class.
        /// </summary>
        public AssetManagerAppSettingsValidator()
        {
            _validationPipeline = Validator<AssetManagerAppSettings>.Create(ValidateInfo);
        }

        /// <summary>
        /// Validates an AssetManagerAppSettings and returns a ValidationResult.
        /// Collects all errors without throwing exceptions.
        /// </summary>
        public ValidationResult Validate(AssetManagerAppSettings appSettings)
        {
            if (appSettings == null)
            {
                throw new ArgumentNullException(nameof(appSettings));
            }

            return _validationPipeline.Validate(appSettings);
        }

        /// <summary>
        /// Validates an AssetManagerAppSettings and throws a ValidationException if invalid.
        /// Use this when you want fail-fast behavior.
        /// </summary>
        public void ValidateAndThrow(AssetManagerAppSettings appSettings)
        {
            _validationPipeline.ValidateAndThrow(appSettings);
        }

        /// <summary>
        /// Validates with a custom error handling callback.
        /// </summary>
        public ValidationResult ValidateWithHandler(AssetManagerAppSettings appSettings, Action<ValidationResult> onError)
        {
            return _validationPipeline.ValidateWithHandler(appSettings, onError);
        }

        private ValidationResult ValidateInfo(AssetManagerAppSettings appSettings)
        {
            var result = new ValidationResult();

            if (!AssetManagerAppSettingsValidationHandler.IsValid(appSettings, out var infoResult))
            {
                result.AddFailuresFrom(infoResult);
            }

            return result;
        }
    }
}