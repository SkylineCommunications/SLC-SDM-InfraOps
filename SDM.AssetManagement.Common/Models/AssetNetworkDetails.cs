namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
	using System;

	public sealed class AssetNetworkDetails : IEquatable<AssetNetworkDetails>
	{
		public string MACAddress { get; set; }

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