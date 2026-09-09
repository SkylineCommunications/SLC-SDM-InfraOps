namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    using System;

    using Newtonsoft.Json;

    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public sealed class ResourceLink : ChangeTrackingBase, IEquatable<ResourceLink>, ISectionTrackable, ISectionEmptyState
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }

        [JsonIgnore]
        [SdmIgnore]
        public bool IsEmpty =>
            ResourceId == Guid.Empty;

        public Guid ResourceId
        {
            get => ResourceIdField.Value;
            set => ResourceIdField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<Guid> ResourceIdField => FieldHandler.GetOrCreateField(
            nameof(ResourceId),
            () => new ChangeTrackingField<Guid>(Guid.Empty));

        public static bool operator ==(ResourceLink left, ResourceLink right)
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

        public static bool operator !=(ResourceLink left, ResourceLink right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ResourceLink);
        }

        public bool Equals(ResourceLink other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return ResourceId.Equals(other.ResourceId);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + ResourceId.GetHashCode();
                return hash;
            }
        }
    }
}