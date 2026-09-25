namespace SDM.AssetManagement.Tests.Connections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SDM.AssetManagement.Tests.Setup;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;

    [TestClass]
    public class ConnectionDomStorageProvider_CRUDTests : BaseRepositoryTest
    {
        private DataPort sourcePort = null!;
        private DataPort destinationPort = null!;
        private CableType cableType = null!;

        [TestInitialize]
        public void PrepareConnectionReferences()
        {
            sourcePort = CreateDataEndpoint("Source", SlcAsset_Management.Enums.Outputtype.Out);
            destinationPort = CreateDataEndpoint("Destination", SlcAsset_Management.Enums.Outputtype.In);
            cableType = Helper.AssetManagement.CableTypes.Create(new CableType
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = "Test Cable Type",
                Description = "Test cable type",
                CategoryLinks = new CategoryRelation
                {
                    Categories = new List<SlcAsset_Management.Enums.CategoriesEnum>
                    {
                        SlcAsset_Management.Enums.CategoriesEnum.Data,
                    },
                },
            });
        }

        [TestMethod]
        public void Create_WithValidConnection_ShouldPersistAndReadBack()
        {
            var connection = CreateConnection();

            Helper.AssetManagement.Connections.Create(connection);
            var persisted = Helper.AssetManagement.Connections
                .Read(new TRUEFilterElement<Connection>())
                .Single();

            using (new AssertionScope())
            {
                persisted.ConnectionType.Should().Be(connection.ConnectionType);
                persisted.CableLength.Should().Be(connection.CableLength);
                persisted.Notes.Should().Be(connection.Notes);
                persisted.Description.Should().Be(connection.Description);
                persisted.Source.Port.Should().Be(connection.Source.Port);
                persisted.Destination.Port.Should().Be(connection.Destination.Port);
                persisted.Source.PortType.Identifier.Should().Be(connection.Source.PortType.Identifier);
                persisted.Destination.PortType.Identifier.Should().Be(connection.Destination.PortType.Identifier);
                persisted.Source.CableTag.Should().Be(connection.Source.CableTag);
                persisted.Destination.CableTag.Should().Be(connection.Destination.CableTag);
                persisted.CableType.Identifier.Should().Be(connection.CableType.Identifier);
            }
        }

        [TestMethod]
        public void CreateOrUpdate_WithNewConnection_ShouldCreate()
        {
            var connection = CreateConnection();

            Helper.AssetManagement.Connections.CreateOrUpdate([connection]);

            Helper.AssetManagement.Connections.Count(new TRUEFilterElement<Connection>())
                .Should().Be(1);
        }

        [TestMethod]
        public void CreateOrUpdate_WithExistingConnection_ShouldUpdate()
        {
            var connection = CreateConnection();
            var created = Helper.AssetManagement.Connections.Create(connection);
            var updated = CreateConnection(created.Identifier);
            updated.CableLength = 42.5;
            updated.Notes = "Updated notes";

            Helper.AssetManagement.Connections.CreateOrUpdate([updated]);
            var persisted = Helper.AssetManagement.Connections
                .Read(new TRUEFilterElement<Connection>())
                .Single();

            using (new AssertionScope())
            {
                persisted.Identifier.Should().Be(created.Identifier);
                persisted.CableLength.Should().Be(42.5);
                persisted.Notes.Should().Be("Updated notes");
            }
        }

        [TestMethod]
        public void Read_WithSourcePortFilter_ShouldReturnMatchingConnection()
        {
            var target = CreateConnection();
            Helper.AssetManagement.Connections.Create(target);
            sourcePort = CreateDataEndpoint("Other Source", SlcAsset_Management.Enums.Outputtype.Out);
            destinationPort = CreateDataEndpoint("Other Destination", SlcAsset_Management.Enums.Outputtype.In);
            Helper.AssetManagement.Connections.Create(CreateConnection());

            var results = Helper.AssetManagement.Connections
                .Read(ConnectionExposers.Source.Port.Equal(target.Source.Port));

            results.Should().ContainSingle()
                .Which.Identifier.Should().Be(target.Identifier);
        }

        [TestMethod]
        public void Read_WithCableTypeFilter_ShouldReturnMatchingConnection()
        {
            var target = CreateConnection();
            Helper.AssetManagement.Connections.Create(target);
            sourcePort = CreateDataEndpoint("Other Source", SlcAsset_Management.Enums.Outputtype.Out);
            destinationPort = CreateDataEndpoint("Other Destination", SlcAsset_Management.Enums.Outputtype.In);
            var other = CreateConnection();
            other.CableType = new SdmObjectReference<CableType>(
                Helper.AssetManagement.CableTypes.Create(new CableType
                {
                    Identifier = Guid.NewGuid().ToString(),
                    Name = "Other Cable Type",
                    CategoryLinks = new CategoryRelation
                    {
                        Categories = new List<SlcAsset_Management.Enums.CategoriesEnum>
                        {
                            SlcAsset_Management.Enums.CategoriesEnum.Data,
                        },
                    },
                }).Identifier);
            Helper.AssetManagement.Connections.Create(other);

            var results = Helper.AssetManagement.Connections
                .Read(ConnectionExposers.CableType.Equal(target.CableType));

            results.Should().ContainSingle()
                .Which.Identifier.Should().Be(target.Identifier);
        }

        [TestMethod]
        public void ReadPaged_WithConnections_ShouldReturnPages()
        {
            Helper.AssetManagement.Connections.Create([CreateConnection(), CreateConnection()]);

            var pages = Helper.AssetManagement.Connections
                .ReadPaged(new TRUEFilterElement<Connection>(), 1);

            using (new AssertionScope())
            {
                pages.Should().HaveCount(2);
                pages.Should().AllSatisfy(page => page.Should().ContainSingle());
            }
        }

        [TestMethod]
        public void Delete_Single_ShouldRemoveConnection()
        {
            var connection = Helper.AssetManagement.Connections.Create(CreateConnection());
            Helper.AssetManagement.Connections.Delete(connection);

            Helper.AssetManagement.Connections.Count(new TRUEFilterElement<Connection>())
                .Should().Be(0);
        }

        [TestMethod]
        public void Delete_Bulk_ShouldRemoveConnections()
        {
            var connections = new[] { CreateConnection(), CreateConnection() };
            Helper.AssetManagement.Connections.Create(connections);

            Helper.AssetManagement.Connections.Delete(connections);

            Helper.AssetManagement.Connections.Count(new TRUEFilterElement<Connection>())
                .Should().Be(0);
        }

        private Connection CreateConnection(string identifier = null)
        {
            return new Connection
            {
                Identifier = identifier ?? Guid.NewGuid().ToString(),
                ConnectionType = SlcAsset_Management.Enums.ConnectionType.Data,
                CableType = new SdmObjectReference<CableType>(cableType.Identifier),
                CableLength = 12.5,
                Notes = "Connection notes",
                Description = "Connection description",
                Source =
                {
                    Port = sourcePort,
                    PortType = sourcePort.DataPortInfo.PortType,
                    CableTag = "Source cable",
                },
                Destination =
                {
                    Port = destinationPort,
                    PortType = destinationPort.DataPortInfo.PortType,
                    CableTag = "Destination cable",
                },
            };
        }

        private DataPort CreateDataEndpoint(string name, SlcAsset_Management.Enums.Outputtype outputType)
        {
            var deviceType = Helper.AssetManagement.DeviceTypes.Create(new DeviceType
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = $"{name} Device Type",
                HierarchyInfo =
                {
                    HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.None,
                },
                TagsInfo =
                {
                    Tags = new List<SlcAsset_Management.Enums.TagOption>
                    {
                        SlcAsset_Management.Enums.TagOption.AcceptsDataConnection,
                    },
                },
            });

            var assetClass = Helper.AssetManagement.AssetClasses.Create(new AssetClass
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = $"{name} Asset Class",
                State = SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Active,
                DeviceTypeId = new SdmObjectReference<DeviceType>(deviceType.Identifier),
                DataPorts = new List<DataPortInfo>(),
                PowerPorts = new List<PowerPortInfo>(),
                Holders = new List<AssetHolder>(),
            });

            var asset = Helper.AssetManagement.Assets.Create(new Asset
            {
                Identifier = Guid.NewGuid().ToString(),
                AssetID = $"{name}-{Guid.NewGuid()}",
                Name = $"{name} Asset",
                AssetClassId = new SdmObjectReference<AssetClass>(assetClass.Identifier),
                State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available,
            });

            var portType = Helper.AssetManagement.PortTypes.Create(new PortType
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = $"{name} Port Type",
                CategoryLinks =
                {
                    Categories = new List<SlcAsset_Management.Enums.CategoriesEnum>
                    {
                        SlcAsset_Management.Enums.CategoriesEnum.Data,
                    },
                },
                CableFKs =
                {
                    CableTypeFks = new List<SdmObjectReference<CableType>>(),
                },
            });

            return Helper.AssetManagement.DataPorts.Create(new DataPort
            {
                Identifier = Guid.NewGuid().ToString(),
                Asset = new SdmObjectReference<Asset>(asset.Identifier),
                DataPortInfo =
                {
                    Name = $"{name} Port",
                    PortNumber = 1,
                    OutputType = outputType,
                    PortExposure = SlcAsset_Management.Enums.PortExposureEnum.Front,
                    PortType = new SdmObjectReference<PortType>(portType.Identifier),
                },
            });
        }

    }
}
