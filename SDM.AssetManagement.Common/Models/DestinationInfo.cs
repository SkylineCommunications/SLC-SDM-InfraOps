namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using Newtonsoft.Json;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public class DestinationInfo : ISectionTrackable
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }

        public string CableTag { get; set; }

        public Guid Port { get; set; }

        public SdmObjectReference<PortType> PortType { get; set; }
    }
}
