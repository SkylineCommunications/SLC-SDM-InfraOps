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

	[TestClass]
	public class PlanAndBuildAppSettingsDomRepositoryTests : BaseRepositoryTest
	{
		private PlanAndBuildAppSettings referenceAppSettings = null!;

		[TestInitialize]
		public void TestInitialize()
		{
			referenceAppSettings = new PlanAndBuildAppSettings
			{
				Identifier = Guid.NewGuid().ToString(),
				JobIDPrefix = "JOB-",
				JobIDNextSequence = 1,
				JobIDIncrement = 1,
				JobIDStartingSeed = 1,
				JobIDMinimumDigits = 4,
			};
		}

		[TestMethod]
		public void PlanAndBuildAppSettingsDomRepository_EmptyDOM_Create()
		{
			Helper.AppSettings.Create(referenceAppSettings);

			AssertCreated();
		}

		[TestMethod]
		public void PlanAndBuildAppSettingsDomRepository_EmptyDOM_CreateOrUpdate_Create()
		{
			Helper.AppSettings.CreateOrUpdate([referenceAppSettings]);

			AssertCreated();
		}

		[TestMethod]
		public void PlanAndBuildAppSettingsDomRepository_EmptyDOM_CreateOrUpdate_Update()
		{
			Helper.AppSettings.Create(referenceAppSettings);

			var updated = new PlanAndBuildAppSettings
			{
				Identifier = referenceAppSettings.Identifier,
				JobIDPrefix = "JOB-",
				JobIDNextSequence = 42,
				JobIDIncrement = 1,
				JobIDStartingSeed = 1,
				JobIDMinimumDigits = 4,
			};

			Helper.AppSettings.CreateOrUpdate([updated]);

			var fetched = Helper.AppSettings.Read(new TRUEFilterElement<PlanAndBuildAppSettings>())
				.Single(a => a.Identifier == referenceAppSettings.Identifier);

			fetched.JobIDNextSequence.Should().Be(42);
		}

		[TestMethod]
		public void PlanAndBuildAppSettingsDomRepository_DeleteSingle()
		{
			Helper.AppSettings.Create(referenceAppSettings);

			Helper.AppSettings.Delete(referenceAppSettings);

			Helper.AppSettings.Count(new TRUEFilterElement<PlanAndBuildAppSettings>()).Should().Be(0);
		}

		[TestMethod]
		public void PlanAndBuildAppSettingsDomRepository_ReadPaged()
		{
			const int pageCount = 1;
			Helper.PopulateAppSettings();

			FilterElement<PlanAndBuildAppSettings> allFilter = new TRUEFilterElement<PlanAndBuildAppSettings>();
			var pagedResult = Helper.AppSettings.ReadPaged(allFilter, pageCount);
			var count = Helper.AppSettings.Count(allFilter);

			using (new AssertionScope())
			{
				pagedResult.Should().NotBeNull();
				pagedResult.Should().HaveCount((int)Math.Ceiling(count / (double)pageCount));
			}
		}

		private void AssertCreated()
		{
			using (new AssertionScope())
			{
				Helper.AppSettings.Count(new TRUEFilterElement<PlanAndBuildAppSettings>()).Should().Be(1);

				var created = Helper.AppSettings.Read(new TRUEFilterElement<PlanAndBuildAppSettings>()).First();
				created.Should().NotBeNull();
				created.JobIDPrefix.Should().Be("JOB-");
				created.JobIDNextSequence.Should().Be(1);
				created.JobIDIncrement.Should().Be(1);
				created.JobIDStartingSeed.Should().Be(1);
				created.JobIDMinimumDigits.Should().Be(4);
			}
		}
	}
}
