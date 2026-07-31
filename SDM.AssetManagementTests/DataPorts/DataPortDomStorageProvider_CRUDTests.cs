namespace SDM.AssetManagement.Tests.DataPorts
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
    using Skyline.DataMiner.SDM.AssetManagement.Models;

    /// <summary>
    /// CRUD tests for DataPort repository operations.
    /// </summary>
    [TestClass]
    public class DataPortDomStorageProvider_CRUDTests : BaseRepositoryTest
    {
        private DataPort referenceDataPort = null!;
        private SdmObjectReference<PortType> updateDataPortTypeRef = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            // Seed the asset dependency chain (DeviceTypes -> AssetClasses -> Assets).
            // This stays below the DataPorts layer, so no data ports are created here.
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.Assets);
            var asset = Helper.TestData.Assets.First();

            // Seed a real data port type so the wired validation middleware can resolve it.
            var dataPortType = new PortType
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = "Test Data Port Type",
                CategoryLinks = new CategoryRelation
                {
                    Categories = [SlcAsset_Management.Enums.CategoriesEnum.Data],
                },
            };
            Helper.AssetManagement.PortTypes.Create(dataPortType);

            // Seed a second data port type used by the update test to change the port's type.
            var updateDataPortType = new PortType
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = "Test Data Port Type (Update)",
                CategoryLinks = new CategoryRelation
                {
                    Categories = [SlcAsset_Management.Enums.CategoriesEnum.Data],
                },
            };
            Helper.AssetManagement.PortTypes.Create(updateDataPortType);
            updateDataPortTypeRef = new SdmObjectReference<PortType>(updateDataPortType.Identifier);

            referenceDataPort = new DataPort
            {
                Identifier = Guid.NewGuid().ToString(),
                DataPortInfo = new DataPortInfo
                {
                    Name = "Test DataPort",
                    PortNumber = 1,
                    OutputType = SlcAsset_Management.Enums.Outputtype.IO,
                    PortExposure = SlcAsset_Management.Enums.PortExposureEnum.Front,
                    Type = new SdmObjectReference<PortType>(dataPortType.Identifier),
                    Label = "Ethernet Port 1",
                },
                Asset = new SdmObjectReference<Asset>(asset.Identifier),
                AddressInfo = new AddressInfo
                {
                    Ipv4Address = "192.168.1.100",
                    Ipv6Address = "2001:0db8:85a3:0000:0000:8a2e:0370:7334",
                    Hostname = "test-hostname",
                    DNS = true,
                },
                PrimaryPortRelation = new PrimaryPortRelation
                {
                    IsPrimaryIpv4 = true,
                    IsPrimaryIpv6 = false,
                },
            };
        }

        #region Create Tests

        [TestMethod]
        public void Create_WithValidData_ShouldPersistDataPort()
        {
            
            // Act
            Helper.AssetManagement.DataPorts.Create(referenceDataPort);

            // Assert
            AssertCreated();
        }

        [TestMethod]
        public void Create_WithoutOutputType_ShouldThrowValidationException()
        {
            // Arrange - OutputType is a mandatory field, so creating a DataPort without it
            // must fail validation before it is persisted.
            referenceDataPort.DataPortInfo.OutputType = null;

            // Act
            var act = () => Helper.AssetManagement.DataPorts.Create(referenceDataPort);

            // Assert
            using (new AssertionScope())
            {
                act.Should().Throw<Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Exceptions.ValidationException>()
                    .WithMessage("*Output Type*");

                Helper.AssetManagement.DataPorts.Count(new TRUEFilterElement<DataPort>()).Should().Be(0);
            }
        }

        [TestMethod]
        public void CreateOrUpdate_WithNewDataPort_ShouldCreate()
        {
            
            // Act
            Helper.AssetManagement.DataPorts.CreateOrUpdate([referenceDataPort]);

            // Assert
            AssertCreated();
        }

        [TestMethod]
        public void CreateBulk_WithNonExistentPortType_ShouldThrowBulkValidationException()
        {
            // Arrange - the bulk path now validates Port Type (existence + category),
            // so a batch referencing a non-existent Port Type must be rejected.
            referenceDataPort.DataPortInfo.Type = new SdmObjectReference<PortType>(Guid.NewGuid().ToString());

            // Act - bulk Create routes through the middleware's bulk validation.
            var act = () => Helper.AssetManagement.DataPorts.Create(new[] { referenceDataPort });

            // Assert
            using (new AssertionScope())
            {
                act.Should().Throw<Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations.BulkValidationException<DataPort>>()
                    .WithMessage("*Port Type*");

                Helper.AssetManagement.DataPorts.Count(new TRUEFilterElement<DataPort>()).Should().Be(0);
            }
        }

        [TestMethod]
        public void CreateOrUpdate_WithNonExistentPortType_ShouldThrowBulkValidationException()
        {
            // Arrange - CreateOrUpdate now routes through the middleware's bulk validation
            // (previously it bypassed validation entirely), so a non-existent Port Type must be rejected.
            referenceDataPort.DataPortInfo.Type = new SdmObjectReference<PortType>(Guid.NewGuid().ToString());

            // Act
            var act = () => Helper.AssetManagement.DataPorts.CreateOrUpdate(new[] { referenceDataPort });

            // Assert
            using (new AssertionScope())
            {
                act.Should().Throw<Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations.BulkValidationException<DataPort>>()
                    .WithMessage("*Port Type*");

                Helper.AssetManagement.DataPorts.Count(new TRUEFilterElement<DataPort>()).Should().Be(0);
            }
        }


        [TestMethod]
        public void CreateOrUpdate_WithExistingDataPort_ShouldUpdate()
        {
            // Arrange
            var created = Helper.AssetManagement.DataPorts.Create(referenceDataPort);

            var updatedDataPort = new DataPort
            {
                Identifier = created.Identifier,
                DataPortInfo = new DataPortInfo
                {
                    Name = "Updated DataPort Name",
                    PortNumber = 2,
                    OutputType = SlcAsset_Management.Enums.Outputtype.Out,
                    PortExposure = SlcAsset_Management.Enums.PortExposureEnum.Back,
                    Type = updateDataPortTypeRef,
                    Label = "Fiber Port 2",
                },
                Asset = created.Asset ,
                AddressInfo = new AddressInfo
                {
                    Ipv4Address = "10.0.0.50",
                    Ipv6Address = "2001:0db8:85a3:0000:0000:8a2e:0370:1234",
                    Hostname = "updated-hostname",
                    DNS = false,
                },
                PrimaryPortRelation = new PrimaryPortRelation
                {
                    IsPrimaryIpv4 = false,
                    IsPrimaryIpv6 = true,
                },
            };

            // Act
            Helper.AssetManagement.DataPorts.CreateOrUpdate([updatedDataPort]);

            // Assert
            var persisted = Helper.AssetManagement.DataPorts.Read(new TRUEFilterElement<DataPort>()).First();
            AssertDataPortUpdateDifferences(created, persisted);
        }

        #endregion

        #region Read Tests

        [TestMethod]
        public void ReadPaged_WithValidFilter_ShouldReturnPages()
        {
            // Arrange
            const int pageSize = 2;
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.DataPorts);

            var allFilter = new TRUEFilterElement<DataPort>();
            var totalCount = Helper.TestData.DataPorts.Count;

            // Act
            var pagedResult = Helper.AssetManagement.DataPorts.ReadPaged(allFilter, pageSize);

            // Assert
            using (new AssertionScope())
            {
                pagedResult.Should().NotBeNull();
                pagedResult.Should().HaveCount((int)(totalCount / pageSize), "should have correct number of pages");
                pagedResult.Should().AllSatisfy(page => page.Should().HaveCount(pageSize), "each page should have correct size");
            }
        }

        #endregion

        #region Delete Tests

        [TestMethod]
        public void Delete_Single_ShouldRemoveDataPort()
        {
            // Arrange
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.DataPorts);

            var initialCount = Helper.TestData.DataPorts.Count;
            var dataPortToDelete = Helper.AssetManagement.DataPorts
                .Read(DataPortExposers.DataPortInfo.Name.Equal("Data Port 3"))
                .First();

            // Act
            Helper.AssetManagement.DataPorts.Delete(dataPortToDelete);

            // Assert
            using (new AssertionScope())
            {
                Helper.AssetManagement.DataPorts.Count(new TRUEFilterElement<DataPort>())
                    .Should().Be(initialCount - 1, "one data port should be deleted");

                Helper.AssetManagement.DataPorts.Count(DataPortExposers.Identifier.Equal(dataPortToDelete.Identifier))
                    .Should().Be(0, "deleted data port should not exist");
            }
        }

        [TestMethod]
        public void Delete_Bulk_ShouldRemoveMultipleDataPorts()
        {
            // Arrange
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.DataPorts);

            var initialCount = Helper.TestData.DataPorts.Count;

            var filter = new ORFilterElement<DataPort>(
                DataPortExposers.DataPortInfo.Name.Equal("Data Port 3"),
                DataPortExposers.DataPortInfo.Label.Equal("Data Port Label 7"));

            var dataPortsToDelete = Helper.AssetManagement.DataPorts.Read(filter).ToList();
            var deleteCount = dataPortsToDelete.Count;

            // Act
            Helper.AssetManagement.DataPorts.Delete(dataPortsToDelete);

            // Assert
            using (new AssertionScope())
            {
                Helper.AssetManagement.DataPorts.Count(new TRUEFilterElement<DataPort>())
                    .Should().Be(initialCount - deleteCount, $"{deleteCount} data ports should be deleted");

                Helper.AssetManagement.DataPorts.Count(DataPortExposers.DataPortInfo.Name.Equal("Data Port 3"))
                    .Should().Be(0, "Data Port 3 should be deleted");

                Helper.AssetManagement.DataPorts.Count(DataPortExposers.DataPortInfo.Label.Equal("Data Port Label 7"))
                    .Should().Be(0, "data port with label 'Data Port Label 7' should be deleted");
            }
        }

        #endregion

        #region Assertion Helpers

        private static void AssertDataPortUpdateDifferences(DataPort original, DataPort updated)
        {
            using (new AssertionScope())
            {
                // Identifiers remain the same
                updated.Identifier.Should().Be(original.Identifier);

                // DataPortInfo changes
                updated.DataPortInfo.Name.Should().Be("Updated DataPort Name");
                updated.DataPortInfo.PortNumber.Should().Be(2);
                updated.DataPortInfo.OutputType.Should().Be(SlcAsset_Management.Enums.Outputtype.Out);
                updated.DataPortInfo.PortExposure.Should().Be(SlcAsset_Management.Enums.PortExposureEnum.Back);
                updated.DataPortInfo.Label.Should().Be("Fiber Port 2");
                updated.DataPortInfo.Type.Should().NotBe(original.DataPortInfo.Type);

                // Asset reference remains the same
                updated.Asset.Should().Be(original.Asset);

                // AddressInfo changes
                updated.AddressInfo.Should().NotBeNull();
                updated.AddressInfo.Ipv4Address.Should().Be("10.0.0.50");
                updated.AddressInfo.Ipv6Address.Should().Be("2001:0db8:85a3:0000:0000:8a2e:0370:1234");
                updated.AddressInfo.Hostname.Should().Be("updated-hostname");
                updated.AddressInfo.DNS.Should().BeFalse();

                // PrimaryPortRelation changes
                updated.PrimaryPortRelation.Should().NotBeNull();
                updated.PrimaryPortRelation.IsPrimaryIpv4.Should().BeFalse();
                updated.PrimaryPortRelation.IsPrimaryIpv6.Should().BeTrue();
            }
        }

        private void AssertCreated()
        {
            using (new AssertionScope())
            {
                Helper.AssetManagement.DataPorts.Count(new TRUEFilterElement<DataPort>()).Should().Be(1);

                var created = Helper.AssetManagement.DataPorts.Read(new TRUEFilterElement<DataPort>()).First();

                // Basic properties
                created.Should().NotBeNull();
                created.DataPortInfo.Name.Should().Be("Test DataPort");
                created.DataPortInfo.PortNumber.Should().Be(1);
                created.DataPortInfo.OutputType.Should().Be(SlcAsset_Management.Enums.Outputtype.IO);
                created.DataPortInfo.PortExposure.Should().Be(SlcAsset_Management.Enums.PortExposureEnum.Front);
                created.DataPortInfo.Label.Should().Be("Ethernet Port 1");

                // Asset reference
                created.Asset.Should().NotBeNull();
                created.Asset.Should().BeAssignableTo<SdmObjectReference<Asset>>();

                // AddressInfo
                created.AddressInfo.Should().NotBeNull();
                created.AddressInfo.Ipv4Address.Should().Be("192.168.1.100");
                created.AddressInfo.Ipv6Address.Should().Be("2001:0db8:85a3:0000:0000:8a2e:0370:7334");
                created.AddressInfo.Hostname.Should().Be("test-hostname");
                created.AddressInfo.DNS.Should().BeTrue();

                // PrimaryPortRelation
                created.PrimaryPortRelation.Should().NotBeNull();
                created.PrimaryPortRelation.IsPrimaryIpv4.Should().BeTrue();
                created.PrimaryPortRelation.IsPrimaryIpv6.Should().BeFalse();
            }
        }

        #endregion
    }
}
