namespace Skyline.DataMiner.SDM.InfraOpsProperties.Helpers
{
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Middleware;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Models;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Validation;

	public class InfraOpsPropertiesApiHelper : IInfraOpsPropertiesApiHelper
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="InfraOpsPropertiesApiHelper"/> class.
		/// </summary>
		/// <param name="connection">The DataMiner connection.</param>
		/// <param name="cascadeDeleteOnProperty">
		/// When <c>true</c> (default), deleting a Property removes any PropertyValue entries referencing it from
		/// all PropertyValues instances first, preventing orphaned references. Set to <c>false</c> to opt out.
		/// </param>
		public InfraOpsPropertiesApiHelper(IConnection connection, bool cascadeDeleteOnProperty = true)
		{
			Connection = connection;

			var propertyValidator = new PropertyValidator(this);
			var propertyValuesValidator = new PropertyValuesValidator(this);

			Properties = new PropertyDomRepository(connection)
				.WithMiddleware(new PropertyValidationMiddleware(propertyValidator, this, cascadeDeleteOnProperty))
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
