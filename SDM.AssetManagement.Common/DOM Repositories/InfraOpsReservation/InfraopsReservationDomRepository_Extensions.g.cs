
namespace Skyline.DataMiner.SDM
{
    using Skyline.DataMiner.SDM.Middleware;
    using SharedCommonLibrary.AssetManagement.Models;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;

    public static class InfraopsReservationDomRepository_Extensions
    {

        public static Skyline.DataMiner.SDM.IBulkRepository<SharedCommonLibrary.AssetManagement.Models.InfraopsReservation> WithMiddleware(
            this Skyline.DataMiner.SDM.IBulkRepository<SharedCommonLibrary.AssetManagement.Models.InfraopsReservation> repository,
            IMiddlewareMarker<SharedCommonLibrary.AssetManagement.Models.InfraopsReservation> middleware)
        {
            return new InfraopsReservationDomRepository_Middleware(repository, middleware);

        }
    }
}
