namespace SDM.AssetManagement.Tests.AssetClasses
{
    using System;
    using System.Linq;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SDM.AssetManagement.Tests.Setup;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Extensions;
 


    /// <summary>
    /// Filter and query tests for AssetClass repository operations.
    /// </summary>
    [TestClass]
    public class AssetClassDomStorageProvider_FilterTests : BaseRepositoryTest
    {
        #region Basic Field Filters

        [TestMethod]
        public void ReadFilter_DeviceName_Equal_ShouldReturnMatchingAssetClass()
        {
            // Arrange
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.AssetClasses);

            const string deviceName = "KVM Switch";
            var filter = AssetClassExposers.DeviceName.Equal(deviceName);

            // Act
            var results = Helper.AssetManagement.AssetClasses.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().HaveCount(1, $"should find exactly one '{deviceName}'");
                var assetClass = results.First();
                assetClass.Name.Should().Be(deviceName);
            }
        }

        [TestMethod]
        public void ReadFilter_DeviceDescription_Contains_ShouldReturnMatchingAssetClasses()
        {
            // Arrange
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.AssetClasses);

            const string searchTerm = "Panel";
            var filter = AssetClassExposers.DeviceDescription.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);

            // Act
            var results = Helper.AssetManagement.AssetClasses.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().NotBeEmpty($"should find asset classes with '{searchTerm}' in description");
                results.Should().OnlyContain(ac => ac.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }
        }

        [TestMethod]
        public void ReadFilter_Width_Equal_ShouldReturnMatchingAssetClass()
        {
            // Arrange
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.AssetClasses);

            var targetAssetClass = Helper.TestData.AssetClasses.First();
            var width = targetAssetClass.Width;
            var filter = AssetClassExposers.Width.UncheckedEqual(width);

            // Act
            var results = Helper.AssetManagement.AssetClasses.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().NotBeEmpty($"should find asset classes with width {width}");
                results.Should().OnlyContain(ac => ac.Width == width);
                results.Should().Contain(ac => ac.Identifier == targetAssetClass.Identifier);
            }
        }

        [TestMethod]
        public void ReadFilter_FrontImage_Equal_ShouldReturnMatchingAssetClass()
        {
            // Arrange
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.AssetClasses);

            const string imageName = "fw-front.png";
            var filter = AssetClassExposers.FrontImage.Equal(imageName);

            // Act
            var results = Helper.AssetManagement.AssetClasses.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().HaveCount(1, $"should find exactly one asset class with front image '{imageName}'");
                results.First().FrontImage.Should().Be(imageName);
            }
        }

        [TestMethod]
        public void ReadFilter_BackImage_NotContains_ShouldReturnMatchingAssetClasses()
        {
            // Arrange
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.AssetClasses);

            const string excludeExtension = ".png";
            var filter = AssetClassExposers.BackImage.NotContains(excludeExtension);

            // Act
            var results = Helper.AssetManagement.AssetClasses.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().NotBeEmpty("should find asset classes without .png back images");
                results.Should().OnlyContain(ac => 
                    string.IsNullOrEmpty(ac.BackImage) || 
                    !ac.BackImage.Contains(excludeExtension));
            }
        }

        #endregion

        #region Numeric Range Filters

        [TestMethod]
        public void ReadFilter_MaximumPowerConsumption_GreaterThanOrEqual_ShouldReturnHighPowerDevices()
        {
            // Arrange
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.AssetClasses);

            const double powerThreshold = 200.0;
            var filter = AssetClassExposers.MaximumPowerConsumption.GreaterThanOrEqual(powerThreshold);

            // Act
            var results = Helper.AssetManagement.AssetClasses.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().NotBeEmpty($"should find devices with power consumption >= {powerThreshold}W");
                results.Should().OnlyContain(ac => ac.MaximumPowerConsumption >= powerThreshold);
            }
        }

        [TestMethod]
        public void ReadFilter_TypicalPowerConsumption_LessThanOrEqual_ShouldReturnLowPowerDevices()
        {
            // Arrange
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.AssetClasses);

            const double powerThreshold = 100.0;
            var filter = AssetClassExposers.TypicalPowerConsumption.LessThanOrEqual(powerThreshold);

            // Act
            var results = Helper.AssetManagement.AssetClasses.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().NotBeEmpty($"should find devices with typical power <= {powerThreshold}W");
                results.Should().OnlyContain(ac => ac.TypicalPowerConsumption <= powerThreshold);
            }
        }

        #endregion

        #region Complex Multi-Field Filters

        [TestMethod]
        public void ReadFilter_DimensionsWithinLimits_ShouldReturnDevicesThatFitInRack()
        {
            // Arrange
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.AssetClasses);

            // Simulating a small rack with size constraints
            const double maxHeight = 2 * 4.45; // 2U height in cm
            const double maxWidth = 40.0;      // 40 cm
            const double maxDepth = 40.0;      // 40 cm

            var filter = AssetClassExposers.Height.LessThanOrEqual(maxHeight)
                .AND(AssetClassExposers.Width.LessThanOrEqual(maxWidth))
                .AND(AssetClassExposers.Depth.LessThanOrEqual(maxDepth));

            // Act
            var results = Helper.AssetManagement.AssetClasses.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().NotBeEmpty("should find devices that fit in small rack");
                results.Should().OnlyContain(ac => 
                    ac.Height <= maxHeight && 
                    ac.Width <= maxWidth && 
                    ac.Depth <= maxDepth,
                    $"all results should fit within H:{maxHeight} W:{maxWidth} D:{maxDepth}");
            }
        }

        #endregion

        #region Nested Collection Filters

        [TestMethod]
        public void ReadFilter_DataPortNumber_Equal_ShouldReturnAssetClassesWithSpecificPort()
        {
            // Arrange
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.AssetClasses);

            const int targetPortNumber = 4;
            var filter = AssetClassExposers.DataPorts.PortNumber.Equal(targetPortNumber);

            // Act
            var results = Helper.AssetManagement.AssetClasses.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().NotBeEmpty($"should find asset classes with data port #{targetPortNumber}");
                results.Should().OnlyContain(ac => ac.DataPorts.Any(port => port.PortNumber == targetPortNumber));
            }
        }

        [TestMethod]
        public void ReadFilter_HolderSlotNumber_Equal_ShouldReturnAssetClassesWithSpecificSlot()
        {
            // Arrange
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.AssetClasses);

            const int targetSlotNumber = 6;
            var acWithSlotNumber6 = Helper.TestData.AssetClasses.Where(ac => ac.Holders.Any(holder => holder.SlotNumber == targetSlotNumber)).Count();

            var filter = AssetClassExposers.Holders.SlotNumber.Equal(targetSlotNumber);

            // Act
            var results = Helper.AssetManagement.AssetClasses.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().HaveCount(acWithSlotNumber6, $"should find {acWithSlotNumber6} asset class(es) with holder slot #{targetSlotNumber}");
                results.Should().OnlyContain(ac => ac.Holders.Any(holder => holder.SlotNumber == targetSlotNumber));
            }
        }

        #endregion
    }
}