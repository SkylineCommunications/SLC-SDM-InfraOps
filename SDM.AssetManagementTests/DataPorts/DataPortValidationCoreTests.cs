namespace SDM.AssetManagement.Tests.DataPorts
{
    using System;
    using System.Collections.Generic;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.AssetManagement.Validation;

    [TestClass]
    public class DataPortValidationCoreTests
    {
        private static readonly SdmObjectReference<Asset> AssetReference = new SdmObjectReference<Asset>(Guid.NewGuid().ToString());

        [TestMethod]
        public void PortTypeAgainst_WithEmptyPortType_ShouldFail()
        {
            var dataPort = CreateDataPort(1);
            var core = new DataPortValidationCore(null);

            var result = core.ValidatePortTypeAgainst(dataPort, null);

            result.IsValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("Port Type cannot be empty."));
        }

        [TestMethod]
        public void PortTypeAgainst_WithMissingPortType_ShouldFail()
        {
            var dataPort = CreateDataPort(1);
            dataPort.DataPortInfo.PortType = new SdmObjectReference<PortType>(Guid.NewGuid().ToString());
            var core = new DataPortValidationCore(null);

            var result = core.ValidatePortTypeAgainst(dataPort, null);

            result.IsValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("Port Type not found."));
        }

        [TestMethod]
        public void PortTypeAgainst_WithPowerPortType_ShouldFail()
        {
            var dataPort = CreateDataPort(1);
            dataPort.DataPortInfo.PortType = new SdmObjectReference<PortType>(Guid.NewGuid().ToString());
            var powerPortType = new PortType
            {
                CategoryLinks =
                {
                    Categories = [SharedMappers.DomIds.SlcAsset_Management.Enums.CategoriesEnum.Power],
                },
            };
            var core = new DataPortValidationCore(null);

            var result = core.ValidatePortTypeAgainst(dataPort, powerPortType);

            result.IsValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("Port Type must be a Data Port Type."));
        }

        [TestMethod]
        public void Collection_WithDuplicatePortNumber_ShouldFail()
        {
            var core = new DataPortValidationCore(null);

            var result = core.ValidateDataPortCollection(new List<DataPort>
            {
                CreateDataPort(1),
                CreateDataPort(1),
            });

            result.IsValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("Duplicate Data Port number found: 1"));
        }

        [TestMethod]
        public void Collection_WithMultiplePrimaryIpv4Ports_ShouldFail()
        {
            var core = new DataPortValidationCore(null);

            var result = core.ValidateDataPortCollection(new List<DataPort>
            {
                CreateDataPort(1, primaryIpv4: true),
                CreateDataPort(2, primaryIpv4: true),
            });

            result.IsValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("Only one Data Port can be marked as Primary IPv4."));
        }

        [TestMethod]
        public void Collection_WithMultiplePrimaryIpv6Ports_ShouldFail()
        {
            var core = new DataPortValidationCore(null);

            var result = core.ValidateDataPortCollection(new List<DataPort>
            {
                CreateDataPort(1, primaryIpv6: true),
                CreateDataPort(2, primaryIpv6: true),
            });

            result.IsValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("Only one Data Port can be marked as Primary IPv6."));
        }

        private static DataPort CreateDataPort(long? portNumber, bool primaryIpv4 = false, bool primaryIpv6 = false)
        {
            return new DataPort
            {
                Identifier = Guid.NewGuid().ToString(),
                Asset = AssetReference,
                DataPortInfo =
                {
                    Name = "ETH",
                    PortNumber = portNumber,
                    OutputType = SharedMappers.DomIds.SlcAsset_Management.Enums.Outputtype.IO,
                },
                PrimaryPortRelation =
                {
                    IsPrimaryIpv4 = primaryIpv4,
                    IsPrimaryIpv6 = primaryIpv6,
                },
            };
        }
    }
}
