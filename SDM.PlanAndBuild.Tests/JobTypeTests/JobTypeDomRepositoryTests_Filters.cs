namespace SDM.PlanAndBuild.Tests.JobTypeTests
{
	using System.Linq;

	using FluentAssertions;
	using FluentAssertions.Execution;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using SDM.PlanAndBuild.Tests.Setup;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM.PlanAndBuild.Models;

	public partial class JobTypeDomRepositoryTests
	{
		[TestMethod]
		public void JobTypeDomRepository_ReadFilter_Name_Equal()
		{
			Helper.PopulateJobTypes();

			var expected = DemoData.JobTypes[0];
			var filter = JobTypeExposers.Name.Equal(expected.Name);

			var results = Helper.JobTypes.Read(filter);

			using (new AssertionScope())
			{
				results.Should().NotBeNull();
				results.Count().Should().Be(1);
				results.First().Name.Should().Be(expected.Name);
			}
		}

		[TestMethod]
		public void JobTypeDomRepository_ReadFilter_Identifier_Equal()
		{
			Helper.PopulateJobTypes();

			var expected = DemoData.JobTypes[1];
			var filter = JobTypeExposers.Identifier.Equal(expected.Identifier);

			var results = Helper.JobTypes.Read(filter);

			using (new AssertionScope())
			{
				results.Should().NotBeNull();
				results.Count().Should().Be(1);
				results.First().Identifier.Should().Be(expected.Identifier);
			}
		}

		[TestMethod]
		public void JobTypeDomRepository_ReadFilter_Description_Contains()
		{
			Helper.PopulateJobTypes();

			var filter = JobTypeExposers.Description.Contains("maintenance", System.StringComparison.OrdinalIgnoreCase);
			var expected = DemoData.JobTypes.Where(jt => jt.Description.Contains("maintenance", System.StringComparison.OrdinalIgnoreCase)).ToArray();

			var results = Helper.JobTypes.Read(filter);

			using (new AssertionScope())
			{
				results.Should().NotBeNull();
				results.Count().Should().Be(expected.Length);
			}
		}

		[TestMethod]
		public void JobTypeDomRepository_ReadFilter_Name_NotEqual()
		{
			Helper.PopulateJobTypes();

			var excluded = DemoData.JobTypes[0];
			var filter = JobTypeExposers.Name.UncheckedNotEqual(excluded.Name);
			var expected = DemoData.JobTypes.Where(jt => jt.Name != excluded.Name).ToArray();

			var results = Helper.JobTypes.Read(filter);

			using (new AssertionScope())
			{
				results.Should().NotBeNull();
				results.Count().Should().Be(expected.Length);
				results.Should().OnlyContain(jt => jt.Name != excluded.Name);
			}
		}
	}
}
