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
        }
    }
}
