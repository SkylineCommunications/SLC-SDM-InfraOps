namespace Skyline.DataMiner.SDM.AssetManagement.Deletion
{
    using System;

    public enum JobEntryRemovalMode
    {
        Remove,
        KeepRemovedSnapshot,
    }

    public enum ConnectionSnapshotFormat
    {
        CableTags,
        AssetAndPortNames,
    }

    /// <summary>
    /// Immutable Job representation choices; live Asset dependencies are always cleaned.
    /// </summary>
    public sealed class AssetDeletionPolicy
    {
        public static AssetDeletionPolicy Default { get; } = new AssetDeletionPolicy();

        public AssetDeletionPolicy(
            JobEntryRemovalMode assetJobEntryMode = JobEntryRemovalMode.Remove,
            JobEntryRemovalMode connectionJobEntryMode = JobEntryRemovalMode.KeepRemovedSnapshot,
            ConnectionSnapshotFormat connectionSnapshotFormat = ConnectionSnapshotFormat.CableTags)
        {
            if (!Enum.IsDefined(typeof(JobEntryRemovalMode), assetJobEntryMode))
            {
                throw new ArgumentOutOfRangeException(nameof(assetJobEntryMode));
            }

            if (!Enum.IsDefined(typeof(JobEntryRemovalMode), connectionJobEntryMode))
            {
                throw new ArgumentOutOfRangeException(nameof(connectionJobEntryMode));
            }

            if (!Enum.IsDefined(typeof(ConnectionSnapshotFormat), connectionSnapshotFormat))
            {
                throw new ArgumentOutOfRangeException(nameof(connectionSnapshotFormat));
            }

            AssetJobEntryMode = assetJobEntryMode;
            ConnectionJobEntryMode = connectionJobEntryMode;
            ConnectionSnapshotFormat = connectionSnapshotFormat;
        }

        public JobEntryRemovalMode AssetJobEntryMode { get; }

        public JobEntryRemovalMode ConnectionJobEntryMode { get; }

        public ConnectionSnapshotFormat ConnectionSnapshotFormat { get; }

        public bool Matches(AssetDeletionPolicy other) =>
            other != null &&
            AssetJobEntryMode == other.AssetJobEntryMode &&
            ConnectionJobEntryMode == other.ConnectionJobEntryMode &&
            ConnectionSnapshotFormat == other.ConnectionSnapshotFormat;
    }
}
