namespace SDM.FacilityManagement.Tests.Rooms
{
    using System;
    using System.Linq;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;

    public partial class RoomDomRepositoryTests
    {
        [TestMethod]
        public void RoomDomRepository_CreateWithFloorReference_ReadBack_PersistsRoomIdAndFloor()
        {
            var floor = Helper.Floors.Create(new Floor { Identifier = Guid.NewGuid().ToString(), FloorId = "FL-ROOM", Name = "Floor ROOM" });
            var room = new Room { Identifier = Guid.NewGuid().ToString(), Name = "Room 1", RoomId = "RM-1" };
            room.FloorFk.Floor = new SdmObjectReference<Floor>(floor.Identifier);

            Helper.Rooms.Create(room);

            var reloaded = Helper.Rooms.Read(RoomExposers.Identifier.Equal(room.Identifier)).SingleOrDefault();

            using (new AssertionScope())
            {
                reloaded.Should().NotBeNull();
                reloaded!.RoomId.Should().Be("RM-1");
                reloaded.FloorFk.Floor.Should().NotBeNull();
                reloaded.FloorFk.Floor.Identifier.Should().Be(floor.Identifier);
            }
        }
    }
}
