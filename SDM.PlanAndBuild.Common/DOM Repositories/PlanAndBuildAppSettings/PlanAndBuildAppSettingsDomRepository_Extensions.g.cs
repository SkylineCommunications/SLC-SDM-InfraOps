
namespace Skyline.DataMiner.SDM
{
    using Skyline.DataMiner.SDM.Middleware;
    using Skyline.DataMiner.SDM.PlanAndBuild.Models;
    using Skyline.DataMiner.SDM;

    public static class PlanAndBuildAppSettingsDomRepository_Extensions
    {

        public static IBulkRepository<PlanAndBuild.Models.PlanAndBuildAppSettings> WithMiddleware(
            this IBulkRepository<PlanAndBuild.Models.PlanAndBuildAppSettings> repository,
            IMiddlewareMarker<PlanAndBuild.Models.PlanAndBuildAppSettings> middleware)
        {
            return new PlanAndBuildAppSettingsDomRepository_Middleware(repository, middleware);
        }
    }
}
