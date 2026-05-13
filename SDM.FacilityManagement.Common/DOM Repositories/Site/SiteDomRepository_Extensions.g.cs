
namespace Skyline.DataMiner.SDM
{
    using Skyline.DataMiner.SDM.Middleware;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM;

    public static class SiteDomRepository_Extensions
    {

        public static Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.SDM.FacilityManagement.Models.Site> WithMiddleware(
            this Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.SDM.FacilityManagement.Models.Site> repository,
            IMiddlewareMarker<Skyline.DataMiner.SDM.FacilityManagement.Models.Site> middleware)
        {
            return new SiteDomRepository_Middleware(repository, middleware);
        }
    }
}
