namespace SDM.AssetManagement.Tests.AssetClasses
{
    using System.Collections.Generic;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.AssetManagement.Validation;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Unit tests for AssetClass validation business rules.
    /// Tests the static validation methods in AssetClassValidationHandler.
    /// </summary>
    [TestClass]
    public class AssetClassValidationHandlerTests
    {
        #region DeviceType Validation

        [TestMethod]
        public void DeviceType_WithValidGuid_ShouldBeValid()
        {
            // Arrange
            var assetClass = new AssetClass
            {
                DeviceTypeId = new SdmObjectReference<DeviceType>(Guid.NewGuid().ToString()),
            };

            // Act
            var isValid = AssetClassValidationHandler.IsAssetClassDeviceTypeValid(assetClass, out var result);

            // Assert
            using (new AssertionScope())
            {
                isValid.Should().BeTrue();
                result.IsValid.Should().BeTrue();
                result.FailureReasons.Should().BeEmpty();
            }
        }

        [TestMethod]
        [DataRow(null, DisplayName = "Null DeviceType")]
        [DataRow("", DisplayName = "Empty DeviceType")]
        public void DeviceType_WithInvalidReference_ShouldBeInvalid(string deviceTypeId)
        {
            // Arrange
            var assetClass = new AssetClass
            {
                DeviceTypeId = string.IsNullOrEmpty(deviceTypeId) 
                    ? (deviceTypeId == null ? null : new SdmObjectReference<DeviceType>()) 
                    : new SdmObjectReference<DeviceType>(deviceTypeId)
            };

            // Act
            var isValid = AssetClassValidationHandler.IsAssetClassDeviceTypeValid(assetClass, out var result);

            // Assert
            using (new AssertionScope())
            {
                isValid.Should().BeFalse();
                result.IsValid.Should().BeFalse();
                result.TryGetFailReason(
                    AssetClassValidationHandler.AssetClassValidationField.DeviceTypeId,
                    out var reason).Should().BeTrue();
                reason.Should().Contain("Device Type");
            }
        }

        #endregion

        #region Physical Dimension Validation

        [TestMethod]
        [DataRow(10.5, DisplayName = "Positive Value")]
        [DataRow(0, DisplayName = "Zero")]
        public void Depth_WithValidValue_ShouldBeValid(double depth)
        {
            // Arrange
            var assetClass = new AssetClass { Depth = depth };

            // Act
            var isValid = AssetClassValidationHandler.IsDepthValid(assetClass, out var result);

            // Assert
            using (new AssertionScope())
            {
                isValid.Should().BeTrue();
                result.IsValid.Should().BeTrue();
            }
        }

        [TestMethod]
        [DataRow(-5.0, "Depth", DisplayName = "Negative Depth")]
        [DataRow(-10.0, "Width", DisplayName = "Negative Width")]
        [DataRow(-1.0, "Height", DisplayName = "Negative Height")]
        [DataRow(-2.0, "HeightU", DisplayName = "Negative HeightU")]
        [DataRow(-100.0, "Weight", DisplayName = "Negative Weight")]
        public void PhysicalDimension_WithNegativeValue_ShouldBeInvalid(double value, string property)
        {
            // Arrange
            var deviceType = new DeviceType();
            var assetClass = new AssetClass()
            {
                DeviceTypeId = deviceType,
            };

            switch (property)
            {
                case "Depth": assetClass.Depth = value; break;
                case "Width": assetClass.Width = value; break;
                case "Height": assetClass.Height = value; break;
                case "HeightU": assetClass.HeightU = value; break;
                case "Weight": assetClass.Weight = value; break;
            }

            // Act
            var isValid = property switch
            {
                "Depth" => AssetClassValidationHandler.IsDepthValid(assetClass, out var r1),
                "Width" => AssetClassValidationHandler.IsWidthValid(assetClass, out var r2),
                "Height" => AssetClassValidationHandler.IsHeightValid(assetClass, out var r3),
                "HeightU" => AssetClassValidationHandler.IsHeightUnitValid(assetClass, deviceType, out var r4),
                "Weight" => AssetClassValidationHandler.IsWeightValid(assetClass, out var r5),
                _ => false
            };

            // Assert - All negative dimension validations should fail
            isValid.Should().BeFalse($"{property} cannot be negative");
        }

