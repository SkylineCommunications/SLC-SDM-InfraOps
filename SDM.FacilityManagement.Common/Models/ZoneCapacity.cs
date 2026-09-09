namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    using System;

    using Newtonsoft.Json;

    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public sealed class ZoneCapacity : ChangeTrackingBase, IEquatable<ZoneCapacity>, ISectionTrackable, ISectionEmptyState
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }

        [JsonIgnore]
        [SdmIgnore]
        public bool IsEmpty =>
            CoolingCapacity == default;

        public double? CoolingCapacity
        {
            get => CoolingCapacityField.Value;
            set => CoolingCapacityField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<double?> CoolingCapacityField => FieldHandler.GetOrCreateField(
            nameof(CoolingCapacity),
            () => new ChangeTrackingField<double?>(null));

        public static bool operator ==(ZoneCapacity left, ZoneCapacity right)
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

        public static bool operator !=(ZoneCapacity left, ZoneCapacity right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ZoneCapacity);
        }

        public bool Equals(ZoneCapacity other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return CoolingCapacity == other.CoolingCapacity;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + CoolingCapacity.GetHashCode();
                return hash;
            }
        }
    }
}