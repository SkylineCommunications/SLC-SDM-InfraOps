namespace Skyline.DataMiner.SDM.PlanAndBuild.Deletion
{
    using Skyline.DataMiner.SDM.AssetManagement.Deletion;

    /// <summary>
    /// Removes Asset references and records Connection snapshots in Plan &amp; Build Jobs.
    /// </summary>
    internal interface IAssetDeletionJobCleanup
    {
        /// <summary>
        /// Applies the captured policy to existing Asset and Connection entries on Jobs.
        /// </summary>
        /// <param name="context">The Asset metadata, policy and Connection snapshots.</param>
        void RemoveDeletedAssetReferences(AssetDeletionRecoveryContext context);
    }
}
