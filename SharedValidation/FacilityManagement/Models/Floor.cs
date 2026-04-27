namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    //[GenerateExposers]
    [SdmDomStorage("(slc)facility_management")]
    public class Floor : SdmObject<Floor>
    {
        public string Name { get; set; }

        public string Plan { get; set; }

        public string Description { get; set; }

        public string FloorId { get; set; }

        public FacilityRelation FacilityFk { get; set; }
    } 
}