namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    using System;

    using Newtonsoft.Json;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    //[GenerateExposers]
    //[SdmDomStorage("(slc)facility_management")]
    public sealed class Row : SdmObject<Row>, IEquatable<Row>, IEntityTracking
    {
        [JsonIgnore]
        private ChangeTrackingFieldHandler _fieldHandler;
        [JsonIgnore]
        private bool _isNew = true;

        public Row()
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
        internal Guid? RowPropertiesSectionId { get; set; }

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

        public string Label
        {
            get => LabelField.Value;
            set => LabelField.Value = value;
        }

        public string RowId
        {
            get => RowIdField.Value;
            set => RowIdField.Value = value;
        }

        public double? YPosition
        {
            get => YPositionField.Value;
            set => YPositionField.Value = value;
        }

        private RoomRelation _roomFk;

        public RoomRelation RoomFk => _roomFk ?? (_roomFk = new RoomRelation());

        private ResourceLink _resource;

        public ResourceLink Resource => _resource ?? (_resource = new ResourceLink());

        [SdmIgnore]
        public SlcFacility_Management.Behaviors.Row_Behaviour.StatusesEnum State { get; internal set; }

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
        [SdmIgnore]
        internal IChangeTrackingField<string> LabelField => FieldHandler.GetOrCreateField(
            nameof(Label), () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> RowIdField => FieldHandler.GetOrCreateField(
            nameof(RowId), () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        internal IChangeTrackingField<double?> YPositionField => FieldHandler.GetOrCreateField(
            nameof(YPosition), () => new ChangeTrackingField<double?>(null));

        public void ResetChangeTracking()
        {
            FieldHandler.ApplyChanges();
            _roomFk?.ResetChangeTracking();
            _resource?.ResetChangeTracking();
        }

        #region Equality

        public static bool operator ==(Row left, Row right)
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

        public static bool operator !=(Row left, Row right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Row);
        }

        public bool Equals(Row other)
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
                string.Equals(Label, other.Label, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(RowId, other.RowId, StringComparison.OrdinalIgnoreCase) &&
                YPosition == other.YPosition &&
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
                hash = (hash * 23) + (Label != null ? Label.GetHashCode() : 0);
                hash = (hash * 23) + (RowId != null ? RowId.GetHashCode() : 0);
                hash = (hash * 23) + (RoomFk != null ? RoomFk.GetHashCode() : 0);
                hash = (hash * 23) + State.GetHashCode();
                return hash;
            }
        }

        #endregion
    }
}