namespace SDM.AssetManagement.Tests.DataPorts
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
    using Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Exceptions;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    using static Skyline.DataMiner.SDM.AssetManagement.Common.Validation.DataPortValidationHandler;

    /// <summary>
    /// Changing a port's OutputType must be rejected when it would invalidate the port's existing connections.
    /// </summary>
    [TestClass]
    public class DataPortConnectionConflictTests : BaseRepositoryTest
    {
        [TestMethod]
        public void Validate_SourcePortChangedToInput_ShouldReturnConnectionConflict()
        {
            var (source, _) = CreateConnectedPorts();
            source.DataPortInfo.OutputType = SlcAsset_Management.Enums.Outputtype.In;

            var result = Helper.AssetManagement.DataPortValidator.Validate(source, RepositoryAction.Update);

            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(DataPortValidationField.ConnectionConflict, out var reason).Should().BeTrue();
            reason.Should().Contain("source");
        }

        [TestMethod]
        public void Validate_DestinationPortChangedToOutput_ShouldReturnConnectionConflict()
        {
            var (_, destination) = CreateConnectedPorts();
            destination.DataPortInfo.OutputType = SlcAsset_Management.Enums.Outputtype.Out;

            var result = Helper.AssetManagement.DataPortValidator.Validate(destination, RepositoryAction.Update);

            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(DataPortValidationField.ConnectionConflict, out var reason).Should().BeTrue();
            reason.Should().Contain("destination");
        }

        [TestMethod]
        public void Validate_SourcePortChangedToOutput_ShouldReturnValid()
        {
            var (source, _) = CreateConnectedPorts(SlcAsset_Management.Enums.Outputtype.IO, SlcAsset_Management.Enums.Outputtype.IO);
            source.DataPortInfo.OutputType = SlcAsset_Management.Enums.Outputtype.Out;

            var result = Helper.AssetManagement.DataPortValidator.Validate(source, RepositoryAction.Update);

            result.IsValid.Should().BeTrue(result.GetCombinedFailureReasons(";"));
        }

        [TestMethod]
        public void Validate_UnconnectedPortChangedToInput_ShouldReturnValid()
        {
            var (port, _) = CreatePort("Lonely");
            port.DataPortInfo.OutputType = SlcAsset_Management.Enums.Outputtype.In;

            var result = Helper.AssetManagement.DataPortValidator.Validate(port, RepositoryAction.Update);

            result.IsValid.Should().BeTrue(result.GetCombinedFailureReasons(";"));
        }

        [TestMethod]
        public void Validate_UnrelatedChangeOnConnectedPort_ShouldReturnValid()
        {
            var (source, _) = CreateConnectedPorts();
            source.DataPortInfo.PortNumber = 5;

            var result = Helper.AssetManagement.DataPortValidator.Validate(source, RepositoryAction.Update);

            result.IsValid.Should().BeTrue(result.GetCombinedFailureReasons(";"));
        }

        [TestMethod]
        public void Update_SourcePortChangedToInput_ShouldThrowAndKeepStoredOutputType()
        {
            var (source, _) = CreateConnectedPorts();
            source.DataPortInfo.OutputType = SlcAsset_Management.Enums.Outputtype.In;

            var act = () => Helper.AssetManagement.DataPorts.Update(source);

            act.Should().Throw<ValidationException>().WithMessage("*Delete the connections first*");
            Helper.AssetManagement.DataPorts.Read(new TRUEFilterElement<DataPort>()).Single(p => p.Identifier == source.Identifier).DataPortInfo.OutputType.Should().Be(SlcAsset_Management.Enums.Outputtype.Out);
        }

        private (DataPort Source, DataPort Destination) CreateConnectedPorts(
            SlcAsset_Management.Enums.Outputtype sourceType = SlcAsset_Management.Enums.Outputtype.Out,
            SlcAsset_Management.Enums.Outputtype destinationType = SlcAsset_Management.Enums.Outputtype.In)
        {
            var (source, sourceType1) = CreatePort("Source", sourceType);
            var (destination, destinationType1) = CreatePort("Destination", destinationType);

            Helper.AssetManagement.Connections.Create(new Connection
            {
                Identifier = Guid.NewGuid().ToString(),
                ConnectionType = SlcAsset_Management.Enums.ConnectionType.Data,
                CableType = sourceType1.CableFKs.CableTypeFks.First(),
                Source =
                {
                    Port = new ISdmObjectReference<IPort>(source.Identifier),
                    PortType = new SdmObjectReference<PortType>(sourceType1.Identifier),
                },
                Destination =
                {
                    Port = new ISdmObjectReference<IPort>(destination.Identifier),
                    PortType = new SdmObjectReference<PortType>(destinationType1.Identifier),
                },
            });

            return (Reload(source), Reload(destination));
        }

        private DataPort Reload(DataPort port)
        {
            return Helper.AssetManagement.DataPorts.Read(new TRUEFilterElement<DataPort>()).Single(p => p.Identifier == port.Identifier);
        }

        private (DataPort Port, PortType PortType) CreatePort(string name, SlcAsset_Management.Enums.Outputtype outputType = SlcAsset_Management.Enums.Outputtype.IO)
        {
            var deviceType = Helper.AssetManagement.DeviceTypes.Create(new DeviceType
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = $"{name} Device Type",
                Description = $"{name} description",
                HierarchyInfo = { HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.None },
                TagsInfo = { Tags = new List<SlcAsset_Management.Enums.TagOption> { SlcAsset_Management.Enums.TagOption.AcceptsDataConnection } },
            });

            var assetClass = Helper.AssetManagement.AssetClasses.Create(new AssetClass
            {
                Manufacturer = PeopleApiMock.NewManufacturer(),
                Identifier = Guid.NewGuid().ToString(),
                Name = $"{name} Asset Class",
                State = SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Active,
                DeviceTypeId = new SdmObjectReference<DeviceType>(deviceType.Identifier),
                Depth = 10,
                Width = 20,
                Height = 30,
                HeightU = 1,
                Weight = 5,
                DataPorts = new List<DataPortInfo>(),
                PowerPorts = new List<PowerPortInfo>(),
                Holders = new List<AssetHolder>(),
            });

            var asset = Helper.AssetManagement.Assets.Create(new Asset
            {
                Identifier = Guid.NewGuid().ToString(),
                AssetID = $"{name}-ASSET",
                Name = $"{name} Asset",
                AssetClassId = new SdmObjectReference<AssetClass>(assetClass.Identifier),
                State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available,
            });

            var portType = Helper.AssetManagement.PortTypes.Create(new PortType
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = $"{name} Port Type",
                CategoryLinks = { Categories = new List<SlcAsset_Management.Enums.CategoriesEnum> { SlcAsset_Management.Enums.CategoriesEnum.Data } },
                CableFKs = { CableTypeFks = Helper.CreateCableTypeReferences($"{name} Cable Type") },
            });

            var port = Helper.AssetManagement.DataPorts.Create(new DataPort
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
            });

            return (Reload(port), portType);
        }
    }
}
