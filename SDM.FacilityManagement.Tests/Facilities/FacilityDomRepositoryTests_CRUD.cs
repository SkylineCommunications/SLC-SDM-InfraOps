namespace SDM.FacilityManagement.Tests.Facilities
{
	using System;
	using System.Linq;
	using FluentAssertions;
	using FluentAssertions.Execution;
	using SDM.FacilityManagement.Tests.Setup;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM.FacilityManagement.Models;

	[TestClass]
	public partial class FacilityDomRepositoryTests : BaseRepositoryTest
	{
		private Facility referenceFacility = null!;

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
				FacilityType = SlcFacility_Management.Enums.FacilityTypeEnum.Building,
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
			Helper.Facilities.Create(referenceFacility);

			AssertCreated();
		}

		[TestMethod]
		public void FacilityDomRepository_EmptyDOM_CreateOrUpdate_Create()
		{
			Helper.Facilities.CreateOrUpdate([referenceFacility]);

			AssertCreated();
		}

		[TestMethod]
		public void FacilityDomRepository_EmptyDOM_CreateOrUpdate_Update()
		{
			Helper.Facilities.Create(referenceFacility);

			var updatedFacility = new Facility
			{
				Identifier = referenceFacility.Identifier,
				FacilityId = "FAC-001",
				Name = "Updated Facility Name",
				Description = "Updated facility description.",
				FacilityType = SlcFacility_Management.Enums.FacilityTypeEnum.Container,
				Address = "456 Updated Street",
				City = "Los Angeles",
				ZipCode = "90001",
				Country = "USA",
				Latitude = 34.0522,
				Longitude = -118.2437,
			};

			Helper.Facilities.CreateOrUpdate([updatedFacility]);
			AssertFacilityUpdateDifferences(referenceFacility, updatedFacility);
		}

		[TestMethod]
		public void FacilityDomRepository_ReadPaged()
		{
			const int pageCount = 2;
			Helper.PopulateFacilities();

			FilterElement<Facility> allFilter = new TRUEFilterElement<Facility>();
			var pagedResult = Helper.Facilities.ReadPaged(allFilter, pageCount);
            var facilityCount = Helper.Facilities.Count(allFilter);

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
			Helper.PopulateFacilities();

			var filter = new ORFilterElement<Facility>(
				FacilityExposers.Country.Equal("Brazil"),
				FacilityExposers.City.Equal("New York"));
			var facilitiesToDelete = Helper.Facilities.Read(filter);

			Helper.Facilities.Delete(facilitiesToDelete);

			using (new AssertionScope())
			{
				Helper.Facilities.Count(new TRUEFilterElement<Facility>()).Should().BeLessThan(DemoData.Facilities.Count);
				Helper.Facilities.Count(FacilityExposers.Country.Equal("Brazil")).Should().Be(0);
				Helper.Facilities.Count(FacilityExposers.City.Equal("New York")).Should().Be(0);
			}
		}

		[TestMethod]
		public void FacilityDomRepository_EmptyDOM_DeleteSingle()
		{
			Helper.PopulateFacilities();

			var facilityToDelete = Helper.Facilities.Read(FacilityExposers.Name.Equal(DemoData.Facilities[8].Name)).First();

			Helper.Facilities.Delete(facilityToDelete);

			Helper.Facilities.Count(new TRUEFilterElement<Facility>()).Should().Be(DemoData.Facilities.Count - 1);
			Helper.Facilities.Count(FacilityExposers.Identifier.Equal(facilityToDelete.Identifier)).Should().Be(0);
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
				updated.FacilityType.Should().Be(SlcFacility_Management.Enums.FacilityTypeEnum.Container);
				updated.Address.Should().Be("456 Updated Street");
				updated.City.Should().Be("Los Angeles");
				updated.ZipCode.Should().Be("90001");
				updated.Country.Should().Be("USA");
				updated.Latitude.Should().Be(34.0522);
				updated.Longitude.Should().Be(-118.2437);
			}
		}

		private void AssertCreated()
		{
			using (new AssertionScope())
			{
				Helper.Facilities.Count(new TRUEFilterElement<Facility>()).Should().Be(1);

				var createdFacility = Helper.Facilities.Read(new TRUEFilterElement<Facility>()).First();
				createdFacility.Should().NotBeNull();
				createdFacility.FacilityId.Should().Be("DTC-A");
				createdFacility.Name.Should().Be("Data Center A");
				createdFacility.Description.Should().Be("A datacenter facility for testing.");
				createdFacility.FacilityType.Should().Be(SlcFacility_Management.Enums.FacilityTypeEnum.Building);
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
