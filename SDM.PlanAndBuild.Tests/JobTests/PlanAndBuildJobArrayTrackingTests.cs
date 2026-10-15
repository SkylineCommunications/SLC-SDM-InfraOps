namespace SDM.PlanAndBuild.Tests.JobTests
{
    using FluentAssertions;

    using Skyline.DataMiner.SDM.PlanAndBuild.Models;

    [TestClass]
    public class PlanAndBuildJobArrayTrackingTests
    {
        [TestMethod]
        public void ArrayFieldsUntouched_ReportsNoChanges()
        {
            var job = new PlanAndBuildJob { JobName = "Original" };
            _ = job.Locations;
            _ = job.AssetsUsed;
            _ = job.Attachments;
            _ = job.ConnectionsOnJob;
            job.ResetChangeTracking();

            job.GetChanges().Should().BeEmpty();
        }

        [TestMethod]
        public void Locations_GuidAdded_IsReported()
        {
            var job = new PlanAndBuildJob { JobName = "Original" };
            job.ResetChangeTracking();
            job.Locations.Add(System.Guid.NewGuid());

            job.GetChanges().Should().ContainSingle(change => change.FieldName == nameof(PlanAndBuildJob.Locations));
        }
    }
}
