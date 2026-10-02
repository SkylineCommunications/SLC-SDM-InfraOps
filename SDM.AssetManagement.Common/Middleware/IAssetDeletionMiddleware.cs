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
        void RecoverAssetDeletion(string assetIdentifier);

        /// <summary>
        /// Retries dependency cleanup for an Asset whose DOM deletion has already succeeded.
        /// </summary>
        /// <param name="context">The context for the Asset deletion recovery.</param>
        void RecoverAssetDeletion(AssetDeletionRecoveryContext context);
    }
}
