namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    using System;

    using Newtonsoft.Json;

    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public sealed class RackCapacity : ChangeTrackingBase, IEquatable<RackCapacity>, ISectionTrackable, ISectionEmptyState
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }

        [JsonIgnore]
        [SdmIgnore]
        public bool IsEmpty =>
            MaximumRackCapacity == default &&
            MaximumPowerCapacity == default;

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

        public static bool operator ==(RackCapacity left, RackCapacity right)
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

        public static bool operator !=(RackCapacity left, RackCapacity right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as RackCapacity);
        }

        public bool Equals(RackCapacity other)
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
                MaximumRackCapacity.Equals(other.MaximumRackCapacity) &&
                MaximumPowerCapacity.Equals(other.MaximumPowerCapacity);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + MaximumRackCapacity.GetHashCode();
                hash = (hash * 23) + MaximumPowerCapacity.GetHashCode();
                return hash;
            }
        }
    }
}