        [TestMethod]
        [DataRow(null, DisplayName = "Null HeightU")]
        [DataRow(0.0, DisplayName = "Zero HeightU")]
        [DataRow(-1.0, DisplayName = "Negative HeightU")]
        public void HeightU_RackUnitConsumer_WithoutPositiveValue_ShouldBeInvalid(double? heightU)
        {
            // Arrange
            var deviceType = new DeviceType
            {
                TagsInfo =
                {
                    Tags = new List<SlcAsset_Management.Enums.TagOption>
                    {
                        SlcAsset_Management.Enums.TagOption.RackUnitConsumer
                    },
                },
            };
            var assetClass = new AssetClass
            {
                HeightU = heightU,
                DeviceTypeId = deviceType,
            };

            // Act
            var isValid = AssetClassValidationHandler.IsHeightUnitValid(assetClass, deviceType, out var result);

            // Assert
            using (new AssertionScope())
            {
                isValid.Should().BeFalse();
                result.TryGetFailReason(
                    AssetClassValidationHandler.AssetClassValidationField.HeightU,
                    out var reason).Should().BeTrue();
                reason.Should().Contain("Rack Unit Consumer");
            }
        }

        [TestMethod]
        public void HeightU_RackUnitConsumer_WithPositiveValue_ShouldBeValid()
        {
            // Arrange
            var deviceType = new DeviceType
            {
                TagsInfo =
                {
                    Tags = new List<SlcAsset_Management.Enums.TagOption>
                    {
                        SlcAsset_Management.Enums.TagOption.RackUnitConsumer
                    },
                },
            };
            var assetClass = new AssetClass
            {
                HeightU = 2.0,
                DeviceTypeId = deviceType,
            };
            // Act
            var isValid = AssetClassValidationHandler.IsHeightUnitValid(assetClass, deviceType, out var result);

            // Assert
            using (new AssertionScope())
            {
                isValid.Should().BeTrue();
                result.IsValid.Should().BeTrue();
            }
        }

        [TestMethod]
        [DataRow(null, DisplayName = "Null HeightU")]
        [DataRow(0.0, DisplayName = "Zero HeightU")]
        public void HeightU_NonRackUnitConsumer_WithoutPositiveValue_ShouldBeValid(double? heightU)
        {
            // Arrange
            var deviceType = new DeviceType
            {
                TagsInfo =
                {
                    Tags = new List<SlcAsset_Management.Enums.TagOption>
                    {
                    },
                },
            };
            var assetClass = new AssetClass
            {
                HeightU = heightU,
                DeviceTypeId = deviceType,
            };
            // Act
            var isValid = AssetClassValidationHandler.IsHeightUnitValid(assetClass, deviceType, out var result);

            // Assert
            using (new AssertionScope())
            {
                isValid.Should().BeTrue();
                result.IsValid.Should().BeTrue();
            }
        }

        #endregion

        #region Power Consumption Validation

        [TestMethod]
        [DataRow(500.0, DisplayName = "Typical Power")]
        [DataRow(1000.0, DisplayName = "Maximum Power")]
        public void PowerConsumption_WithPositiveValue_ShouldBeValid(double power)
        {
            // Arrange
            var assetClass = new AssetClass
            {
                TypicalPowerConsumption = power,
                MaximumPowerConsumption = power
            };

            // Act
            var isTypicalValid = AssetClassValidationHandler.IsTypicalPowerConsumptionValid(assetClass, out var typicalResult);
            var isMaxValid = AssetClassValidationHandler.IsMaxPowerConsumptionValid(assetClass, out var maxResult);

            // Assert
            using (new AssertionScope())
            {
                isTypicalValid.Should().BeTrue();
                typicalResult.IsValid.Should().BeTrue();
                isMaxValid.Should().BeTrue();
                maxResult.IsValid.Should().BeTrue();
            }
        }

        [TestMethod]
        [DataRow(-100.0, "Typical", DisplayName = "Negative Typical Power")]
        [DataRow(-500.0, "Maximum", DisplayName = "Negative Maximum Power")]
        public void PowerConsumption_WithNegativeValue_ShouldBeInvalid(double power, string type)
        {
            // Arrange
            var assetClass = new AssetClass();
            if (type == "Typical")
                assetClass.TypicalPowerConsumption = power;
            else
                assetClass.MaximumPowerConsumption = power;

            // Act
            var isValid = type == "Typical"
                ? AssetClassValidationHandler.IsTypicalPowerConsumptionValid(assetClass, out var r1)
                : AssetClassValidationHandler.IsMaxPowerConsumptionValid(assetClass, out var r2);

            // Assert
            isValid.Should().BeFalse($"{type} power consumption cannot be negative");
        }

