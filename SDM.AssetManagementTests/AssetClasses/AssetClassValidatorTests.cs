namespace SDM.AssetManagement.Tests.AssetClasses
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SDM.AssetManagement.Tests;
    using SDM.AssetManagement.Tests.Setup;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.AssetManagement.Validation;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Tests for AssetClassValidator which validates AssetClass business rules
    /// with repository lookups (e.g., name uniqueness, DeviceType existence).
    /// </summary>
    [TestClass]
    public class AssetClassValidatorTests : BaseRepositoryTest
    {
        private ITestApiHelper _helper = null!;
        private AssetClassValidator _validator = null!;

        [TestInitialize]
        public void Setup()
        {
            _helper = Helper;
            _validator = _helper.AssetManagement.AssetClassValidator;
        }

        #region Validate - Happy Path

        [TestMethod]
        public void Validate_WithAllValidFields_ShouldReturnValid()
        {
            // Arrange
            _helper.PopulateWithDemoData(upTo: DemoDataLayer.DeviceTypes);
            var deviceType = _helper.TestData.NonPowerProviderDeviceType();

            var assetClass = new AssetClass
            {
                Name = "Valid Device",
                DeviceTypeId = new SdmObjectReference<DeviceType>(deviceType.Identifier),
                Depth = 10,
                Width = 20,
                Height = 30,
                HeightU = 2,
                Weight = 50,
                TypicalPowerConsumption = 100,
                MaximumPowerConsumption = 150,
                DataPorts = new List<DataPortInfo>(),
                PowerPorts = new List<PowerPortInfo>(),
                Holders = new List<AssetHolder>()
            };

            // Act
            var result = _validator.Validate(assetClass, RepositoryAction.Create);

            // Assert
            using (new AssertionScope())
            {
                result.IsValid.Should().BeTrue();
                result.FailureReasons.Should().BeEmpty();
            }
        }

        [TestMethod]
        public void Validate_WithNullAssetClass_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            _validator.Invoking(v => v.Validate(null, RepositoryAction.Create))
                .Should().Throw<ArgumentNullException>();
        }

        #endregion

        #region Validate - Multiple Errors

        [TestMethod]
        public void Validate_WithMultipleInvalidFields_ShouldReturnAllErrors()
        {
            // Arrange
            _helper.PopulateWithDemoData(upTo: DemoDataLayer.DeviceTypes);
            var deviceType = _helper.TestData.NonPowerProviderDeviceType();

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
            var result = _validator.Validate(assetClass, RepositoryAction.Create);

            // Assert
            using (new AssertionScope())
            {
                result.IsValid.Should().BeFalse();
                result.FailureReasons.Should().HaveCountGreaterOrEqualTo(4, 
                    "should report errors for depth, width, height, and power consumption");
            }
        }

        [TestMethod]
        public void Validate_WithNegativeDimensions_ShouldReturnInvalid()
        {
            // Arrange
            _helper.PopulateWithDemoData(upTo: DemoDataLayer.DeviceTypes);
            var deviceType = _helper.TestData.NonPowerProviderDeviceType();

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
            var result = _validator.Validate(assetClass, RepositoryAction.Create);

            // Assert
            using (new AssertionScope())
            {
                result.IsValid.Should().BeFalse();
                result.FailureReasons.Should().HaveCountGreaterOrEqualTo(5,
                    "should report errors for all negative dimensions");
            }
        }

        #endregion

        #region Name Validation

        [TestMethod]
        public void NameValidation_WithValidUniqueName_ShouldReturnValid()
        {
            // Act
            var result = _validator.IsAssetClassNameValid("Unique Name", null);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        [DataRow("", DisplayName = "Empty Name")]
        [DataRow("   ", DisplayName = "Whitespace Name")]
        [DataRow(null, DisplayName = "Null Name")]
        public void NameValidation_WithInvalidName_ShouldReturnInvalid(string name)
        {
            // Act
            var result = _validator.IsAssetClassNameValid(name, null);

            // Assert
            using (new AssertionScope())
            {
                result.IsValid.Should().BeFalse();
                
                if (!string.IsNullOrEmpty(name))
                {
                    result.TryGetFailReason(
                        AssetClassValidationHandler.AssetClassValidationField.Name,
                        out var reason).Should().BeTrue();
                    reason.Should().Contain("cannot be empty or whitespace");
                }
            }
        }

        [TestMethod]
        public void NameValidation_WithNameInUse_ShouldReturnInvalid()
        {
            // Arrange
            _helper.PopulateWithDemoData(upTo: DemoDataLayer.AssetClasses);
            var existingAssetClass = _helper.TestData.AssetClasses.First();

            // Act
            var result = _validator.IsAssetClassNameValid(existingAssetClass.Name, null);

            // Assert
            using (new AssertionScope())
            {
                result.IsValid.Should().BeFalse();
                result.TryGetFailReason(
                    AssetClassValidationHandler.AssetClassValidationField.Name,
                    out var reason).Should().BeTrue();
                reason.Should().Contain("already in use");
            }
        }

        [TestMethod]
        public void NameValidation_WithNameInUseButExcluded_ShouldReturnValid()
        {
            // Arrange
            _helper.PopulateWithDemoData(upTo: DemoDataLayer.AssetClasses);
            var existingAssetClass = _helper.TestData.AssetClasses.First();

            // Act
            var result = _validator.IsAssetClassNameValid(existingAssetClass.Name, existingAssetClass.Identifier);

            // Assert
            result.IsValid.Should().BeTrue("the name belongs to the excluded identifier");
        }

        [TestMethod]
        public void NameValidation_WithAssetClassObject_ShouldExcludeOwnIdentifier()
        {
            // Arrange
            var assetClass = new AssetClass
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = "Test Device",
            };

            // Act
            var result = _validator.IsAssetClassNameValid(assetClass);

            // Assert
            result.IsValid.Should().BeTrue("validation should exclude its own identifier");
        }

        #endregion

        #region DeviceType Validation

        [TestMethod]
        public void Validate_WithMissingDeviceType_ShouldReturnInvalid()
        {
            // Arrange
            var assetClass = new AssetClass
            {
                Name = "Test Device",
                DeviceTypeId = new SdmObjectReference<DeviceType>("dt-missing"),
            };

            // Act
            var result = _validator.Validate(assetClass, RepositoryAction.Create);

            // Assert
            using (new AssertionScope())
            {
                result.IsValid.Should().BeFalse();
                result.TryGetFailReason(
                    AssetClassValidationHandler.AssetClassValidationField.DeviceTypeId,
                    out var reason).Should().BeTrue();
                reason.Should().Contain("Device Type id needs to be a Guid");
            }
        }

        [TestMethod]
        public void Validate_WithPowerProviderDeviceType_AndPowerSupply_ShouldReturnValid()
        {
            // Arrange
            _helper.PopulateWithDemoData(upTo: DemoDataLayer.DeviceTypes);
            
            var powerProviderDeviceType = _helper.TestData.DeviceTypes
                .FirstOrDefault(dt => dt.TagsInfo.Tags.Contains(SlcAsset_Management.Enums.TagOption.PowerProvider));

            if (powerProviderDeviceType == null)
            {
                Assert.Inconclusive("No PowerProvider device type in test data");
                return;
            }

            var assetClass = new AssetClass
            {
                Name = "Power Device",
                DeviceTypeId = new SdmObjectReference<DeviceType>(powerProviderDeviceType.Identifier),
                PowerSupply = SlcAsset_Management.Enums.PowerSupplyEnum.AC,
            };

            // Act
            var result = _validator.Validate(assetClass, RepositoryAction.Create);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        #endregion

        #region Collection Validation

        [TestMethod]
        public void Validate_WithInvalidDataPorts_ShouldReturnInvalid()
        {
            // Arrange
            _helper.PopulateWithDemoData(upTo: DemoDataLayer.DeviceTypes);
            var deviceType = _helper.TestData.NonPowerProviderDeviceType();

            var assetClass = new AssetClass
            {
                Name = "Test",
                DeviceTypeId = new SdmObjectReference<DeviceType>(deviceType.Identifier),
                DataPorts = new List<DataPortInfo>
                {
                    new DataPortInfo { PortNumber = 1 },
                    new DataPortInfo { PortNumber = 1 },  // Duplicate
                }
            };

            // Act
            var result = _validator.Validate(assetClass, RepositoryAction.Create);

            // Assert
            using (new AssertionScope())
            {
                result.IsValid.Should().BeFalse();
                result.TryGetFailReason(
                    AssetClassValidationHandler.AssetClassValidationField.DataPortNumber,
                    out var reason).Should().BeTrue();
                reason.Should().Contain("Duplicate Data Port number found");
            }
        }

        [TestMethod]
        public void Validate_WithInvalidPowerPorts_ShouldReturnInvalid()
        {
            // Arrange
            _helper.PopulateWithDemoData(upTo: DemoDataLayer.DeviceTypes);
            var deviceType = _helper.TestData.NonPowerProviderDeviceType();

            var assetClass = new AssetClass
            {
                Name = "Test",
                DeviceTypeId = new SdmObjectReference<DeviceType>(deviceType.Identifier),
                PowerPorts = new List<PowerPortInfo>
                {
                    new PowerPortInfo { PortNumber = -1 } // Negative
                }
            };

            // Act
            var result = _validator.Validate(assetClass, RepositoryAction.Create);

            // Assert
            using (new AssertionScope())
            {
                result.IsValid.Should().BeFalse();
                result.TryGetFailReason(
                    AssetClassValidationHandler.AssetClassValidationField.PowerPortNumber,
                    out var reason).Should().BeTrue();
                reason.Should().Contain("cannot be negative");
            }
        }

        [TestMethod]
        public void Validate_WithInvalidHolders_ShouldReturnInvalid()
        {
            // Arrange
            _helper.PopulateWithDemoData(upTo: DemoDataLayer.DeviceTypes);
            var deviceType = _helper.TestData.NonPowerProviderDeviceType();

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
            var result = _validator.Validate(assetClass, RepositoryAction.Create);

            // Assert
            using (new AssertionScope())
            {
                result.IsValid.Should().BeFalse();
                result.TryGetFailReason(
                    AssetClassValidationHandler.AssetClassValidationField.HolderSlotNumber,
                    out var reason).Should().BeTrue();
                reason.Should().Contain("Multiple Holders");
            }
        }

        #endregion

        #region Change Tracking

        [TestMethod]
        public void Validate_OnlyValidatesChangedFields()
        {
            // Arrange
            _helper.PopulateWithDemoData(upTo: DemoDataLayer.DeviceTypes);
            var deviceType = _helper.TestData.NonPowerProviderDeviceType();

            // Create a valid asset class and persist it
            var assetClass = new AssetClass
            {
                Name = "Test Change Tracking",
                DeviceTypeId = new SdmObjectReference<DeviceType>(deviceType.Identifier),
                Depth = 5,       // Valid initially
                Width = 20       // Valid
            };

            var created = _helper.AssetManagement.AssetClasses.Create(assetClass);

            // Read it back from the database using TRUEFilter and filter in memory
            // This ensures the entity is properly loaded from DB with IsNew = false
            var loaded = _helper.AssetManagement.AssetClasses
                .Read(new TRUEFilterElement<AssetClass>())
                .Single(ac => ac.Identifier == created.Identifier);

            // Now set an invalid value and change a valid field
            loaded.Depth = -5;  // Make invalid (but don't save)
            loaded.ResetChangeTracking(); // Reset to establish this as the "loaded state"

            // Only change Width
            loaded.Width = 25;

            // Act
            var result = _validator.Validate(loaded, RepositoryAction.Create);

            // Assert
            result.IsValid.Should().BeTrue("Depth error should not be reported since it wasn't changed after the reset");
        }

        #endregion

        #region Integration Tests

        [TestMethod]
        public void Validate_WithExistingRepositoryData_ShouldWorkCorrectly()
        {
            // Arrange
            _helper.PopulateWithDemoData(upTo: DemoDataLayer.AssetClasses);

            var deviceType = _helper.TestData.NonPowerProviderDeviceType();
           
            var newAssetClass = new AssetClass
            {
                Name = "Brand New Device",
                DeviceTypeId = new SdmObjectReference<DeviceType>(deviceType.Identifier),
                Depth = 10,
                Width = 20
            };

            // Act
            var result = _validator.Validate(newAssetClass, RepositoryAction.Create);

            // Assert
            result.IsValid.Should().BeTrue("new asset class should be valid with existing repository data");
        }

        #endregion
    }
}