namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.InfraOps.Core.Models;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    //[GenerateExposers]
    //[SdmDomStorage("(slc)facility_management")]
    public sealed class FacilityManagerAppSettings : SdmObjectBase<FacilityManagerAppSettings>, IEquatable<FacilityManagerAppSettings>, IEntityTracking
    {
        [JsonIgnore]
        private ChangeTrackingFieldHandler _fieldHandler;
        [JsonIgnore]
        private bool _isNew = true;

        public FacilityManagerAppSettings()
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
        public bool IsNew => _isNew;

        /// <summary>
        /// Sets the IsNew flag. Used internally when loading from database.
        /// </summary>
        [JsonIgnore]
        [SdmIgnore]
        internal bool IsNewInternal
        {
            get => _isNew;
            set => _isNew = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        public bool Changed => FieldHandler.HasChanges;

        #region Section Tracking

        [JsonIgnore]
        [SdmIgnore]
        internal Guid? AppSettingsSectionId { get; set; }

        #endregion

        public string GoogleMapsAPIKey
        {
            get => GoogleMapsAPIKeyField.Value;
            set => GoogleMapsAPIKeyField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> GoogleMapsAPIKeyField => FieldHandler.GetOrCreateField(
            nameof(GoogleMapsAPIKey),
            () => new ChangeTrackingStringField(null));

        public IEnumerable<TrackingFieldValueDifference> GetChanges()
        {
            return FieldHandler.GetChanges()
                .Select(kvp => new TrackingFieldValueDifference
                {
                    FieldName = kvp.Key,
                    OldValue = kvp.Value.prevVal,
                    NewValue = kvp.Value.newVal,
                });
        }

        public void ResetChangeTracking()
        {
            FieldHandler?.ApplyChanges();
        }

        public static bool operator ==(FacilityManagerAppSettings left, FacilityManagerAppSettings right)
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

        public static bool operator !=(FacilityManagerAppSettings left, FacilityManagerAppSettings right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as FacilityManagerAppSettings);
        }

        public bool Equals(FacilityManagerAppSettings other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(GoogleMapsAPIKey, other.GoogleMapsAPIKey, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + (GoogleMapsAPIKey != null ? GoogleMapsAPIKey.GetHashCode() : 0);
                return hash;
            }
        }
    }
}