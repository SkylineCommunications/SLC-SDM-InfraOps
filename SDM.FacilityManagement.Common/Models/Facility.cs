namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    using SharedMappers.DomIds;
    using Skyline.DataMiner.SDM.InfraOps.Core.Models;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    //[GenerateExposers]
    //[SdmDomStorage("(slc)facility_management")]
    public sealed class Facility : SdmObjectBase<Facility>, IEquatable<Facility>, IEntityTracking
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
        public bool Changed => FieldHandler.HasChanges || _siteFk?.Changed == true;

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

        public IEnumerable<TrackingFieldValueDifference> GetChanges()
        {
            return FieldHandler.GetChanges()
                .Select(kvp => new TrackingFieldValueDifference
                {
                    FieldName = kvp.Key,
                    OldValue = kvp.Value.prevVal,
                    NewValue = kvp.Value.newVal,
                })
                .Concat(_siteFk?.GetChanges() ?? Enumerable.Empty<TrackingFieldValueDifference>());
        }

        public void ResetChangeTracking()
        {
            FieldHandler.ApplyChanges();
            _siteFk?.ResetChangeTracking();
        }

        #region Equality

        public static bool operator ==(Facility left, Facility right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left is null || right is null)
            {
                return false;
            }

            return left.Equals(right);
        }

        public static bool operator !=(Facility left, Facility right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Facility);
        }

        public bool Equals(Facility other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return
                string.Equals(FacilityId, other.FacilityId, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(Description, other.Description, StringComparison.OrdinalIgnoreCase) &&
                FacilityType == other.FacilityType &&
                string.Equals(Address, other.Address, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(City, other.City, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(ZipCode, other.ZipCode, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(Country, other.Country, StringComparison.OrdinalIgnoreCase) &&
                Latitude == other.Latitude &&
                Longitude == other.Longitude &&
                Equals(SiteFk, other.SiteFk) &&
                State == other.State;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + (FacilityId != null ? FacilityId.GetHashCode() : 0);
                hash = (hash * 23) + (Name != null ? Name.GetHashCode() : 0);
                hash = (hash * 23) + (Description != null ? Description.GetHashCode() : 0);
                hash = (hash * 23) + (Address != null ? Address.GetHashCode() : 0);
                hash = (hash * 23) + (City != null ? City.GetHashCode() : 0);
                hash = (hash * 23) + (SiteFk != null ? SiteFk.GetHashCode() : 0);
                hash = (hash * 23) + State.GetHashCode();
                return hash;
            }
        }

        #endregion
    }
}