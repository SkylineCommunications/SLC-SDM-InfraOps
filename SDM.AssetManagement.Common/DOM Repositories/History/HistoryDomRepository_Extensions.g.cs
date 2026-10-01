
namespace Skyline.DataMiner.SDM
{
    using Skyline.DataMiner.SDM.Middleware;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM;

    public static class HistoryDomRepository_Extensions
    {

        public static IBulkRepository<AssetManagement.Models.History> WithMiddleware(
            this IBulkRepository<AssetManagement.Models.History> repository,
            IMiddlewareMarker<AssetManagement.Models.History> middleware)
        {
            return new HistoryDomRepository_Middleware(repository, middleware);
        }
    }
}