namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    //[GenerateExposers]
    //[SdmDomStorage("(slc)facility_management")]
    public sealed class Zone : SdmObject<Zone>, IEquatable<Zone>, IEntityTracking
    {
        [JsonIgnore]
        private ChangeTrackingFieldHandler _fieldHandler;
        [JsonIgnore]
        private bool _isNew = true;

        public Zone()
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
        public bool Changed =>
            FieldHandler.HasChanges ||
            _zoneCapacity?.Changed == true ||
            _roomFk?.Changed == true ||
            _resource?.Changed == true;

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
        internal Guid? ZonePropertiesSectionId { get; set; }

        #endregion

        public string Name
        {
            get => NameField.Value;
            set => NameField.Value = value;
        }

        public string Plan
        {
            get => PlanField.Value;
            set => PlanField.Value = value;
        }

        public string Description
        {
            get => DescriptionField.Value;
            set => DescriptionField.Value = value;
        }

        public SlcFacility_Management.Enums.ThermalType? ThermalType
        {
            get => ThermalTypeField.Value;
            set => ThermalTypeField.Value = value;
        }

        public double? XPosition
        {
            get => XPositionField.Value;
            set => XPositionField.Value = value;
        }

        public double? YPosition
        {
            get => YPositionField.Value;
            set => YPositionField.Value = value;
        }

        public double? Width
        {
            get => WidthField.Value;
            set => WidthField.Value = value;
        }

        public double? Depth
        {
            get => DepthField.Value;
            set => DepthField.Value = value;
        }

        public string ZoneId
        {
            get => ZoneIdField.Value;
            set => ZoneIdField.Value = value;
        }

        private ZoneCapacity _zoneCapacity;

        public ZoneCapacity ZoneCapacity => _zoneCapacity ?? (_zoneCapacity = new ZoneCapacity());

        private RoomRelation _roomFk;

        public RoomRelation RoomFk => _roomFk ?? (_roomFk = new RoomRelation());

        private ResourceLink _resource;

        public ResourceLink Resource => _resource ?? (_resource = new ResourceLink());

        [SdmIgnore]
        public SlcFacility_Management.Behaviors.Zone_Behaviour.StatusesEnum State { get; internal set; }

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> NameField => FieldHandler.GetOrCreateField(
            nameof(Name), () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> PlanField => FieldHandler.GetOrCreateField(
            nameof(Plan), () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> DescriptionField => FieldHandler.GetOrCreateField(
            nameof(Description), () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        internal IChangeTrackingField<SlcFacility_Management.Enums.ThermalType?> ThermalTypeField => FieldHandler.GetOrCreateField(
            nameof(ThermalType), () => new ChangeTrackingField<SlcFacility_Management.Enums.ThermalType?>(null));

        [JsonIgnore]
        internal IChangeTrackingField<double?> XPositionField => FieldHandler.GetOrCreateField(
            nameof(XPosition), () => new ChangeTrackingField<double?>(null));

        [JsonIgnore]
        internal IChangeTrackingField<double?> YPositionField => FieldHandler.GetOrCreateField(
            nameof(YPosition), () => new ChangeTrackingField<double?>(null));

        [JsonIgnore]
        internal IChangeTrackingField<double?> WidthField => FieldHandler.GetOrCreateField(
            nameof(Width), () => new ChangeTrackingField<double?>(null));

        [JsonIgnore]
        internal IChangeTrackingField<double?> DepthField => FieldHandler.GetOrCreateField(
            nameof(Depth), () => new ChangeTrackingField<double?>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> ZoneIdField => FieldHandler.GetOrCreateField(
            nameof(ZoneId), () => new ChangeTrackingStringField(null));

        public IEnumerable<TrackingFieldValueDifference> GetChanges()
        {
            return FieldHandler.GetChanges()
                .Select(kvp => new TrackingFieldValueDifference
                {
                    FieldName = kvp.Key,
                    OldValue = kvp.Value.prevVal,
                    NewValue = kvp.Value.newVal,
                })
                .Concat(_zoneCapacity?.GetChanges() ?? Enumerable.Empty<TrackingFieldValueDifference>())
                .Concat(_roomFk?.GetChanges() ?? Enumerable.Empty<TrackingFieldValueDifference>())
                .Concat(_resource?.GetChanges() ?? Enumerable.Empty<TrackingFieldValueDifference>());
        }

        public void ResetChangeTracking()
        {
            FieldHandler.ApplyChanges();
            _zoneCapacity?.ResetChangeTracking();
            _roomFk?.ResetChangeTracking();
            _resource?.ResetChangeTracking();
        }

        #region Equality

        public static bool operator ==(Zone left, Zone right)
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

        public static bool operator !=(Zone left, Zone right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Zone);
        }

        public bool Equals(Zone other)
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
                string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(Plan, other.Plan, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(Description, other.Description, StringComparison.OrdinalIgnoreCase) &&
                ThermalType == other.ThermalType &&
                XPosition == other.XPosition &&
                YPosition == other.YPosition &&
                Width == other.Width &&
                Depth == other.Depth &&
                string.Equals(ZoneId, other.ZoneId, StringComparison.OrdinalIgnoreCase) &&
                Equals(ZoneCapacity, other.ZoneCapacity) &&
                Equals(RoomFk, other.RoomFk) &&
                Equals(Resource, other.Resource) &&
                State == other.State;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + (Name != null ? Name.GetHashCode() : 0);
                hash = (hash * 23) + (Plan != null ? Plan.GetHashCode() : 0);
                hash = (hash * 23) + (Description != null ? Description.GetHashCode() : 0);
                hash = (hash * 23) + (ZoneId != null ? ZoneId.GetHashCode() : 0);
                hash = (hash * 23) + (ZoneCapacity != null ? ZoneCapacity.GetHashCode() : 0);
                hash = (hash * 23) + (RoomFk != null ? RoomFk.GetHashCode() : 0);
                hash = (hash * 23) + State.GetHashCode();
                return hash;
            }
        }

        #endregion
    }
}