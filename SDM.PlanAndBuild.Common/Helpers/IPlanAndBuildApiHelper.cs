namespace Skyline.DataMiner.SDM.PlanAndBuild.Helpers
{
    using Skyline.DataMiner.Net;
    using Skyline.DataMiner.SDM.PlanAndBuild.Models;
    using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

    public interface IPlanAndBuildApiHelper
    {
        IConnection Connection { get; }

        IPlanAndBuildJobRepository Jobs { get; }

        IJobTypeRepository JobTypes { get; }

        IBulkRepository<PlanAndBuildAppSettings> AppSettings { get; }
    }
}
