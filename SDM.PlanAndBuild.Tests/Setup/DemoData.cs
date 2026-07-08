namespace SDM.PlanAndBuild.Tests.Setup
{
	using System;
	using System.Collections.Generic;

	using SharedMappers.DomIds;

	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.SDM.PlanAndBuild.Models;

	public static class DemoData
	{
		public static readonly List<JobType> JobTypes =
		[
			new JobType
			{
				Identifier = Guid.NewGuid().ToString(),
				Name = "Installation",
				Description = "New equipment installation jobs",
				Icon = "install-icon",
			},
			new JobType
			{
				Identifier = Guid.NewGuid().ToString(),
				Name = "Maintenance",
				Description = "Scheduled maintenance jobs",
				Icon = "maintenance-icon",
			},
			new JobType
			{
				Identifier = Guid.NewGuid().ToString(),
				Name = "Decommissioning",
				Description = "Equipment decommissioning jobs",
				Icon = "decommission-icon",
			},
		];

		public static readonly List<PlanAndBuildJob> Jobs =
		[
			new PlanAndBuildJob
			{
				Identifier = Guid.NewGuid().ToString(),
				JobID = "JOB-0001",
				JobName = "Install Rack 1 Equipment",
				Start = new DateTime(2026, 1, 10),
				End = new DateTime(2026, 1, 15),
				JobType = new SdmObjectReference<JobType>(JobTypes[0].Identifier),
				Type = SlcPlan_And_Build.Enums.JobtypeEnum.Add,
				JobDescription = "Install new equipment in Rack 1",
				Remarks = string.Empty,
				Priority = SlcPlan_And_Build.Enums.PriorityEnum.High,
				SubState = SlcPlan_And_Build.Enums.SubStateEnum.Scheduled,
			},
			new PlanAndBuildJob
			{
				Identifier = Guid.NewGuid().ToString(),
				JobID = "JOB-0002",
				JobName = "Quarterly Maintenance Check",
				Start = new DateTime(2026, 2, 1),
				End = new DateTime(2026, 2, 2),
				JobType = new SdmObjectReference<JobType>(JobTypes[1].Identifier),
				Type = SlcPlan_And_Build.Enums.JobtypeEnum.Update,
				JobDescription = "Routine maintenance check",
				Remarks = "Bring spare fans",
				Priority = SlcPlan_And_Build.Enums.PriorityEnum.Normal,
				SubState = SlcPlan_And_Build.Enums.SubStateEnum.InProgress,
			},
			new PlanAndBuildJob
			{
				Identifier = Guid.NewGuid().ToString(),
				JobID = "JOB-0003",
				JobName = "Decommission Legacy Server",
				Start = new DateTime(2026, 3, 5),
				End = null,
				JobType = new SdmObjectReference<JobType>(JobTypes[2].Identifier),
				Type = SlcPlan_And_Build.Enums.JobtypeEnum.Remove,
				JobDescription = "Remove legacy server from Rack 2",
				Remarks = string.Empty,
				Priority = SlcPlan_And_Build.Enums.PriorityEnum.Low,
				SubState = SlcPlan_And_Build.Enums.SubStateEnum.Draft,
			},
			new PlanAndBuildJob
			{
				Identifier = Guid.NewGuid().ToString(),
				JobID = "JOB-0004",
				JobName = "Emergency Cabling Fix",
				Start = new DateTime(2026, 1, 20),
				End = new DateTime(2026, 1, 21),
				JobType = new SdmObjectReference<JobType>(JobTypes[0].Identifier),
				Type = SlcPlan_And_Build.Enums.JobtypeEnum.Update,
				JobDescription = "Fix damaged cabling",
				Remarks = "Customer escalation",
				Priority = SlcPlan_And_Build.Enums.PriorityEnum.Critical,
				SubState = SlcPlan_And_Build.Enums.SubStateEnum.PendingKickoff,
			},
		];

		public static readonly List<PlanAndBuildAppSettings> AppSettingsList =
		[
			new PlanAndBuildAppSettings
			{
				Identifier = Guid.NewGuid().ToString(),
				JobIDPrefix = "JOB-",
				JobIDNextSequence = 5,
				JobIDIncrement = 1,
				JobIDStartingSeed = 1,
				JobIDMinimumDigits = 4,
			},
		];
	}
}
