
namespace Skyline.DataMiner.SDM
{
    using Skyline.DataMiner.SDM.Middleware;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM;

    public static class PortTypeDomRepository_Extensions
    {

        public static IBulkRepository<AssetManagement.Models.PortType> WithMiddleware(
            this IBulkRepository<AssetManagement.Models.PortType> repository,
            IMiddlewareMarker<AssetManagement.Models.PortType> middleware)
        {
            return new PortTypeDomRepository_Middleware(repository, middleware);
        }
    }
}
