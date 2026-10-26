namespace SDM.FacilityManagement.Tests.Zones
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM.InfraOps.Core.Converters;

    [TestClass]
    public class ZoneJsonRoundTripTests
    {
        [TestMethod]
        public void Zone_RoundTrip_PreservesAllProperties()
        {
            var zone = new Zone
            {
                Identifier = Guid.NewGuid().ToString(),
                ZoneId = "ZONE-1",
                Name = "Zone",
                Plan = "plan.png",
                Description = "Cold aisle",
                ThermalType = SlcFacility_Management.Enums.ThermalType.Cold,
                XPosition = 10.5,
                YPosition = 20.25,
                Width = 300.5,
                Depth = 150.75,
            };
            zone.ZoneCapacity.CoolingCapacity = 12.5;
            zone.RoomFk.Room = new SdmObjectReference<Room>(Guid.NewGuid().ToString());
            zone.Resource.ResourceId = Guid.NewGuid();

            string json = InfraOpsJsonConvert.SerializeObject(zone);
            var restored = InfraOpsJsonConvert.DeserializeObject<Zone>(json);

            restored.Identifier.Should().Be(zone.Identifier);
            restored.ZoneId.Should().Be("ZONE-1");
            restored.Name.Should().Be("Zone");
            restored.Plan.Should().Be("plan.png");
            restored.Description.Should().Be("Cold aisle");
            restored.ThermalType.Should().Be(SlcFacility_Management.Enums.ThermalType.Cold);
            restored.XPosition.Should().Be(10.5);
            restored.YPosition.Should().Be(20.25);
            restored.Width.Should().Be(300.5);
            restored.Depth.Should().Be(150.75);
            restored.ZoneCapacity.CoolingCapacity.Should().Be(12.5);
            restored.RoomFk.Room.Should().Be(zone.RoomFk.Room);
            restored.Resource.ResourceId.Should().Be(zone.Resource.ResourceId);
            restored.Should().Be(zone);
        }

        [TestMethod]
        public void Zone_Serialize_WritesReferencesAsStringsAndEnumsAsNames()
        {
            var roomId = Guid.NewGuid().ToString();
            var zone = new Zone
            {
                ThermalType = SlcFacility_Management.Enums.ThermalType.Warm,
            };
            zone.RoomFk.Room = new SdmObjectReference<Room>(roomId);

            string json = InfraOpsJsonConvert.SerializeObject(zone);

            json.Should().Contain($"\"Room\":\"{roomId}\"");
            json.Should().Contain("\"ThermalType\":\"Warm\"");
        }

        [TestMethod]
        public void Zone_Serialize_EmptyReferencesAreNull()
        {
            string json = InfraOpsJsonConvert.SerializeObject(new Zone());

            json.Should().Contain("\"Room\":null");
        }

        [TestMethod]
        public void Zone_Deserialize_NullAndEmptyReferencesBecomeDefault()
        {
            var fromNull = InfraOpsJsonConvert.DeserializeObject<Zone>("{\"RoomFk\":{\"Room\":null}}");
            var fromEmpty = InfraOpsJsonConvert.DeserializeObject<Zone>("{\"RoomFk\":{\"Room\":\"\"}}");

            fromNull.RoomFk.Room.Identifier.Should().BeNullOrEmpty();
            fromEmpty.RoomFk.Room.Identifier.Should().BeNullOrEmpty();
        }
    }
}
