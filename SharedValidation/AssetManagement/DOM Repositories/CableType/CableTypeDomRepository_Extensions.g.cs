
namespace Skyline.DataMiner.SDM
{
    using Skyline.DataMiner.SDM.Middleware;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM;

    public static class CableTypeDomRepository_Extensions
    {

        public static Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.SDM.AssetManagement.Models.CableType> WithMiddleware(
            this Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.SDM.AssetManagement.Models.CableType> repository,
            IMiddlewareMarker<Skyline.DataMiner.SDM.AssetManagement.Models.CableType> middleware)
        {
            return new CableTypeDomRepository_Middleware(repository, middleware);
        }
    }
}
