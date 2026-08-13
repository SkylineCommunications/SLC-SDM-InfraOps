namespace Skyline.DataMiner.SDM.PlanAndBuild.Models
{
    using System;

    using Newtonsoft.Json;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    //[GenerateExposers]
    //[SdmDomStorage("(slc)plan_and_build")]
    public class PlanAndBuildAppSettings : SdmObject<PlanAndBuildAppSettings>, IEntityTracking
    {
        [JsonIgnore]
        private ChangeTrackingFieldHandler _fieldHandler;
        [JsonIgnore]
        private bool _isNew = true;

        public PlanAndBuildAppSettings()
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

        public string JobIDPrefix
        {
            get => JobIDPrefixField.Value;
            set => JobIDPrefixField.Value = value;
        }

        public long JobIDNextSequence
        {
            get => JobIDNextSequenceField.Value;
            set => JobIDNextSequenceField.Value = value;
        }

        public long JobIDIncrement
        {
            get => JobIDIncrementField.Value;
            set => JobIDIncrementField.Value = value;
        }

        public long JobIDStartingSeed
        {
            get => JobIDStartingSeedField.Value;
            set => JobIDStartingSeedField.Value = value;
        }

        public long JobIDMinimumDigits
        {
            get => JobIDMinimumDigitsField.Value;
            set => JobIDMinimumDigitsField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> JobIDPrefixField => FieldHandler.GetOrCreateField(
            nameof(JobIDPrefix),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<long> JobIDNextSequenceField => FieldHandler.GetOrCreateField(
            nameof(JobIDNextSequence),
            () => new ChangeTrackingField<long>(0));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<long> JobIDIncrementField => FieldHandler.GetOrCreateField(
            nameof(JobIDIncrement),
            () => new ChangeTrackingField<long>(0));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<long> JobIDStartingSeedField => FieldHandler.GetOrCreateField(
            nameof(JobIDStartingSeed),
            () => new ChangeTrackingField<long>(0));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<long> JobIDMinimumDigitsField => FieldHandler.GetOrCreateField(
            nameof(JobIDMinimumDigits),
            () => new ChangeTrackingField<long>(0));

        #region Section Tracking

        [JsonIgnore]
        [SdmIgnore]
        internal Guid? PlanAndBuildAppSettingsPropertiesSectionId { get; set; }

        #endregion

        public void ResetChangeTracking()
        {
            FieldHandler?.ApplyChanges();
        }
    }
}
