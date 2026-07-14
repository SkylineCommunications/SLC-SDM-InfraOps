namespace SDM.FacilityManagement.Tests.FacilityManagerAppSettings
{
    using System;
    using System.Linq;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using SDM.FacilityManagement.Tests.Setup;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;

    public partial class FacilityManagerAppSettingsTests : BaseRepositoryTest
    {
        [TestMethod]
        public void FacilityManagerAppSettingsTests_ReadFilter_GoogleMapsAPIKey_Equal()
        {
            Helper.PopulateFacilityManagerAppSettings();

            const string apiKey = "key1";
            var filter = FacilityManagerAppSettingsExposers.AppSettings.GoogleMapsAPIKey.Equal(apiKey);
            var expected = DemoData.FacilityManagerAppSettings.Single(s => s.GoogleMapsAPIKey == apiKey);

            var settingsRetrieved = Helper.AppSettings.Read(filter);

            using (new AssertionScope())
            {
                settingsRetrieved.Should().HaveCount(1);
                settingsRetrieved.First().Should().BeEquivalentTo(expected);
            }
        }

        [TestMethod]
        public void FacilityManagerAppSettingsTests_ReadFilter_GoogleMapsAPIKey_Contains()
        {
            Helper.PopulateFacilityManagerAppSettings();

            const string searchTerm = "key";
            var filter = FacilityManagerAppSettingsExposers.AppSettings.GoogleMapsAPIKey.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
            var expected = DemoData.FacilityManagerAppSettings
                .Where(s => s.GoogleMapsAPIKey != null && s.GoogleMapsAPIKey.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            var settingsRetrieved = Helper.AppSettings.Read(filter);

            using (new AssertionScope())
            {
                settingsRetrieved.Should().HaveCount(expected.Length);
                settingsRetrieved.Should().BeEquivalentTo(expected);
            }
        }

        [TestMethod]
        public void FacilityManagerAppSettingsTests_ReadFilter_Identifier_Equal()
        {
            Helper.PopulateFacilityManagerAppSettings();

            var identifier = DemoData.FacilityManagerAppSettings[1].Identifier;
            var filter = FacilityManagerAppSettingsExposers.Identifier.Equal(identifier);
            var expected = DemoData.FacilityManagerAppSettings.Single(s => s.Identifier == identifier);

            var settingsRetrieved = Helper.AppSettings.Read(filter);

            using (new AssertionScope())
            {
                settingsRetrieved.Should().HaveCount(1);
                settingsRetrieved.First().Should().BeEquivalentTo(expected);
            }
        }
    }
}
