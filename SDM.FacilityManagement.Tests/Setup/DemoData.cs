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
		public static readonly List<Rack> Racks =
		[
			CreateRack(
				new Rack
			{
				Identifier = Guid.NewGuid().ToString(),
				Name = "Alpha Server Rack",
				Model = "APC NetShelter SX",
				Position = SlcFacility_Management.Enums.RackpositionenumEnum.Bottom,
				Width = 60.0,
				Height = 42.0,
				Depth = 107.0,
				Description = "Primary server rack in row A",
				Bookable = true,
				CoolingFlow = SlcFacility_Management.Enums.CoolingflowenumEnum.FrontToRear,
				Label = "A01",
				Orientation = SlcFacility_Management.Enums.Placementorientationenum.Vertical,
				RackId = "RCK-001",
			}, 42),
			CreateRack(
				new Rack
			{
				Identifier = Guid.NewGuid().ToString(),
				Name = "Beta Network Rack",
				Model = "APC NetShelter SX",
				Position = SlcFacility_Management.Enums.RackpositionenumEnum.Top,
				Width = 60.0,
				Height = 24.0,
				Depth = 107.0,
				Description = "Network equipment rack in row A",
				Bookable = false,
				CoolingFlow = SlcFacility_Management.Enums.CoolingflowenumEnum.RearToFront,
				Label = "A02",
				Orientation = SlcFacility_Management.Enums.Placementorientationenum.Vertical,
				RackId = "RCK-002",
			}, 24),
			CreateRack(
				new Rack
			{
				Identifier = Guid.NewGuid().ToString(),
				Name = "Core Switch Enclosure",
				Model = "Schneider Electric",
				Position = SlcFacility_Management.Enums.RackpositionenumEnum.Bottom,
				Width = 80.0,
				Height = 42.0,
				Depth = 120.0,
				Description = "Core switching equipment in row B",
				Bookable = true,
				CoolingFlow = SlcFacility_Management.Enums.CoolingflowenumEnum.FrontToRear,
				Label = "B01",
				Orientation = SlcFacility_Management.Enums.Placementorientationenum.Vertical,
				RackId = "RCK-003",
			}, 42),
			CreateRack(
				new Rack
			{
				Identifier = Guid.NewGuid().ToString(),
				Name = "Edge Compute Rack",
				Model = "Schneider Electric",
				Position = SlcFacility_Management.Enums.RackpositionenumEnum.Top,
				Width = 80.0,
				Height = 36.0,
				Depth = 100.0,
				Description = "Edge computing nodes in row B",
				Bookable = false,
				CoolingFlow = SlcFacility_Management.Enums.CoolingflowenumEnum.SideToSide,
				Label = "B02",
				Orientation = SlcFacility_Management.Enums.Placementorientationenum.Horizontal,
				RackId = "RCK-004",
			}, 36),
			CreateRack(
				new Rack
			{
				Identifier = Guid.NewGuid().ToString(),
				Name = "Alpha Patch Panel",
				Model = "Rittal TS IT",
				Position = SlcFacility_Management.Enums.RackpositionenumEnum.Bottom,
				Width = 60.0,
				Height = 12.0,
				Depth = 80.0,
				Description = "Cable management and patch panel rack",
				Bookable = true,
				CoolingFlow = SlcFacility_Management.Enums.CoolingflowenumEnum.FrontToRear,
				Label = "C01",
				Orientation = SlcFacility_Management.Enums.Placementorientationenum.Vertical,
				RackId = "RCK-005",
			}, 12),
			CreateRack(
				new Rack
			{
				Identifier = Guid.NewGuid().ToString(),
				Name = "Storage Array Cabinet",
				Model = "Rittal TS IT",
				Position = SlcFacility_Management.Enums.RackpositionenumEnum.Bottom,
				Width = 100.0,
				Height = 48.0,
				Depth = 120.0,
				Description = "High-density storage array enclosure",
				Bookable = false,
				CoolingFlow = SlcFacility_Management.Enums.CoolingflowenumEnum.BottomToTop,
				Label = "C02",
				Orientation = SlcFacility_Management.Enums.Placementorientationenum.Vertical,
				RackId = "RCK-006",
			}, 48),
		];

		private static Rack CreateRack(Rack rack, double maximumRackCapacity)
		{
			rack.Capacity.MaximumRackCapacity = maximumRackCapacity;
			return rack;
		}

		public static readonly List<Desk> Desks =
		[
			new Desk
			{
				Identifier = Guid.NewGuid().ToString(),
				DeskID = "DSK-001",
				Name = "Desk A01",
				Description = "Shared workstation near the entrance",
				Plan = "Level-1-A01",
			},
			new Desk
			{
				Identifier = Guid.NewGuid().ToString(),
				DeskID = "DSK-002",
				Name = "Desk A02",
				Description = "Corner desk with dual monitors",
				Plan = "Level-1-A02",
			},
			new Desk
			{
				Identifier = Guid.NewGuid().ToString(),
				DeskID = "DSK-003",
				Name = "Desk B01",
				Description = "Desk near the operations wall",
				Plan = "Level-1-B01",
			},
			new Desk
			{
				Identifier = Guid.NewGuid().ToString(),
				DeskID = "DSK-004",
				Name = "Desk B02",
				Description = "Standing desk in the collaboration area",
				Plan = "Level-1-B02",
			},
			new Desk
			{
				Identifier = Guid.NewGuid().ToString(),
				DeskID = "DSK-005",
				Name = "Desk C01",
				Description = "Quiet zone focus desk",
				Plan = "Level-2-C01",
			},
			new Desk
			{
				Identifier = Guid.NewGuid().ToString(),
				DeskID = "DSK-006",
				Name = "Desk C02",
				Description = "Hot desk beside meeting room Delta",
				Plan = "Level-2-C02",
			},
		];

		public static readonly List<FacilityManagerAppSettings> FacilityManagerAppSettings =
		[
			new FacilityManagerAppSettings
			{
				Identifier = Guid.NewGuid().ToString(),
				GoogleMapsAPIKey = "key1",
			},
			new FacilityManagerAppSettings
			{
				Identifier = Guid.NewGuid().ToString(),
				GoogleMapsAPIKey = "key2",
			},
			new FacilityManagerAppSettings
			{
				Identifier = Guid.NewGuid().ToString(),
				GoogleMapsAPIKey = null,
			},
		];

		public static readonly List<Room> Rooms =
		[
			new Room
			{
				Identifier = Guid.NewGuid().ToString(),
				Name = "Main Server Room",
				Plan = "G-Floor-A",
				Description = "Primary data center floor with raised flooring",
				Width = 1500,
				Depth = 2000,
				RoomId = "RM-001",
			},
			new Room
			{
				Identifier = Guid.NewGuid().ToString(),
				Name = "Network Operations Center",
				Plan = "G-Floor-B",
				Description = "NOC room with monitoring stations",
				Width = 900,
				Depth = 1200,
				RoomId = "RM-002",
			},
			new Room
			{
				Identifier = Guid.NewGuid().ToString(),
				Name = "Main Storage Area",
				Plan = "Level-1-A",
				Description = "Cold storage for media equipment",
				Width = 600,
				Depth = 900,
				RoomId = "RM-003",
			},
			new Room
			{
				Identifier = Guid.NewGuid().ToString(),
				Name = "Battery Backup Room",
				Plan = "B1-Floor-A",
				Description = "UPS and battery systems",
				Width = 400,
				Depth = 600,
				RoomId = "RM-004",
			},
			new Room
			{
				Identifier = Guid.NewGuid().ToString(),
				Name = "Cooling Equipment Room",
				Plan = "B1-Floor-B",
				Description = "Precision cooling and HVAC units",
				Width = 800,
				Depth = 1000,
				RoomId = "RM-005",
			},
			new Room
			{
				Identifier = Guid.NewGuid().ToString(),
				Name = "Patch and Distribution",
				Plan = "Level-1-B",
				Description = "Cable patching and distribution infrastructure",
				Width = 300,
				Depth = 500,
				RoomId = "RM-006",
			},
		];

		public static readonly List<Floor> Floors =
		[
			new Floor { Identifier = Guid.NewGuid().ToString(), Name = "Ground Floor",  Plan = "GF-Plan", Description = "Main lobby with reception",     FloorId = "FL-001" },
			new Floor { Identifier = Guid.NewGuid().ToString(), Name = "First Floor",   Plan = "L1-Plan", Description = "Open plan offices",             FloorId = "FL-002" },
			new Floor { Identifier = Guid.NewGuid().ToString(), Name = "Second Floor",  Plan = "L2-Plan", Description = "Executive suites",              FloorId = "FL-003" },
			new Floor { Identifier = Guid.NewGuid().ToString(), Name = "Basement Level", Plan = "B1-Plan", Description = "Parking and storage",          FloorId = "FL-004" },
			new Floor { Identifier = Guid.NewGuid().ToString(), Name = "Rooftop Level", Plan = "RF-Plan", Description = "HVAC and antenna equipment",    FloorId = "FL-005" },
			new Floor { Identifier = Guid.NewGuid().ToString(), Name = "Technical Floor", Plan = "TF-Plan", Description = "Server and network equipment", FloorId = "FL-006" },
		];

		public static readonly List<Zone> Zones =
		[
			new Zone { Identifier = Guid.NewGuid().ToString(), Name = "Cold Aisle A",    Plan = "Z-A", Description = "Cold aisle containment zone",   ThermalType = SlcFacility_Management.Enums.ThermalType.Cold, XPosition = 0.0,  YPosition = 0.0,  Width = 5.0,  Depth = 10.0, ZoneId = "ZN-001", ZoneCapacity = { CoolingCapacity = 5.0 } },
			new Zone { Identifier = Guid.NewGuid().ToString(), Name = "Hot Aisle A",     Plan = "Z-A", Description = "Hot aisle exhaust zone",        ThermalType = SlcFacility_Management.Enums.ThermalType.Warm, XPosition = 5.0,  YPosition = 0.0,  Width = 5.0,  Depth = 10.0, ZoneId = "ZN-002", ZoneCapacity = { CoolingCapacity = 0.0 } },
			new Zone { Identifier = Guid.NewGuid().ToString(), Name = "Cold Aisle B",    Plan = "Z-B", Description = "Secondary cold aisle",          ThermalType = SlcFacility_Management.Enums.ThermalType.Cold, XPosition = 0.0,  YPosition = 10.0, Width = 6.0,  Depth = 12.0, ZoneId = "ZN-003", ZoneCapacity = { CoolingCapacity = 6.0 } },
			new Zone { Identifier = Guid.NewGuid().ToString(), Name = "Neutral Staging", Plan = "Z-C", Description = "Staging and unboxing area",     ThermalType = SlcFacility_Management.Enums.ThermalType.None, XPosition = 10.0, YPosition = 0.0,  Width = 8.0,  Depth = 8.0,  ZoneId = "ZN-004", ZoneCapacity = { CoolingCapacity = 0.0 } },
			new Zone { Identifier = Guid.NewGuid().ToString(), Name = "Hot Aisle B",     Plan = "Z-B", Description = "Secondary hot aisle exhaust",   ThermalType = SlcFacility_Management.Enums.ThermalType.Warm, XPosition = 5.0,  YPosition = 10.0, Width = 6.0,  Depth = 12.0, ZoneId = "ZN-005", ZoneCapacity = { CoolingCapacity = 0.0 } },
			new Zone { Identifier = Guid.NewGuid().ToString(), Name = "Cooling Plant",   Plan = "Z-D", Description = "Chiller and cooling plant zone", ThermalType = SlcFacility_Management.Enums.ThermalType.Cold, XPosition = 20.0, YPosition = 0.0,  Width = 10.0, Depth = 15.0, ZoneId = "ZN-006", ZoneCapacity = { CoolingCapacity = 15.0 } },
		];

		public static readonly List<Row> Rows =
		[
			new Row { Identifier = Guid.NewGuid().ToString(), Name = "Row A", Plan = "R-1", Description = "First equipment row",       Label = "A", RowId = "RW-001", YPosition = 0.0 },
			new Row { Identifier = Guid.NewGuid().ToString(), Name = "Row B", Plan = "R-1", Description = "Second equipment row",      Label = "B", RowId = "RW-002", YPosition = 2.0 },
			new Row { Identifier = Guid.NewGuid().ToString(), Name = "Row C", Plan = "R-2", Description = "Third equipment row",       Label = "C", RowId = "RW-003", YPosition = 4.0 },
			new Row { Identifier = Guid.NewGuid().ToString(), Name = "Row D", Plan = "R-2", Description = "Storage row",               Label = "D", RowId = "RW-004", YPosition = 6.0 },
			new Row { Identifier = Guid.NewGuid().ToString(), Name = "Row E", Plan = "R-3", Description = "Network distribution row",  Label = "E", RowId = "RW-005", YPosition = 8.0 },
			new Row { Identifier = Guid.NewGuid().ToString(), Name = "Row F", Plan = "R-3", Description = "Spare capacity row",        Label = "F", RowId = "RW-006", YPosition = 10.0 },
		];

		public static readonly List<Site> Sites =
		[
			new Site { Identifier = Guid.NewGuid().ToString(), Name = "New York Campus", Description = "East coast HQ",   Address = "123 Broadway",         City = "New York", ZipCode = "10001",   Country = "USA",            Latitude = 40.7128,  Longitude = -74.0060, SiteId = "ST-001" },
			new Site { Identifier = Guid.NewGuid().ToString(), Name = "London Campus",   Description = "European HQ",    Address = "456 Oxford Street",    City = "London",   ZipCode = "W1D 1BS", Country = "United Kingdom", Latitude = 51.5074,  Longitude = -0.1278,  SiteId = "ST-002" },
			new Site { Identifier = Guid.NewGuid().ToString(), Name = "Tokyo Campus",    Description = "APAC HQ",        Address = "789 Shibuya",          City = "Tokyo",    ZipCode = "150-0002", Country = "Japan",         Latitude = 35.6762,  Longitude = 139.6503, SiteId = "ST-003" },
			new Site { Identifier = Guid.NewGuid().ToString(), Name = "Berlin Lab",      Description = "R&D site",       Address = "555 Unter den Linden", City = "Berlin",   ZipCode = "10117",   Country = "Germany",        Latitude = 52.5200,  Longitude = 13.4050,  SiteId = "ST-004" },
			new Site { Identifier = Guid.NewGuid().ToString(), Name = "Austin Office",   Description = "US south office", Address = "321 Congress Ave",    City = "Austin",   ZipCode = "73301",   Country = "USA",            Latitude = 30.2672,  Longitude = -97.7431, SiteId = "ST-005" },
			new Site { Identifier = Guid.NewGuid().ToString(), Name = "Sydney Office",   Description = "APAC office",    Address = "888 George St",        City = "Sydney",   ZipCode = "2000",    Country = "Australia",      Latitude = -33.8688, Longitude = 151.2093, SiteId = "ST-006" },
		];
	}
}