namespace Skyline.DataMiner.SDM.AssetManagement.Common.Validation
{
    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Static validation handler for Connection business rules.
    /// Contains pure validation logic without data access.
    /// </summary>
    public static class ConnectionValidationHandler
    {
        public enum ConnectionValidationField
        {
            CableLength,
            CableType,
            SourcePort,
            SourcePortType,
            SourceAsset,
            DestinationPort,
            DestinationPortType,
            DestinationAsset,
        }

        /// <summary>
        /// Validates that a source endpoint asset is in a usable state.
        /// </summary>
        public static bool IsSourceAssetStateValid(Asset asset, out ValidationResult result)
        {
            return IsEndpointAssetStateValid(asset, ConnectionValidationField.SourceAsset, out result);
        }

        /// <summary>
        /// Validates that a destination endpoint asset is in a usable state.
        /// </summary>
        public static bool IsDestinationAssetStateValid(Asset asset, out ValidationResult result)
        {
            return IsEndpointAssetStateValid(asset, ConnectionValidationField.DestinationAsset, out result);
        }

        /// <summary>
        /// Validates that an endpoint asset is provided and is not Not Available or Disposed.
        /// </summary>
        public static bool IsEndpointAssetStateValid(Asset asset, ConnectionValidationField field, out ValidationResult result)
        {
            result = new ValidationResult();

            if (asset == null)
            {
                result.AddFailReason(field, "The asset must be provided.");
                return result.IsValid;
            }

            if (asset.State == SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable
                || asset.State == SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Disposed)
            {
                result.AddFailReason(field, "The asset must not be in the 'Not Available' or 'Disposed' state.");
            }

            return result.IsValid;
        }
    }
}
