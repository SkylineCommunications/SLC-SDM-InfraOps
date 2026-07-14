using SharedMappers.DomIds;

namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    // [GenerateExposers]
    //[SdmDomStorage("(slc)facility_management")]
    public class Zone : SdmObject<Zone>
    {
        public string Name { get; set; }

        public string Plan { get; set; }

        public string Description { get; set; }

        public SlcFacility_Management.Enums.ThermalType? ThermalType { get; set; }

        public double? XPosition { get; set; }

        public double? YPosition { get; set; }

        public double? Width { get; set; }

        public double? Depth { get; set; }

        public string ZoneId { get; set; }

        public ZoneCapacity ZoneCapacity { get; set; }

        public RoomRelation RoomFk { get; set; }

        public ResourceLink Resource { get; set; }
    } 
}