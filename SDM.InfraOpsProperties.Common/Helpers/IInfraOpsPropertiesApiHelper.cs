namespace Skyline.DataMiner.SDM.InfraOpsProperties.Helpers
{
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Models;

	public interface IInfraOpsPropertiesApiHelper
	{
		IConnection Connection { get; }

		IPropertyRepository Properties { get; }

		IPropertyValuesRepository PropertyValues { get; }
	}
}
