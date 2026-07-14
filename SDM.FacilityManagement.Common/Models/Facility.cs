namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    using SharedMappers.DomIds;

    //[GenerateExposers]
    //[SdmDomStorage("(slc)facility_management")]
    public class Facility : SdmObject<Facility>
	{
		public string FacilityId { get; set; }

		public string Name { get; set; }

		public string Description { get; set; }

		public SlcFacility_Management.Enums.FacilityTypeEnum? FacilityType { get; set; }

		public string Address { get; set; }

		public string City { get; set; }

		public string ZipCode { get; set; }

		public string Country { get; set; }

		public double? Latitude { get; set; }

		public double? Longitude { get; set; }

        public SiteRelation SiteFk { get; set; }
    }
}