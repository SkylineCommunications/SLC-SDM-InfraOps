using System.Collections.Generic;
namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;

    // [GenerateExposers]
    [SdmDomStorage("(slc)facility_management")]
    public class Rack: SdmObject<Rack>
    {
        public string Name { get; set; }

        public string Model { get; set; }

        public SlcFacility_Management.Enums.RackpositionenumEnum Position { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }

        public string Description { get; set; }

        public bool Bookable { get; set; }

        public SlcFacility_Management.Enums.CoolingflowenumEnum CoolingFlow { get; set; }

        public double XPosition { get; set; }

        public double YPosition { get; set; }

        public string Label { get; set; }

        public SlcFacility_Management.Enums.Placementorientationenum Orientation { get; set; }

        public string RackId { get; set; }

        public RackCapacity Capacity { get; set; }

        public RowRelation RowFk { get; set; }

        public ZoneRelation ZoneFk { get; set; }

        public ResourceLink Resource { get; set; }

        public List<ImageInfo> ImageDetails { get; set; }
    }
}