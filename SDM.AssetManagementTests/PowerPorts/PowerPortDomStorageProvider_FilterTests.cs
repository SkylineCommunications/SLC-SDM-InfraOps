namespace SDM.AssetManagement.Tests.PowerPorts
{
    using System.Linq;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SDM.AssetManagement.Tests.Setup;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;

    /// <summary>
    /// Filter and query tests for PowerPort repository operations.
    /// </summary>
    [TestClass]
    public class PowerPortDomStorageProvider_FilterTests : BaseRepositoryTest
    {
        #region Basic Field Filters

        [TestMethod]
        public void ReadFilter_PortName_Equal_ShouldReturnMatchingPowerPort()
        {
            // Arrange
            
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.PowerPorts);

            var targetPowerPort = Helper.TestData.PowerPorts.Skip(3).First();
            var filter = PowerPortExposers.PowerPortInfo.Name.Equal(targetPowerPort.PowerPortInfo.Name);

            // Act
            var results = Helper.AssetManagement.PowerPorts.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().HaveCount(1, $"should find power port with name '{targetPowerPort.PowerPortInfo.Name}'");
                var powerPort = results.First();
                powerPort.PowerPortInfo.Name.Should().Be(targetPowerPort.PowerPortInfo.Name);
                powerPort.Identifier.Should().Be(targetPowerPort.Identifier);
            }
        }

        [TestMethod]
        public void ReadFilter_PortNumber_Equal_ShouldReturnMatchingPowerPort()
        {
            // Arrange
            
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.PowerPorts);

            // Test with port number 0 to verify default value handling
            const long portNumber = 0;
            var filter = PowerPortExposers.PowerPortInfo.PortNumber.Equal(portNumber);

            // Act
            var results = Helper.AssetManagement.PowerPorts.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().NotBeEmpty($"should find power ports with port number {portNumber}");
                results.Should().OnlyContain(pp => pp.PowerPortInfo.PortNumber == portNumber);
            }
        }

        [TestMethod]
        public void ReadFilter_Label_Contains_ShouldReturnMatchingPowerPorts()
        {
            // Arrange
            
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.PowerPorts);

            const string labelPattern = "Power";
            var filter = PowerPortExposers.PowerPortInfo.Label.Contains(labelPattern);

            // Act
            var results = Helper.AssetManagement.PowerPorts.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().NotBeEmpty($"should find power ports with '{labelPattern}' in label");
                results.Should().OnlyContain(pp => pp.PowerPortInfo.Label.Contains(labelPattern));
            }
        }

        [TestMethod]
        public void ReadFilter_PortExposure_Equal_ShouldReturnMatchingPowerPorts()
        {
            // Arrange
            
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.PowerPorts);

            var portExposure = SlcAsset_Management.Enums.PortExposureEnum.Back;
            var filter = PowerPortExposers.PowerPortInfo.PortExposure.UncheckedEqual(portExposure);

            // Act
            var results = Helper.AssetManagement.PowerPorts.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().NotBeEmpty($"should find power ports with exposure '{portExposure}'");
                results.Should().OnlyContain(pp => pp.PowerPortInfo.PortExposure == portExposure);
            }
        }

        [TestMethod]
        public void ReadFilter_OutputType_Equal_ShouldReturnMatchingPowerPorts()
        {
            // Arrange
            
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.PowerPorts);

            var outputType = SlcAsset_Management.Enums.Outputtype.IO;
            var filter = PowerPortExposers.PowerPortInfo.OutputType.UncheckedEqual(outputType);

            // Act
            var results = Helper.AssetManagement.PowerPorts.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().NotBeEmpty($"should find power ports with output type '{outputType}'");
                results.Should().OnlyContain(pp => pp.PowerPortInfo.OutputType == outputType);
            }
        }

        #endregion

        #region Relationship Filters

        [TestMethod]
        public void ReadFilter_LinkedAsset_Equal_ShouldReturnPowerPortsForAsset()
        {
            // Arrange
            
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.PowerPorts);

            // Get an asset that has multiple power ports
            var targetAsset = Helper.TestData.Assets
                .First(a => Helper.TestData.PowerPorts.Count(pp => pp.Asset.Identifier == a.Identifier) > 0);

            var filter = PowerPortExposers.Asset.Equal(new SdmObjectReference<Asset>(targetAsset.Identifier));

            // Act
            var results = Helper.AssetManagement.PowerPorts.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().NotBeEmpty($"should find power ports linked to asset '{targetAsset.Name}'");
                results.Should().OnlyContain(pp => pp.Asset.Identifier == targetAsset.Identifier);
            }
        }

        #endregion
    }
}
