
namespace Skyline.DataMiner.SDM
{
    using Skyline.DataMiner.SDM.Middleware;
    using Skyline.DataMiner.SDM.PlanAndBuild.Models;
    using Skyline.DataMiner.SDM;

    public static class JobTypeDomRepository_Extensions
    {

        public static IBulkRepository<PlanAndBuild.Models.JobType> WithMiddleware(
            this IBulkRepository<PlanAndBuild.Models.JobType> repository,
            IMiddlewareMarker<PlanAndBuild.Models.JobType> middleware)
        {
            return new JobTypeDomRepository_Middleware(repository, middleware);
        }
    }
}
