namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using Newtonsoft.Json;
    // [GenerateExposers]
    // [SdmDomStorage("(slc)asset_management")]
    public class PowerPort : SdmObject<PowerPort>
	{
		public PowerPortInfo PowerPortInfo { get; set; } = new PowerPortInfo();

		// within AssetRelation section definition
		public SdmObjectReference<Asset> Asset { get; set; }

        #region Section Tracking

        [JsonIgnore]
        [SdmIgnore]
        internal Guid? AssetRelationPropertiesSectionId { get; set; }

        #endregion

	}
}