namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;

    /// <summary>
    /// Exposers for the fields shared by the DataPort and PowerPort DOM definitions.
    /// Filters built with these exposers can be passed to the port repository, which
    /// applies them to both definitions using each definition's own field descriptors.
    /// </summary>
    public static class PortExposers
	{
		public static readonly Exposer<IPort, string> Identifier = new Exposer<IPort, string>(
			(obj) => obj is DataPort dataPort ? dataPort.Identifier : (obj as PowerPort)?.Identifier,
			"Identifier");

		public static readonly Exposer<IPort, SdmObjectReference<AssetManagement.Models.Asset>> Asset = new Exposer<IPort, SdmObjectReference<AssetManagement.Models.Asset>>(
			(obj) => obj is DataPort dataPort ? dataPort.Asset : (obj as PowerPort)?.Asset ?? default,
			"Asset");

		public static class PortInfo
		{
			public static readonly Exposer<IPort, string> Name = new Exposer<IPort, string>(
				(obj) => obj is DataPort dataPort ? dataPort.DataPortInfo.Name : (obj as PowerPort)?.PowerPortInfo.Name,
				"PortInfo.Name");

			public static readonly Exposer<IPort, long?> PortNumber = new Exposer<IPort, long?>(
				(obj) => obj is DataPort dataPort ? dataPort.DataPortInfo.PortNumber : (obj as PowerPort)?.PowerPortInfo.PortNumber,
				"PortInfo.PortNumber");

			public static readonly Exposer<IPort, SlcAsset_Management.Enums.Outputtype?> OutputType = new Exposer<IPort, SlcAsset_Management.Enums.Outputtype?>(
				(obj) => obj is DataPort dataPort ? dataPort.DataPortInfo.OutputType : (obj as PowerPort)?.PowerPortInfo.OutputType,
				"PortInfo.OutputType");

			public static readonly Exposer<IPort, SlcAsset_Management.Enums.PortExposureEnum> PortExposure = new Exposer<IPort, SlcAsset_Management.Enums.PortExposureEnum>(
				(obj) => obj is DataPort dataPort ? dataPort.DataPortInfo.PortExposure : (obj as PowerPort)?.PowerPortInfo.PortExposure ?? default,
				"PortInfo.PortExposure");

			public static readonly Exposer<IPort, SdmObjectReference<AssetManagement.Models.PortType>> Type = new Exposer<IPort, SdmObjectReference<AssetManagement.Models.PortType>>(
				(obj) => obj is DataPort dataPort ? dataPort.DataPortInfo.Type : (obj as PowerPort)?.PowerPortInfo.PortType ?? default,
				"PortInfo.Type");

			public static readonly Exposer<IPort, string> Label = new Exposer<IPort, string>(
				(obj) => obj is DataPort dataPort ? dataPort.DataPortInfo.Label : (obj as PowerPort)?.PowerPortInfo.Label,
				"PortInfo.Label");
		}

		/// <summary>
		/// Exposers for fields that only exist on the DataPort definition.
		/// Using any of these in a filter makes the PowerPort definition useless to filter,
		/// so the repository will only query the DataPort definition.
		/// Combining these with <see cref="PowerPortOnly"/> exposers yields an empty result.
		/// </summary>
		public static class DataPortOnly
		{
			public static class AddressInfo
			{
				public static readonly Exposer<IPort, string> Ipv4Address = new Exposer<IPort, string>(
					(obj) => (obj as DataPort)?.AddressInfo.Ipv4Address,
					"AddressInfo.Ipv4Address");

				public static readonly Exposer<IPort, string> Ipv6Address = new Exposer<IPort, string>(
					(obj) => (obj as DataPort)?.AddressInfo.Ipv6Address,
					"AddressInfo.Ipv6Address");

				public static readonly Exposer<IPort, string> Hostname = new Exposer<IPort, string>(
					(obj) => (obj as DataPort)?.AddressInfo.Hostname,
					"AddressInfo.Hostname");

				public static readonly Exposer<IPort, bool> DNS = new Exposer<IPort, bool>(
					(obj) => obj is DataPort dataPort && dataPort.AddressInfo.DNS,
					"AddressInfo.DNS");
			}

			public static class PrimaryPortRelation
			{
				public static readonly Exposer<IPort, bool> IsPrimaryIpv6 = new Exposer<IPort, bool>(
					(obj) => obj is DataPort dataPort && dataPort.PrimaryPortRelation.IsPrimaryIpv6,
					"PrimaryPortRelation.IsPrimaryIpv6");

				public static readonly Exposer<IPort, bool> IsPrimaryIpv4 = new Exposer<IPort, bool>(
					(obj) => obj is DataPort dataPort && dataPort.PrimaryPortRelation.IsPrimaryIpv4,
					"PrimaryPortRelation.IsPrimaryIpv4");
			}
		}

		/// <summary>
		/// Exposers for fields that only exist on the PowerPort definition.
		/// Using any of these in a filter makes the DataPort definition useless to filter,
		/// so the repository will only query the PowerPort definition.
		/// Combining these with <see cref="DataPortOnly"/> exposers yields an empty result.
		/// The PowerPort definition currently has no fields that the DataPort definition lacks.
		/// </summary>
		public static class PowerPortOnly
		{
		}
	}
}
