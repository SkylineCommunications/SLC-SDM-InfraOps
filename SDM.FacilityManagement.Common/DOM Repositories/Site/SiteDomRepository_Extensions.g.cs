
namespace Skyline.DataMiner.SDM
{
    using Skyline.DataMiner.SDM.Middleware;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM;

    public static class SiteDomRepository_Extensions
    {

        public static IBulkRepository<FacilityManagement.Models.Site> WithMiddleware(
            this IBulkRepository<FacilityManagement.Models.Site> repository,
            IMiddlewareMarker<FacilityManagement.Models.Site> middleware)
        {
            return new SiteDomRepository_Middleware(repository, middleware);
        }
    }
}
