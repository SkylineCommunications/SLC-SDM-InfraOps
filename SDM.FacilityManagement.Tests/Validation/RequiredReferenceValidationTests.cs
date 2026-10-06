namespace SDM.FacilityManagement.Tests.Validation
{
    using System;

    using FluentAssertions;

    using SDM.FacilityManagement.Tests.Setup;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;

    [TestClass]
    public class RequiredReferenceValidationTests : BaseRepositoryTest
    {
        [TestMethod]
        public void Floor_Create_WithoutFacility_ShouldThrow()
        {
            var floor = new Floor { Identifier = Guid.NewGuid().ToString(), FloorId = "FLR-1", Name = "Floor FLR-1" };

            var action = () => Helper.Floors.Create(floor);

            action.Should().Throw<Exception>().WithMessage("*Facility is required.*");
        }

        [TestMethod]
        public void Floor_Update_WithoutTouchingFacility_ShouldSucceed()
        {
            var facility = Helper.Facilities.Create(new Facility { Identifier = Guid.NewGuid().ToString(), FacilityId = "FAC-1", Name = "Facility FAC-1" });
            var floor = new Floor { Identifier = Guid.NewGuid().ToString(), FloorId = "FLR-1", Name = "Floor FLR-1" };
            floor.FacilityFk.Facility = new SdmObjectReference<Facility>(facility.Identifier);
            var created = Helper.Floors.Create(floor);

            created.Name = "Renamed";
            Action action = () => Helper.Floors.Update(created);

            action.Should().NotThrow();
        }

        [TestMethod]
        public void Room_Create_WithoutFloor_ShouldThrow()
        {
            var room = new Room { Identifier = Guid.NewGuid().ToString(), RoomId = "ROOM-1", Name = "Room ROOM-1" };

            var action = () => Helper.Rooms.Create(room);

            action.Should().Throw<Exception>().WithMessage("*Floor is required.*");
        }

        [TestMethod]
        public void Row_Create_WithoutRoom_ShouldThrow()
        {
            var row = new Row { Identifier = Guid.NewGuid().ToString(), RowId = "ROW-1", Name = "Row ROW-1" };

            var action = () => Helper.Rows.Create(row);

            action.Should().Throw<Exception>().WithMessage("*Room is required.*");
        }

        [TestMethod]
        public void Zone_Create_WithoutRoom_ShouldThrow()
        {
            var zone = new Zone { Identifier = Guid.NewGuid().ToString(), ZoneId = "ZONE-1", Name = "Zone ZONE-1", ZoneCapacity = { CoolingCapacity = 5.0 } };

            var action = () => Helper.Zones.Create(zone);

            action.Should().Throw<Exception>().WithMessage("*Room is required.*");
        }

        [TestMethod]
        public void Desk_Create_WithoutRoom_ShouldThrow()
        {
            var desk = new Desk { Identifier = Guid.NewGuid().ToString(), DeskID = "DESK-1", Name = "Desk DESK-1" };

            var action = () => Helper.Desks.Create(desk);

            action.Should().Throw<Exception>().WithMessage("*Room is required.*");
        }

        [TestMethod]
        public void Rack_Create_WithoutRow_ShouldThrow()
        {
            var rack = new Rack
            {
                Identifier = Guid.NewGuid().ToString(),
                RackId = "RACK-1",
                Name = "Rack RACK-1",
                Position = SlcFacility_Management.Enums.RackpositionenumEnum.Bottom,
            };
            rack.Capacity.MaximumRackCapacity = 42;

            var action = () => Helper.Racks.Create(rack);

            action.Should().Throw<Exception>().WithMessage("*Row is required.*");
        }

        [TestMethod]
        public void Floor_CreateOrUpdate_BatchWithOneMissingFacility_ShouldThrow()
        {
            var facility = Helper.Facilities.Create(new Facility { Identifier = Guid.NewGuid().ToString(), FacilityId = "FAC-1", Name = "Facility FAC-1" });
            var valid = new Floor { Identifier = Guid.NewGuid().ToString(), FloorId = "FLR-1", Name = "Floor FLR-1" };
            valid.FacilityFk.Facility = new SdmObjectReference<Facility>(facility.Identifier);
            var invalid = new Floor { Identifier = Guid.NewGuid().ToString(), FloorId = "FLR-2", Name = "Floor FLR-2" };

            var action = () => Helper.Floors.CreateOrUpdate(new[] { valid, invalid });

            action.Should().Throw<Exception>().WithMessage("*Facility is required.*");
        }
    }
}
