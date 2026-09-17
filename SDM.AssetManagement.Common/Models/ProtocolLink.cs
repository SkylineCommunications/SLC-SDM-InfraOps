namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using Newtonsoft.Json;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public sealed class ProtocolLink : ChangeTrackingBase, IEquatable<ProtocolLink>
    {
        public string Protocol
        {
            get => ProtocolField.Value;
            set => ProtocolField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> ProtocolField => FieldHandler.GetOrCreateField(
            nameof(Protocol),
            () => new ChangeTrackingField<string>(null));

        public bool Equals(ProtocolLink other)
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
                Protocol == other.Protocol;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ProtocolLink);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (2 << 12) - 1;
                hash = (hash * 23) + (Protocol?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }
}