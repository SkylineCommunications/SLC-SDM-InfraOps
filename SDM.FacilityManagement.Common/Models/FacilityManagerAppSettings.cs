namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    using System;

    using Newtonsoft.Json;

    using Skyline.DataMiner.SDM;

    //[GenerateExposers]
    //[SdmDomStorage("(slc)facility_management")]
    public class FacilityManagerAppSettings : SdmObject<FacilityManagerAppSettings>
    {
        #region Section Tracking

        [JsonIgnore]
        [SdmIgnore]
        internal Guid? AppSettingsSectionId { get; set; }

        #endregion

        public string GoogleMapsAPIKey { get; set; }
    }
}
