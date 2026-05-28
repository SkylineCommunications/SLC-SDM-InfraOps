namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using System.Collections.Generic;
    using System.Linq;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
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

        public static bool IsAssetClassDeviceTypeValid(AssetClass asset, out ValidationResult result)
        {
            result = new ValidationResult();
            if (!asset.DeviceTypeId.HasValue())
            {
                result.AddFailReason(AssetClassValidationField.DeviceTypeId, "Asset Class Device Type id needs to be a Guid.");
            }
            return result.IsValid;
        }

        public static bool IsDepthValid(AssetClass asset, out ValidationResult result)
        {
            return NumericValidators.ValidateNonNegative(
                asset.Depth,
                AssetClassValidationField.Depth,
                out result);
        }

        public static bool IsWidthValid(AssetClass asset, out ValidationResult result)
        {
            return NumericValidators.ValidateNonNegative(
                asset.Width,
                AssetClassValidationField.Width,
                out result);
        }

        public static bool IsHeightValid(AssetClass asset, out ValidationResult result)
        {
            return NumericValidators.ValidateNonNegative(
                asset.Height,
                AssetClassValidationField.Height,
                out result);
        }

        public static bool IsHeightUnitValid(AssetClass asset, out ValidationResult result)
        {
            return NumericValidators.ValidateNonNegative(
                asset.HeightU,
                AssetClassValidationField.HeightU,
                out result);
        }

        public static bool IsWeightValid(AssetClass asset, out ValidationResult result)
        {
            return NumericValidators.ValidateNonNegative(
                asset.Weight,
                AssetClassValidationField.Weight,
                out result);
        }

        public static bool IsTypicalPowerConsumptionValid(AssetClass asset, out ValidationResult result)
        {
            return NumericValidators.ValidateNonNegative(
                asset.TypicalPowerConsumption,
                AssetClassValidationField.TypicalPowerConsumption,
                out result);
        }

        public static bool IsMaxPowerConsumptionValid(AssetClass asset, out ValidationResult result)
        {
            return NumericValidators.ValidateNonNegative(
                asset.MaximumPowerConsumption,
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