namespace Skyline.DataMiner.SDM.InfraOpsProperties.Helpers
{
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Middleware;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Models;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Validation;
	using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Middleware;

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
		/// <param name="externalReferenceChecker">
		/// Optional cross-module checker used to verify referenced ids exist. When <c>null</c>, those checks are skipped.
		/// </param>
		public InfraOpsPropertiesApiHelper(
			IConnection connection,
			bool cascadeDeleteOnProperty = true,
			IInfraOpsPropertiesExternalReferenceChecker externalReferenceChecker = null)
		{
			Connection = connection;

			var propertyValidator = new PropertyValidator(this);
			var propertyValuesValidator = new PropertyValuesValidator(this, externalReferenceChecker);

			Properties = new PropertyDomRepository(connection)
				.WithMiddleware(new PropertyValidationMiddleware(propertyValidator))
				.WithMiddleware(new PropertyCascadeDeleteMiddleware(this, cascadeDeleteOnProperty))
				.WithMiddleware(new IdentifierMiddleware<Property>());

			PropertyValues = new PropertyValuesDomRepository(connection)
					.WithMiddleware(new PropertyValuesValidationMiddleware(propertyValuesValidator))
					.WithMiddleware(new IdentifierMiddleware<PropertyValues>());
		}

		public IConnection Connection { get; }

		public IPropertyRepository Properties { get; }

		public IPropertyValuesRepository PropertyValues { get; }
	}
}
