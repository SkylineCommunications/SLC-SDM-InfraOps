namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;

    using Newtonsoft.Json;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences;
    using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public sealed class AssetOwnership : ChangeTrackingBase, IEquatable<AssetOwnership>, ISectionTrackable, ISectionEmptyState
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }
        [JsonIgnore]
        [SdmIgnore]
        public bool IsEmpty => !Organization.HasValue() &&
            !ContactPerson.HasValue() &&
            !ContactPersonRole.HasValue() &&
            !Team.HasValue();

        public PnoObjectReference<Organization> Organization
        {
            get => OrganizationField.Value;
            set => OrganizationField.Value = value;
        }

        public PnoObjectReference<Person> ContactPerson
        {
            get => ContactPersonField.Value;
            set => ContactPersonField.Value = value;
        }

        public PnoObjectReference<Role> ContactPersonRole
        {
            get => ContactPersonRoleField.Value;
            set => ContactPersonRoleField.Value = value;
        }

        public PnoObjectReference<Team> Team
        {
            get => TeamField.Value;
            set => TeamField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<PnoObjectReference<Organization>> OrganizationField => FieldHandler.GetOrCreateField(
            nameof(Organization),
            () => new ChangeTrackingField<PnoObjectReference<Organization>>(default));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<PnoObjectReference<Person>> ContactPersonField => FieldHandler.GetOrCreateField(
            nameof(ContactPerson),
            () => new ChangeTrackingField<PnoObjectReference<Person>>(default));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<PnoObjectReference<Role>> ContactPersonRoleField => FieldHandler.GetOrCreateField(
            nameof(ContactPersonRole),
            () => new ChangeTrackingField<PnoObjectReference<Role>>(default));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<PnoObjectReference<Team>> TeamField => FieldHandler.GetOrCreateField(
            nameof(Team),
            () => new ChangeTrackingField<PnoObjectReference<Team>>(default));

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
                Organization == other.Organization &&
                ContactPerson == other.ContactPerson &&
                ContactPersonRole == other.ContactPersonRole &&
                Team == other.Team;
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