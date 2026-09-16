namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    using System;

    using Newtonsoft.Json;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences;
    using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public sealed class RoomOwnership : ChangeTrackingBase, IEquatable<RoomOwnership>, ISectionTrackable, ISectionEmptyState
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }

        [JsonIgnore]
        [SdmIgnore]
        public bool IsEmpty =>
            !Team.HasValue() &&
            !Owner.HasValue();

        public PnoObjectReference<Team> Team
        {
            get => TeamField.Value;
            set => TeamField.Value = value;
        }

        public PnoObjectReference<Person> Owner
        {
            get => OwnerField.Value;
            set => OwnerField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<PnoObjectReference<Team>> TeamField => FieldHandler.GetOrCreateField(
            nameof(Team),
            () => new ChangeTrackingField<PnoObjectReference<Team>>(default));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<PnoObjectReference<Person>> OwnerField => FieldHandler.GetOrCreateField(
            nameof(Owner),
            () => new ChangeTrackingField<PnoObjectReference<Person>>(default));

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
                Team == other.Team &&
                Owner == other.Owner;
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