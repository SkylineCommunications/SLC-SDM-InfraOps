namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using SharedMappers.DomIds;
    using Skyline.DataMiner.SDM.InfraOps.Core.Models;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    //[GenerateExposers]
    //[SdmDomStorage("(slc)asset_management")]
    public sealed class Connection : SdmObjectBase<Connection>, IEquatable<Connection>, IEntityTracking
    {
        [JsonIgnore]
        private ChangeTrackingFieldHandler _fieldHandler;
        [JsonIgnore]
        private SourceInfo _source;
        [JsonIgnore]
        private DestinationInfo _destination;
        [JsonIgnore]
        private bool _isNew = true;

        public Connection()
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
        public bool Changed =>
            FieldHandler.HasChanges ||
            _source?.Changed == true ||
            _destination?.Changed == true;

        public string Notes
        {
            get => NotesField.Value;
            set => NotesField.Value = value;
        }

        public string Description
        {
            get => DescriptionField.Value;
            set => DescriptionField.Value = value;
        }

        public SlcAsset_Management.Enums.ConnectionType? ConnectionType
        {
            get => ConnectionTypeField.Value;
            set => ConnectionTypeField.Value = value;
        }

        public SdmObjectReference<CableType> CableType
        {
            get => CableTypeField.Value;
            set => CableTypeField.Value = value;
        }

        /// <summary>
        /// Gets or sets the length of the cable in meters.
        /// </summary>
        public double? CableLength
        {
            get => CableLengthField.Value;
            set => CableLengthField.Value = value;
        }

        public SourceInfo Source => _source ?? (_source = new SourceInfo());

        public DestinationInfo Destination => _destination ?? (_destination = new DestinationInfo());

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> NotesField => FieldHandler.GetOrCreateField(
            nameof(Notes),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> DescriptionField => FieldHandler.GetOrCreateField(
            nameof(Description),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<SlcAsset_Management.Enums.ConnectionType?> ConnectionTypeField => FieldHandler.GetOrCreateField(
            nameof(ConnectionType),
            () => new ChangeTrackingField<SlcAsset_Management.Enums.ConnectionType?>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<SdmObjectReference<CableType>> CableTypeField => FieldHandler.GetOrCreateField(
            nameof(CableType),
            () => new ChangeTrackingField<SdmObjectReference<CableType>>(default));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<double?> CableLengthField => FieldHandler.GetOrCreateField(
            nameof(CableLength),
            () => new ChangeTrackingField<double?>(null));

        public IEnumerable<TrackingFieldValueDifference> GetChanges()
        {
            return FieldHandler.GetChanges()
                .Select(kvp => new TrackingFieldValueDifference
                {
                    FieldName = kvp.Key,
                    OldValue = kvp.Value.prevVal,
                    NewValue = kvp.Value.newVal,
                })
                .Concat(_source?.GetChanges() ?? Enumerable.Empty<TrackingFieldValueDifference>())
                .Concat(_destination?.GetChanges() ?? Enumerable.Empty<TrackingFieldValueDifference>());
        }

        public void ResetChangeTracking()
        {
            FieldHandler?.ApplyChanges();
            _source?.ResetChangeTracking();
            _destination?.ResetChangeTracking();
        }

        public static bool operator ==(Connection left, Connection right)
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

        public static bool operator !=(Connection left, Connection right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Connection);
        }

        public bool Equals(Connection other)
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
                string.Equals(Notes, other.Notes, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(Description, other.Description, StringComparison.OrdinalIgnoreCase) &&
                ConnectionType == other.ConnectionType &&
                CableType == other.CableType &&
                CableLength == other.CableLength &&
                Equals(Source, other.Source) &&
                Equals(Destination, other.Destination);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + (Notes != null ? Notes.GetHashCode() : 0);
                hash = (hash * 23) + (Description != null ? Description.GetHashCode() : 0);
                hash = (hash * 23) + ConnectionType.GetHashCode();
                hash = (hash * 23) + (CableType != null ? CableType.GetHashCode() : 0);
                hash = (hash * 23) + CableLength.GetHashCode();
                hash = (hash * 23) + (Source != null ? Source.GetHashCode() : 0);
                hash = (hash * 23) + (Destination != null ? Destination.GetHashCode() : 0);
                return hash;
            }
        }

        #region Section Tracking

        [JsonIgnore]
        [SdmIgnore]
        internal Guid? ConnectionPropertiesSectionId { get; set; }

        [JsonIgnore]
        [SdmIgnore]
        internal Guid? CableInformationSectionId { get; set; }

        #endregion

    }
}
