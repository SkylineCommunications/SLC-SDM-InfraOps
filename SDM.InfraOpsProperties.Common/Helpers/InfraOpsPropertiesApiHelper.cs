namespace Skyline.DataMiner.SDM.InfraOpsProperties.Helpers
{
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Models;

	public class InfraOpsPropertiesApiHelper : IInfraOpsPropertiesApiHelper
	{
		public InfraOpsPropertiesApiHelper(IConnection connection)
		{
			Connection = connection;
			//Properties = new PropertyDomRepository(connection);
			//PropertyValues = new PropertyValuesDomRepository(connection);
		}

		public IConnection Connection { get; }

		public IBulkRepository<Property> Properties { get; }

		public IBulkRepository<PropertyValues> PropertyValues { get; }
	}
}
