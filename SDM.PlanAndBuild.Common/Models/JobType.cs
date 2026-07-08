namespace Skyline.DataMiner.SDM.PlanAndBuild.Models
{
    using Newtonsoft.Json;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    //[GenerateExposers]
    //[SdmDomStorage("(slc)plan_and_build")]
    public class JobType : SdmObject<JobType>, IEntityTracking
    {
        [JsonIgnore]
        private ChangeTrackingFieldHandler _fieldHandler;
        [JsonIgnore]
        private bool _isNew = true;

        public JobType()
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
        public bool Changed => FieldHandler.HasChanges;

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

        public string Icon
        {
            get => IconField.Value;
            set => IconField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> NameField => FieldHandler.GetOrCreateField(
            nameof(Name),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> DescriptionField => FieldHandler.GetOrCreateField(
            nameof(Description),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> IconField => FieldHandler.GetOrCreateField(
            nameof(Icon),
            () => new ChangeTrackingStringField(null));

        public void ResetChangeTracking()
        {
            FieldHandler?.ApplyChanges();
        }
    }
}
