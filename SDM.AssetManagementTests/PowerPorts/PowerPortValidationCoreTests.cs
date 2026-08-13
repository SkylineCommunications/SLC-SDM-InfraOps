namespace SDM.AssetManagement.Tests.PowerPorts
{
    using System;
    using System.Collections.Generic;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.AssetManagement.Validation;

    [TestClass]
    public class PowerPortValidationCoreTests
    {
        private static readonly SdmObjectReference<Asset> AssetReference = new SdmObjectReference<Asset>(Guid.NewGuid().ToString());

        [TestMethod]
        public void PortTypeAgainst_WithEmptyPortType_ShouldFail()
        {
            var powerPort = CreatePowerPort(1);
            var core = new PowerPortValidationCore(null);

            var result = core.ValidatePortTypeAgainst(powerPort, null);

            result.IsValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("Port Type cannot be empty."));
        }

        [TestMethod]
        public void PortTypeAgainst_WithMissingPortType_ShouldFail()
        {
            var powerPort = CreatePowerPort(1);
            powerPort.PowerPortInfo.PortType = new SdmObjectReference<PortType>(Guid.NewGuid().ToString());
            var core = new PowerPortValidationCore(null);

            var result = core.ValidatePortTypeAgainst(powerPort, null);

            result.IsValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("Port Type not found."));
        }

        [TestMethod]
        public void PortTypeAgainst_WithDataPortType_ShouldFail()
        {
            var powerPort = CreatePowerPort(1);
            powerPort.PowerPortInfo.PortType = new SdmObjectReference<PortType>(Guid.NewGuid().ToString());
            var dataPortType = new PortType
            {
                CategoryLinks =
                {
                    Categories = [SharedMappers.DomIds.SlcAsset_Management.Enums.CategoriesEnum.Data],
                },
            };
            var core = new PowerPortValidationCore(null);

            var result = core.ValidatePortTypeAgainst(powerPort, dataPortType);

            result.IsValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("Port Type must be a Power Port Type."));
        }

        [TestMethod]
        public void Collection_WithDuplicatePortNumber_ShouldFail()
        {
            var core = new PowerPortValidationCore(null);

            var result = core.ValidatePowerPortCollection(new List<PowerPort>
            {
                CreatePowerPort(1),
                CreatePowerPort(1),
            });

            result.IsValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("Duplicate Power Port number found: 1"));
        }

        private static PowerPort CreatePowerPort(long? portNumber)
        {
            return new PowerPort
            {
                Identifier = Guid.NewGuid().ToString(),
                Asset = AssetReference,
                PowerPortInfo =
                {
                    Name = "PSU",
                    PortNumber = portNumber,
                    OutputType = SharedMappers.DomIds.SlcAsset_Management.Enums.Outputtype.IO,
                },
            };
        }
    }
}
