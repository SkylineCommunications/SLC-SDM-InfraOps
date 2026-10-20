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

        [TestMethod]
        public void JobCollections_InPlaceItemMutation_AreReportedAndReverted()
        {
            var job = new PlanAndBuildJob
            {
                JobName = "Original",
                AssetsUsed = new System.Collections.Generic.List<JobAsset>
                {
                    new JobAsset { AssetName = "Server-1" },
                },
                Attachments = new System.Collections.Generic.List<JobAttachment>
                {
                    new JobAttachment { FilePath = "doc-v1.pdf" },
                },
                ConnectionsOnJob = new System.Collections.Generic.List<JobConnection>
                {
                    new JobConnection { Source = "PortA", Destination = "PortB" },
                },
            };
            job.ResetChangeTracking();
            job.IsNewInternal = false;

            job.AssetsUsed[0].AssetName = "Server-2";
            job.Attachments[0].FilePath = "doc-v2.pdf";
            job.ConnectionsOnJob[0].Status = "Connected";

            job.Changed.Should().BeTrue();
            var changes = job.GetChanges();
            changes.Should().Contain(c => c.FieldName == nameof(PlanAndBuildJob.AssetsUsed));
            changes.Should().Contain(c => c.FieldName == nameof(PlanAndBuildJob.Attachments));
            changes.Should().Contain(c => c.FieldName == nameof(PlanAndBuildJob.ConnectionsOnJob));

            Skyline.DataMiner.Utils.InfraOps.Common.Fields.SdmChangeTracking.RevertFieldChanges(job);

            job.Changed.Should().BeFalse();
            job.AssetsUsed[0].AssetName.Should().Be("Server-1");
            job.Attachments[0].FilePath.Should().Be("doc-v1.pdf");
            job.ConnectionsOnJob[0].Status.Should().BeNull();
        }
    }
}
