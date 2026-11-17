namespace SDM.AssetManagement.Tests.InfraopsReservations
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences;
    using Skyline.DataMiner.SDM.InfraOps.Core.Converters;

    [TestClass]
    public class InfraopsReservationJsonRoundTripTests
    {
        [TestMethod]
        public void InfraopsReservation_RoundTrip_PreservesAllPropertiesAndReferences()
        {
            var reservation = new InfraopsReservation
            {
                Identifier = Guid.NewGuid().ToString(),
                Description = "Reservation",
            };
            reservation.JobFk.Job = new ISdmObjectReference<ISdmObject>(Guid.NewGuid().ToString());
            reservation.RackFk.Rack = new SdmObjectReference<Rack>(Guid.NewGuid().ToString());
            reservation.ReservedPositions.Add(new InfraopsReservationBounderies { LowerBound = 1, UpperBound = 4 });
            reservation.ReservedPositions.Add(new InfraopsReservationBounderies { LowerBound = 10, UpperBound = 12 });

            string json = InfraOpsJsonConvert.SerializeObject(reservation);
            var restored = InfraOpsJsonConvert.DeserializeObject<InfraopsReservation>(json);

            restored.Identifier.Should().Be(reservation.Identifier);
            restored.JobFk.Job.Should().Be(reservation.JobFk.Job);
            restored.RackFk.Rack.Should().Be(reservation.RackFk.Rack);
            restored.ReservedPositions.Should().HaveCount(2);
            restored.ReservedPositions[1].UpperBound.Should().Be(12);
            restored.Should().Be(reservation);
            InfraOpsJsonConvert.SerializeObject(restored).Should().Be(json);
        }

        [TestMethod]
        public void InfraopsReservation_AuditFields_ExportedButNotImported()
        {
            var reservation = new InfraopsReservation
            {
                CreatedAt = new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc),
                CreatedBy = "creator",
                LastModified = new DateTime(2021, 6, 7, 8, 9, 10, DateTimeKind.Utc),
                LastModifiedBy = "modifier",
            };

            string json = InfraOpsJsonConvert.SerializeObject(reservation);
            var restored = InfraOpsJsonConvert.DeserializeObject<InfraopsReservation>(json);

            json.Should().Contain("\"CreatedBy\":\"creator\"");
            json.Should().Contain("\"LastModifiedBy\":\"modifier\"");
            restored.CreatedAt.Should().Be(default(DateTime));
            restored.CreatedBy.Should().BeNull();
            restored.LastModified.Should().Be(default(DateTime));
            restored.LastModifiedBy.Should().BeNull();
        }

        [TestMethod]
        public void InfraopsReservation_Serialize_WritesReferencesAsStrings()
        {
            var jobId = Guid.NewGuid().ToString();
            var rackId = Guid.NewGuid().ToString();
            var reservation = new InfraopsReservation();
            reservation.JobFk.Job = new ISdmObjectReference<ISdmObject>(jobId);
            reservation.RackFk.Rack = new SdmObjectReference<Rack>(rackId);

            string json = InfraOpsJsonConvert.SerializeObject(reservation);

            json.Should().Contain($"\"Job\":\"{jobId}\"");
            json.Should().Contain($"\"Rack\":\"{rackId}\"");
        }

        [TestMethod]
        public void InfraopsReservation_Serialize_EmptyReferencesAreNull()
        {
            string json = InfraOpsJsonConvert.SerializeObject(new InfraopsReservation());

            json.Should().Contain("\"Job\":null");
            json.Should().Contain("\"Rack\":null");
        }

        [TestMethod]
        public void InfraopsReservation_Deserialize_NullAndEmptyReferencesBecomeDefault()
        {
            var restored = InfraOpsJsonConvert.DeserializeObject<InfraopsReservation>(
                "{\"JobFk\":{\"Job\":null},\"RackFk\":{\"Rack\":\"\"}}");

            restored.JobFk.Job.Identifier.Should().BeNullOrEmpty();
            restored.RackFk.Rack.Identifier.Should().BeNullOrEmpty();
        }
    }
}
