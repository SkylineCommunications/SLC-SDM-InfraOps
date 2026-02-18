namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
	using System;
	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.SDM.AssetManagement;

	public sealed class AssetLocation : IEquatable<AssetLocation>
	{
		public long HolderNumber { get; set; }

		public SdmObjectReference<Asset> ParentAsset { get; set; }

		public Guid RackId { get; set; }

		public long RackPosition { get; set; }

		public SlcAssetManagement.Enums.Side Side { get; set; }

		public Guid DeskId { get; set; }

		public Guid ContainerId { get; set; }

		public Guid RoomId { get; set; }

		public static bool operator ==(AssetLocation left, AssetLocation right)
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

		public static bool operator !=(AssetLocation left, AssetLocation right)
		{
			return !(left == right);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as AssetLocation);
		}

		public bool Equals(AssetLocation other)
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
				HolderNumber == other.HolderNumber &&
				Equals(ParentAsset, other.ParentAsset) &&
				RackId.Equals(other.RackId) &&
				RackPosition == other.RackPosition &&
				Side == other.Side &&
				DeskId.Equals(other.DeskId) &&
				ContainerId.Equals(other.ContainerId) &&
				RoomId.Equals(other.RoomId);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = (hash * 23) + HolderNumber.GetHashCode();
				hash = (hash * 23) + (ParentAsset != null ? ParentAsset.GetHashCode() : 0);
				hash = (hash * 23) + RackId.GetHashCode();
				hash = (hash * 23) + RackPosition.GetHashCode();
				hash = (hash * 23) + Side.GetHashCode();
				hash = (hash * 23) + DeskId.GetHashCode();
				hash = (hash * 23) + ContainerId.GetHashCode();
				hash = (hash * 23) + RoomId.GetHashCode();
				return hash;
			}
		}
	}
}