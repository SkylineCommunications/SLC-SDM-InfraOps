namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using Newtonsoft.Json;
    // [GenerateExposers]
    // [SdmDomStorage("(slc)asset_management")]
    public class PowerPort : SdmObject<PowerPort>
	{
        [JsonIgnore]
        private PowerPortInfo _powerPortInfo;

		public PowerPortInfo PowerPortInfo => _powerPortInfo ?? (_powerPortInfo = new PowerPortInfo());

		// within AssetRelation section definition
		public SdmObjectReference<Asset> Asset { get; set; }

        #region Section Tracking

        [JsonIgnore]
        [SdmIgnore]
        internal Guid? AssetRelationPropertiesSectionId { get; set; }

        #endregion

	}
}