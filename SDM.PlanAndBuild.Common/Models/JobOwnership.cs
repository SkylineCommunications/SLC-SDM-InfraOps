namespace Skyline.DataMiner.SDM.PlanAndBuild.Models
{
    using System;

    using Newtonsoft.Json;

    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public sealed class JobOwnership : ChangeTrackingBase, IEquatable<JobOwnership>, ISectionTrackable, ISectionEmptyState
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }

        [JsonIgnore]
        [SdmIgnore]
        public bool IsEmpty =>
            AssignedTo == default &&
            AssignmentGroup == default;

        public Guid? AssignedTo
        {
            get => AssignedToField.Value;
            set => AssignedToField.Value = value;
        }

        public Guid? AssignmentGroup
        {
            get => AssignmentGroupField.Value;
            set => AssignmentGroupField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<Guid?> AssignedToField => FieldHandler.GetOrCreateField(
            nameof(AssignedTo),
            () => new ChangeTrackingField<Guid?>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<Guid?> AssignmentGroupField => FieldHandler.GetOrCreateField(
            nameof(AssignmentGroup),
            () => new ChangeTrackingField<Guid?>(null));

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