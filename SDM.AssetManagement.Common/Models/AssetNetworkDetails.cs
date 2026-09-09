namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
	using System;

	using Newtonsoft.Json;

	using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

	public sealed class AssetNetworkDetails : ChangeTrackingBase, IEquatable<AssetNetworkDetails>, ISectionTrackable, ISectionEmptyState
	{
		[JsonIgnore]
		[SdmIgnore]
		Guid? ISectionTrackable.SectionId { get; set; }
		[JsonIgnore]
		[SdmIgnore]
		public bool IsEmpty => MACAddress == default;

		public string MACAddress
		{
			get => MACAddressField.Value;
			set => MACAddressField.Value = value;
		}

		[JsonIgnore]
		[SdmIgnore]
		internal IChangeTrackingField<string> MACAddressField => FieldHandler.GetOrCreateField(
			nameof(MACAddress),
			() => new ChangeTrackingStringField(null));

		public static bool operator ==(AssetNetworkDetails left, AssetNetworkDetails right)
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

		public static bool operator !=(AssetNetworkDetails left, AssetNetworkDetails right)
		{
			return !(left == right);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as AssetNetworkDetails);
		}

		public bool Equals(AssetNetworkDetails other)
		{
			if (other is null)
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return string.Equals(MACAddress, other.MACAddress, StringComparison.OrdinalIgnoreCase);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = (2 << 12) - 1;
				hash = (hash * 23) + (MACAddress != null ? MACAddress.GetHashCode() : 0);
				return hash;
			}
		}
	}
}