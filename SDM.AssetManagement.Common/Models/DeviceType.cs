namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using Newtonsoft.Json;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;
    using System;

    //[GenerateExposers]
    //[SdmDomStorage("(slc)asset_management")]
    public sealed class DeviceType : SdmObject<DeviceType>, IEquatable<DeviceType>, IEntityTracking
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

        public TagsInfo TagsInfo => _tagsInfo ?? (_tagsInfo = new TagsInfo());

        public HierarchyInfo HierarchyInfo => _hierarchyInfo ?? (_hierarchyInfo = new HierarchyInfo());

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

        public static bool operator ==(DeviceType left, DeviceType right)
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

        public static bool operator !=(DeviceType left, DeviceType right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DeviceType);
        }

        public bool Equals(DeviceType other)
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
                string.Equals(Description, other.Description, StringComparison.OrdinalIgnoreCase) &&
                Equals(TagsInfo, other.TagsInfo) &&
                Equals(HierarchyInfo, other.HierarchyInfo);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + (Name != null ? Name.GetHashCode() : 0);
                hash = (hash * 23) + (Description != null ? Description.GetHashCode() : 0);
                hash = (hash * 23) + (TagsInfo != null ? TagsInfo.GetHashCode() : 0);
                hash = (hash * 23) + (HierarchyInfo != null ? HierarchyInfo.GetHashCode() : 0);
                return hash;
            }
        }

        #region Section Tracking

        [JsonIgnore]
        [SdmIgnore]
        internal Guid? DeviceTypePropertiesSectionId { get; set; }

        #endregion

    }
}