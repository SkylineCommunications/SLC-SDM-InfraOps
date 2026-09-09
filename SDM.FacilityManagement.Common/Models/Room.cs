namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    using System;

    using Newtonsoft.Json;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    //[GenerateExposers]
    //[SdmDomStorage("(slc)facility_management")]
    public sealed class Room : SdmObject<Room>, IEquatable<Room>, IEntityTracking
    {
        [JsonIgnore]
        private ChangeTrackingFieldHandler _fieldHandler;
        [JsonIgnore]
        private bool _isNew = true;

        public Room()
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
            _ownership?.Changed == true ||
            _resourceLink?.Changed == true ||
            _floorFk?.Changed == true;

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
        internal Guid? RoomPropertiesSectionId { get; set; }

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

        public long? Width
        {
            get => WidthField.Value;
            set => WidthField.Value = value;
        }

        public long? Depth
        {
            get => DepthField.Value;
            set => DepthField.Value = value;
        }

        public string RoomId
        {
            get => RoomIdField.Value;
            set => RoomIdField.Value = value;
        }

        private RoomOwnership _ownership;

        public RoomOwnership Ownership => _ownership ?? (_ownership = new RoomOwnership());

        private ResourceLink _resourceLink;

        public ResourceLink ResourceLink => _resourceLink ?? (_resourceLink = new ResourceLink());

        private FloorRelation _floorFk;

        public FloorRelation FloorFk => _floorFk ?? (_floorFk = new FloorRelation());

        [SdmIgnore]
        public SlcFacility_Management.Behaviors.Room_Behaviour.StatusesEnum State { get; internal set; }

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
        internal IChangeTrackingField<long?> WidthField => FieldHandler.GetOrCreateField(
            nameof(Width), () => new ChangeTrackingField<long?>(null));

        [JsonIgnore]
        internal IChangeTrackingField<long?> DepthField => FieldHandler.GetOrCreateField(
            nameof(Depth), () => new ChangeTrackingField<long?>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> RoomIdField => FieldHandler.GetOrCreateField(
            nameof(RoomId), () => new ChangeTrackingStringField(null));

        public void ResetChangeTracking()
        {
            FieldHandler.ApplyChanges();
            _ownership?.ResetChangeTracking();
            _resourceLink?.ResetChangeTracking();
            _floorFk?.ResetChangeTracking();
        }

        #region Equality

        public static bool operator ==(Room left, Room right)
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

        public static bool operator !=(Room left, Room right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Room);
        }

        public bool Equals(Room other)
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
                Width == other.Width &&
                Depth == other.Depth &&
                string.Equals(RoomId, other.RoomId, StringComparison.OrdinalIgnoreCase) &&
                Equals(Ownership, other.Ownership) &&
                Equals(ResourceLink, other.ResourceLink) &&
                Equals(FloorFk, other.FloorFk) &&
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
                hash = (hash * 23) + (RoomId != null ? RoomId.GetHashCode() : 0);
                hash = (hash * 23) + (Ownership != null ? Ownership.GetHashCode() : 0);
                hash = (hash * 23) + (FloorFk != null ? FloorFk.GetHashCode() : 0);
                hash = (hash * 23) + State.GetHashCode();
                return hash;
            }
        }

        #endregion
    }
}