
namespace Skyline.DataMiner.SDM
{
    using Skyline.DataMiner.SDM.Middleware;
    using Skyline.DataMiner.SDM.PlanAndBuild.Models;
    using Skyline.DataMiner.SDM;

    public static class PlanAndBuildAppSettingsDomRepository_Extensions
    {

        public static Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.SDM.PlanAndBuild.Models.PlanAndBuildAppSettings> WithMiddleware(
            this Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.SDM.PlanAndBuild.Models.PlanAndBuildAppSettings> repository,
            IMiddlewareMarker<Skyline.DataMiner.SDM.PlanAndBuild.Models.PlanAndBuildAppSettings> middleware)
        {
            return new PlanAndBuildAppSettingsDomRepository_Middleware(repository, middleware);
        }
    }
}
