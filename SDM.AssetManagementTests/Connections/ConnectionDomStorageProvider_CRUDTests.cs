namespace SDM.AssetManagement.Tests.Connections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    using SDM.AssetManagement.Tests.Setup;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Deletion;
    using Skyline.DataMiner.SDM.AssetManagement.Helpers;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.InfraOps.Orchestration;
    using Skyline.DataMiner.SDM.InfraOps.Orchestration.AssetDeletion;
    using Skyline.DataMiner.SDM.PlanAndBuild.Deletion;
    using Skyline.DataMiner.SDM.PlanAndBuild.Models;

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

        [TestMethod]
        public void DeleteConnectedAsset_ShouldCascadeConnectionPortsAndKeepRemovedJobSnapshot()
        {
            var connection = Helper.AssetManagement.Connections.Create(CreateConnection());
            var asset = Helper.AssetManagement.Assets
                .Read(AssetExposers.Identifier.Equal(sourcePort.Asset.Identifier))
                .Single();
            var appSettings = Helper.PlanAndBuild.AppSettings.Create(new PlanAndBuildAppSettings
            {
                JobIDPrefix = "DEL",
                JobIDNextSequence = 1,
                JobIDIncrement = 1,
                JobIDMinimumDigits = 4,
            });
            var jobType = Helper.PlanAndBuild.JobTypes.Create(new JobType
            {
                Name = "Delete connected Asset job type",
            });
            var job = Helper.PlanAndBuild.Jobs.Create(new PlanAndBuildJob
            {
                JobName = "Delete connected Asset job",
                Type = new SdmObjectReference<JobType>(jobType.Identifier),
                AssetsUsed = new List<JobAsset>
                {
                    new JobAsset { AssetId = new SdmObjectReference<Asset>(asset.Identifier) },
                },
                ConnectionsOnJob = new List<JobConnection>
                {
                    new JobConnection
                    {
                        ConnectionId = new SdmObjectReference<Connection>(connection.Identifier),
                        Status = "Connected",
                    },
                },
            });

            asset.State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable;
            Helper.AssetManagement.Assets.Delete(asset);

            Helper.AssetManagement.Connections.Count(new TRUEFilterElement<Connection>())
                .Should().Be(0);
            Helper.AssetManagement.DataPorts.Read(new TRUEFilterElement<DataPort>())
                .Select(port => port.Identifier)
                .Should().NotContain(sourcePort.Identifier)
                .And.Contain(destinationPort.Identifier);

            var updatedJob = Helper.PlanAndBuild.Jobs
                .Read(new TRUEFilterElement<PlanAndBuildJob>())
                .Single(item => item.Identifier == job.Identifier);
            updatedJob.AssetsUsed.Should().BeEmpty();
            updatedJob.ConnectionsOnJob.Should().ContainSingle().Which.Should().BeEquivalentTo(new JobConnection
            {
                ConnectionId = new SdmObjectReference<Connection>(connection.Identifier),
                Source = connection.Source.CableTag,
                Destination = connection.Destination.CableTag,
                Status = "Removed",
                CableType = connection.CableType,
                CableLength = connection.CableLength,
            });

            Helper.AssetManagement.RecoverAssetDeletion(asset.Identifier);

            Helper.PlanAndBuild.Jobs.Read(new TRUEFilterElement<PlanAndBuildJob>())
                .Single(item => item.Identifier == job.Identifier)
                .ConnectionsOnJob.Should().ContainSingle().Which.Should().BeEquivalentTo(updatedJob.ConnectionsOnJob.Single());
        }

        [DataTestMethod]
        [DataRow(JobEntryRemovalMode.Remove, JobEntryRemovalMode.Remove, false)]
        [DataRow(JobEntryRemovalMode.Remove, JobEntryRemovalMode.KeepRemovedSnapshot, false)]
        [DataRow(JobEntryRemovalMode.KeepRemovedSnapshot, JobEntryRemovalMode.Remove, false)]
        [DataRow(JobEntryRemovalMode.KeepRemovedSnapshot, JobEntryRemovalMode.KeepRemovedSnapshot, false)]
        [DataRow(JobEntryRemovalMode.KeepRemovedSnapshot, JobEntryRemovalMode.KeepRemovedSnapshot, true)]
        public void Delete_WithSelectedPolicy_ShouldPreserveJobChoices(
            JobEntryRemovalMode assetMode,
            JobEntryRemovalMode connectionMode,
            bool bulk)
        {
            var connection = Helper.AssetManagement.Connections.Create(CreateConnection());
            var asset = Helper.AssetManagement.Assets.Read(AssetExposers.Identifier.Equal(sourcePort.Asset.Identifier)).Single();
            var otherAsset = Helper.AssetManagement.Assets.Read(AssetExposers.Identifier.Equal(destinationPort.Asset.Identifier)).Single();
            var className = Helper.AssetManagement.AssetClasses.Read(AssetClassExposers.Identifier.Equal(asset.AssetClassId.Identifier)).Single().Name;
            sourcePort.PrimaryPortRelation.IsPrimaryIpv4 = true;
            sourcePort.AddressInfo.Ipv4Address = "192.0.2.1";
            Helper.AssetManagement.DataPorts.Update(sourcePort);
            Helper.PlanAndBuild.AppSettings.Create(new PlanAndBuildAppSettings
            {
                JobIDPrefix = "POL", JobIDNextSequence = 1, JobIDIncrement = 1, JobIDMinimumDigits = 4,
            });
            var jobType = Helper.PlanAndBuild.JobTypes.Create(new JobType { Name = "Policy job type" });
            var job = Helper.PlanAndBuild.Jobs.Create(new PlanAndBuildJob
            {
                JobName = "Policy job",
                Type = new SdmObjectReference<JobType>(jobType.Identifier),
                AssetsUsed = new List<JobAsset>
                {
                    new JobAsset { AssetId = new SdmObjectReference<Asset>(asset.Identifier) },
                    new JobAsset { AssetId = new SdmObjectReference<Asset>(otherAsset.Identifier) },
                },
                ConnectionsOnJob = new List<JobConnection>
                {
                    new JobConnection { ConnectionId = new SdmObjectReference<Connection>(connection.Identifier), Status = "Connected" },
                },
            });
            var policy = new AssetDeletionPolicy(assetMode, connectionMode, ConnectionSnapshotFormat.AssetAndPortNames);
            var selected = InfraOpsApiComposition.Create(((TestApiHelper)Helper).Connection, PeopleApiMock.CreateDefault(), policy);
            asset.State = otherAsset.State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable;

            if (bulk)
            {
                selected.AssetManagement.Assets.Delete(new[] { asset, otherAsset });
            }
            else
            {
                selected.AssetManagement.Assets.Delete(asset);
            }

            var updated = Helper.PlanAndBuild.Jobs.Read(new TRUEFilterElement<PlanAndBuildJob>()).Single(item => item.Identifier == job.Identifier);
            if (assetMode == JobEntryRemovalMode.Remove)
            {
                updated.AssetsUsed.Should().NotContain(item => item.AssetId.Identifier == asset.Identifier);
            }
            else
            {
                updated.AssetsUsed.Single(item => item.AssetId.Identifier == asset.Identifier).Should().BeEquivalentTo(new JobAsset
                {
                    AssetId = new SdmObjectReference<Asset>(asset.Identifier),
                    Action = SlcPlan_And_Build.Enums.ActionforassetenumEnum.Removed,
                    AssetName = asset.Name,
                    AssetClassName = className,
                    IPAddress = "192.0.2.1",
                });
            }

            if (connectionMode == JobEntryRemovalMode.Remove)
            {
                updated.ConnectionsOnJob.Should().BeEmpty();
            }
            else
            {
                updated.ConnectionsOnJob.Should().ContainSingle().Which.Should().BeEquivalentTo(new JobConnection
                {
                    ConnectionId = new SdmObjectReference<Connection>(connection.Identifier),
                    Source = $"{asset.Name} - {sourcePort.DataPortInfo.Name}",
                    Destination = $"{otherAsset.Name} - {destinationPort.DataPortInfo.Name}",
                    Status = "Removed", CableType = connection.CableType, CableLength = connection.CableLength,
                });
            }

            if (!bulk)
            {
                updated.AssetsUsed.Single(item => item.AssetId.Identifier == otherAsset.Identifier).Action
                    .Should().NotBe(SlcPlan_And_Build.Enums.ActionforassetenumEnum.Removed);
            }

            Helper.AssetManagement.Connections.Count(new TRUEFilterElement<Connection>()).Should().Be(0);
            Helper.AssetManagement.DataPorts.Read(new TRUEFilterElement<DataPort>())
                .Should().NotContain(item => item.Asset.Identifier == asset.Identifier);
        }

        [TestMethod]
        public void Recovery_ShouldReuseSerializedConnectionSnapshotWhenEndpointPortIsGone()
        {
            var connection = Helper.AssetManagement.Connections.Create(CreateConnection());
            var asset = Helper.AssetManagement.Assets.Read(AssetExposers.Identifier.Equal(sourcePort.Asset.Identifier)).Single();
            Helper.PlanAndBuild.AppSettings.Create(new PlanAndBuildAppSettings
            {
                JobIDPrefix = "REC", JobIDNextSequence = 1, JobIDIncrement = 1, JobIDMinimumDigits = 4,
            });
            var jobType = Helper.PlanAndBuild.JobTypes.Create(new JobType { Name = "Connection recovery type" });
            var job = Helper.PlanAndBuild.Jobs.Create(new PlanAndBuildJob
            {
                JobName = "Connection recovery",
                Type = new SdmObjectReference<JobType>(jobType.Identifier),
                ConnectionsOnJob = new List<JobConnection>
                {
                    new JobConnection { ConnectionId = new SdmObjectReference<Connection>(connection.Identifier) },
                },
            });
            var policy = new AssetDeletionPolicy(connectionSnapshotFormat: ConnectionSnapshotFormat.AssetAndPortNames);
            var actual = Helper.AssetManagement;
            var connections = new Mock<IBulkRepository<Connection>>(MockBehavior.Strict);
            connections.Setup(repository => repository.Read(It.IsAny<FilterElement<Connection>>()))
                .Returns((FilterElement<Connection> filter) => actual.Connections.Read(filter));
            var attempts = 0;
            connections.Setup(repository => repository.Delete(It.IsAny<IEnumerable<Connection>>()))
                .Callback((IEnumerable<Connection> items) =>
                {
                    if (attempts++ == 0)
                    {
                        throw new InvalidOperationException("Simulated Connection cleanup failure.");
                    }

                    actual.Connections.Delete(items);
                });
            var helper = new Mock<IAssetManagementApiHelper>(MockBehavior.Strict);
            helper.SetupGet(item => item.Assets).Returns(actual.Assets);
            helper.SetupGet(item => item.DataPorts).Returns(actual.DataPorts);
            helper.SetupGet(item => item.PowerPorts).Returns(actual.PowerPorts);
            helper.SetupGet(item => item.Ports).Returns(actual.Ports);
            helper.SetupGet(item => item.Connections).Returns(connections.Object);
            var cascade = new AssetDeletionMiddleware(policy);
            cascade.Configure(helper.Object, (IAssetDeletionJobCleanup)Helper.PlanAndBuild, Helper.InfraOpsProperties);
            var rawAssets = new AssetDomRepository(((TestApiHelper)Helper).Connection);
            Action delete = () => cascade.OnDelete(asset, item => rawAssets.Delete(item));
            var outcome = delete.Should().Throw<AssetDeletionCascadeException>().Which.Outcomes.Single();
            outcome.FailedStage.Should().Be("Connections");
            var retained = outcome.RecoveryContext.ConnectionSnapshots.Should().ContainSingle().Which;
            retained.Source.Should().Be($"{asset.Name} - {sourcePort.DataPortInfo.Name}");
            // Simulate a missing endpoint left by another failed cleanup.
            new DataPortDomRepository(((TestApiHelper)Helper).Connection).Delete(destinationPort);
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(outcome.RecoveryContext);
            var restored = Newtonsoft.Json.JsonConvert.DeserializeObject<AssetDeletionRecoveryContext>(json)!;

            cascade.RecoverAssetDeletion(restored);
            cascade.RecoverAssetDeletion(restored);

            actual.Connections.Count(new TRUEFilterElement<Connection>()).Should().Be(0);
            var entry = Helper.PlanAndBuild.Jobs.Read(new TRUEFilterElement<PlanAndBuildJob>())
                .Single(item => item.Identifier == job.Identifier).ConnectionsOnJob.Should().ContainSingle().Which;
            entry.Source.Should().Be(retained.Source);
            entry.Destination.Should().Be(retained.Destination);
        }

        [TestMethod]
        public void Compositions_WithDifferentPolicies_ShouldNotChangeDefaultDeletion()
        {
            var custom = InfraOpsApiComposition.Create(
                ((TestApiHelper)Helper).Connection,
                PeopleApiMock.CreateDefault(),
                new AssetDeletionPolicy(JobEntryRemovalMode.KeepRemovedSnapshot, JobEntryRemovalMode.Remove));
            var defaults = InfraOpsApiComposition.Create(((TestApiHelper)Helper).Connection, PeopleApiMock.CreateDefault());
            var connection = Helper.AssetManagement.Connections.Create(CreateConnection());
            var asset = Helper.AssetManagement.Assets.Read(AssetExposers.Identifier.Equal(sourcePort.Asset.Identifier)).Single();
            Helper.PlanAndBuild.AppSettings.Create(new PlanAndBuildAppSettings
            {
                JobIDPrefix = "DEF", JobIDNextSequence = 1, JobIDIncrement = 1, JobIDMinimumDigits = 4,
            });
            var type = Helper.PlanAndBuild.JobTypes.Create(new JobType { Name = "Default isolation type" });
            var job = Helper.PlanAndBuild.Jobs.Create(new PlanAndBuildJob
            {
                JobName = "Default isolation job",
                Type = new SdmObjectReference<JobType>(type.Identifier),
                AssetsUsed = new List<JobAsset> { new JobAsset { AssetId = new SdmObjectReference<Asset>(asset.Identifier) } },
                ConnectionsOnJob = new List<JobConnection>
                {
                    new JobConnection { ConnectionId = new SdmObjectReference<Connection>(connection.Identifier) },
                },
            });
            asset.State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable;

            defaults.AssetManagement.Assets.Delete(asset);

            var updated = Helper.PlanAndBuild.Jobs.Read(new TRUEFilterElement<PlanAndBuildJob>()).Single(item => item.Identifier == job.Identifier);
            updated.AssetsUsed.Should().BeEmpty();
            updated.ConnectionsOnJob.Should().ContainSingle().Which.Source.Should().Be(connection.Source.CableTag);
            Action customIdentifierRecovery = () => custom.RecoverAssetDeletion(asset.Identifier);
            customIdentifierRecovery.Should().Throw<InvalidOperationException>();
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
                Manufacturer = SDM.AssetManagement.Tests.Setup.PeopleApiMock.NewManufacturer(),
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
                    CableTypeFks = Helper.CreateCableTypeReferences($"{name} Port Type Cable"),
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
