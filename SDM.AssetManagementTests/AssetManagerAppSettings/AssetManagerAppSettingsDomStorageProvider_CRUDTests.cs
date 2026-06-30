namespace SDM.AssetManagement.Tests.AssetManagerAppSettings
{
    using System;
    using System.Linq;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using SDM.AssetManagement.Tests.Setup;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Extensions;

    [TestClass]
    public class AssetManagerAppSettingsDomStorageProvider_CRUDTests : BaseRepositoryTest
    {
        private AssetManagerAppSettings referenceSettings = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            referenceSettings = new AssetManagerAppSettings
            {
                Identifier = Guid.NewGuid().ToString(),
                EnableAssetHistory = true,
                PlanAndBuildJobPrompt = 1,
                EnableConnectionHistory = true,
                HistoryTTL = TimeSpan.FromDays(30),
                HistoryLimit = 1000L,
            };
        }

        [TestMethod]
        public void AssetManagerAppSettingsDomStorageProvider_EmptyDOM_Create()
        {
            Helper.AssetManagement.AppSettings.Create(referenceSettings);

            AssertCreated();
        }

        [TestMethod]
        public void AssetManagerAppSettingsDomStorageProvider_EmptyDOM_CreateOrUpdate_Create()
        {
            Helper.AssetManagement.AppSettings.CreateOrUpdate([referenceSettings]);

            AssertCreated();
        }

        [TestMethod]
        public void AssetManagerAppSettingsDomStorageProvider_EmptyDOM_CreateOrUpdate_Update()
        {
            Helper.AssetManagement.AppSettings.Create(referenceSettings);

            var updatedSettings = new AssetManagerAppSettings
            {
                Identifier = referenceSettings.Identifier,
                EnableAssetHistory = false,
                PlanAndBuildJobPrompt = referenceSettings.PlanAndBuildJobPrompt,
                EnableConnectionHistory = referenceSettings.EnableConnectionHistory,
                HistoryTTL = TimeSpan.FromDays(60),
                HistoryLimit = referenceSettings.HistoryLimit,
            };

            Helper.AssetManagement.AppSettings.CreateOrUpdate([updatedSettings]);

            var persistedSettings = Helper.AssetManagement.AppSettings.Read(AssetManagerAppSettingsExposers.Identifier.Equal(referenceSettings.Identifier)).Single();

            using (new AssertionScope())
            {
                persistedSettings.EnableAssetHistory.Should().BeFalse();
                persistedSettings.EnableAssetHistory.Should().NotBe(referenceSettings.EnableAssetHistory);
                persistedSettings.HistoryTTL.Should().Be(TimeSpan.FromDays(60));
                persistedSettings.HistoryTTL.Should().NotBe(referenceSettings.HistoryTTL!.Value);
                persistedSettings.PlanAndBuildJobPrompt.Should().Be(referenceSettings.PlanAndBuildJobPrompt);
                persistedSettings.EnableConnectionHistory.Should().Be(referenceSettings.EnableConnectionHistory);
                persistedSettings.HistoryLimit.Should().Be(referenceSettings.HistoryLimit);
            }
        }

        [TestMethod]
        public void AssetManagerAppSettingsDomStorageProvider_ReadPaged()
        {
            const int pageSize = 2;
            Helper.PopulateAssetManagerAppSettings();

            var allFilter = new TRUEFilterElement<AssetManagerAppSettings>();
            var pagedResult = Helper.AssetManagement.AppSettings.ReadPaged(allFilter, pageSize);
            var settingsCount = Helper.AssetManagement.AppSettings.Count(allFilter);

            using (new AssertionScope())
            {
                pagedResult.Should().NotBeNull();
                pagedResult.Should().HaveCount((int)(settingsCount / pageSize));
                pagedResult.Should().AllSatisfy(page => page.Should().HaveCount(pageSize));
            }
        }

        [TestMethod]
        public void AssetManagerAppSettingsDomStorageProvider_DeleteBulk()
        {
            Helper.PopulateAssetManagerAppSettings();

            var filter = AssetManagerAppSettingsExposers.EnableAssetHistory.Equal(false)
                .AND(AssetManagerAppSettingsExposers.PlanAndBuildJobPrompt.Equal(1));
            var settingsToDelete = Helper.AssetManagement.AppSettings.Read(filter).ToList();

            Helper.AssetManagement.AppSettings.Delete(settingsToDelete);

            using (new AssertionScope())
            {
                Helper.AssetManagement.AppSettings.Count(new TRUEFilterElement<AssetManagerAppSettings>()).Should().Be(DemoData.AssetManagerAppSettings.Count - settingsToDelete.Count);
                Helper.AssetManagement.AppSettings.Count(filter).Should().Be(0);
            }
        }

        [TestMethod]
        public void AssetManagerAppSettingsDomStorageProvider_EmptyDOM_DeleteSingle()
        {
            Helper.AssetManagement.AppSettings.Create(referenceSettings);

            var settingsToDelete = Helper.AssetManagement.AppSettings.Read(new TRUEFilterElement<AssetManagerAppSettings>()).Single();

            Helper.AssetManagement.AppSettings.Delete(settingsToDelete);

            using (new AssertionScope())
            {
                Helper.AssetManagement.AppSettings.Count(new TRUEFilterElement<AssetManagerAppSettings>()).Should().Be(0);
                Helper.AssetManagement.AppSettings.Count(AssetManagerAppSettingsExposers.Identifier.Equal(settingsToDelete.Identifier)).Should().Be(0);
            }
        }

        private void AssertCreated()
        {
            using (new AssertionScope())
            {
                Helper.AssetManagement.AppSettings.Count(new TRUEFilterElement<AssetManagerAppSettings>()).Should().Be(1);

                var createdSettings = Helper.AssetManagement.AppSettings.Read(new TRUEFilterElement<AssetManagerAppSettings>()).Single();
                createdSettings.EnableAssetHistory.Should().Be(referenceSettings.EnableAssetHistory);
                createdSettings.PlanAndBuildJobPrompt.Should().Be(referenceSettings.PlanAndBuildJobPrompt);
                createdSettings.EnableConnectionHistory.Should().Be(referenceSettings.EnableConnectionHistory);
                createdSettings.HistoryTTL.Should().Be(referenceSettings.HistoryTTL!.Value);
                createdSettings.HistoryLimit.Should().Be(referenceSettings.HistoryLimit!.Value);
            }
        }
    }
}
