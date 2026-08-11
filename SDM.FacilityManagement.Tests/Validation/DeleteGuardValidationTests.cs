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
	public class DeleteGuardValidationTests : BaseRepositoryTest
	{
		[TestMethod]
		public void Site_Delete_WithAssignedFacility_ShouldThrow()
		{
			var site = Helper.Sites.Create(NewSite("SITE-1"));
			Helper.Facilities.Create(NewFacility("FAC-1", site));

			var action = () => Helper.Sites.Delete(site);

			action.Should().Throw<Exception>().WithMessage("*Can't remove site, since it still has facilities assigned to it. Please remove all the facilities assigned to this site before removing it.*");
		}

		[TestMethod]
		public void Site_Delete_WithoutAssignedFacilities_ShouldSucceed()
		{
			var site = Helper.Sites.Create(NewSite("SITE-1"));

			Action action = () => Helper.Sites.Delete(site);

			action.Should().NotThrow();
		}

		[TestMethod]
		public void Facility_Delete_WithAssignedFloor_ShouldThrow()
		{
			var facility = Helper.Facilities.Create(NewFacility("FAC-1"));
			Helper.Floors.Create(NewFloor("FLR-1", facility));

			var action = () => Helper.Facilities.Delete(facility);

			action.Should().Throw<Exception>().WithMessage("*Can't remove facility, since it still has floors assigned to it. Please remove all the floors assigned to this facility before removing it.*");
		}

		[TestMethod]
		public void Facility_Delete_WithoutAssignedFloorsOrAssets_ShouldSucceed()
		{
			var facility = Helper.Facilities.Create(NewFacility("FAC-1"));

			Action action = () => Helper.Facilities.Delete(facility);

			action.Should().NotThrow();
		}

		[TestMethod]
		public void Floor_Delete_WithAssignedRoom_ShouldThrow()
		{
			var floor = Helper.Floors.Create(NewFloor("FLR-1"));
			Helper.Rooms.Create(NewRoom("ROOM-1", floor));

			var action = () => Helper.Floors.Delete(floor);

			action.Should().Throw<Exception>().WithMessage("*Can't remove floor, since it still has rooms assigned to it. Please remove all the rooms assigned to this floor before removing it.*");
		}

		[TestMethod]
		public void Floor_Delete_WithoutAssignedRooms_ShouldSucceed()
		{
			var floor = Helper.Floors.Create(NewFloor("FLR-1"));

			Action action = () => Helper.Floors.Delete(floor);

			action.Should().NotThrow();
		}

		[TestMethod]
		public void Room_Delete_WithAssignedRow_ShouldThrow()
		{
			var room = Helper.Rooms.Create(NewRoom("ROOM-1"));
			Helper.Rows.Create(NewRow("ROW-1", room));

			var action = () => Helper.Rooms.Delete(room);

			action.Should().Throw<Exception>().WithMessage("*Can't remove room, since it still has rows assigned to it. Please remove all the rows assigned to this room before removing it.*");
		}

		[TestMethod]
		public void Room_Delete_WithAssignedZone_ShouldThrow()
		{
			var room = Helper.Rooms.Create(NewRoom("ROOM-1"));
			Helper.Zones.Create(NewZone("ZONE-1", room));

			var action = () => Helper.Rooms.Delete(room);

			action.Should().Throw<Exception>().WithMessage("*Can't remove room, since it still has zones assigned to it. Please remove all the zones assigned to this room before removing it.*");
		}

		[TestMethod]
		public void Room_Delete_WithAssignedDesk_ShouldThrow()
		{
			var room = Helper.Rooms.Create(NewRoom("ROOM-1"));
			Helper.Desks.Create(NewDesk("DSK-1", room));

			var action = () => Helper.Rooms.Delete(room);

			action.Should().Throw<Exception>().WithMessage("*Can't remove room, since it still has desks assigned to it. Please remove all the desks assigned to this room before removing it.*");
		}

		[TestMethod]
		public void Room_Delete_WithoutAssignedRowsZonesDesksOrAssets_ShouldSucceed()
		{
			var room = Helper.Rooms.Create(NewRoom("ROOM-1"));

			Action action = () => Helper.Rooms.Delete(room);

			action.Should().NotThrow();
		}

		[TestMethod]
		public void Row_Delete_WithAssignedRack_ShouldThrow()
		{
			var row = Helper.Rows.Create(NewRow("ROW-1"));
			Helper.Racks.Create(NewRack("RACK-1", row: row));

			var action = () => Helper.Rows.Delete(row);

			action.Should().Throw<Exception>().WithMessage("*Can't remove row, since it still has racks assigned to it. Please remove all the racks assigned to this row before removing it.*");
		}

		[TestMethod]
		public void Row_Delete_WithoutAssignedRacks_ShouldSucceed()
		{
			var row = Helper.Rows.Create(NewRow("ROW-1"));

			Action action = () => Helper.Rows.Delete(row);

			action.Should().NotThrow();
		}

		[TestMethod]
		public void Zone_Delete_WithAssignedRack_ShouldThrow()
		{
			var zone = Helper.Zones.Create(NewZone("ZONE-1"));
			Helper.Racks.Create(NewRack("RACK-1", zone: zone));

			var action = () => Helper.Zones.Delete(zone);

			action.Should().Throw<Exception>().WithMessage("*Can't remove zone, since it still has racks assigned to it. Please remove all the racks assigned to this zone before removing it.*");
		}

		[TestMethod]
		public void Zone_Delete_WithoutAssignedRacks_ShouldSucceed()
		{
			var zone = Helper.Zones.Create(NewZone("ZONE-1"));

			Action action = () => Helper.Zones.Delete(zone);

			action.Should().NotThrow();
		}

		[TestMethod]
		public void Desk_Delete_WithoutAssignedAssets_ShouldSucceed()
		{
			var desk = Helper.Desks.Create(NewDesk("DSK-1"));

			Action action = () => Helper.Desks.Delete(desk);

			action.Should().NotThrow();
		}

		[TestMethod]
		public void Rack_Delete_WithoutAssignedAssets_ShouldSucceed()
		{
			var rack = Helper.Racks.Create(NewRack("RACK-1"));

			Action action = () => Helper.Racks.Delete(rack);

			action.Should().NotThrow();
		}

		private static Site NewSite(string id)
		{
			return new Site { Identifier = Guid.NewGuid().ToString(), SiteId = id };
		}

		private static Facility NewFacility(string id, Site? site = null)
		{
			return new Facility
			{
				Identifier = Guid.NewGuid().ToString(),
				FacilityId = id,
				SiteFk = site == null ? null : new SiteRelation { Site = new SdmObjectReference<Site>(site.Identifier) },
			};
		}

		private static Floor NewFloor(string id, Facility? facility = null)
		{
			return new Floor
			{
				Identifier = Guid.NewGuid().ToString(),
				FloorId = id,
				FacilityFk = facility == null ? null : new FacilityRelation { Facility = new SdmObjectReference<Facility>(facility.Identifier) },
			};
		}

		private static Room NewRoom(string id, Floor? floor = null)
		{
			return new Room
			{
				Identifier = Guid.NewGuid().ToString(),
				RoomId = id,
				FloorFk = floor == null ? null : new FloorRelation { Floor = new SdmObjectReference<Floor>(floor.Identifier) },
			};
		}

		private static Row NewRow(string id, Room? room = null)
		{
			return new Row
			{
				Identifier = Guid.NewGuid().ToString(),
				RowId = id,
				RoomFk = room == null ? null : new RoomRelation { Room = new SdmObjectReference<Room>(room.Identifier) },
			};
		}

		private static Zone NewZone(string id, Room? room = null)
		{
			return new Zone
			{
				Identifier = Guid.NewGuid().ToString(),
				ZoneId = id,
				RoomFk = room == null ? null : new RoomRelation { Room = new SdmObjectReference<Room>(room.Identifier) },
			};
		}

		private static Desk NewDesk(string id, Room? room = null)
		{
			return new Desk
			{
				Identifier = Guid.NewGuid().ToString(),
				DeskID = id,
				RoomFk = room == null ? null : new RoomRelation { Room = new SdmObjectReference<Room>(room.Identifier) },
			};
		}

		private static Rack NewRack(string id, Row? row = null, Zone? zone = null)
		{
			return new Rack
			{
				Identifier = Guid.NewGuid().ToString(),
				RackId = id,
				Capacity = new RackCapacity { MaximumRackCapacity = 42 },
				RowFk = row == null ? null : new RowRelation { Row = new SdmObjectReference<Row>(row.Identifier) },
				ZoneFk = zone == null ? null : new ZoneRelation { Zone = new SdmObjectReference<Zone>(zone.Identifier) },
			};
		}
	}
}
