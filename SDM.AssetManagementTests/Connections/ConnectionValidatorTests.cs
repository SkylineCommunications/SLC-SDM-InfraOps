namespace SDM.AssetManagement.Tests.Connections
{
    using System;
    using System.Collections.Generic;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SDM.AssetManagement.Tests.Setup;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Exceptions;

    [TestClass]
    public class ConnectionValidatorTests : BaseRepositoryTest
    {
        [TestMethod]
        public void DataConnection_WithValidEndpoints_ShouldBeAllowed()
        {
            var sourcePort = CreateDataEndpoint("Source", SlcAsset_Management.Enums.Outputtype.IO);
            var destinationPort = CreateDataEndpoint("Destination", SlcAsset_Management.Enums.Outputtype.IO);

            Action act = () => CreateDataConnection(sourcePort, destinationPort);

            act.Should().NotThrow();
        }

        [TestMethod]
        public void DataConnection_WithSourcePortInputOnly_ShouldBeBlocked()
        {
            var sourcePort = CreateDataEndpoint("Source", SlcAsset_Management.Enums.Outputtype.In);
            var destinationPort = CreateDataEndpoint("Destination", SlcAsset_Management.Enums.Outputtype.IO);

            Action act = () => CreateDataConnection(sourcePort, destinationPort);

            act.Should().Throw<ValidationException>()
                .WithMessage("*source port must be of type Output or I/O*");
        }

        [TestMethod]
        public void DataConnection_WithDestinationPortOutputOnly_ShouldBeBlocked()
        {
            var sourcePort = CreateDataEndpoint("Source", SlcAsset_Management.Enums.Outputtype.IO);
            var destinationPort = CreateDataEndpoint("Destination", SlcAsset_Management.Enums.Outputtype.Out);

            Action act = () => CreateDataConnection(sourcePort, destinationPort);

            act.Should().Throw<ValidationException>()
                .WithMessage("*destination port must be of type Input or I/O*");
        }

        [TestMethod]
        public void DataConnection_WithNegativeCableLength_ShouldBeBlocked()
        {
            var sourcePort = CreateDataEndpoint("Source", SlcAsset_Management.Enums.Outputtype.IO);
            var destinationPort = CreateDataEndpoint("Destination", SlcAsset_Management.Enums.Outputtype.IO);

            Action act = () => CreateDataConnection(sourcePort, destinationPort, cableLength: -5);

            act.Should().Throw<ValidationException>()
                .WithMessage("*Cable length cannot be negative*");
        }

        [TestMethod]
        public void DataConnection_WithOnlySource_ShouldBeAllowed()
        {
            var sourcePort = CreateDataEndpoint("Source", SlcAsset_Management.Enums.Outputtype.IO);

            var connection = new Connection
            {
                Identifier = Guid.NewGuid().ToString(),
                ConnectionType = SlcAsset_Management.Enums.ConnectionType.Data,
                Source = new SourceInfo
                {
                    Port = Guid.Parse(sourcePort.PortId),
                    PortType = new SdmObjectReference<PortType>(sourcePort.PortType.Identifier),
                },
                Destination = new DestinationInfo { Port = Guid.Empty },
            };

            Action act = () => Helper.AssetManagement.Connections.Create(connection);

            act.Should().NotThrow();
        }

        [TestMethod]
        public void DataConnection_WithSameSourceAndDestinationPort_ShouldBeBlocked()
        {
            var sourcePort = CreateDataEndpoint("Source", SlcAsset_Management.Enums.Outputtype.IO);

            var connection = new Connection
            {
                Identifier = Guid.NewGuid().ToString(),
                ConnectionType = SlcAsset_Management.Enums.ConnectionType.Data,
                Source = new SourceInfo { Port = Guid.Parse(sourcePort.PortId) },
                Destination = new DestinationInfo { Port = Guid.Parse(sourcePort.PortId) },
            };

            Action act = () => Helper.AssetManagement.Connections.Create(connection);

            act.Should().Throw<ValidationException>()
                .WithMessage("*same as*");
        }

        [TestMethod]
        public void DataConnection_WithSourceAssetNotAvailable_ShouldBeBlocked()
        {
            var sourcePort = CreateDataEndpoint("Source", SlcAsset_Management.Enums.Outputtype.IO, assetState: SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable);
            var destinationPort = CreateDataEndpoint("Destination", SlcAsset_Management.Enums.Outputtype.IO);

            Action act = () => CreateDataConnection(sourcePort, destinationPort);

            act.Should().Throw<ValidationException>()
                .WithMessage("*Not Available*");
        }

        [TestMethod]
        public void DataConnection_WithoutAcceptsDataConnectionTag_ShouldBeBlocked()
        {
            var sourcePort = CreateDataEndpoint("Source", SlcAsset_Management.Enums.Outputtype.IO, acceptsData: false);
            var destinationPort = CreateDataEndpoint("Destination", SlcAsset_Management.Enums.Outputtype.IO);

            Action act = () => CreateDataConnection(sourcePort, destinationPort);

            act.Should().Throw<ValidationException>()
                .WithMessage("*must accept data connections*");
        }

        [TestMethod]
        public void DataConnection_WithSourcePortAlreadyInUse_ShouldBeBlocked()
        {
            var sourcePort = CreateDataEndpoint("Source", SlcAsset_Management.Enums.Outputtype.IO);
            var firstDestination = CreateDataEndpoint("Destination1", SlcAsset_Management.Enums.Outputtype.IO);
            var secondDestination = CreateDataEndpoint("Destination2", SlcAsset_Management.Enums.Outputtype.IO);

            CreateDataConnection(sourcePort, firstDestination);

            Action act = () => CreateDataConnection(sourcePort, secondDestination);

            act.Should().Throw<ValidationException>()
                .WithMessage("*is already in use*");
        }

        [TestMethod]
        public void PowerConnection_WithSourceNotPowerProvider_ShouldBeBlocked()
        {
            var sourcePort = CreatePowerEndpoint("Source", SlcAsset_Management.Enums.Outputtype.IO, isPowerProvider: false);
            var destinationPort = CreatePowerEndpoint("Destination", SlcAsset_Management.Enums.Outputtype.IO, isPowerProvider: false);

            Action act = () => CreatePowerConnection(sourcePort, destinationPort);

            act.Should().Throw<ValidationException>()
                .WithMessage("*must be a Power Provider*");
        }

        [TestMethod]
        public void PowerConnection_WithPowerProviderSource_ShouldBeAllowed()
        {
            var sourcePort = CreatePowerEndpoint("Source", SlcAsset_Management.Enums.Outputtype.IO, isPowerProvider: true);
            var destinationPort = CreatePowerEndpoint("Destination", SlcAsset_Management.Enums.Outputtype.IO, isPowerProvider: false);

            Action act = () => CreatePowerConnection(sourcePort, destinationPort);

            act.Should().NotThrow();
        }

        private Endpoint CreateDataEndpoint(
            string name,
            SlcAsset_Management.Enums.Outputtype outputType,
            bool acceptsData = true,
            SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum assetState = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available)
        {
            var tags = acceptsData
                ? new List<SlcAsset_Management.Enums.TagOption> { SlcAsset_Management.Enums.TagOption.AcceptsDataConnection }
                : new List<SlcAsset_Management.Enums.TagOption>();

            var deviceType = CreateDeviceType($"{name} Device Type", tags);
            var assetClass = CreateAssetClass($"{name} Asset Class", SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Active, deviceType);
            var asset = CreateAsset(assetClass, $"{name}-ASSET", assetState);
            var portType = CreatePortType($"{name} Port Type", SlcAsset_Management.Enums.CategoriesEnum.Data);
            var port = CreateDataPort(asset, portType, outputType, name);

            return new Endpoint { PortId = port.Identifier, PortType = portType };
        }

        private Endpoint CreatePowerEndpoint(
            string name,
            SlcAsset_Management.Enums.Outputtype outputType,
            bool isPowerProvider)
        {
            var tags = isPowerProvider
                ? new List<SlcAsset_Management.Enums.TagOption> { SlcAsset_Management.Enums.TagOption.PowerProvider }
                : new List<SlcAsset_Management.Enums.TagOption>();

            // An Asset Class carrying a Power Provider device type must declare a power supply.
            var powerSupply = isPowerProvider ? (SlcAsset_Management.Enums.PowerSupplyEnum?)SlcAsset_Management.Enums.PowerSupplyEnum.AC : null;

            var deviceType = CreateDeviceType($"{name} Device Type", tags);
            var assetClass = CreateAssetClass($"{name} Asset Class", SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Active, deviceType, powerSupply);
            var asset = CreateAsset(assetClass, $"{name}-ASSET", SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available);
            var portType = CreatePortType($"{name} Port Type", SlcAsset_Management.Enums.CategoriesEnum.Power);
            var port = CreatePowerPort(asset, portType, outputType, name);

            return new Endpoint { PortId = port.Identifier, PortType = portType };
        }

        private void CreateDataConnection(Endpoint source, Endpoint destination, double? cableLength = null)
        {
            var connection = new Connection
            {
                Identifier = Guid.NewGuid().ToString(),
                ConnectionType = SlcAsset_Management.Enums.ConnectionType.Data,
                CableLength = cableLength,
                Source = new SourceInfo
                {
                    Port = Guid.Parse(source.PortId),
                    PortType = new SdmObjectReference<PortType>(source.PortType.Identifier),
                },
                Destination = new DestinationInfo
                {
                    Port = Guid.Parse(destination.PortId),
                    PortType = new SdmObjectReference<PortType>(destination.PortType.Identifier),
                },
            };

            Helper.AssetManagement.Connections.Create(connection);
        }

        private void CreatePowerConnection(Endpoint source, Endpoint destination, double? cableLength = null)
        {
            var connection = new Connection
            {
                Identifier = Guid.NewGuid().ToString(),
                ConnectionType = SlcAsset_Management.Enums.ConnectionType.Power,
                CableLength = cableLength,
                Source = new SourceInfo
                {
                    Port = Guid.Parse(source.PortId),
                    PortType = new SdmObjectReference<PortType>(source.PortType.Identifier),
                },
                Destination = new DestinationInfo
                {
                    Port = Guid.Parse(destination.PortId),
                    PortType = new SdmObjectReference<PortType>(destination.PortType.Identifier),
                },
            };

            Helper.AssetManagement.Connections.Create(connection);
        }

        private DeviceType CreateDeviceType(string name, List<SlcAsset_Management.Enums.TagOption> tags)
        {
            var deviceType = new DeviceType
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = name,
                Description = $"{name} description",
                HierarchyInfo = new HierarchyInfo
                {
                    HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.None,
                },
                TagsInfo = new TagsInfo
                {
                    Tags = tags,
                },
            };

            return Helper.AssetManagement.DeviceTypes.Create(deviceType);
        }

        private AssetClass CreateAssetClass(string name, SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum state, DeviceType deviceType, SlcAsset_Management.Enums.PowerSupplyEnum? powerSupply = null)
        {
            var assetClass = new AssetClass
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = name,
                State = state,
                DeviceTypeId = new SdmObjectReference<DeviceType>(deviceType.Identifier),
                PowerSupply = powerSupply,
                Depth = 10,
                Width = 20,
                Height = 30,
                HeightU = 1,
                Weight = 5,
                DataPorts = new List<DataPortInfo>(),
                PowerPorts = new List<PowerPortInfo>(),
                Holders = new List<AssetHolder>(),
            };

            return Helper.AssetManagement.AssetClasses.Create(assetClass);
        }

        private Asset CreateAsset(AssetClass assetClass, string assetId, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum state)
        {
            var asset = new Asset
            {
                Identifier = Guid.NewGuid().ToString(),
                AssetID = assetId,
                Name = $"{assetId} Name",
                AssetClassId = new SdmObjectReference<AssetClass>(assetClass.Identifier),
                State = state,
            };

            return Helper.AssetManagement.Assets.Create(asset);
        }

        private PortType CreatePortType(string name, SlcAsset_Management.Enums.CategoriesEnum category)
        {
            var portType = new PortType
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = name,
                CategoryLinks = new CategoryRelation
                {
                    Categories = new List<SlcAsset_Management.Enums.CategoriesEnum> { category },
                },
                CableFKs = new CableRelation
                {
                    CableTypeFks = new List<SdmObjectReference<CableType>>(),
                },
            };

            return Helper.AssetManagement.PortTypes.Create(portType);
        }

        private DataPort CreateDataPort(Asset asset, PortType portType, SlcAsset_Management.Enums.Outputtype outputType, string name)
        {
            var dataPort = new DataPort
            {
                Identifier = Guid.NewGuid().ToString(),
                Asset = new SdmObjectReference<Asset>(asset.Identifier),
                DataPortInfo = new DataPortInfo
                {
                    Name = $"{name} Data {Guid.NewGuid()}",
                    PortNumber = 1,
                    OutputType = outputType,
                    PortExposure = SlcAsset_Management.Enums.PortExposureEnum.Front,
                    Type = new SdmObjectReference<PortType>(portType.Identifier),
                },
            };

            return Helper.AssetManagement.DataPorts.Create(dataPort);
        }

        private PowerPort CreatePowerPort(Asset asset, PortType portType, SlcAsset_Management.Enums.Outputtype outputType, string name)
        {
            var powerPort = new PowerPort
            {
                Identifier = Guid.NewGuid().ToString(),
                Asset = new SdmObjectReference<Asset>(asset.Identifier),
                PowerPortInfo = new PowerPortInfo
                {
                    Name = $"{name} Power {Guid.NewGuid()}",
                    PortNumber = 1,
                    OutputType = outputType,
                    PortExposure = SlcAsset_Management.Enums.PortExposureEnum.Front,
                    PortType = new SdmObjectReference<PortType>(portType.Identifier),
                },
            };

            return Helper.AssetManagement.PowerPorts.Create(powerPort);
        }

        private sealed class Endpoint
        {
            public string PortId { get; set; }

            public PortType PortType { get; set; }
        }
    }
}

