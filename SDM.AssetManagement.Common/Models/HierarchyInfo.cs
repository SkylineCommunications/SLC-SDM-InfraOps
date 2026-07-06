namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using Newtonsoft.Json;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public class HierarchyInfo : ChangeTrackingBase
    {
        public SlcAsset_Management.Enums.HierarchyRoleEnum HierarchyRole
        {
            get => HierarchyRoleField.Value;
            set => HierarchyRoleField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<SlcAsset_Management.Enums.HierarchyRoleEnum> HierarchyRoleField => FieldHandler.GetOrCreateField(
            nameof(HierarchyRole),
            () => new ChangeTrackingField<SlcAsset_Management.Enums.HierarchyRoleEnum>(default));
    }
}