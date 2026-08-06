namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using Newtonsoft.Json;
    //[GenerateExposers]
    //[SdmDomStorage("(slc)asset_management")]
    public class CableType : SdmObject<CableType>
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public CategoryRelation CategoryLinks { get; set; }

        #region Section Tracking

        [JsonIgnore]
        [SdmIgnore]
        internal Guid? CableTypePropertiesSectionId { get; set; }

        #endregion

    }
}
