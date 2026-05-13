namespace SDM.AssetManagement.Tests.Setup
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    [TestClass]
    public class RepositoryInitializeTests
    {
        #region InitializeEmptyRepositories Tests

        [TestMethod]
        public void InitializeEmptyRepositories_ShouldReturnValidHelper()
        {
            // Act
            var helper = RepositoryInitialize.InitializeEmptyRepositories();

            // Assert
            Assert.IsNotNull(helper);
            Assert.IsNotNull(helper.AssetManagement.Assets);
            Assert.IsNotNull(helper.AssetManagement.AssetClasses);
            Assert.IsNotNull(helper.AssetManagement.DeviceTypes);
            Assert.IsNotNull(helper.AssetManagement.DataPorts);
            Assert.IsNotNull(helper.AssetManagement.PowerPorts);
        }

        [TestMethod]
        public void InitializeEmptyRepositories_ShouldHaveEmptyRepositories()
        {
            // Act
            var helper = RepositoryInitialize.InitializeEmptyRepositories();

            // Assert
            Assert.AreEqual(0, helper.AssetManagement.Assets.Count(new TRUEFilterElement<Asset>()));
            Assert.AreEqual(0, helper.AssetManagement.AssetClasses.Count(new TRUEFilterElement<AssetClass>()));
            Assert.AreEqual(0, helper.AssetManagement.DeviceTypes.Count(new TRUEFilterElement<DeviceType>()));
            Assert.AreEqual(0, helper.AssetManagement.DataPorts.Count(new TRUEFilterElement<DataPort>()));
            Assert.AreEqual(0, helper.AssetManagement.PowerPorts.Count(new TRUEFilterElement<PowerPort>()));
        }

        #endregion

        #region PopulateAssets Tests

        [TestMethod]
        public void PopulateAssets_WithDefaultData_ShouldPopulateRepository()
        {
            // Arrange
            var helper = RepositoryInitialize.InitializeEmptyRepositories()
                .PopulateDeviceTypes()
                .PopulateAssetClasses();

            // Act
            helper.PopulateAssets();

            // Assert
            Assert.IsTrue(helper.AssetManagement.Assets.Count(new TRUEFilterElement<Asset>()) > 0, "Should populate with default demo data");
        }

        [TestMethod]
        public void PopulateAssets_WithValidCustomData_ShouldPopulateRepository()
        {
            // Arrange
            var helper = RepositoryInitialize.InitializeEmptyRepositories()
                .PopulateDeviceTypes()
                .PopulateAssetClasses();

            var assetClass = helper.AssetManagement.AssetClasses.Read(new TRUEFilterElement<AssetClass>()).First();
            var customAssets = new List<Asset>
            {
                CreateValidAsset("Test Asset 1", "TA-001", assetClass.Identifier),
                CreateValidAsset("Test Asset 2", "TA-002", assetClass.Identifier)
            };

            // Act
            helper.PopulateAssets(customAssets);

            // Assert
            var assets = helper.AssetManagement.Assets.Read(new TRUEFilterElement<Asset>()).ToList();
            Assert.IsTrue(assets.Count >= 2);
            Assert.IsTrue(assets.Any(a => a.Name == "Test Asset 1"));
            Assert.IsTrue(assets.Any(a => a.Name == "Test Asset 2"));
        }

        [TestMethod]
        public void PopulateAssets_WithNullCollection_ShouldThrowArgumentNullException()
        {
            // Arrange
            var helper = RepositoryInitialize.InitializeEmptyRepositories()
                .PopulateDeviceTypes()
                .PopulateAssetClasses();

            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(
                () => helper.PopulateAssets((IEnumerable<Asset>)null),
                "Should throw ArgumentNullException when null collection is passed");
        }

        [TestMethod]
        public void PopulateAssets_WithEmptyCollection_ShouldNotPopulate()
        {
            // Arrange
            var helper = RepositoryInitialize.InitializeEmptyRepositories()
                .PopulateDeviceTypes()
                .PopulateAssetClasses();

            // Act
            helper.PopulateAssets(new List<Asset>());

            // Assert
            Assert.AreEqual(0, helper.AssetManagement.Assets.Count(new TRUEFilterElement<Asset>()), 
                "Should not populate when empty collection is passed");
        }

        [TestMethod]
        public void PopulateAssets_WithInvalidData_ShouldThrowException()
        {
            // Arrange
            var helper = RepositoryInitialize.InitializeEmptyRepositories()
                .PopulateDeviceTypes()
                .PopulateAssetClasses();

            var invalidAssets = new List<Asset>
            {
                CreateInvalidAsset() // Asset with no name or ID
            };

            // Act & Assert
            Assert.ThrowsException<BulkValidationException<Asset>>(
                () => helper.PopulateAssets(invalidAssets),
                "Should throw BulkValidationException for invalid assets");
        }

        [TestMethod]
        public void PopulateAssets_ShouldSupportFluentChaining()
        {
            // Arrange
            var helper = RepositoryInitialize.InitializeEmptyRepositories();

            // Act
            var result = helper
                .PopulateDeviceTypes()
                .PopulateAssetClasses()
                .PopulateAssets();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreSame(helper, result, "Should return same helper instance for chaining");
        }

        [TestMethod]
        public void PopulateAssets_WithoutRacks_ShouldCreateAssetsWithoutRackLocation()
        {
            // Arrange
            var helper = RepositoryInitialize.InitializeEmptyRepositories()
                .PopulateDeviceTypes()
                .PopulateAssetClasses();
            // Note: NOT calling PopulateRacks()

            // Act
            helper.PopulateAssets();

            // Assert
            var assets = helper.AssetManagement.Assets.Read(new TRUEFilterElement<Asset>()).ToList();
            Assert.IsTrue(assets.Count > 0, "Assets should be created even without racks");
            Assert.IsTrue(assets.All(a => a.Location?.RackId == null || !a.Location.RackId.HasValue()), 
                "Assets without racks should have no rack assignment");
        }

        [TestMethod]
        public void PopulateAssets_WithRacks_ShouldAssignRackLocations()
        {
            // Arrange
            var helper = RepositoryInitialize.InitializeEmptyRepositories()
                .PopulateRacks()
                .PopulateDeviceTypes()
                .PopulateAssetClasses();

            // Act
            helper.PopulateAssets();

            // Assert
            var assets = helper.AssetManagement.Assets.Read(new TRUEFilterElement<Asset>()).ToList();
            Assert.IsTrue(assets.Count > 0, "Assets should be created");
            Assert.IsTrue(assets.All(a => a.Location?.RackId != null && a.Location.RackId.HasValue()), 
                "All assets should be assigned to racks when racks are available");
        }

        [TestMethod]
        public void PopulateAssets_WithoutAssetClasses_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var helper = RepositoryInitialize.InitializeEmptyRepositories()
                .PopulateRacks(); // Racks available but no AssetClasses

            // Act & Assert
            var ex = Assert.ThrowsException<InvalidOperationException>(
                () => helper.PopulateAssets(),
                "Should throw when asset classes are not populated");
            
            Assert.IsTrue(ex.Message.Contains("No AssetClasses found"));
        }

        #endregion

        #region PopulateAssetClasses Tests

        [TestMethod]
        public void PopulateAssetClasses_WithDefaultData_ShouldPopulateRepository()
        {
            // Arrange
            var helper = RepositoryInitialize.InitializeEmptyRepositories()
                .PopulateDeviceTypes();

            // Act
            helper.PopulateAssetClasses();

            // Assert
            Assert.IsTrue(helper.AssetManagement.AssetClasses.Count(new TRUEFilterElement<AssetClass>()) > 0, "Should populate with default demo data");
        }

        [TestMethod]
        public void PopulateAssetClasses_WithValidCustomData_ShouldPopulateRepository()
        {
            // Arrange
            var helper = RepositoryInitialize.InitializeEmptyRepositories()
                .PopulateDeviceTypes();

            var deviceType = helper.AssetManagement.DeviceTypes.Read(new TRUEFilterElement<DeviceType>()).First();
            var customAssetClasses = new List<AssetClass>
            {
                CreateValidAssetClass("Test Class 1", deviceType.Identifier),
                CreateValidAssetClass("Test Class 2", deviceType.Identifier)
            };

            // Act
            helper.PopulateAssetClasses(customAssetClasses);

            // Assert
            var assetClasses = helper.AssetManagement.AssetClasses.Read(new TRUEFilterElement<AssetClass>()).ToList();
            Assert.IsTrue(assetClasses.Count >= 2);
            Assert.IsTrue(assetClasses.Any(ac => ac.Name == "Test Class 1"));
            Assert.IsTrue(assetClasses.Any(ac => ac.Name == "Test Class 2"));
        }

        [TestMethod]
        public void PopulateAssetClasses_WithNullCollection_ShouldThrowArgumentNullException()
        {
            // Arrange
            var helper = RepositoryInitialize.InitializeEmptyRepositories()
                .PopulateDeviceTypes();

            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(
                () => helper.PopulateAssetClasses((IEnumerable<AssetClass>)null),
                "Should throw ArgumentNullException when null collection is passed");
        }

        [TestMethod]
        public void PopulateAssetClasses_WithEmptyCollection_ShouldNotPopulate()
        {
            // Arrange
            var helper = RepositoryInitialize.InitializeEmptyRepositories()
                .PopulateDeviceTypes();

            // Act
            helper.PopulateAssetClasses(new List<AssetClass>());

            // Assert
            Assert.AreEqual(0, helper.AssetManagement.AssetClasses.Count(new TRUEFilterElement<AssetClass>()), 
                "Should not populate when empty collection is passed");
        }

        [TestMethod]
        public void PopulateAssetClasses_WithInvalidData_ShouldThrowException()
        {
            // Arrange
            var helper = RepositoryInitialize.InitializeEmptyRepositories()
                .PopulateDeviceTypes();

            var invalidAssetClasses = new List<AssetClass>
            {
                CreateInvalidAssetClass() // AssetClass with no name
            };

            // Act & Assert
            Assert.ThrowsException<BulkValidationException<AssetClass>>(
                () => helper.PopulateAssetClasses(invalidAssetClasses),
                "Should throw BulkValidationException for invalid asset classes");
        }

        #endregion

        #region PopulateDeviceTypes Tests

        [TestMethod]
        public void PopulateDeviceTypes_WithDefaultData_ShouldPopulateRepository()
        {
            // Arrange
            var helper = RepositoryInitialize.InitializeEmptyRepositories();

            // Act
            helper.PopulateDeviceTypes();

            // Assert
            Assert.IsTrue(helper.AssetManagement.DeviceTypes.Count(new TRUEFilterElement<DeviceType>()) > 0, "Should populate with default demo data");
        }

        [TestMethod]
        public void PopulateDeviceTypes_WithValidCustomData_ShouldPopulateRepository()
        {
            // Arrange
            var helper = RepositoryInitialize.InitializeEmptyRepositories();
            var customDeviceTypes = new List<DeviceType>
            {
                CreateValidDeviceType("Test Device Type 1"),
                CreateValidDeviceType("Test Device Type 2")
            };

            // Act
            helper.PopulateDeviceTypes(customDeviceTypes);

            // Assert
            Assert.AreEqual(2, helper.AssetManagement.DeviceTypes.Count(new TRUEFilterElement<DeviceType>()), "Should have exactly 2 device types");
        }

        [TestMethod]
        public void PopulateDeviceTypes_WithNullCollection_ShouldThrowArgumentNullException()
        {
            // Arrange
            var helper = RepositoryInitialize.InitializeEmptyRepositories();

            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(
                () => helper.PopulateDeviceTypes((IEnumerable<DeviceType>)null),
                "Should throw ArgumentNullException when null collection is passed");
        }

        [TestMethod]
        public void PopulateDeviceTypes_WithEmptyCollection_ShouldNotPopulate()
        {
            // Arrange
            var helper = RepositoryInitialize.InitializeEmptyRepositories();

            // Act
            helper.PopulateDeviceTypes(new List<DeviceType>());

            // Assert
            Assert.AreEqual(0, helper.AssetManagement.DeviceTypes.Count(new TRUEFilterElement<DeviceType>()), 
                "Should not populate when empty collection is passed");
        }

        #endregion

        #region PopulateDataPorts Tests

        [TestMethod]
        public void PopulateDataPorts_WithDefaultData_ShouldPopulateRepository()
        {
            // Arrange
            var helper = RepositoryInitialize.InitializeEmptyRepositories()
                .PopulateDeviceTypes()
                .PopulateAssetClasses()
                .PopulateAssets();

            // Act
            helper.PopulateDataPorts();

            // Assert
            Assert.IsTrue(helper.AssetManagement.DataPorts.Count(new TRUEFilterElement<DataPort>()) > 0, "Should populate with default demo data");
        }

        [TestMethod]
        public void PopulateDataPorts_WithValidCustomData_ShouldPopulateRepository()
        {
            // Arrange
            var helper = RepositoryInitialize.InitializeEmptyRepositories()
                .PopulateDeviceTypes()
                .PopulateAssetClasses()
                .PopulateAssets();

            var asset = helper.AssetManagement.Assets.Read(new TRUEFilterElement<Asset>()).First();
            var customDataPorts = new List<DataPort>
            {
                CreateValidDataPort(asset.Identifier, 1),
                CreateValidDataPort(asset.Identifier, 2)
            };

            // Act
            helper.PopulateDataPorts(customDataPorts);

            // Assert
            Assert.IsTrue(helper.AssetManagement.DataPorts.Count(new TRUEFilterElement<DataPort>()) >= 2, "Should have at least 2 data ports");
        }

        [TestMethod]
        public void PopulateDataPorts_WithNullCollection_ShouldThrowArgumentNullException()
        {
            // Arrange
            var helper = RepositoryInitialize.InitializeEmptyRepositories()
                .PopulateDeviceTypes()
                .PopulateAssetClasses()
                .PopulateAssets();

            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(
                () => helper.PopulateDataPorts((IEnumerable<DataPort>)null),
                "Should throw ArgumentNullException when null collection is passed");
        }

        [TestMethod]
        public void PopulateDataPorts_WithEmptyCollection_ShouldNotPopulate()
        {
            // Arrange
            var helper = RepositoryInitialize.InitializeEmptyRepositories()
                .PopulateDeviceTypes()
                .PopulateAssetClasses()
                .PopulateAssets();

            // Act
            helper.PopulateDataPorts(new List<DataPort>());

            // Assert
            Assert.AreEqual(0, helper.AssetManagement.DataPorts.Count(new TRUEFilterElement<DataPort>()), 
                "Should not populate when empty collection is passed");
        }

        #endregion

        #region PopulatePowerPorts Tests

        [TestMethod]
        public void PopulatePowerPorts_WithDefaultData_ShouldPopulateRepository()
        {
            // Arrange
            var helper = RepositoryInitialize.InitializeEmptyRepositories()
                .PopulateDeviceTypes()
                .PopulateAssetClasses()
                .PopulateAssets();

            // Act
            helper.PopulatePowerPorts();

            // Assert
            Assert.IsTrue(helper.AssetManagement.PowerPorts.Count(new TRUEFilterElement<PowerPort>()) > 0, "Should populate with default demo data");
        }

        #endregion

        #region Integration Tests

        [TestMethod]
        public void PopulateAll_InCorrectOrder_ShouldSucceed()
        {
            // Arrange
            var helper = RepositoryInitialize.InitializeEmptyRepositories();

            // Act - Populate in dependency order
            helper.PopulateRacks()
                .PopulateDeviceTypes()
                .PopulateAssetClasses()
                .PopulateAssets()
                .PopulateDataPorts()
                .PopulatePowerPorts();

            // Assert
            Assert.IsTrue(helper.AssetManagement.DeviceTypes.Count(new TRUEFilterElement<DeviceType>()) > 0);
            Assert.IsTrue(helper.AssetManagement.AssetClasses.Count(new TRUEFilterElement<AssetClass>()) > 0);
            Assert.IsTrue(helper.AssetManagement.Assets.Count(new TRUEFilterElement<Asset>()) > 0);
            Assert.IsTrue(helper.AssetManagement.DataPorts.Count(new TRUEFilterElement<DataPort>()) > 0);
            Assert.IsTrue(helper.AssetManagement.PowerPorts.Count(new TRUEFilterElement<PowerPort>()) > 0);
        }

        #endregion

        #region Helper Methods

        private static Asset CreateValidAsset(string name, string assetId, string assetClassId)
        {
            var asset = new Asset
            {
                State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available,
                AssetClassId = new SdmObjectReference<AssetClass>(assetClassId)
            };

            // Important: Set values AFTER construction to trigger change tracking
            asset.Name = name;
            asset.AssetID = assetId;

            return asset;
        }

        private static Asset CreateInvalidAsset()
        {
            // Create asset with no name or ID - should fail validation
            var asset = new Asset
            {
                State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available
            };

            // Set name to empty to trigger validation failure
            asset.Name = "";
            asset.AssetID = "";

            return asset;
        }

        private static AssetClass CreateValidAssetClass(string name, string deviceTypeIdentifier)
        {
            var assetClass = new AssetClass
            {
                DeviceTypeId = new SdmObjectReference<DeviceType>(deviceTypeIdentifier)
            };

            // Set name after construction to trigger change tracking
            assetClass.Name = name;

            return assetClass;
        }

        private static AssetClass CreateInvalidAssetClass()
        {
            // Create asset class with no name - should fail validation
            var assetClass = new AssetClass();
            assetClass.Name = ""; // Empty name

            return assetClass;
        }

        private static DeviceType CreateValidDeviceType(string name)
        {
            var deviceType = new DeviceType();
            deviceType.Name = name;

            return deviceType;
        }

        private static DataPort CreateValidDataPort(string assetId, long portNumber)
        {
            var dataPort = new DataPort
            {
                AssetFk = new AssetRelation
                {
                    Asset = new SdmObjectReference<Asset>(assetId),
                }

            };

            dataPort.DataPortInfo.PortNumber = portNumber;

            return dataPort;
        }

        #endregion

        
    }
}