namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;

    using Newtonsoft.Json;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public sealed class JobRelation : ChangeTrackingBase, IEquatable<JobRelation>, ISectionTrackable, ISectionEmptyState
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }
        [JsonIgnore]
        [SdmIgnore]
        public bool IsEmpty => !Job.HasValue();

        public ISdmObjectReference<ISdmObject> Job
        {
            get => JobField.Value;
            set => JobField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<ISdmObjectReference<ISdmObject>> JobField => FieldHandler.GetOrCreateField(
            nameof(Job),
            () => new ChangeTrackingField<ISdmObjectReference<ISdmObject>>(default));

        public static bool operator ==(JobRelation left, JobRelation right)
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

        public static bool operator !=(JobRelation left, JobRelation right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as JobRelation);
        }

        public bool Equals(JobRelation other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Job == other.Job;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + Job.GetHashCode();
                return hash;
            }
        }
    }
}