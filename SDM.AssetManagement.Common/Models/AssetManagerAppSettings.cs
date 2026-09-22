namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using Newtonsoft.Json;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    //[GenerateExposers]
    //[SdmDomStorage("(slc)asset_management")]
    public class AssetManagerAppSettings : SdmObject<AssetManagerAppSettings>, IReadOnlyModuleIdReferencer
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

        #region Module Tracking

        public string ModuleId => SlcAsset_Management.ModuleId;

        #endregion

    }
}
