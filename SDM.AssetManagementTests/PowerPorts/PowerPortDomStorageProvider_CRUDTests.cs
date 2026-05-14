namespace SDM.AssetManagement.Tests
{
    using System;
    using System.Linq;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SDM.AssetManagement.Tests.Setup;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Helpers;
    using Skyline.DataMiner.SDM.AssetManagement.Models;

    /// <summary>
    /// CRUD tests for PowerPort repository operations.
    /// </summary>
    [TestClass]
    public class PowerPortDomStorageProvider_CRUDTests : BaseRepositoryTest
    {
        private PowerPort referencePowerPort;

        [TestInitialize]
        public void TestInitialize()
        {
            referencePowerPort = new PowerPort
            {
                Identifier = Guid.NewGuid().ToString(),
                PowerPortInfo = new PowerPortInfo
                {
                    Name = "Test PowerPort",
                    PortNumber = 1,
                    OutputType = SlcAsset_Management.Enums.Outputtype.IO,
                    PortExposure = SlcAsset_Management.Enums.PortExposureEnum.Front,
                    Label = "Power Port 1",
                },
                Asset = new SdmObjectReference<Asset>(Guid.NewGuid().ToString()),
            };
        }

        #region Create Tests

        [TestMethod]
        public void Create_WithValidData_ShouldPersistPowerPort()
        {
            // Arrange
           

            // Act
            Helper.AssetManagement.PowerPorts.Create(referencePowerPort);

            // Assert
            AssertCreated(Helper.AssetManagement);
        }

        [TestMethod]
        public void CreateOrUpdate_WithNewPowerPort_ShouldCreate()
        {
            // Arrange
           

            // Act
            Helper.AssetManagement.PowerPorts.CreateOrUpdate([referencePowerPort]);

            // Assert
            AssertCreated(Helper.AssetManagement);
        }

        [TestMethod]
        public void CreateOrUpdate_WithExistingPowerPort_ShouldUpdate()
        {
            // Arrange
           
            Helper.AssetManagement.PowerPorts.Create(referencePowerPort);

            var updatedPowerPort = new PowerPort
            {
                Identifier = referencePowerPort.Identifier,
                PowerPortInfo = new PowerPortInfo
                {
                    Name = "Updated PowerPort Name",
                    PortNumber = 2,
                    OutputType = SlcAsset_Management.Enums.Outputtype.Out,
                    PortExposure = SlcAsset_Management.Enums.PortExposureEnum.Back,
                    Label = "Power Port 2",
                },
                Asset = referencePowerPort.Asset,
            };

            // Act
            Helper.AssetManagement.PowerPorts.CreateOrUpdate([updatedPowerPort]);

            // Assert
            var persisted = Helper.AssetManagement.PowerPorts.Read(new TRUEFilterElement<PowerPort>()).First();
            AssertPowerPortUpdateDifferences(referencePowerPort, persisted);
        }

        #endregion

        #region Read Tests

        [TestMethod]
        public void ReadPaged_WithValidFilter_ShouldReturnPages()
        {
            // Arrange
            const int pageSize = 3;
           
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.PowerPorts);

            var allFilter = new TRUEFilterElement<PowerPort>();
            var totalCount = Helper.TestData.PowerPorts.Count;

            // Act
            var pagedResult = Helper.AssetManagement.PowerPorts.ReadPaged(allFilter, pageSize);

            // Assert
            using (new AssertionScope())
            {
                pagedResult.Should().NotBeNull();
                pagedResult.Should().HaveCountGreaterOrEqualTo((int)(totalCount / pageSize), 
                    "should have at least the expected number of pages");
                pagedResult.Should().AllSatisfy(page => 
                    page.Should().HaveCountLessOrEqualTo(pageSize), 
                    "each page should not exceed page size");
            }
        }

        #endregion

        #region Delete Tests

        [TestMethod]
        public void Delete_Single_ShouldRemovePowerPort()
        {
            // Arrange
           
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.PowerPorts);

            var initialCount = Helper.TestData.PowerPorts.Count;
            var powerPortToDelete = Helper.AssetManagement.PowerPorts
                .Read(PowerPortExposers.PowerPortInfo.Name.Equal("Power Port 3"))
                .First();

            // Act
            Helper.AssetManagement.PowerPorts.Delete(powerPortToDelete);

            // Assert
            using (new AssertionScope())
            {
                Helper.AssetManagement.PowerPorts.Count(new TRUEFilterElement<PowerPort>())
                    .Should().Be(initialCount - 1, "one power port should be deleted");

                Helper.AssetManagement.PowerPorts.Count(PowerPortExposers.Identifier.Equal(powerPortToDelete.Identifier))
                    .Should().Be(0, "deleted power port should not exist");
            }
        }

        [TestMethod]
        public void Delete_Bulk_ShouldRemoveMultiplePowerPorts()
        {
            // Arrange
           
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.PowerPorts);

            var initialCount = Helper.TestData.PowerPorts.Count;

            var filter = new ORFilterElement<PowerPort>(
                PowerPortExposers.PowerPortInfo.Name.Equal("Power Port 3"),
                PowerPortExposers.PowerPortInfo.Label.Equal("Power Port Label 7"));

            var powerPortsToDelete = Helper.AssetManagement.PowerPorts.Read(filter).ToList();
            var deleteCount = powerPortsToDelete.Count;

            // Act
            Helper.AssetManagement.PowerPorts.Delete(powerPortsToDelete);

            // Assert
            using (new AssertionScope())
            {
                Helper.AssetManagement.PowerPorts.Count(new TRUEFilterElement<PowerPort>())
                    .Should().Be(initialCount - deleteCount, $"{deleteCount} power ports should be deleted");

                Helper.AssetManagement.PowerPorts.Count(PowerPortExposers.PowerPortInfo.Name.Equal("Power Port 3"))
                    .Should().Be(0, "Power Port 3 should be deleted");

                Helper.AssetManagement.PowerPorts.Count(PowerPortExposers.PowerPortInfo.Label.Equal("Power Port Label 7"))
                    .Should().Be(0, "power port with label 'Power Port Label 7' should be deleted");
            }
        }

        #endregion

        #region Assertion Helpers

        private static void AssertPowerPortUpdateDifferences(PowerPort original, PowerPort updated)
        {
            using (new AssertionScope())
            {
                // Identifiers remain the same
                updated.Identifier.Should().Be(original.Identifier);

                // PowerPortInfo changes
                updated.PowerPortInfo.Name.Should().Be("Updated PowerPort Name");
                updated.PowerPortInfo.PortNumber.Should().Be(2);
                updated.PowerPortInfo.OutputType.Should().Be(SlcAsset_Management.Enums.Outputtype.Out);
                updated.PowerPortInfo.PortExposure.Should().Be(SlcAsset_Management.Enums.PortExposureEnum.Back);
                updated.PowerPortInfo.Label.Should().Be("Power Port 2");
                updated.PowerPortInfo.PortType.Should().Be(original.PowerPortInfo.PortType);

                // Asset reference remains the same
                updated.Asset.Should().Be(original.Asset);
            }
        }

        private void AssertCreated(IAssetManagementApiHelper helper)
        {
            using (new AssertionScope())
            {
                Helper.AssetManagement.PowerPorts.Count(new TRUEFilterElement<PowerPort>()).Should().Be(1);

                var created = Helper.AssetManagement.PowerPorts.Read(new TRUEFilterElement<PowerPort>()).First();

                // Basic properties
                created.Should().NotBeNull();
                created.PowerPortInfo.Name.Should().Be(referencePowerPort.PowerPortInfo.Name);
                created.PowerPortInfo.PortNumber.Should().Be(referencePowerPort.PowerPortInfo.PortNumber);
                created.PowerPortInfo.OutputType.Should().Be(referencePowerPort.PowerPortInfo.OutputType);
                created.PowerPortInfo.PortExposure.Should().Be(referencePowerPort.PowerPortInfo.PortExposure);
                created.PowerPortInfo.Label.Should().Be(referencePowerPort.PowerPortInfo.Label);

                // Asset reference
                created.Asset.Should().NotBeNull();
                created.Asset.Should().BeAssignableTo<SdmObjectReference<Asset>>();
            }
        }

        #endregion
    }
}