using System.Collections.Generic;
using System.Linq;

using SharedMappers.DomIds;
using System;
using Newtonsoft.Json;
using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    public sealed class CategoryRelation : ChangeTrackingBase, IEquatable<CategoryRelation>, ISectionTrackable, ISectionEmptyState
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }
        [JsonIgnore]
        [SdmIgnore]
        public bool IsEmpty => (Categories == null || Categories.Count == 0);

        public List<SlcAsset_Management.Enums.CategoriesEnum> Categories
        {
            get => CategoriesField.Value ?? new List<SlcAsset_Management.Enums.CategoriesEnum>();
            set => CategoriesField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal ChangeTrackingArrayField<SlcAsset_Management.Enums.CategoriesEnum> CategoriesField => FieldHandler.GetOrCreateArrayField(
            nameof(Categories),
            () => new ChangeTrackingArrayField<SlcAsset_Management.Enums.CategoriesEnum>(new List<SlcAsset_Management.Enums.CategoriesEnum>()));

        public static bool operator ==(CategoryRelation left, CategoryRelation right)
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

        public static bool operator !=(CategoryRelation left, CategoryRelation right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CategoryRelation);
        }

        public bool Equals(CategoryRelation other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Categories.SequenceEqual(other.Categories);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                foreach (var category in Categories)
                {
                    hash = (hash * 23) + category.GetHashCode();
                }

                return hash;
            }
        }
    }
}