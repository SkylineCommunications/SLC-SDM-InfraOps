
namespace Skyline.DataMiner.SDM
{
    using Skyline.DataMiner.SDM.Middleware;
    using Skyline.DataMiner.SDM.InfraOpsProperties.Models;
    using Skyline.DataMiner.SDM;

    public static class PropertyDomRepository_Extensions
    {

        public static Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.SDM.InfraOpsProperties.Models.Property> WithMiddleware(
            this Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.SDM.InfraOpsProperties.Models.Property> repository,
            IMiddlewareMarker<Skyline.DataMiner.SDM.InfraOpsProperties.Models.Property> middleware)
        {
            return new PropertyDomRepository_Middleware(repository, middleware);
        }
    }
}
