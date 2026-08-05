namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    using Newtonsoft.Json;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    //[GenerateExposers]
    //[SdmDomStorage("(slc)facility_management")]
    public class Zone : SdmObject<Zone>, IEntityTracking
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

        public ZoneCapacity ZoneCapacity { get; set; }

        public RoomRelation RoomFk { get; set; }

        public ResourceLink Resource { get; set; }

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

        public void ResetChangeTracking()
        {
            FieldHandler.ApplyChanges();
        }
    }
}
