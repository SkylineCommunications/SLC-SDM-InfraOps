namespace SDM.FacilityManagement.Tests.Floors
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM.InfraOps.Core.Converters;

    [TestClass]
    public class FloorJsonRoundTripTests
    {
        [TestMethod]
        public void Floor_RoundTrip_PreservesAllProperties()
        {
            var floor = new Floor
            {
                Identifier = Guid.NewGuid().ToString(),
                FloorId = "FLOOR-1",
                Name = "Floor",
                Plan = "plan.png",
                Description = "First floor",
            };
            floor.FacilityFk.Facility = new SdmObjectReference<Facility>(Guid.NewGuid().ToString());

            string json = InfraOpsJsonConvert.SerializeObject(floor);
            var restored = InfraOpsJsonConvert.DeserializeObject<Floor>(json);

            restored.Identifier.Should().Be(floor.Identifier);
            restored.FloorId.Should().Be("FLOOR-1");
            restored.Name.Should().Be("Floor");
            restored.Plan.Should().Be("plan.png");
            restored.Description.Should().Be("First floor");
            restored.FacilityFk.Facility.Should().Be(floor.FacilityFk.Facility);
            restored.Should().Be(floor);
        }

        [TestMethod]
        public void Floor_Serialize_WritesReferencesAsStrings()
        {
            var facilityId = Guid.NewGuid().ToString();
            var floor = new Floor();
            floor.FacilityFk.Facility = new SdmObjectReference<Facility>(facilityId);

            string json = InfraOpsJsonConvert.SerializeObject(floor);

            json.Should().Contain($"\"Facility\":\"{facilityId}\"");
        }

        [TestMethod]
        public void Floor_Serialize_EmptyReferencesAreNull()
        {
            string json = InfraOpsJsonConvert.SerializeObject(new Floor());

            json.Should().Contain("\"Facility\":null");
        }

        [TestMethod]
        public void Floor_Deserialize_NullAndEmptyReferencesBecomeDefault()
        {
            var fromNull = InfraOpsJsonConvert.DeserializeObject<Floor>("{\"FacilityFk\":{\"Facility\":null}}");
            var fromEmpty = InfraOpsJsonConvert.DeserializeObject<Floor>("{\"FacilityFk\":{\"Facility\":\"\"}}");

            fromNull.FacilityFk.Facility.Identifier.Should().BeNullOrEmpty();
            fromEmpty.FacilityFk.Facility.Identifier.Should().BeNullOrEmpty();
        }
    }
}
