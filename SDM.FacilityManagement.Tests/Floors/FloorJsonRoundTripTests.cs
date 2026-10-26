namespace SDM.FacilityManagement.Tests.Floors
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SharedMappers.DomIds;

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
            InfraOpsJsonConvert.SerializeObject(restored).Should().Be(json);
        }

        [TestMethod]
        public void Floor_AuditFields_ExportedButNotImported()
        {
            var floor = new Floor
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = "Floor",
                CreatedAt = new DateTime(2023, 1, 2, 3, 4, 5, DateTimeKind.Utc),
                CreatedBy = "creator",
                LastModified = new DateTime(2024, 6, 7, 8, 9, 10, DateTimeKind.Utc),
                LastModifiedBy = "modifier",
                State = SlcFacility_Management.Behaviors.Floor_Behaviour.StatusesEnum.Active,
            };

            string json = InfraOpsJsonConvert.SerializeObject(floor);
            var restored = InfraOpsJsonConvert.DeserializeObject<Floor>(json);

            json.Should().Contain("\"CreatedBy\":\"creator\"");
            json.Should().Contain("\"LastModifiedBy\":\"modifier\"");
            restored.Name.Should().Be("Floor");
            restored.CreatedAt.Should().Be(default(DateTime));
            restored.CreatedBy.Should().BeNull();
            restored.LastModified.Should().Be(default(DateTime));
            restored.LastModifiedBy.Should().BeNull();
            restored.State.Should().Be(default(SlcFacility_Management.Behaviors.Floor_Behaviour.StatusesEnum));
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
