namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    using Newtonsoft.Json;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public class TagsInfo : SdmObject<TagsInfo>
    {
        [JsonIgnore]
        private ChangeTrackingFieldHandler _fieldHandler;

        public TagsInfo()
        {
            _fieldHandler = new ChangeTrackingFieldHandler();
        }

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

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            ResetChangeTracking();
        }

        public List<SlcAsset_Management.Enums.TagOption> Tags
        {
            get => TagsField.Value ?? new List<SlcAsset_Management.Enums.TagOption>();
            set => TagsField.Value = value;
        }

        [JsonIgnore]
        internal ChangeTrackingArrayField<SlcAsset_Management.Enums.TagOption> TagsField => FieldHandler.GetOrCreateArrayField(
            nameof(Tags),
            () => new ChangeTrackingArrayField<SlcAsset_Management.Enums.TagOption>(new List<SlcAsset_Management.Enums.TagOption>()));

        public void ResetChangeTracking()
        {
            _fieldHandler?.ApplyChanges();
        }
    }
}