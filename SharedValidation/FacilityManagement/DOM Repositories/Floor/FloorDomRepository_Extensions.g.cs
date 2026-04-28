
namespace Skyline.DataMiner.SDM
{
    using Skyline.DataMiner.SDM.Middleware;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM;

    public static class FloorDomRepository_Extensions
    {

        public static Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.SDM.FacilityManagement.Models.Floor> WithMiddleware(
            this Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.SDM.FacilityManagement.Models.Floor> repository,
            IMiddlewareMarker<Skyline.DataMiner.SDM.FacilityManagement.Models.Floor> middleware)
        {
            return new FloorDomRepository_Middleware(repository, middleware);
        }
    }
}
