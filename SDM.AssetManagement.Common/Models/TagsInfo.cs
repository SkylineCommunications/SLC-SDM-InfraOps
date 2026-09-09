namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System.Collections.Generic;
    using System.Linq;

    using Newtonsoft.Json;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;
    using System;

    public sealed class TagsInfo : ChangeTrackingBase, IEquatable<TagsInfo>, ISectionTrackable, ISectionEmptyState
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }
        [JsonIgnore]
        [SdmIgnore]
        public bool IsEmpty => (Tags == null || Tags.Count == 0);

        public List<SlcAsset_Management.Enums.TagOption> Tags
        {
            get => TagsField.Value ?? new List<SlcAsset_Management.Enums.TagOption>();
            set => TagsField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal ChangeTrackingArrayField<SlcAsset_Management.Enums.TagOption> TagsField => FieldHandler.GetOrCreateArrayField(
            nameof(Tags),
            () => new ChangeTrackingArrayField<SlcAsset_Management.Enums.TagOption>(new List<SlcAsset_Management.Enums.TagOption>()));

        public static bool operator ==(TagsInfo left, TagsInfo right)
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

        public static bool operator !=(TagsInfo left, TagsInfo right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TagsInfo);
        }

        public bool Equals(TagsInfo other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Tags.SequenceEqual(other.Tags);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                foreach (var tag in Tags)
                {
                    hash = (hash * 23) + tag.GetHashCode();
                }

                return hash;
            }
        }
    }
}