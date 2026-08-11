
namespace Skyline.DataMiner.SDM
{
    using Skyline.DataMiner.SDM.Middleware;
    using Skyline.DataMiner.SDM.InfraOpsProperties.Models;
    using Skyline.DataMiner.SDM;

    public static class PropertyValuesDomRepository_Extensions
    {

        public static IBulkRepository<InfraOpsProperties.Models.PropertyValues> WithMiddleware(
            this IBulkRepository<InfraOpsProperties.Models.PropertyValues> repository,
            IMiddlewareMarker<InfraOpsProperties.Models.PropertyValues> middleware)
        {
            return new PropertyValuesDomRepository_Middleware(repository, middleware);
        }
    }
}
