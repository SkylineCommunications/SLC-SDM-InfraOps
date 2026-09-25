namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using System;
    using Newtonsoft.Json;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public sealed class CableRelation : ChangeTrackingBase, IEquatable<CableRelation>, ISectionTrackable, ISectionEmptyState
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }
        [JsonIgnore]
        [SdmIgnore]
        public bool IsEmpty => CableTypeFks == null || !CableTypeFks.Any(cableTypeFk => cableTypeFk.HasValue());

        public List<SdmObjectReference<CableType>> CableTypeFks
        {
            get => CableTypeFksField.Value ?? new List<SdmObjectReference<CableType>>();
            set => CableTypeFksField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal ChangeTrackingArrayField<SdmObjectReference<CableType>> CableTypeFksField => FieldHandler.GetOrCreateArrayField(
            nameof(CableTypeFks),
            () => new ChangeTrackingArrayField<SdmObjectReference<CableType>>(new List<SdmObjectReference<CableType>>()));

        public static bool operator ==(CableRelation left, CableRelation right)
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

        public static bool operator !=(CableRelation left, CableRelation right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CableRelation);
        }

        public bool Equals(CableRelation other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return CableTypeFks.SequenceEqual(other.CableTypeFks);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                foreach (var cableTypeFk in CableTypeFks)
                {
                    hash = (hash * 23) + (cableTypeFk != null ? cableTypeFk.GetHashCode() : 0);
                }

                return hash;
            }
        }
    }
}