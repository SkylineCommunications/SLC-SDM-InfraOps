namespace Skyline.DataMiner.SDM.AssetManagement.Helpers
{
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.AssetManagement.Validation;

	public interface IAssetManagementApiHelper
	{
		IConnection Connection { get; }

		IBulkRepository<Asset> Assets { get; }

		IBulkRepository<AssetClass> AssetClasses { get; }

		IBulkRepository<PowerPort> PowerPorts { get; }

		IBulkRepository<DataPort> DataPorts { get; }

		IBulkRepository<DeviceType> DeviceTypes { get; }

        IBulkRepository<PortType> PortTypes { get; }

        AssetClassValidator AssetClassValidator { get; }

        //DeviceTypeValidator DeviceTypeValidator { get; }
    }
}