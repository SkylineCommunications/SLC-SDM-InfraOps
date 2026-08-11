
namespace Skyline.DataMiner.SDM
{
    using Skyline.DataMiner.SDM.Middleware;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM;

    public static class ConnectionDomRepository_Extensions
    {

        public static IBulkRepository<AssetManagement.Models.Connection> WithMiddleware(
            this IBulkRepository<AssetManagement.Models.Connection> repository,
            IMiddlewareMarker<AssetManagement.Models.Connection> middleware)
        {
            return new ConnectionDomRepository_Middleware(repository, middleware);
        }
    }
}
