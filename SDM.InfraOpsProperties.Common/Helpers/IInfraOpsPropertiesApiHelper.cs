namespace Skyline.DataMiner.SDM.InfraOpsProperties.Helpers
{
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Models;

	public interface IInfraOpsPropertiesApiHelper
	{
		IConnection Connection { get; }

		IBulkRepository<Property> Properties { get; }

		IBulkRepository<PropertyValues> PropertyValues { get; }
	}
}
