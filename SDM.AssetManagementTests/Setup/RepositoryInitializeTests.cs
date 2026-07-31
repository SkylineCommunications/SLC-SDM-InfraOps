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

    /// <summary>
    /// Tests for repository initialization and population functionality.
    /// </summary>
    [TestClass]
    public class RepositoryInitializeTests : BaseRepositoryTest
    {
        #region Initialization Tests

        [TestMethod]
        public void InitializeEmptyRepositories_ShouldReturnValidHelper()
        {
            // Act


            // Assert
            Assert.IsNotNull(Helper);
            Assert.IsNotNull(Helper.AssetManagement.Assets);
            Assert.IsNotNull(Helper.AssetManagement.AssetClasses);
            Assert.IsNotNull(Helper.AssetManagement.DeviceTypes);
            Assert.IsNotNull(Helper.AssetManagement.DataPorts);
            Assert.IsNotNull(Helper.AssetManagement.PowerPorts);
        }

        [TestMethod]
        public void InitializeEmptyRepositories_ShouldHaveEmptyRepositories()
        {
            // Act


            // Assert
            Assert.AreEqual(0, Helper.AssetManagement.Assets.Count(new TRUEFilterElement<Asset>()));
            Assert.AreEqual(0, Helper.AssetManagement.AssetClasses.Count(new TRUEFilterElement<AssetClass>()));
            Assert.AreEqual(0, Helper.AssetManagement.DeviceTypes.Count(new TRUEFilterElement<DeviceType>()));
            Assert.AreEqual(0, Helper.AssetManagement.DataPorts.Count(new TRUEFilterElement<DataPort>()));
            Assert.AreEqual(0, Helper.AssetManagement.PowerPorts.Count(new TRUEFilterElement<PowerPort>()));
        }

        #endregion

        #region PopulateWithDemoData Integration Tests

        [TestMethod]
        public void PopulateWithDemoData_AllLayers_ShouldSucceed()
        {
            // Arrange


            // Act
            Helper.PopulateWithDemoData();

            // Assert
            Assert.IsTrue(Helper.TestData.DeviceTypes.Any(), "DeviceTypes should be populated");
            Assert.IsTrue(Helper.TestData.AssetClasses.Any(), "AssetClasses should be populated");
            Assert.IsTrue(Helper.TestData.Assets.Any(), "Assets should be populated");
            Assert.IsTrue(Helper.TestData.DataPorts.Any(), "DataPorts should be populated");
            Assert.IsTrue(Helper.TestData.PowerPorts.Any(), "PowerPorts should be populated");
            Assert.IsTrue(Helper.TestData.Racks.Any(), "Racks should be populated");
        }

        [TestMethod]
        public void PopulateWithDemoData_UpToSpecificLayer_ShouldPopulateDependenciesOnly()
        {
            // Arrange


            // Act - Only populate up to AssetClasses
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.AssetClasses);

            // Assert
            Assert.IsTrue(Helper.TestData.DeviceTypes.Any(), "DeviceTypes should be populated (dependency)");
            Assert.IsTrue(Helper.TestData.AssetClasses.Any(), "AssetClasses should be populated");
            Assert.IsFalse(Helper.TestData.Assets.Any(), "Assets should NOT be populated");
            Assert.IsFalse(Helper.TestData.DataPorts.Any(), "DataPorts should NOT be populated");
            Assert.IsFalse(Helper.TestData.PowerPorts.Any(), "PowerPorts should NOT be populated");
        }

        [TestMethod]
        public void PopulateWithDemoData_ShouldAssignRackLocations()
        {
            // Act
            Helper.PopulateWithDemoData(DemoDataLayer.Assets);

            // Assert
            var assets = Helper.TestData.Assets;
            Assert.IsTrue(assets.Any(), "Assets should be populated");
            Assert.IsTrue(Helper.TestData.Racks.Any(), "Racks should be populated");
            Assert.IsTrue(assets.All(a => a.Location?.RackId != null && a.Location.RackId.HasValue()),
                "All assets should be assigned to racks");
        }

        [TestMethod]
        public void PopulateWithDemoData_ShouldSupportFluentChaining()
        {
            // Arrange


            // Act
            var result = Helper
                .PopulateWithDemoData(DemoDataLayer.DeviceTypes)
                .PopulateWithDemoData(DemoDataLayer.AssetClasses);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreSame(Helper, result, "Should return same helper instance for chaining");
        }

        #endregion

        #region Custom Data Population - Validation Tests

        [TestMethod]
        public void PopulateAssets_WithValidCustomData_ShouldSucceed()
        {
            // Arrange

            Helper.PopulateWithDemoData(DemoDataLayer.AssetClasses);

            var assetClass = Helper.TestData.AssetClasses.First();
            var customAssets = new List<Asset>
            {
                CreateValidAsset("Custom Asset 1", "CA-001", assetClass.Identifier),
                CreateValidAsset("Custom Asset 2", "CA-002", assetClass.Identifier)
            };

            // Act
            Helper.PopulateAssets(customAssets);

            // Assert
            var allAssets = Helper.TestData.Assets;
            Assert.IsTrue(allAssets.Any(a => a.Name == "Custom Asset 1"), "Custom asset 1 should be created");
            Assert.IsTrue(allAssets.Any(a => a.Name == "Custom Asset 2"), "Custom asset 2 should be created");
        }

        [TestMethod]
        public void PopulateAssets_WithInvalidData_ShouldThrowValidationException()
        {
            // Arrange

            Helper.PopulateWithDemoData(DemoDataLayer.AssetClasses);

            var invalidAssets = new List<Asset>
            {
                CreateInvalidAsset() // Asset with no name or ID
            };

            // Act & Assert
            Assert.ThrowsException<BulkValidationException<Asset>>(
                () => Helper.PopulateAssets(invalidAssets),
                "Should throw validation exception for invalid assets");
        }

        [TestMethod]
        public void PopulateAssetClasses_WithValidCustomData_ShouldSucceed()
        {
            // Arrange

            Helper.PopulateWithDemoData(DemoDataLayer.DeviceTypes);

            var deviceType = Helper.TestData.DeviceTypes.First();
            var customAssetClasses = new List<AssetClass>
            {
                CreateValidAssetClass("Custom Class 1", deviceType),
                CreateValidAssetClass("Custom Class 2", deviceType)
            };

            // Act
            Helper.PopulateAssetClasses(customAssetClasses);

            // Assert
            var allAssetClasses = Helper.TestData.AssetClasses;
            Assert.IsTrue(allAssetClasses.Any(ac => ac.Name == "Custom Class 1"), "Custom class 1 should be created");
            Assert.IsTrue(allAssetClasses.Any(ac => ac.Name == "Custom Class 2"), "Custom class 2 should be created");
        }

        [TestMethod]
        public void PopulateAssetClasses_WithInvalidData_ShouldThrowValidationException()
        {
            // Arrange

            Helper.PopulateWithDemoData(DemoDataLayer.DeviceTypes);

            var invalidAssetClasses = new List<AssetClass>
            {
                CreateInvalidAssetClass()
            };

            // Act & Assert
            Assert.ThrowsException<BulkValidationException<AssetClass>>(
                () => Helper.PopulateAssetClasses(invalidAssetClasses),
                "Should throw validation exception for invalid asset classes");
        }

        [TestMethod]
        public void PopulateDeviceTypes_WithValidCustomData_ShouldSucceed()
        {
            // Arrange

            var customDeviceTypes = new List<DeviceType>
            {
                CreateValidDeviceType("Custom Device Type 1"),
                CreateValidDeviceType("Custom Device Type 2")
            };

            // Act
            Helper.PopulateDeviceTypes(customDeviceTypes);

            // Assert
            Assert.AreEqual(2, Helper.TestData.DeviceTypes.Count, "Should have exactly 2 device types");
        }

        [TestMethod]
        public void PopulateDataPorts_WithValidCustomData_ShouldSucceed()
        {
            // Arrange
            Helper.PopulateWithDemoData(DemoDataLayer.Assets);

            var asset = Helper.TestData.Assets.First();

            // Data ports require a valid data Port Type (validation is wired into the repository).
            var dataPortType = new PortType
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = "Custom Data Port Type",
                CategoryLinks = new CategoryRelation
                {
                    Categories = [SlcAsset_Management.Enums.CategoriesEnum.Data],
                },
            };
            Helper.AssetManagement.PortTypes.Create(dataPortType);

            var customDataPorts = new List<DataPort>
            {
                CreateValidDataPort(asset.Identifier, dataPortType.Identifier, 1),
                CreateValidDataPort(asset.Identifier, dataPortType.Identifier, 2)
            };

            // Act
            Helper.PopulateDataPorts(customDataPorts);

            // Assert
            var allDataPorts = Helper.TestData.DataPorts;
            Assert.IsTrue(allDataPorts.Count(dp => dp.Asset.Identifier == asset.Identifier) >= 2,
                "Should have at least 2 data ports for the asset");
        }

        #endregion

        #region Null/Empty Collection Tests

        [TestMethod]
        public void PopulateAssets_WithNullCollection_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(
                () => Helper.PopulateAssets(null!));
        }

        [TestMethod]
        public void PopulateAssets_WithEmptyCollection_ShouldNotPopulate()
        {
            // Arrange


            // Act
            Helper.PopulateAssets(new List<Asset>());

            // Assert
            Assert.AreEqual(0, Helper.TestData.Assets.Count, "Should not populate with empty collection");
        }

        [TestMethod]
        public void PopulateAssetClasses_WithNullCollection_ShouldThrowArgumentNullException()
        {
            // Arrange


            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(
                () => Helper.PopulateAssetClasses((IEnumerable<AssetClass>)null!));
        }

        [TestMethod]
        public void PopulateAssetClasses_WithEmptyCollection_ShouldNotPopulate()
        {
            // Arrange


            // Act
            Helper.PopulateAssetClasses(new List<AssetClass>());

            // Assert
            Assert.AreEqual(0, Helper.TestData.AssetClasses.Count, "Should not populate with empty collection");
        }

        [TestMethod]
        public void PopulateDeviceTypes_WithNullCollection_ShouldThrowArgumentNullException()
        {
            // Arrange


            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(
                () => Helper.PopulateDeviceTypes((IEnumerable<DeviceType>)null!));
        }

        [TestMethod]
        public void PopulateDeviceTypes_WithEmptyCollection_ShouldNotPopulate()
        {
            // Arrange


            // Act
            Helper.PopulateDeviceTypes(new List<DeviceType>());

            // Assert
            Assert.AreEqual(0, Helper.TestData.DeviceTypes.Count, "Should not populate with empty collection");
        }

        [TestMethod]
        public void PopulateDataPorts_WithNullCollection_ShouldThrowArgumentNullException()
        {
            // Arrange


            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(
                () => Helper.PopulateDataPorts((IEnumerable<DataPort>)null!));
        }

        [TestMethod]
        public void PopulateDataPorts_WithEmptyCollection_ShouldNotPopulate()
        {
            // Arrange


            // Act
            Helper.PopulateDataPorts(new List<DataPort>());

            // Assert
            Assert.AreEqual(0, Helper.TestData.DataPorts.Count, "Should not populate with empty collection");
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

            // Set values after construction to trigger change tracking
            asset.Name = name;
            asset.AssetID = assetId;

            return asset;
        }

        private static Asset CreateInvalidAsset()
        {
            var asset = new Asset
            {
                State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available
            };

            // Set empty name/ID to trigger validation failure
            asset.Name = "";
            asset.AssetID = "";

            return asset;
        }

        private static AssetClass CreateValidAssetClass(string name, DeviceType deviceType)
        {
            var assetClass = new AssetClass
            {
                DeviceTypeId = new SdmObjectReference<DeviceType>(deviceType.Identifier),
                Name = name,
            };

            if (deviceType.TagsInfo.Tags.Contains(SlcAsset_Management.Enums.TagOption.PowerProvider))
            {
                assetClass.PowerSupply = SlcAsset_Management.Enums.PowerSupplyEnum.DC;
            }

            return assetClass;
        }

        private static AssetClass CreateInvalidAssetClass()
        {
            var assetClass = new AssetClass();
            assetClass.Name = ""; // Empty name triggers validation failure
            return assetClass;
        }

        private static DeviceType CreateValidDeviceType(string name)
        {
            var deviceType = new DeviceType();
            deviceType.Name = name;
            return deviceType;
        }

        private static DataPort CreateValidDataPort(string assetId, string portTypeId, long portNumber)
        {
            var dataPort = new DataPort
            {
                Asset = new SdmObjectReference<Asset>(assetId),
            };

            dataPort.DataPortInfo.Name = $"Data Port {portNumber}";
            dataPort.DataPortInfo.PortNumber = portNumber;
            dataPort.DataPortInfo.OutputType = SlcAsset_Management.Enums.Outputtype.IO;
            dataPort.DataPortInfo.Type = new SdmObjectReference<PortType>(portTypeId);
            return dataPort;
        }

        #endregion
    }
}