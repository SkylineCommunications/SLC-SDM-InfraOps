namespace Skyline.DataMiner.SDM.PlanAndBuild.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Newtonsoft.Json;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    //[GenerateExposers]
    //[SdmDomStorage("(slc)plan_and_build")]
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
        public bool Changed =>
            FieldHandler.HasChanges ||
            StateField?.Changed == true ||
            Ownership?.Changed == true ||
            AssetsUsedField?.Changed == true ||
            AttachmentsField?.Changed == true ||
            ConnectionsOnJobField?.Changed == true;

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
        [SdmIgnore]
        public SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum State
        {
            get => StateField.Value; internal set => StateField.Value = value;
        }

        #region Ownership Properties

        public JobOwnership Ownership { get; set; } = new JobOwnership();

        #endregion

        #region Info Properties

        /// <summary>
        /// Gets the system-generated Job ID.
        /// </summary>
        /// <remarks>
        /// This is allocated once at creation time by <see cref="Skyline.DataMiner.SDM.PlanAndBuild.Helpers.JobIdAllocator"/>
        /// (mirroring the legacy JobIdManager) and must never be changed afterwards. The setter is internal so that
        /// only the allocator and the DOM mapper (both within this assembly) can assign it.
        /// </remarks>
        public string JobID
        {
            get => JobIDField.Value;
            internal set => JobIDField.Value = value;
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

        /// <summary>
        /// Soft-deleted field, mirroring legacy's <c>JobtypeEnum</c>. Superseded by <see cref="Type"/>
        /// (a DOM reference to the JobType definition). Get-only to match legacy: legacy's equivalent
        /// property has no setter, and its DOM persistence path never writes this field back. Kept only
        /// for backward-compat reads of old data; not written by any business logic.
        /// </summary>
        /// <summary>
        /// Soft-deleted field, mirroring legacy's <c>JobtypeEnum</c>. Superseded by <see cref="Type"/>
        /// (a DOM reference to the JobType definition). Setter is internal to match legacy: legacy's
        /// wrapper-facing equivalent property is get-only, and its DOM persistence path never writes this
        /// field back. The internal setter exists only so the repository can populate it from old DOM data
        /// on read; not written by any business logic.
        /// </summary>
        [Obsolete("Soft deleted field. Replaced with 'Type'.")]
        public SlcPlan_And_Build.Enums.JobtypeEnum JobType
        {
            get => JobTypeField.Value;
            internal set => JobTypeField.Value = value;
        }

        public SdmObjectReference<JobType> Type
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

        /// <summary>
        /// Gets or sets the DOM instance identifiers (Facility, Floor, Room, Zone, Row, Desk or Rack) linked to this job.
        /// </summary>
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

        public List<JobAttachment> Attachments
        {
            get => AttachmentsField.Value ?? new List<JobAttachment>();
            set => AttachmentsField.Value = value;
        }

        public List<JobConnection> ConnectionsOnJob
        {
            get => ConnectionsOnJobField.Value ?? new List<JobConnection>();
            set => ConnectionsOnJobField.Value = value;
        }

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
            () => new ChangeTrackingField<DateTime?>(DateTime.UtcNow));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<DateTime?> EndField => FieldHandler.GetOrCreateField(
            nameof(End),
            () => new ChangeTrackingField<DateTime?>(null));

        [JsonIgnore]
        [SdmIgnore]
        [Obsolete("Soft deleted field. Replaced with 'TypeField'.")]
        internal IChangeTrackingField<SlcPlan_And_Build.Enums.JobtypeEnum> JobTypeField => FieldHandler.GetOrCreateField(
            nameof(JobType),
            () => new ChangeTrackingField<SlcPlan_And_Build.Enums.JobtypeEnum>(SlcPlan_And_Build.Enums.JobtypeEnum.Add));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<SdmObjectReference<JobType>> TypeField => FieldHandler.GetOrCreateField(
            nameof(Type),
            () => new ChangeTrackingField<SdmObjectReference<JobType>>(default));

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
            () => new ChangeTrackingField<SlcPlan_And_Build.Enums.SubStateEnum?>(SlcPlan_And_Build.Enums.SubStateEnum.Draft));

        [JsonIgnore]
        [SdmIgnore]
        internal ChangeTrackingArrayField<Guid> LocationsField => FieldHandler.GetOrCreateArrayField(
            nameof(Locations),
            () => new ChangeTrackingArrayField<Guid>(new List<Guid>(), list => list.Cast<object>().ToList()));

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

        [JsonIgnore]
        [SdmIgnore]
        internal ChangeTrackingArrayField<JobAttachment> AttachmentsField => FieldHandler.GetOrCreateArrayField(
            nameof(Attachments),
            () => new ChangeTrackingArrayField<JobAttachment>(new List<JobAttachment>()));

        [JsonIgnore]
        [SdmIgnore]
        internal ChangeTrackingArrayField<JobConnection> ConnectionsOnJobField => FieldHandler.GetOrCreateArrayField(
            nameof(ConnectionsOnJob),
            () => new ChangeTrackingArrayField<JobConnection>(new List<JobConnection>()));

        #endregion

        public void ResetChangeTracking()
        {
            FieldHandler?.ApplyChanges();
            Ownership?.ResetChangeTracking();

            // Cascade to list items if they implement IChangeTracking
            if (AssetsUsed != null)
            {
                foreach (var jobAsset in AssetsUsed.OfType<IChangeTracking>())
                {
                    jobAsset?.ResetChangeTracking();
                }
            }

            if (Attachments != null)
            {
                foreach (var attachment in Attachments.OfType<IChangeTracking>())
                {
                    attachment?.ResetChangeTracking();
                }
            }

            if (ConnectionsOnJob != null)
            {
                foreach (var connection in ConnectionsOnJob.OfType<IChangeTracking>())
                {
                    connection?.ResetChangeTracking();
                }
            }
        }
    }
}
