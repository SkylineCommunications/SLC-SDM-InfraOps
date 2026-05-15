namespace SDM.AssetManagement.Tests
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
    /// Filter and query tests for DataPort repository operations.
    /// </summary>
    [TestClass]
    public class DataPortDomStorageProvider_FilterTests: BaseRepositoryTest
    {
        #region Basic Field Filters

        [TestMethod]
        public void ReadFilter_PortName_Equal_ShouldReturnMatchingDataPort()
        {
            // Arrange
             Helper.PopulateWithDemoData(upTo: DemoDataLayer.DataPorts);

            var targetDataPort = Helper.TestData.DataPorts.Skip(3).First();
            var filter = DataPortExposers.DataPortInfo.Name.Equal(targetDataPort.DataPortInfo.Name);

            // Act
            var results = Helper.AssetManagement.DataPorts.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().HaveCount(1, $"should find data port with name '{targetDataPort.DataPortInfo.Name}'");
                var dataPort = results.First();
                dataPort.DataPortInfo.Name.Should().Be(targetDataPort.DataPortInfo.Name);
                dataPort.Identifier.Should().Be(targetDataPort.Identifier);
            }
        }

        [TestMethod]
        public void ReadFilter_PortNumber_Equal_ShouldReturnMatchingDataPort()
        {
            // Arrange
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.DataPorts);

            // Test with port number 0 to verify default value handling
            const long portNumber = 0;
            var filter = DataPortExposers.DataPortInfo.PortNumber.Equal(portNumber);

            // Act
            var results = Helper.AssetManagement.DataPorts.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().NotBeEmpty($"should find data ports with port number {portNumber}");
                results.Should().OnlyContain(dp => dp.DataPortInfo.PortNumber == portNumber);
            }
        }

        [TestMethod]
        public void ReadFilter_Label_Equal_ShouldReturnMatchingDataPort()
        {
            // Arrange
            
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.DataPorts);

            var targetDataPort = Helper.TestData.DataPorts.Skip(5).First();
            var label = targetDataPort.DataPortInfo.Label;
            var filter = DataPortExposers.DataPortInfo.Label.Equal(label);

            // Act
            var results = Helper.AssetManagement.DataPorts.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().HaveCount(1, $"should find data port with label '{label}'");
                results.First().DataPortInfo.Label.Should().Be(label);
            }
        }

        [TestMethod]
        public void ReadFilter_PortExposure_Equal_ShouldReturnMatchingDataPorts()
        {
            // Arrange
            
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.DataPorts);

            var portExposure = SlcAsset_Management.Enums.PortExposureEnum.Back;
            var filter = DataPortExposers.DataPortInfo.PortExposure.UncheckedEqual(portExposure);

            // Act
            var results = Helper.AssetManagement.DataPorts.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().NotBeEmpty($"should find data ports with exposure '{portExposure}'");
                results.Should().OnlyContain(dp => dp.DataPortInfo.PortExposure == portExposure);
            }
        }

        #endregion

        #region Nested Object Filters - AddressInfo

        [TestMethod]
        public void ReadFilter_Hostname_Equal_ShouldReturnMatchingDataPort()
        {
            // Arrange
            
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.DataPorts);

            var targetHostname = "device4.example.com";
            var filter = DataPortExposers.AddressInfo.Hostname.Equal(targetHostname);

            // Act
            var results = Helper.AssetManagement.DataPorts.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().HaveCount(1, $"should find data port with hostname '{targetHostname}'");
                results.First().AddressInfo.Hostname.Should().Be(targetHostname);
            }
        }

        [TestMethod]
        public void ReadFilter_Ipv4Address_Contains_ShouldReturnMatchingDataPort()
        {
            // Arrange
            
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.DataPorts);

            var ipv4Pattern = "1.9";
            var filter = DataPortExposers.AddressInfo.Ipv4Address.Contains(ipv4Pattern);

            // Act
            var results = Helper.AssetManagement.DataPorts.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().NotBeEmpty($"should find data ports with IPv4 containing '{ipv4Pattern}'");
                results.Should().OnlyContain(dp => dp.AddressInfo.Ipv4Address.Contains(ipv4Pattern));
            }
        }

        #endregion

        #region Nested Object Filters - PrimaryPortRelation

        [TestMethod]
        public void ReadFilter_IsPrimaryIpv4_True_ShouldReturnPrimaryPorts()
        {
            // Arrange
            
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.DataPorts);

            var filter = DataPortExposers.PrimaryPortRelation.IsPrimaryIpv4.Equal(true);

            // Act
            var results = Helper.AssetManagement.DataPorts.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().NotBeEmpty("should find data ports marked as primary IPv4");
                results.Should().OnlyContain(dp => dp.PrimaryPortRelation.IsPrimaryIpv4);
            }
        }

        #endregion

        #region Relationship Filters

        [TestMethod]
        public void ReadFilter_LinkedAsset_Equal_ShouldReturnDataPortsForAsset()
        {
            // Arrange
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.DataPorts);

            // Get an asset that has data ports in the cached test data
            var targetAsset = Helper.TestData.Assets
                .First(a => Helper.TestData.DataPorts.Count(dp => dp.Asset.Identifier == a.Identifier) > 0);

            var filter = DataPortExposers.Asset.Equal(new SdmObjectReference<Asset>(targetAsset.Identifier));

            // Act
            var results = Helper.AssetManagement.DataPorts.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().NotBeEmpty($"should find data ports linked to asset '{targetAsset.Name}'");
                results.Should().OnlyContain(dp => dp.Asset.Identifier == targetAsset.Identifier);
            }
        }

        #endregion
    }
}
