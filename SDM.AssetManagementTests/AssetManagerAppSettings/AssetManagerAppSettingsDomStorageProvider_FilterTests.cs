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
    public class AssetManagerAppSettingsDomStorageProvider_FilterTests : BaseRepositoryTest
    {
        [TestMethod]
        public void AssetManagerAppSettings_ReadFilter_EnableAssetHistory_True()
        {
            Helper.PopulateAssetManagerAppSettings();

            var filter = AssetManagerAppSettingsExposers.EnableAssetHistory.Equal(true);
            var expected = DemoData.AssetManagerAppSettings.Where(s => s.EnableAssetHistory).ToArray();

            var settingsRetrieved = Helper.AssetManagement.AppSettings.Read(filter);

            using (new AssertionScope())
            {
                settingsRetrieved.Should().HaveCount(expected.Length);
                settingsRetrieved.Should().BeEquivalentTo(expected);
            }
        }

        [TestMethod]
        public void AssetManagerAppSettings_ReadFilter_EnableAssetHistory_False()
        {
            Helper.PopulateAssetManagerAppSettings();

            var filter = AssetManagerAppSettingsExposers.EnableAssetHistory.Equal(false);
            var expected = DemoData.AssetManagerAppSettings.Where(s => !s.EnableAssetHistory).ToArray();

            var settingsRetrieved = Helper.AssetManagement.AppSettings.Read(filter);

            using (new AssertionScope())
            {
                settingsRetrieved.Should().HaveCount(expected.Length);
                settingsRetrieved.Should().BeEquivalentTo(expected);
            }
        }

        [TestMethod]
        public void AssetManagerAppSettings_ReadFilter_PlanAndBuildJobPrompt_Equal()
        {
            Helper.PopulateAssetManagerAppSettings();

            const int promptValue = 1;
            var filter = AssetManagerAppSettingsExposers.PlanAndBuildJobPrompt.Equal(promptValue);
            var expected = DemoData.AssetManagerAppSettings.Where(s => s.PlanAndBuildJobPrompt == promptValue).ToArray();

            var settingsRetrieved = Helper.AssetManagement.AppSettings.Read(filter);

            using (new AssertionScope())
            {
                settingsRetrieved.Should().HaveCount(expected.Length);
                settingsRetrieved.Should().BeEquivalentTo(expected);
            }
        }

        [TestMethod]
        public void AssetManagerAppSettings_ReadFilter_EnableConnectionHistory_True()
        {
            Helper.PopulateAssetManagerAppSettings();

            var filter = AssetManagerAppSettingsExposers.EnableConnectionHistory.Equal(true);
            var expected = DemoData.AssetManagerAppSettings.Where(s => s.EnableConnectionHistory).ToArray();

            var settingsRetrieved = Helper.AssetManagement.AppSettings.Read(filter);

            using (new AssertionScope())
            {
                settingsRetrieved.Should().HaveCount(expected.Length);
                settingsRetrieved.Should().BeEquivalentTo(expected);
            }
        }

        [TestMethod]
        public void AssetManagerAppSettings_ReadFilter_Identifier_Equal()
        {
            Helper.PopulateAssetManagerAppSettings();

            var identifier = DemoData.AssetManagerAppSettings[2].Identifier;
            var filter = AssetManagerAppSettingsExposers.Identifier.Equal(identifier);
            var expected = DemoData.AssetManagerAppSettings.Single(s => s.Identifier == identifier);

            var settingsRetrieved = Helper.AssetManagement.AppSettings.Read(filter);

            using (new AssertionScope())
            {
                settingsRetrieved.Should().HaveCount(1);
                settingsRetrieved.First().Should().BeEquivalentTo(expected);
            }
        }

        [TestMethod]
        public void AssetManagerAppSettings_ReadFilter_EnableAssetHistoryAndPrompt_Combined()
        {
            Helper.PopulateAssetManagerAppSettings();

            var filter = AssetManagerAppSettingsExposers.EnableAssetHistory.Equal(true)
                .AND(AssetManagerAppSettingsExposers.PlanAndBuildJobPrompt.Equal(1));
            var expected = DemoData.AssetManagerAppSettings
                .Where(s => s.EnableAssetHistory && s.PlanAndBuildJobPrompt == 1)
                .ToArray();

            var settingsRetrieved = Helper.AssetManagement.AppSettings.Read(filter);

            using (new AssertionScope())
            {
                settingsRetrieved.Should().HaveCount(expected.Length);
                settingsRetrieved.Should().BeEquivalentTo(expected);
            }
        }
    }
}
