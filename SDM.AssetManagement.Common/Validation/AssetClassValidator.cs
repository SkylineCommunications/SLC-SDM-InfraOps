namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

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
        public ValidationResult Validate(AssetClass assetClass)
        {
            if (assetClass == null)
            {
                throw new ArgumentNullException(nameof(assetClass));
            }

            // Modular validation - each concern separated
            List<Func<ValidationResult>> validations = new List<Func<ValidationResult>>()
            {
                () => ValidateInfo(assetClass),
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

        /// <summary>
        /// Validates name uniqueness. Useful for real-time UI validation.
        /// </summary>
        public ValidationResult ValidateNameUniqueness(AssetClass assetClass)
        {
            return ValidateNameUniqueness(assetClass.DeviceName, new List<string> { assetClass.Identifier });
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

        private static ValidationResult ValidateAssetClassInfo(AssetClass assetClass)
        {
            var validationFactory = ValidationFactory<AssetClassWrapper>
                .PrepareValidation(
                (dat) => dat.Object.NameField.Changed,
                (dat) =>
                {
                    IsAssetClassNameValid(dat.Object.ModuleHandlers, dat.Object.Name, dat.Context, out var result);
                    return result;
                })
                .AddValidation(
                (dat) => !dat.Object.HasDeviceType || dat.Object.DeviceTypeIdField.Changed || dat.Object.PowerSupplyField.Changed,
                (dat) =>
                {
                    ValidationResult result = new ValidationResult();
                    if (!IsAssetClassDeviceTypeValid(dat.Object, out var deviceTypeResult))
                    {
                        result.CombineResults(deviceTypeResult);
                    }

                    if (dat.Context.ReturnWhenInvalid && !result.IsValid)
                    {
                        return result;
                    }

                    if (dat.Object.HasDeviceType && !IsAssetClassPowerSupplyValid(dat.Object, out var powerSupplyResult))
                    {
                        result.CombineResults(powerSupplyResult);
                    }

                    return result;
                })
                .AddValidation(
                (dat) => dat.Object.DepthField.Changed,
                (dat) =>
                {
                    IsDepthValid(dat.Object, out var result);
                    return result;
                })
                .AddValidation(
                (dat) => dat.Object.WidthField.Changed,
                (dat) =>
                {
                    IsWidthValid(dat.Object, out var result);
                    return result;
                })
                .AddValidation(
                (dat) => dat.Object.HeightField.Changed,
                (dat) =>
                {
                    IsHeightValid(dat.Object, out var result);
                    return result;
                })
                .AddValidation(
                (dat) => dat.Object.HeightUField.Changed,
                (dat) =>
                {
                    IsHeightUnitValid(dat.Object, out var result);
                    return result;
                })
                .AddValidation(
                (dat) => dat.Object.WeightField.Changed,
                (dat) =>
                {
                    IsWeightValid(dat.Object, out var result);
                    return result;
                })
                .AddValidation(
                (dat) => dat.Object.TypicalPowerConsumptionField.Changed,
                (dat) =>
                {
                    IsTypicalPowerConsumptionValid(dat.Object, out var result);
                    return result;
                })
                .AddValidation(
                (dat) => dat.Object.MaximumPowerConsumptionField.Changed,
                (dat) =>
                {
                    IsMaxPowerConsumptionValid(dat.Object, out var result);
                    return result;
                });

            validationFactory.Validate(assetClass, context, out var assetClassValidationResult);
            return assetClassValidationResult;
        }

        private ValidationResult ValidateInfo(AssetClass assetClass)
        {
            var result = new ValidationResult();

            // Name validation
            if (assetClass.DeviceNameField.Changed)
            {
                result.CombineResults(ValidateNameUniqueness(assetClass));
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