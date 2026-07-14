namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using SharedMappers.DomIds;

    //[GenerateExposers]
    //[SdmDomStorage("(slc)asset_management")]
    public class Connection : SdmObject<Connection>
    {
        public string Notes { get; set; }

        public string Description { get; set; }

        public SlcAsset_Management.Enums.ConnectionType? ConnectionType { get; set; }

        public SdmObjectReference<CableType> CableType { get; set; }

        /// <summary>
        /// Gets or sets the length of the cable in meters.
        /// </summary>
        public double? CableLength { get; set; }

        public SourceInfo Source { get; set; }

        public DestinationInfo Destination { get; set; }
    }
}
