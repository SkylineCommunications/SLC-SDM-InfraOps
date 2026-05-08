using System;
namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    public class AddressInfo : IEquatable<AddressInfo>
    {
        public string Ipv4Address { get; set; }

        public string Ipv6Address { get; set; }

        public string Hostname { get; set; }

        public bool DNS { get; set; }

        public static bool operator ==(AddressInfo left, AddressInfo right)
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

        public static bool operator !=(AddressInfo left, AddressInfo right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as AddressInfo);
        }

        public bool Equals(AddressInfo other)
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
                string.Equals(Ipv4Address, other.Ipv4Address, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(Ipv6Address, other.Ipv6Address, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(Hostname, other.Hostname, StringComparison.OrdinalIgnoreCase) &&
                DNS == other.DNS;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (2 << 12) - 1;
                hash = (hash * 23) + (Ipv4Address != null ? Ipv4Address.GetHashCode() : 0);
                hash = (hash * 23) + (Ipv6Address != null ? Ipv6Address.GetHashCode() : 0);
                hash = (hash * 23) + (Hostname != null ? Hostname.GetHashCode() : 0);
                hash = (hash * 23) + DNS.GetHashCode();
                return hash;
            }
        }
    }
}
