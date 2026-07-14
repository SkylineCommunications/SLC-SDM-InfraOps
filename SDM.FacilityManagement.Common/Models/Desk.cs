namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;

    //[GenerateExposers]
    //[SdmDomStorage("(slc)facility_management")]
    public class Desk : SdmObject<Desk>
    {
        public string Name { get; set; }

        public string Plan { get; set; }

        public string Description { get; set; }

        public string DeskID { get; set; }

        public RoomRelation RoomFk { get; set; }

        public ResourceLink Resource { get; set; }

        [SdmIgnore]
        public SlcFacility_Management.Behaviors.Desk_Behaviour.StatusesEnum State { get; internal set; }
    }
}
