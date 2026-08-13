namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
	using System;

    using SharedMappers.DomIds;
    using Newtonsoft.Json;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public sealed class PowerPortInfo : IEquatable<PowerPortInfo>, ISectionTrackable, ISectionEmptyState
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

		public string Name { get; set; }

		public long? PortNumber { get; set; }

		public SlcAsset_Management.Enums.Outputtype? OutputType { get; set; }

		public SlcAsset_Management.Enums.PortExposureEnum PortExposure { get; set; }

        public SdmObjectReference<PortType> PortType { get; set; }

		public string Label { get; set; }

		public override bool Equals(object obj)
		{
			return Equals(obj as PowerPortInfo);
		}

		public bool Equals(PowerPortInfo other)
		{
			if (other == null)
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