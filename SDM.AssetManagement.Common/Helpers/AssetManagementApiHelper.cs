namespace Skyline.DataMiner.SDM.AssetManagement
{
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.SDM.AssetManagement.Helpers;
	using Skyline.DataMiner.SDM.AssetManagement.Models;

	public class AssetManagementApiHelper : IAssetManagementApiHelper
	{
		public AssetManagementApiHelper(IConnection connection)
		{
			Connection = connection;
			Assets = new AssetDomRepository(connection);
			AssetClasses = new AssetClassDomRepository(connection);
			PowerPorts = new PowerPortDomRepository(connection);
			DataPorts = new DataPortDomRepository(connection);
			DeviceTypes = new DeviceTypeDomRepository(connection);
		}

		public IConnection Connection { get; }

		public IBulkRepository<Asset> Assets { get; }

		public IBulkRepository<AssetClass> AssetClasses { get; }

		public IBulkRepository<PowerPort> PowerPorts { get; }

		public IBulkRepository<DataPort> DataPorts { get; }

		public IBulkRepository<DeviceType> DeviceTypes { get; }
	}
}