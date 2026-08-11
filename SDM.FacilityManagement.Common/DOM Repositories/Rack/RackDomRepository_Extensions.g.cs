
namespace Skyline.DataMiner.SDM
{
    using Skyline.DataMiner.SDM.Middleware;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM;

    public static class RackDomRepository_Extensions
    {

        public static IBulkRepository<FacilityManagement.Models.Rack> WithMiddleware(
            this IBulkRepository<FacilityManagement.Models.Rack> repository,
            IMiddlewareMarker<FacilityManagement.Models.Rack> middleware)
        {
            return new RackDomRepository_Middleware(repository, middleware);
        }
    }
}
