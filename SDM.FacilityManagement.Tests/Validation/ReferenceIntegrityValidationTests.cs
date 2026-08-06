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
	using Skyline.DataMiner.SDM.FacilityManagement.Validation;

	[TestClass]
	public class ReferenceIntegrityValidationTests : BaseRepositoryTest
	{
		[TestMethod]
		public void Facility_Create_WithDanglingSiteReference_ShouldThrow()
		{
			var missingSiteId = Guid.NewGuid().ToString();
			var facility = NewFacility("FAC-1");
			facility.SiteFk = new SiteRelation { Site = new SdmObjectReference<Site>(missingSiteId) };

			var action = () => Helper.Facilities.Create(facility);

			action.Should().Throw<Exception>().WithMessage($"*Referenced Site '{missingSiteId}' does not exist.*");
		}

		[TestMethod]
		public void Facility_Create_WithExistingSiteReference_ShouldSucceed()
		{
			var site = Helper.Sites.Create(NewSite("SITE-1"));
			var facility = NewFacility("FAC-1");
			facility.SiteFk = new SiteRelation { Site = new SdmObjectReference<Site>(site.Identifier) };

			Action action = () => Helper.Facilities.Create(facility);

			action.Should().NotThrow();
		}

		[TestMethod]
		public void Floor_Create_WithDanglingFacilityReference_ShouldThrow()
		{
			var missingFacilityId = Guid.NewGuid().ToString();
			var floor = NewFloor("FLR-1");
			floor.FacilityFk = new FacilityRelation { Facility = new SdmObjectReference<Facility>(missingFacilityId) };

			var action = () => Helper.Floors.Create(floor);

			action.Should().Throw<Exception>().WithMessage($"*Referenced Facility '{missingFacilityId}' does not exist.*");
		}

		[TestMethod]
		public void Floor_Create_WithExistingFacilityReference_ShouldSucceed()
		{
			var facility = Helper.Facilities.Create(NewFacility("FAC-1"));
			var floor = NewFloor("FLR-1");
			floor.FacilityFk = new FacilityRelation { Facility = new SdmObjectReference<Facility>(facility.Identifier) };

			Action action = () => Helper.Floors.Create(floor);

			action.Should().NotThrow();
		}

		[TestMethod]
		public void Room_Create_WithDanglingFloorReference_ShouldThrow()
		{
			var missingFloorId = Guid.NewGuid().ToString();
			var room = NewRoom("ROOM-1");
			room.FloorFk = new FloorRelation { Floor = new SdmObjectReference<Floor>(missingFloorId) };

			var action = () => Helper.Rooms.Create(room);

			action.Should().Throw<Exception>().WithMessage($"*Referenced Floor '{missingFloorId}' does not exist.*");
		}

		[TestMethod]
		public void Room_Create_WithExistingFloorReference_ShouldSucceed()
		{
			var floor = Helper.Floors.Create(NewFloor("FLR-1"));
			var room = NewRoom("ROOM-1");
			room.FloorFk = new FloorRelation { Floor = new SdmObjectReference<Floor>(floor.Identifier) };

			Action action = () => Helper.Rooms.Create(room);

			action.Should().NotThrow();
		}

		[TestMethod]
		public void Room_Create_WithDanglingOwnerReference_ShouldThrow()
		{
			var ownerId = Guid.NewGuid();
			var checker = new ExternalReferenceCheckerStub();
			var helper = NewHelper(checker);
			var room = NewRoom("ROOM-1");
			room.Ownership = new RoomOwnership { Owner = ownerId };

			var action = () => helper.Rooms.Create(room);

			action.Should().Throw<Exception>().WithMessage($"*Referenced Person '{ownerId}' does not exist.*");
		}

		[TestMethod]
		public void Room_Create_WithExistingOwnerReference_ShouldSucceed()
		{
			var ownerId = Guid.NewGuid();
			var checker = new ExternalReferenceCheckerStub(existingPersonIds: new[] { ownerId });
			var helper = NewHelper(checker);
			var room = NewRoom("ROOM-1");
			room.Ownership = new RoomOwnership { Owner = ownerId };

			Action action = () => helper.Rooms.Create(room);

			action.Should().NotThrow();
		}

		[TestMethod]
		public void Room_Create_WithDanglingTeamReference_ShouldThrow()
		{
			var teamId = Guid.NewGuid();
			var checker = new ExternalReferenceCheckerStub();
			var helper = NewHelper(checker);
			var room = NewRoom("ROOM-1");
			room.Ownership = new RoomOwnership { Team = teamId };

			var action = () => helper.Rooms.Create(room);

			action.Should().Throw<Exception>().WithMessage($"*Referenced Team '{teamId}' does not exist.*");
		}

		[TestMethod]
		public void Room_Create_WithExistingTeamReference_ShouldSucceed()
		{
			var teamId = Guid.NewGuid();
			var checker = new ExternalReferenceCheckerStub(existingTeamIds: new[] { teamId });
			var helper = NewHelper(checker);
			var room = NewRoom("ROOM-1");
			room.Ownership = new RoomOwnership { Team = teamId };

			Action action = () => helper.Rooms.Create(room);

			action.Should().NotThrow();
		}

		[TestMethod]
		public void Room_Create_WithExternalReferencesAndNullChecker_ShouldSucceed()
		{
			var room = NewRoom("ROOM-1");
			room.Ownership = new RoomOwnership { Owner = Guid.NewGuid(), Team = Guid.NewGuid() };
			room.ResourceLink = new ResourceLink { ResourceId = Guid.NewGuid() };

			Action action = () => Helper.Rooms.Create(room);

			action.Should().NotThrow();
		}

		[TestMethod]
		public void Room_Create_WithDanglingResourceReference_ShouldThrow()
		{
			var resourceId = Guid.NewGuid();
			var checker = new ExternalReferenceCheckerStub();
			var helper = NewHelper(checker);
			var room = NewRoom("ROOM-1");
			room.ResourceLink = new ResourceLink { ResourceId = resourceId };

			var action = () => helper.Rooms.Create(room);

			action.Should().Throw<Exception>().WithMessage($"*Referenced Resource '{resourceId}' does not exist.*");
		}

		[TestMethod]
		public void Room_Create_WithExistingResourceReference_ShouldSucceed()
		{
			var resourceId = Guid.NewGuid();
			var checker = new ExternalReferenceCheckerStub(existingResourceIds: new[] { resourceId });
			var helper = NewHelper(checker);
			var room = NewRoom("ROOM-1");
			room.ResourceLink = new ResourceLink { ResourceId = resourceId };

			Action action = () => helper.Rooms.Create(room);

			action.Should().NotThrow();
		}

		[TestMethod]
		public void Row_Create_WithDanglingRoomReference_ShouldThrow()
		{
			var missingRoomId = Guid.NewGuid().ToString();
			var row = NewRow("ROW-1");
			row.RoomFk = new RoomRelation { Room = new SdmObjectReference<Room>(missingRoomId) };

			var action = () => Helper.Rows.Create(row);

			action.Should().Throw<Exception>().WithMessage($"*Referenced Room '{missingRoomId}' does not exist.*");
		}

		[TestMethod]
		public void Row_Create_WithExistingRoomReference_ShouldSucceed()
		{
			var room = Helper.Rooms.Create(NewRoom("ROOM-1"));
			var row = NewRow("ROW-1");
			row.RoomFk = new RoomRelation { Room = new SdmObjectReference<Room>(room.Identifier) };

			Action action = () => Helper.Rows.Create(row);

			action.Should().NotThrow();
		}

		[TestMethod]
		public void Row_Create_WithDanglingResourceReference_ShouldThrow()
		{
			var resourceId = Guid.NewGuid();
			var helper = NewHelper(new ExternalReferenceCheckerStub());
			var row = NewRow("ROW-1");
			row.Resource = new ResourceLink { ResourceId = resourceId };

			var action = () => helper.Rows.Create(row);

			action.Should().Throw<Exception>().WithMessage($"*Referenced Resource '{resourceId}' does not exist.*");
		}

		[TestMethod]
		public void Row_Create_WithExistingResourceReference_ShouldSucceed()
		{
			var resourceId = Guid.NewGuid();
			var helper = NewHelper(new ExternalReferenceCheckerStub(existingResourceIds: new[] { resourceId }));
			var row = NewRow("ROW-1");
			row.Resource = new ResourceLink { ResourceId = resourceId };

			Action action = () => helper.Rows.Create(row);

			action.Should().NotThrow();
		}

		[TestMethod]
		public void Zone_Create_WithDanglingRoomReference_ShouldThrow()
		{
			var missingRoomId = Guid.NewGuid().ToString();
			var zone = NewZone("ZONE-1");
			zone.RoomFk = new RoomRelation { Room = new SdmObjectReference<Room>(missingRoomId) };

			var action = () => Helper.Zones.Create(zone);

			action.Should().Throw<Exception>().WithMessage($"*Referenced Room '{missingRoomId}' does not exist.*");
		}

		[TestMethod]
		public void Zone_Create_WithExistingRoomReference_ShouldSucceed()
		{
			var room = Helper.Rooms.Create(NewRoom("ROOM-1"));
			var zone = NewZone("ZONE-1");
			zone.RoomFk = new RoomRelation { Room = new SdmObjectReference<Room>(room.Identifier) };

			Action action = () => Helper.Zones.Create(zone);

			action.Should().NotThrow();
		}

		[TestMethod]
		public void Zone_Create_WithDanglingResourceReference_ShouldThrow()
		{
			var resourceId = Guid.NewGuid();
			var helper = NewHelper(new ExternalReferenceCheckerStub());
			var zone = NewZone("ZONE-1");
			zone.Resource = new ResourceLink { ResourceId = resourceId };

			var action = () => helper.Zones.Create(zone);

			action.Should().Throw<Exception>().WithMessage($"*Referenced Resource '{resourceId}' does not exist.*");
		}

		[TestMethod]
		public void Zone_Create_WithExistingResourceReference_ShouldSucceed()
		{
			var resourceId = Guid.NewGuid();
			var helper = NewHelper(new ExternalReferenceCheckerStub(existingResourceIds: new[] { resourceId }));
			var zone = NewZone("ZONE-1");
			zone.Resource = new ResourceLink { ResourceId = resourceId };

			Action action = () => helper.Zones.Create(zone);

			action.Should().NotThrow();
		}

		[TestMethod]
		public void Desk_Create_WithDanglingRoomReference_ShouldThrow()
		{
			var missingRoomId = Guid.NewGuid().ToString();
			var desk = NewDesk("DSK-1");
			desk.RoomFk = new RoomRelation { Room = new SdmObjectReference<Room>(missingRoomId) };

			var action = () => Helper.Desks.Create(desk);

			action.Should().Throw<Exception>().WithMessage($"*Referenced Room '{missingRoomId}' does not exist.*");
		}

		[TestMethod]
		public void Desk_Create_WithExistingRoomReference_ShouldSucceed()
		{
			var room = Helper.Rooms.Create(NewRoom("ROOM-1"));
			var desk = NewDesk("DSK-1");
			desk.RoomFk = new RoomRelation { Room = new SdmObjectReference<Room>(room.Identifier) };

			Action action = () => Helper.Desks.Create(desk);

			action.Should().NotThrow();
		}

		[TestMethod]
		public void Desk_Create_WithDanglingResourceReference_ShouldThrow()
		{
			var resourceId = Guid.NewGuid();
			var helper = NewHelper(new ExternalReferenceCheckerStub());
			var desk = NewDesk("DSK-1");
			desk.Resource = new ResourceLink { ResourceId = resourceId };

			var action = () => helper.Desks.Create(desk);

			action.Should().Throw<Exception>().WithMessage($"*Referenced Resource '{resourceId}' does not exist.*");
		}

		[TestMethod]
		public void Desk_Create_WithExistingResourceReference_ShouldSucceed()
		{
			var resourceId = Guid.NewGuid();
			var helper = NewHelper(new ExternalReferenceCheckerStub(existingResourceIds: new[] { resourceId }));
			var desk = NewDesk("DSK-1");
			desk.Resource = new ResourceLink { ResourceId = resourceId };

			Action action = () => helper.Desks.Create(desk);

			action.Should().NotThrow();
		}

		[TestMethod]
		public void Rack_Create_WithDanglingRowReference_ShouldThrow()
		{
			var missingRowId = Guid.NewGuid().ToString();
			var rack = NewRack("RACK-1");
			rack.RowFk = new RowRelation { Row = new SdmObjectReference<Row>(missingRowId) };

			var action = () => Helper.Racks.Create(rack);

			action.Should().Throw<Exception>().WithMessage($"*Referenced Row '{missingRowId}' does not exist.*");
		}

		[TestMethod]
		public void Rack_Create_WithExistingRowReference_ShouldSucceed()
		{
			var row = Helper.Rows.Create(NewRow("ROW-1"));
			var rack = NewRack("RACK-1");
			rack.RowFk = new RowRelation { Row = new SdmObjectReference<Row>(row.Identifier) };

			Action action = () => Helper.Racks.Create(rack);

			action.Should().NotThrow();
		}

		[TestMethod]
		public void Rack_Create_WithDanglingZoneReference_ShouldThrow()
		{
			var missingZoneId = Guid.NewGuid().ToString();
			var rack = NewRack("RACK-1");
			rack.ZoneFk = new ZoneRelation { Zone = new SdmObjectReference<Zone>(missingZoneId) };

			var action = () => Helper.Racks.Create(rack);

			action.Should().Throw<Exception>().WithMessage($"*Referenced Zone '{missingZoneId}' does not exist.*");
		}

		[TestMethod]
		public void Rack_Create_WithExistingZoneReference_ShouldSucceed()
		{
			var zone = Helper.Zones.Create(NewZone("ZONE-1"));
			var rack = NewRack("RACK-1");
			rack.ZoneFk = new ZoneRelation { Zone = new SdmObjectReference<Zone>(zone.Identifier) };

			Action action = () => Helper.Racks.Create(rack);

			action.Should().NotThrow();
		}

		[TestMethod]
		public void Rack_Create_WithDanglingResourceReference_ShouldThrow()
		{
			var resourceId = Guid.NewGuid();
			var helper = NewHelper(new ExternalReferenceCheckerStub());
			var rack = NewRack("RACK-1");
			rack.Resource = new ResourceLink { ResourceId = resourceId };

			var action = () => helper.Racks.Create(rack);

			action.Should().Throw<Exception>().WithMessage($"*Referenced Resource '{resourceId}' does not exist.*");
		}

		[TestMethod]
		public void Rack_Create_WithExistingResourceReference_ShouldSucceed()
		{
			var resourceId = Guid.NewGuid();
			var helper = NewHelper(new ExternalReferenceCheckerStub(existingResourceIds: new[] { resourceId }));
			var rack = NewRack("RACK-1");
			rack.Resource = new ResourceLink { ResourceId = resourceId };

			Action action = () => helper.Racks.Create(rack);

			action.Should().NotThrow();
		}

		private static IFacilityManagementApiHelper NewHelper(IFacilityManagementExternalReferenceChecker checker)
		{
			return ConnectionHelper.CreateConnection().GetMockedHelper(checker);
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
			return new Rack
			{
				Identifier = Guid.NewGuid().ToString(),
				RackId = id,
				Capacity = new RackCapacity { MaximumRackCapacity = 42 },
			};
		}

		private sealed class ExternalReferenceCheckerStub : IFacilityManagementExternalReferenceChecker
		{
			private readonly HashSet<Guid> _existingPersonIds;
			private readonly HashSet<Guid> _existingTeamIds;
			private readonly HashSet<Guid> _existingResourceIds;

			public ExternalReferenceCheckerStub(
				IEnumerable<Guid> existingPersonIds = null,
				IEnumerable<Guid> existingTeamIds = null,
				IEnumerable<Guid> existingResourceIds = null)
			{
				_existingPersonIds = new HashSet<Guid>(existingPersonIds ?? Enumerable.Empty<Guid>());
				_existingTeamIds = new HashSet<Guid>(existingTeamIds ?? Enumerable.Empty<Guid>());
				_existingResourceIds = new HashSet<Guid>(existingResourceIds ?? Enumerable.Empty<Guid>());
			}

			public IReadOnlyCollection<string> GetIdentifiersWithAssets(FacilityManagementEntityType entityType, IReadOnlyCollection<string> identifiers)
			{
				return Array.Empty<string>();
			}

			public IReadOnlyCollection<Guid> GetExistingPersonIds(IReadOnlyCollection<Guid> personIds)
			{
				return personIds.Where(id => _existingPersonIds.Contains(id)).ToList();
			}

			public IReadOnlyCollection<Guid> GetExistingTeamIds(IReadOnlyCollection<Guid> teamIds)
			{
				return teamIds.Where(id => _existingTeamIds.Contains(id)).ToList();
			}

			public IReadOnlyCollection<Guid> GetExistingResourceIds(IReadOnlyCollection<Guid> resourceIds)
			{
				return resourceIds.Where(id => _existingResourceIds.Contains(id)).ToList();
			}
		}
	}
}
