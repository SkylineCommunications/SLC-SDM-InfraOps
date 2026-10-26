namespace SDM.PlanAndBuild.Tests.AppSettingsTests
{
	using System;

	using FluentAssertions;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using Skyline.DataMiner.SDM.InfraOps.Core.Converters;
	using Skyline.DataMiner.SDM.PlanAndBuild.Models;

	[TestClass]
	public class PlanAndBuildAppSettingsJsonRoundTripTests
	{
		[TestMethod]
		public void PlanAndBuildAppSettings_RoundTrip_PreservesAllFields()
		{
			var settings = new PlanAndBuildAppSettings
			{
				Identifier = Guid.NewGuid().ToString(),
				JobIDPrefix = "PB-",
				JobIDNextSequence = 42,
				JobIDIncrement = 3,
				JobIDStartingSeed = 7,
				JobIDMinimumDigits = 4,
			};

			string json = InfraOpsJsonConvert.SerializeObject(settings);
			var restored = InfraOpsJsonConvert.DeserializeObject<PlanAndBuildAppSettings>(json);

			restored.Identifier.Should().Be(settings.Identifier);
			restored.JobIDPrefix.Should().Be("PB-");
			restored.JobIDNextSequence.Should().Be(42);
			restored.JobIDIncrement.Should().Be(3);
			restored.JobIDStartingSeed.Should().Be(7);
			restored.JobIDMinimumDigits.Should().Be(4);
			restored.Should().Be(settings);
			InfraOpsJsonConvert.SerializeObject(restored).Should().Be(json);
		}

		[TestMethod]
		public void PlanAndBuildAppSettings_AuditFields_ExportedButNotImported()
		{
			var settings = new PlanAndBuildAppSettings
			{
				CreatedAt = new DateTime(2025, 5, 1, 10, 0, 0, DateTimeKind.Utc),
				CreatedBy = "creator",
				LastModified = new DateTime(2025, 6, 2, 11, 0, 0, DateTimeKind.Utc),
				LastModifiedBy = "modifier",
			};

			string json = InfraOpsJsonConvert.SerializeObject(settings);
			var restored = InfraOpsJsonConvert.DeserializeObject<PlanAndBuildAppSettings>(json);

			json.Should().Contain("\"CreatedBy\":\"creator\"");
			json.Should().Contain("\"LastModifiedBy\":\"modifier\"");
			restored.CreatedAt.Should().Be(default(DateTime));
			restored.CreatedBy.Should().BeNull();
			restored.LastModified.Should().Be(default(DateTime));
			restored.LastModifiedBy.Should().BeNull();
		}
	}
}
