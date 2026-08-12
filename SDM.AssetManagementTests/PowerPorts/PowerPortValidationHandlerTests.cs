namespace SDM.AssetManagement.Tests.PowerPorts
{
    using FluentAssertions;
    using FluentAssertions.Execution;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using System;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Common.Validation;
    using Skyline.DataMiner.SDM.AssetManagement.Models;

    /// <summary>
    /// Unit tests for PowerPort validation business rules.
    /// Tests the static validation methods in PowerPortValidationHandler.
    /// </summary>
    [TestClass]
    public class PowerPortValidationHandlerTests
    {
        private static PowerPort CreateValidPowerPort()
        {
            return new PowerPort
            {
                PowerPortInfo =
                {
                    Name = "PSU-1",
                    PortNumber = 1,
                    OutputType = SharedMappers.DomIds.SlcAsset_Management.Enums.Outputtype.IO,
                },
                Asset = new SdmObjectReference<Asset>(Guid.NewGuid().ToString()),
            };
        }

        #region Mandatory Fields

        [TestMethod]
        public void MandatoryFields_WithValidPowerPort_ShouldBeValid()
        {
            // Arrange
            var powerPort = CreateValidPowerPort();

            // Act
            var isValid = PowerPortValidationHandler.AreMandatoryFieldsValid(powerPort, out var result);

            // Assert
            using (new AssertionScope())
            {
                isValid.Should().BeTrue();
                result.IsValid.Should().BeTrue();
                result.FailureReasons.Should().BeEmpty();
            }
        }

        [TestMethod]
        public void MandatoryFields_WithNullPowerPort_ShouldBeInvalid()
        {
            // Act
            var isValid = PowerPortValidationHandler.AreMandatoryFieldsValid(null, out var result);

            // Assert
            using (new AssertionScope())
            {
                isValid.Should().BeFalse();
                result.IsValid.Should().BeFalse();
            }
        }

        [TestMethod]
        [DataRow(null, DisplayName = "Null name")]
        [DataRow("", DisplayName = "Empty name")]
        [DataRow("   ", DisplayName = "Whitespace name")]
        public void MandatoryFields_WithInvalidName_ShouldBeInvalid(string name)
        {
            // Arrange
            var powerPort = CreateValidPowerPort();
            powerPort.PowerPortInfo.Name = name;

            // Act
            var isValid = PowerPortValidationHandler.AreMandatoryFieldsValid(powerPort, out var result);

            // Assert
            using (new AssertionScope())
            {
                isValid.Should().BeFalse();
                result.IsValid.Should().BeFalse();
            }
        }

        [TestMethod]
        public void MandatoryFields_WithNullPortNumber_ShouldBeInvalid()
        {
            // Arrange
            var powerPort = CreateValidPowerPort();
            powerPort.PowerPortInfo.PortNumber = null;

            // Act
            var isValid = PowerPortValidationHandler.AreMandatoryFieldsValid(powerPort, out var result);

            // Assert
            using (new AssertionScope())
            {
                isValid.Should().BeFalse();
                result.IsValid.Should().BeFalse();
            }
        }

        [TestMethod]
        public void MandatoryFields_WithNegativePortNumber_ShouldBeInvalid()
        {
            // Arrange
            var powerPort = CreateValidPowerPort();
            powerPort.PowerPortInfo.PortNumber = -1;

            // Act
            var isValid = PowerPortValidationHandler.AreMandatoryFieldsValid(powerPort, out var result);

            // Assert
            using (new AssertionScope())
            {
                isValid.Should().BeFalse();
                result.IsValid.Should().BeFalse();
            }
        }

        [TestMethod]
        public void MandatoryFields_WithZeroPortNumber_ShouldBeValid()
        {
            // Arrange
            var powerPort = CreateValidPowerPort();
            powerPort.PowerPortInfo.PortNumber = 0;

            // Act
            var isValid = PowerPortValidationHandler.AreMandatoryFieldsValid(powerPort, out var result);

            // Assert
            isValid.Should().BeTrue();
        }

        [TestMethod]
        public void MandatoryFields_WithNullOutputType_ShouldBeInvalid()
        {
            // Arrange
            var powerPort = CreateValidPowerPort();
            powerPort.PowerPortInfo.OutputType = null;

            // Act
            var isValid = PowerPortValidationHandler.AreMandatoryFieldsValid(powerPort, out var result);

            // Assert
            using (new AssertionScope())
            {
                isValid.Should().BeFalse();
                result.IsValid.Should().BeFalse();
            }
        }

        #endregion

        #region Asset Link

        [TestMethod]
        public void AssetLink_WithValidAsset_ShouldBeValid()
        {
            // Arrange
            var powerPort = CreateValidPowerPort();

            // Act
            var isValid = PowerPortValidationHandler.IsAssetLinkValid(powerPort, out var result);

            // Assert
            using (new AssertionScope())
            {
                isValid.Should().BeTrue();
                result.IsValid.Should().BeTrue();
            }
        }

        [TestMethod]
        public void AssetLink_WithNullAsset_ShouldBeInvalid()
        {
            // Arrange
            var powerPort = CreateValidPowerPort();
            powerPort.Asset = null;

            // Act
            var isValid = PowerPortValidationHandler.IsAssetLinkValid(powerPort, out var result);

            // Assert
            using (new AssertionScope())
            {
                isValid.Should().BeFalse();
                result.IsValid.Should().BeFalse();
            }
        }

        #endregion
    }
}
