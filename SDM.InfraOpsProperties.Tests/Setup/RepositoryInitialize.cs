namespace SDM.InfraOpsProperties.Tests
{
	using SDM.InfraOpsProperties.Tests.Setup;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Helpers;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Models;

	public static class RepositoryInitialize
	{
		public static IInfraOpsPropertiesApiHelper InitializeEmptyRepositories()
		{
			return ConnectionHelper.CreateConnection().GetMockedHelper();
		}

		/// <summary>
		/// Initializes an empty set of repositories using an API helper with a specific cascade-delete-on-Property setting.
		/// </summary>
		/// <param name="cascadeDeleteOnProperty">
		/// When <c>false</c>, deleting a Property will not cascade-remove referencing PropertyValue entries.
		/// </param>
		public static IInfraOpsPropertiesApiHelper InitializeEmptyRepositories(bool cascadeDeleteOnProperty)
		{
			return ConnectionHelper.CreateConnection().GetMockedHelper(cascadeDeleteOnProperty);
		}

		/// <summary>
		/// Populates the Properties repository with the provided <paramref name="properties"/> test data.
		/// </summary>
		/// <param name="helper">Mocked API helper.</param>
		/// <param name="properties">Predefined collection of <see cref="Property"/> objects to create.</param>
		/// <returns><see cref="IInfraOpsPropertiesApiHelper"/> API helper interface with populated data.</returns>
		public static IInfraOpsPropertiesApiHelper PopulateProperties(this IInfraOpsPropertiesApiHelper helper, IEnumerable<Property> properties)
		{
			if (properties is null || !properties.Any())
			{
				return helper.PopulateProperties();
			}

			helper.Properties.Create(properties);

			return helper;
		}

		/// <summary>
		/// Populates the Properties repository with default <see cref="Property"/> test data.
		/// </summary>
		/// <param name="helper">Mocked API helper.</param>
		/// <returns><see cref="IInfraOpsPropertiesApiHelper"/> API helper interface with populated data.</returns>
		public static IInfraOpsPropertiesApiHelper PopulateProperties(this IInfraOpsPropertiesApiHelper helper)
		{
			helper.Properties.Create(DemoData.Properties);

			return helper;
		}

		/// <summary>
		/// Populates the PropertyValues repository with the provided <paramref name="propertyValuesList"/> test data.
		/// </summary>
		/// <param name="helper">Mocked API helper.</param>
		/// <param name="propertyValuesList">Predefined collection of <see cref="PropertyValues"/> objects to create.</param>
		/// <returns><see cref="IInfraOpsPropertiesApiHelper"/> API helper interface with populated data.</returns>
		public static IInfraOpsPropertiesApiHelper PopulatePropertyValues(this IInfraOpsPropertiesApiHelper helper, IEnumerable<PropertyValues> propertyValuesList)
		{
			if (propertyValuesList is null || !propertyValuesList.Any())
			{
				return helper.PopulatePropertyValues();
			}

			helper.PropertyValues.Create(propertyValuesList);

			return helper;
		}

		/// <summary>
		/// Populates the PropertyValues repository with default <see cref="PropertyValues"/> test data.
		/// </summary>
		/// <param name="helper">Mocked API helper.</param>
		/// <returns><see cref="IInfraOpsPropertiesApiHelper"/> API helper interface with populated data.</returns>
		public static IInfraOpsPropertiesApiHelper PopulatePropertyValues(this IInfraOpsPropertiesApiHelper helper)
		{
			if (!helper.Properties.Read(new TRUEFilterElement<Property>()).Any())
			{
				helper.Properties.Create(DemoData.Properties);
			}

			helper.PropertyValues.Create(DemoData.PropertyValuesList);

			return helper;
		}
	}
}
