
namespace Skyline.DataMiner.SDM
{
    using Skyline.DataMiner.SDM.Middleware;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM;

    public static class FacilityDomRepository_Extensions
    {

        public static IBulkRepository<FacilityManagement.Models.Facility> WithMiddleware(
            this IBulkRepository<FacilityManagement.Models.Facility> repository,
            IMiddlewareMarker<FacilityManagement.Models.Facility> middleware)
        {
            return new FacilityDomRepository_Middleware(repository, middleware);
        }
    }
}
