
namespace Skyline.DataMiner.SDM
{
    using Skyline.DataMiner.SDM.Middleware;
    using Skyline.DataMiner.SDM.PlanAndBuild.Models;
    using Skyline.DataMiner.SDM;

    public static class PlanAndBuildJobDomRepository_Extensions
    {

        public static IBulkRepository<PlanAndBuild.Models.PlanAndBuildJob> WithMiddleware(
            this IBulkRepository<PlanAndBuild.Models.PlanAndBuildJob> repository,
            IMiddlewareMarker<PlanAndBuild.Models.PlanAndBuildJob> middleware)
        {
            return new PlanAndBuildJobDomRepository_Middleware(repository, middleware);
        }
    }
}
