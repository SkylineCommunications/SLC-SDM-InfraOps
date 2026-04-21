namespace SDM.AssetManagementTests.Validation
{
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Common.Validation;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    [TestClass]
    public class AssetClassValidationHandlerTests
    {
        #region IsAssetClassDeviceTypeValid Tests

        [TestMethod]
        public void IsAssetClassDeviceTypeValid_WithValidDeviceType_ReturnsTrue()
        {
            // Arrange
            var assetClass = new AssetClass
            {
                DeviceTypeId = new SdmObjectReference<DeviceType>("device-type-123"),
            };

            // Act
            var result = AssetClassValidationHandler.IsAssetClassDeviceTypeValid(assetClass, out var validationResult);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(validationResult.IsValid);
            Assert.AreEqual(0, validationResult.FailureReasons.Count);
        }

        [TestMethod]
        public void IsAssetClassDeviceTypeValid_WithEmptyDeviceType_ReturnsFalse()
        {
            // Arrange
            var assetClass = new AssetClass
            {
                DeviceTypeId = new SdmObjectReference<DeviceType>()
            };

            // Act
            var result = AssetClassValidationHandler.IsAssetClassDeviceTypeValid(assetClass, out var validationResult);

            // Assert
            Assert.IsFalse(result);
            Assert.IsFalse(validationResult.IsValid);
            Assert.IsTrue(validationResult.TryGetFailReason(
                AssetClassValidationHandler.AssetClassValidationField.DeviceTypeId,
                out var reason));
            Assert.AreEqual("Asset Class Device Type cannot be empty.", reason);
        }

        [TestMethod]
        public void IsAssetClassDeviceTypeValid_WithNullDeviceType_ReturnsFalse()
        {
            // Arrange
            var assetClass = new AssetClass
            {
                DeviceTypeId = null
            };

            // Act
            var result = AssetClassValidationHandler.IsAssetClassDeviceTypeValid(assetClass, out var validationResult);

            // Assert
            Assert.IsFalse(result);
            Assert.IsFalse(validationResult.IsValid);
        }

        #endregion

        #region Dimension Validation Tests

        [TestMethod]
        public void IsDepthValid_WithPositiveValue_ReturnsTrue()
        {
            // Arrange
            var assetClass = new AssetClass { Depth = 10.5 };

            // Act
            var result = AssetClassValidationHandler.IsDepthValid(assetClass, out var validationResult);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(validationResult.IsValid);
        }

        [TestMethod]
        public void IsDepthValid_WithZero_ReturnsTrue()
        {
            // Arrange
            var assetClass = new AssetClass { Depth = 0 };

            // Act
            var result = AssetClassValidationHandler.IsDepthValid(assetClass, out var validationResult);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(validationResult.IsValid);
        }

        [TestMethod]
        public void IsDepthValid_WithNegativeValue_ReturnsFalse()
        {
            // Arrange
            var assetClass = new AssetClass { Depth = -5.0 };

            // Act
            var result = AssetClassValidationHandler.IsDepthValid(assetClass, out var validationResult);

            // Assert
            Assert.IsFalse(result);
            Assert.IsFalse(validationResult.IsValid);
            Assert.IsTrue(validationResult.TryGetFailReason(
                AssetClassValidationHandler.AssetClassValidationField.Depth,
                out var reason));
            StringAssert.Contains(reason, "depth");
            StringAssert.Contains(reason, "negative");
        }

        [TestMethod]
        public void IsWidthValid_WithNegativeValue_ReturnsFalse()
        {
            // Arrange
            var assetClass = new AssetClass { Width = -10 };

            // Act
            var result = AssetClassValidationHandler.IsWidthValid(assetClass, out var validationResult);

            // Assert
            Assert.IsFalse(result);
            Assert.IsFalse(validationResult.IsValid);
        }

        [TestMethod]
        public void IsHeightValid_WithNegativeValue_ReturnsFalse()
        {
            // Arrange
            var assetClass = new AssetClass { Height = -1 };

            // Act
            var result = AssetClassValidationHandler.IsHeightValid(assetClass, out var validationResult);

            // Assert
            Assert.IsFalse(result);
            Assert.IsFalse(validationResult.IsValid);
        }

        [TestMethod]
        public void IsHeightUnitValid_WithNegativeValue_ReturnsFalse()
        {
            // Arrange
            var assetClass = new AssetClass { HeightU = -2 };

            // Act
            var result = AssetClassValidationHandler.IsHeightUnitValid(assetClass, out var validationResult);

            // Assert
            Assert.IsFalse(result);
            Assert.IsFalse(validationResult.IsValid);
        }

        [TestMethod]
        public void IsWeightValid_WithNegativeValue_ReturnsFalse()
        {
            // Arrange
            var assetClass = new AssetClass { Weight = -100 };

            // Act
            var result = AssetClassValidationHandler.IsWeightValid(assetClass, out var validationResult);

            // Assert
            Assert.IsFalse(result);
            Assert.IsFalse(validationResult.IsValid);
        }

        #endregion

        #region Power Consumption Validation Tests

        [TestMethod]
        public void IsTypicalPowerConsumptionValid_WithPositiveValue_ReturnsTrue()
        {
            // Arrange
            var assetClass = new AssetClass { TypicalPowerConsumption = 500 };

            // Act
            var result = AssetClassValidationHandler.IsTypicalPowerConsumptionValid(assetClass, out var validationResult);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(validationResult.IsValid);
        }

        [TestMethod]
        public void IsTypicalPowerConsumptionValid_WithNegativeValue_ReturnsFalse()
        {
            // Arrange
            var assetClass = new AssetClass { TypicalPowerConsumption = -100 };

            // Act
            var result = AssetClassValidationHandler.IsTypicalPowerConsumptionValid(assetClass, out var validationResult);

            // Assert
            Assert.IsFalse(result);
            Assert.IsFalse(validationResult.IsValid);
            Assert.IsTrue(validationResult.TryGetFailReason(
                AssetClassValidationHandler.AssetClassValidationField.TypicalPowerConsumption,
                out var reason));
            StringAssert.Contains(reason, "typical power consumption");
        }

        [TestMethod]
        public void IsMaxPowerConsumptionValid_WithPositiveValue_ReturnsTrue()
        {
            // Arrange
            var assetClass = new AssetClass { MaximumPowerConsumption = 1000 };

            // Act
            var result = AssetClassValidationHandler.IsMaxPowerConsumptionValid(assetClass, out var validationResult);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(validationResult.IsValid);
        }

        [TestMethod]
        public void IsMaxPowerConsumptionValid_WithNegativeValue_ReturnsFalse()
        {
            // Arrange
            var assetClass = new AssetClass { MaximumPowerConsumption = -500 };

            // Act
            var result = AssetClassValidationHandler.IsMaxPowerConsumptionValid(assetClass, out var validationResult);

            // Assert
            Assert.IsFalse(result);
            Assert.IsFalse(validationResult.IsValid);
        }

        #endregion

        #region ValidateAssetClassDataPort Tests

        [TestMethod]
        public void ValidateAssetClassDataPort_WithValidPorts_ReturnsValid()
        {
            // Arrange
            var assetClass = new AssetClass
            {
                DataPorts = new List<DataPortInfo>
                {
                    new DataPortInfo { PortNumber = 1 },
                    new DataPortInfo { PortNumber = 2 },
                    new DataPortInfo { PortNumber = 3 }
                }
            };

            // Act
            var result = AssetClassValidationHandler.ValidateAssetClassDataPort(assetClass);

            // Assert
            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void ValidateAssetClassDataPort_WithNullAssetClass_ReturnsInvalid()
        {
            // Arrange
            AssetClass assetClass = null;

            // Act
            var result = AssetClassValidationHandler.ValidateAssetClassDataPort(assetClass);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.TryGetFailReason(
                AssetClassValidationHandler.AssetClassValidationField.DataPortNumber,
                out var reason));
            StringAssert.Contains(reason, "Asset Class must be provided");
        }

        [TestMethod]
        public void ValidateAssetClassDataPort_WithEmptyPorts_ReturnsInvalid()
        {
            // Arrange
            var assetClass = new AssetClass
            {
                DataPorts = new List<DataPortInfo>()
            };

            // Act
            var result = AssetClassValidationHandler.ValidateAssetClassDataPort(assetClass);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.TryGetFailReason(
                AssetClassValidationHandler.AssetClassValidationField.DataPortNumber,
                out var reason));
            StringAssert.Contains(reason, "does not contain Data Ports");
        }

        [TestMethod]
        public void ValidateAssetClassDataPort_WithNegativePortNumber_ReturnsInvalid()
        {
            // Arrange
            var assetClass = new AssetClass
            {
                DataPorts = new List<DataPortInfo>
                {
                    new DataPortInfo { PortNumber = 1 },
                    new DataPortInfo { PortNumber = -5 }
                }
            };

            // Act
            var result = AssetClassValidationHandler.ValidateAssetClassDataPort(assetClass);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.TryGetFailReason(
                AssetClassValidationHandler.AssetClassValidationField.DataPortNumber,
                out var reason));
            StringAssert.Contains(reason, "cannot be negative");
        }

        [TestMethod]
        public void ValidateAssetClassDataPort_WithDuplicatePortNumbers_ReturnsInvalid()
        {
            // Arrange
            var assetClass = new AssetClass
            {
                DataPorts = new List<DataPortInfo>
                {
                    new DataPortInfo { PortNumber = 1 },
                    new DataPortInfo { PortNumber = 2 },
                    new DataPortInfo { PortNumber = 1 } // Duplicate
                }
            };

            // Act
            var result = AssetClassValidationHandler.ValidateAssetClassDataPort(assetClass);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.TryGetFailReason(
                AssetClassValidationHandler.AssetClassValidationField.DataPortNumber,
                out var reason));
            StringAssert.Contains(reason, "Multiple Data Ports");
            StringAssert.Contains(reason, "same Port Number");
        }

        #endregion

        #region ValidateAssetClassPowerPort Tests

        [TestMethod]
        public void ValidateAssetClassPowerPort_WithValidPorts_ReturnsValid()
        {
            // Arrange
            var assetClass = new AssetClass
            {
                PowerPorts = new List<PowerPortInfo>
                {
                    new PowerPortInfo { PortNumber = 1 },
                    new PowerPortInfo { PortNumber = 2 }
                }
            };

            // Act
            var result = AssetClassValidationHandler.ValidateAssetClassPowerPort(assetClass);

            // Assert
            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void ValidateAssetClassPowerPort_WithNullAssetClass_ReturnsInvalid()
        {
            // Arrange
            AssetClass assetClass = null;

            // Act
            var result = AssetClassValidationHandler.ValidateAssetClassPowerPort(assetClass);

            // Assert
            Assert.IsFalse(result.IsValid);
        }

        [TestMethod]
        public void ValidateAssetClassPowerPort_WithEmptyPorts_ReturnsInvalid()
        {
            // Arrange
            var assetClass = new AssetClass
            {
                PowerPorts = new List<PowerPortInfo>()
            };

            // Act
            var result = AssetClassValidationHandler.ValidateAssetClassPowerPort(assetClass);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.TryGetFailReason(
                AssetClassValidationHandler.AssetClassValidationField.PowerPortNumber,
                out var reason));
            StringAssert.Contains(reason, "does not contain Power Ports");
        }

        [TestMethod]
        public void ValidateAssetClassPowerPort_WithNegativePortNumber_ReturnsInvalid()
        {
            // Arrange
            var assetClass = new AssetClass
            {
                PowerPorts = new List<PowerPortInfo>
                {
                    new PowerPortInfo { PortNumber = -1 }
                }
            };
           
            // Act
            var result = AssetClassValidationHandler.ValidateAssetClassPowerPort(assetClass);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.TryGetFailReason(
                AssetClassValidationHandler.AssetClassValidationField.PowerPortNumber,
                out var reason));
            StringAssert.Contains(reason, "cannot be negative");
        }

        [TestMethod]
        public void ValidateAssetClassPowerPort_WithDuplicatePortNumbers_ReturnsInvalid()
        {
            // Arrange
            var assetClass = new AssetClass
            {
                PowerPorts = new List<PowerPortInfo>
                {
                    new PowerPortInfo { PortNumber = 5 },
                    new PowerPortInfo { PortNumber = 5 }
                }
            };
           
            // Act
            var result = AssetClassValidationHandler.ValidateAssetClassPowerPort(assetClass);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.TryGetFailReason(
                AssetClassValidationHandler.AssetClassValidationField.PowerPortNumber,
                out var reason));
            StringAssert.Contains(reason, "Multiple Power Ports");
        }

        #endregion

        #region ValidateAssetClassHolders Tests

        [TestMethod]
        public void ValidateAssetClassHolders_WithValidHolders_ReturnsValid()
        {
            // Arrange
            var assetClass = new AssetClass
            {
                Holders = new List<AssetHolder>
                {
                    new AssetHolder
                    {
                        SlotNumber = 1,
                        HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Card
                    },
                    new AssetHolder
                    {
                        SlotNumber = 2,
                        HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Card
                    }
                }
            };
         
            // Act
            var result = AssetClassValidationHandler.ValidateAssetClassHolders(assetClass);

            // Assert
            Assert.IsTrue(result.IsValid);
        }

        [TestMethod]
        public void ValidateAssetClassHolders_WithNullAssetClass_ReturnsInvalid()
        {
            // Arrange
            AssetClass assetClass = null;

            // Act
            var result = AssetClassValidationHandler.ValidateAssetClassHolders(assetClass);

            // Assert
            Assert.IsFalse(result.IsValid);
        }

        [TestMethod]
        public void ValidateAssetClassHolders_WithNullHolders_ReturnsInvalid()
        {
            // Arrange
            var assetClass = new AssetClass
            {
                Holders = null
            };

            // Act
            var result = AssetClassValidationHandler.ValidateAssetClassHolders(assetClass);

            // Assert
            Assert.IsFalse(result.IsValid);
        }

        //todo when support to nullable is added
        //[TestMethod] 
        //public void ValidateAssetClassHolders_WithNullSlotNumber_ReturnsInvalid()
        //{
        //    // Arrange
        //    var assetClass = new AssetClass
        //    {
        //        Holders = new List<AssetHolder>
        //        {
        //            new AssetHolder
        //            {
        //                SlotNumber = null,
        //                HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Blade
        //            }
        //        }
        //    };
        //    assetClass.HoldersField.Changed = true;

        //    // Act
        //    var result = AssetClassValidationHandler.ValidateAssetClassHolders(assetClass);

        //    // Assert
        //    Assert.IsFalse(result.IsValid);
        //    Assert.IsTrue(result.TryGetFailReason(
        //        AssetClassValidationHandler.AssetClassValidationField.HolderSlotNumber,
        //        out var reason));
        //    StringAssert.Contains(reason, "must have a value");
        //}

        [TestMethod]
        public void ValidateAssetClassHolders_WithNegativeSlotNumber_ReturnsInvalid()
        {
            // Arrange
            var assetClass = new AssetClass
            {
                Holders = new List<AssetHolder>
                {
                    new AssetHolder
                    {
                        SlotNumber = -1,
                        HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Chassis
                    }
                }
            };
           
            // Act
            var result = AssetClassValidationHandler.ValidateAssetClassHolders(assetClass);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.TryGetFailReason(
                AssetClassValidationHandler.AssetClassValidationField.HolderSlotNumber,
                out var reason));
            StringAssert.Contains(reason, "cannot be negative");
        }

        [TestMethod]
        public void ValidateAssetClassHolders_WithDuplicateSlotNumberAndRole_ReturnsInvalid()
        {
            // Arrange
            var assetClass = new AssetClass
            {
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
            var result = AssetClassValidationHandler.ValidateAssetClassHolders(assetClass);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsTrue(result.TryGetFailReason(
                AssetClassValidationHandler.AssetClassValidationField.HolderSlotNumber,
                out var reason));
            StringAssert.Contains(reason, "Multiple Holders");
            StringAssert.Contains(reason, "same Slot Number");
        }

        [TestMethod]
        public void ValidateAssetClassHolders_WithSameSlotNumberDifferentRole_ReturnsValid()
        {
            // Arrange
            var assetClass = new AssetClass
            {
                Holders = new List<AssetHolder>
                {
                    new AssetHolder
                    {
                        SlotNumber = 1,
                        HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Fan
                    },
                    new AssetHolder
                    {
                        SlotNumber = 1,
                        HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Chassis
                    }
                }
            };
           
            // Act
            var result = AssetClassValidationHandler.ValidateAssetClassHolders(assetClass);

            // Assert
            Assert.IsTrue(result.IsValid);
        }

        #endregion
    }
}