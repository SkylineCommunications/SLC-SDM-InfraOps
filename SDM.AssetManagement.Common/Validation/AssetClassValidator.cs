namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.AssetManagement.Common.Validation;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.AssetManagement.Repositories;
    using Skyline.DataMiner.SDM.Common.Services;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Public validator service for AssetClass validation with comprehensive error handling.
    /// </summary>
    public class AssetClassValidator
    {
        private readonly SdmEntityLoader _entityLoader;
        private readonly Validator<AssetClass> _validationPipeline;


        /// <summary>
        /// Initializes a new instance of the <see cref="AssetClassValidator"/> class.
        /// </summary>
        /// <param name="assetClassRepository">Repository for querying asset classes.</param>
        /// <param name="deviceTypeRepository">Repository for querying device types.</param>
        public AssetClassValidator(SdmEntityLoader entityLoader)
        {
            _entityLoader = entityLoader ?? throw new ArgumentNullException(nameof(entityLoader));
            _validationPipeline = BuildValidationPipeline();
        }

        /// <summary>
        /// Validates an AssetClass and returns ValidationResult.
        /// Collects all errors without throwing exceptions.
        /// </summary>
        public ValidationResult Validate(AssetClass assetClass)
        {
            if (assetClass == null)
            {
                throw new ArgumentNullException(nameof(assetClass));
            }

            return _validationPipeline.Validate(assetClass);
        }

        /// <summary>
        /// Validates an AssetClass and throws ValidationException if invalid.
        /// Use this when you want fail-fast behavior.
        /// </summary>
        public void ValidateAndThrow(AssetClass assetClass)
        {
            _validationPipeline.ValidateAndThrow(assetClass);
        }

        /// <summary>
        /// Validates with custom error handling callback.
        /// </summary>
        public ValidationResult ValidateWithHandler(AssetClass assetClass, Action<ValidationResult> onError)
        {
            return _validationPipeline.ValidateWithHandler(assetClass, onError);
        }

        /// <summary>
        /// Validates name uniqueness - used for real-time UI validation.
        /// </summary>
        public ValidationResult IsAssetClassNameValid(string name, List<string> exceptIdentifiers = null)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(name))
            {
                result.AddFailReason(AssetClassValidationHandler.AssetClassValidationField.Name,
                    "Asset Class Name cannot be empty or whitespace.");
                return result;
            }

            if (IsNameInUse(name, exceptIdentifiers))
            {
                result.AddFailReason(AssetClassValidationHandler.AssetClassValidationField.Name,
                    $"Asset Class Name '{name}' is already in use.");
            }

            return result;
        }

        /// <summary>
        /// Validates the uniqueness of the AssetClass name for the specified <see cref="AssetClass"/> instance.
        /// Excludes the current asset class identifier from the uniqueness check.
        /// </summary>
        /// <param name="assetClass">The asset class to validate.</param>
        /// <returns>A <see cref="ValidationResult"/> indicating whether the asset class name is valid.</returns>
        public ValidationResult IsAssetClassNameValid(AssetClass assetClass)
        {
            return IsAssetClassNameValid(assetClass.Name, new List<string> { assetClass.Identifier });
        }

        #region Pipeline Construction

        private Validator<AssetClass> BuildValidationPipeline()
        {
            // Critical validations - stop on failure
            var criticalValidations = Validator<AssetClass>
                .Create(ValidateCriticalFields)
                .StopOnFailure();

            // Standard validations - collect all errors
            var standardValidations = Validator<AssetClass>
                .Create(ValidateInfo)
                .AndThen(ValidateDimensions)
                .AndThen(ValidatePowerConsumption)
                .AndThen(ValidateCollections);

            // Combine: critical first, then standard
            return criticalValidations.AndThen(standardValidations);
        }

        #endregion

        #region Validation Methods

        private ValidationResult ValidateCriticalFields(AssetClass assetClass)
        {
            var result = new ValidationResult();

            // Name is critical - must be valid before other checks
            if (assetClass.ShouldValidate(assetClass.NameField))
            {
                result.AddFailuresFrom(IsAssetClassNameValid(assetClass));
            }

            // Device Type is critical
            if (assetClass.ShouldValidate(assetClass.DeviceTypeIdField))
            {
                if (!AssetClassValidationHandler.IsAssetClassDeviceTypeValid(assetClass, out var deviceTypeResult))
                {
                    result.AddFailuresFrom(deviceTypeResult);
                }
            }

            return result;
        }

        private ValidationResult ValidateInfo(AssetClass assetClass)
        {
            var result = new ValidationResult();

            // Power Supply validation (if device type or power supply changed)
            if ((assetClass.DeviceTypeIdField.Changed || assetClass.PowerSupplyField.Changed)
                && assetClass.DeviceTypeId.HasValue())
            {
                try
                {
                    result.AddFailuresFrom(ValidatePowerSupply(assetClass));
                }
                catch (Exception ex)
                {
                    result.AddFailReason(AssetClassValidationHandler.AssetClassValidationField.PowerSupply,
                        $"Error validating power supply: {ex.Message}");
                }
            }

            return result;
        }

        private ValidationResult ValidatePowerSupply(AssetClass assetClass)
        {
            var result = new ValidationResult();

            if (!assetClass.DeviceTypeId.HasValue())
            {
                return result;
            }

            var deviceType = _entityLoader.LoadDeviceType(assetClass.DeviceTypeId);

            if (deviceType == null)
            {
                result.AddFailReason(AssetClassValidationHandler.AssetClassValidationField.DeviceTypeId,
                    "Device Type not found.");
                return result;
            }

            if (deviceType.TagsInfo.Tags.Contains(SlcAsset_Management.Enums.TagOption.PowerProvider) && assetClass.PowerSupply == null)
            {
                result.AddFailReason(AssetClassValidationHandler.AssetClassValidationField.PowerSupply,
                    "Asset Class with 'Power Provider' Device Type must have a Power Supply.");
            }

            return result;
        }

        private ValidationResult ValidateDimensions(AssetClass assetClass)
        {
            var validations = new List<ValidationResult>();

            if(assetClass.ShouldValidate(assetClass.DepthField))
            {
                if (!AssetClassValidationHandler.IsDepthValid(assetClass, out var depthResult))
                {
                    validations.Add(depthResult);
                }
            }

            if (assetClass.ShouldValidate(assetClass.WidthField))
            {
                if (!AssetClassValidationHandler.IsWidthValid(assetClass, out var widthResult))
                {
                    validations.Add(widthResult);
                }
            }

            if (assetClass.ShouldValidate(assetClass.HeightField))
            {
                if (!AssetClassValidationHandler.IsHeightValid(assetClass, out var heightResult))
                {
                    validations.Add(heightResult);
                }
            }

            if (assetClass.ShouldValidate(assetClass.HeightUField))
            {
                if (!AssetClassValidationHandler.IsHeightUnitValid(assetClass, out var heightUResult))
                {
                    validations.Add(heightUResult);
                }
            }

            if (assetClass.ShouldValidate(assetClass.WeightField))
            {
                if (!AssetClassValidationHandler.IsWeightValid(assetClass, out var weightResult))
                {
                    validations.Add(weightResult);
                }
            }

            return validations.MergeAll();
        }

        private ValidationResult ValidatePowerConsumption(AssetClass assetClass)
        {
            var validations = new List<ValidationResult>();

            if (assetClass.ShouldValidate(assetClass.TypicalPowerConsumptionField))
            {
                if (!AssetClassValidationHandler.IsTypicalPowerConsumptionValid(assetClass, out var typicalResult))
                {
                    validations.Add(typicalResult);
                }
            }

            if (assetClass.ShouldValidate(assetClass.MaximumPowerConsumptionField))
            {
                if (!AssetClassValidationHandler.IsMaxPowerConsumptionValid(assetClass, out var maxResult))
                {
                    validations.Add(maxResult);
                }
            }

            return validations.MergeAll();
        }

        private ValidationResult ValidateCollections(AssetClass assetClass)
        {
            var validations = new List<ValidationResult>();

            if (assetClass.ShouldValidate(assetClass.DataPortsField))
            {
                validations.Add(AssetClassValidationHandler.ValidateAssetClassDataPort(assetClass));
            }

            if (assetClass.ShouldValidate(assetClass.PowerPortsField))
            {
                validations.Add(AssetClassValidationHandler.ValidateAssetClassPowerPort(assetClass));
            }

            if (assetClass.ShouldValidate(assetClass.HoldersField))
            {
                validations.Add(AssetClassValidationHandler.ValidateAssetClassHolders(assetClass));
            }

            return validations.MergeAll();
        }

        #endregion

        #region Helper Methods

        private bool IsNameInUse(string name, List<string> exceptIdentifiers = null)
        {
            return _entityLoader.CountAssetClassesByName(name, exceptIdentifiers) > 0;
        }

        #endregion
    }
}