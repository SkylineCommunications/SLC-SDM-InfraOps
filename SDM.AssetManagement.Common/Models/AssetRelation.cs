namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;

    using Newtonsoft.Json;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public sealed class AssetRelation : ChangeTrackingBase, IEquatable<AssetRelation>, ISectionTrackable, ISectionEmptyState
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }
        [JsonIgnore]
        [SdmIgnore]
        public bool IsEmpty => !Asset.HasValue();

        public SdmObjectReference<Asset> Asset
        {
            get => AssetField.Value;
            set => AssetField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<SdmObjectReference<Asset>> AssetField => FieldHandler.GetOrCreateField(
            nameof(Asset),
            () => new ChangeTrackingField<SdmObjectReference<Asset>>(default));

        public static bool operator ==(AssetRelation left, AssetRelation right)
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

        public static bool operator !=(AssetRelation left, AssetRelation right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as AssetRelation);
        }

        public bool Equals(AssetRelation other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Asset == other.Asset;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + (Asset != null ? Asset.GetHashCode() : 0);
                return hash;
            }
        }
    }
}