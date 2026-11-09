namespace SDM.AssetManagement.Tests.Connections
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences;
    using Skyline.DataMiner.SDM.InfraOps.Core.Converters;

    [TestClass]
    public class ConnectionJsonRoundTripTests
    {
        [TestMethod]
        public void Connection_RoundTrip_PreservesAllPropertiesAndReferences()
        {
            var connection = new Connection
            {
                Identifier = Guid.NewGuid().ToString(),
                Notes = "Notes",
                Description = "Description",
                ConnectionType = SlcAsset_Management.Enums.ConnectionType.Power,
                CableType = new SdmObjectReference<CableType>(Guid.NewGuid().ToString()),
                CableLength = 2.5,
            };
            connection.Source.CableTag = "SRC";
            connection.Source.Port = new ISdmObjectReference<IPort>(Guid.NewGuid().ToString());
            connection.Source.PortType = new SdmObjectReference<PortType>(Guid.NewGuid().ToString());
            connection.Destination.CableTag = "DST";
            connection.Destination.Port = new ISdmObjectReference<IPort>(Guid.NewGuid().ToString());
            connection.Destination.PortType = new SdmObjectReference<PortType>(Guid.NewGuid().ToString());

            string json = InfraOpsJsonConvert.SerializeObject(connection);
            var restored = InfraOpsJsonConvert.DeserializeObject<Connection>(json);

            restored.Identifier.Should().Be(connection.Identifier);
            restored.CableType.Should().Be(connection.CableType);
            restored.ConnectionType.Should().Be(SlcAsset_Management.Enums.ConnectionType.Power);
            restored.Source.Port.Should().Be(connection.Source.Port);
            restored.Source.PortType.Should().Be(connection.Source.PortType);
            restored.Destination.Port.Should().Be(connection.Destination.Port);
            restored.Destination.PortType.Should().Be(connection.Destination.PortType);
            restored.Should().Be(connection);
            InfraOpsJsonConvert.SerializeObject(restored).Should().Be(json);
        }

        [TestMethod]
        public void Connection_AuditFields_ExportedButNotImported()
        {
            var connection = new Connection
            {
                CreatedAt = new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc),
                CreatedBy = "creator",
                LastModified = new DateTime(2021, 6, 7, 8, 9, 10, DateTimeKind.Utc),
                LastModifiedBy = "modifier",
            };

            string json = InfraOpsJsonConvert.SerializeObject(connection);
            var restored = InfraOpsJsonConvert.DeserializeObject<Connection>(json);

            json.Should().Contain("\"CreatedBy\":\"creator\"");
            json.Should().Contain("\"LastModifiedBy\":\"modifier\"");
            restored.CreatedAt.Should().Be(default(DateTime));
            restored.CreatedBy.Should().BeNull();
            restored.LastModified.Should().Be(default(DateTime));
            restored.LastModifiedBy.Should().BeNull();
        }

        [TestMethod]
        public void Connection_Serialize_WritesReferencesAsStringsAndEnumsAsNames()
        {
            var cableTypeId = Guid.NewGuid().ToString();
            var sourcePortId = Guid.NewGuid().ToString();
            var destinationPortTypeId = Guid.NewGuid().ToString();
            var connection = new Connection
            {
                ConnectionType = SlcAsset_Management.Enums.ConnectionType.Power,
                CableType = new SdmObjectReference<CableType>(cableTypeId),
            };
            connection.Source.Port = new ISdmObjectReference<IPort>(sourcePortId);
            connection.Destination.PortType = new SdmObjectReference<PortType>(destinationPortTypeId);

            string json = InfraOpsJsonConvert.SerializeObject(connection);

            json.Should().Contain($"\"CableType\":\"{cableTypeId}\"");
            json.Should().Contain($"\"Port\":\"{sourcePortId}\"");
            json.Should().Contain($"\"PortType\":\"{destinationPortTypeId}\"");
            json.Should().Contain("\"ConnectionType\":\"Power\"");
        }

        [TestMethod]
        public void Connection_Serialize_EmptyReferencesAreNull()
        {
            string json = InfraOpsJsonConvert.SerializeObject(new Connection());

            json.Should().Contain("\"CableType\":null");
            json.Should().Contain("\"Port\":null");
            json.Should().Contain("\"PortType\":null");
        }

        [TestMethod]
        public void Connection_Deserialize_NullAndEmptyReferencesBecomeDefault()
        {
            var restored = InfraOpsJsonConvert.DeserializeObject<Connection>(
                "{\"CableType\":\"\",\"Source\":{\"Port\":null,\"PortType\":\"\"},\"Destination\":{\"Port\":\"\",\"PortType\":null}}");

            restored.CableType.Identifier.Should().BeNullOrEmpty();
            restored.Source.Port.Identifier.Should().BeNullOrEmpty();
            restored.Source.PortType.Identifier.Should().BeNullOrEmpty();
            restored.Destination.Port.Identifier.Should().BeNullOrEmpty();
            restored.Destination.PortType.Identifier.Should().BeNullOrEmpty();
        }
    }
}
