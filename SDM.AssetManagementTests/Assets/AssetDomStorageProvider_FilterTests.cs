namespace SDM.AssetManagement.Tests.Assets
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

    /// <summary>
    /// Filter and query tests for Asset repository operations.
    /// </summary>
    [TestClass]
    public class AssetDomStorageProvider_FilterTests : BaseRepositoryTest
    {
        #region Basic Field Filters

        [TestMethod]
        public void ReadFilter_AssetName_Equal_ShouldReturnMatchingAsset()
        {
            // Arrange
            
           Helper.PopulateWithDemoData(upTo: DemoDataLayer.Assets);

            var targetAsset =Helper.TestData.Assets.First();
            var filter = AssetExposers.AssetName.Equal(targetAsset.Name);

            // Act
            var results =Helper.AssetManagement.Assets.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().HaveCount(1, $"should find exactly one asset named '{targetAsset.Name}'");
                var asset = results.First();
                asset.Name.Should().Be(targetAsset.Name);
                asset.AssetID.Should().Be(targetAsset.AssetID);
            }
        }

        [TestMethod]
        public void ReadFilter_AssetDescription_Equal_ShouldReturnMatchingAsset()
        {
            // Arrange
            
           Helper.PopulateWithDemoData(upTo: DemoDataLayer.Assets);

            var targetAsset =Helper.TestData.Assets.Skip(3).First();
            var filter = AssetExposers.AssetDescription.Equal(targetAsset.Description);

            // Act
            var results =Helper.AssetManagement.Assets.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().HaveCount(1, $"should find asset with description '{targetAsset.Description}'");
                var asset = results.First();
                asset.Name.Should().Be(targetAsset.Name);
                asset.AssetID.Should().Be(targetAsset.AssetID);
            }
        }

        [TestMethod]
        public void ReadFilter_AssetClass_Equal_ShouldReturnAssetsOfSameClass()
        {
            // Arrange
            
           Helper.PopulateWithDemoData(upTo: DemoDataLayer.Assets);

            var targetAssetClass =Helper.TestData.AssetClasses.First();
            var filter = AssetExposers.AssetClass.Equal(new SdmObjectReference<AssetClass>(targetAssetClass.Identifier));

            // Act
            var results =Helper.AssetManagement.Assets.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().NotBeEmpty($"should find assets with AssetClass '{targetAssetClass.Name}'");
                results.Should().OnlyContain(a => a.AssetClassId.Identifier == targetAssetClass.Identifier);
            }
        }

        [TestMethod]
        public void ReadFilter_FwOs_Equal_ShouldReturnMatchingAsset()
        {
            // Arrange
            
           Helper.PopulateWithDemoData(upTo: DemoDataLayer.Assets);

            var targetAsset =Helper.TestData.Assets.First(a => !string.IsNullOrEmpty(a.FW_OS));
            var filter = AssetExposers.FwOs.Equal(targetAsset.FW_OS);

            // Act
            var results =Helper.AssetManagement.Assets.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().NotBeEmpty($"should find assets with FW_OS '{targetAsset.FW_OS}'");
                results.Should().OnlyContain(a => a.FW_OS == targetAsset.FW_OS);
            }
        }

        [TestMethod]
        public void ReadFilter_SerialNumber_Equal_ShouldReturnMatchingAsset()
        {
            // Arrange
            
           Helper.PopulateWithDemoData(upTo: DemoDataLayer.Assets);

            var targetAsset =Helper.TestData.Assets.First(a => !string.IsNullOrEmpty(a.SerialNumber));
            var filter = AssetExposers.SerialNumber.Equal(targetAsset.SerialNumber);

            // Act
            var results =Helper.AssetManagement.Assets.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().HaveCount(1, $"serial numbers should be unique");
                var asset = results.First();
                asset.SerialNumber.Should().Be(targetAsset.SerialNumber);
                asset.AssetID.Should().Be(targetAsset.AssetID);
            }
        }

        [TestMethod]
        public void ReadFilter_HardwareVersion_Equal_ShouldReturnMatchingAssets()
        {
            // Arrange
            
           Helper.PopulateWithDemoData(upTo: DemoDataLayer.Assets);

            var targetAsset =Helper.TestData.Assets.First(a => !string.IsNullOrEmpty(a.HardwareVersion));
            var filter = AssetExposers.HardwareVersion.Equal(targetAsset.HardwareVersion);

            // Act
            var results =Helper.AssetManagement.Assets.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().NotBeEmpty($"should find assets with hardware version '{targetAsset.HardwareVersion}'");
                results.Should().OnlyContain(a => a.HardwareVersion == targetAsset.HardwareVersion);
            }
        }

        #endregion

        #region Nested Object Filters

        [TestMethod]
        public void ReadFilter_MACAddress_Equal_ShouldReturnMatchingAsset()
        {
            // Arrange
            
           Helper.PopulateWithDemoData(upTo: DemoDataLayer.Assets);

            var targetAsset =Helper.TestData.Assets.First(a => !string.IsNullOrEmpty(a.MacAddress));
            var filter = AssetExposers.NetworkDetails.MACAddress.Equal(targetAsset.MacAddress);

            // Act
            var results =Helper.AssetManagement.Assets.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().HaveCount(1, "MAC addresses should be unique");
                var asset = results.First();
                asset.MacAddress.Should().Be(targetAsset.MacAddress);
                asset.AssetID.Should().Be(targetAsset.AssetID);
            }
        }

        [TestMethod]
        [Ignore("TODO SDM-1234: RackPosition is a non-nullable long with default value 0, causing incorrect filter behavior for assets without rack locations. Skip until nullable types are implemented.")]
        public void ReadFilter_RackPosition_NotEqual_ShouldReturnNonMatchingAssets()
        {
            // Arrange
            
           Helper.PopulateWithDemoData(upTo: DemoDataLayer.Assets);

            const int excludedPosition = 7;
            var filter = AssetExposers.Location.RackPosition.UncheckedNotEqual((long?)excludedPosition);

            // Act
            var results = Helper.AssetManagement.Assets.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().NotBeEmpty("should find assets not at position 7");
                results.Should().OnlyContain(a => a.Location == null || a.Location.RackPosition != excludedPosition);
            }
        }

        #endregion

        #region Date Range Filters

        [TestMethod]
        public void ReadFilter_FirstUseDate_LessThanOrEqual_ShouldReturnOlderAssets()
        {
            // Arrange
            
           Helper.PopulateWithDemoData(upTo: DemoDataLayer.Assets);

            var cutoffDate = DateTime.UtcNow.AddYears(-3);
            var filter = AssetExposers.Lifecycle.FirstUseDate.UncheckedLessThanOrEqual((DateTime?)cutoffDate);

            // Act
            var results =Helper.AssetManagement.Assets.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().NotBeEmpty($"should find assets first used before {cutoffDate:yyyy-MM-dd}");
                results.Should().OnlyContain(a => a.FirstUseDate <= cutoffDate);
            }
        }

        [TestMethod]
        public void ReadFilter_EndOfWarrantyDate_LessThan_ShouldReturnAssetsWithWarrantyExpiringSoon()
        {
            // Arrange
            
           Helper.PopulateWithDemoData(upTo: DemoDataLayer.Assets);

            var cutoffDate = DateTime.UtcNow.AddYears(5);
            var filter = AssetExposers.Lifecycle.EndOfWarrantyDate.UncheckedLessThan((DateTime?)cutoffDate);

            // Act
            var results =Helper.AssetManagement.Assets.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().NotBeEmpty($"should find assets with warranty expiring before {cutoffDate:yyyy-MM-dd}");
                results.Should().OnlyContain(a => a.EndOfWarrantyDate < cutoffDate);
            }
        }

        [TestMethod]
        public void ReadFilter_InstallationDate_Between_ShouldReturnAssetsInstalledInRange()
        {
            // Arrange
            
           Helper.PopulateWithDemoData(upTo: DemoDataLayer.Assets);

            var startDate = DateTime.UtcNow.AddYears(-6);
            var endDate = DateTime.UtcNow.AddYears(-3);

            var filter = AssetExposers.Lifecycle.InstallationDate.UncheckedGreaterThan((DateTime?)startDate)
                .AND(AssetExposers.Lifecycle.InstallationDate.UncheckedLessThan((DateTime?)endDate));

            // Act
            var results =Helper.AssetManagement.Assets.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().NotBeEmpty($"should find assets installed between {startDate:yyyy-MM-dd} and {endDate:yyyy-MM-dd}");
                results.Should().OnlyContain(a => 
                    a.InstallationDate > startDate && 
                    a.InstallationDate < endDate);
            }
        }

        #endregion

        #region Collection Filters

        [TestMethod]
        public void ReadFilter_ElementID_Equal_ShouldReturnAssetsWithSpecificElement()
        {
            // Arrange
            
           Helper.PopulateWithDemoData(upTo: DemoDataLayer.Assets);

            var targetAsset =Helper.TestData.Assets.First(a => a.ElementLinks.Any());
            var targetElementId = targetAsset.ElementLinks.First().ElementID;

            var filter = AssetExposers.ElementLinks.ElementID.Equal(targetElementId);

            // Act
            var results =Helper.AssetManagement.Assets.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().NotBeEmpty($"should find assets with element ID '{targetElementId}'");
                results.Should().OnlyContain(a => a.ElementLinks.Any(el => el.ElementID == targetElementId));
            }
        }

        #endregion
    }
}