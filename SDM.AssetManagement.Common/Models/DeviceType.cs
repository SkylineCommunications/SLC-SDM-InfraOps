namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using Newtonsoft.Json;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;
    using System;

    //[GenerateExposers]
    //[SdmDomStorage("(slc)asset_management")]
    public class DeviceType : SdmObject<DeviceType>, IEntityTracking
    {
        [JsonIgnore]
        private ChangeTrackingFieldHandler _fieldHandler;
        [JsonIgnore]
        private TagsInfo _tagsInfo;
        [JsonIgnore]
        private HierarchyInfo _hierarchyInfo;
        [JsonIgnore]
        private bool _isNew = true;

        public DeviceType()
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
        public bool Changed =>
            FieldHandler.HasChanges ||
            _tagsInfo?.Changed == true ||
            _hierarchyInfo?.Changed == true;

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

        public TagsInfo TagsInfo
        {
            get => _tagsInfo ?? (_tagsInfo = new TagsInfo());
            set => _tagsInfo = value ?? new TagsInfo();
        }

        public HierarchyInfo HierarchyInfo
        {
            get => _hierarchyInfo ?? (_hierarchyInfo = new HierarchyInfo());
            set => _hierarchyInfo = value ?? new HierarchyInfo();
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

        public void ResetChangeTracking()
        {
            FieldHandler?.ApplyChanges();
            _tagsInfo?.ResetChangeTracking();
            _hierarchyInfo?.ResetChangeTracking();
        }

        #region Section Tracking

        [JsonIgnore]
        [SdmIgnore]
        internal Guid? DeviceTypePropertiesSectionId { get; set; }

        #endregion

    }
}