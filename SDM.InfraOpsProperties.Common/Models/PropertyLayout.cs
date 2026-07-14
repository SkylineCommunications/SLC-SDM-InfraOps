namespace Skyline.DataMiner.SDM.InfraOpsProperties.Models
{
    using Newtonsoft.Json;

    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public class PropertyLayout : ChangeTrackingBase
    {
        public string SectionName
        {
            get => SectionNameField.Value;
            set => SectionNameField.Value = value;
        }

        public long? Order
        {
            get => OrderField.Value;
            set => OrderField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> SectionNameField => FieldHandler.GetOrCreateField(
            nameof(SectionName),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<long?> OrderField => FieldHandler.GetOrCreateField(
            nameof(Order),
            () => new ChangeTrackingField<long?>(null));
    }
}
