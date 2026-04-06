namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;

    public sealed class AssetCustody : IEquatable<AssetCustody>
	{
		public DateTime From { get; set; }

		public DateTime Till { get; set; }

		public Guid ContactPersonId { get; set; }

		public Guid TeamId { get; set; }

		public Guid OrganizationId { get; set; }

		public Guid ContactPersonRoleId { get; set; }

		public static bool operator ==(AssetCustody left, AssetCustody right)
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

		public static bool operator !=(AssetCustody left, AssetCustody right)
		{
			return !(left == right);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as AssetCustody);
		}

		public bool Equals(AssetCustody other)
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
				From.Equals(other.From) &&
				Till.Equals(other.Till) &&
				ContactPersonId.Equals(other.ContactPersonId) &&
				TeamId.Equals(other.TeamId) &&
				OrganizationId.Equals(other.OrganizationId) &&
				ContactPersonRoleId.Equals(other.ContactPersonRoleId);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = (hash * 23) + From.GetHashCode();
				hash = (hash * 23) + Till.GetHashCode();
				hash = (hash * 23) + ContactPersonId.GetHashCode();
				hash = (hash * 23) + TeamId.GetHashCode();
				hash = (hash * 23) + OrganizationId.GetHashCode();
				hash = (hash * 23) + ContactPersonRoleId.GetHashCode();
				return hash;
			}
		}
	}
}