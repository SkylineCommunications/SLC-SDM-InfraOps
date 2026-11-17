namespace SDM.FacilityManagement.Tests.Rows
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM.InfraOps.Core.Converters;

    [TestClass]
    public class RowJsonRoundTripTests
    {
        [TestMethod]
        public void Row_RoundTrip_PreservesAllProperties()
        {
            var row = new Row
            {
                Identifier = Guid.NewGuid().ToString(),
                RowId = "ROW-1",
                Name = "Row",
                Plan = "plan.png",
                Description = "Row A",
                Label = "A",
                YPosition = 42.5,
            };
            row.RoomFk.Room = new SdmObjectReference<Room>(Guid.NewGuid().ToString());
            row.Resource.ResourceId = Guid.NewGuid();

            string json = InfraOpsJsonConvert.SerializeObject(row);
            var restored = InfraOpsJsonConvert.DeserializeObject<Row>(json);

            restored.Identifier.Should().Be(row.Identifier);
            restored.RowId.Should().Be("ROW-1");
            restored.Name.Should().Be("Row");
            restored.Plan.Should().Be("plan.png");
            restored.Description.Should().Be("Row A");
            restored.Label.Should().Be("A");
            restored.YPosition.Should().Be(42.5);
            restored.RoomFk.Room.Should().Be(row.RoomFk.Room);
            restored.Resource.ResourceId.Should().Be(row.Resource.ResourceId);
            restored.Should().Be(row);
            InfraOpsJsonConvert.SerializeObject(restored).Should().Be(json);
        }

        [TestMethod]
        public void Row_AuditFields_ExportedButNotImported()
        {
            var row = new Row
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = "Row",
                CreatedAt = new DateTime(2023, 1, 2, 3, 4, 5, DateTimeKind.Utc),
                CreatedBy = "creator",
                LastModified = new DateTime(2024, 6, 7, 8, 9, 10, DateTimeKind.Utc),
                LastModifiedBy = "modifier",
                State = SlcFacility_Management.Behaviors.Row_Behaviour.StatusesEnum.Active,
            };

            string json = InfraOpsJsonConvert.SerializeObject(row);
            var restored = InfraOpsJsonConvert.DeserializeObject<Row>(json);

            json.Should().Contain("\"CreatedBy\":\"creator\"");
            json.Should().Contain("\"LastModifiedBy\":\"modifier\"");
            restored.Name.Should().Be("Row");
            restored.CreatedAt.Should().Be(default(DateTime));
            restored.CreatedBy.Should().BeNull();
            restored.LastModified.Should().Be(default(DateTime));
            restored.LastModifiedBy.Should().BeNull();
            restored.State.Should().Be(default(SlcFacility_Management.Behaviors.Row_Behaviour.StatusesEnum));
        }

        [TestMethod]
        public void Row_Serialize_WritesReferencesAsStrings()
        {
            var roomId = Guid.NewGuid().ToString();
            var row = new Row();
            row.RoomFk.Room = new SdmObjectReference<Room>(roomId);

            string json = InfraOpsJsonConvert.SerializeObject(row);

            json.Should().Contain($"\"Room\":\"{roomId}\"");
        }

        [TestMethod]
        public void Row_Serialize_EmptyReferencesAreNull()
        {
            string json = InfraOpsJsonConvert.SerializeObject(new Row());

            json.Should().Contain("\"Room\":null");
        }

        [TestMethod]
        public void Row_Deserialize_NullAndEmptyReferencesBecomeDefault()
        {
            var fromNull = InfraOpsJsonConvert.DeserializeObject<Row>("{\"RoomFk\":{\"Room\":null}}");
            var fromEmpty = InfraOpsJsonConvert.DeserializeObject<Row>("{\"RoomFk\":{\"Room\":\"\"}}");

            fromNull.RoomFk.Room.Identifier.Should().BeNullOrEmpty();
            fromEmpty.RoomFk.Room.Identifier.Should().BeNullOrEmpty();
        }
    }
}
