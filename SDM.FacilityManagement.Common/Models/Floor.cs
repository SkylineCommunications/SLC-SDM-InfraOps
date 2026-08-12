namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    using System;

    using Newtonsoft.Json;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    //[GenerateExposers]
    //[SdmDomStorage("(slc)facility_management")]
    public class Floor : SdmObject<Floor>, IEntityTracking
    {
        [JsonIgnore]
        private ChangeTrackingFieldHandler _fieldHandler;
        [JsonIgnore]
        private bool _isNew = true;

        public Floor()
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
        internal Guid? FloorPropertiesSectionId { get; set; }

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

        public string FloorId
        {
            get => FloorIdField.Value;
            set => FloorIdField.Value = value;
        }

        private FacilityRelation _facilityFk;

        public FacilityRelation FacilityFk => _facilityFk ?? (_facilityFk = new FacilityRelation());

        [SdmIgnore]
        public SlcFacility_Management.Behaviors.Floor_Behaviour.StatusesEnum State { get; internal set; }

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
        internal IChangeTrackingField<string> FloorIdField => FieldHandler.GetOrCreateField(
            nameof(FloorId), () => new ChangeTrackingStringField(null));

        public void ResetChangeTracking()
        {
            FieldHandler.ApplyChanges();
        }
    }
}
