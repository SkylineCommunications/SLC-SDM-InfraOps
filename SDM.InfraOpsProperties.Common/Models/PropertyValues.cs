namespace Skyline.DataMiner.SDM.InfraOpsProperties.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Newtonsoft.Json;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.InfraOps.Core.Models;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    //[GenerateExposers]
    //[SdmDomStorage("(infraops)properties")]
    public sealed class PropertyValues : SdmObjectBase<PropertyValues>, IEquatable<PropertyValues>, IEntityTracking
    {
        [JsonIgnore]
        private ChangeTrackingFieldHandler _fieldHandler;
        [JsonIgnore]
        private bool _isNew = true;

        public PropertyValues()
        {
            _fieldHandler = new ChangeTrackingFieldHandler();
        }

        [JsonIgnore]
        [SdmIgnore]
        internal ChangeTrackingFieldHandler FieldHandler
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

        /// <summary>
        /// Gets a value indicating whether the entity has not yet been persisted.
        /// </summary>
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
        public bool Changed =>
            FieldHandler.HasChanges ||
            ValuesField?.Changed == true;

        #region PropertyValueInfo

        public Guid LinkedObjectID
        {
            get => LinkedObjectIDField.Value;
            set => LinkedObjectIDField.Value = value;
        }

        public string Scope
        {
            get => ScopeField.Value;
            set => ScopeField.Value = value;
        }

        public string SubID
        {
            get => SubIDField.Value;
            set => SubIDField.Value = value;
        }

        #endregion

        #region PropertyValue

        public List<PropertyValue> Values
        {
            get => ValuesField.Value ?? (ValuesField.Value = new List<PropertyValue>());
            set => ValuesField.Value = value;
        }

        #endregion

        #region Section Tracking

        /// <summary>
        /// Gets or sets the DOM Section ID of the PropertyValuesProperties section, captured on read so it can be reused on update.
        /// </summary>
        [JsonIgnore]
        [SdmIgnore]
        internal Guid? PropertyValuesPropertiesSectionId { get; set; }

        #endregion

        #region PropertyValueInfo Tracking Fields

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<Guid> LinkedObjectIDField => FieldHandler.GetOrCreateField(
            nameof(LinkedObjectID),
            () => new ChangeTrackingField<Guid>(Guid.Empty));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> ScopeField => FieldHandler.GetOrCreateField(
            nameof(Scope),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> SubIDField => FieldHandler.GetOrCreateField(
            nameof(SubID),
            () => new ChangeTrackingStringField(null));

        #endregion

        #region Collection Tracking Fields

        [JsonIgnore]
        [SdmIgnore]
        internal ChangeTrackingArrayField<PropertyValue> ValuesField => FieldHandler.GetOrCreateArrayField(
            nameof(Values),
            () => new ChangeTrackingArrayField<PropertyValue>(new List<PropertyValue>()));

        #endregion

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

            // Cascade to list items if they implement IChangeTracking
            if (Values != null)
            {
                foreach (var value in Values.OfType<IChangeTracking>())
                {
                    value?.ResetChangeTracking();
                }
            }
        }

        #region Equality

        public static bool operator ==(PropertyValues left, PropertyValues right)
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

        public static bool operator !=(PropertyValues left, PropertyValues right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PropertyValues);
        }

        public bool Equals(PropertyValues other)
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
                LinkedObjectID.Equals(other.LinkedObjectID) &&
                string.Equals(Scope, other.Scope, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(SubID, other.SubID, StringComparison.OrdinalIgnoreCase) &&
                ListsEqual(Values, other.Values);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + LinkedObjectID.GetHashCode();
                hash = (hash * 23) + (Scope != null ? Scope.GetHashCode() : 0);
                hash = (hash * 23) + (SubID != null ? SubID.GetHashCode() : 0);
                return hash;
            }
        }

        private static bool ListsEqual<T>(List<T> left, List<T> right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left is null || right is null)
            {
                return false;
            }

            return left.SequenceEqual(right);
        }

        #endregion
    }
}