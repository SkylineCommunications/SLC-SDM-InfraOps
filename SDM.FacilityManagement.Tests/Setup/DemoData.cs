namespace SDM.FacilityManagement.Tests.Setup
{
    using System.Collections.Generic;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM.FacilityManagement.Models;

    public class DemoData
	{
		public static readonly List<Facility> Facilities =
		[
			new Facility
			{
				Identifier = Guid.NewGuid().ToString(),
				FacilityId = "FAC-001",
				Name = "New York OB 1",
				Description = "OB Truck 1 New York",
				FacilityType = SlcFacility_Management.Enums.FacilityTypeEnum.Truck,
				Address = "123 Broadway Avenue",
				City = "New York",
				ZipCode = "10001",
				Country = "USA",
				Latitude = 40.7128,
				Longitude = -74.0060,
			},
			new Facility
			{
				Identifier = Guid.NewGuid().ToString(),
				FacilityId = "FAC-002",
				Name = "London Office",
				Description = "Corporate headquarters in London",
				FacilityType = SlcFacility_Management.Enums.FacilityTypeEnum.Building,
				Address = "456 Oxford Street",
				City = "London",
				ZipCode = "W1D 1BS",
				Country = "United Kingdom",
				Latitude = 51.5074,
				Longitude = -0.1278,
			},
			new Facility
			{
				Identifier = Guid.NewGuid().ToString(),
				FacilityId = "FAC-003",
				Name = "Tokyo Warehouse",
				Description = "Main distribution warehouse in Tokyo",
				FacilityType = SlcFacility_Management.Enums.FacilityTypeEnum.Building,
				Address = "789 Shibuya Crossing",
				City = "Tokyo",
				ZipCode = "150-0002",
				Country = "Japan",
				Latitude = 35.6762,
				Longitude = 139.6503,
			},
			new Facility
			{
				Identifier = Guid.NewGuid().ToString(),
				FacilityId = "FAC-004",
				Name = "Sydney Manufacturing Plant",
				Description = "Production facility in Sydney",
				FacilityType = SlcFacility_Management.Enums.FacilityTypeEnum.Building,
				Address = "321 Harbor Boulevard",
				City = "Sydney",
				ZipCode = "2000",
				Country = "Australia",
				Latitude = -33.8688,
				Longitude = 151.2093,
			},
			new Facility
			{
				Identifier = Guid.NewGuid().ToString(),
				FacilityId = "FAC-005",
				Name = "Berlin Research Lab",
				Description = "Research and development laboratory",
				FacilityType = SlcFacility_Management.Enums.FacilityTypeEnum.Container,
				Address = "555 Unter den Linden",
				City = "Berlin",
				ZipCode = "10117",
				Country = "Germany",
				Latitude = 52.5200,
				Longitude = 13.4050,
			},
			new Facility
			{
				Identifier = Guid.NewGuid().ToString(),
				FacilityId = "FAC-006",
				Name = "Toronto Retail Store",
				Description = "Flagship retail location in Toronto",
				FacilityType = SlcFacility_Management.Enums.FacilityTypeEnum.Building,
				Address = "888 Yonge Street",
				City = "Toronto",
				ZipCode = "M4W 2H1",
				Country = "Canada",
				Latitude = 43.6532,
				Longitude = -79.3832,
			},
			new Facility
			{
				Identifier = Guid.NewGuid().ToString(),
				FacilityId = "FAC-007",
				Name = "Singapore Backup Data Center",
				Description = "Secondary data center for disaster recovery",
				FacilityType = SlcFacility_Management.Enums.FacilityTypeEnum.Building,
				Address = "101 Marina Bay Sands",
				City = "Singapore",
				ZipCode = "018956",
				Country = "Singapore",
				Latitude = 1.3521,
				Longitude = 103.8198,
			},
			new Facility
			{
				Identifier = Guid.NewGuid().ToString(),
				FacilityId = "FAC-008",
				Name = "Paris Regional Office",
				Description = "European regional office",
				FacilityType = SlcFacility_Management.Enums.FacilityTypeEnum.Building,
				Address = "777 Champs-Élysées",
				City = "Paris",
				ZipCode = "75008",
				Country = "France",
				Latitude = 48.8566,
				Longitude = 2.3522,
			},
			new Facility
			{
				Identifier = Guid.NewGuid().ToString(),
				FacilityId = "FAC-009",
				Name = "Dubai Logistics Hub",
				Description = "Central logistics and distribution hub",
				FacilityType = SlcFacility_Management.Enums.FacilityTypeEnum.Container,
				Address = "999 Sheikh Zayed Road",
				City = "Dubai",
				ZipCode = "00000",
				Country = "United Arab Emirates",
				Latitude = 25.2048,
				Longitude = 55.2708,
			},
			new Facility
			{
				Identifier = Guid.NewGuid().ToString(),
				FacilityId = "FAC-010",
				Name = "São Paulo Assembly Plant",
				Description = "Assembly and quality control facility",
				FacilityType = SlcFacility_Management.Enums.FacilityTypeEnum.Building,
				Address = "246 Avenida Paulista",
				City = "São Paulo",
				ZipCode = "01310-100",
				Country = "Brazil",
				Latitude = -23.5505,
				Longitude = -46.6333,
			},
		];
	}
}