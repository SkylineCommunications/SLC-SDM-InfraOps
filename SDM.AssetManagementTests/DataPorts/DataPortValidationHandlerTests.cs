namespace SDM.AssetManagement.Tests.DataPorts
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Common.Validation;
    using Skyline.DataMiner.SDM.AssetManagement.Models;

    [TestClass]
    public class DataPortValidationHandlerTests
    {
        private static DataPort CreateValidDataPort()
        {
            return new DataPort
            {
                DataPortInfo = new DataPortInfo
                {
                    Name = "ETH-1",
                    PortNumber = 1,
                    OutputType = SharedMappers.DomIds.SlcAsset_Management.Enums.Outputtype.IO,
                },
                Asset = new SdmObjectReference<Asset>(Guid.NewGuid().ToString()),
            };
        }

        [TestMethod]
        public void MandatoryFields_WithNullDataPort_ShouldFail()
        {
            var isValid = DataPortValidationHandler.AreMandatoryFieldsValid(null, out var result);

            isValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("DataPort cannot be null."));
        }

        [TestMethod]
        public void MandatoryFields_WithEmptyName_ShouldFail()
        {
            var dataPort = CreateValidDataPort();
            dataPort.DataPortInfo.Name = " ";

            var isValid = DataPortValidationHandler.AreMandatoryFieldsValid(dataPort, out var result);

            isValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("DataPort Name cannot be empty."));
        }

        [TestMethod]
        public void MandatoryFields_WithMissingPortNumber_ShouldFail()
        {
            var dataPort = CreateValidDataPort();
            dataPort.DataPortInfo.PortNumber = null;

            var isValid = DataPortValidationHandler.AreMandatoryFieldsValid(dataPort, out var result);

            isValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("DataPort Number must be provided."));
        }

        [TestMethod]
        public void MandatoryFields_WithNegativePortNumber_ShouldFail()
        {
            var dataPort = CreateValidDataPort();
            dataPort.DataPortInfo.PortNumber = -1;

            var isValid = DataPortValidationHandler.AreMandatoryFieldsValid(dataPort, out var result);

            isValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("DataPort Number cannot be negative. Found: -1"));
        }

        [TestMethod]
        public void MandatoryFields_WithMissingOutputType_ShouldFail()
        {
            var dataPort = CreateValidDataPort();
            dataPort.DataPortInfo.OutputType = null;

            var isValid = DataPortValidationHandler.AreMandatoryFieldsValid(dataPort, out var result);

            isValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("DataPort Output Type must be provided."));
        }

        [TestMethod]
        public void AssetLink_WithMissingAsset_ShouldFail()
        {
            var dataPort = CreateValidDataPort();
            dataPort.Asset = null;

            var isValid = DataPortValidationHandler.IsAssetLinkValid(dataPort, out var result);

            isValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("DataPort must be linked to an Asset."));
        }

        [TestMethod]
        public void AddressInfo_WithInvalidIpv4Address_ShouldFail()
        {
            var dataPort = CreateValidDataPort();
            dataPort.AddressInfo = new AddressInfo { Ipv4Address = "not-an-ipv4-address" };

            var isValid = DataPortValidationHandler.IsAddressInfoValid(dataPort, out var result);

            isValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("Invalid IPv4 address format"));
        }

        [TestMethod]
        public void AddressInfo_WithInvalidIpv6Address_ShouldFail()
        {
            var dataPort = CreateValidDataPort();
            dataPort.AddressInfo = new AddressInfo { Ipv6Address = "not-an-ipv6-address" };

            var isValid = DataPortValidationHandler.IsAddressInfoValid(dataPort, out var result);

            isValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("Invalid IPv6 address format"));
        }

        [TestMethod]
        public void AddressInfo_WithPrimaryIpv4ButNoIpv4Address_ShouldFail()
        {
            var dataPort = CreateValidDataPort();
            dataPort.PrimaryPortRelation = new PrimaryPortRelation { IsPrimaryIpv4 = true };

            var isValid = DataPortValidationHandler.IsAddressInfoValid(dataPort, out var result);

            isValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("DataPort marked as Primary IPv4 must have an IPv4 address."));
        }

        [TestMethod]
        public void AddressInfo_WithPrimaryIpv6ButNoIpv6Address_ShouldFail()
        {
            var dataPort = CreateValidDataPort();
            dataPort.PrimaryPortRelation = new PrimaryPortRelation { IsPrimaryIpv6 = true };

            var isValid = DataPortValidationHandler.IsAddressInfoValid(dataPort, out var result);

            isValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("DataPort marked as Primary IPv6 must have an IPv6 address."));
        }
    }
}
