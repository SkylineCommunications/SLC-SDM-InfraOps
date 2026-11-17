namespace SDM.FacilityManagement.Tests.Racks
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM.InfraOps.Core.Converters;

    [TestClass]
    public class RackJsonRoundTripTests
    {
        [TestMethod]
        public void Rack_RoundTrip_PreservesAllProperties()
        {
            var firstUpload = new DateTime(2024, 5, 1, 10, 30, 0, DateTimeKind.Utc);
            var secondUpload = new DateTime(2024, 6, 2, 11, 45, 0, DateTimeKind.Utc);
            var rack = new Rack
            {
                Identifier = Guid.NewGuid().ToString(),
                RackId = "RACK-1",
                Name = "Rack",
                Model = "Model X",
                Position = SlcFacility_Management.Enums.RackpositionenumEnum.Top,
                Width = 60.5,
                Height = 200.25,
                Depth = 100.75,
                Description = "Main rack",
                Bookable = true,
                CoolingFlow = SlcFacility_Management.Enums.CoolingflowenumEnum.SideToSide,
                XPosition = 10.5,
                YPosition = 20.5,
                Label = "R1",
                Color = "#FF0000",
                Orientation = SlcFacility_Management.Enums.Placementorientationenum.Horizontal,
            };
            rack.Capacity.MaximumRackCapacity = 42;
            rack.Capacity.MaximumPowerCapacity = 7.5;
            rack.RowFk.Row = new SdmObjectReference<Row>(Guid.NewGuid().ToString());
            rack.ZoneFk.Zone = new SdmObjectReference<Zone>(Guid.NewGuid().ToString());
            rack.Resource.ResourceId = Guid.NewGuid();
            rack.ImageDetails.Add(new ImageInfo { ImageFilePath = "front.png", UploadTimestamp = firstUpload });
            rack.ImageDetails.Add(new ImageInfo { ImageFilePath = "rear.png", UploadTimestamp = secondUpload });

            string json = InfraOpsJsonConvert.SerializeObject(rack);
            var restored = InfraOpsJsonConvert.DeserializeObject<Rack>(json);

            restored.Identifier.Should().Be(rack.Identifier);
            restored.RackId.Should().Be("RACK-1");
            restored.Name.Should().Be("Rack");
            restored.Model.Should().Be("Model X");
            restored.Position.Should().Be(SlcFacility_Management.Enums.RackpositionenumEnum.Top);
            restored.Width.Should().Be(60.5);
            restored.Height.Should().Be(200.25);
            restored.Depth.Should().Be(100.75);
            restored.Description.Should().Be("Main rack");
            restored.Bookable.Should().BeTrue();
            restored.CoolingFlow.Should().Be(SlcFacility_Management.Enums.CoolingflowenumEnum.SideToSide);
            restored.XPosition.Should().Be(10.5);
            restored.YPosition.Should().Be(20.5);
            restored.Label.Should().Be("R1");
            restored.Color.Should().Be("#FF0000");
            restored.Orientation.Should().Be(SlcFacility_Management.Enums.Placementorientationenum.Horizontal);
            restored.Capacity.MaximumRackCapacity.Should().Be(42);
            restored.Capacity.MaximumPowerCapacity.Should().Be(7.5);
            restored.RowFk.Row.Should().Be(rack.RowFk.Row);
            restored.ZoneFk.Zone.Should().Be(rack.ZoneFk.Zone);
            restored.Resource.ResourceId.Should().Be(rack.Resource.ResourceId);
            restored.ImageDetails.Should().HaveCount(2);
            restored.ImageDetails[0].ImageFilePath.Should().Be("front.png");
            restored.ImageDetails[0].UploadTimestamp.Should().Be(firstUpload);
            restored.ImageDetails[1].ImageFilePath.Should().Be("rear.png");
            restored.ImageDetails[1].UploadTimestamp.Should().Be(secondUpload);
            restored.Should().Be(rack);
            InfraOpsJsonConvert.SerializeObject(restored).Should().Be(json);
        }

        [TestMethod]
        public void Rack_AuditFields_ExportedButNotImported()
        {
            var rack = new Rack
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = "Rack",
                CreatedAt = new DateTime(2023, 1, 2, 3, 4, 5, DateTimeKind.Utc),
                CreatedBy = "creator",
                LastModified = new DateTime(2024, 6, 7, 8, 9, 10, DateTimeKind.Utc),
                LastModifiedBy = "modifier",
                State = SlcFacility_Management.Behaviors.Rack_Behaviour.StatusesEnum.Active,
            };

            string json = InfraOpsJsonConvert.SerializeObject(rack);
            var restored = InfraOpsJsonConvert.DeserializeObject<Rack>(json);

            json.Should().Contain("\"CreatedBy\":\"creator\"");
            json.Should().Contain("\"LastModifiedBy\":\"modifier\"");
            restored.Name.Should().Be("Rack");
            restored.CreatedAt.Should().Be(default(DateTime));
            restored.CreatedBy.Should().BeNull();
            restored.LastModified.Should().Be(default(DateTime));
            restored.LastModifiedBy.Should().BeNull();
            restored.State.Should().Be(default(SlcFacility_Management.Behaviors.Rack_Behaviour.StatusesEnum));
        }

        [TestMethod]
        public void Rack_Serialize_WritesReferencesAsStringsAndEnumsAsNames()
        {
            var rowId = Guid.NewGuid().ToString();
            var zoneId = Guid.NewGuid().ToString();
            var rack = new Rack
            {
                Position = SlcFacility_Management.Enums.RackpositionenumEnum.Bottom,
                CoolingFlow = SlcFacility_Management.Enums.CoolingflowenumEnum.FrontToRear,
                Orientation = SlcFacility_Management.Enums.Placementorientationenum.Vertical,
            };
            rack.RowFk.Row = new SdmObjectReference<Row>(rowId);
            rack.ZoneFk.Zone = new SdmObjectReference<Zone>(zoneId);

            string json = InfraOpsJsonConvert.SerializeObject(rack);

            json.Should().Contain($"\"Row\":\"{rowId}\"");
            json.Should().Contain($"\"Zone\":\"{zoneId}\"");
            json.Should().Contain("\"Position\":\"Bottom\"");
            json.Should().Contain("\"CoolingFlow\":\"FrontToRear\"");
            json.Should().Contain("\"Orientation\":\"Vertical\"");
        }

        [TestMethod]
        public void Rack_Serialize_EmptyReferencesAreNull()
        {
            string json = InfraOpsJsonConvert.SerializeObject(new Rack());

            json.Should().Contain("\"Row\":null");
            json.Should().Contain("\"Zone\":null");
        }

        [TestMethod]
        public void Rack_Deserialize_NullAndEmptyReferencesBecomeDefault()
        {
            var fromNull = InfraOpsJsonConvert.DeserializeObject<Rack>("{\"RowFk\":{\"Row\":null},\"ZoneFk\":{\"Zone\":null}}");
            var fromEmpty = InfraOpsJsonConvert.DeserializeObject<Rack>("{\"RowFk\":{\"Row\":\"\"},\"ZoneFk\":{\"Zone\":\"\"}}");

            fromNull.RowFk.Row.Identifier.Should().BeNullOrEmpty();
            fromNull.ZoneFk.Zone.Identifier.Should().BeNullOrEmpty();
            fromEmpty.RowFk.Row.Identifier.Should().BeNullOrEmpty();
            fromEmpty.ZoneFk.Zone.Identifier.Should().BeNullOrEmpty();
        }
    }
}
