namespace SDM.AssetManagementTests.Validation
{
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SDM.AssetManagement.Tests;
    using SDM.AssetManagement.Tests.Setup;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Common.Validation;
    using Skyline.DataMiner.SDM.AssetManagement.Helpers;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.AssetManagement.Repositories;
    using Skyline.DataMiner.SDM.AssetManagement.Validation;

    [TestClass]
    public class AssetClassValidatorTests
    {
        private IAssetManagementApiHelper _helper;
        private AssetClassValidator _validator;

        [TestInitialize]
        public void Setup()
        {
            _helper = RepositoryInitialize.InitializeEmptyRepositories();
            _validator = _helper.AssetClassValidator;
        }

        #region Constructor Tests

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void Constructor_WithNullAssetClassRepository_ThrowsArgumentNullException()
        {
            // Act
            var validator = new AssetClassValidator(null, (IDeviceTypeQueryRepository)_helper.DeviceTypes);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void Constructor_WithNullDeviceTypeRepository_ThrowsArgumentNullException()
        {
            // Act
            var validator = new AssetClassValidator((IAssetClassQueryRepository)_helper.AssetClasses, null);
        }

        #endregion

        #region Validate Tests

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentNullException))]
        public void Validate_WithNullAssetClass_ThrowsArgumentNullException()
        {
            // Act
            _validator.Validate(null);
        }

        [TestMethod]
        public void Validate_WithAllValidFields_ReturnsValid()
        {
            // Arrange
            var deviceType = DemoData.DeviceTypes.First();
            _helper.PopulateDeviceTypes(new[] { deviceType });

            var assetClass = new AssetClass
            {
                Identifier = "ac-123",
                Name = "Valid Device",
                DeviceTypeId = new SdmObjectReference<DeviceType>(deviceType.Identifier),
                Depth = 10,
                Width = 20,
                Height = 30,
                HeightU = 2,
                Weight = 50,
                TypicalPowerConsumption = 100,
                MaximumPowerConsumption = 150,
                DataPorts = new List<DataPort>(),
                PowerPorts = new List<PowerPort>(),
                Holders = new List<AssetHolder>()
            };

            // Act
            var result = _validator.Validate(assetClass);

            // Assert
            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(0, result.FailureReasons.Count);
        }

        [TestMethod]
        public void Validate_WithMultipleInvalidFields_ReturnsAllErrors()
        {
            // Arrange
            var deviceType = DemoData.DeviceTypes.First();
            _helper.PopulateDeviceTypes(new[] { deviceType });

            var assetClass = new AssetClass
            {
                Name = "Test",
                DeviceTypeId = new SdmObjectReference<DeviceType>(deviceType.Identifier),
                Depth = -5,      // Invalid: negative
                Width = -10,     // Invalid: negative
                Height = -1,     // Invalid: negative
                TypicalPowerConsumption = -100  // Invalid: negative
            };

            // Act
            var result = _validator.Validate(assetClass);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.FailureReasons.Count >= 4); // At least depth, width, height, power consumption
        }

        [TestMethod]
        public void Validate_OnlyValidatesChangedFields()
        {
            // Arrange
            var deviceType = DemoData.DeviceTypes.First();
            _helper.PopulateDeviceTypes(new[] { deviceType });

            var assetClass = new AssetClass
            {
                Name = "Test",
                DeviceTypeId = new SdmObjectReference<DeviceType>(deviceType.Identifier),
                Depth = -5,
                Width = 20
            };

            // Reset change tracking after initialization
            assetClass.ResetChangeTracking();

            // Now only change Width
            assetClass.Width = 25;

            // Act
            var result = _validator.Validate(assetClass);

            // Assert
            Assert.IsTrue(result.IsValid); // Depth error not reported because not changed after reset
        }

        #endregion

        #region IsAssetClassNameValid(string, List<string>) Tests

        [TestMethod]
        public void IsAssetClassNameValid_WithValidUniqueName_ReturnsValid()
        {
            // Act
            var result = _validator.IsAssetClassNameValid("Unique Name", null);

            // Assert
            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void IsAssetClassNameValid_WithEmptyName_ReturnsInvalid()
        {
            // Act
            var result = _validator.IsAssetClassNameValid(string.Empty, null);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.TryGetFailReason(
                AssetClassValidationHandler.AssetClassValidationField.Name,
                out var reason));
            StringAssert.Contains(reason, "cannot be empty or whitespace");
        }

        [TestMethod]
        public void IsAssetClassNameValid_WithWhitespaceName_ReturnsInvalid()
        {
            // Act
            var result = _validator.IsAssetClassNameValid("   ", null);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.TryGetFailReason(
                AssetClassValidationHandler.AssetClassValidationField.Name,
                out var reason));
            StringAssert.Contains(reason, "cannot be empty or whitespace");
        }

        [TestMethod]
        public void IsAssetClassNameValid_WithNullName_ReturnsInvalid()
        {
            // Act
            var result = _validator.IsAssetClassNameValid(null, null);

            // Assert
            Assert.IsFalse(result.IsValid);
        }

        [TestMethod]
        public void IsAssetClassNameValid_WithNameInUse_ReturnsInvalid()
        {
            // Arrange
            var existingAssetClass = DemoData.AssetClasses.First();
            _helper.PopulateAssetClasses(new[] { existingAssetClass });

            // Act
            var result = _validator.IsAssetClassNameValid(existingAssetClass.Name, null);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.TryGetFailReason(
                AssetClassValidationHandler.AssetClassValidationField.Name,
                out var reason));
            StringAssert.Contains(reason, "already in use");
        }

        [TestMethod]
        public void IsAssetClassNameValid_WithNameInUseButExcluded_ReturnsValid()
        {
            // Arrange
            var existingAssetClass = DemoData.AssetClasses.First();
            _helper.PopulateAssetClasses(new[] { existingAssetClass });

            var exceptIdentifiers = new List<string> { existingAssetClass.Identifier };

            // Act
            var result = _validator.IsAssetClassNameValid(existingAssetClass.Name, exceptIdentifiers);

            // Assert
            Assert.IsTrue(result.IsValid);
        }

        #endregion

        #region IsAssetClassNameValid(AssetClass) Tests

        [TestMethod]
        public void IsAssetClassNameValid_WithAssetClass_PassesIdentifierToExclusion()
        {
            // Arrange
            var assetClass = new AssetClass
            {
                Identifier = "ac-123",
                Name = "Test Device"
            };

            // Act
            var result = _validator.IsAssetClassNameValid(assetClass);

            // Assert
            Assert.IsTrue(result.IsValid);
        }

        #endregion

        #region Power Supply Validation Tests

        [TestMethod]
        public void Validate_WithPowerProviderDeviceType_AndPowerSupply_ReturnsValid()
        {
            // Arrange - Find or skip if no PowerProvider device types
            var powerProviderDeviceType = DemoData.DeviceTypes
                .FirstOrDefault(dt => dt.TagsInfo.Tags.Contains(SlcAsset_Management.Enums.TagOption.PowerProvider));

            if (powerProviderDeviceType == null)
            {
                Assert.Inconclusive("No PowerProvider device type in test data");
                return;
            }

            _helper.PopulateDeviceTypes(new[] { powerProviderDeviceType });

            var assetClass = new AssetClass
            {
                Name = "Power Device",
                DeviceTypeId = new SdmObjectReference<DeviceType>(powerProviderDeviceType.Identifier),
                PowerSupply = SlcAsset_Management.Enums.PowerSupplyEnum.AC,
            };

            // Act
            var result = _validator.Validate(assetClass);

            // Assert
            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void Validate_WithMissingDeviceType_ReturnsInvalid()
        {
            // Arrange
            var assetClass = new AssetClass
            {
                Name = "Test Device",
                DeviceTypeId = new SdmObjectReference<DeviceType>("dt-missing"),
                PowerSupply = SlcAsset_Management.Enums.PowerSupplyEnum.AC
            };

            // Act
            var result = _validator.Validate(assetClass);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.TryGetFailReason(
                AssetClassValidationHandler.AssetClassValidationField.DeviceTypeId,
                out var reason));
            StringAssert.Contains(reason, "Device Type not found");
        }

        #endregion

        #region Collection Validation Tests

        [TestMethod]
        public void Validate_WithInvalidDataPorts_ReturnsInvalid()
        {
            // Arrange
            var deviceType = DemoData.DeviceTypes.First();
            _helper.PopulateDeviceTypes(new[] { deviceType });

            var assetClass = new AssetClass
            {
                Name = "Test",
                DeviceTypeId = new SdmObjectReference<DeviceType>(deviceType.Identifier),
                DataPorts = new List<DataPort>
                {
                   new DataPort{ DataPortInfo =  new DataPortInfo { PortNumber = 1 } },
                   new DataPort{ DataPortInfo = new DataPortInfo { PortNumber = 1 } }  // Duplicate
                }
            };

            // Act
            var result = _validator.Validate(assetClass);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.TryGetFailReason(
                AssetClassValidationHandler.AssetClassValidationField.DataPortNumber,
                out var reason));
            StringAssert.Contains(reason, "Multiple Data Ports");
        }

        [TestMethod]
        public void Validate_WithInvalidPowerPorts_ReturnsInvalid()
        {
            // Arrange
            var deviceType = DemoData.DeviceTypes.First();
            _helper.PopulateDeviceTypes(new[] { deviceType });

            var assetClass = new AssetClass
            {
                Name = "Test",
                DeviceTypeId = new SdmObjectReference<DeviceType>(deviceType.Identifier),
                PowerPorts = new List<PowerPort>
                {
                    new PowerPort{ PowerPortInfo = new PowerPortInfo { PortNumber = -1 } }  // Negative
                }
            };

            // Act
            var result = _validator.Validate(assetClass);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.TryGetFailReason(
                AssetClassValidationHandler.AssetClassValidationField.PowerPortNumber,
                out var reason));
            StringAssert.Contains(reason, "cannot be negative");
        }

        [TestMethod]
        public void Validate_WithInvalidHolders_ReturnsInvalid()
        {
            // Arrange
            var deviceType = DemoData.DeviceTypes.First();
            _helper.PopulateDeviceTypes(new[] { deviceType });

            var assetClass = new AssetClass
            {
                Name = "Test",
                DeviceTypeId = new SdmObjectReference<DeviceType>(deviceType.Identifier),
                Holders = new List<AssetHolder>
                {
                    new AssetHolder
                    {
                        SlotNumber = 1,
                        HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Chassis
                    },
                    new AssetHolder
                    {
                        SlotNumber = 1,
                        HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Chassis
                    }
                }
            };

            // Act
            var result = _validator.Validate(assetClass);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.TryGetFailReason(
                AssetClassValidationHandler.AssetClassValidationField.HolderSlotNumber,
                out var reason));
            StringAssert.Contains(reason, "Multiple Holders");
        }

        #endregion

        #region Dimension Validation Tests

        [TestMethod]
        public void Validate_WithNegativeDimensions_ReturnsInvalid()
        {
            // Arrange
            var deviceType = DemoData.DeviceTypes.First();
            _helper.PopulateDeviceTypes(new[] { deviceType });

            var assetClass = new AssetClass
            {
                Name = "Test",
                DeviceTypeId = new SdmObjectReference<DeviceType>(deviceType.Identifier),
                Depth = -10,
                Width = -20,
                Height = -5,
                HeightU = -1,
                Weight = -100
            };

            // Act
            var result = _validator.Validate(assetClass);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.FailureReasons.Count >= 5);
        }

        #endregion

        #region Integration Tests with Real Repository

        [TestMethod]
        public void Validate_WithExistingDataInRepository_WorksCorrectly()
        {
            // Arrange
            _helper.PopulateAssetClasses()
                   .PopulateDeviceTypes();

            var deviceType = DemoData.DeviceTypes.First();

            var newAssetClass = new AssetClass
            {
                Identifier = "new-ac",
                Name = "Brand New Device",
                DeviceTypeId = new SdmObjectReference<DeviceType>(deviceType.Identifier),
                Depth = 10,
                Width = 20
            };

            // Act
            var result = _validator.Validate(newAssetClass);

            // Assert
            Assert.IsTrue(result.IsValid);
        }

        #endregion
    }
}