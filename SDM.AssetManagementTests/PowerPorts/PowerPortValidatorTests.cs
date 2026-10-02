namespace SDM.AssetManagement.Tests.PowerPorts
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

    using static Skyline.DataMiner.SDM.AssetManagement.Common.Validation.PowerPortValidationHandler;

    /// <summary>
    /// Validator-level and repository-rejection tests for PowerPort, covering the scenarios of the
    /// shared PowerPortValidationHandler/PowerPortWrapper tests (wrapper constructor guards are enforced
    /// by the validation middleware in SDM).
    /// </summary>
    [TestClass]
    public class PowerPortValidatorTests : BaseRepositoryTest
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
        public void Validate_WithValidPowerPort_ShouldReturnValid()
        {
            var result = Validate(CreatePowerPort(1));

            result.IsValid.Should().BeTrue(result.GetCombinedFailureReasons(";"));
        }

        [TestMethod]
        public void Validate_WithPowerPortType_ShouldPassPortTypeValidation()
        {
            var result = Validate(CreatePowerPort(1));

            result.TryGetFailReason(PowerPortValidationField.PortType, out _).Should().BeFalse();
        }

        [TestMethod]
        public void Validate_WithEmptyName_ShouldReturnInvalid()
        {
            var powerPort = CreatePowerPort(1);
            powerPort.PowerPortInfo.Name = string.Empty;

            var result = Validate(powerPort);

            result.IsValid.Should().BeFalse();
            result.GetFailReason(PowerPortValidationField.Name).Should().Contain("PowerPort Name cannot be empty.");
        }

        [TestMethod]
        public void Validate_WithWrongCategoryPortType_ShouldReturnInvalid()
        {
            var powerPort = CreatePowerPort(1);
            powerPort.PowerPortInfo.PortType = new SdmObjectReference<PortType>(dataPortType.Identifier);

            var result = Validate(powerPort);

            result.IsValid.Should().BeFalse();
            result.GetFailReason(PowerPortValidationField.PortType).Should().Contain("Port Type must be a Power Port Type.");
        }

        [TestMethod]
        public void Validate_WithDuplicatePortNumberOnAsset_ShouldReturnInvalid()
        {
            Helper.AssetManagement.PowerPorts.Create(CreatePowerPort(1));

            var result = Validate(CreatePowerPort(1));

            result.IsValid.Should().BeFalse();
            result.GetFailReason(PowerPortValidationField.PortNumber).Should().Contain("Duplicate Power Port number found: 1");
        }

        [TestMethod]
        public void Validate_WithUniquePortNumberOnAsset_ShouldReturnValid()
        {
            Helper.AssetManagement.PowerPorts.Create(CreatePowerPort(1));

            var result = Validate(CreatePowerPort(99));

            result.IsValid.Should().BeTrue(result.GetCombinedFailureReasons(";"));
        }

        [TestMethod]
        public void Validate_WithSamePortNumberAsSavedEntry_ShouldReturnValid()
        {
            var saved = CreatePowerPort(1);
            Helper.AssetManagement.PowerPorts.Create(saved);

            var revalidated = CreatePowerPort(1);
            revalidated.Identifier = saved.Identifier;

            var result = Helper.AssetManagement.PowerPortValidator.Validate(revalidated, RepositoryAction.Update);

            result.IsValid.Should().BeTrue(result.GetCombinedFailureReasons(";"));
        }

        [TestMethod]
        public void Create_WithValidInputs_ShouldPersistExactPortTypeAndAsset()
        {
            var powerPort = CreatePowerPort(1);

            Helper.AssetManagement.PowerPorts.Create(powerPort);

            var persisted = Helper.AssetManagement.PowerPorts.Read(new TRUEFilterElement<PowerPort>()).Single();
            using (new AssertionScope())
            {
                persisted.PowerPortInfo.Name.Should().Be(powerPort.PowerPortInfo.Name);
                persisted.PowerPortInfo.PortNumber.Should().Be(1);
                persisted.PowerPortInfo.PortType.Identifier.Should().Be(powerPortType.Identifier);
                persisted.Asset.Identifier.Should().Be(asset.Identifier);
            }
        }

        [TestMethod]
        public void Create_WithNullName_ShouldThrowValidationException()
        {
            var powerPort = CreatePowerPort(1);
            powerPort.PowerPortInfo.Name = null;

            AssertCreateRejected(powerPort, "*Name*");
        }

        [TestMethod]
        public void Create_WithNegativePortNumber_ShouldThrowValidationException()
        {
            AssertCreateRejected(CreatePowerPort(-1), "*cannot be negative*");
        }

        [TestMethod]
        public void Create_WithNullPortType_ShouldThrowValidationException()
        {
            var powerPort = CreatePowerPort(1);
            powerPort.PowerPortInfo.PortType = null;

            AssertCreateRejected(powerPort, "*Port Type*");
        }

        [TestMethod]
        public void Create_WithDataPortType_ShouldThrowValidationException()
        {
            var powerPort = CreatePowerPort(1);
            powerPort.PowerPortInfo.PortType = new SdmObjectReference<PortType>(dataPortType.Identifier);

            AssertCreateRejected(powerPort, "*Port Type must be a Power Port Type*");
        }

        [TestMethod]
        public void Create_WithNullAsset_ShouldThrowValidationException()
        {
            var powerPort = CreatePowerPort(1);
            powerPort.Asset = null;

            AssertCreateRejected(powerPort, "*Asset*");
        }

        private ValidationResult Validate(PowerPort powerPort)
        {
            return Helper.AssetManagement.PowerPortValidator.Validate(powerPort, RepositoryAction.Create);
        }

        private void AssertCreateRejected(PowerPort powerPort, string messagePattern)
        {
            var act = () => Helper.AssetManagement.PowerPorts.Create(powerPort);

            using (new AssertionScope())
            {
                act.Should().Throw<ValidationException>().WithMessage(messagePattern);
                Helper.AssetManagement.PowerPorts.Count(new TRUEFilterElement<PowerPort>()).Should().Be(0);
            }
        }

        private PowerPort CreatePowerPort(long portNumber)
        {
            return new PowerPort
            {
                Identifier = Guid.NewGuid().ToString(),
                PowerPortInfo =
                {
                    Name = $"Power Port {portNumber} {Guid.NewGuid()}",
                    PortNumber = portNumber,
                    OutputType = SlcAsset_Management.Enums.Outputtype.IO,
                    PortExposure = SlcAsset_Management.Enums.PortExposureEnum.Front,
                    PortType = new SdmObjectReference<PortType>(powerPortType.Identifier),
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
