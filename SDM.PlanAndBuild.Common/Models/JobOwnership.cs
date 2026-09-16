namespace Skyline.DataMiner.SDM.PlanAndBuild.Models
{
    using System;

    using Newtonsoft.Json;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences;
    using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public sealed class JobOwnership : ChangeTrackingBase, IEquatable<JobOwnership>, ISectionTrackable, ISectionEmptyState
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }

        [JsonIgnore]
        [SdmIgnore]
        public bool IsEmpty =>
            !AssignedTo.HasValue() &&
            !AssignmentGroup.HasValue();

        public PnoObjectReference<Person> AssignedTo
        {
            get => AssignedToField.Value;
            set => AssignedToField.Value = value;
        }

        public PnoObjectReference<Team> AssignmentGroup
        {
            get => AssignmentGroupField.Value;
            set => AssignmentGroupField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<PnoObjectReference<Person>> AssignedToField => FieldHandler.GetOrCreateField(
            nameof(AssignedTo),
            () => new ChangeTrackingField<PnoObjectReference<Person>>(default));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<PnoObjectReference<Team>> AssignmentGroupField => FieldHandler.GetOrCreateField(
            nameof(AssignmentGroup),
            () => new ChangeTrackingField<PnoObjectReference<Team>>(default));

        public static bool operator ==(JobOwnership left, JobOwnership right)
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

        public static bool operator !=(JobOwnership left, JobOwnership right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as JobOwnership);
        }

        public bool Equals(JobOwnership other)
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
                AssignedTo == other.AssignedTo &&
                AssignmentGroup == other.AssignmentGroup;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + AssignedTo.GetHashCode();
                hash = (hash * 23) + AssignmentGroup.GetHashCode();
                return hash;
            }
        }
    }
}