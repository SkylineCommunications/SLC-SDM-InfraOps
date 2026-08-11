
namespace Skyline.DataMiner.SDM
{
    using Skyline.DataMiner.SDM.Middleware;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM;

    public static class InfraopsReservationDomRepository_Extensions
    {

        public static IBulkRepository<AssetManagement.Models.InfraopsReservation> WithMiddleware(
            this IBulkRepository<AssetManagement.Models.InfraopsReservation> repository,
            IMiddlewareMarker<AssetManagement.Models.InfraopsReservation> middleware)
        {
            return new InfraopsReservationDomRepository_Middleware(repository, middleware);

        }
    }
}
