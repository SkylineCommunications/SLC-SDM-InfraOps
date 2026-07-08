namespace Skyline.DataMiner.SDM.PlanAndBuild.Helpers
{
    using Skyline.DataMiner.Net;
    using Skyline.DataMiner.SDM.PlanAndBuild.Models;

    public interface IPlanAndBuildApiHelper
    {
        IConnection Connection { get; }

        IPlanAndBuildJobRepository Jobs { get; }

        IBulkRepository<JobType> JobTypes { get; }

        IBulkRepository<PlanAndBuildAppSettings> AppSettings { get; }
    }
}
