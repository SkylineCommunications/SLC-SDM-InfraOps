namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;

    public sealed class AssetOwnership : IEquatable<AssetOwnership>
	{
		public Guid Organization { get; set; }

		public Guid ContactPersonId { get; set; } // Linked to People DOM Definition in P&O

		public Guid ContactPersonRoleId { get; set; } // Linked to Roles DOM Definition in P&O

		public Guid TeamId { get; set; } // Linked to Teams DOM Definition in P&O

		public static bool operator ==(AssetOwnership left, AssetOwnership right)
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

		public static bool operator !=(AssetOwnership left, AssetOwnership right)
		{
			return !(left == right);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as AssetOwnership);
		}

		public bool Equals(AssetOwnership other)
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
				Organization.Equals(other.Organization) &&
				ContactPersonId.Equals(other.ContactPersonId) &&
				ContactPersonRoleId.Equals(other.ContactPersonRoleId) &&
				TeamId.Equals(other.TeamId);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = (hash * 23) + Organization.GetHashCode();
				hash = (hash * 23) + ContactPersonId.GetHashCode();
				hash = (hash * 23) + ContactPersonRoleId.GetHashCode();
				hash = (hash * 23) + TeamId.GetHashCode();
				return hash;
			}
		}
	}
}