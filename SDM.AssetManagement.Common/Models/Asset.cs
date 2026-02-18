namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.SDM;

	// [GenerateExposers]
	// [SdmDomStorage("(slc)asset_management")]
	public class Asset : SdmObject<Asset>
	{
		public string AssetId { get; set; }

		public string AssetName { get; set; }

		public SdmObjectReference<AssetClass> AssetClass { get; set; }

		public string AssetDescription { get; set; }

		public string FwOs { get; set; }

		public string Notes { get; set; }

		public string SerialNumber { get; set; }

		public string HardwareVersion { get; set; }

		/// <summary>
		/// Gets or sets the network details of the asset.
		/// </summary>
		public AssetNetworkDetails NetworkDetails { get; set; } = new AssetNetworkDetails();

		/// <summary>
		/// Gets or sets the location details of the asset.
		/// </summary>
		public AssetLocation Location { get; set; } = new AssetLocation();

		/// <summary>
		/// Gets or sets the lifecycle information of the asset.
		/// </summary>
		public AssetLifecycle Lifecycle { get; set; } = new AssetLifecycle();

		/// <summary>
		/// Gets or sets the ownership information of the asset.
		/// </summary>
		public AssetOwnership Ownership { get; set; } = new AssetOwnership();

		/// <summary>
		/// Gets or sets the custody information of the asset.
		/// </summary>
		public AssetCustody Custody { get; set; } = new AssetCustody();

		/// <summary>
		/// Gets or sets the list of holders (slots) associated with the asset.
		/// </summary>
		public List<AssetHolder> Holders { get; set; } = new List<AssetHolder>();

		/// <summary>
		/// Gets or sets the list of DataMiner element links.
		/// </summary>
		public List<ElementLink> ElementLinks { get; set; } = new List<ElementLink>();
	}

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