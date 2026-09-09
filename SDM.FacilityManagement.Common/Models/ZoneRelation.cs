namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    using System;

    using Newtonsoft.Json;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public sealed class ZoneRelation : ChangeTrackingBase, IEquatable<ZoneRelation>, ISectionTrackable, ISectionEmptyState
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }

        [JsonIgnore]
        [SdmIgnore]
        public bool IsEmpty =>
            !Zone.HasValue();

        public SdmObjectReference<Zone> Zone
        {
            get => ZoneField.Value;
            set => ZoneField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<SdmObjectReference<Zone>> ZoneField => FieldHandler.GetOrCreateField(
            nameof(Zone),
            () => new ChangeTrackingField<SdmObjectReference<Zone>>(default));

        public static bool operator ==(ZoneRelation left, ZoneRelation right)
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

        public static bool operator !=(ZoneRelation left, ZoneRelation right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ZoneRelation);
        }

        public bool Equals(ZoneRelation other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Zone == other.Zone;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + (Zone != null ? Zone.GetHashCode() : 0);
                return hash;
            }
        }
    }
}