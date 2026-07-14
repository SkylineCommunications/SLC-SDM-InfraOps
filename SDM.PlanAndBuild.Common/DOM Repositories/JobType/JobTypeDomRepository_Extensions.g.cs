
namespace Skyline.DataMiner.SDM
{
    using Skyline.DataMiner.SDM.Middleware;
    using Skyline.DataMiner.SDM.PlanAndBuild.Models;
    using Skyline.DataMiner.SDM;

    public static class JobTypeDomRepository_Extensions
    {

        public static Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.SDM.PlanAndBuild.Models.JobType> WithMiddleware(
            this Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.SDM.PlanAndBuild.Models.JobType> repository,
            IMiddlewareMarker<Skyline.DataMiner.SDM.PlanAndBuild.Models.JobType> middleware)
        {
            return new JobTypeDomRepository_Middleware(repository, middleware);
        }
    }
}
