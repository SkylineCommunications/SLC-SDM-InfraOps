namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    using System;

    using Newtonsoft.Json;

    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public sealed class FacilityRelation : ChangeTrackingBase, IEquatable<FacilityRelation>, ISectionTrackable, ISectionEmptyState
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }

        [JsonIgnore]
        [SdmIgnore]
        public bool IsEmpty =>
            !Facility.HasValue();

        public SdmObjectReference<Facility> Facility
        {
            get => FacilityField.Value;
            set => FacilityField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<SdmObjectReference<Facility>> FacilityField => FieldHandler.GetOrCreateField(
            nameof(Facility),
            () => new ChangeTrackingField<SdmObjectReference<Facility>>(default));

        public static bool operator ==(FacilityRelation left, FacilityRelation right)
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

        public static bool operator !=(FacilityRelation left, FacilityRelation right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as FacilityRelation);
        }

        public bool Equals(FacilityRelation other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Facility == other.Facility;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + (Facility != null ? Facility.GetHashCode() : 0);
                return hash;
            }
        }
    }
}