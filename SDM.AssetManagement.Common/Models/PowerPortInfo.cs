namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
	using System;
    using Newtonsoft.Json;
    using SharedMappers.DomIds;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public sealed class PowerPortInfo : ChangeTrackingBase, IEquatable<PowerPortInfo>, ISectionTrackable, ISectionEmptyState, IPortInfo
	{
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }
        [JsonIgnore]
        [SdmIgnore]
        public bool IsEmpty => Name == default &&
            PortNumber == default &&
            OutputType == default &&
            PortExposure == default &&
            !PortType.HasValue() &&
            Label == default;

        public PowerPortInfo() : base()
        {
        }

        public string Name
        {
            get => NameField.Value;
            set => NameField.Value = value;
        }

        public long? PortNumber
        {
            get => PortNumberField.Value;
            set => PortNumberField.Value = value;
        }

        public SlcAsset_Management.Enums.Outputtype? OutputType
        {
            get => OutputTypeField.Value;
            set => OutputTypeField.Value = value;
        }

        public SlcAsset_Management.Enums.PortExposureEnum PortExposure
        {
            get => PortExposureField.Value;
            set => PortExposureField.Value = value;
        }

        public SdmObjectReference<PortType> PortType
        {
            get => PortTypeField.Value;
            set => PortTypeField.Value = value;
        }

        public string Label
        {
            get => LabelField.Value;
            set => LabelField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> NameField => FieldHandler.GetOrCreateField(
            nameof(Name),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<long?> PortNumberField => FieldHandler.GetOrCreateField(
            nameof(PortNumber),
            () => new ChangeTrackingField<long?>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<SlcAsset_Management.Enums.Outputtype?> OutputTypeField => FieldHandler.GetOrCreateField(
            nameof(OutputType),
            () => new ChangeTrackingField<SlcAsset_Management.Enums.Outputtype?>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<SlcAsset_Management.Enums.PortExposureEnum> PortExposureField => FieldHandler.GetOrCreateField(
            nameof(PortExposure),
            () => new ChangeTrackingField<SlcAsset_Management.Enums.PortExposureEnum>(default));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<SdmObjectReference<PortType>> PortTypeField => FieldHandler.GetOrCreateField(
            nameof(Type),
            () => new ChangeTrackingField<SdmObjectReference<PortType>>(default));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> LabelField => FieldHandler.GetOrCreateField(
            nameof(Label),
            () => new ChangeTrackingStringField(null));

        public bool Equals(PowerPortInfo other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase) &&
                   PortNumber == other.PortNumber &&
                   OutputType == other.OutputType &&
                   PortExposure == other.PortExposure &&
                   PortType == other.PortType &&
                   string.Equals(Label, other.Label, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
		{
			return Equals(obj as PowerPortInfo);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = (3 << 12) - 1;
				hash = (hash * 23) + (Name != null ? Name.GetHashCode() : 0);
				hash = (hash * 23) + PortNumber.GetHashCode();
				hash = (hash * 23) + OutputType.GetHashCode();
				hash = (hash * 23) + PortExposure.GetHashCode();
				hash = (hash * 23) + PortType.GetHashCode();
				hash = (hash * 23) + (Label != null ? Label.GetHashCode() : 0);
				return hash;
			}
		}
	}
}