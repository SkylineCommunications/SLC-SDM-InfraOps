namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using Newtonsoft.Json;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public sealed class SourceInfo : ChangeTrackingBase, IEquatable<SourceInfo>, ISectionTrackable, ISectionEmptyState
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }
        [JsonIgnore]
        [SdmIgnore]
        public bool IsEmpty => CableTag == default &&
            Port == Guid.Empty &&
            !PortType.HasValue();

        public string CableTag
        {
            get => CableTagField.Value;
            set => CableTagField.Value = value;
        }

        public Guid Port
        {
            get => PortField.Value;
            set => PortField.Value = value;
        }

        public SdmObjectReference<PortType> PortType
        {
            get => PortTypeField.Value;
            set => PortTypeField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> CableTagField => FieldHandler.GetOrCreateField(
            nameof(CableTag),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<Guid> PortField => FieldHandler.GetOrCreateField(
            nameof(Port),
            () => new ChangeTrackingField<Guid>(Guid.Empty));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<SdmObjectReference<PortType>> PortTypeField => FieldHandler.GetOrCreateField(
            nameof(PortType),
            () => new ChangeTrackingField<SdmObjectReference<PortType>>(default));

        public static bool operator ==(SourceInfo left, SourceInfo right)
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

        public static bool operator !=(SourceInfo left, SourceInfo right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SourceInfo);
        }

        public bool Equals(SourceInfo other)
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
                string.Equals(CableTag, other.CableTag, StringComparison.OrdinalIgnoreCase) &&
                Port.Equals(other.Port) &&
                PortType == other.PortType;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + (CableTag != null ? CableTag.GetHashCode() : 0);
                hash = (hash * 23) + Port.GetHashCode();
                hash = (hash * 23) + (PortType != null ? PortType.GetHashCode() : 0);
                return hash;
            }
        }
    }
}