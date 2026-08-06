namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    using System;

    using Newtonsoft.Json;

    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public class RackCapacity : ChangeTrackingBase, ISectionTrackable
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }

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
        [SdmIgnore]
        internal IChangeTrackingField<double> RackUnitsField => FieldHandler.GetOrCreateField(
            nameof(MaximumRackCapacity),
            () => new ChangeTrackingField<double>(0));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<double> PowerCapacityField => FieldHandler.GetOrCreateField(
            nameof(MaximumPowerCapacity),
            () => new ChangeTrackingField<double>(0));
    }
}