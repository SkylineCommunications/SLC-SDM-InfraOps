namespace SDM.FacilityManagement.Tests.Rows
{
    using System;
    using System.Linq;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;

    public partial class RowDomRepositoryTests
    {
        [TestMethod]
        public void RowDomRepository_CreateWithRoomReference_ReadBack_PersistsRowIdRoomAndYPosition()
        {
            var room = Helper.Rooms.Create(new Room { Identifier = Guid.NewGuid().ToString(), RoomId = "RM-ROW", Name = "Room ROW" });
            var row = new Row { Identifier = Guid.NewGuid().ToString(), Name = "Row 1", RowId = "RW-1", YPosition = 12.5 };
            row.RoomFk.Room = new SdmObjectReference<Room>(room.Identifier);

            Helper.Rows.Create(row);

            var reloaded = Helper.Rows.Read(RowExposers.Identifier.Equal(row.Identifier)).SingleOrDefault();

            using (new AssertionScope())
            {
                reloaded.Should().NotBeNull();
                reloaded!.RowId.Should().Be("RW-1");
                reloaded.RoomFk.Room.Should().NotBeNull();
                reloaded.RoomFk.Room.Identifier.Should().Be(room.Identifier);
                reloaded.YPosition.Should().Be(12.5);
            }
        }
    }
}
