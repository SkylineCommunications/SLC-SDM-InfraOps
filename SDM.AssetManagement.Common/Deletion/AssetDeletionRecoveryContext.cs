namespace Skyline.DataMiner.SDM.AssetManagement.Deletion
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    /// <summary>
    /// Caller-retained metadata and policy for cleanup after Asset DOM deletion.
    /// Contains no mutable SDM models and may be serialized by the caller.
    /// </summary>
    public sealed class AssetDeletionRecoveryContext
    {
        public AssetDeletionRecoveryContext(
            string assetIdentifier,
            AssetDeletionPolicy policy,
            string assetName,
            string assetClassName,
            string ipAddress,
            IReadOnlyDictionary<string, string> deletedAssetNames,
            IReadOnlyCollection<AssetDeletionConnectionSnapshot> connectionSnapshots)
        {
            if (!Guid.TryParse(assetIdentifier, out _))
            {
                throw new ArgumentException("Asset identifier must be a GUID.", nameof(assetIdentifier));
            }

            AssetIdentifier = assetIdentifier;
            Policy = policy ?? throw new ArgumentNullException(nameof(policy));
            AssetName = assetName;
            AssetClassName = assetClassName;
            IPAddress = ipAddress;
            DeletedAssetNames = new ReadOnlyDictionary<string, string>(
                (deletedAssetNames ?? throw new ArgumentNullException(nameof(deletedAssetNames)))
                .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase));
            var snapshots = (connectionSnapshots ?? throw new ArgumentNullException(nameof(connectionSnapshots))).ToList();
            if (snapshots.Any(snapshot => snapshot == null))
            {
                throw new ArgumentException("Connection snapshots cannot contain null.", nameof(connectionSnapshots));
            }

            ConnectionSnapshots = snapshots.AsReadOnly();
        }

        public string AssetIdentifier { get; }

        public AssetDeletionPolicy Policy { get; }

        public string AssetName { get; }

        public string AssetClassName { get; }

        public string IPAddress { get; }

        public IReadOnlyDictionary<string, string> DeletedAssetNames { get; }

        public IReadOnlyCollection<AssetDeletionConnectionSnapshot> ConnectionSnapshots { get; }

        public AssetDeletionRecoveryContext WithConnectionSnapshots(
            IReadOnlyCollection<AssetDeletionConnectionSnapshot> snapshots) =>
            new AssetDeletionRecoveryContext(
                AssetIdentifier, Policy, AssetName, AssetClassName, IPAddress, DeletedAssetNames, snapshots);
    }

    public sealed class AssetDeletionConnectionSnapshot
    {
        public AssetDeletionConnectionSnapshot(
            string connectionIdentifier,
            string source,
            string destination,
            string cableTypeIdentifier,
            double? cableLength)
        {
            if (!Guid.TryParse(connectionIdentifier, out _))
            {
                throw new ArgumentException("Connection identifier must be a GUID.", nameof(connectionIdentifier));
            }

            ConnectionIdentifier = connectionIdentifier;
            Source = source;
            Destination = destination;
            CableTypeIdentifier = cableTypeIdentifier;
            CableLength = cableLength;
        }

        public string ConnectionIdentifier { get; }

        public string Source { get; }

        public string Destination { get; }

        public string CableTypeIdentifier { get; }

        public double? CableLength { get; }
    }
}
