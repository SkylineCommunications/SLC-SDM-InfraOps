namespace SDM.FacilityManagement.Tests.Desks
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM.InfraOps.Core.Converters;

    [TestClass]
    public class DeskJsonRoundTripTests
    {
        [TestMethod]
        public void Desk_RoundTrip_PreservesAllProperties()
        {
            var desk = new Desk
            {
                Identifier = Guid.NewGuid().ToString(),
                DeskID = "DESK-1",
                Name = "Desk",
                Plan = "plan.png",
                Description = "Window desk",
            };
            desk.RoomFk.Room = new SdmObjectReference<Room>(Guid.NewGuid().ToString());
            desk.Resource.ResourceId = Guid.NewGuid();

            string json = InfraOpsJsonConvert.SerializeObject(desk);
            var restored = InfraOpsJsonConvert.DeserializeObject<Desk>(json);

            restored.Identifier.Should().Be(desk.Identifier);
            restored.DeskID.Should().Be("DESK-1");
            restored.Name.Should().Be("Desk");
            restored.Plan.Should().Be("plan.png");
            restored.Description.Should().Be("Window desk");
            restored.RoomFk.Room.Should().Be(desk.RoomFk.Room);
            restored.Resource.ResourceId.Should().Be(desk.Resource.ResourceId);
            restored.Should().Be(desk);
        }

        [TestMethod]
        public void Desk_Serialize_WritesReferencesAsStrings()
        {
            var roomId = Guid.NewGuid().ToString();
            var desk = new Desk();
            desk.RoomFk.Room = new SdmObjectReference<Room>(roomId);

            string json = InfraOpsJsonConvert.SerializeObject(desk);

            json.Should().Contain($"\"Room\":\"{roomId}\"");
        }

        [TestMethod]
        public void Desk_Serialize_EmptyReferencesAreNull()
        {
            string json = InfraOpsJsonConvert.SerializeObject(new Desk());

            json.Should().Contain("\"Room\":null");
        }

        [TestMethod]
        public void Desk_Deserialize_NullAndEmptyReferencesBecomeDefault()
        {
            var fromNull = InfraOpsJsonConvert.DeserializeObject<Desk>("{\"RoomFk\":{\"Room\":null}}");
            var fromEmpty = InfraOpsJsonConvert.DeserializeObject<Desk>("{\"RoomFk\":{\"Room\":\"\"}}");

            fromNull.RoomFk.Room.Identifier.Should().BeNullOrEmpty();
            fromEmpty.RoomFk.Room.Identifier.Should().BeNullOrEmpty();
        }
    }
}
