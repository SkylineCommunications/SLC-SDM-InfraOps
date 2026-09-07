namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using SharedMappers.DomIds;

    internal sealed partial class AssetClassDomRepository_Middleware : IAssetClassRepository
    {
        public AssetClass TransitionTo(
            AssetClass assetClass,
            SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum newState)
        {
            return ((IAssetClassRepository)_inner).TransitionTo(assetClass, newState);
        }

        public AssetClass ReadAssetClassById(string id)
        {
            return ((IAssetClassRepository)_inner).ReadAssetClassById(id);
        }
    }
}
