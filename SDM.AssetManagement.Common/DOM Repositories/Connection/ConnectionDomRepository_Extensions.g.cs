
namespace Skyline.DataMiner.SDM
{
    using Skyline.DataMiner.SDM.Middleware;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM;

    public static class ConnectionDomRepository_Extensions
    {

        public static Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.SDM.AssetManagement.Models.Connection> WithMiddleware(
            this Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.SDM.AssetManagement.Models.Connection> repository,
            IMiddlewareMarker<Skyline.DataMiner.SDM.AssetManagement.Models.Connection> middleware)
        {
            return new ConnectionDomRepository_Middleware(repository, middleware);
        }
    }
}
