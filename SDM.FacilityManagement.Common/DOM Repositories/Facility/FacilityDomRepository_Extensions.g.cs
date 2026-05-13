
namespace Skyline.DataMiner.SDM
{
    using Skyline.DataMiner.SDM.Middleware;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM;

    public static class FacilityDomRepository_Extensions
    {

        public static Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.SDM.FacilityManagement.Models.Facility> WithMiddleware(
            this Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.SDM.FacilityManagement.Models.Facility> repository,
            IMiddlewareMarker<Skyline.DataMiner.SDM.FacilityManagement.Models.Facility> middleware)
        {
            return new FacilityDomRepository_Middleware(repository, middleware);
        }
    }
}
