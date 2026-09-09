namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;

    using Newtonsoft.Json;

    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public sealed class AssetOwnership : ChangeTrackingBase, IEquatable<AssetOwnership>, ISectionTrackable, ISectionEmptyState
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }
        [JsonIgnore]
        [SdmIgnore]
        public bool IsEmpty => Organization == Guid.Empty &&
            ContactPerson == Guid.Empty &&
            ContactPersonRole == Guid.Empty &&
            Team == Guid.Empty;

        public Guid Organization
        {
            get => OrganizationField.Value;
            set => OrganizationField.Value = value;
        }

        public Guid ContactPerson
        {
            get => ContactPersonField.Value;
            set => ContactPersonField.Value = value;
        }

        public Guid ContactPersonRole
        {
            get => ContactPersonRoleField.Value;
            set => ContactPersonRoleField.Value = value;
        }

        public Guid Team
        {
            get => TeamField.Value;
            set => TeamField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<Guid> OrganizationField => FieldHandler.GetOrCreateField(
            nameof(Organization),
            () => new ChangeTrackingField<Guid>(Guid.Empty));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<Guid> ContactPersonField => FieldHandler.GetOrCreateField(
            nameof(ContactPerson),
            () => new ChangeTrackingField<Guid>(Guid.Empty));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<Guid> ContactPersonRoleField => FieldHandler.GetOrCreateField(
            nameof(ContactPersonRole),
            () => new ChangeTrackingField<Guid>(Guid.Empty));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<Guid> TeamField => FieldHandler.GetOrCreateField(
            nameof(Team),
            () => new ChangeTrackingField<Guid>(Guid.Empty));

        public static bool operator ==(AssetOwnership left, AssetOwnership right)
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

        public static bool operator !=(AssetOwnership left, AssetOwnership right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as AssetOwnership);
        }

        public bool Equals(AssetOwnership other)
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
                Organization.Equals(other.Organization) &&
                ContactPerson.Equals(other.ContactPerson) &&
                ContactPersonRole.Equals(other.ContactPersonRole) &&
                Team.Equals(other.Team);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + Organization.GetHashCode();
                hash = (hash * 23) + ContactPerson.GetHashCode();
                hash = (hash * 23) + ContactPersonRole.GetHashCode();
                hash = (hash * 23) + Team.GetHashCode();
                return hash;
            }
        }
    }
}