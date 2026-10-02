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
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Exceptions;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    using static Skyline.DataMiner.SDM.AssetManagement.Common.Validation.DataPortValidationHandler;

    /// <summary>
    /// Validator-level and repository-rejection tests for DataPort, covering the scenarios of the
    /// shared DataPortValidationHandler/DataPortWrapper tests (wrapper constructor guards are enforced
    /// by the validation middleware in SDM).
    /// </summary>
    [TestClass]
    public class DataPortValidatorTests : BaseRepositoryTest
    {
        private Asset asset = null!;
        private PortType dataPortType = null!;
        private PortType powerPortType = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.Assets);
            asset = Helper.TestData.Assets.First();
            dataPortType = CreatePortType("Validator Data Port Type", SlcAsset_Management.Enums.CategoriesEnum.Data);
            powerPortType = CreatePortType("Validator Power Port Type", SlcAsset_Management.Enums.CategoriesEnum.Power);
        }

        [TestMethod]
        public void Validate_WithValidDataPort_ShouldReturnValid()
        {
            var result = Validate(CreateDataPort(1));

            result.IsValid.Should().BeTrue(result.GetCombinedFailureReasons(";"));
        }

        [TestMethod]
        public void Validate_WithDataPortType_ShouldPassPortTypeValidation()
        {
            var result = Validate(CreateDataPort(1));

            result.TryGetFailReason(DataPortValidationField.PortType, out _).Should().BeFalse();
        }

        [TestMethod]
        public void Validate_WithEmptyName_ShouldReturnInvalid()
        {
            var dataPort = CreateDataPort(1);
            dataPort.DataPortInfo.Name = string.Empty;

            var result = Validate(dataPort);

            result.IsValid.Should().BeFalse();
            result.GetFailReason(DataPortValidationField.Name).Should().Contain("DataPort Name cannot be empty.");
        }

        [TestMethod]
        public void Validate_WithWrongCategoryPortType_ShouldReturnInvalid()
        {
            var dataPort = CreateDataPort(1);
            dataPort.DataPortInfo.PortType = new SdmObjectReference<PortType>(powerPortType.Identifier);

            var result = Validate(dataPort);

            result.IsValid.Should().BeFalse();
            result.GetFailReason(DataPortValidationField.PortType).Should().Contain("Port Type must be a Data Port Type.");
        }

        [TestMethod]
        public void Validate_WithDuplicatePortNumberOnAsset_ShouldReturnInvalid()
        {
            Helper.AssetManagement.DataPorts.Create(CreateDataPort(1));

            var result = Validate(CreateDataPort(1));

            result.IsValid.Should().BeFalse();
            result.GetFailReason(DataPortValidationField.PortNumber).Should().Contain("Duplicate Data Port number found: 1");
        }

        [TestMethod]
        public void Validate_WithUniquePortNumberOnAsset_ShouldReturnValid()
        {
            Helper.AssetManagement.DataPorts.Create(CreateDataPort(1));

            var result = Validate(CreateDataPort(99));

            result.IsValid.Should().BeTrue(result.GetCombinedFailureReasons(";"));
        }

        [TestMethod]
        public void Validate_WithSamePortNumberAsSavedEntry_ShouldReturnValid()
        {
            var saved = CreateDataPort(1);
            Helper.AssetManagement.DataPorts.Create(saved);

            var revalidated = CreateDataPort(1);
            revalidated.Identifier = saved.Identifier;

            var result = Helper.AssetManagement.DataPortValidator.Validate(revalidated, RepositoryAction.Update);

            result.IsValid.Should().BeTrue(result.GetCombinedFailureReasons(";"));
        }

        [TestMethod]
        public void Create_WithValidInputs_ShouldPersistExactPortTypeAndAsset()
        {
            var dataPort = CreateDataPort(1);

            Helper.AssetManagement.DataPorts.Create(dataPort);

            var persisted = Helper.AssetManagement.DataPorts.Read(new TRUEFilterElement<DataPort>()).Single();
            using (new AssertionScope())
            {
                persisted.DataPortInfo.Name.Should().Be(dataPort.DataPortInfo.Name);
                persisted.DataPortInfo.PortNumber.Should().Be(1);
                persisted.DataPortInfo.PortType.Identifier.Should().Be(dataPortType.Identifier);
                persisted.Asset.Identifier.Should().Be(asset.Identifier);
            }
        }

        [TestMethod]
        public void Create_WithNullName_ShouldThrowValidationException()
        {
            var dataPort = CreateDataPort(1);
            dataPort.DataPortInfo.Name = null;

            AssertCreateRejected(dataPort, "*Name*");
        }

        [TestMethod]
        public void Create_WithNegativePortNumber_ShouldThrowValidationException()
        {
            AssertCreateRejected(CreateDataPort(-1), "*cannot be negative*");
        }

        [TestMethod]
        public void Create_WithNullPortType_ShouldThrowValidationException()
        {
            var dataPort = CreateDataPort(1);
            dataPort.DataPortInfo.PortType = null;

            AssertCreateRejected(dataPort, "*Port Type*");
        }

        [TestMethod]
        public void Create_WithPowerPortType_ShouldThrowValidationException()
        {
            var dataPort = CreateDataPort(1);
            dataPort.DataPortInfo.PortType = new SdmObjectReference<PortType>(powerPortType.Identifier);

            AssertCreateRejected(dataPort, "*Port Type must be a Data Port Type*");
        }

        [TestMethod]
        [Ignore("SDM missing validation: DataPortValidationCore only checks the asset link when AssetField changed, so DataPorts.Create with Asset = null succeeds; consumer expects rejection of a port without asset")]
        public void Create_WithNullAsset_ShouldThrowValidationException()
        {
            var dataPort = new DataPort
            {
                Identifier = Guid.NewGuid().ToString(),
                DataPortInfo =
                {
                    Name = "eth0",
                    PortNumber = 1,
                    OutputType = SlcAsset_Management.Enums.Outputtype.IO,
                    PortExposure = SlcAsset_Management.Enums.PortExposureEnum.Front,
                    PortType = new SdmObjectReference<PortType>(dataPortType.Identifier),
                },
                Asset = null,
            };

            AssertCreateRejected(dataPort, "*Asset*");
        }

        [TestMethod]
        public void Update_WithPortTypeCleared_ShouldThrowValidationException()
        {
            Helper.AssetManagement.DataPorts.Create(CreateDataPort(1));
            var persisted = Helper.AssetManagement.DataPorts.Read(new TRUEFilterElement<DataPort>()).Single();
            persisted.DataPortInfo.PortType = null;

            var act = () => Helper.AssetManagement.DataPorts.Update(persisted);

            act.Should().Throw<ValidationException>().WithMessage("*Port Type*");
        }

        [TestMethod]
        public void Update_WithAssetCleared_ShouldThrowValidationException()
        {
            Helper.AssetManagement.DataPorts.Create(CreateDataPort(1));
            var persisted = Helper.AssetManagement.DataPorts.Read(new TRUEFilterElement<DataPort>()).Single();
            persisted.Asset = null;

            var act = () => Helper.AssetManagement.DataPorts.Update(persisted);

            act.Should().Throw<ValidationException>().WithMessage("*Asset*");
        }

        private ValidationResult Validate(DataPort dataPort)
        {
            return Helper.AssetManagement.DataPortValidator.Validate(dataPort, RepositoryAction.Create);
        }

        private void AssertCreateRejected(DataPort dataPort, string messagePattern)
        {
            var act = () => Helper.AssetManagement.DataPorts.Create(dataPort);

            using (new AssertionScope())
            {
                act.Should().Throw<ValidationException>().WithMessage(messagePattern);
                Helper.AssetManagement.DataPorts.Count(new TRUEFilterElement<DataPort>()).Should().Be(0);
            }
        }

        private DataPort CreateDataPort(long portNumber)
        {
            return new DataPort
            {
                Identifier = Guid.NewGuid().ToString(),
                DataPortInfo =
                {
                    Name = $"Data Port {portNumber} {Guid.NewGuid()}",
                    PortNumber = portNumber,
                    OutputType = SlcAsset_Management.Enums.Outputtype.IO,
                    PortExposure = SlcAsset_Management.Enums.PortExposureEnum.Front,
                    PortType = new SdmObjectReference<PortType>(dataPortType.Identifier),
                },
                Asset = new SdmObjectReference<Asset>(asset.Identifier),
            };
        }

        private PortType CreatePortType(string name, SlcAsset_Management.Enums.CategoriesEnum category)
        {
            var portType = new PortType
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = name,
                CategoryLinks =
                {
                    Categories = [category],
                },
                CableFKs =
                {
                    CableTypeFks = Helper.CreateCableTypeReferences($"{name} Cable"),
                },
            };

            return Helper.AssetManagement.PortTypes.Create(portType);
        }
    }
}
