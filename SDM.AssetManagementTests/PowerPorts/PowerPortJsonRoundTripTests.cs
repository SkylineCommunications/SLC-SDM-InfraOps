namespace SDM.AssetManagement.Tests.PowerPorts
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.InfraOps.Core.Converters;

    [TestClass]
    public class PowerPortJsonRoundTripTests
    {
        [TestMethod]
        public void PowerPort_RoundTrip_PreservesAllPropertiesAndReferences()
        {
            var powerPort = new PowerPort
            {
                Identifier = Guid.NewGuid().ToString(),
                Asset = new SdmObjectReference<Asset>(Guid.NewGuid().ToString()),
            };
            powerPort.PowerPortInfo.Name = "PSU 1";
            powerPort.PowerPortInfo.PortNumber = 3;
            powerPort.PowerPortInfo.OutputType = SlcAsset_Management.Enums.Outputtype.IO;
            powerPort.PowerPortInfo.PortExposure = SlcAsset_Management.Enums.PortExposureEnum.Front;
            powerPort.PowerPortInfo.PortType = new SdmObjectReference<PortType>(Guid.NewGuid().ToString());
            powerPort.PowerPortInfo.Label = "Label";

            string json = InfraOpsJsonConvert.SerializeObject(powerPort);
            var restored = InfraOpsJsonConvert.DeserializeObject<PowerPort>(json);

            restored.Identifier.Should().Be(powerPort.Identifier);
            restored.Asset.Should().Be(powerPort.Asset);
            restored.PowerPortInfo.PortType.Should().Be(powerPort.PowerPortInfo.PortType);
            restored.PowerPortInfo.OutputType.Should().Be(SlcAsset_Management.Enums.Outputtype.IO);
            restored.PowerPortInfo.PortExposure.Should().Be(SlcAsset_Management.Enums.PortExposureEnum.Front);
            restored.Should().Be(powerPort);
            InfraOpsJsonConvert.SerializeObject(restored).Should().Be(json);
        }

        [TestMethod]
        public void PowerPort_AuditFields_ExportedButNotImported()
        {
            var powerPort = new PowerPort
            {
                CreatedAt = new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc),
                CreatedBy = "creator",
                LastModified = new DateTime(2021, 6, 7, 8, 9, 10, DateTimeKind.Utc),
                LastModifiedBy = "modifier",
            };

            string json = InfraOpsJsonConvert.SerializeObject(powerPort);
            var restored = InfraOpsJsonConvert.DeserializeObject<PowerPort>(json);

            json.Should().Contain("\"CreatedBy\":\"creator\"");
            json.Should().Contain("\"LastModifiedBy\":\"modifier\"");
            restored.CreatedAt.Should().Be(default(DateTime));
            restored.CreatedBy.Should().BeNull();
            restored.LastModified.Should().Be(default(DateTime));
            restored.LastModifiedBy.Should().BeNull();
        }

        [TestMethod]
        public void PowerPort_Serialize_WritesReferencesAsStringsAndEnumsAsNames()
        {
            var assetId = Guid.NewGuid().ToString();
            var portTypeId = Guid.NewGuid().ToString();
            var powerPort = new PowerPort
            {
                Asset = new SdmObjectReference<Asset>(assetId),
            };
            powerPort.PowerPortInfo.PortType = new SdmObjectReference<PortType>(portTypeId);
            powerPort.PowerPortInfo.OutputType = SlcAsset_Management.Enums.Outputtype.IO;
            powerPort.PowerPortInfo.PortExposure = SlcAsset_Management.Enums.PortExposureEnum.Back;

            string json = InfraOpsJsonConvert.SerializeObject(powerPort);

            json.Should().Contain($"\"Asset\":\"{assetId}\"");
            json.Should().Contain($"\"PortType\":\"{portTypeId}\"");
            json.Should().Contain("\"OutputType\":\"IO\"");
            json.Should().Contain("\"PortExposure\":\"Back\"");
        }

        [TestMethod]
        public void PowerPort_Serialize_EmptyReferencesAreNull()
        {
            string json = InfraOpsJsonConvert.SerializeObject(new PowerPort());

            json.Should().Contain("\"Asset\":null");
            json.Should().Contain("\"PortType\":null");
        }

        [TestMethod]
        public void PowerPort_Deserialize_NullAndEmptyReferencesBecomeDefault()
        {
            var restored = InfraOpsJsonConvert.DeserializeObject<PowerPort>(
                "{\"Asset\":null,\"PowerPortInfo\":{\"PortType\":\"\"}}");

            restored.Asset.Identifier.Should().BeNullOrEmpty();
            restored.PowerPortInfo.PortType.Identifier.Should().BeNullOrEmpty();
        }
    }
}
