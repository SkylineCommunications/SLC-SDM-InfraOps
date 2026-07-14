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

        /// <summary>
        /// Gets the People &amp; Organizations API, used to validate the existence of the Person/Team referenced
        /// by <see cref="Models.JobOwnership.AssignedTo"/>, <see cref="Models.JobOwnership.AssignmentGroup"/> and
        /// <see cref="Models.JobAttachment.AttachedBy"/>.
        /// </summary>
        IPeopleAndOrganizationsApi People { get; }
    }
}
