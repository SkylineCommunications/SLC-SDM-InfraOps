namespace SDM.PlanAndBuild.Tests.JobTests
{
	using System;
	using System.Linq;

	using FluentAssertions;
	using FluentAssertions.Execution;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using SDM.PlanAndBuild.Tests.Setup;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.SDM.PlanAndBuild.Models;
	using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Exceptions;

	[TestClass]
	public partial class PlanAndBuildJobDomRepositoryTests : BaseRepositoryTest
	{
		private JobType referenceJobType = null!;
		private PlanAndBuildJob referenceJob = null!;

		[TestInitialize]
		public void TestInitialize()
		{
			Helper.PopulateAppSettings();

			referenceJobType = new JobType { Name = "Installation" };

			referenceJob = new PlanAndBuildJob
			{
				Identifier = Guid.NewGuid().ToString(),
				JobName = "Install Rack 1 Equipment",
				Start = new DateTime(2026, 1, 10),
				End = new DateTime(2026, 1, 15),
			};
		}

		[TestMethod]
		public void PlanAndBuildJobDomRepository_EmptyDOM_Create()
		{
			referenceJobType = Helper.JobTypes.Create(referenceJobType);
			referenceJob.Type = new SdmObjectReference<JobType>(referenceJobType.Identifier);

			Helper.Jobs.Create(referenceJob);

			AssertCreated();
		}

		[TestMethod]
		public void PlanAndBuildJobDomRepository_EmptyDOM_CreateOrUpdate_Create()
		{
			referenceJobType = Helper.JobTypes.Create(referenceJobType);
			referenceJob.Type = new SdmObjectReference<JobType>(referenceJobType.Identifier);

			Helper.Jobs.CreateOrUpdate([referenceJob]);

			AssertCreated();
		}

		[TestMethod]
		public void PlanAndBuildJobDomRepository_EmptyDOM_CreateOrUpdate_Update()
		{
			referenceJobType = Helper.JobTypes.Create(referenceJobType);
			referenceJob.Type = new SdmObjectReference<JobType>(referenceJobType.Identifier);
			Helper.Jobs.Create(referenceJob);

			var updatedJob = new PlanAndBuildJob
			{
				Identifier = referenceJob.Identifier,
				JobName = "Install Rack 1 Equipment - Updated",
				Type = new SdmObjectReference<JobType>(referenceJobType.Identifier),
				Remarks = "Updated remarks",
			};

			Helper.Jobs.CreateOrUpdate([updatedJob]);

			using (new AssertionScope())
			{
				updatedJob.JobName.Should().Be("Install Rack 1 Equipment - Updated");
				updatedJob.Remarks.Should().Be("Updated remarks");
			}
		}

		[TestMethod]
		public void PlanAndBuildJobDomRepository_ReadPaged()
		{
			const int pageCount = 2;
			Helper.PopulateJobTypes();
			Helper.PopulateJobs();

			FilterElement<PlanAndBuildJob> allFilter = new TRUEFilterElement<PlanAndBuildJob>();
			var pagedResult = Helper.Jobs.ReadPaged(allFilter, pageCount);
			var jobCount = Helper.Jobs.Count(allFilter);

			using (new AssertionScope())
			{
				pagedResult.Should().NotBeNull();
				pagedResult.Should().HaveCount((int)Math.Ceiling(jobCount / (double)pageCount));
			}
		}

		[TestMethod]
		public void PlanAndBuildJobDomRepository_DeleteSingle()
		{
			referenceJobType = Helper.JobTypes.Create(referenceJobType);
			referenceJob.Type = new SdmObjectReference<JobType>(referenceJobType.Identifier);
			Helper.Jobs.Create(referenceJob);

			Helper.Jobs.Delete(referenceJob);

			Helper.Jobs.Count(new TRUEFilterElement<PlanAndBuildJob>()).Should().Be(0);
		}

		[TestMethod]
		public void PlanAndBuildJobDomRepository_DeleteBulk()
		{
			Helper.PopulateJobTypes();
			Helper.PopulateJobs();

			var filter = new ORFilterElement<PlanAndBuildJob>(
				PlanAndBuildJobExposers.JobName.Equal(DemoData.Jobs[0].JobName),
				PlanAndBuildJobExposers.JobName.Equal(DemoData.Jobs[1].JobName));
			var jobsToDelete = Helper.Jobs.Read(filter);

			Helper.Jobs.Delete(jobsToDelete);

			Helper.Jobs.Count(new TRUEFilterElement<PlanAndBuildJob>()).Should().BeLessThan(DemoData.Jobs.Count);
		}

		[TestMethod]
		public void PlanAndBuildJobDomRepository_Create_WithDuplicateJobName_ShouldThrow()
		{
			referenceJobType = Helper.JobTypes.Create(referenceJobType);
			referenceJob.Type = new SdmObjectReference<JobType>(referenceJobType.Identifier);
			Helper.Jobs.Create(referenceJob);

			var duplicate = new PlanAndBuildJob
			{
				JobName = referenceJob.JobName,
				Type = new SdmObjectReference<JobType>(referenceJobType.Identifier),
			};

			Action act = () => Helper.Jobs.Create(duplicate);

			act.Should().Throw<ValidationException>();
		}

		[TestMethod]
		public void PlanAndBuildJobDomRepository_Create_WithoutJobType_ShouldThrow()
		{
			var jobWithoutType = new PlanAndBuildJob
			{
				JobName = "Job Without Type",
				Type = null,
			};

			Action act = () => Helper.Jobs.Create(jobWithoutType);

			act.Should().Throw<ValidationException>();
		}

		private void AssertCreated()
		{
			using (new AssertionScope())
			{
				Helper.Jobs.Count(new TRUEFilterElement<PlanAndBuildJob>()).Should().Be(1);

				var createdJob = Helper.Jobs.Read(new TRUEFilterElement<PlanAndBuildJob>()).First();
				createdJob.Should().NotBeNull();
				createdJob.JobName.Should().Be("Install Rack 1 Equipment");
				createdJob.Type.Should().Be(new SdmObjectReference<JobType>(referenceJobType.Identifier));
				createdJob.Start.Should().Be(new DateTime(2026, 1, 10));
				createdJob.End.Should().Be(new DateTime(2026, 1, 15));
			}
		}
	}
}
