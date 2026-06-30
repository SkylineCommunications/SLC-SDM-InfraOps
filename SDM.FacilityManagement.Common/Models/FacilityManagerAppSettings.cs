namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    using Skyline.DataMiner.SDM;

    //[GenerateExposers]
    //[SdmDomStorage("(slc)facility_management")]
    public class FacilityManagerAppSettings : SdmObject<FacilityManagerAppSettings>
    {
        public string GoogleMapsAPIKey { get; set; }
    }
}
