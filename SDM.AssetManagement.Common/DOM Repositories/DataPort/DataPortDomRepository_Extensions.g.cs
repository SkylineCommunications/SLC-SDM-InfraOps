
namespace Skyline.DataMiner.SDM
{
    using Skyline.DataMiner.SDM.Middleware;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM;

    public static class DataPortDomRepository_Extensions
    {

        public static IBulkRepository<AssetManagement.Models.DataPort> WithMiddleware(
            this IBulkRepository<AssetManagement.Models.DataPort> repository,
            IMiddlewareMarker<AssetManagement.Models.DataPort> middleware)
        {
            return new DataPortDomRepository_Middleware(repository, middleware);
        }
    }
}
