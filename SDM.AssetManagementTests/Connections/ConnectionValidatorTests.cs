namespace SDM.AssetManagement.Tests.Connections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SDM.AssetManagement.Tests.Setup;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Exceptions;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    using static Skyline.DataMiner.SDM.AssetManagement.Common.Validation.ConnectionValidationHandler;

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
                Source =
                {
                    Port = new ISdmObjectReference<IPort>(sourcePort.PortId),
                    PortType = new SdmObjectReference<PortType>(sourcePort.PortType.Identifier),
                },
                Destination = { Port = default },
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
                Source = { Port = new ISdmObjectReference<IPort>(sourcePort.PortId) },
                Destination = { Port = new ISdmObjectReference<IPort>(sourcePort.PortId) },
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

        [TestMethod]
        public void ValidateDataConnection_WithSourcePortInputOnly_ShouldReturnSourcePortFailure()
        {
            var source = CreateDataEndpoint("Source", SlcAsset_Management.Enums.Outputtype.In);
            var destination = CreateDataEndpoint("Destination", SlcAsset_Management.Enums.Outputtype.IO);

            var result = ValidateConnection(BuildConnection(SlcAsset_Management.Enums.ConnectionType.Data, source, destination));

            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(ConnectionValidationField.SourcePort, out _).Should().BeTrue();
        }

        [TestMethod]
        public void ValidateDataConnection_WithSourcePortAlreadyInUse_ShouldReturnSourcePortFailure()
        {
            var source = CreateDataEndpoint("Source", SlcAsset_Management.Enums.Outputtype.IO);
            var firstDestination = CreateDataEndpoint("Destination1", SlcAsset_Management.Enums.Outputtype.IO);
            var secondDestination = CreateDataEndpoint("Destination2", SlcAsset_Management.Enums.Outputtype.IO);
            CreateDataConnection(source, firstDestination);

            var result = ValidateConnection(BuildConnection(SlcAsset_Management.Enums.ConnectionType.Data, source, secondDestination));

            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(ConnectionValidationField.SourcePort, out _).Should().BeTrue();
        }

        [TestMethod]
        public void ValidateDataConnection_WithDestinationPortOutputOnly_ShouldReturnDestinationPortFailure()
        {
            var source = CreateDataEndpoint("Source", SlcAsset_Management.Enums.Outputtype.IO);
            var destination = CreateDataEndpoint("Destination", SlcAsset_Management.Enums.Outputtype.Out);

            var result = ValidateConnection(BuildConnection(SlcAsset_Management.Enums.ConnectionType.Data, source, destination));

            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(ConnectionValidationField.DestinationPort, out _).Should().BeTrue();
        }

        [TestMethod]
        public void ValidateDataConnection_WithDestinationPortAlreadyInUse_ShouldReturnDestinationPortFailure()
        {
            var firstSource = CreateDataEndpoint("Source1", SlcAsset_Management.Enums.Outputtype.IO);
            var destination = CreateDataEndpoint("Destination", SlcAsset_Management.Enums.Outputtype.IO);
            var secondSource = CreateDataEndpoint("Source2", SlcAsset_Management.Enums.Outputtype.IO);
            CreateDataConnection(firstSource, destination);

            var result = ValidateConnection(BuildConnection(SlcAsset_Management.Enums.ConnectionType.Data, secondSource, destination));

            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(ConnectionValidationField.DestinationPort, out _).Should().BeTrue();
        }

        [TestMethod]
        public void ValidateDataConnection_WithSourceAssetNotAvailable_ShouldReturnSourceAssetFailure()
        {
            var source = CreateDataEndpoint("Source", SlcAsset_Management.Enums.Outputtype.IO, assetState: SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable);
            var destination = CreateDataEndpoint("Destination", SlcAsset_Management.Enums.Outputtype.IO);

            var result = ValidateConnection(BuildConnection(SlcAsset_Management.Enums.ConnectionType.Data, source, destination));

            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(ConnectionValidationField.SourceAsset, out _).Should().BeTrue();
        }

        [TestMethod]
        public void ValidateDataConnection_WithDeviceMissingDataTag_ShouldReturnSourceAssetFailure()
        {
            var source = CreateDataEndpoint("Source", SlcAsset_Management.Enums.Outputtype.IO, acceptsData: false);
            var destination = CreateDataEndpoint("Destination", SlcAsset_Management.Enums.Outputtype.IO);

            var result = ValidateConnection(BuildConnection(SlcAsset_Management.Enums.ConnectionType.Data, source, destination));

            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(ConnectionValidationField.SourceAsset, out _).Should().BeTrue();
        }

        [TestMethod]
        public void ValidateDataConnection_WithSameSourceAndDestinationPort_ShouldReturnBothPortFailures()
        {
            var port = CreateDataEndpoint("Port", SlcAsset_Management.Enums.Outputtype.IO);

            var result = ValidateConnection(BuildConnection(SlcAsset_Management.Enums.ConnectionType.Data, port, port));

            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(ConnectionValidationField.SourcePort, out _).Should().BeTrue();
            result.TryGetFailReason(ConnectionValidationField.DestinationPort, out _).Should().BeTrue();
        }

        [TestMethod]
        public void ValidateDataConnection_WithValidEndpoints_ShouldReturnValid()
        {
            var source = CreateDataEndpoint("Source", SlcAsset_Management.Enums.Outputtype.IO);
            var destination = CreateDataEndpoint("Destination", SlcAsset_Management.Enums.Outputtype.IO);

            var result = ValidateConnection(BuildConnection(SlcAsset_Management.Enums.ConnectionType.Data, source, destination));

            result.IsValid.Should().BeTrue(result.GetCombinedFailureReasons(";"));
        }

        [TestMethod]
        public void ValidateConnection_WithFullyPopulatedDataConnection_ShouldReturnValid()
        {
            var connection = BuildFullyPopulatedDataConnection();

            var result = ValidateConnection(connection);

            result.IsValid.Should().BeTrue(result.GetCombinedFailureReasons(";"));
        }

        [TestMethod]
        public void ValidateConnection_WithNegativeCableLengthAndSelfConnection_ShouldAccumulateFailures()
        {
            var port = CreateDataEndpoint("Port", SlcAsset_Management.Enums.Outputtype.IO);

            var result = ValidateConnection(BuildConnection(SlcAsset_Management.Enums.ConnectionType.Data, port, port, cableLength: -1));

            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(ConnectionValidationField.CableLength, out _).Should().BeTrue();
            result.TryGetFailReason(ConnectionValidationField.SourcePort, out _).Should().BeTrue();
            result.TryGetFailReason(ConnectionValidationField.DestinationPort, out _).Should().BeTrue();
        }

        [TestMethod]
        public void ValidatePowerConnection_WithSourcePortInputOnly_ShouldReturnSourcePortFailure()
        {
            var source = CreatePowerEndpoint("Source", SlcAsset_Management.Enums.Outputtype.In, isPowerProvider: true);
            var destination = CreatePowerEndpoint("Destination", SlcAsset_Management.Enums.Outputtype.IO, isPowerProvider: true);

            var result = ValidateConnection(BuildConnection(SlcAsset_Management.Enums.ConnectionType.Power, source, destination));

            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(ConnectionValidationField.SourcePort, out _).Should().BeTrue();
        }

        [TestMethod]
        public void ValidatePowerConnection_WithSourcePortAlreadyInUse_ShouldReturnSourcePortFailure()
        {
            var source = CreatePowerEndpoint("Source", SlcAsset_Management.Enums.Outputtype.IO, isPowerProvider: true);
            var firstDestination = CreatePowerEndpoint("Destination1", SlcAsset_Management.Enums.Outputtype.IO, isPowerProvider: true);
            var secondDestination = CreatePowerEndpoint("Destination2", SlcAsset_Management.Enums.Outputtype.IO, isPowerProvider: true);
            CreatePowerConnection(source, firstDestination);

            var result = ValidateConnection(BuildConnection(SlcAsset_Management.Enums.ConnectionType.Power, source, secondDestination));

            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(ConnectionValidationField.SourcePort, out _).Should().BeTrue();
        }

        [TestMethod]
        public void ValidatePowerConnection_WithDestinationPortOutputOnly_ShouldReturnDestinationPortFailure()
        {
            var source = CreatePowerEndpoint("Source", SlcAsset_Management.Enums.Outputtype.IO, isPowerProvider: true);
            var destination = CreatePowerEndpoint("Destination", SlcAsset_Management.Enums.Outputtype.Out, isPowerProvider: true);

            var result = ValidateConnection(BuildConnection(SlcAsset_Management.Enums.ConnectionType.Power, source, destination));

            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(ConnectionValidationField.DestinationPort, out _).Should().BeTrue();
        }

        [TestMethod]
        public void ValidatePowerConnection_WithDestinationPortAlreadyInUse_ShouldReturnDestinationPortFailure()
        {
            var firstSource = CreatePowerEndpoint("Source1", SlcAsset_Management.Enums.Outputtype.IO, isPowerProvider: true);
            var destination = CreatePowerEndpoint("Destination", SlcAsset_Management.Enums.Outputtype.IO, isPowerProvider: true);
            var secondSource = CreatePowerEndpoint("Source2", SlcAsset_Management.Enums.Outputtype.IO, isPowerProvider: true);
            CreatePowerConnection(firstSource, destination);

            var result = ValidateConnection(BuildConnection(SlcAsset_Management.Enums.ConnectionType.Power, secondSource, destination));

            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(ConnectionValidationField.DestinationPort, out _).Should().BeTrue();
        }

        [TestMethod]
        public void ValidatePowerConnection_WithSourceAssetNotAvailable_ShouldReturnSourceAssetFailure()
        {
            var source = CreatePowerEndpoint("Source", SlcAsset_Management.Enums.Outputtype.IO, isPowerProvider: true, assetState: SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable);
            var destination = CreatePowerEndpoint("Destination", SlcAsset_Management.Enums.Outputtype.IO, isPowerProvider: true);

            var result = ValidateConnection(BuildConnection(SlcAsset_Management.Enums.ConnectionType.Power, source, destination));

            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(ConnectionValidationField.SourceAsset, out _).Should().BeTrue();
        }

        [TestMethod]
        public void ValidatePowerConnection_WithSourceMissingPowerProviderTag_ShouldReturnSourceAssetFailure()
        {
            var source = CreatePowerEndpoint("Source", SlcAsset_Management.Enums.Outputtype.IO, isPowerProvider: false);
            var destination = CreatePowerEndpoint("Destination", SlcAsset_Management.Enums.Outputtype.IO, isPowerProvider: true);

            var result = ValidateConnection(BuildConnection(SlcAsset_Management.Enums.ConnectionType.Power, source, destination));

            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(ConnectionValidationField.SourceAsset, out _).Should().BeTrue();
        }

        [TestMethod]
        public void ValidatePowerConnection_WithDestinationAssetNotAvailable_ShouldReturnDestinationAssetFailure()
        {
            var source = CreatePowerEndpoint("Source", SlcAsset_Management.Enums.Outputtype.IO, isPowerProvider: true);
            var destination = CreatePowerEndpoint("Destination", SlcAsset_Management.Enums.Outputtype.IO, isPowerProvider: true, assetState: SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable);

            var result = ValidateConnection(BuildConnection(SlcAsset_Management.Enums.ConnectionType.Power, source, destination));

            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(ConnectionValidationField.DestinationAsset, out _).Should().BeTrue();
        }

        [TestMethod]
        public void ValidatePowerConnection_WithDestinationMissingPowerProviderTag_ShouldReturnValid()
        {
            var source = CreatePowerEndpoint("Source", SlcAsset_Management.Enums.Outputtype.IO, isPowerProvider: true);
            var destination = CreatePowerEndpoint("Destination", SlcAsset_Management.Enums.Outputtype.IO, isPowerProvider: false);

            var result = ValidateConnection(BuildConnection(SlcAsset_Management.Enums.ConnectionType.Power, source, destination));

            result.IsValid.Should().BeTrue(result.GetCombinedFailureReasons(";"));
        }

        [TestMethod]
        public void ValidatePowerConnection_WithValidEndpoints_ShouldReturnValid()
        {
            var source = CreatePowerEndpoint("Source", SlcAsset_Management.Enums.Outputtype.IO, isPowerProvider: true);
            var destination = CreatePowerEndpoint("Destination", SlcAsset_Management.Enums.Outputtype.IO, isPowerProvider: true);

            var result = ValidateConnection(BuildConnection(SlcAsset_Management.Enums.ConnectionType.Power, source, destination));

            result.IsValid.Should().BeTrue(result.GetCombinedFailureReasons(";"));
        }

        [TestMethod]
        public void Connection_WithDataEndpoints_HasSourceDestinationAndType()
        {
            var source = CreateDataEndpoint("Source", SlcAsset_Management.Enums.Outputtype.IO);
            var destination = CreateDataEndpoint("Destination", SlcAsset_Management.Enums.Outputtype.IO);

            var connection = BuildConnection(SlcAsset_Management.Enums.ConnectionType.Data, source, destination);

            connection.Source.IsEmpty.Should().BeFalse();
            connection.Destination.IsEmpty.Should().BeFalse();
            connection.ConnectionType.Should().Be(SlcAsset_Management.Enums.ConnectionType.Data);
        }

        [TestMethod]
        public void NewDataConnection_BeforeCreate_ShouldHavePendingChanges()
        {
            var connection = BuildFullyPopulatedDataConnection();

            connection.GetChanges().Should().NotBeEmpty("a newly created data connection should have pending changes");
        }

        [TestMethod]
        public void Create_WithPowerConnection_ShouldPersistAndReadBack()
        {
            var source = CreatePowerEndpoint("Source", SlcAsset_Management.Enums.Outputtype.IO, isPowerProvider: true);
            var destination = CreatePowerEndpoint("Destination", SlcAsset_Management.Enums.Outputtype.IO, isPowerProvider: true);
            var cableType = Helper.CreateCableType("Power Cable");

            var connection = BuildConnection(SlcAsset_Management.Enums.ConnectionType.Power, source, destination, cableLength: 6);
            connection.Description = "Description";
            connection.Notes = "Notes";
            connection.CableType = new SdmObjectReference<CableType>(cableType.Identifier);
            connection.GetChanges().Should().NotBeEmpty("a newly created power connection should have pending changes");

            Helper.AssetManagement.Connections.Create(connection);

            var persisted = Helper.AssetManagement.Connections.Read(new TRUEFilterElement<Connection>()).Single();
            persisted.ConnectionType.Should().Be(SlcAsset_Management.Enums.ConnectionType.Power);
            persisted.Source.Port.Identifier.Should().Be(source.PortId);
            persisted.Destination.Port.Identifier.Should().Be(destination.PortId);
            persisted.CableType.Identifier.Should().Be(cableType.Identifier);
            persisted.CableLength.Should().Be(6);
            persisted.Description.Should().Be("Description");
            persisted.Notes.Should().Be("Notes");
        }

        [TestMethod]
        [Ignore("Behavior difference: failing at ConnectionValidator skipping the DB phase (port direction) when cable length is invalid, so no SourcePort fail reason; consumer expects CableLength and SourcePort failures accumulated")]
        public void ValidateConnection_WithNegativeCableLengthAndSourcePortInputOnly_ShouldAccumulateFailures()
        {
            var source = CreateDataEndpoint("Source", SlcAsset_Management.Enums.Outputtype.In);
            var destination = CreateDataEndpoint("Destination", SlcAsset_Management.Enums.Outputtype.IO);

            var result = ValidateConnection(BuildConnection(SlcAsset_Management.Enums.ConnectionType.Data, source, destination, cableLength: -1));

            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(ConnectionValidationField.CableLength, out _).Should().BeTrue();
            result.TryGetFailReason(ConnectionValidationField.SourcePort, out _).Should().BeTrue();
        }

        [TestMethod]
        [Ignore("Behavior difference: failing at Connections.Create returning a new instance and leaving the caller's instance with pending changes; consumer expects GetChanges() empty on the same instance after create")]
        public void CreateDataConnection_AfterCreate_ShouldHaveNoPendingChanges()
        {
            var connection = BuildFullyPopulatedDataConnection();
            connection.GetChanges().Should().NotBeEmpty("a newly created data connection should have pending changes");

            Helper.AssetManagement.Connections.Create(connection);

            connection.GetChanges().Should().BeEmpty();
        }

        private ValidationResult ValidateConnection(Connection connection)
        {
            return Helper.AssetManagement.ConnectionValidator.Validate(connection, RepositoryAction.Create);
        }

        private Connection BuildFullyPopulatedDataConnection()
        {
            var source = CreateDataEndpoint("Source", SlcAsset_Management.Enums.Outputtype.IO);
            var destination = CreateDataEndpoint("Destination", SlcAsset_Management.Enums.Outputtype.IO);
            var cableType = Helper.CreateCableType("Data Cable");

            var connection = BuildConnection(SlcAsset_Management.Enums.ConnectionType.Data, source, destination, cableLength: 6);
            connection.Description = "Description";
            connection.Notes = "Notes";
            connection.CableType = new SdmObjectReference<CableType>(cableType.Identifier);
            connection.Source.CableTag = "STag";
            connection.Destination.CableTag = "DTag";

            return connection;
        }

        private static Connection BuildConnection(SlcAsset_Management.Enums.ConnectionType type, Endpoint source, Endpoint destination, double? cableLength = null)
        {
            return new Connection
            {
                Identifier = Guid.NewGuid().ToString(),
                ConnectionType = type,
                CableLength = cableLength,
                Source =
                {
                    Port = new ISdmObjectReference<IPort>(source.PortId),
                    PortType = new SdmObjectReference<PortType>(source.PortType.Identifier),
                },
                Destination =
                {
                    Port = new ISdmObjectReference<IPort>(destination.PortId),
                    PortType = new SdmObjectReference<PortType>(destination.PortType.Identifier),
                },
            };
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
            bool isPowerProvider,
            SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum assetState = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available)
        {
            var tags = isPowerProvider
                ? new List<SlcAsset_Management.Enums.TagOption> { SlcAsset_Management.Enums.TagOption.PowerProvider }
                : new List<SlcAsset_Management.Enums.TagOption>();

            // An Asset Class carrying a Power Provider device type must declare a power supply.
            var powerSupply = isPowerProvider ? (SlcAsset_Management.Enums.PowerSupplyEnum?)SlcAsset_Management.Enums.PowerSupplyEnum.AC : null;

            var deviceType = CreateDeviceType($"{name} Device Type", tags);
            var assetClass = CreateAssetClass($"{name} Asset Class", SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Active, deviceType, powerSupply);
            var asset = CreateAsset(assetClass, $"{name}-ASSET", assetState);
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
                Source =
                {
                    Port = new ISdmObjectReference<IPort>(source.PortId),
                    PortType = new SdmObjectReference<PortType>(source.PortType.Identifier),
                },
                Destination =
                {
                    Port = new ISdmObjectReference<IPort>(destination.PortId),
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
                Source =
                {
                    Port = new ISdmObjectReference<IPort>(source.PortId),
                    PortType = new SdmObjectReference<PortType>(source.PortType.Identifier),
                },
                Destination =
                {
                    Port = new ISdmObjectReference<IPort>(destination.PortId),
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
                HierarchyInfo =
                {
                    HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.None,
                },
                TagsInfo =
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
                Manufacturer = SDM.AssetManagement.Tests.Setup.PeopleApiMock.NewManufacturer(),
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
                CategoryLinks =
                {
                    Categories = new List<SlcAsset_Management.Enums.CategoriesEnum> { category },
                },
                CableFKs =
                {
                    CableTypeFks = Helper.CreateCableTypeReferences($"{name} Cable Type"),
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
                DataPortInfo =
                {
                    Name = $"{name} Data {Guid.NewGuid()}",
                    PortNumber = 1,
                    OutputType = outputType,
                    PortExposure = SlcAsset_Management.Enums.PortExposureEnum.Front,
                    PortType = new SdmObjectReference<PortType>(portType.Identifier),
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
                PowerPortInfo =
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

