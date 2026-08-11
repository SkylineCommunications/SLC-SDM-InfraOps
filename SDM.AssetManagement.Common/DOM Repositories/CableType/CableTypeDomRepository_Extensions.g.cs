
namespace Skyline.DataMiner.SDM
{
    using Skyline.DataMiner.SDM.Middleware;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM;

    public static class CableTypeDomRepository_Extensions
    {

        public static IBulkRepository<AssetManagement.Models.CableType> WithMiddleware(
            this IBulkRepository<AssetManagement.Models.CableType> repository,
            IMiddlewareMarker<AssetManagement.Models.CableType> middleware)
        {
            return new CableTypeDomRepository_Middleware(repository, middleware);
        }
    }
}
