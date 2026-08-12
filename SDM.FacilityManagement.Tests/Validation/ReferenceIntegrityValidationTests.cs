namespace SDM.FacilityManagement.Tests.Validation
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using FluentAssertions;

	using SDM.FacilityManagement.Tests.Setup;

	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.SDM.FacilityManagement.Helpers;
	using Skyline.DataMiner.SDM.FacilityManagement.Models;

	[TestClass]
	public class ReferenceIntegrityValidationTests : BaseRepositoryTest
	{
		[TestMethod]
		public void Facility_Create_WithDanglingSiteReference_ShouldThrow()
		{
			var missingSiteId = Guid.NewGuid().ToString();
			var facility = NewFacility("FAC-1");
			facility.SiteFk.Site = new SdmObjectReference<Site>(missingSiteId);

			var action = () => Helper.Facilities.Create(facility);

			action.Should().Throw<Exception>().WithMessage($"*Referenced Site '{missingSiteId}' does not exist.*");
		}

		[TestMethod]
		public void Facility_Create_WithExistingSiteReference_ShouldSucceed()
		{
			var site = Helper.Sites.Create(NewSite("SITE-1"));
			var facility = NewFacility("FAC-1");
			facility.SiteFk.Site = new SdmObjectReference<Site>(site.Identifier);

			Action action = () => Helper.Facilities.Create(facility);

			action.Should().NotThrow();
		}

		[TestMethod]
		public void Floor_Create_WithDanglingFacilityReference_ShouldThrow()
		{
			var missingFacilityId = Guid.NewGuid().ToString();
			var floor = NewFloor("FLR-1");
			floor.FacilityFk.Facility = new SdmObjectReference<Facility>(missingFacilityId);

			var action = () => Helper.Floors.Create(floor);

			action.Should().Throw<Exception>().WithMessage($"*Referenced Facility '{missingFacilityId}' does not exist.*");
		}

		[TestMethod]
		public void Floor_Create_WithExistingFacilityReference_ShouldSucceed()
		{
			var facility = Helper.Facilities.Create(NewFacility("FAC-1"));
			var floor = NewFloor("FLR-1");
			floor.FacilityFk.Facility = new SdmObjectReference<Facility>(facility.Identifier);

			Action action = () => Helper.Floors.Create(floor);

			action.Should().NotThrow();
		}

		[TestMethod]
		public void Room_Create_WithDanglingFloorReference_ShouldThrow()
		{
			var missingFloorId = Guid.NewGuid().ToString();
			var room = NewRoom("ROOM-1");
			room.FloorFk.Floor = new SdmObjectReference<Floor>(missingFloorId);

			var action = () => Helper.Rooms.Create(room);

			action.Should().Throw<Exception>().WithMessage($"*Referenced Floor '{missingFloorId}' does not exist.*");
		}

		[TestMethod]
		public void Room_Create_WithExistingFloorReference_ShouldSucceed()
		{
			var floor = Helper.Floors.Create(NewFloor("FLR-1"));
			var room = NewRoom("ROOM-1");
			room.FloorFk.Floor = new SdmObjectReference<Floor>(floor.Identifier);

			Action action = () => Helper.Rooms.Create(room);

			action.Should().NotThrow();
		}

		[TestMethod]
		public void Room_Create_WithExternalReferencesAndNullChecker_ShouldSucceed()
		{
			var room = NewRoom("ROOM-1");
			room.Ownership.Owner = Guid.NewGuid();
			room.Ownership.Team = Guid.NewGuid();
			room.ResourceLink.ResourceId = Guid.NewGuid();

			Action action = () => Helper.Rooms.Create(room);

			action.Should().NotThrow();
		}

		[TestMethod]
		public void Row_Create_WithDanglingRoomReference_ShouldThrow()
		{
			var missingRoomId = Guid.NewGuid().ToString();
			var row = NewRow("ROW-1");
			row.RoomFk.Room = new SdmObjectReference<Room>(missingRoomId);

			var action = () => Helper.Rows.Create(row);

			action.Should().Throw<Exception>().WithMessage($"*Referenced Room '{missingRoomId}' does not exist.*");
		}

		[TestMethod]
		public void Row_Create_WithExistingRoomReference_ShouldSucceed()
		{
			var room = Helper.Rooms.Create(NewRoom("ROOM-1"));
			var row = NewRow("ROW-1");
			row.RoomFk.Room = new SdmObjectReference<Room>(room.Identifier);

			Action action = () => Helper.Rows.Create(row);

			action.Should().NotThrow();
		}

		[TestMethod]
		public void Zone_Create_WithDanglingRoomReference_ShouldThrow()
		{
			var missingRoomId = Guid.NewGuid().ToString();
			var zone = NewZone("ZONE-1");
			zone.RoomFk.Room = new SdmObjectReference<Room>(missingRoomId);

			var action = () => Helper.Zones.Create(zone);

			action.Should().Throw<Exception>().WithMessage($"*Referenced Room '{missingRoomId}' does not exist.*");
		}

		[TestMethod]
		public void Zone_Create_WithExistingRoomReference_ShouldSucceed()
		{
			var room = Helper.Rooms.Create(NewRoom("ROOM-1"));
			var zone = NewZone("ZONE-1");
			zone.RoomFk.Room = new SdmObjectReference<Room>(room.Identifier);

			Action action = () => Helper.Zones.Create(zone);

			action.Should().NotThrow();
		}

		[TestMethod]
		public void Desk_Create_WithDanglingRoomReference_ShouldThrow()
		{
			var missingRoomId = Guid.NewGuid().ToString();
			var desk = NewDesk("DSK-1");
			desk.RoomFk.Room = new SdmObjectReference<Room>(missingRoomId);

			var action = () => Helper.Desks.Create(desk);

			action.Should().Throw<Exception>().WithMessage($"*Referenced Room '{missingRoomId}' does not exist.*");
		}

		[TestMethod]
		public void Desk_Create_WithExistingRoomReference_ShouldSucceed()
		{
			var room = Helper.Rooms.Create(NewRoom("ROOM-1"));
			var desk = NewDesk("DSK-1");
			desk.RoomFk.Room = new SdmObjectReference<Room>(room.Identifier);

			Action action = () => Helper.Desks.Create(desk);

			action.Should().NotThrow();
		}

		[TestMethod]
		public void Rack_Create_WithDanglingRowReference_ShouldThrow()
		{
			var missingRowId = Guid.NewGuid().ToString();
			var rack = NewRack("RACK-1");
			rack.RowFk.Row = new SdmObjectReference<Row>(missingRowId);

			var action = () => Helper.Racks.Create(rack);

			action.Should().Throw<Exception>().WithMessage($"*Referenced Row '{missingRowId}' does not exist.*");
		}

		[TestMethod]
		public void Rack_Create_WithExistingRowReference_ShouldSucceed()
		{
			var row = Helper.Rows.Create(NewRow("ROW-1"));
			var rack = NewRack("RACK-1");
			rack.RowFk.Row = new SdmObjectReference<Row>(row.Identifier);

			Action action = () => Helper.Racks.Create(rack);

			action.Should().NotThrow();
		}

		[TestMethod]
		public void Rack_Create_WithDanglingZoneReference_ShouldThrow()
		{
			var missingZoneId = Guid.NewGuid().ToString();
			var rack = NewRack("RACK-1");
			rack.ZoneFk.Zone = new SdmObjectReference<Zone>(missingZoneId);

			var action = () => Helper.Racks.Create(rack);

			action.Should().Throw<Exception>().WithMessage($"*Referenced Zone '{missingZoneId}' does not exist.*");
		}

		[TestMethod]
		public void Rack_Create_WithExistingZoneReference_ShouldSucceed()
		{
			var zone = Helper.Zones.Create(NewZone("ZONE-1"));
			var rack = NewRack("RACK-1");
			rack.ZoneFk.Zone = new SdmObjectReference<Zone>(zone.Identifier);

			Action action = () => Helper.Racks.Create(rack);

			action.Should().NotThrow();
		}

		private static Site NewSite(string id)
		{
			return new Site { Identifier = Guid.NewGuid().ToString(), SiteId = id };
		}

		private static Facility NewFacility(string id)
		{
			return new Facility { Identifier = Guid.NewGuid().ToString(), FacilityId = id };
		}

		private static Floor NewFloor(string id)
		{
			return new Floor { Identifier = Guid.NewGuid().ToString(), FloorId = id };
		}

		private static Room NewRoom(string id)
		{
			return new Room { Identifier = Guid.NewGuid().ToString(), RoomId = id };
		}

		private static Row NewRow(string id)
		{
			return new Row { Identifier = Guid.NewGuid().ToString(), RowId = id };
		}

		private static Zone NewZone(string id)
		{
			return new Zone { Identifier = Guid.NewGuid().ToString(), ZoneId = id };
		}

		private static Desk NewDesk(string id)
		{
			return new Desk { Identifier = Guid.NewGuid().ToString(), DeskID = id };
		}

		private static Rack NewRack(string id)
		{
			var rack = new Rack
			{
				Identifier = Guid.NewGuid().ToString(),
				RackId = id,
			};

			rack.Capacity.MaximumRackCapacity = 42;
			return rack;
		}
	}
}
