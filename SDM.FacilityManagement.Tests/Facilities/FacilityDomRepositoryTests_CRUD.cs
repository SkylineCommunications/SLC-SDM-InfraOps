namespace SDM.FacilityManagement.Tests.Facilities
{
	using System;
	using System.Linq;
	using FluentAssertions;
	using FluentAssertions.Execution;
	using SDM.FacilityManagement.Tests.Setup;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM.FacilityManagement.Enums;
	using Skyline.DataMiner.SDM.FacilityManagement.Helpers;
	using Skyline.DataMiner.SDM.FacilityManagement.Models;

	[TestClass]
	public partial class FacilityDomRepositoryTests
	{
		private Facility referenceFacility;

		[TestInitialize]
		public void TestInitialize()
		{
			var id = Guid.NewGuid();
			referenceFacility = new Facility
			{
				Identifier = id.ToString(),
				FacilityId = "DTC-A",
				Name = "Data Center A",
				Description = "A datacenter facility for testing.",
				FacilityType = SlcFacilityManagement.Enums.FacilityType.Building,
				Address = "Ombstrat 12",
				City = "Oslo",
				ZipCode = "7000",
				Country = "Norway",
				Latitude = 59.9122,
				Longitude = 10.7313,
			};
		}

		[TestMethod]
		public void FacilityDomRepository_EmptyDOM_Create()
		{
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.Facilities.Create(referenceFacility);

			AssertCreated(helper);
		}

		[TestMethod]
		public void FacilityDomRepository_EmptyDOM_CreateOrUpdate_Create()
		{
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.Facilities.CreateOrUpdate([referenceFacility]);

			AssertCreated(helper);
		}

		[TestMethod]
		public void FacilityDomRepository_EmptyDOM_CreateOrUpdate_Update()
		{
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.Facilities.Create(referenceFacility);

			var updatedFacility = new Facility
			{
				Identifier = referenceFacility.Identifier,
				FacilityId = "FAC-001",
				Name = "Updated Facility Name",
				Description = "Updated facility description.",
				FacilityType = SlcFacilityManagement.Enums.FacilityType.Container,
				Address = "456 Updated Street",
				City = "Los Angeles",
				ZipCode = "90001",
				Country = "USA",
				Latitude = 34.0522,
				Longitude = -118.2437,
			};

			helper.Facilities.CreateOrUpdate([updatedFacility]);
			AssertFacilityUpdateDifferences(referenceFacility, updatedFacility);
		}

		[TestMethod]
		public void FacilityDomRepository_ReadPaged()
		{
			const int pageCount = 2;
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateFacilities();

			FilterElement<Facility> allFilter = new TRUEFilterElement<Facility>();
			var pagedResult = helper.Facilities.ReadPaged(allFilter, pageCount);
			var facilityCount = helper.Facilities.Count(allFilter);

			using (new AssertionScope())
			{
				pagedResult.Should().NotBeNull();
				pagedResult.Should().HaveCount((int)(facilityCount / pageCount));
				pagedResult.Should().AllSatisfy(page => page.Should().HaveCount(pageCount));
			}
		}

		[TestMethod]
		public void FacilityDomRepository_DeleteBulk()
		{
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateFacilities();

			var filter = new ORFilterElement<Facility>(
				FacilityExposers.Country.Equal("Brazil"),
				FacilityExposers.City.Equal("New York"));
			var facilitiesToDelete = helper.Facilities.Read(filter);

			helper.Facilities.Delete(facilitiesToDelete);

			using (new AssertionScope())
			{
				helper.Facilities.Count(new TRUEFilterElement<Facility>()).Should().BeLessThan(DemoData.Facilities.Count);
				helper.Facilities.Count(FacilityExposers.Country.Equal("Brazil")).Should().Be(0);
				helper.Facilities.Count(FacilityExposers.City.Equal("New York")).Should().Be(0);
			}
		}

		[TestMethod]
		public void FacilityDomRepository_EmptyDOM_DeleteSingle()
		{
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateFacilities();

			var facilityToDelete = helper.Facilities.Read(FacilityExposers.Name.Equal(DemoData.Facilities[8].Name)).First();

			helper.Facilities.Delete(facilityToDelete);

			helper.Facilities.Count(new TRUEFilterElement<Facility>()).Should().Be(DemoData.Facilities.Count - 1);
			helper.Facilities.Count(FacilityExposers.Identifier.Equal(facilityToDelete.Identifier)).Should().Be(0);
		}

		private static void AssertFacilityUpdateDifferences(Facility original, Facility updated)
		{
			using (new AssertionScope())
			{
				updated.FacilityId.Should().Be("FAC-001");
				updated.Name.Should().NotBe(original.Name);
				updated.Name.Should().Be("Updated Facility Name");
				updated.Description.Should().NotBe(original.Description);
				updated.Description.Should().Be("Updated facility description.");
				updated.FacilityType.Should().Be(SlcFacilityManagement.Enums.FacilityType.Container);
				updated.Address.Should().Be("456 Updated Street");
				updated.City.Should().Be("Los Angeles");
				updated.ZipCode.Should().Be("90001");
				updated.Country.Should().Be("USA");
				updated.Latitude.Should().Be(34.0522);
				updated.Longitude.Should().Be(-118.2437);
			}
		}

		private void AssertCreated(IFacilityManagementApiHelper helper)
		{
			using (new AssertionScope())
			{
				helper.Facilities.Count(new TRUEFilterElement<Facility>()).Should().Be(1);

				var createdFacility = helper.Facilities.Read(new TRUEFilterElement<Facility>()).First();
				createdFacility.Should().NotBeNull();
				createdFacility.FacilityId.Should().Be("DTC-A");
				createdFacility.Name.Should().Be("Data Center A");
				createdFacility.Description.Should().Be("A datacenter facility for testing.");
				createdFacility.FacilityType.Should().Be(SlcFacilityManagement.Enums.FacilityType.Building);
				createdFacility.Address.Should().Be("Ombstrat 12");
				createdFacility.City.Should().Be("Oslo");
				createdFacility.ZipCode.Should().Be("7000");
				createdFacility.Country.Should().Be("Norway");
				createdFacility.Latitude.Should().Be(59.9122);
				createdFacility.Longitude.Should().Be(10.7313);
			}
		}
	}
}
