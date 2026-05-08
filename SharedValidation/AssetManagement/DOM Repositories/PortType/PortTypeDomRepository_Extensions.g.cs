
namespace Skyline.DataMiner.SDM
{
    using Skyline.DataMiner.SDM.Middleware;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM;

    public static class PortTypeDomRepository_Extensions
    {

        public static Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.SDM.AssetManagement.Models.PortType> WithMiddleware(
            this Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.SDM.AssetManagement.Models.PortType> repository,
            IMiddlewareMarker<Skyline.DataMiner.SDM.AssetManagement.Models.PortType> middleware)
        {
            return new PortTypeDomRepository_Middleware(repository, middleware);
        }
    }
}
