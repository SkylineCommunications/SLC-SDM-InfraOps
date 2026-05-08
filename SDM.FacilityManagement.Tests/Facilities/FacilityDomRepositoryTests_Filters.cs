namespace SDM.FacilityManagement.Tests.Facilities
{
    using System;
    using System.Linq;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SDM.FacilityManagement.Tests.Setup;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;

    public partial class FacilityDomRepositoryTests
	{
		[TestMethod]
		public void FacilityDomRepository_ReadFilter_Name_Equals()
		{
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateFacilities();

			string facilityName = "Tokyo Warehouse";
			var nameFilter = FacilityExposers.Name.Equal(facilityName);
			var expected = DemoData.Facilities.Single(facility => facility.Name.Equals(facilityName));

			var facilitiesRetrieved = helper.Facilities.Read(nameFilter);

			using (new AssertionScope())
			{
				facilitiesRetrieved.Should().NotBeNull();
				facilitiesRetrieved.Count().Should().Be(1);
				var facility = facilitiesRetrieved.First();

				facility.Name.Should().Be(expected.Name);
				facility.FacilityId.Should().Be(expected.FacilityId);
				facility.Description.Should().Be(expected.Description);
				facility.FacilityType.Should().Be(expected.FacilityType);
				facility.Address.Should().Be(expected.Address);
				facility.City.Should().Be(expected.City);
				facility.ZipCode.Should().Be(expected.ZipCode);
				facility.Country.Should().Be(expected.Country);
				facility.Latitude.Should().Be(expected.Latitude);
				facility.Longitude.Should().Be(expected.Longitude);
			}
		}

		[TestMethod]
		public void FacilityDomRepository_ReadFilter_Description_Contains()
		{
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateFacilities();

			var descriptionFilter = FacilityExposers.Description.Contains("data center", StringComparison.OrdinalIgnoreCase);
			var expected = DemoData.Facilities.Where(f => f.Description.Contains("data center", StringComparison.OrdinalIgnoreCase)).ToArray();

			var facilitiesRetrieved = helper.Facilities.Read(descriptionFilter);

			using (new AssertionScope())
			{
				facilitiesRetrieved.Should().NotBeNull();
				facilitiesRetrieved.Should().HaveCount(expected.Length);
				facilitiesRetrieved.Should().BeEquivalentTo(expected);
			}
		}

		[TestMethod]
		public void FacilityDomRepository_ReadFilter_Type_NotEquals()
		{
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateFacilities();

			var typeFilter = FacilityExposers.FacilityType.UncheckedNotEqual(SlcFacility_Management.Enums.FacilityTypeEnum.Building);
			var expected = DemoData.Facilities.Where(f => f.FacilityType != SlcFacility_Management.Enums.FacilityTypeEnum.Building).ToArray();

			var facilitiesRetrieved = helper.Facilities.Read(typeFilter);

			using (new AssertionScope())
			{
				facilitiesRetrieved.Should().NotBeNull();
				facilitiesRetrieved.Should().HaveCount(expected.Length);
				facilitiesRetrieved.Should().BeEquivalentTo(expected);
			}
		}

		[TestMethod]
		public void FacilityDomRepository_ReadFilter_City_Equal()
		{
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateFacilities();

			var city = "New York";
			var cityFilter = FacilityExposers.City.Equal(city);

			var facilitiesRetrieved = helper.Facilities.Read(cityFilter);
			var expected = DemoData.Facilities.Where(f => f.City == city).ToArray();

			using (new AssertionScope())
			{
				facilitiesRetrieved.Should().NotBeNull();
				facilitiesRetrieved.Count().Should().Be(expected.Length);
				facilitiesRetrieved.Should().BeEquivalentTo(expected);
			}
		}

		[TestMethod]
		public void FacilityDomRepository_ReadFilter_Country_Equal()
		{
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateFacilities();

			var country = "USA";
			var countryFilter = FacilityExposers.Country.Equal(country);

			var facilitiesRetrieved = helper.Facilities.Read(countryFilter);
			var expected = DemoData.Facilities.Where(f => f.Country == country).ToArray();

			using (new AssertionScope())
			{
				facilitiesRetrieved.Should().NotBeNull();
				facilitiesRetrieved.Count().Should().Be(expected.Length);
				facilitiesRetrieved.Should().BeEquivalentTo(expected);
			}
		}

		[TestMethod]
		public void FacilityDomRepository_ReadFilter_Latitude_GreaterThanOrEqual()
		{
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateFacilities();

			double latitudeThreshold = 40.0;
			var latitudeFilter = FacilityExposers.Latitude.GreaterThanOrEqual(latitudeThreshold);

			var facilitiesRetrieved = helper.Facilities.Read(latitudeFilter);
			var expected = DemoData.Facilities.Where(f => f.Latitude >= latitudeThreshold).ToArray();

			using (new AssertionScope())
			{
				facilitiesRetrieved.Should().NotBeNull();
				facilitiesRetrieved.Count().Should().Be(expected.Length);
				facilitiesRetrieved.Should().BeEquivalentTo(expected);
			}
		}

		[TestMethod]
		public void FacilityDomRepository_ReadFilter_Longitude_LessThanOrEqual()
		{
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateFacilities();

			double longitudeThreshold = -70.0;
			var longitudeFilter = FacilityExposers.Longitude.LessThanOrEqual(longitudeThreshold);

			var facilitiesRetrieved = helper.Facilities.Read(longitudeFilter);
			var expected = DemoData.Facilities.Where(f => f.Longitude <= longitudeThreshold).ToArray();

			using (new AssertionScope())
			{
				facilitiesRetrieved.Should().NotBeNull();
				facilitiesRetrieved.Count().Should().Be(expected.Length);
				facilitiesRetrieved.Should().BeEquivalentTo(expected);
			}
		}

		[TestMethod]
		public void FacilityDomRepository_ReadFilter_CityAndCountry_Equal()
		{
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateFacilities();

			var city = "London";
			var country = "UK";

			var combinedFilter = FacilityExposers.City.Equal(city)
				.AND(FacilityExposers.Country.Equal(country));

			var facilitiesRetrieved = helper.Facilities.Read(combinedFilter);
			var expected = DemoData.Facilities.Where(f => f.City == city && f.Country == country).ToArray();

			using (new AssertionScope())
			{
				facilitiesRetrieved.Should().NotBeNull();
				facilitiesRetrieved.Count().Should().Be(expected.Length);
				facilitiesRetrieved.Should().BeEquivalentTo(expected);
			}
		}

		[TestMethod]
		public void FacilityDomRepository_ReadFilter_LatitudeLongitude_Range()
		{
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateFacilities();

			double minLatitude = 40.0;
			double maxLatitude = 50.0;
			double minLongitude = -80.0;
			double maxLongitude = -70.0;

			var locationFilter = FacilityExposers.Latitude.GreaterThanOrEqual(minLatitude)
				.AND(FacilityExposers.Latitude.LessThanOrEqual(maxLatitude))
				.AND(FacilityExposers.Longitude.GreaterThanOrEqual(minLongitude))
				.AND(FacilityExposers.Longitude.LessThanOrEqual(maxLongitude));

			var facilitiesRetrieved = helper.Facilities.Read(locationFilter);
			var expected = DemoData.Facilities.Where(f =>
				f.Latitude >= minLatitude && f.Latitude <= maxLatitude &&
				f.Longitude >= minLongitude && f.Longitude <= maxLongitude).ToArray();

			using (new AssertionScope())
			{
				facilitiesRetrieved.Should().NotBeNull();
				facilitiesRetrieved.Should().HaveCount(expected.Length);
				facilitiesRetrieved.Should().BeEquivalentTo(expected);
			}
		}

		[TestMethod]
		public void FacilityDomRepository_ReadFilter_ZipCode_NotContains()
		{
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateFacilities();

			var zipCodeFilter = FacilityExposers.ZipCode.NotContains("000");
			var facilitiesRetrieved = helper.Facilities.Read(zipCodeFilter);

			var expected = DemoData.Facilities.Where(zipCodeFilter.getLambda()).ToArray();

			using (new AssertionScope())
			{
				facilitiesRetrieved.Should().NotBeNull();
				facilitiesRetrieved.Count().Should().Be(expected.Length);
				facilitiesRetrieved.Should().BeEquivalentTo(expected);
			}
		}

		[TestMethod]
		public void FacilityDomRepository_ReadFilter_FacilityId_Equal()
		{
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateFacilities();

			var facilityId = DemoData.Facilities[5].FacilityId;
			var filter = FacilityExposers.FacilityId.Equal(facilityId);

			var facilitiesRetrieved = helper.Facilities.Read(filter);
			var expected = DemoData.Facilities.Single(filter.getLambda());

			using (new AssertionScope())
			{
				facilitiesRetrieved.Should().NotBeNull();
				facilitiesRetrieved.Count().Should().Be(1);
				var facility = facilitiesRetrieved.First();

				facility.FacilityId.Should().Be(expected.FacilityId);
				facility.Name.Should().Be(expected.Name);
				facility.Description.Should().Be(expected.Description);
				facility.FacilityType.Should().Be(expected.FacilityType);
				facility.Address.Should().Be(expected.Address);
				facility.City.Should().Be(expected.City);
				facility.ZipCode.Should().Be(expected.ZipCode);
				facility.Country.Should().Be(expected.Country);
				facility.Latitude.Should().Be(expected.Latitude);
				facility.Longitude.Should().Be(expected.Longitude);
			}
		}
	}
}
