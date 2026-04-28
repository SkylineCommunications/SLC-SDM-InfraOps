namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System.Runtime.Serialization;

    using Newtonsoft.Json;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    //[GenerateExposers]
    //[SdmDomStorage("(slc)asset_management")]
    public class DeviceType : SdmObject<DeviceType>
    {
        [JsonIgnore]
        private ChangeTrackingFieldHandler _fieldHandler;

        public DeviceType()
        {
            _fieldHandler = new ChangeTrackingFieldHandler();
        }

        // Ensure _fieldHandler is always initialized (handles JSON deserialization without constructor)
        [JsonIgnore]
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

        // Called after JSON deserialization to reset change tracking
        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            ResetChangeTracking();
        }

        // PUBLIC API: Simple types (consumers see these)
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
            get => TagsInfoField.Value ?? new TagsInfo();
            set => TagsInfoField.Value = value;
        }

        public HierarchyInfo HierarchyInfo
        {
            get => HierarchyInfoField.Value ?? new HierarchyInfo();
            set => HierarchyInfoField.Value = value;
        }

        // INTERNAL: Change tracking fields (validation handler uses these)
        [JsonIgnore]
        internal IChangeTrackingField<string> NameField => FieldHandler.GetOrCreateField(
            nameof(Name),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        internal IChangeTrackingField<string> DescriptionField => FieldHandler.GetOrCreateField(
            nameof(Description),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        internal IChangeTrackingField<TagsInfo> TagsInfoField => FieldHandler.GetOrCreateField(
            nameof(TagsInfo),
            () => new ChangeTrackingField<TagsInfo>(new TagsInfo()));

        [JsonIgnore]
        internal IChangeTrackingField<HierarchyInfo> HierarchyInfoField => FieldHandler.GetOrCreateField(
            nameof(HierarchyInfo),
            () => new ChangeTrackingField<HierarchyInfo>(new HierarchyInfo()));

        public void ResetChangeTracking()
        {
            _fieldHandler?.ApplyChanges();
        }
    }
}