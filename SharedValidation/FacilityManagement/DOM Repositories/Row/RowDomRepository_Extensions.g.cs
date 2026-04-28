
namespace Skyline.DataMiner.SDM
{
    using Skyline.DataMiner.SDM.Middleware;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM;

    public static class RowDomRepository_Extensions
    {

        public static Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.SDM.FacilityManagement.Models.Row> WithMiddleware(
            this Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.SDM.FacilityManagement.Models.Row> repository,
            IMiddlewareMarker<Skyline.DataMiner.SDM.FacilityManagement.Models.Row> middleware)
        {
            return new RowDomRepository_Middleware(repository, middleware);
        }
    }
}
