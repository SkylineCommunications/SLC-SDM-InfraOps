namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;

    public class DestinationInfo
    {
        public string CableTag { get; set; }

        public Guid Port { get; set; }

        public SdmObjectReference<PortType> PortType { get; set; }
    }
}
