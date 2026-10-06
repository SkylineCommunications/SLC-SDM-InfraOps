namespace SDM.AssetManagement.Tests.Setup
{
    using System.Linq;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.FacilityManagement.Helpers;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;

    /// <summary>
    /// Creates the minimal Facility > Floor > Room > Row chain that Facility entities now require.
    /// </summary>
    public static class FacilityParentChain
    {
        private const string FacilityIdentifier = "0b6f1c2e-0000-4000-8000-000000000001";
        private const string FloorIdentifier = "0b6f1c2e-0000-4000-8000-000000000002";
        private const string RoomIdentifier = "0b6f1c2e-0000-4000-8000-000000000003";
        private const string RowIdentifier = "0b6f1c2e-0000-4000-8000-000000000004";

        public static Room AttachFloor(this IFacilityManagementApiHelper helper, Room room)
        {
            if (room.FloorFk.IsEmpty || room.FloorFk.Floor.Identifier == FloorIdentifier)
            {
                EnsureFloor(helper);
                room.FloorFk.Floor = new SdmObjectReference<Floor>(FloorIdentifier);
            }

            return room;
        }

        public static Rack AttachRow(this IFacilityManagementApiHelper helper, Rack rack)
        {
            if (rack.RowFk.IsEmpty || rack.RowFk.Row.Identifier == RowIdentifier)
            {
                EnsureRow(helper);
                rack.RowFk.Row = new SdmObjectReference<Row>(RowIdentifier);
            }

            return rack;
        }

        private static void EnsureFloor(IFacilityManagementApiHelper helper)
        {
            if (!helper.Facilities.Read(FacilityExposers.Identifier.Equal(FacilityIdentifier)).Any())
            {
                helper.Facilities.Create(new Facility { Identifier = FacilityIdentifier, FacilityId = "FAC-PARENT", Name = "Parent Facility" });
            }

            if (!helper.Floors.Read(FloorExposers.Identifier.Equal(FloorIdentifier)).Any())
            {
                var floor = new Floor { Identifier = FloorIdentifier, FloorId = "FLR-PARENT", Name = "Parent Floor" };
                floor.FacilityFk.Facility = new SdmObjectReference<Facility>(FacilityIdentifier);
                helper.Floors.Create(floor);
            }
        }

        private static void EnsureRow(IFacilityManagementApiHelper helper)
        {
            if (!helper.Rows.Read(RowExposers.Identifier.Equal(RowIdentifier)).Any())
            {
                var room = helper.Rooms.Read(RoomExposers.Identifier.Equal(RoomIdentifier)).FirstOrDefault();
                if (room is null)
                {
                    helper.Rooms.Create(helper.AttachFloor(new Room { Identifier = RoomIdentifier, RoomId = "ROOM-PARENT", Name = "Parent Room" }));
                }

                var row = new Row { Identifier = RowIdentifier, RowId = "ROW-PARENT", Name = "Parent Row" };
                row.RoomFk.Room = new SdmObjectReference<Room>(RoomIdentifier);
                helper.Rows.Create(row);
            }
        }
    }
}