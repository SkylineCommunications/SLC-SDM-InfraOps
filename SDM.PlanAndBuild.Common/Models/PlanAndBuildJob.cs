namespace Skyline.DataMiner.SDM.PlanAndBuild.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Newtonsoft.Json;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    [GenerateExposers]
    [SdmDomStorage("(slc)plan_and_build")]
    public class PlanAndBuildJob : SdmObject<PlanAndBuildJob>, IEntityTracking
    {
        [JsonIgnore]
        private ChangeTrackingFieldHandler _fieldHandler;
        [JsonIgnore]
        private bool _isNew = true;

        public PlanAndBuildJob()
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
        public Guid Id { get; set; }

        [JsonIgnore]
        [SdmIgnore]
        public bool Changed =>
            FieldHandler.HasChanges ||
            StateField?.Changed == true ||
            AssetsUsedField?.Changed == true;

        /// <summary>
        /// Gets a value indicating whether the current object has not been assigned an identifier.
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

        /// <summary>
        /// Gets or sets the current status of the job.
        /// </summary>
        public SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum State
        {
            get => StateField.Value; internal set => StateField.Value = value;
        }

        #region Ownership Properties

        public Guid? AssignedTo
        {
            get => AssignedToField.Value;
            set => AssignedToField.Value = value;
        }

        public Guid? AssignmentGroup
        {
            get => AssignmentGroupField.Value;
            set => AssignmentGroupField.Value = value;
        }

        #endregion

        #region Info Properties

        public string JobID
        {
            get => JobIDField.Value;
            set => JobIDField.Value = value;
        }

        public string JobName
        {
            get => JobNameField.Value;
            set => JobNameField.Value = value;
        }

        public DateTime? Start
        {
            get => StartField.Value;
            set => StartField.Value = value;
        }

        public DateTime? End
        {
            get => EndField.Value;
            set => EndField.Value = value;
        }

        public SdmObjectReference<JobType> JobType
        {
            get => JobTypeField.Value;
            set => JobTypeField.Value = value;
        }

        public SlcPlan_And_Build.Enums.JobtypeEnum Type
        {
            get => TypeField.Value;
            set => TypeField.Value = value;
        }

        public string JobDescription
        {
            get => JobDescriptionField.Value;
            set => JobDescriptionField.Value = value;
        }

        public string Remarks
        {
            get => RemarksField.Value;
            set => RemarksField.Value = value;
        }

        public SlcPlan_And_Build.Enums.PriorityEnum Priority
        {
            get => PriorityField.Value;
            set => PriorityField.Value = value;
        }

        public SlcPlan_And_Build.Enums.SubStateEnum? SubState
        {
            get => SubStateField.Value;
            set => SubStateField.Value = value;
        }

        public List<Guid> Locations
        {
            get => LocationsField.Value ?? new List<Guid>();
            set => LocationsField.Value = value;
        }

        #endregion

        #region Collection Properties

        public List<JobAsset> AssetsUsed
        {
            get => AssetsUsedField.Value ?? new List<JobAsset>();
            set => AssetsUsedField.Value = value;
        }

        #endregion

        #region Ownership Tracking Fields

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<Guid?> AssignedToField => FieldHandler.GetOrCreateField(
            nameof(AssignedTo),
            () => new ChangeTrackingField<Guid?>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<Guid?> AssignmentGroupField => FieldHandler.GetOrCreateField(
            nameof(AssignmentGroup),
            () => new ChangeTrackingField<Guid?>(null));

        #endregion

        #region Info Tracking Fields

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> JobIDField => FieldHandler.GetOrCreateField(
            nameof(JobID),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> JobNameField => FieldHandler.GetOrCreateField(
            nameof(JobName),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<DateTime?> StartField => FieldHandler.GetOrCreateField(
            nameof(Start),
            () => new ChangeTrackingField<DateTime?>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<DateTime?> EndField => FieldHandler.GetOrCreateField(
            nameof(End),
            () => new ChangeTrackingField<DateTime?>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<SdmObjectReference<JobType>> JobTypeField => FieldHandler.GetOrCreateField(
            nameof(JobType),
            () => new ChangeTrackingField<SdmObjectReference<JobType>>(default));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<SlcPlan_And_Build.Enums.JobtypeEnum> TypeField => FieldHandler.GetOrCreateField(
            nameof(Type),
            () => new ChangeTrackingField<SlcPlan_And_Build.Enums.JobtypeEnum>(SlcPlan_And_Build.Enums.JobtypeEnum.Add));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> JobDescriptionField => FieldHandler.GetOrCreateField(
            nameof(JobDescription),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> RemarksField => FieldHandler.GetOrCreateField(
            nameof(Remarks),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<SlcPlan_And_Build.Enums.PriorityEnum> PriorityField => FieldHandler.GetOrCreateField(
            nameof(Priority),
            () => new ChangeTrackingField<SlcPlan_And_Build.Enums.PriorityEnum>(SlcPlan_And_Build.Enums.PriorityEnum.Normal));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<SlcPlan_And_Build.Enums.SubStateEnum?> SubStateField => FieldHandler.GetOrCreateField(
            nameof(SubState),
            () => new ChangeTrackingField<SlcPlan_And_Build.Enums.SubStateEnum?>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal ChangeTrackingArrayField<Guid> LocationsField => FieldHandler.GetOrCreateArrayField(
            nameof(Locations),
            () => new ChangeTrackingArrayField<Guid>(new List<Guid>()));

        #endregion

        #region State Tracking Field

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum> StateField => FieldHandler.GetOrCreateField(
            nameof(State),
            () => new ChangeTrackingField<SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum>(SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum.New));

        #endregion

        #region Collection Tracking Fields

        [JsonIgnore]
        [SdmIgnore]
        internal ChangeTrackingArrayField<JobAsset> AssetsUsedField => FieldHandler.GetOrCreateArrayField(
            nameof(AssetsUsed),
            () => new ChangeTrackingArrayField<JobAsset>(new List<JobAsset>()));

        #endregion

        public void ResetChangeTracking()
        {
            FieldHandler?.ApplyChanges();

            // Cascade to list items if they implement IChangeTracking
            if (AssetsUsed != null)
            {
                foreach (var jobAsset in AssetsUsed.OfType<IChangeTracking>())
                {
                    jobAsset?.ResetChangeTracking();
                }
            }
        }
    }
}
