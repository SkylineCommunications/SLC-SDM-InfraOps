namespace SDM.PlanAndBuild.Tests.JobTests
{
	using System.Linq;

	using FluentAssertions;
	using FluentAssertions.Execution;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using SDM.PlanAndBuild.Tests.Setup;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.SDM.PlanAndBuild.Models;

	using SharedMappers.DomIds;

	public partial class PlanAndBuildJobDomRepositoryTests
	{
		[TestMethod]
		public void PlanAndBuildJobDomRepository_ReadFilter_JobName_Equal()
		{
			Helper.PopulateJobTypes();
			Helper.PopulateJobs();

			var expected = DemoData.Jobs[0];
			var filter = PlanAndBuildJobExposers.JobName.Equal(expected.JobName);

			var results = Helper.Jobs.Read(filter);

			using (new AssertionScope())
			{
				results.Should().NotBeNull();
				results.Count().Should().Be(1);
				results.First().JobName.Should().Be(expected.JobName);
			}
		}

		[TestMethod]
		public void PlanAndBuildJobDomRepository_ReadFilter_JobType_Equal()
		{
			Helper.PopulateJobTypes();
			Helper.PopulateJobs();

			var jobTypeReference = new SdmObjectReference<JobType>(DemoData.JobTypes[0].Identifier);
			var filter = PlanAndBuildJobExposers.JobType.Equal(jobTypeReference);
			var expected = DemoData.Jobs.Where(j => j.JobType == jobTypeReference).ToArray();

			var results = Helper.Jobs.Read(filter);

			using (new AssertionScope())
			{
				results.Should().NotBeNull();
				results.Count().Should().Be(expected.Length);
				results.Should().OnlyContain(j => j.JobType == jobTypeReference);
			}
		}

		[TestMethod]
		public void PlanAndBuildJobDomRepository_ReadFilter_Priority_Equal()
		{
			Helper.PopulateJobTypes();
			Helper.PopulateJobs();

			var filter = PlanAndBuildJobExposers.Priority.Equal(SlcPlan_And_Build.Enums.PriorityEnum.Critical);
			var expected = DemoData.Jobs.Where(j => j.Priority == SlcPlan_And_Build.Enums.PriorityEnum.Critical).ToArray();

			var results = Helper.Jobs.Read(filter);

			using (new AssertionScope())
			{
				results.Should().NotBeNull();
				results.Count().Should().Be(expected.Length);
				results.Should().OnlyContain(j => j.Priority == SlcPlan_And_Build.Enums.PriorityEnum.Critical);
			}
		}

		[TestMethod]
		public void PlanAndBuildJobDomRepository_ReadFilter_SubState_Equal()
		{
			Helper.PopulateJobTypes();
			Helper.PopulateJobs();

			var filter = PlanAndBuildJobExposers.SubState.UncheckedEqual(SlcPlan_And_Build.Enums.SubStateEnum.InProgress);
			var expected = DemoData.Jobs.Where(j => j.SubState == SlcPlan_And_Build.Enums.SubStateEnum.InProgress).ToArray();

			var results = Helper.Jobs.Read(filter);

			using (new AssertionScope())
			{
				results.Should().NotBeNull();
				results.Count().Should().Be(expected.Length);
			}
		}

		[TestMethod]
		public void PlanAndBuildJobDomRepository_ReadFilter_Start_GreaterThanOrEqual()
		{
			Helper.PopulateJobTypes();
			Helper.PopulateJobs();

			var threshold = new System.DateTime(2026, 2, 1);
			var filter = PlanAndBuildJobExposers.Start.GreaterThanOrEqual(threshold);
			var expected = DemoData.Jobs.Where(j => j.Start != null && j.Start >= threshold).ToArray();

			var results = Helper.Jobs.Read(filter);

			using (new AssertionScope())
			{
				results.Should().NotBeNull();
				results.Count().Should().Be(expected.Length);
			}
		}

		[TestMethod]
		public void PlanAndBuildJobDomRepository_ReadFilter_JobDescription_Contains()
		{
			Helper.PopulateJobTypes();
			Helper.PopulateJobs();

			var filter = PlanAndBuildJobExposers.JobDescription.Contains("legacy", System.StringComparison.OrdinalIgnoreCase);
			var expected = DemoData.Jobs.Where(j => j.JobDescription.Contains("legacy", System.StringComparison.OrdinalIgnoreCase)).ToArray();

			var results = Helper.Jobs.Read(filter);

			using (new AssertionScope())
			{
				results.Should().NotBeNull();
				results.Count().Should().Be(expected.Length);
			}
		}

		[TestMethod]
		public void PlanAndBuildJobDomRepository_ReadFilter_JobTypeAndPriority_Combined()
		{
			Helper.PopulateJobTypes();
			Helper.PopulateJobs();

			var jobTypeReference = new SdmObjectReference<JobType>(DemoData.JobTypes[0].Identifier);
			var combinedFilter = PlanAndBuildJobExposers.JobType.Equal(jobTypeReference)
				.AND(PlanAndBuildJobExposers.Priority.Equal(SlcPlan_And_Build.Enums.PriorityEnum.High));

			var expected = DemoData.Jobs.Where(j => j.JobType == jobTypeReference && j.Priority == SlcPlan_And_Build.Enums.PriorityEnum.High).ToArray();

			var results = Helper.Jobs.Read(combinedFilter);

			using (new AssertionScope())
			{
				results.Should().NotBeNull();
				results.Count().Should().Be(expected.Length);
			}
		}
	}
}
