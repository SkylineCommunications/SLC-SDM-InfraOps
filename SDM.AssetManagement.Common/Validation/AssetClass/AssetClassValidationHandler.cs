namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
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
            if(!asset.Depth.HasValue)
            {
                result = new ValidationResult();
                return true;
            }

            return NumericValidators.ValidateNonNegative(
                asset.Depth.Value,
                AssetClassValidationField.Depth,
                out result);
        }

        public static bool IsWidthValid(AssetClass asset, out ValidationResult result)
        {
            if(!asset.Width.HasValue)
            {
                result = new ValidationResult();
                return true;
            }

            return NumericValidators.ValidateNonNegative(
                asset.Width.Value,
                AssetClassValidationField.Width,
                out result);
        }

        public static bool IsHeightValid(AssetClass asset, out ValidationResult result)
        {
            if(!asset.Height.HasValue)
            {
                result = new ValidationResult();
                return true;
            }

            return NumericValidators.ValidateNonNegative(
                asset.Height.Value,
                AssetClassValidationField.Height,
                out result);
        }

        public static bool IsHeightUnitValid(AssetClass asset, out ValidationResult result)
        {
            if(!asset.HeightU.HasValue)
            {
                result = new ValidationResult();
                return true;
            }

            return NumericValidators.ValidateNonNegative(
                asset.HeightU.Value,
                AssetClassValidationField.HeightU,
                out result);
        }

        /// <summary>
        /// Validates HeightU in the context of the Device Type's Rack Unit Consumer tag.
        /// A Rack Unit Consumer must have a Height Unit greater than 0.
        /// </summary>
        /// <remarks>
        /// Not part of the public surface: whether the Asset Class is a Rack Unit Consumer is derived from the
        /// AssetClass -> Device Type relation, which requires database access. Callers that have already resolved
        /// the Device Type pass the flag in; public callers should use <see cref="IsHeightUnitValid(AssetClass, out ValidationResult)"/>.
        /// </remarks>
        internal static bool IsHeightUnitValid(AssetClass asset, bool isRackUnitConsumer, out ValidationResult result)
        {
            result = new ValidationResult();

            if (isRackUnitConsumer && (!asset.HeightU.HasValue || asset.HeightU.Value <= 0))
            {
                result.AddFailReason(AssetClassValidationField.HeightU,
                    "Asset Class with 'Rack Unit Consumer' Device Type must have a Height Unit greater than 0.");
                return result.IsValid;
            }

            return IsHeightUnitValid(asset, out result);
        }

        public static bool IsWeightValid(AssetClass asset, out ValidationResult result)
        {
            if(!asset.Weight.HasValue)
            {
                result = new ValidationResult();
                return true;
            }

            return NumericValidators.ValidateNonNegative(
                asset.Weight.Value,
                AssetClassValidationField.Weight,
                out result);
        }

        public static bool IsTypicalPowerConsumptionValid(AssetClass asset, out ValidationResult result)
        {
            if(!asset.TypicalPowerConsumption.HasValue)
            {
                result = new ValidationResult();
                return true;
            }

            return NumericValidators.ValidateNonNegative(
                asset.TypicalPowerConsumption.Value,
                AssetClassValidationField.TypicalPowerConsumption,
                out result);
        }

        public static bool IsMaxPowerConsumptionValid(AssetClass asset, out ValidationResult result)
        {
            if(!asset.MaximumPowerConsumption.HasValue)
            {
                result = new ValidationResult();
                return true;
            }

            return NumericValidators.ValidateNonNegative(
                asset.MaximumPowerConsumption.Value,
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