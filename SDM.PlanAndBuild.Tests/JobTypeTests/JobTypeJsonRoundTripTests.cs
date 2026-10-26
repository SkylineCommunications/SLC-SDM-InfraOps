namespace SDM.PlanAndBuild.Tests.JobTypeTests
{
	using System;

	using FluentAssertions;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using Skyline.DataMiner.SDM.InfraOps.Core.Converters;
	using Skyline.DataMiner.SDM.PlanAndBuild.Models;

	[TestClass]
	public class JobTypeJsonRoundTripTests
	{
		[TestMethod]
		public void JobType_RoundTrip_PreservesAllFields()
		{
			var jobType = new JobType
			{
				Identifier = Guid.NewGuid().ToString(),
				Name = "Installation",
				Description = "Install new equipment",
				Icon = "wrench",
			};

			string json = InfraOpsJsonConvert.SerializeObject(jobType);
			var restored = InfraOpsJsonConvert.DeserializeObject<JobType>(json);

			restored.Identifier.Should().Be(jobType.Identifier);
			restored.Name.Should().Be("Installation");
			restored.Description.Should().Be("Install new equipment");
			restored.Icon.Should().Be("wrench");
			restored.Should().Be(jobType);
		}
	}
}
