namespace SDM.AssetManagement.Tests.Setup
{
    using Skyline.DataMiner.Net;
    using Skyline.DataMiner.SDM.AssetManagement.Helpers;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.FacilityManagement.Helpers;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;

    /// <summary>
    /// Composite helper interface that provides access to both Asset Management and Facility Management APIs,
    /// along with test data caching for populated entities.
    /// </summary>
    public interface ITestApiHelper
    {
        /// <summary>
        /// Access to Asset Management repositories.
        /// </summary>
        IAssetManagementApiHelper AssetManagement { get; }
        
        /// <summary>
        /// Access to Facility Management repositories.
        /// </summary>
        IFacilityManagementApiHelper FacilityManagement { get; }

        /// <summary>
        /// Test data cache - persisted entities from Populate methods.
        /// Use these instead of DemoData templates for accurate test assertions.
        /// </summary>
        ITestDataCache TestData { get; }
    }

    /// <summary>
    /// Cached persisted test data. Populated by RepositoryInitialize.Populate* methods.
    /// </summary>
    public interface ITestDataCache
    {
        IReadOnlyList<DeviceType> DeviceTypes { get; internal set; }
        IReadOnlyList<AssetClass> AssetClasses { get; internal set; }
        IReadOnlyList<Asset> Assets { get; internal set; }
        IReadOnlyList<DataPort> DataPorts { get; internal set; }
        IReadOnlyList<PowerPort> PowerPorts { get; internal set; }
        IReadOnlyList<Rack> Racks { get; internal set; }
    }

    /// <summary>
    /// Internal implementation of test data cache.
    /// </summary>
    internal class TestDataCache : ITestDataCache
    {
        public IReadOnlyList<DeviceType> DeviceTypes { get; set; } = Array.Empty<DeviceType>();
        public IReadOnlyList<AssetClass> AssetClasses { get; set; } = Array.Empty<AssetClass>();
        public IReadOnlyList<Asset> Assets { get; set; } = Array.Empty<Asset>();
        public IReadOnlyList<DataPort> DataPorts { get; set; } = Array.Empty<DataPort>();
        public IReadOnlyList<PowerPort> PowerPorts { get; set; } = Array.Empty<PowerPort>();
        public IReadOnlyList<Rack> Racks { get; set; } = Array.Empty<Rack>();
    }

    /// <summary>
    /// Composite helper class that provides access to both Asset Management and Facility Management APIs.
    /// </summary>
    public class TestApiHelper : ITestApiHelper
    {
        public TestApiHelper(IConnection connection)
        {
            AssetManagement = new AssetManagementApiHelper(connection);
            FacilityManagement = new FacilityManagementApiHelper(connection);
            TestData = new TestDataCache();
        }

        public IAssetManagementApiHelper AssetManagement { get; }
        public IFacilityManagementApiHelper FacilityManagement { get; }
        public ITestDataCache TestData { get; }
    }
}