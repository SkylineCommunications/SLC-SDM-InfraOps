namespace SDM.PlanAndBuild.Tests.JobTypeTests
{
	using System.Linq;

	using FluentAssertions;
	using FluentAssertions.Execution;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using SDM.PlanAndBuild.Tests.Setup;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM.PlanAndBuild.Models;

	/// <summary>
	/// Round-trip tests for the JobType DOM mapper (ToInstance/FromInstance), exercising every mapped
	/// field to catch any field the generated mapper might silently drop or mistranslate.
	/// </summary>
	[TestClass]
	public class JobTypeMapperRoundTripTests : BaseRepositoryTest
	{
		[TestMethod]
		public void RoundTrip_JobTypeWithAllFieldsPopulated_ShouldPreserveEveryField()
		{
			var original = new JobType
			{
				Identifier = System.Guid.NewGuid().ToString(),
				Name = "Installation",
				Description = "New equipment installation jobs",
				Icon = "install-icon",
			};

			Helper.JobTypes.Create(original);

			var roundTripped = Helper.JobTypes.Read(JobTypeExposers.Identifier.Equal(original.Identifier)).Single();

			using (new AssertionScope())
			{
				roundTripped.Identifier.Should().Be(original.Identifier);
				roundTripped.Name.Should().Be(original.Name);
				roundTripped.Description.Should().Be(original.Description);
				roundTripped.Icon.Should().Be(original.Icon);
			}
		}

		[TestMethod]
		public void RoundTrip_AfterFetch_ShouldNotBeNewAndShouldHaveNoPendingChanges()
		{
			var original = new JobType
			{
				Identifier = System.Guid.NewGuid().ToString(),
				Name = "Maintenance",
				Description = "Scheduled maintenance jobs",
				Icon = "maintenance-icon",
			};

			Helper.JobTypes.Create(original);

			var roundTripped = Helper.JobTypes.Read(JobTypeExposers.Identifier.Equal(original.Identifier)).Single();

			using (new AssertionScope())
			{
				roundTripped.IsNew.Should().BeFalse();
				roundTripped.Changed.Should().BeFalse();
			}
		}
	}
}
