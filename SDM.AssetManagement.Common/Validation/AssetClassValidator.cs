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
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Public validator service for AssetClass validation with comprehensive error handling.
    /// </summary>
    public class AssetClassValidator
    {
        private readonly IAssetClassQueryRepository _assetClassRepository;
        private readonly IDeviceTypeQueryRepository _deviceTypeRepository;
        private readonly Validator<AssetClass> _validationPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetClassValidator"/> class.
        /// </summary>
        /// <param name="assetClassRepository">Repository for querying asset classes.</param>
        /// <param name="deviceTypeRepository">Repository for querying device types.</param>
        public AssetClassValidator(IAssetClassQueryRepository assetClassRepository, IDeviceTypeQueryRepository deviceTypeRepository)
        {
            _assetClassRepository = assetClassRepository ?? throw new ArgumentNullException(nameof(assetClassRepository));
            _deviceTypeRepository = deviceTypeRepository ?? throw new ArgumentNullException(nameof(deviceTypeRepository));

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
            return IsAssetClassNameValid(assetClass.DeviceName, new List<string> { assetClass.Identifier });
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
            if (assetClass.DeviceNameField.Changed)
            {
                result.AddFailuresFrom(IsAssetClassNameValid(assetClass));
            }

            // Device Type is critical
            if (assetClass.DeviceTypeIdField.Changed)
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

            var deviceType = LoadDeviceType(assetClass.DeviceTypeId);

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
            var result = new ValidationResult();

            if (assetClass.DepthField.Changed
                && !AssetClassValidationHandler.IsDepthValid(assetClass, out var depthResult))
            {
                result.AddFailuresFrom(depthResult);
            }

            if (assetClass.WidthField.Changed
                && !AssetClassValidationHandler.IsWidthValid(assetClass, out var widthResult))
            {
                result.AddFailuresFrom(widthResult);
            }

            if (assetClass.HeightField.Changed
                && !AssetClassValidationHandler.IsHeightValid(assetClass, out var heightResult))
            {
                result.AddFailuresFrom(heightResult);
            }

            if (assetClass.HeightUField.Changed
                && !AssetClassValidationHandler.IsHeightUnitValid(assetClass, out var heightUResult))
            {
                result.AddFailuresFrom(heightUResult);
            }

            if (assetClass.WeightField.Changed
                && !AssetClassValidationHandler.IsWeightValid(assetClass, out var weightResult))
            {
                result.AddFailuresFrom(weightResult);
            }

            return result;
        }

        private ValidationResult ValidatePowerConsumption(AssetClass assetClass)
        {
            var result = new ValidationResult();

            if (assetClass.TypicalPowerConsumptionField.Changed
                && !AssetClassValidationHandler.IsTypicalPowerConsumptionValid(assetClass, out var typicalResult))
            {
                result.AddFailuresFrom(typicalResult);
            }

            if (assetClass.MaximumPowerConsumptionField.Changed
                && !AssetClassValidationHandler.IsMaxPowerConsumptionValid(assetClass, out var maxResult))
            {
                result.AddFailuresFrom(maxResult);
            }

            return result;
        }

        private ValidationResult ValidateCollections(AssetClass assetClass)
        {
            var result = new ValidationResult();

            if (assetClass.DataPortsField.Changed)
            {
                result.AddFailuresFrom(AssetClassValidationHandler.ValidateAssetClassDataPort(assetClass));
            }

            if (assetClass.PowerPortsField.Changed)
            {
                result.AddFailuresFrom(AssetClassValidationHandler.ValidateAssetClassPowerPort(assetClass));
            }

            if (assetClass.HoldersField.Changed)
            {
                result.AddFailuresFrom(AssetClassValidationHandler.ValidateAssetClassHolders(assetClass));
            }

            return result;
        }

        #endregion

        #region Helper Methods

        private DeviceType LoadDeviceType(SdmObjectReference<DeviceType> reference)
        {
            try
            {
                var filter = DeviceTypeExposers.Identifier.Equal(reference.Identifier);
                return _deviceTypeRepository.Read(filter).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load DeviceType: {ex.Message}", ex);
            }
        }

        private bool IsNameInUse(string name, List<string> exceptIdentifiers = null)
        {
            FilterElement<AssetClass> filter = AssetClassExposers.DeviceName.Equal(name);

            if (exceptIdentifiers != null && exceptIdentifiers.Any())
            {
                var clauses = exceptIdentifiers
                    .Select(id => AssetClassExposers.Identifier.NotEqual(id))
                    .Cast<FilterElement<AssetClass>>()
                    .ToArray();
                filter = filter.AND(new ANDFilterElement<AssetClass>(clauses));
            }

            return _assetClassRepository.Count(filter) > 0;
        }

        #endregion
    }
}