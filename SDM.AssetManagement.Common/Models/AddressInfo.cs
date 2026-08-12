using System;

using Newtonsoft.Json;

using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    public sealed class AddressInfo : ChangeTrackingBase, IEquatable<AddressInfo>, ISectionTrackable, ISectionEmptyState
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }
        [JsonIgnore]
        [SdmIgnore]
        public bool IsEmpty => Ipv4Address == default &&
            Ipv6Address == default &&
            Hostname == default &&
            DNS == default;

        public string Ipv4Address
        {
            get => Ipv4AddressField.Value;
            set => Ipv4AddressField.Value = value;
        }

        public string Ipv6Address
        {
            get => Ipv6AddressField.Value;
            set => Ipv6AddressField.Value = value;
        }

        public string Hostname
        {
            get => HostnameField.Value;
            set => HostnameField.Value = value;
        }

        public bool DNS
        {
            get => DNSField.Value;
            set => DNSField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> Ipv4AddressField => FieldHandler.GetOrCreateField(
            nameof(Ipv4Address),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> Ipv6AddressField => FieldHandler.GetOrCreateField(
            nameof(Ipv6Address),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> HostnameField => FieldHandler.GetOrCreateField(
            nameof(Hostname),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<bool> DNSField => FieldHandler.GetOrCreateField(
            nameof(DNS),
            () => new ChangeTrackingField<bool>(false));

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
