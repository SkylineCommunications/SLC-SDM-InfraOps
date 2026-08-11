
namespace Skyline.DataMiner.SDM
{
    using Skyline.DataMiner.SDM.Middleware;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM;

    public static class RoomDomRepository_Extensions
    {

        public static IBulkRepository<FacilityManagement.Models.Room> WithMiddleware(
            this IBulkRepository<FacilityManagement.Models.Room> repository,
            IMiddlewareMarker<FacilityManagement.Models.Room> middleware)
        {
            return new RoomDomRepository_Middleware(repository, middleware);
        }
    }
}
