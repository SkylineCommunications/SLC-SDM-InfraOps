
namespace Skyline.DataMiner.SDM
{
    using Skyline.DataMiner.SDM.Middleware;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM;

    public static class InfraopsReservationDomRepository_Extensions
    {

        public static Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.SDM.AssetManagement.Models.InfraopsReservation> WithMiddleware(
            this Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.SDM.AssetManagement.Models.InfraopsReservation> repository,
            IMiddlewareMarker<Skyline.DataMiner.SDM.AssetManagement.Models.InfraopsReservation> middleware)
        {
            return new InfraopsReservationDomRepository_Middleware(repository, middleware);

        }
    }
}
