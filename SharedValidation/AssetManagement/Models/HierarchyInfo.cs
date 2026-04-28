namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System.Runtime.Serialization;

    using Newtonsoft.Json;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public class HierarchyInfo : SdmObject<HierarchyInfo>
    {
        [JsonIgnore]
        private ChangeTrackingFieldHandler _fieldHandler;

        public HierarchyInfo()
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

        public SlcAsset_Management.Enums.HierarchyRoleEnum HierarchyRole
        {
            get => HierarchyRoleField.Value;
            set => HierarchyRoleField.Value = value;
        }

        [JsonIgnore]
        internal IChangeTrackingField<SlcAsset_Management.Enums.HierarchyRoleEnum> HierarchyRoleField => FieldHandler.GetOrCreateField(
            nameof(HierarchyRole),
            () => new ChangeTrackingField<SlcAsset_Management.Enums.HierarchyRoleEnum>(default));

        public void ResetChangeTracking()
        {
            _fieldHandler?.ApplyChanges();
        }
    }
}