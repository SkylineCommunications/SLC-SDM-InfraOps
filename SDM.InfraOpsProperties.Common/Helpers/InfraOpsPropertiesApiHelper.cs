namespace Skyline.DataMiner.SDM.InfraOpsProperties.Helpers
{
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Middleware;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Models;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Validation;

	public class InfraOpsPropertiesApiHelper : IInfraOpsPropertiesApiHelper
	{
		public InfraOpsPropertiesApiHelper(IConnection connection)
		{
			Connection = connection;

			var propertyValidator = new PropertyValidator();
			var propertyValuesValidator = new PropertyValuesValidator();

			Properties = new PropertyDomRepository(connection)
				.WithMiddleware(new PropertyValidationMiddleware(propertyValidator))
				.WithMiddleware(new IdentifierMiddleware<Property>());

			PropertyValues = new PropertyValuesDomRepository(connection)
				.WithMiddleware(new PropertyValuesValidationMiddleware(propertyValuesValidator))
				.WithMiddleware(new IdentifierMiddleware<PropertyValues>());
		}

		public IConnection Connection { get; }

		public IBulkRepository<Property> Properties { get; }

		public IBulkRepository<PropertyValues> PropertyValues { get; }
	}
}
