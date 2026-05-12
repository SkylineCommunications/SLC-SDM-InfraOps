namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using Newtonsoft.Json;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public class AssetRelation : ChangeTrackingBase
    {
        public SdmObjectReference<Asset> Asset
        {
            get => AssetField.Value;
            set => AssetField.Value = value;
        }

        [JsonIgnore]
        internal IChangeTrackingField<SdmObjectReference<Asset>> AssetField => FieldHandler.GetOrCreateField(
            nameof(Asset),
            () => new ChangeTrackingField<SdmObjectReference<Asset>>(default));
    }
}
