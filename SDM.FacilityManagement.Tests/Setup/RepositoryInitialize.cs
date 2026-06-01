namespace SDM.FacilityManagement.Tests
{
	using SDM.FacilityManagement.Tests.Setup;
	using Skyline.DataMiner.SDM.FacilityManagement.Helpers;
	using Skyline.DataMiner.SDM.FacilityManagement.Models;

	public static class RepositoryInitialize
	{
		public static IFacilityManagementApiHelper InitializeEmptyRepositories()
		{
			return ConnectionHelper.CreateConnection().GetMockedHelper();
		}

		/// <summary>
		/// Populates the Facilities repository with the provided <paramref name="facilities"/> test data.
		/// </summary>
		/// <param name="helper">Mocked API helper.</param>
		/// <param name="facilities">Predefined collection of <see cref="Facility"/> objects to create.</param>
		/// <returns><see cref="IFacilityManagementApiHelper"/> API helper interface with populated data.</returns>
		public static IFacilityManagementApiHelper PopulateFacilities(this IFacilityManagementApiHelper helper, IEnumerable<Facility> facilities)
		{
			if (facilities is null || !facilities.Any())
			{
				return helper.PopulateFacilities();
			}

			helper.Facilities.Create(facilities);

			return helper;
		}

		/// <summary>
		/// Populates the Facilities repository with default <seealso cref="Asset"/> test data.
		/// </summary>
		/// <param name="helper">Mocked API helper.</param>
		/// <returns><see cref="IAssetManagementApiHelper"/> API helper interface with populated data.</returns>
		public static IFacilityManagementApiHelper PopulateFacilities(this IFacilityManagementApiHelper helper)
		{
			helper.Facilities.Create(DemoData.Facilities);

			return helper;
		}

		/// <summary>
		/// Populates the Racks repository with default <see cref="Rack"/> test data.
		/// </summary>
		/// <param name="helper">Mocked API helper.</param>
		/// <returns><see cref="IFacilityManagementApiHelper"/> API helper interface with populated data.</returns>
		public static IFacilityManagementApiHelper PopulateRacks(this IFacilityManagementApiHelper helper)
		{
			helper.Racks.Create(DemoData.Racks);

			return helper;
		}

		/// <summary>
		/// Populates the Rooms repository with default <see cref="Room"/> test data.
		/// </summary>
		/// <param name="helper">Mocked API helper.</param>
		/// <returns><see cref="IFacilityManagementApiHelper"/> API helper interface with populated data.</returns>
		public static IFacilityManagementApiHelper PopulateRooms(this IFacilityManagementApiHelper helper)
		{
			helper.Rooms.Create(DemoData.Rooms);

			return helper;
		}
	}
}