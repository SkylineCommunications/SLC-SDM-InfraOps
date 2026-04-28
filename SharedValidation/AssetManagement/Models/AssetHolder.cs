namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
	using System;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM.AssetManagement;

	public sealed class AssetHolder : SdmObject<AssetHolder>, IEquatable<AssetHolder>
	{
		public long SlotNumber { get; set; }

		public SlcAsset_Management.Enums.HierarchyRoleEnum HierarchyRole { get; set; }

		public static bool operator ==(AssetHolder left, AssetHolder right)
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

		public static bool operator !=(AssetHolder left, AssetHolder right)
		{
			return !(left == right);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as AssetHolder);
		}

		public bool Equals(AssetHolder other)
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
				SlotNumber == other.SlotNumber &&
				HierarchyRole == other.HierarchyRole;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = (hash * 23) + SlotNumber.GetHashCode();
				hash = (hash * 23) + HierarchyRole.GetHashCode();
				return hash;
			}
		}
	}
}