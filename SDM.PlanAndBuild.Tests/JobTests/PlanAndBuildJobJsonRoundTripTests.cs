namespace SDM.PlanAndBuild.Tests.JobTests
{
	using System;
	using System.Collections.Generic;

	using FluentAssertions;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using SharedMappers.DomIds;

	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.SDM.AssetManagement.Models;
	using Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences;
	using Skyline.DataMiner.SDM.InfraOps.Core.Converters;
	using Skyline.DataMiner.SDM.PlanAndBuild.Models;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

	[TestClass]
	public class PlanAndBuildJobJsonRoundTripTests
	{
		[TestMethod]
		public void PlanAndBuildJob_RoundTrip_PreservesAllFields()
		{
			var location1 = Guid.NewGuid();
			var location2 = Guid.NewGuid();
			var job = new PlanAndBuildJob
			{
				Identifier = Guid.NewGuid().ToString(),
				JobName = "Install Rack 1 Equipment",
				Start = new DateTime(2026, 1, 10, 8, 0, 0, DateTimeKind.Utc),
				End = new DateTime(2026, 1, 15, 17, 30, 0, DateTimeKind.Utc),
				Type = new SdmObjectReference<JobType>(Guid.NewGuid().ToString()),
				JobDescription = "Install new equipment in Rack 1",
				Remarks = "Bring spare parts",
				Priority = SlcPlan_And_Build.Enums.PriorityEnum.High,
				SubState = SlcPlan_And_Build.Enums.SubStateEnum.Scheduled,
				Locations = new List<Guid> { location1, location2 },
			};
			job.Ownership.AssignedTo = new PnoObjectReference<Person>(Guid.NewGuid());
			job.Ownership.AssignmentGroup = new PnoObjectReference<Team>(Guid.NewGuid());
			job.AssetsUsed.Add(new JobAsset
			{
				AssetId = new SdmObjectReference<Asset>(Guid.NewGuid().ToString()),
				Action = SlcPlan_And_Build.Enums.ActionforassetenumEnum.NewlyInstalled,
				AssetName = "Switch 1",
				AssetClassName = "Switch",
				IPAddress = "10.0.0.1",
			});
			job.Attachments.Add(new JobAttachment
			{
				FilePath = @"C:\attachments\plan.pdf",
				AttachedAt = new DateTime(2026, 1, 9, 12, 0, 0, DateTimeKind.Utc),
				AttachedBy = "operator",
			});
			job.ConnectionsOnJob.Add(new JobConnection
			{
				ConnectionId = new SdmObjectReference<Connection>(Guid.NewGuid().ToString()),
				Source = "Rack1/Port1",
				Destination = "Rack2/Port7",
				Status = "Planned",
				CableType = new SdmObjectReference<CableType>(Guid.NewGuid().ToString()),
				CableLength = 12.5,
			});

			string json = InfraOpsJsonConvert.SerializeObject(job);
			var restored = InfraOpsJsonConvert.DeserializeObject<PlanAndBuildJob>(json);

			restored.Identifier.Should().Be(job.Identifier);
			restored.JobName.Should().Be(job.JobName);
			restored.Start.Should().Be(job.Start);
			restored.End.Should().Be(job.End);
			restored.Type.Should().Be(job.Type);
			restored.JobDescription.Should().Be(job.JobDescription);
			restored.Remarks.Should().Be(job.Remarks);
			restored.Priority.Should().Be(job.Priority);
			restored.SubState.Should().Be(job.SubState);
			restored.Locations.Should().Equal(location1, location2);
			restored.Ownership.AssignedTo.Should().Be(job.Ownership.AssignedTo);
			restored.Ownership.AssignmentGroup.Should().Be(job.Ownership.AssignmentGroup);

			restored.AssetsUsed.Should().ContainSingle();
			restored.AssetsUsed[0].AssetId.Should().Be(job.AssetsUsed[0].AssetId);
			restored.AssetsUsed[0].Action.Should().Be(job.AssetsUsed[0].Action);
			restored.AssetsUsed[0].AssetName.Should().Be("Switch 1");
			restored.AssetsUsed[0].AssetClassName.Should().Be("Switch");
			restored.AssetsUsed[0].IPAddress.Should().Be("10.0.0.1");

			restored.Attachments.Should().ContainSingle();
			restored.Attachments[0].FilePath.Should().Be(job.Attachments[0].FilePath);
			restored.Attachments[0].AttachedAt.Should().Be(job.Attachments[0].AttachedAt);
			restored.Attachments[0].AttachedBy.Should().Be("operator");

			restored.ConnectionsOnJob.Should().ContainSingle();
			restored.ConnectionsOnJob[0].ConnectionId.Should().Be(job.ConnectionsOnJob[0].ConnectionId);
			restored.ConnectionsOnJob[0].Source.Should().Be("Rack1/Port1");
			restored.ConnectionsOnJob[0].Destination.Should().Be("Rack2/Port7");
			restored.ConnectionsOnJob[0].Status.Should().Be("Planned");
			restored.ConnectionsOnJob[0].CableType.Should().Be(job.ConnectionsOnJob[0].CableType);
			restored.ConnectionsOnJob[0].CableLength.Should().Be(12.5);

			restored.Should().Be(job);
		}

		[TestMethod]
		public void PlanAndBuildJob_Serialize_WritesReferencesAsStringsAndEnumsAsNames()
		{
			var assignedTo = Guid.NewGuid();
			var typeId = Guid.NewGuid().ToString();
			var assetId = Guid.NewGuid().ToString();
			var job = new PlanAndBuildJob
			{
				Type = new SdmObjectReference<JobType>(typeId),
				Priority = SlcPlan_And_Build.Enums.PriorityEnum.High,
				SubState = SlcPlan_And_Build.Enums.SubStateEnum.Scheduled,
			};
			job.Ownership.AssignedTo = new PnoObjectReference<Person>(assignedTo);
			job.AssetsUsed.Add(new JobAsset
			{
				AssetId = new SdmObjectReference<Asset>(assetId),
				Action = SlcPlan_And_Build.Enums.ActionforassetenumEnum.NewlyInstalled,
			});

			string json = InfraOpsJsonConvert.SerializeObject(job);

			json.Should().Contain($"\"Type\":\"{typeId}\"");
			json.Should().Contain($"\"AssignedTo\":\"{assignedTo}\"");
			json.Should().Contain($"\"AssetId\":\"{assetId}\"");
			json.Should().Contain("\"Priority\":\"High\"");
			json.Should().Contain("\"SubState\":\"Scheduled\"");
			json.Should().Contain("\"Action\":\"NewlyInstalled\"");
		}

		[TestMethod]
		public void PlanAndBuildJob_Serialize_EmptyReferencesAreNull()
		{
			var job = new PlanAndBuildJob();
			job.AssetsUsed.Add(new JobAsset());
			job.ConnectionsOnJob.Add(new JobConnection());

			string json = InfraOpsJsonConvert.SerializeObject(job);

			json.Should().Contain("\"Type\":null");
			json.Should().Contain("\"AssignedTo\":null");
			json.Should().Contain("\"AssignmentGroup\":null");
			json.Should().Contain("\"AssetId\":null");
			json.Should().Contain("\"ConnectionId\":null");
			json.Should().Contain("\"CableType\":null");
		}

		[TestMethod]
		public void PlanAndBuildJob_Deserialize_NullAndEmptyReferencesBecomeDefault()
		{
			var restored = InfraOpsJsonConvert.DeserializeObject<PlanAndBuildJob>(
				"{\"Type\":\"\",\"Ownership\":{\"AssignedTo\":null,\"AssignmentGroup\":\"\"}," +
				"\"AssetsUsed\":[{\"AssetId\":null}],\"ConnectionsOnJob\":[{\"ConnectionId\":\"\",\"CableType\":null}]}");

			restored.Type.Identifier.Should().BeNullOrEmpty();
			restored.Ownership.AssignedTo.Identifier.Should().Be(Guid.Empty);
			restored.Ownership.AssignmentGroup.Identifier.Should().Be(Guid.Empty);
			restored.AssetsUsed.Should().ContainSingle().Which.AssetId.Identifier.Should().BeNullOrEmpty();
			restored.ConnectionsOnJob.Should().ContainSingle();
			restored.ConnectionsOnJob[0].ConnectionId.Identifier.Should().BeNullOrEmpty();
			restored.ConnectionsOnJob[0].CableType.Identifier.Should().BeNullOrEmpty();
		}
	}
}
