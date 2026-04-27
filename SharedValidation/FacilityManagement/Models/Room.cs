namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    using Skyline.DataMiner.SDM;

    //[GenerateExposers]
    [SdmDomStorage("(slc)facility_management")]
    public class Room : SdmObject<Room>
    {
        public string Name { get; set; }

        public string Plan { get; set; }

        public string Description { get; set; }

        public long Width { get; set; }

        public long Depth { get; set; }

        public string RoomId { get; set; }

        public RoomOwnership Onwership { get; set; }

        public ResourceLink ResourceLink { get; set; }

        public FloorRelation FloorFk { get; set; }
    }
}
