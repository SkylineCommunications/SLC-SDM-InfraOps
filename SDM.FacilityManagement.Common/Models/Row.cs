namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    // [GenerateExposers]
    //[SdmDomStorage("(slc)facility_management")]
    public class Row : SdmObject<Row>
    {
        public string Name { get; set; }

        public string Plan { get; set; }

        public string Description { get; set; }

        public string Label { get; set; }

        public string RowId { get; set; }

        public double YPosition { get; set; }

        public RoomRelation RoomFk { get; set; }

        public ResourceLink Resource { get; set; }
    }
}
