namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using Newtonsoft.Json;

    //[GenerateExposers]
    //[SdmDomStorage("(slc)asset_management")]
    public class AssetManagerAppSettings : SdmObject<AssetManagerAppSettings>
    {
        public bool EnableAssetHistory { get; set; }

        public int PlanAndBuildJobPrompt { get; set; }

        public bool EnableConnectionHistory { get; set; }

        public TimeSpan? HistoryTTL { get; set; }

        public long? HistoryLimit { get; set; }

        #region Section Tracking

        [JsonIgnore]
        [SdmIgnore]
        internal Guid? AppSettingsSectionId { get; set; }

        #endregion

    }
}
