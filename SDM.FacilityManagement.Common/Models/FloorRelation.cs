namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    using System;

    using Newtonsoft.Json;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public sealed class FloorRelation : ChangeTrackingBase, IEquatable<FloorRelation>, ISectionTrackable, ISectionEmptyState
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }

        [JsonIgnore]
        [SdmIgnore]
        public bool IsEmpty =>
            !Floor.HasValue();

        public SdmObjectReference<Floor> Floor
        {
            get => FloorField.Value;
            set => FloorField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<SdmObjectReference<Floor>> FloorField => FieldHandler.GetOrCreateField(
            nameof(Floor),
            () => new ChangeTrackingField<SdmObjectReference<Floor>>(default));

        public static bool operator ==(FloorRelation left, FloorRelation right)
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

        public static bool operator !=(FloorRelation left, FloorRelation right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as FloorRelation);
        }

        public bool Equals(FloorRelation other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Floor == other.Floor;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + (Floor != null ? Floor.GetHashCode() : 0);
                return hash;
            }
        }
    }
}