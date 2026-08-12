namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    using System;

    using Newtonsoft.Json;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    //[GenerateExposers]
    //[SdmDomStorage("(slc)facility_management")]
    public class Facility : SdmObject<Facility>, IEntityTracking
    {
        [JsonIgnore]
        private ChangeTrackingFieldHandler _fieldHandler;
        [JsonIgnore]
        private bool _isNew = true;

        public Facility()
        {
            _fieldHandler = new ChangeTrackingFieldHandler();
        }

        [JsonIgnore]
        [SdmIgnore]
        private ChangeTrackingFieldHandler FieldHandler
        {
            get
            {
                if (_fieldHandler == null)
                {
                    _fieldHandler = new ChangeTrackingFieldHandler();
                }
                return _fieldHandler;
            }
        }

        [JsonIgnore]
        [SdmIgnore]
        public bool Changed => FieldHandler.HasChanges;

        [JsonIgnore]
        [SdmIgnore]
        internal bool IsNewInternal
        {
            get => _isNew;
            set => _isNew = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        public bool IsNew => _isNew;

        #region Section Tracking

        [JsonIgnore]
        [SdmIgnore]
        internal Guid? FacilityPropertiesSectionId { get; set; }

        #endregion

        public string FacilityId
        {
            get => FacilityIdField.Value;
            set => FacilityIdField.Value = value;
        }

        public string Name
        {
            get => NameField.Value;
            set => NameField.Value = value;
        }

        public string Description
        {
            get => DescriptionField.Value;
            set => DescriptionField.Value = value;
        }

        public SlcFacility_Management.Enums.FacilityTypeEnum? FacilityType
        {
            get => FacilityTypeField.Value;
            set => FacilityTypeField.Value = value;
        }

        public string Address
        {
            get => AddressField.Value;
            set => AddressField.Value = value;
        }

        public string City
        {
            get => CityField.Value;
            set => CityField.Value = value;
        }

        public string ZipCode
        {
            get => ZipCodeField.Value;
            set => ZipCodeField.Value = value;
        }

        public string Country
        {
            get => CountryField.Value;
            set => CountryField.Value = value;
        }

        public double? Latitude
        {
            get => LatitudeField.Value;
            set => LatitudeField.Value = value;
        }

        public double? Longitude
        {
            get => LongitudeField.Value;
            set => LongitudeField.Value = value;
        }

        private SiteRelation _siteFk;

        public SiteRelation SiteFk => _siteFk ?? (_siteFk = new SiteRelation());

        [SdmIgnore]
        public SlcFacility_Management.Behaviors.Facility_Behaviour.StatusesEnum State { get; internal set; }

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> FacilityIdField => FieldHandler.GetOrCreateField(
            nameof(FacilityId), () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> NameField => FieldHandler.GetOrCreateField(
            nameof(Name), () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> DescriptionField => FieldHandler.GetOrCreateField(
            nameof(Description), () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        internal IChangeTrackingField<SlcFacility_Management.Enums.FacilityTypeEnum?> FacilityTypeField => FieldHandler.GetOrCreateField(
            nameof(FacilityType), () => new ChangeTrackingField<SlcFacility_Management.Enums.FacilityTypeEnum?>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> AddressField => FieldHandler.GetOrCreateField(
            nameof(Address), () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> CityField => FieldHandler.GetOrCreateField(
            nameof(City), () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> ZipCodeField => FieldHandler.GetOrCreateField(
            nameof(ZipCode), () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> CountryField => FieldHandler.GetOrCreateField(
            nameof(Country), () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        internal IChangeTrackingField<double?> LatitudeField => FieldHandler.GetOrCreateField(
            nameof(Latitude), () => new ChangeTrackingField<double?>(null));

        [JsonIgnore]
        internal IChangeTrackingField<double?> LongitudeField => FieldHandler.GetOrCreateField(
            nameof(Longitude), () => new ChangeTrackingField<double?>(null));

        public void ResetChangeTracking()
        {
            FieldHandler.ApplyChanges();
        }
    }
}
