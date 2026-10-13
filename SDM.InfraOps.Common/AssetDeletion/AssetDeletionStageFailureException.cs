namespace Skyline.DataMiner.SDM.InfraOps.Orchestration.AssetDeletion
{
    using System;

    public sealed class AssetDeletionStageFailureException : Exception
    {
        public AssetDeletionStageFailureException(string assetIdentifier, string stage, Exception innerException)
            : base($"Asset '{assetIdentifier}' cleanup failed at stage '{stage}': {innerException.Message}", innerException)
        {
            AssetIdentifier = assetIdentifier;
            Stage = stage;
        }

        public string AssetIdentifier { get; }

        public string Stage { get; }
    }
}
