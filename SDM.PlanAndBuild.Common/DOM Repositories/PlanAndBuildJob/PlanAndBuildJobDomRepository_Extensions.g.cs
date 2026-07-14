
namespace Skyline.DataMiner.SDM
{
    using Skyline.DataMiner.SDM.Middleware;
    using Skyline.DataMiner.SDM.PlanAndBuild.Models;
    using Skyline.DataMiner.SDM;

    public static class PlanAndBuildJobDomRepository_Extensions
    {

        public static Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.SDM.PlanAndBuild.Models.PlanAndBuildJob> WithMiddleware(
            this Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.SDM.PlanAndBuild.Models.PlanAndBuildJob> repository,
            IMiddlewareMarker<Skyline.DataMiner.SDM.PlanAndBuild.Models.PlanAndBuildJob> middleware)
        {
            return new PlanAndBuildJobDomRepository_Middleware(repository, middleware);
        }
    }
}
