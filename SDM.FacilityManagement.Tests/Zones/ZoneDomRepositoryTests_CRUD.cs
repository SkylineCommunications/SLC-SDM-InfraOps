namespace SDM.FacilityManagement.Tests.Zones
{
    using System;
    using System.Linq;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;

    public partial class ZoneDomRepositoryTests
    {
        [TestMethod]
        public void ZoneDomRepository_CreateWithRoomReference_ReadBack_PersistsZoneIdRoomWidthAndDepth()
        {
            var room = Helper.Rooms.Create(new Room { Identifier = Guid.NewGuid().ToString(), RoomId = "RM-ZONE", Name = "Room ZONE" });
            var zone = new Zone
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = "Zone 1",
                ZoneId = "ZN-1",
                Width = 10.0,
                Depth = 20.0,
                ZoneCapacity = { CoolingCapacity = 5.0 },
            };
            zone.RoomFk.Room = new SdmObjectReference<Room>(room.Identifier);

            Helper.Zones.Create(zone);

            var reloaded = Helper.Zones.Read(ZoneExposers.Identifier.Equal(zone.Identifier)).SingleOrDefault();

            using (new AssertionScope())
            {
                reloaded.Should().NotBeNull();
                reloaded!.ZoneId.Should().Be("ZN-1");
                reloaded.RoomFk.Room.Should().NotBeNull();
                reloaded.RoomFk.Room.Identifier.Should().Be(room.Identifier);
                reloaded.Width.Should().Be(10.0);
                reloaded.Depth.Should().Be(20.0);
            }
        }
    }
}
