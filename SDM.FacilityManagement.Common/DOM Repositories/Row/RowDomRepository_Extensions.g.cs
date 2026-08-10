
namespace Skyline.DataMiner.SDM
{
    using Skyline.DataMiner.SDM.Middleware;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM;

    public static class RowDomRepository_Extensions
    {

        public static IBulkRepository<FacilityManagement.Models.Row> WithMiddleware(
            this IBulkRepository<FacilityManagement.Models.Row> repository,
            IMiddlewareMarker<FacilityManagement.Models.Row> middleware)
        {
            return new RowDomRepository_Middleware(repository, middleware);
        }
    }
}
