namespace SDM.FacilityManagement.Tests.Rooms
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences;
    using Skyline.DataMiner.SDM.InfraOps.Core.Converters;
    using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

    [TestClass]
    public class RoomJsonRoundTripTests
    {
        [TestMethod]
        public void Room_RoundTrip_PreservesAllProperties()
        {
            var room = new Room
            {
                Identifier = Guid.NewGuid().ToString(),
                RoomId = "ROOM-1",
                Name = "Room",
                Plan = "plan.png",
                Description = "Server room",
                Width = 1200,
                Depth = 800,
            };
            room.Ownership.Team = new PnoObjectReference<Team>(Guid.NewGuid());
            room.Ownership.Owner = new PnoObjectReference<Person>(Guid.NewGuid());
            room.ResourceLink.ResourceId = Guid.NewGuid();
            room.FloorFk.Floor = new SdmObjectReference<Floor>(Guid.NewGuid().ToString());

            string json = InfraOpsJsonConvert.SerializeObject(room);
            var restored = InfraOpsJsonConvert.DeserializeObject<Room>(json);

            restored.Identifier.Should().Be(room.Identifier);
            restored.RoomId.Should().Be("ROOM-1");
            restored.Name.Should().Be("Room");
            restored.Plan.Should().Be("plan.png");
            restored.Description.Should().Be("Server room");
            restored.Width.Should().Be(1200);
            restored.Depth.Should().Be(800);
            restored.Ownership.Team.Should().Be(room.Ownership.Team);
            restored.Ownership.Owner.Should().Be(room.Ownership.Owner);
            restored.ResourceLink.ResourceId.Should().Be(room.ResourceLink.ResourceId);
            restored.FloorFk.Floor.Should().Be(room.FloorFk.Floor);
            restored.Should().Be(room);
            InfraOpsJsonConvert.SerializeObject(restored).Should().Be(json);
        }

        [TestMethod]
        public void Room_AuditFields_ExportedButNotImported()
        {
            var room = new Room
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = "Room",
                CreatedAt = new DateTime(2023, 1, 2, 3, 4, 5, DateTimeKind.Utc),
                CreatedBy = "creator",
                LastModified = new DateTime(2024, 6, 7, 8, 9, 10, DateTimeKind.Utc),
                LastModifiedBy = "modifier",
                State = SlcFacility_Management.Behaviors.Room_Behaviour.StatusesEnum.Active,
            };

            string json = InfraOpsJsonConvert.SerializeObject(room);
            var restored = InfraOpsJsonConvert.DeserializeObject<Room>(json);

            json.Should().Contain("\"CreatedBy\":\"creator\"");
            json.Should().Contain("\"LastModifiedBy\":\"modifier\"");
            restored.Name.Should().Be("Room");
            restored.CreatedAt.Should().Be(default(DateTime));
            restored.CreatedBy.Should().BeNull();
            restored.LastModified.Should().Be(default(DateTime));
            restored.LastModifiedBy.Should().BeNull();
            restored.State.Should().Be(default(SlcFacility_Management.Behaviors.Room_Behaviour.StatusesEnum));
        }

        [TestMethod]
        public void Room_Serialize_WritesReferencesAsStrings()
        {
            var floorId = Guid.NewGuid().ToString();
            var teamId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var room = new Room();
            room.FloorFk.Floor = new SdmObjectReference<Floor>(floorId);
            room.Ownership.Team = new PnoObjectReference<Team>(teamId);
            room.Ownership.Owner = new PnoObjectReference<Person>(ownerId);

            string json = InfraOpsJsonConvert.SerializeObject(room);

            json.Should().Contain($"\"Floor\":\"{floorId}\"");
            json.Should().Contain($"\"Team\":\"{teamId}\"");
            json.Should().Contain($"\"Owner\":\"{ownerId}\"");
        }

        [TestMethod]
        public void Room_Serialize_EmptyReferencesAreNull()
        {
            string json = InfraOpsJsonConvert.SerializeObject(new Room());

            json.Should().Contain("\"Floor\":null");
            json.Should().Contain("\"Team\":null");
            json.Should().Contain("\"Owner\":null");
        }

        [TestMethod]
        public void Room_Deserialize_NullAndEmptyReferencesBecomeDefault()
        {
            var restored = InfraOpsJsonConvert.DeserializeObject<Room>(
                "{\"FloorFk\":{\"Floor\":\"\"},\"Ownership\":{\"Team\":null,\"Owner\":null}}");

            restored.FloorFk.Floor.Identifier.Should().BeNullOrEmpty();
            restored.Ownership.Team.Identifier.Should().Be(Guid.Empty);
            restored.Ownership.Owner.Identifier.Should().Be(Guid.Empty);

            var fromNull = InfraOpsJsonConvert.DeserializeObject<Room>("{\"FloorFk\":{\"Floor\":null}}");

            fromNull.FloorFk.Floor.Identifier.Should().BeNullOrEmpty();
        }
    }
}
