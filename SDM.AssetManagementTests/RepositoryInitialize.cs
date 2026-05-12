namespace SDM.AssetManagement.Tests
{
    using SDM.AssetManagement.Tests.Setup;

    using Skyline.DataMiner.SDM.AssetManagement.Helpers;
    using Skyline.DataMiner.SDM.AssetManagement.Models;

    public static partial class RepositoryInitialize
    {
        public static IAssetManagementApiHelper InitializeEmptyRepositories()
        {
            return ConnectionHelper.CreateConnection().GetMockedHelper();
        }


        /// <summary>
        /// Populates the Assets repository with the provided <paramref name="assets"/> test data.
        /// </summary>
        /// <param name="helper">Mocked API helper.</param>
        /// <param name="assets">Predefined collection of <see cref="Asset"/>s to create.</param>
        /// <returns><see cref="IAssetManagementApiHelper"/> API helper interface with populated data.</returns>
        public static IAssetManagementApiHelper PopulateAssets(this IAssetManagementApiHelper helper, IEnumerable<Asset> assets)
        {
            if (assets is null || !assets.Any())
            {
                return helper.PopulateAssets();
            }

            helper.Assets.Create(assets);

            return helper;
        }

        /// <summary>
        /// Populates the Assets repository with default <seealso cref="Asset"/> test data.
        /// </summary>
        /// <param name="helper">Mocked API helper.</param>
        /// <returns><see cref="IAssetManagementApiHelper"/> API helper interface with populated data.</returns>
        public static IAssetManagementApiHelper PopulateAssets(this IAssetManagementApiHelper helper)
        {
            helper.Assets.Create(DemoData.Assets);

            return helper;
        }

        /// <summary>
        /// Populates the Assets repository with the provided <paramref name="assetClasses"/> test data.
        /// </summary>
        /// <param name="helper">Mocked API helper.</param>
        /// <param name="assetClasses">Predefined collection of <see cref="AssetClass"/>es to create.</param>
        /// <returns><see cref="IAssetManagementApiHelper"/> API helper interface with populated data.</returns>
        public static IAssetManagementApiHelper PopulateAssetClasses(this IAssetManagementApiHelper helper, IEnumerable<AssetClass> assetClasses)
        {
            if (assetClasses is null || !assetClasses.Any())
            {
                return helper.PopulateAssetClasses();
            }

            helper.AssetClasses.Create(assetClasses);

            return helper;
        }

        /// <summary>
        /// Populates the AssetClasses repository with default <seealso cref="AssetClass"/> test data.
        /// </summary>
        /// <param name="helper">Mocked API helper.</param>
        /// <returns><see cref="IAssetManagementApiHelper"/> API helper interface with populated data.</returns>
        public static IAssetManagementApiHelper PopulateAssetClasses(this IAssetManagementApiHelper helper)
        {
            helper.AssetClasses.Create(DemoData.AssetClasses);

            return helper;
        }

        /// <summary>
        /// Populates the DataPorts repository with default <seealso cref="DataPort"/> test data.
        /// </summary>
        /// <param name="helper">Mocked API helper.</param>
        /// <returns><see cref="IAssetManagementApiHelper"/> API helper interface with populated data.</returns>
        public static IAssetManagementApiHelper PopulateDataPorts(this IAssetManagementApiHelper helper)
        {
            helper.DataPorts.Create(DemoData.DataPorts);

            return helper;
        }

        /// <summary>
        /// Populates the DataPorts repository with the provided <paramref name="dataPorts"/> test data.
        /// </summary>
        /// <param name="helper">Mocked API helper.</param>
        /// <param name="dataPorts">Predefined collection of <see cref="DataPort"/>s to create.</param>
        /// <returns><see cref="IAssetManagementApiHelper"/> API helper interface with populated data.</returns>
        public static IAssetManagementApiHelper PopulateDataPorts(this IAssetManagementApiHelper helper, IEnumerable<DataPort> dataPorts)
        {
            if (dataPorts is null || !dataPorts.Any())
            {
                return helper.PopulateDataPorts();
            }

            helper.DataPorts.Create(dataPorts);

            return helper;
        }

        /// <summary>
        /// Populates the PowerPorts repository with default <seealso cref="PowerPort"/> test data.
        /// </summary>
        /// <param name="helper">Mocked API helper.</param>
        /// <returns><see cref="IAssetManagementApiHelper"/> API helper interface with populated data.</returns>
        public static IAssetManagementApiHelper PopulatePowerPorts(this IAssetManagementApiHelper helper)
        {
            helper.PowerPorts.Create(DemoData.PowerPorts);

            return helper;
        }

        /// <summary>
        /// Populates the DeviceType repository with default <seealso cref="DeviceType"/> test data.
        /// </summary>
        /// <param name="helper">Mocked API helper.</param>
        /// <returns><see cref="IPeopleAndOrganizationsApiHelper"/> API helper interface with populated data.</returns>
        public static IAssetManagementApiHelper PopulateDeviceTypes(this IAssetManagementApiHelper helper)
        {
            helper.DeviceTypes.Create(DemoData.DeviceTypes);

            return helper;
        }

        /// <summary>
        /// Populates the DeviceType repository with the provided <paramref name="deviceTypes"/> test data.
        /// </summary>
        /// <param name="helper">Mocked API helper.</param>
        /// <param name="deviceTypes">Predefined collection of <see cref="DeviceType"/>s to create.</param>
        /// <returns><see cref="IPeopleAndOrganizationsApiHelper"/> API helper interface with populated data.</returns>
        public static IAssetManagementApiHelper PopulateDeviceTypes(this IAssetManagementApiHelper helper, IEnumerable<DeviceType> deviceTypes)
        {
            if (deviceTypes is null || !deviceTypes.Any())
            {
                return helper.PopulateDeviceTypes();
            }

            helper.DeviceTypes.Create(deviceTypes);

            return helper;
        }
    }
}