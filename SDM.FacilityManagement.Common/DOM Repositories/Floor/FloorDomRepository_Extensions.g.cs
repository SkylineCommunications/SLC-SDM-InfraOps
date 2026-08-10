
namespace Skyline.DataMiner.SDM
{
    using Skyline.DataMiner.SDM.Middleware;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM;

    public static class FloorDomRepository_Extensions
    {

        public static IBulkRepository<FacilityManagement.Models.Floor> WithMiddleware(
            this IBulkRepository<FacilityManagement.Models.Floor> repository,
            IMiddlewareMarker<FacilityManagement.Models.Floor> middleware)
        {
            return new FloorDomRepository_Middleware(repository, middleware);
        }
    }
}
