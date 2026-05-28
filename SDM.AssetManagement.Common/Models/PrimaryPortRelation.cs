namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using Newtonsoft.Json;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public sealed class PrimaryPortRelation : ChangeTrackingBase, IEquatable<PrimaryPortRelation>
    {
        public bool IsPrimaryIpv6
        {
            get => IsPrimaryIpv6Field.Value;
            set => IsPrimaryIpv6Field.Value = value;
        }

        public bool IsPrimaryIpv4
        {
            get => IsPrimaryIpv4Field.Value;
            set => IsPrimaryIpv4Field.Value = value;
        }

        [JsonIgnore]
        internal IChangeTrackingField<bool> IsPrimaryIpv6Field => FieldHandler.GetOrCreateField(
            nameof(IsPrimaryIpv6),
            () => new ChangeTrackingField<bool>(false));

        [JsonIgnore]
        internal IChangeTrackingField<bool> IsPrimaryIpv4Field => FieldHandler.GetOrCreateField(
            nameof(IsPrimaryIpv4),
            () => new ChangeTrackingField<bool>(false));

        public override bool Equals(object obj)
        {
            return Equals(obj as PrimaryPortRelation);
        }

        public bool Equals(PrimaryPortRelation other)
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
                IsPrimaryIpv6 == other.IsPrimaryIpv6 &&
                IsPrimaryIpv4 == other.IsPrimaryIpv4;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (2 << 12) - 1;
                hash = (hash * 23) + IsPrimaryIpv6.GetHashCode();
                hash = (hash * 23) + IsPrimaryIpv4.GetHashCode();
                return hash;
            }
        }
    }
}
