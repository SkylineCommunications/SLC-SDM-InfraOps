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
    /// Public validator service for AssetClass validation including data access for uniqueness checks.
    /// Can be used by both internal CRUD operations and external applications.
    /// </summary>
    public class AssetClassValidator
    {
        private readonly IAssetClassQueryRepository _assetClassRepository;
        private readonly IDeviceTypeQueryRepository _deviceTypeRepository;

        public AssetClassValidator(IAssetClassQueryRepository assetClassRepository, IDeviceTypeQueryRepository deviceTypeRepository)
        {
            _assetClassRepository = assetClassRepository ?? throw new ArgumentNullException(nameof(assetClassRepository));
            _deviceTypeRepository = deviceTypeRepository ?? throw new ArgumentNullException(nameof(deviceTypeRepository));
        }

        /// <summary>
        /// Validates an AssetClass (works for both Create and Update).
        /// Only validates fields that have Changed = true.
        /// </summary>
        /// <param name="assetClass">The asset class to validate.</param>
        /// <param name="exceptIdentifiers">Identifiers to exclude from uniqueness Name checks (e.g., the object being updated).</param>
        public ValidationResult Validate(AssetClass assetClass, List<string> exceptIdentifiers = null)
        {
            if (assetClass == null)
            {
                throw new ArgumentNullException(nameof(assetClass));
            }

            // Modular validation - each concern separated
            List<Func<ValidationResult>> validations = new List<Func<ValidationResult>>()
            {
                () => ValidateInfo(assetClass, exceptIdentifiers),
                () => ValidateLifecycle(assetClass),
                () => ValidateDimensions(assetClass),
                () => ValidatePowerConsumption(assetClass),
                () => ValidateCollections(assetClass),
            };

            ValidationResult result = new ValidationResult();
            foreach (var validation in validations)
            {
                result.CombineResults(validation());
            }

            return result;
        }

        /// <summary>
        /// Validates name uniqueness. Useful for real-time UI validation.
        /// </summary>
        public ValidationResult ValidateNameUniqueness(string name, List<string> exceptIdentifiers = null)
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

        #region Private Validation Methods

        private ValidationResult ValidatePowerSupply(AssetClass assetClass)
        {
            var result = new ValidationResult();

            if (!assetClass.DeviceTypeId.HasValue())
            {
                return result;
            }

            // Load the device type from repository
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

        private DeviceType LoadDeviceType(SdmObjectReference<DeviceType> reference)
        {
            var filter = DeviceTypeExposers.Identifier.Equal(reference.Identifier);
            return _deviceTypeRepository.Read(filter).FirstOrDefault();
        }

        private ValidationResult ValidateInfo(AssetClass assetClass, List<string> exceptIdentifiers)
        {
            var result = new ValidationResult();

            // Name validation
            if (assetClass.DeviceNameField.Changed)
            {
                result.CombineResults(ValidateNameUniqueness(assetClass.DeviceName, exceptIdentifiers));
            }

            if (assetClass.DeviceTypeIdField.Changed || assetClass.PowerSupplyField.Changed)
            {
                if (!AssetClassValidationHandler.IsAssetClassDeviceTypeValid(assetClass, out var deviceTypeResult))
                {
                    result.CombineResults(deviceTypeResult);
                }
                else if (assetClass.DeviceTypeId.HasValue())
                {
                    // Load device type and validate power supply
                    result.CombineResults(ValidatePowerSupply(assetClass));
                }
            }

            return result;
        }

        private ValidationResult ValidateLifecycle(AssetClass assetClass)
        {
            var result = new ValidationResult();

            // Add lifecycle validation when needed
            // if (assetClass.LifecycleField.Changed) { ... }

            return result;
        }

        private ValidationResult ValidateDimensions(AssetClass assetClass)
        {
            var result = new ValidationResult();

            if (assetClass.DepthField.Changed
                && !AssetClassValidationHandler.IsDepthValid(assetClass, out var depthResult))
            {
                result.CombineResults(depthResult);
            }

            if (assetClass.WidthField.Changed
                && !AssetClassValidationHandler.IsWidthValid(assetClass, out var widthResult))
            {
                result.CombineResults(widthResult);
            }

            if (assetClass.HeightField.Changed
                && !AssetClassValidationHandler.IsHeightValid(assetClass, out var heightResult))
            {
                result.CombineResults(heightResult);
            }

            if (assetClass.HeightUField.Changed
                && !AssetClassValidationHandler.IsHeightUnitValid(assetClass, out var heightUResult))
            {
                result.CombineResults(heightUResult);
            }

            if (assetClass.WeightField.Changed
                && !AssetClassValidationHandler.IsWeightValid(assetClass, out var weightResult))
            {
                result.CombineResults(weightResult);
            }

            return result;
        }

        private ValidationResult ValidatePowerConsumption(AssetClass assetClass)
        {
            var result = new ValidationResult();

            if (assetClass.TypicalPowerConsumptionField.Changed
                && !AssetClassValidationHandler.IsTypicalPowerConsumptionValid(assetClass, out var typicalResult))
            {
                result.CombineResults(typicalResult);
            }

            if (assetClass.MaximumPowerConsumptionField.Changed
                && !AssetClassValidationHandler.IsMaxPowerConsumptionValid(assetClass, out var maxResult))
            {
                result.CombineResults(maxResult);
            }

            return result;
        }

        private ValidationResult ValidateCollections(AssetClass assetClass)
        {
            var result = new ValidationResult();

            if (assetClass.DataPortsField.Changed)
            {
                result.CombineResults(AssetClassValidationHandler.ValidateAssetClassDataPort(assetClass));
            }

            if (assetClass.PowerPortsField.Changed)
            {
                result.CombineResults(AssetClassValidationHandler.ValidateAssetClassPowerPort(assetClass));
            }

            if (assetClass.HoldersField.Changed)
            {
                result.CombineResults(AssetClassValidationHandler.ValidateAssetClassHolders(assetClass));
            }

            return result;
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