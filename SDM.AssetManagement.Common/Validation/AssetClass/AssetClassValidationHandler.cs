namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;


    public static class AssetClassValidationHandler
    {
        public enum AssetClassValidationField
        {
            State,
            Name,
            AssetClass,
            DeviceTypeId,
            DeviceTypeName,
            PowerSupply,

            Depth,
            Width,
            Height,
            HeightU,
            Weight,

            TypicalPowerConsumption,
            MaxPowerConsumption,

            DataPortNumber,

            PowerPortNumber,

            HolderSlotNumber,
        }

        #region Info Validators

        public static bool IsAssetClassDeviceTypeValid(AssetClass assetClass, out ValidationResult result)
        {
            result = new ValidationResult();
            if (!assetClass.DeviceTypeId.HasValue())
            {
                result.AddFailReason(AssetClassValidationField.DeviceTypeId, "Asset Class Device Type id needs to be a Guid.");
            }
            return result.IsValid;
        }

        public static bool IsDepthValid(AssetClass assetClass, out ValidationResult result)
        {
            if(!assetClass.Depth.HasValue)
            {
                result = new ValidationResult();
                return true;
            }

            return NumericValidators.ValidateNonNegative(
                assetClass.Depth.Value,
                AssetClassValidationField.Depth,
                out result);
        }

        public static bool IsWidthValid(AssetClass assetClass, out ValidationResult result)
        {
            if(!assetClass.Width.HasValue)
            {
                result = new ValidationResult();
                return true;
            }

            return NumericValidators.ValidateNonNegative(
                assetClass.Width.Value,
                AssetClassValidationField.Width,
                out result);
        }

        public static bool IsHeightValid(AssetClass assetClass, out ValidationResult result)
        {
            if(!assetClass.Height.HasValue)
            {
                result = new ValidationResult();
                return true;
            }

            return NumericValidators.ValidateNonNegative(
                assetClass.Height.Value,
                AssetClassValidationField.Height,
                out result);
        }

        public static bool IsHeightUnitValid(AssetClass assetClass, DeviceType deviceType, out ValidationResult result)
        {
            if(assetClass.DeviceTypeId != deviceType.Identifier)
            {
                throw new InvalidOperationException("The provided DeviceType does not match the DeviceTypeId of the AssetClass. Please ensure that the correct DeviceType is provided for validation.");
            }

            bool isRackUnitConsumer = deviceType.TagsInfo?.Tags?.Contains(SlcAsset_Management.Enums.TagOption.RackUnitConsumer) ?? false;
            if (!isRackUnitConsumer)
            {
                return ValidateHeightUnitBusinessRules(assetClass, out result);
            }
            else
            {
                return IsHeightUnitValidForRackConsumer(assetClass, out result);
            }
        }


        internal static bool ValidateHeightUnitBusinessRules(AssetClass assetClass, out ValidationResult result)
        {
            if (!assetClass.HeightU.HasValue)
            {
                result = new ValidationResult();
                return true;
            }

            return NumericValidators.ValidateNonNegative(
                    assetClass.HeightU.Value,
                    AssetClassValidationField.HeightU,
                    out result);
        }

        /// <summary>
        /// Validates HeightU in the context of the Device Type's Rack Unit Consumer tag.
        /// A Rack Unit Consumer must have a Height Unit greater than 0.
        /// </summary>
        private static bool IsHeightUnitValidForRackConsumer(AssetClass assetClass, out ValidationResult result)
        {
            result = new ValidationResult();

            if (!assetClass.HeightU.HasValue || assetClass.HeightU.Value <= 0)
            {
                result.AddFailReason(AssetClassValidationField.HeightU,
                    "Asset Class with 'Rack Unit Consumer' Device Type must have a Height Unit greater than 0.");
                return result.IsValid;
            }

            return true;
        }

        public static bool IsWeightValid(AssetClass assetClass, out ValidationResult result)
        {
            if(!assetClass.Weight.HasValue)
            {
                result = new ValidationResult();
                return true;
            }

            return NumericValidators.ValidateNonNegative(
                assetClass.Weight.Value,
                AssetClassValidationField.Weight,
                out result);
        }

        public static bool IsTypicalPowerConsumptionValid(AssetClass assetClass, out ValidationResult result)
        {
            if(!assetClass.TypicalPowerConsumption.HasValue)
            {
                result = new ValidationResult();
                return true;
            }

            return NumericValidators.ValidateNonNegative(
                assetClass.TypicalPowerConsumption.Value,
                AssetClassValidationField.TypicalPowerConsumption,
                out result);
        }

        public static bool IsMaxPowerConsumptionValid(AssetClass assetClass, out ValidationResult result)
        {
            if(!assetClass.MaximumPowerConsumption.HasValue)
            {
                result = new ValidationResult();
                return true;
            }

            return NumericValidators.ValidateNonNegative(
                assetClass.MaximumPowerConsumption.Value,
                AssetClassValidationField.MaxPowerConsumption,
                out result);
        }
        #endregion

        #region Data Ports

        public static ValidationResult ValidateAssetClassDataPort(AssetClass assetClass)
        {
            var result = new ValidationResult();

            if (assetClass == null)
            {
                result.AddFailReason(AssetClassValidationField.DataPortNumber, $"An Asset Class must be provided.");
                return result;
            }

            if (!assetClass.DataPorts.Any())
            {
                return result;
            }

            result.AddFailuresFrom(PortNumberValidator.ValidateCollection(
                assetClass.DataPorts, p => p.PortNumber, AssetClassValidationField.DataPortNumber, "Data Port"));

            return result;
        }

        #endregion

        #region Power Ports

        public static ValidationResult ValidateAssetClassPowerPort(AssetClass assetClass)
        {
            ValidationResult result = new ValidationResult();

            if (assetClass == null)
            {
                result.AddFailReason(AssetClassValidationField.PowerPortNumber, $"An Asset Class must be provided.");
                return result;
            }

            if (!assetClass.PowerPorts.Any())
            {
               return result;
            }

            result.AddFailuresFrom(PortNumberValidator.ValidateCollection(
                assetClass.PowerPorts, p => p.PortNumber, AssetClassValidationField.PowerPortNumber, "Power Port"));

            return result;
        }

        #endregion

        #region Holders

        public static ValidationResult ValidateAssetClassHolders(AssetClass assetClass)
        {
            ValidationResult result = new ValidationResult();

            if (assetClass == null)
            {
                result.AddFailReason(AssetClassValidationField.HolderSlotNumber, $"An Asset Class must be provided.");
                return result;
            }

            var seenHolders = new HashSet<(long Number, SlcAsset_Management.Enums.HierarchyRoleEnum? HierarchyRole)>();

            foreach (var holder in assetClass.Holders)
            {
                if (holder.SlotNumber < 0)
                {
                    result.AddFailReason(AssetClassValidationField.HolderSlotNumber, "Holder Slot Number cannot be negative.");
                    return result;
                }

                var holderKey = (holder.SlotNumber, holder.HierarchyRole);
                if (!seenHolders.Add(holderKey))
                {
                    result.AddFailReason(AssetClassValidationField.HolderSlotNumber, $"Multiple Holders have the same Slot Number '{holder.SlotNumber}' and Hierarchy Role '{holder.HierarchyRole}'.");
                    return result;
                }
            }

            return result;
        }
        #endregion
    }
}