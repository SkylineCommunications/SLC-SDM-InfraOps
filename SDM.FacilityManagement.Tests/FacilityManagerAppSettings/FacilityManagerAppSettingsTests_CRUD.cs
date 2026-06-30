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

    [TestClass]
    public partial class FacilityManagerAppSettingsTests : BaseRepositoryTest
    {
        private FacilityManagerAppSettings referenceSettings = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            referenceSettings = new FacilityManagerAppSettings
            {
                Identifier = Guid.NewGuid().ToString(),
                GoogleMapsAPIKey = "AIzaSyTest1234",
            };
        }

        [TestMethod]
        public void FacilityManagerAppSettings_EmptyDOM_Create()
        {
            Helper.AppSettings.Create(referenceSettings);

            AssertCreated();
        }

        [TestMethod]
        public void FacilityManagerAppSettings_EmptyDOM_CreateOrUpdate_Create()
        {
            Helper.AppSettings.CreateOrUpdate([referenceSettings]);

            AssertCreated();
        }

        [TestMethod]
        public void FacilityManagerAppSettings_EmptyDOM_CreateOrUpdate_Update()
        {
            Helper.AppSettings.Create(referenceSettings);

            var updatedSettings = new FacilityManagerAppSettings
            {
                Identifier = referenceSettings.Identifier,
                GoogleMapsAPIKey = "AIzaSyUpdated5678",
            };

            Helper.AppSettings.CreateOrUpdate([updatedSettings]);

            var persistedSettings = Helper.AppSettings.Read(FacilityManagerAppSettingsExposers.Identifier.Equal(referenceSettings.Identifier)).Single();

            using (new AssertionScope())
            {
                persistedSettings.GoogleMapsAPIKey.Should().Be("AIzaSyUpdated5678");
                persistedSettings.GoogleMapsAPIKey.Should().NotBe(referenceSettings.GoogleMapsAPIKey);
            }
        }

        [TestMethod]
        public void FacilityManagerAppSettings_EmptyDOM_DeleteSingle()
        {
            Helper.AppSettings.Create(referenceSettings);

            var settingsToDelete = Helper.AppSettings.Read(new TRUEFilterElement<FacilityManagerAppSettings>()).Single();

            Helper.AppSettings.Delete(settingsToDelete);

            using (new AssertionScope())
            {
                Helper.AppSettings.Count(new TRUEFilterElement<FacilityManagerAppSettings>()).Should().Be(0);
                Helper.AppSettings.Count(FacilityManagerAppSettingsExposers.Identifier.Equal(settingsToDelete.Identifier)).Should().Be(0);
            }
        }

        private void AssertCreated()
        {
            using (new AssertionScope())
            {
                Helper.AppSettings.Count(new TRUEFilterElement<FacilityManagerAppSettings>()).Should().Be(1);

                var createdSettings = Helper.AppSettings.Read(new TRUEFilterElement<FacilityManagerAppSettings>()).Single();
                createdSettings.GoogleMapsAPIKey.Should().Be(referenceSettings.GoogleMapsAPIKey);
            }
        }
    }
}
