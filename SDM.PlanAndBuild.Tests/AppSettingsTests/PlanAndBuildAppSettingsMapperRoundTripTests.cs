namespace SDM.PlanAndBuild.Tests.AppSettingsTests
{
	using System;
	using System.Linq;

	using FluentAssertions;
	using FluentAssertions.Execution;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using SDM.PlanAndBuild.Tests.Setup;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM.PlanAndBuild.Models;

	/// <summary>
	/// Round-trip tests for the PlanAndBuildAppSettings DOM mapper (ToInstance/FromInstance).
	/// </summary>
	[TestClass]
	public class PlanAndBuildAppSettingsMapperRoundTripTests : BaseRepositoryTest
	{
		[TestMethod]
		public void RoundTrip_AppSettingsWithAllFieldsPopulated_ShouldPreserveEveryField()
		{
			var original = new PlanAndBuildAppSettings
			{
				Identifier = Guid.NewGuid().ToString(),
				JobIDPrefix = "JOB-",
				JobIDNextSequence = 5,
				JobIDIncrement = 1,
				JobIDStartingSeed = 1,
				JobIDMinimumDigits = 4,
			};

			Helper.AppSettings.Create(original);

			var roundTripped = Helper.AppSettings.Read(new TRUEFilterElement<PlanAndBuildAppSettings>())
				.Single(a => a.Identifier == original.Identifier);

			using (new AssertionScope())
			{
				roundTripped.Identifier.Should().Be(original.Identifier);
				roundTripped.JobIDPrefix.Should().Be(original.JobIDPrefix);
				roundTripped.JobIDNextSequence.Should().Be(original.JobIDNextSequence);
				roundTripped.JobIDIncrement.Should().Be(original.JobIDIncrement);
				roundTripped.JobIDStartingSeed.Should().Be(original.JobIDStartingSeed);
				roundTripped.JobIDMinimumDigits.Should().Be(original.JobIDMinimumDigits);
			}
		}

		[TestMethod]
		public void RoundTrip_AfterFetch_ShouldNotBeNewAndShouldHaveNoPendingChanges()
		{
			var original = new PlanAndBuildAppSettings
			{
				Identifier = Guid.NewGuid().ToString(),
				JobIDPrefix = "JOB-",
				JobIDNextSequence = 1,
				JobIDIncrement = 1,
				JobIDStartingSeed = 1,
				JobIDMinimumDigits = 4,
			};

			Helper.AppSettings.Create(original);

			var roundTripped = Helper.AppSettings.Read(new TRUEFilterElement<PlanAndBuildAppSettings>())
				.Single(a => a.Identifier == original.Identifier);

			using (new AssertionScope())
			{
				roundTripped.IsNew.Should().BeFalse();
				roundTripped.Changed.Should().BeFalse();
			}
		}
	}
}
