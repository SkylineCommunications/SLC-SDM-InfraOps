
namespace Skyline.DataMiner.SDM
{
    using Skyline.DataMiner.SDM.Middleware;
    using Skyline.DataMiner.SDM.InfraOpsProperties.Models;
    using Skyline.DataMiner.SDM;

    public static class PropertyDomRepository_Extensions
    {

        public static IBulkRepository<InfraOpsProperties.Models.Property> WithMiddleware(
            this IBulkRepository<InfraOpsProperties.Models.Property> repository,
            IMiddlewareMarker<InfraOpsProperties.Models.Property> middleware)
        {
            return new PropertyDomRepository_Middleware(repository, middleware);
        }
    }
}
