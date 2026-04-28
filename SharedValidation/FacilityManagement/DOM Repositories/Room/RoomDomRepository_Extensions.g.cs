
namespace Skyline.DataMiner.SDM
{
    using Skyline.DataMiner.SDM.Middleware;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM;

    public static class RoomDomRepository_Extensions
    {

        public static Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.SDM.FacilityManagement.Models.Room> WithMiddleware(
            this Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.SDM.FacilityManagement.Models.Room> repository,
            IMiddlewareMarker<Skyline.DataMiner.SDM.FacilityManagement.Models.Room> middleware)
        {
            return new RoomDomRepository_Middleware(repository, middleware);
        }
    }
}
