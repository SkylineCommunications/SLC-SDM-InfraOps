namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    using System;

    using Newtonsoft.Json;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    //[GenerateExposers]
    //[SdmDomStorage("(slc)facility_management")]
    public class FacilityManagerAppSettings : SdmObject<FacilityManagerAppSettings>, IReadOnlyModuleIdReferencer
    {
        #region Section Tracking

        [JsonIgnore]
        [SdmIgnore]
        internal Guid? AppSettingsSectionId { get; set; }

        #endregion

        #region Module Tracking

        public string ModuleId => SlcFacility_Management.ModuleId;

        #endregion

        public string GoogleMapsAPIKey { get; set; }
    }
}
