
namespace Skyline.DataMiner.SDM
{
    using Skyline.DataMiner.SDM.Middleware;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM;

    public static class ZoneDomRepository_Extensions
    {

        public static IBulkRepository<FacilityManagement.Models.Zone> WithMiddleware(
            this IBulkRepository<FacilityManagement.Models.Zone> repository,
            IMiddlewareMarker<FacilityManagement.Models.Zone> middleware)
        {
            return new ZoneDomRepository_Middleware(repository, middleware);
        }
    }
}
