namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
	using System;

	// [GenerateExposers]
	// [SdmDomStorage("(slc)asset_management")]
	public sealed class DataPort : SdmObject<DataPort>, IEquatable<DataPort>
	{
		public DataPortInfo DataPortInfo { get; set; } = new DataPortInfo();

		// within AssetRelation section definition
		public SdmObjectReference<Asset> Asset { get; set; }

		public AddressInfo AddressInfo { get; set; } = new AddressInfo();

		public PrimaryPortRelation PrimaryPortRelation { get; set; } = new PrimaryPortRelation();

		public bool Equals(DataPort other)
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
				Equals(DataPortInfo, other.DataPortInfo) &&
				Equals(Asset, other.Asset) &&
				Equals(AddressInfo, other.AddressInfo) &&
				Equals(PrimaryPortRelation, other.PrimaryPortRelation);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as DataPort);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = (2 << 12) - 1;
				hash = (hash * 23) + (DataPortInfo != null ? DataPortInfo.GetHashCode() : 0);
				hash = (hash * 23) + (Asset != null ? Asset.GetHashCode() : 0);
				hash = (hash * 23) + (AddressInfo != null ? AddressInfo.GetHashCode() : 0);
				hash = (hash * 23) + (PrimaryPortRelation != null ? PrimaryPortRelation.GetHashCode() : 0);
				return hash;
			}
		}
	}

	public sealed class DataPortInfo : SdmObject<DataPortInfo>, IEquatable<DataPortInfo>
	{
		public string Name { get; set; }

		public long PortNumber { get; set; }

		public SlcAssetManagement.Enums.Outputtype OutputType { get; set; }

		public SlcAssetManagement.Enums.PortExposure PortExposure { get; set; }

		public Guid Type { get; set; } = Guid.Empty;

		public string Label { get; set; }

		public bool Equals(DataPortInfo other)
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
				string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase) &&
				PortNumber == other.PortNumber &&
				OutputType == other.OutputType &&
				PortExposure == other.PortExposure &&
				Type == other.Type &&
				string.Equals(Label, other.Label, StringComparison.OrdinalIgnoreCase);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as DataPortInfo);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = (2 << 12) - 1;
				hash = (hash * 23) + (Name != null ? Name.GetHashCode() : 0);
				hash = (hash * 23) + PortNumber.GetHashCode();
				hash = (hash * 23) + OutputType.GetHashCode();
				hash = (hash * 23) + PortExposure.GetHashCode();
				hash = (hash * 23) + Type.GetHashCode();
				hash = (hash * 23) + (Label != null ? Label.GetHashCode() : 0);
				return hash;
			}
		}
	}

	public sealed class AddressInfo : IEquatable<AddressInfo>
	{
		public string Ipv4Address { get; set; }

		public string Ipv6Address { get; set; }

		public string Hostname { get; set; }

		public bool DNS { get; set; }

		public static bool operator ==(AddressInfo left, AddressInfo right)
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

		public static bool operator !=(AddressInfo left, AddressInfo right)
		{
			return !(left == right);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as AddressInfo);
		}

		public bool Equals(AddressInfo other)
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
				string.Equals(Ipv4Address, other.Ipv4Address, StringComparison.OrdinalIgnoreCase) &&
				string.Equals(Ipv6Address, other.Ipv6Address, StringComparison.OrdinalIgnoreCase) &&
				string.Equals(Hostname, other.Hostname, StringComparison.OrdinalIgnoreCase) &&
				DNS == other.DNS;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = (2 << 12) - 1;
				hash = (hash * 23) + (Ipv4Address != null ? Ipv4Address.GetHashCode() : 0);
				hash = (hash * 23) + (Ipv6Address != null ? Ipv6Address.GetHashCode() : 0);
				hash = (hash * 23) + (Hostname != null ? Hostname.GetHashCode() : 0);
				hash = (hash * 23) + DNS.GetHashCode();
				return hash;
			}
		}
	}

	public sealed class PrimaryPortRelation : IEquatable<PrimaryPortRelation>
	{
		public bool IsPrimaryIpv6 { get; set; }

		public bool IsPrimaryIpv4 { get; set; }

		public override bool Equals(object obj)
		{
			return Equals(obj as PrimaryPortRelation);
		}

		public bool Equals(PrimaryPortRelation other)
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
				IsPrimaryIpv6 == other.IsPrimaryIpv6 &&
				IsPrimaryIpv4 == other.IsPrimaryIpv4;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = (2 << 12) - 1;
				hash = (hash * 23) + IsPrimaryIpv6.GetHashCode();
				hash = (hash * 23) + IsPrimaryIpv4.GetHashCode();
				return hash;
			}
		}
	}
}