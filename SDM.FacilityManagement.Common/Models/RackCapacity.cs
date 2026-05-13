namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    using Newtonsoft.Json;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public class RackCapacity : ChangeTrackingBase
    {
        public double MaximumRackCapacity
        {
            get => RackUnitsField.Value;
            set => RackUnitsField.Value = value;
        }

        public double MaximumPowerCapacity
        {
            get => PowerCapacityField.Value;
            set => PowerCapacityField.Value = value;
        }

        [JsonIgnore]
        internal IChangeTrackingField<double> RackUnitsField => FieldHandler.GetOrCreateField(
            nameof(MaximumRackCapacity),
            () => new ChangeTrackingField<double>(0));

        [JsonIgnore]
        internal IChangeTrackingField<double> PowerCapacityField => FieldHandler.GetOrCreateField(
            nameof(MaximumPowerCapacity),
            () => new ChangeTrackingField<double>(0));
    }
}