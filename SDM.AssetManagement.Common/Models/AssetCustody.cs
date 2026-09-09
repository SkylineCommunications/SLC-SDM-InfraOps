namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;

    using Newtonsoft.Json;

    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public sealed class AssetCustody : ChangeTrackingBase, IEquatable<AssetCustody>, ISectionTrackable, ISectionEmptyState
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }
        [JsonIgnore]
        [SdmIgnore]
        public bool IsEmpty => From == default &&
            Till == default &&
            ContactPerson == Guid.Empty &&
            Team == Guid.Empty &&
            Organization == Guid.Empty &&
            ContactPersonRole == Guid.Empty;

        public DateTime? From
        {
            get => FromField.Value;
            set => FromField.Value = value;
        }

        public DateTime? Till
        {
            get => TillField.Value;
            set => TillField.Value = value;
        }

        public Guid ContactPerson
        {
            get => ContactPersonField.Value;
            set => ContactPersonField.Value = value;
        }

        public Guid Team
        {
            get => TeamField.Value;
            set => TeamField.Value = value;
        }

        public Guid Organization
        {
            get => OrganizationField.Value;
            set => OrganizationField.Value = value;
        }

        public Guid ContactPersonRole
        {
            get => ContactPersonRoleField.Value;
            set => ContactPersonRoleField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<DateTime?> FromField => FieldHandler.GetOrCreateField(
            nameof(From),
            () => new ChangeTrackingField<DateTime?>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<DateTime?> TillField => FieldHandler.GetOrCreateField(
            nameof(Till),
            () => new ChangeTrackingField<DateTime?>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<Guid> ContactPersonField => FieldHandler.GetOrCreateField(
            nameof(ContactPerson),
            () => new ChangeTrackingField<Guid>(Guid.Empty));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<Guid> TeamField => FieldHandler.GetOrCreateField(
            nameof(Team),
            () => new ChangeTrackingField<Guid>(Guid.Empty));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<Guid> OrganizationField => FieldHandler.GetOrCreateField(
            nameof(Organization),
            () => new ChangeTrackingField<Guid>(Guid.Empty));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<Guid> ContactPersonRoleField => FieldHandler.GetOrCreateField(
            nameof(ContactPersonRole),
            () => new ChangeTrackingField<Guid>(Guid.Empty));

        public static bool operator ==(AssetCustody left, AssetCustody right)
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

        public static bool operator !=(AssetCustody left, AssetCustody right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as AssetCustody);
        }

        public bool Equals(AssetCustody other)
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
                From.Equals(other.From) &&
                Till.Equals(other.Till) &&
                ContactPerson.Equals(other.ContactPerson) &&
                Team.Equals(other.Team) &&
                Organization.Equals(other.Organization) &&
                ContactPersonRole.Equals(other.ContactPersonRole);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + From.GetHashCode();
                hash = (hash * 23) + Till.GetHashCode();
                hash = (hash * 23) + ContactPerson.GetHashCode();
                hash = (hash * 23) + Team.GetHashCode();
                hash = (hash * 23) + Organization.GetHashCode();
                hash = (hash * 23) + ContactPersonRole.GetHashCode();
                return hash;
            }
        }
    }
}