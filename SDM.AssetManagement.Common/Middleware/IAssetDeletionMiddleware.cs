namespace Skyline.DataMiner.SDM.AssetManagement.Common.Middleware
{
    using Skyline.DataMiner.SDM.AssetManagement.Deletion;
    using Skyline.DataMiner.SDM.AssetManagement.Models;

    /// <summary>
    /// Provides the required Asset deletion cascade and its recovery operation.
    /// </summary>
    public interface IAssetDeletionMiddleware : IBulkRepositoryMiddleware<Asset>
    {
        /// <summary>
        /// Retries dependency cleanup for an Asset that has already been deleted.
        /// </summary>
        /// <param name="assetIdentifier">The Asset DOM identifier.</param>
        /// <remarks>
        /// Identifier-only recovery supports only the default deletion policy, which removes Asset Job
        /// entries and reads Connection snapshot cable tags from Connections that still exist.
        /// It cannot reconstruct snapshots of already-deleted Connections.
        /// Prefer <see cref="RecoverAssetDeletion(AssetDeletionRecoveryContext)"/> with the original failure
        /// context whenever available. Custom policies require that context.
        /// </remarks>
        void RecoverAssetDeletion(string assetIdentifier);

        /// <summary>
        /// Retries dependency cleanup for an Asset whose DOM deletion has already succeeded.
        /// </summary>
        /// <param name="context">The context for the Asset deletion recovery.</param>
        /// <remarks>
        /// Preferred for both default and custom policies because captured Asset metadata and Connection
        /// snapshots remain available even after their source objects have been deleted.
        /// Use the original context from the failed deletion outcome. Its policy must match this
        /// middleware's deletion policy, and the Asset must no longer exist.
        /// If recovery fails, use the context from the new failure outcome for the next attempt.
        /// </remarks>
        void RecoverAssetDeletion(AssetDeletionRecoveryContext context);
    }
}
