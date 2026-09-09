namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    using System;

    using Newtonsoft.Json;

    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public sealed class RoomOwnership : ChangeTrackingBase, IEquatable<RoomOwnership>, ISectionTrackable, ISectionEmptyState
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }

        [JsonIgnore]
        [SdmIgnore]
        public bool IsEmpty =>
            Team == Guid.Empty &&
            Owner == Guid.Empty;

        public Guid Team
        {
            get => TeamField.Value;
            set => TeamField.Value = value;
        }

        public Guid Owner
        {
            get => OwnerField.Value;
            set => OwnerField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<Guid> TeamField => FieldHandler.GetOrCreateField(
            nameof(Team),
            () => new ChangeTrackingField<Guid>(Guid.Empty));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<Guid> OwnerField => FieldHandler.GetOrCreateField(
            nameof(Owner),
            () => new ChangeTrackingField<Guid>(Guid.Empty));

        public static bool operator ==(RoomOwnership left, RoomOwnership right)
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

        public static bool operator !=(RoomOwnership left, RoomOwnership right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as RoomOwnership);
        }

        public bool Equals(RoomOwnership other)
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
                Team.Equals(other.Team) &&
                Owner.Equals(other.Owner);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + Team.GetHashCode();
                hash = (hash * 23) + Owner.GetHashCode();
                return hash;
            }
        }
    }
}