        #endregion

        #region Data Port Validation

        [TestMethod]
        public void DataPorts_WithNullAssetClass_ShouldBeInvalid()
        {
            // Act
            var result = AssetClassValidationHandler.ValidateAssetClassDataPort(null);

            // Assert
            using (new AssertionScope())
            {
                result.IsValid.Should().BeFalse();
                result.TryGetFailReason(
                    AssetClassValidationHandler.AssetClassValidationField.DataPortNumber,
                    out var reason).Should().BeTrue();
                reason.Should().Contain("must be provided");
            }
        }

        [TestMethod]
        public void DataPorts_WithEmptyList_ShouldBeValid()
        {
            // Arrange
            var assetClass = new AssetClass
            {
                DataPorts = new List<DataPortInfo>()
            };

            // Act
            var result = AssetClassValidationHandler.ValidateAssetClassDataPort(assetClass);

            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void DataPorts_WithNegativePortNumber_ShouldBeInvalid()
        {
            // Arrange
            var assetClass = new AssetClass
            {
                DataPorts = new List<DataPortInfo>
                {
                    new DataPortInfo { PortNumber = 1 },
                    new DataPortInfo { PortNumber = -5 },
                }
            };

            // Act
            var result = AssetClassValidationHandler.ValidateAssetClassDataPort(assetClass);

            // Assert
            using (new AssertionScope())
            {
                result.IsValid.Should().BeFalse();
                result.TryGetFailReason(
                    AssetClassValidationHandler.AssetClassValidationField.DataPortNumber,
                    out var reason).Should().BeTrue();
                reason.Should().Contain("cannot be negative");
            }
        }

        [TestMethod]
        public void DataPorts_WithDuplicatePortNumbers_ShouldBeInvalid()
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
        public void DataPorts_WithMissingName_ShouldBeInvalid()
        {
            // Arrange
            var assetClass = new AssetClass
            {
                DataPorts = new List<DataPortInfo>
                {
                    new DataPortInfo { PortNumber = 1, Name = string.Empty, PortType = new SdmObjectReference<PortType>(System.Guid.NewGuid().ToString()) },
                }
            };

            // Act
            var result = AssetClassValidationHandler.ValidateAssetClassDataPort(assetClass);

            // Assert
            using (new AssertionScope())
            {
                result.IsValid.Should().BeFalse();
                result.TryGetFailReason(
                    AssetClassValidationHandler.AssetClassValidationField.DataPortName,
                    out var reason).Should().BeTrue();
                reason.Should().Contain("name must have a value");
            }
        }

        [TestMethod]
        public void DataPorts_WithMissingPortType_ShouldBeInvalid()
        {
            // Arrange
            var assetClass = new AssetClass
            {
                DataPorts = new List<DataPortInfo>
                {
                    new DataPortInfo { PortNumber = 1, Name = "Data 1", PortType = null },
                }
            };

            // Act
            var result = AssetClassValidationHandler.ValidateAssetClassDataPort(assetClass);

            // Assert
            using (new AssertionScope())
            {
                result.IsValid.Should().BeFalse();
                result.TryGetFailReason(
                    AssetClassValidationHandler.AssetClassValidationField.DataPortType,
                    out var reason).Should().BeTrue();
                reason.Should().Contain("type must be selected");
            }
        }

        [TestMethod]
        public void DataPorts_WithNameAndPortTypeSet_ShouldBeValid()
        {
            // Arrange
            var assetClass = new AssetClass
            {
                DataPorts = new List<DataPortInfo>
                {
                    new DataPortInfo { PortNumber = 1, Name = "Data 1", PortType = new SdmObjectReference<PortType>(System.Guid.NewGuid().ToString()) },
                }
            };

            // Act
            var result = AssetClassValidationHandler.ValidateAssetClassDataPort(assetClass);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        #endregion

        #region Power Port Validation

        [TestMethod]
        public void PowerPorts_WithNullAssetClass_ShouldBeInvalid()
        {
            // Act
            var result = AssetClassValidationHandler.ValidateAssetClassPowerPort(null);

            // Assert
            result.IsValid.Should().BeFalse();
        }

        [TestMethod]
        public void PowerPorts_WithEmptyList_ShouldBeValid()
        {
            // Arrange
            var assetClass = new AssetClass
            {
                PowerPorts = new List<PowerPortInfo>()
            };

            // Act
            var result = AssetClassValidationHandler.ValidateAssetClassPowerPort(assetClass);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void PowerPorts_WithNegativePortNumber_ShouldBeInvalid()
        {
            // Arrange
            var assetClass = new AssetClass
            {
                PowerPorts = new List<PowerPortInfo>
                {
                    new PowerPortInfo { PortNumber = -1 },
                }
            };

            // Act
            var result = AssetClassValidationHandler.ValidateAssetClassPowerPort(assetClass);

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
        public void PowerPorts_WithDuplicatePortNumbers_ShouldBeInvalid()
        {
            // Arrange
            var assetClass = new AssetClass
            {
                PowerPorts = new List<PowerPortInfo>
                {
                    new PowerPortInfo { PortNumber = 5 },
                    new PowerPortInfo { PortNumber = 5 }, // Duplicate
                }
            };

            // Act
            var result = AssetClassValidationHandler.ValidateAssetClassPowerPort(assetClass);

            // Assert
            using (new AssertionScope())
            {
                result.IsValid.Should().BeFalse();
                result.TryGetFailReason(
                    AssetClassValidationHandler.AssetClassValidationField.PowerPortNumber,
                    out var reason).Should().BeTrue();
                reason.Should().Contain("Duplicate Power Port number found");
            }
        }

        [TestMethod]
        public void PowerPorts_WithMissingName_ShouldBeInvalid()
        {
            // Arrange
            var assetClass = new AssetClass
            {
                PowerPorts = new List<PowerPortInfo>
                {
                    new PowerPortInfo { PortNumber = 1, Name = string.Empty, PortType = new SdmObjectReference<PortType>(System.Guid.NewGuid().ToString()) },
                }
            };

            // Act
            var result = AssetClassValidationHandler.ValidateAssetClassPowerPort(assetClass);

            // Assert
            using (new AssertionScope())
            {
                result.IsValid.Should().BeFalse();
                result.TryGetFailReason(
                    AssetClassValidationHandler.AssetClassValidationField.PowerPortName,
                    out var reason).Should().BeTrue();
                reason.Should().Contain("name must have a value");
            }
        }

        [TestMethod]
        public void PowerPorts_WithMissingPortType_ShouldBeInvalid()
        {
            // Arrange
            var assetClass = new AssetClass
            {
                PowerPorts = new List<PowerPortInfo>
                {
                    new PowerPortInfo { PortNumber = 1, Name = "Power 1", PortType = null },
                }
            };

            // Act
            var result = AssetClassValidationHandler.ValidateAssetClassPowerPort(assetClass);

            // Assert
            using (new AssertionScope())
            {
                result.IsValid.Should().BeFalse();
                result.TryGetFailReason(
                    AssetClassValidationHandler.AssetClassValidationField.PowerPortType,
                    out var reason).Should().BeTrue();
                reason.Should().Contain("type must be selected");
            }
        }

        [TestMethod]
        public void PowerPorts_WithNameAndPortTypeSet_ShouldBeValid()
        {
            // Arrange
            var assetClass = new AssetClass
            {
                PowerPorts = new List<PowerPortInfo>
                {
                    new PowerPortInfo { PortNumber = 1, Name = "Power 1", PortType = new SdmObjectReference<PortType>(System.Guid.NewGuid().ToString()) },
                }
            };

            // Act
            var result = AssetClassValidationHandler.ValidateAssetClassPowerPort(assetClass);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        #endregion

        #region Holder Validation

        [TestMethod]
        public void Holders_WithNullAssetClass_ShouldBeInvalid()
        {
            // Act
            var result = AssetClassValidationHandler.ValidateAssetClassHolders(null);

            // Assert
            result.IsValid.Should().BeFalse();
        }

        [TestMethod]
        public void Holders_WithNullHoldersList_ShouldBeValid()
        {
            // Arrange
            var assetClass = new AssetClass { Holders = null };

            // Act
            var result = AssetClassValidationHandler.ValidateAssetClassHolders(assetClass);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void Holders_WithNegativeSlotNumber_ShouldBeInvalid()
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
            using (new AssertionScope())
            {
                result.IsValid.Should().BeFalse();
                result.TryGetFailReason(
                    AssetClassValidationHandler.AssetClassValidationField.HolderSlotNumber,
                    out var reason).Should().BeTrue();
                reason.Should().Contain("cannot be negative");
            }
        }

        [TestMethod]
        public void Holders_WithDuplicateSlotNumberAndRole_ShouldBeInvalid()
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
            using (new AssertionScope())
            {
                result.IsValid.Should().BeFalse();
                result.TryGetFailReason(
                    AssetClassValidationHandler.AssetClassValidationField.HolderSlotNumber,
                    out var reason).Should().BeTrue();
                reason.Should().Contain("Multiple Holders");
                reason.Should().Contain("same Slot Number");
            }
        }

        [TestMethod]
        public void Holders_WithSameSlotNumberDifferentRole_ShouldBeValid()
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
            result.IsValid.Should().BeTrue("same slot number with different roles is allowed");
        }

        #endregion

        #region Ported From Shared Tests

        [TestMethod]
        [DataRow("Depth", null, DisplayName = "Null Depth")]
        [DataRow("Width", 20.0, DisplayName = "Positive Width")]
        [DataRow("Width", 0.0, DisplayName = "Zero Width")]
        [DataRow("Width", null, DisplayName = "Null Width")]
        [DataRow("Height", 15.0, DisplayName = "Positive Height")]
        [DataRow("Height", 0.0, DisplayName = "Zero Height")]
        [DataRow("Height", null, DisplayName = "Null Height")]
        [DataRow("HeightU", 2.0, DisplayName = "Positive HeightU")]
        [DataRow("Weight", 7.5, DisplayName = "Positive Weight")]
        [DataRow("Weight", 0.0, DisplayName = "Zero Weight")]
        [DataRow("Weight", null, DisplayName = "Null Weight")]
        [DataRow("Typical", 0.0, DisplayName = "Zero Typical Power")]
        [DataRow("Typical", null, DisplayName = "Null Typical Power")]
        [DataRow("Maximum", 0.0, DisplayName = "Zero Maximum Power")]
        [DataRow("Maximum", null, DisplayName = "Null Maximum Power")]
        public void NumericProperty_WithNonNegativeOrNullValue_ShouldBeValid(string property, double? value)
        {
            // Arrange
            var deviceType = new DeviceType();
            var assetClass = new AssetClass { DeviceTypeId = deviceType };

            switch (property)
            {
                case "Depth": assetClass.Depth = value; break;
                case "Width": assetClass.Width = value; break;
                case "Height": assetClass.Height = value; break;
                case "HeightU": assetClass.HeightU = value; break;
                case "Weight": assetClass.Weight = value; break;
                case "Typical": assetClass.TypicalPowerConsumption = value; break;
                case "Maximum": assetClass.MaximumPowerConsumption = value; break;
            }

            // Act
            ValidationResult result;
            var isValid = property switch
            {
                "Depth" => AssetClassValidationHandler.IsDepthValid(assetClass, out result),
                "Width" => AssetClassValidationHandler.IsWidthValid(assetClass, out result),
                "Height" => AssetClassValidationHandler.IsHeightValid(assetClass, out result),
                "HeightU" => AssetClassValidationHandler.IsHeightUnitValid(assetClass, deviceType, out result),
                "Weight" => AssetClassValidationHandler.IsWeightValid(assetClass, out result),
                "Typical" => AssetClassValidationHandler.IsTypicalPowerConsumptionValid(assetClass, out result),
                _ => AssetClassValidationHandler.IsMaxPowerConsumptionValid(assetClass, out result),
            };

            // Assert
            using (new AssertionScope())
            {
                isValid.Should().BeTrue();
                result.IsValid.Should().BeTrue();
                result.FailureReasons.Should().BeEmpty();
            }
        }

        [TestMethod]
        [DataRow("Depth", -1.0, DisplayName = "Negative Depth")]
        [DataRow("Width", -1.0, DisplayName = "Negative Width")]
        [DataRow("Height", -1.0, DisplayName = "Negative Height")]
        [DataRow("HeightU", -1.0, DisplayName = "Negative HeightU")]
        [DataRow("Weight", -1.0, DisplayName = "Negative Weight")]
        [DataRow("Typical", -1.0, DisplayName = "Negative Typical Power")]
        [DataRow("Maximum", -1.0, DisplayName = "Negative Maximum Power")]
        public void NumericProperty_WithNegativeValue_ShouldReturnFailReasonOnField(string property, double value)
        {
            // Arrange
            var deviceType = new DeviceType();
            var assetClass = new AssetClass { DeviceTypeId = deviceType };
            AssetClassValidationHandler.AssetClassValidationField expectedField;

            switch (property)
            {
                case "Depth": assetClass.Depth = value; expectedField = AssetClassValidationHandler.AssetClassValidationField.Depth; break;
                case "Width": assetClass.Width = value; expectedField = AssetClassValidationHandler.AssetClassValidationField.Width; break;
                case "Height": assetClass.Height = value; expectedField = AssetClassValidationHandler.AssetClassValidationField.Height; break;
                case "HeightU": assetClass.HeightU = value; expectedField = AssetClassValidationHandler.AssetClassValidationField.HeightU; break;
                case "Weight": assetClass.Weight = value; expectedField = AssetClassValidationHandler.AssetClassValidationField.Weight; break;
                case "Typical": assetClass.TypicalPowerConsumption = value; expectedField = AssetClassValidationHandler.AssetClassValidationField.TypicalPowerConsumption; break;
                default: assetClass.MaximumPowerConsumption = value; expectedField = AssetClassValidationHandler.AssetClassValidationField.MaxPowerConsumption; break;
            }

            // Act
            ValidationResult result;
            var isValid = property switch
            {
                "Depth" => AssetClassValidationHandler.IsDepthValid(assetClass, out result),
                "Width" => AssetClassValidationHandler.IsWidthValid(assetClass, out result),
                "Height" => AssetClassValidationHandler.IsHeightValid(assetClass, out result),
                "HeightU" => AssetClassValidationHandler.IsHeightUnitValid(assetClass, deviceType, out result),
                "Weight" => AssetClassValidationHandler.IsWeightValid(assetClass, out result),
                "Typical" => AssetClassValidationHandler.IsTypicalPowerConsumptionValid(assetClass, out result),
                _ => AssetClassValidationHandler.IsMaxPowerConsumptionValid(assetClass, out result),
            };

            // Assert
            using (new AssertionScope())
            {
                isValid.Should().BeFalse();
                result.IsValid.Should().BeFalse();
                result.TryGetFailReason(expectedField, out var reason).Should().BeTrue();
                reason.Should().NotBeNullOrWhiteSpace();
            }
        }

        [TestMethod]
        [DataRow(false, 1L, SlcAsset_Management.Enums.HierarchyRoleEnum.Module, true, DisplayName = "No holders, new slot 1")]
        [DataRow(false, 0L, SlcAsset_Management.Enums.HierarchyRoleEnum.Module, true, DisplayName = "No holders, slot 0 boundary")]
        [DataRow(true, 0L, SlcAsset_Management.Enums.HierarchyRoleEnum.Card, false, DisplayName = "Existing slot and role duplicated")]
        [DataRow(true, 0L, SlcAsset_Management.Enums.HierarchyRoleEnum.Module, true, DisplayName = "Existing slot, different role")]
        [DataRow(true, -1L, SlcAsset_Management.Enums.HierarchyRoleEnum.Chassis, false, DisplayName = "Negative slot")]
        public void Holders_WithCandidateSlot_ShouldValidateAgainstExistingHolders(
            bool withExistingHolders,
            long candidateSlot,
            SlcAsset_Management.Enums.HierarchyRoleEnum candidateRole,
            bool expectedValid)
        {
            // Arrange
            var holders = new List<AssetHolder>();
            if (withExistingHolders)
            {
                holders.Add(new AssetHolder { SlotNumber = 0, HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Card });
                holders.Add(new AssetHolder { SlotNumber = 1, HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Card });
                holders.Add(new AssetHolder { SlotNumber = 0, HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Fan });
            }

            holders.Add(new AssetHolder { SlotNumber = candidateSlot, HierarchyRole = candidateRole });
            var assetClass = new AssetClass { Holders = holders };

            // Act
            var result = AssetClassValidationHandler.ValidateAssetClassHolders(assetClass);

            // Assert
            using (new AssertionScope())
            {
                result.IsValid.Should().Be(expectedValid);
                if (!expectedValid)
                {
                    result.TryGetFailReason(
                        AssetClassValidationHandler.AssetClassValidationField.HolderSlotNumber,
                        out var reason).Should().BeTrue();
                }
            }
        }

        #endregion
    }
}