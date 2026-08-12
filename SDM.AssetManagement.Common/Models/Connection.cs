namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using SharedMappers.DomIds;
    using System;
    using Newtonsoft.Json;

    //[GenerateExposers]
    //[SdmDomStorage("(slc)asset_management")]
    public class Connection : SdmObject<Connection>
    {
        [JsonIgnore]
        private SourceInfo _source;
        [JsonIgnore]
        private DestinationInfo _destination;

        public string Notes { get; set; }

        public string Description { get; set; }

        public SlcAsset_Management.Enums.ConnectionType? ConnectionType { get; set; }

        public SdmObjectReference<CableType> CableType { get; set; }

        /// <summary>
        /// Gets or sets the length of the cable in meters.
        /// </summary>
        public double? CableLength { get; set; }

        public SourceInfo Source => _source ?? (_source = new SourceInfo());

        public DestinationInfo Destination => _destination ?? (_destination = new DestinationInfo());

        #region Section Tracking

        [JsonIgnore]
        [SdmIgnore]
        internal Guid? ConnectionPropertiesSectionId { get; set; }

        [JsonIgnore]
        [SdmIgnore]
        internal Guid? CableInformationSectionId { get; set; }

        #endregion

    }
}
