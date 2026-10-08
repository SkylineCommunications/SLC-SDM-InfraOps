namespace Skyline.DataMiner.SDM.InfraOps.Orchestration.AssetDeletion
{
    using System;

    using Skyline.DataMiner.SDM.AssetManagement.Deletion;

    internal sealed class AssetDeletionCleanupPreparation
    {
        public AssetDeletionCleanupPreparation(
            AssetDeletionRecoveryContext recoveryContext,
            AssetDeletionSnapshot snapshot,
            Guid linkedObjectId)
        {
            RecoveryContext = recoveryContext;
            Snapshot = snapshot;
            LinkedObjectId = linkedObjectId;
        }

        public AssetDeletionRecoveryContext RecoveryContext { get; }

        public AssetDeletionSnapshot Snapshot { get; }

        public Guid LinkedObjectId { get; }
    }
}
