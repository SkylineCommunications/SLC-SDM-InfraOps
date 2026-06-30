namespace Skyline.DataMiner.SDM.AssetManagement.Helpers
{
	using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.AssetManagement.Validation;
    using Connection = Skyline.DataMiner.SDM.AssetManagement.Models.Connection;

    public interface IAssetManagementApiHelper
	{
		IAssetRepository Assets { get; }

        IBulkRepository<AssetManagerAppSettings> AppSettings { get; }

		IBulkRepository<AssetClass> AssetClasses { get; }

		IBulkRepository<PowerPort> PowerPorts { get; }

		IBulkRepository<DataPort> DataPorts { get; }

		IBulkRepository<DeviceType> DeviceTypes { get; }

        IBulkRepository<PortType> PortTypes { get; }

        IBulkRepository<Connection> Connections { get; }

        AssetClassValidator AssetClassValidator { get; }
    }
}