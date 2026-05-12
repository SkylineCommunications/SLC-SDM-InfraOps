namespace SDM.AssetManagement.Tests
{
    using SDM.AssetManagement.Tests.Setup;

    using Skyline.DataMiner.SDM.AssetManagement.Helpers;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;

    public static partial class RepositoryInitialize
    {
        public static ITestApiHelper InitializeEmptyRepositories()
        {
            var connection = ConnectionHelper.CreateConnection();
            var assetHelper = connection.GetMockedAssetManagementHelper();
            var facilityHelper = connection.GetMockedFacilityManagementHelper();
            
            return new TestApiHelper(assetHelper, facilityHelper);
        }

        /// <summary>
        /// Populates the Assets repository with the provided <paramref name="assets"/> test data.
        /// </summary>
        /// <param name="helper">Composite test helper.</param>
        /// <param name="assets">Predefined collection of <see cref="Asset"/>s to create.</param>
        /// <returns><see cref="ITestApiHelper"/> with populated data.</returns>
        public static ITestApiHelper PopulateAssets(this ITestApiHelper helper, IEnumerable<Asset> assets)
        {
            if (assets is null || !assets.Any())
            {
                return helper.PopulateAssets();
            }

            helper.AssetManagement.Assets.Create(assets);

            return helper;
        }

        /// <summary>
        /// Populates the Assets repository with default <seealso cref="Asset"/> test data.
        /// </summary>
        /// <param name="helper">Composite test helper.</param>
        /// <returns><see cref="ITestApiHelper"/> with populated data.</returns>
        public static ITestApiHelper PopulateAssets(this ITestApiHelper helper)
        {
            helper.AssetManagement.Assets.Create(DemoData.Assets);

            return helper;
        }

        /// <summary>
        /// Populates the AssetClasses repository with the provided <paramref name="assetClasses"/> test data.
        /// </summary>
        /// <param name="helper">Composite test helper.</param>
        /// <param name="assetClasses">Predefined collection of <see cref="AssetClass"/>es to create.</param>
        /// <returns><see cref="ITestApiHelper"/> with populated data.</returns>
        public static ITestApiHelper PopulateAssetClasses(this ITestApiHelper helper, IEnumerable<AssetClass> assetClasses)
        {
            if (assetClasses is null || !assetClasses.Any())
            {
                return helper.PopulateAssetClasses();
            }

            helper.AssetManagement.AssetClasses.Create(assetClasses);

            return helper;
        }

        /// <summary>
        /// Populates the AssetClasses repository with default <seealso cref="AssetClass"/> test data.
        /// </summary>
        /// <param name="helper">Composite test helper.</param>
        /// <returns><see cref="ITestApiHelper"/> with populated data.</returns>
        public static ITestApiHelper PopulateAssetClasses(this ITestApiHelper helper)
        {
            helper.AssetManagement.AssetClasses.Create(DemoData.AssetClasses);

            return helper;
        }

        /// <summary>
        /// Populates the DataPorts repository with default <seealso cref="DataPort"/> test data.
        /// </summary>
        /// <param name="helper">Composite test helper.</param>
        /// <returns><see cref="ITestApiHelper"/> with populated data.</returns>
        public static ITestApiHelper PopulateDataPorts(this ITestApiHelper helper)
        {
            helper.AssetManagement.DataPorts.Create(DemoData.DataPorts);

            return helper;
        }

        /// <summary>
        /// Populates the DataPorts repository with the provided <paramref name="dataPorts"/> test data.
        /// </summary>
        /// <param name="helper">Composite test helper.</param>
        /// <param name="dataPorts">Predefined collection of <see cref="DataPort"/>s to create.</param>
        /// <returns><see cref="ITestApiHelper"/> with populated data.</returns>
        public static ITestApiHelper PopulateDataPorts(this ITestApiHelper helper, IEnumerable<DataPort> dataPorts)
        {
            if (dataPorts is null || !dataPorts.Any())
            {
                return helper.PopulateDataPorts();
            }

            helper.AssetManagement.DataPorts.Create(dataPorts);

            return helper;
        }

        /// <summary>
        /// Populates the PowerPorts repository with default <seealso cref="PowerPort"/> test data.
        /// </summary>
        /// <param name="helper">Composite test helper.</param>
        /// <returns><see cref="ITestApiHelper"/> with populated data.</returns>
        public static ITestApiHelper PopulatePowerPorts(this ITestApiHelper helper)
        {
            helper.AssetManagement.PowerPorts.Create(DemoData.PowerPorts);

            return helper;
        }

        /// <summary>
        /// Populates the DeviceType repository with default <seealso cref="DeviceType"/> test data.
        /// </summary>
        /// <param name="helper">Composite test helper.</param>
        /// <returns><see cref="ITestApiHelper"/> with populated data.</returns>
        public static ITestApiHelper PopulateDeviceTypes(this ITestApiHelper helper)
        {
            helper.AssetManagement.DeviceTypes.Create(DemoData.DeviceTypes);

            return helper;
        }

        /// <summary>
        /// Populates the DeviceType repository with the provided <paramref name="deviceTypes"/> test data.
        /// </summary>
        /// <param name="helper">Composite test helper.</param>
        /// <param name="deviceTypes">Predefined collection of <see cref="DeviceType"/>s to create.</param>
        /// <returns><see cref="ITestApiHelper"/> with populated data.</returns>
        public static ITestApiHelper PopulateDeviceTypes(this ITestApiHelper helper, IEnumerable<DeviceType> deviceTypes)
        {
            if (deviceTypes is null || !deviceTypes.Any())
            {
                return helper.PopulateDeviceTypes();
            }

            helper.AssetManagement.DeviceTypes.Create(deviceTypes);

            return helper;
        }

        /// <summary>
        /// Populates the Racks repository with default <seealso cref="Rack"/> test data.
        /// </summary>
        /// <param name="helper">Composite test helper.</param>
        /// <returns><see cref="ITestApiHelper"/> with populated data.</returns>
        public static ITestApiHelper PopulateRacks(this ITestApiHelper helper)
        {
            helper.FacilityManagement.Racks.Create(DemoData.Racks);
            
            return helper;
        }

        /// <summary>
        /// Populates the Racks repository with the provided <paramref name="racks"/> test data.
        /// </summary>
        /// <param name="helper">Composite test helper.</param>
        /// <param name="racks">Predefined collection of <see cref="Rack"/>s to create.</param>
        /// <returns><see cref="ITestApiHelper"/> with populated data.</returns>
        public static ITestApiHelper PopulateRacks(this ITestApiHelper helper, IEnumerable<Rack> racks)
        {
            if (racks is null || !racks.Any())
            {
                return helper.PopulateRacks();
            }

            helper.FacilityManagement.Racks.Create(racks);

            return helper;
        }
    }
}