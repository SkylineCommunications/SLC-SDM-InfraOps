namespace SDM.FacilityManagement.Tests.FacilityManagerAppSettings
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM.InfraOps.Core.Converters;

    [TestClass]
    public class FacilityManagerAppSettingsJsonRoundTripTests
    {
        [TestMethod]
        public void FacilityManagerAppSettings_RoundTrip_PreservesAllProperties()
        {
            var settings = new FacilityManagerAppSettings
            {
                Identifier = Guid.NewGuid().ToString(),
                GoogleMapsAPIKey = "api-key-123",
            };

            string json = InfraOpsJsonConvert.SerializeObject(settings);
            var restored = InfraOpsJsonConvert.DeserializeObject<FacilityManagerAppSettings>(json);

            restored.Identifier.Should().Be(settings.Identifier);
            restored.GoogleMapsAPIKey.Should().Be("api-key-123");
            restored.Should().Be(settings);
            InfraOpsJsonConvert.SerializeObject(restored).Should().Be(json);
        }

        [TestMethod]
        public void FacilityManagerAppSettings_AuditFields_ExportedButNotImported()
        {
            var settings = new FacilityManagerAppSettings
            {
                Identifier = Guid.NewGuid().ToString(),
                GoogleMapsAPIKey = "api-key-123",
                CreatedAt = new DateTime(2023, 1, 2, 3, 4, 5, DateTimeKind.Utc),
                CreatedBy = "creator",
                LastModified = new DateTime(2024, 6, 7, 8, 9, 10, DateTimeKind.Utc),
                LastModifiedBy = "modifier",
            };

            string json = InfraOpsJsonConvert.SerializeObject(settings);
            var restored = InfraOpsJsonConvert.DeserializeObject<FacilityManagerAppSettings>(json);

            json.Should().Contain("\"CreatedBy\":\"creator\"");
            json.Should().Contain("\"LastModifiedBy\":\"modifier\"");
            restored.GoogleMapsAPIKey.Should().Be("api-key-123");
            restored.CreatedAt.Should().Be(default(DateTime));
            restored.CreatedBy.Should().BeNull();
            restored.LastModified.Should().Be(default(DateTime));
            restored.LastModifiedBy.Should().BeNull();
        }
    }
}
