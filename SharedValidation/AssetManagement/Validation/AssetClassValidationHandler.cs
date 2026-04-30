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
                result.AddFailReason(AssetClassValidationField.DeviceTypeId, "Asset Class Device Type cannot be empty.");
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

        //public static bool IsRackAttachable(AssetClass assetClass, out ValidationResult result)
        //{
        //    result = new ValidationResult();

        //    if (assetClass == null)
        //    {
        //        result.AddFailReason(AssetClassValidationField.AssetClass, "Asset Class must be provided.");
        //        return result.IsValid;
        //    }

        //    if (assetClass.Height <= 0)
        //    {
        //        result.AddFailReason(AssetClassValidationField.AssetClass, "Asset Class must have a defined height higher than 0.");
        //        return result.IsValid;
        //    }

        //    result.AddFailuresFrom(DeviceTypeValidationHandler.IsRackAttacheable(assetClass.DeviceType));
        //    return result.IsValid;
        //}

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
                result.AddFailReason(AssetClassValidationField.DataPortNumber, $"Asset Class does not contain Data Ports.");
                return result;
            }

            var seenPorts = new HashSet<long>();

            foreach (var port in assetClass.DataPorts)
            {
                if (port.DataPortInfo.PortNumber < 0)
                {
                    result.AddFailReason(AssetClassValidationField.DataPortNumber, "Data Port Number cannot be negative.");
                    return result;
                }

                if (!seenPorts.Add(port.DataPortInfo.PortNumber))
                {
                    result.AddFailReason(AssetClassValidationField.DataPortNumber, $"Multiple Data Ports have the same Port Number '{port.DataPortInfo.PortNumber}'.");
                    return result;
                }
            }

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
                result.AddFailReason(AssetClassValidationField.PowerPortNumber, $"Asset Class does not contain Power Ports.");
                return result;
            }

            var seenPorts = new HashSet<long>();

            foreach (var port in assetClass.PowerPorts)
            {
                if (port.PowerPortInfo.PortNumber < 0)
                {
                    result.AddFailReason(AssetClassValidationField.PowerPortNumber, "Power Port Number cannot be negative.");
                    return result;
                }

                if (!seenPorts.Add(port.PowerPortInfo.PortNumber))
                {
                    result.AddFailReason(AssetClassValidationField.PowerPortNumber, $"Multiple Power Ports have the same Port Number '{port.PowerPortInfo.PortNumber}'.");
                    return result;
                }
            }

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

            if (assetClass.Holders == null)
            {
                result.AddFailReason(AssetClassValidationField.HolderSlotNumber, $"Asset Class does not contain Holders.");
                return result;
            }

            var seenHolders = new HashSet<(long Number, SlcAsset_Management.Enums.HierarchyRoleEnum? HierarchyRole)>();

            foreach (var holder in assetClass.Holders)
            {
                if (holder.SlotNumber == null)
                {
                    result.AddFailReason(AssetClassValidationField.HolderSlotNumber, "Holder Slot Number must have a value.");
                    return result;
                }

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