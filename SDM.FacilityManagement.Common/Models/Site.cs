namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    using Newtonsoft.Json;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    // [GenerateExposers]
    //[SdmDomStorage("(slc)facility_management")]
    public class Site : SdmObject<Site>, IEntityTracking
    {
        [JsonIgnore]
        private ChangeTrackingFieldHandler _fieldHandler;
        [JsonIgnore]
        private bool _isNew = true;

        public Site()
        {
            _fieldHandler = new ChangeTrackingFieldHandler();
        }

        // Ensure _fieldHandler is always initialized (handles JSON deserialization without constructor)
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

        // PUBLIC API: Simple properties
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

        public string SiteId
        {
            get => SiteIdField.Value;
            set => SiteIdField.Value = value;
        }

        [SdmIgnore]
        public SlcFacility_Management.Behaviors.Site_Behaviour.StatusesEnum State { get; internal set; }

        // INTERNAL: Change tracking fields (validation handler uses these)
        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> NameField => FieldHandler.GetOrCreateField(
            nameof(Name), () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> DescriptionField => FieldHandler.GetOrCreateField(
            nameof(Description), () => new ChangeTrackingStringField(null));

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

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> SiteIdField => FieldHandler.GetOrCreateField(
            nameof(SiteId), () => new ChangeTrackingStringField(null));

        // Reset change tracking after deserialization or save
        public void ResetChangeTracking()
        {
            FieldHandler.ApplyChanges();
        }
    }
}
