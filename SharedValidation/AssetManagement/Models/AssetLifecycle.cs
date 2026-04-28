namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;

    public sealed class AssetLifecycle : IEquatable<AssetLifecycle>
	{
		public DateTime PurchaseDate { get; set; }

		public DateTime FirstUseDate { get; set; }

		public DateTime EndOfWarrantyDate { get; set; }

		public DateTime InstallationDate { get; set; }

		public Guid InstallationUserId { get; set; }

		public DateTime ModificationDate { get; set; }

		public Guid ModificationUserId { get; set; }

		public DateTime EndOfLife { get; set; }

		public override bool Equals(object obj)
		{
			return Equals(obj as AssetLifecycle);
		}

		public bool Equals(AssetLifecycle other)
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
				PurchaseDate.Equals(other.PurchaseDate) &&
				FirstUseDate.Equals(other.FirstUseDate) &&
				EndOfWarrantyDate.Equals(other.EndOfWarrantyDate) &&
				InstallationDate.Equals(other.InstallationDate) &&
				InstallationUserId.Equals(other.InstallationUserId) &&
				ModificationDate.Equals(other.ModificationDate) &&
				ModificationUserId.Equals(other.ModificationUserId) &&
				EndOfLife.Equals(other.EndOfLife);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = (hash * 23) + PurchaseDate.GetHashCode();
				hash = (hash * 23) + FirstUseDate.GetHashCode();
				hash = (hash * 23) + EndOfWarrantyDate.GetHashCode();
				hash = (hash * 23) + InstallationDate.GetHashCode();
				hash = (hash * 23) + InstallationUserId.GetHashCode();
				hash = (hash * 23) + ModificationDate.GetHashCode();
				hash = (hash * 23) + ModificationUserId.GetHashCode();
				hash = (hash * 23) + EndOfLife.GetHashCode();
				return hash;
			}
		}
	}
}