namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;

    public class PrimaryPortRelation : IEquatable<PrimaryPortRelation>
    {
        public bool IsPrimaryIpv6 { get; set; }

        public bool IsPrimaryIpv4 { get; set; }

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
