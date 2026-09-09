namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using Newtonsoft.Json;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;
    using System;

    public sealed class HierarchyInfo : ChangeTrackingBase, IEquatable<HierarchyInfo>, ISectionTrackable, ISectionEmptyState
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }
        [JsonIgnore]
        [SdmIgnore]
        public bool IsEmpty => HierarchyRole == default;

        public SlcAsset_Management.Enums.HierarchyRoleEnum? HierarchyRole
        {
            get => HierarchyRoleField.Value;
            set => HierarchyRoleField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<SlcAsset_Management.Enums.HierarchyRoleEnum?> HierarchyRoleField => FieldHandler.GetOrCreateField(
            nameof(HierarchyRole),
            () => new ChangeTrackingField<SlcAsset_Management.Enums.HierarchyRoleEnum?>(null));

        public static bool operator ==(HierarchyInfo left, HierarchyInfo right)
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

        public static bool operator !=(HierarchyInfo left, HierarchyInfo right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as HierarchyInfo);
        }

        public bool Equals(HierarchyInfo other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return HierarchyRole == other.HierarchyRole;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + HierarchyRole.GetHashCode();
                return hash;
            }
        }
    }
}