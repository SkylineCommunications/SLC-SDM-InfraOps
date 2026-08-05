namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using System.Collections.Generic;
    using System.Linq;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Static validation handler for DeviceType business rules.
    /// Contains pure validation logic without data access.
    /// </summary>
    public static class DeviceTypeValidationHandler
    {
        public enum DeviceTypeValidationField
        {
            Name,
            DeviceType,
        }

        /// <summary>
        /// Validates that a DeviceType can be deleted based on the state of assets referencing it.
        /// </summary>
        public static bool CanDelete(IEnumerable<Asset> referencingAssets, out ValidationResult result)
        {
            result = new ValidationResult();

            if (referencingAssets != null
                && referencingAssets.Any(asset => asset.State != SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Disposed))
            {
                result.AddFailReason(DeviceTypeValidationField.DeviceType, "There are already assets assigned to this device type not in the 'Disposed' State");
            }

            return result.IsValid;
        }
    }
}
