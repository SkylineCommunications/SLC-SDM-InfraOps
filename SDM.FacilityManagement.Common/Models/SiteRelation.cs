namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    using System;

    using Newtonsoft.Json;

    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public sealed class SiteRelation : ChangeTrackingBase, IEquatable<SiteRelation>, ISectionTrackable, ISectionEmptyState
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }

        [JsonIgnore]
        [SdmIgnore]
        public bool IsEmpty =>
            !Site.HasValue();

        public SdmObjectReference<Site> Site
        {
            get => SiteField.Value;
            set => SiteField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<SdmObjectReference<Site>> SiteField => FieldHandler.GetOrCreateField(
            nameof(Site),
            () => new ChangeTrackingField<SdmObjectReference<Site>>(default));

        public static bool operator ==(SiteRelation left, SiteRelation right)
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

        public static bool operator !=(SiteRelation left, SiteRelation right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SiteRelation);
        }

        public bool Equals(SiteRelation other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Site == other.Site;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + (Site != null ? Site.GetHashCode() : 0);
                return hash;
            }
        }
    }
}