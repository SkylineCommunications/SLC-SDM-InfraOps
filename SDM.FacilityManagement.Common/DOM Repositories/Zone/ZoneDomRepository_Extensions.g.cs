
namespace Skyline.DataMiner.SDM
{
    using Skyline.DataMiner.SDM.Middleware;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM;

    public static class ZoneDomRepository_Extensions
    {

        public static Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.SDM.FacilityManagement.Models.Zone> WithMiddleware(
            this Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.SDM.FacilityManagement.Models.Zone> repository,
            IMiddlewareMarker<Skyline.DataMiner.SDM.FacilityManagement.Models.Zone> middleware)
        {
            return new ZoneDomRepository_Middleware(repository, middleware);
        }
    }
}
