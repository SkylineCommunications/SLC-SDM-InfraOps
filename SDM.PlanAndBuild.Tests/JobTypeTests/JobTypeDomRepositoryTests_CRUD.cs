namespace SDM.PlanAndBuild.Tests.JobTypeTests
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
	public partial class JobTypeDomRepositoryTests : BaseRepositoryTest
	{
		private JobType referenceJobType = null!;

		[TestInitialize]
		public void TestInitialize()
		{
			var id = Guid.NewGuid();
			referenceJobType = new JobType
			{
				Identifier = id.ToString(),
				Name = "Installation",
				Description = "New equipment installation jobs",
				Icon = "install-icon",
			};
		}

		[TestMethod]
		public void JobTypeDomRepository_EmptyDOM_Create()
		{
			Helper.JobTypes.Create(referenceJobType);

			AssertCreated();
		}

		[TestMethod]
		public void JobTypeDomRepository_EmptyDOM_CreateOrUpdate_Create()
		{
			Helper.JobTypes.CreateOrUpdate([referenceJobType]);

			AssertCreated();
		}

		[TestMethod]
		public void JobTypeDomRepository_EmptyDOM_CreateOrUpdate_Update()
		{
			Helper.JobTypes.Create(referenceJobType);

			var updatedJobType = new JobType
			{
				Identifier = referenceJobType.Identifier,
				Name = "Updated Installation",
				Description = "Updated description",
				Icon = "updated-icon",
			};

			Helper.JobTypes.CreateOrUpdate([updatedJobType]);
			AssertJobTypeUpdateDifferences(updatedJobType);
		}

		[TestMethod]
		public void JobTypeDomRepository_ReadPaged()
		{
			const int pageCount = 2;
			Helper.PopulateJobTypes();

			FilterElement<JobType> allFilter = new TRUEFilterElement<JobType>();
			var pagedResult = Helper.JobTypes.ReadPaged(allFilter, pageCount);
			var jobTypeCount = Helper.JobTypes.Count(allFilter);

			using (new AssertionScope())
			{
				pagedResult.Should().NotBeNull();
				pagedResult.Should().HaveCount((int)Math.Ceiling(jobTypeCount / (double)pageCount));
			}
		}

		[TestMethod]
		public void JobTypeDomRepository_DeleteBulk()
		{
			Helper.PopulateJobTypes();

			var filter = new ORFilterElement<JobType>(
				JobTypeExposers.Name.Equal(DemoData.JobTypes[0].Name),
				JobTypeExposers.Name.Equal(DemoData.JobTypes[1].Name));
			var jobTypesToDelete = Helper.JobTypes.Read(filter);

			Helper.JobTypes.Delete(jobTypesToDelete);

			using (new AssertionScope())
			{
				Helper.JobTypes.Count(new TRUEFilterElement<JobType>()).Should().BeLessThan(DemoData.JobTypes.Count);
				Helper.JobTypes.Count(JobTypeExposers.Name.Equal(DemoData.JobTypes[0].Name)).Should().Be(0);
			}
		}

		[TestMethod]
		public void JobTypeDomRepository_EmptyDOM_DeleteSingle()
		{
			Helper.PopulateJobTypes();

			var jobTypeToDelete = Helper.JobTypes.Read(JobTypeExposers.Name.Equal(DemoData.JobTypes[0].Name)).First();

			Helper.JobTypes.Delete(jobTypeToDelete);

			Helper.JobTypes.Count(new TRUEFilterElement<JobType>()).Should().Be(DemoData.JobTypes.Count - 1);
			Helper.JobTypes.Count(JobTypeExposers.Identifier.Equal(jobTypeToDelete.Identifier)).Should().Be(0);
		}

		[TestMethod]
		public void JobTypeDomRepository_DeleteSingle_WhenInUseByJob_ShouldThrow()
		{
			Helper.PopulateJobTypes();
			var jobTypeInUse = Helper.JobTypes.Read(JobTypeExposers.Name.Equal(DemoData.JobTypes[0].Name)).First();
			Helper.Jobs.Create(new PlanAndBuildJob
			{
				JobName = "Job referencing type",
				JobType = new SdmObjectReference<JobType>(jobTypeInUse.Identifier),
			});

			Action act = () => Helper.JobTypes.Delete(jobTypeInUse);

			act.Should().Throw<ValidationException>();
		}

		private static void AssertJobTypeUpdateDifferences(JobType updated)
		{
			using (new AssertionScope())
			{
				updated.Name.Should().Be("Updated Installation");
				updated.Description.Should().Be("Updated description");
				updated.Icon.Should().Be("updated-icon");
			}
		}

		private void AssertCreated()
		{
			using (new AssertionScope())
			{
				Helper.JobTypes.Count(new TRUEFilterElement<JobType>()).Should().Be(1);

				var createdJobType = Helper.JobTypes.Read(new TRUEFilterElement<JobType>()).First();
				createdJobType.Should().NotBeNull();
				createdJobType.Name.Should().Be("Installation");
				createdJobType.Description.Should().Be("New equipment installation jobs");
				createdJobType.Icon.Should().Be("install-icon");
			}
		}
	}
}
