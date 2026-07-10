namespace SDM.PlanAndBuild.Tests.JobTests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using FluentAssertions;
	using FluentAssertions.Execution;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using SDM.PlanAndBuild.Tests.Setup;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.SDM.AssetManagement.Models;
	using Skyline.DataMiner.SDM.PlanAndBuild.Models;

	using SharedMappers.DomIds;

	/// <summary>
	/// Round-trip tests for the PlanAndBuildJob DOM mapper (ToInstance/FromInstance), exercising every mapped
	/// field including the Ownership section and the AssetsUsed/Attachments/ConnectionsOnJob collections.
	/// </summary>
	[TestClass]
	public class PlanAndBuildJobMapperRoundTripTests : BaseRepositoryTest
	{
		[TestMethod]
		public void RoundTrip_JobWithAllFieldsPopulated_ShouldPreserveEveryField()
		{
			Helper.PopulateAppSettings();
			var jobType = Helper.JobTypes.Create(new JobType { Name = "Installation" });

			var original = new PlanAndBuildJob
			{
				Identifier = Guid.NewGuid().ToString(),
				JobID = "JOB-0001",
				JobName = "Install Rack 1 Equipment",
				Start = new DateTime(2026, 1, 10),
				End = new DateTime(2026, 1, 15),
				Type = new SdmObjectReference<JobType>(jobType.Identifier),
#pragma warning disable CS0618 // Soft deleted field, exercised here for round-trip coverage only.
				JobType = SlcPlan_And_Build.Enums.JobtypeEnum.Add,
#pragma warning restore CS0618
				JobDescription = "Install new equipment in Rack 1",
				Remarks = "Bring spare parts",
				Priority = SlcPlan_And_Build.Enums.PriorityEnum.High,
				SubState = SlcPlan_And_Build.Enums.SubStateEnum.Scheduled,
				LocationGuids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() },
			};
			original.Ownership.AssignedTo = Guid.NewGuid();
			original.Ownership.AssignmentGroup = Guid.NewGuid();
			original.AssetsUsed = new List<JobAsset>
			{
				new JobAsset { AssetId = new SdmObjectReference<Asset>(Guid.NewGuid().ToString()), Action = SlcPlan_And_Build.Enums.ActionforassetenumEnum.NewlyInstalled },
			};
			original.Attachments = new List<JobAttachment>
			{
				new JobAttachment { FilePath = @"C:\attachments\plan.pdf", AttachedAt = new DateTime(2026, 1, 9), AttachedBy = Guid.NewGuid() },
			};
			original.ConnectionsOnJob = new List<JobConnection>
			{
				new JobConnection
				{
					ConnectionId = new SdmObjectReference<Connection>(Guid.NewGuid().ToString()),
					Source = "Patch Panel A - Port 1",
					Destination = "Switch B - Port 12",
					Status = "Installed",
					CableType = new SdmObjectReference<CableType>(Guid.NewGuid().ToString()),
					CableLength = 12.5,
				},
			};

			Helper.Jobs.Create(original);

			var roundTripped = Helper.Jobs.Read(PlanAndBuildJobExposers.Identifier.Equal(original.Identifier)).Single();

			using (new AssertionScope())
			{
				roundTripped.Identifier.Should().Be(original.Identifier);
				roundTripped.JobID.Should().Be(original.JobID);
				roundTripped.JobName.Should().Be(original.JobName);
				roundTripped.Start.Should().Be(original.Start);
				roundTripped.End.Should().Be(original.End);
				roundTripped.Type.Should().Be(original.Type);
#pragma warning disable CS0618 // Soft deleted field, exercised here for round-trip coverage only.
				roundTripped.JobType.Should().Be(original.JobType);
#pragma warning restore CS0618
				roundTripped.JobDescription.Should().Be(original.JobDescription);
				roundTripped.Remarks.Should().Be(original.Remarks);
				roundTripped.Priority.Should().Be(original.Priority);
				roundTripped.SubState.Should().Be(original.SubState);
				roundTripped.LocationGuids.Should().BeEquivalentTo(original.LocationGuids);
				roundTripped.Ownership.AssignedTo.Should().Be(original.Ownership.AssignedTo);
				roundTripped.Ownership.AssignmentGroup.Should().Be(original.Ownership.AssignmentGroup);
				roundTripped.AssetsUsed.Should().BeEquivalentTo(original.AssetsUsed);
				roundTripped.Attachments.Should().BeEquivalentTo(original.Attachments);
				roundTripped.ConnectionsOnJob.Should().BeEquivalentTo(original.ConnectionsOnJob);
			}
		}

		[TestMethod]
		public void RoundTrip_JobWithNoEndDate_ShouldPreserveNullEnd()
		{
			Helper.PopulateAppSettings();
			var jobType = Helper.JobTypes.Create(new JobType { Name = "Decommissioning" });

			var original = new PlanAndBuildJob
			{
				Identifier = Guid.NewGuid().ToString(),
				JobName = "Decommission Legacy Server",
				Type = new SdmObjectReference<JobType>(jobType.Identifier),
				Start = new DateTime(2026, 3, 5),
				End = null,
			};

			Helper.Jobs.Create(original);

			var roundTripped = Helper.Jobs.Read(PlanAndBuildJobExposers.Identifier.Equal(original.Identifier)).Single();

			roundTripped.End.Should().BeNull();
		}

		[TestMethod]
		public void RoundTrip_AfterFetch_ShouldNotBeNewAndShouldHaveNoPendingChanges()
		{
			Helper.PopulateAppSettings();
			var jobType = Helper.JobTypes.Create(new JobType { Name = "Maintenance" });

			var original = new PlanAndBuildJob
			{
				Identifier = Guid.NewGuid().ToString(),
				JobName = "Quarterly Maintenance Check",
				Type = new SdmObjectReference<JobType>(jobType.Identifier),
			};

			Helper.Jobs.Create(original);

			var roundTripped = Helper.Jobs.Read(PlanAndBuildJobExposers.Identifier.Equal(original.Identifier)).Single();

			using (new AssertionScope())
			{
				roundTripped.IsNew.Should().BeFalse();
				roundTripped.Changed.Should().BeFalse();
			}
		}
	}
}
