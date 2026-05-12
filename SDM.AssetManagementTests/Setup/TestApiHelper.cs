namespace SDM.AssetManagement.Tests.Setup
{
    using Skyline.DataMiner.SDM.AssetManagement.Helpers;
    using Skyline.DataMiner.SDM.FacilityManagement.Helpers;

    /// <summary>
    /// Composite helper interface that provides access to both Asset Management and Facility Management APIs.
    /// </summary>
    public interface ITestApiHelper
    {
        IAssetManagementApiHelper AssetManagement { get; }
        IFacilityManagementApiHelper FacilityManagement { get; }
    }

    /// <summary>
    /// Composite helper class that provides access to both Asset Management and Facility Management APIs.
    /// </summary>
    public class TestApiHelper : ITestApiHelper
    {
        public TestApiHelper(IAssetManagementApiHelper assetManagement, IFacilityManagementApiHelper facilityManagement)
        {
            AssetManagement = assetManagement ?? throw new ArgumentNullException(nameof(assetManagement));
            FacilityManagement = facilityManagement ?? throw new ArgumentNullException(nameof(facilityManagement));
        }

        public IAssetManagementApiHelper AssetManagement { get; }
        public IFacilityManagementApiHelper FacilityManagement { get; }
    }
}