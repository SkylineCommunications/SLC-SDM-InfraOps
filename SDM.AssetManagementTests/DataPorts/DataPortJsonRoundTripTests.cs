namespace SDM.AssetManagement.Tests.DataPorts
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.InfraOps.Core.Converters;

    [TestClass]
    public class DataPortJsonRoundTripTests
    {
        [TestMethod]
        public void DataPort_RoundTrip_PreservesAllPropertiesAndReferences()
        {
            var dataPort = new DataPort
            {
                Identifier = Guid.NewGuid().ToString(),
                Asset = new SdmObjectReference<Asset>(Guid.NewGuid().ToString()),
            };
            dataPort.DataPortInfo.Name = "Port 1";
            dataPort.DataPortInfo.PortNumber = 7;
            dataPort.DataPortInfo.OutputType = SlcAsset_Management.Enums.Outputtype.IO;
            dataPort.DataPortInfo.PortExposure = SlcAsset_Management.Enums.PortExposureEnum.Front;
            dataPort.DataPortInfo.PortType = new SdmObjectReference<PortType>(Guid.NewGuid().ToString());
            dataPort.DataPortInfo.Label = "Label";
            dataPort.AddressInfo.Ipv4Address = "10.0.0.1";
            dataPort.AddressInfo.Ipv6Address = "fe80::1";
            dataPort.AddressInfo.Hostname = "host";
            dataPort.AddressInfo.DNS = true;
            dataPort.PrimaryPortRelation.IsPrimaryIpv4 = true;
            dataPort.PrimaryPortRelation.IsPrimaryIpv6 = true;

            string json = InfraOpsJsonConvert.SerializeObject(dataPort);
            var restored = InfraOpsJsonConvert.DeserializeObject<DataPort>(json);

            restored.Identifier.Should().Be(dataPort.Identifier);
            restored.Asset.Should().Be(dataPort.Asset);
            restored.DataPortInfo.PortType.Should().Be(dataPort.DataPortInfo.PortType);
            restored.DataPortInfo.OutputType.Should().Be(SlcAsset_Management.Enums.Outputtype.IO);
            restored.DataPortInfo.PortExposure.Should().Be(SlcAsset_Management.Enums.PortExposureEnum.Front);
            restored.AddressInfo.DNS.Should().BeTrue();
            restored.Should().Be(dataPort);
            InfraOpsJsonConvert.SerializeObject(restored).Should().Be(json);
        }

        [TestMethod]
        public void DataPort_AuditFields_ExportedButNotImported()
        {
            var dataPort = new DataPort
            {
                CreatedAt = new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc),
                CreatedBy = "creator",
                LastModified = new DateTime(2021, 6, 7, 8, 9, 10, DateTimeKind.Utc),
                LastModifiedBy = "modifier",
            };

            string json = InfraOpsJsonConvert.SerializeObject(dataPort);
            var restored = InfraOpsJsonConvert.DeserializeObject<DataPort>(json);

            json.Should().Contain("\"CreatedBy\":\"creator\"");
            json.Should().Contain("\"LastModifiedBy\":\"modifier\"");
            restored.CreatedAt.Should().Be(default(DateTime));
            restored.CreatedBy.Should().BeNull();
            restored.LastModified.Should().Be(default(DateTime));
            restored.LastModifiedBy.Should().BeNull();
        }

        [TestMethod]
        public void DataPort_Serialize_WritesReferencesAsStringsAndEnumsAsNames()
        {
            var assetId = Guid.NewGuid().ToString();
            var portTypeId = Guid.NewGuid().ToString();
            var dataPort = new DataPort
            {
                Asset = new SdmObjectReference<Asset>(assetId),
            };
            dataPort.DataPortInfo.PortType = new SdmObjectReference<PortType>(portTypeId);
            dataPort.DataPortInfo.OutputType = SlcAsset_Management.Enums.Outputtype.In;
            dataPort.DataPortInfo.PortExposure = SlcAsset_Management.Enums.PortExposureEnum.Back;

            string json = InfraOpsJsonConvert.SerializeObject(dataPort);

            json.Should().Contain($"\"Asset\":\"{assetId}\"");
            json.Should().Contain($"\"PortType\":\"{portTypeId}\"");
            json.Should().Contain("\"OutputType\":\"In\"");
            json.Should().Contain("\"PortExposure\":\"Back\"");
        }

        [TestMethod]
        public void DataPort_Serialize_EmptyReferencesAreNull()
        {
            string json = InfraOpsJsonConvert.SerializeObject(new DataPort());

            json.Should().Contain("\"Asset\":null");
            json.Should().Contain("\"PortType\":null");
        }

        [TestMethod]
        public void DataPort_Deserialize_NullAndEmptyReferencesBecomeDefault()
        {
            var restored = InfraOpsJsonConvert.DeserializeObject<DataPort>(
                "{\"Asset\":\"\",\"DataPortInfo\":{\"PortType\":null}}");

            restored.Asset.Identifier.Should().BeNullOrEmpty();
            restored.DataPortInfo.PortType.Identifier.Should().BeNullOrEmpty();
        }
    }
}
