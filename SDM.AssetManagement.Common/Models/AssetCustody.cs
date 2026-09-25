namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;

    using Newtonsoft.Json;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences;
    using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
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
            !ContactPerson.HasValue() &&
            !Team.HasValue() &&
            !Organization.HasValue() &&
            !ContactPersonRole.HasValue();

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

        public PnoObjectReference<Person> ContactPerson
        {
            get => ContactPersonField.Value;
            set => ContactPersonField.Value = value;
        }

        public PnoObjectReference<Team> Team
        {
            get => TeamField.Value;
            set => TeamField.Value = value;
        }

        public PnoObjectReference<Organization> Organization
        {
            get => OrganizationField.Value;
            set => OrganizationField.Value = value;
        }

        public PnoObjectReference<Role> ContactPersonRole
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
        internal IChangeTrackingField<PnoObjectReference<Person>> ContactPersonField => FieldHandler.GetOrCreateField(
            nameof(ContactPerson),
            () => new ChangeTrackingField<PnoObjectReference<Person>>(default, reference => reference.Identifier));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<PnoObjectReference<Team>> TeamField => FieldHandler.GetOrCreateField(
            nameof(Team),
            () => new ChangeTrackingField<PnoObjectReference<Team>>(default, reference => reference.Identifier));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<PnoObjectReference<Organization>> OrganizationField => FieldHandler.GetOrCreateField(
            nameof(Organization),
            () => new ChangeTrackingField<PnoObjectReference<Organization>>(default, reference => reference.Identifier));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<PnoObjectReference<Role>> ContactPersonRoleField => FieldHandler.GetOrCreateField(
            nameof(ContactPersonRole),
            () => new ChangeTrackingField<PnoObjectReference<Role>>(default, reference => reference.Identifier));

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
                ContactPerson == other.ContactPerson &&
                Team == other.Team &&
                Organization == other.Organization &&
                ContactPersonRole == other.ContactPersonRole;
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