namespace Skyline.DataMiner.SDM.AssetManagement.Common.Validation
{
    using System;

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
        /// Validates that, when a cable length is provided, it is not negative.
        /// </summary>
        public static bool IsCableLengthValid(double? cableLength, out ValidationResult result)
        {
            result = new ValidationResult();

            if (cableLength.HasValue && cableLength.Value < 0)
            {
                result.AddFailReason(ConnectionValidationField.CableLength, "Cable length cannot be negative.");
            }

            return result.IsValid;
        }

        /// <summary>
        /// Validates the direction of a port used in a connection:
        /// a source port cannot be Input-only, a destination port cannot be Output-only.
        /// </summary>
        public static bool IsPortDirectionValid(SlcAsset_Management.Enums.Outputtype? outputType, bool isSource, out ValidationResult result)
        {
            result = new ValidationResult();

            if (isSource)
            {
                if (outputType == SlcAsset_Management.Enums.Outputtype.In)
                {
                    result.AddFailReason(ConnectionValidationField.SourcePort, "The source port must be of type Output or I/O.");
                }
            }
            else
            {
                if (outputType == SlcAsset_Management.Enums.Outputtype.Out)
                {
                    result.AddFailReason(ConnectionValidationField.DestinationPort, "The destination port must be of type Input or I/O.");
                }
            }

            return result.IsValid;
        }

        /// <summary>
        /// Validates that a connection does not link a port to itself.
        /// </summary>
        public static bool IsNotSelfConnection(Guid sourcePort, Guid destinationPort, out ValidationResult result)
        {
            result = new ValidationResult();

            if (sourcePort != Guid.Empty && sourcePort == destinationPort)
            {
                result.AddFailReason(ConnectionValidationField.SourcePort, "Source Port is the same as destination.");
                result.AddFailReason(ConnectionValidationField.DestinationPort, "Destination Port is the same as source.");
            }

            return result.IsValid;
        }

        /// <summary>
        /// Validates an endpoint asset of a connection: it must be provided, be in a usable state,
        /// have an Active Asset Class, and carry the tag required for the connection type
        /// (AcceptsDataConnection for data connections; PowerProvider for the source of a power connection).
        /// </summary>
        public static bool IsEndpointAssetValid(
            Asset asset,
            AssetClass assetClass,
            DeviceType deviceType,
            SlcAsset_Management.Enums.ConnectionType connectionType,
            bool isSource,
            out ValidationResult result)
        {
            var field = isSource ? ConnectionValidationField.SourceAsset : ConnectionValidationField.DestinationAsset;
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
                return result.IsValid;
            }

            if (assetClass == null)
            {
                result.AddFailReason(field, "The asset must have an asset class.");
                return result.IsValid;
            }

            if (assetClass.State != SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Active)
            {
                result.AddFailReason(field, "The asset's Asset Class must be active.");
                return result.IsValid;
            }

            if (connectionType == SlcAsset_Management.Enums.ConnectionType.Data)
            {
                if (deviceType == null || !deviceType.TagsInfo.Tags.Contains(SlcAsset_Management.Enums.TagOption.AcceptsDataConnection))
                {
                    result.AddFailReason(field, "The asset must accept data connections.");
                }
            }
            else if (connectionType == SlcAsset_Management.Enums.ConnectionType.Power && isSource)
            {
                if (deviceType == null || !deviceType.TagsInfo.Tags.Contains(SlcAsset_Management.Enums.TagOption.PowerProvider))
                {
                    result.AddFailReason(field, "The asset must be a Power Provider.");
                }
            }

            return result.IsValid;
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
