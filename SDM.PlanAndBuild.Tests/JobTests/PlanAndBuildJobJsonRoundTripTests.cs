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
				Action = SlcPlan_And_Build.Enums.ActionforassetenumEnum.Reinstalled,
				AssetName = "Switch 1",
				AssetClassName = "Switch",
				IPAddress = "10.0.0.1",
			});
			job.AssetsUsed.Add(new JobAsset
			{
				AssetId = new SdmObjectReference<Asset>(Guid.NewGuid().ToString()),
				Action = SlcPlan_And_Build.Enums.ActionforassetenumEnum.Provisioned,
				AssetName = "Router 2",
				AssetClassName = "Router",
				IPAddress = "10.0.0.2",
			});
			job.Attachments.Add(new JobAttachment
			{
				FilePath = @"C:\attachments\plan.pdf",
				AttachedAt = new DateTime(2026, 1, 9, 12, 0, 0, DateTimeKind.Utc),
				AttachedBy = "operator",
			});
			job.Attachments.Add(new JobAttachment
			{
				FilePath = @"C:\attachments\layout.png",
				AttachedAt = new DateTime(2026, 1, 11, 9, 15, 0, DateTimeKind.Utc),
				AttachedBy = "supervisor",
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
			job.ConnectionsOnJob.Add(new JobConnection
			{
				ConnectionId = new SdmObjectReference<Connection>(Guid.NewGuid().ToString()),
				Source = "Rack3/Port2",
				Destination = "Rack4/Port9",
				Status = "Installed",
				CableType = new SdmObjectReference<CableType>(Guid.NewGuid().ToString()),
				CableLength = 3.75,
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

			restored.AssetsUsed.Should().HaveCount(2);
			for (int i = 0; i < 2; i++)
			{
				restored.AssetsUsed[i].AssetId.Should().Be(job.AssetsUsed[i].AssetId);
				restored.AssetsUsed[i].Action.Should().Be(job.AssetsUsed[i].Action);
				restored.AssetsUsed[i].AssetName.Should().Be(job.AssetsUsed[i].AssetName);
				restored.AssetsUsed[i].AssetClassName.Should().Be(job.AssetsUsed[i].AssetClassName);
				restored.AssetsUsed[i].IPAddress.Should().Be(job.AssetsUsed[i].IPAddress);
			}

			restored.Attachments.Should().HaveCount(2);
			for (int i = 0; i < 2; i++)
			{
				restored.Attachments[i].FilePath.Should().Be(job.Attachments[i].FilePath);
				restored.Attachments[i].AttachedAt.Should().Be(job.Attachments[i].AttachedAt);
				restored.Attachments[i].AttachedBy.Should().Be(job.Attachments[i].AttachedBy);
			}

			restored.ConnectionsOnJob.Should().HaveCount(2);
			for (int i = 0; i < 2; i++)
			{
				restored.ConnectionsOnJob[i].ConnectionId.Should().Be(job.ConnectionsOnJob[i].ConnectionId);
				restored.ConnectionsOnJob[i].Source.Should().Be(job.ConnectionsOnJob[i].Source);
				restored.ConnectionsOnJob[i].Destination.Should().Be(job.ConnectionsOnJob[i].Destination);
				restored.ConnectionsOnJob[i].Status.Should().Be(job.ConnectionsOnJob[i].Status);
				restored.ConnectionsOnJob[i].CableType.Should().Be(job.ConnectionsOnJob[i].CableType);
				restored.ConnectionsOnJob[i].CableLength.Should().Be(job.ConnectionsOnJob[i].CableLength);
			}

			restored.Should().Be(job);
			InfraOpsJsonConvert.SerializeObject(restored).Should().Be(json);
		}

		[TestMethod]
		public void PlanAndBuildJob_AuditFields_ExportedButNotImported()
		{
			var createdAt = new DateTime(2025, 5, 1, 10, 0, 0, DateTimeKind.Utc);
			var modifiedAt = new DateTime(2025, 6, 2, 11, 0, 0, DateTimeKind.Utc);
			var job = new PlanAndBuildJob
			{
				CreatedAt = createdAt,
				CreatedBy = "creator",
				LastModified = modifiedAt,
				LastModifiedBy = "modifier",
				JobID = "PB-0042",
				State = SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum.Assigned,
			};
#pragma warning disable CS0618 // Soft deleted field, internal setter
			job.JobType = SlcPlan_And_Build.Enums.JobtypeEnum.Remove;
#pragma warning restore CS0618

			string json = InfraOpsJsonConvert.SerializeObject(job);
			var restored = InfraOpsJsonConvert.DeserializeObject<PlanAndBuildJob>(json);

			json.Should().Contain("\"CreatedBy\":\"creator\"");
			json.Should().Contain("\"LastModifiedBy\":\"modifier\"");
			json.Should().Contain("\"JobID\":\"PB-0042\"");
			json.Should().Contain("\"State\":\"Assigned\"");

			restored.CreatedAt.Should().Be(default(DateTime));
			restored.CreatedBy.Should().BeNull();
			restored.LastModified.Should().Be(default(DateTime));
			restored.LastModifiedBy.Should().BeNull();
			restored.JobID.Should().BeNull();
			restored.State.Should().Be(SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum.New);
#pragma warning disable CS0618
			restored.JobType.Should().Be(SlcPlan_And_Build.Enums.JobtypeEnum.Add);
#pragma warning restore CS0618
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
