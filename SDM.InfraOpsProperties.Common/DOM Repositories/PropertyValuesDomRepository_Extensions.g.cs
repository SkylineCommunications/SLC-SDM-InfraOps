
namespace Skyline.DataMiner.SDM
{
    using Skyline.DataMiner.SDM.Middleware;
    using Skyline.DataMiner.SDM.InfraOpsProperties.Models;
    using Skyline.DataMiner.SDM;

    public static class PropertyValuesDomRepository_Extensions
    {

        public static Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.SDM.InfraOpsProperties.Models.PropertyValues> WithMiddleware(
            this Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.SDM.InfraOpsProperties.Models.PropertyValues> repository,
            IMiddlewareMarker<Skyline.DataMiner.SDM.InfraOpsProperties.Models.PropertyValues> middleware)
        {
            return new PropertyValuesDomRepository_Middleware(repository, middleware);
        }
    }
}
