namespace SDM.AssetManagement.Tests.AssetManagerAppSettings
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.InfraOps.Core.Converters;

    [TestClass]
    public class AssetManagerAppSettingsJsonRoundTripTests
    {
        [TestMethod]
        public void AssetManagerAppSettings_RoundTrip_PreservesAllProperties()
        {
            var settings = new AssetManagerAppSettings
            {
                Identifier = Guid.NewGuid().ToString(),
                EnableAssetHistory = true,
                PlanAndBuildJobPrompt = SlcAsset_Management.Enums.Planandbuildjobpromptenum.Enabled,
                EnableConnectionHistory = true,
                HistoryTTL = new TimeSpan(30, 2, 3, 4),
                HistoryLimit = 500,
                EnableResourceLink = true,
            };

            string json = InfraOpsJsonConvert.SerializeObject(settings);
            var restored = InfraOpsJsonConvert.DeserializeObject<AssetManagerAppSettings>(json);

            restored.Identifier.Should().Be(settings.Identifier);
            restored.PlanAndBuildJobPrompt.Should().Be(SlcAsset_Management.Enums.Planandbuildjobpromptenum.Enabled);
            restored.HistoryTTL.Should().Be(new TimeSpan(30, 2, 3, 4));
            restored.HistoryLimit.Should().Be(500);
            restored.Should().Be(settings);
        }

        [TestMethod]
        public void AssetManagerAppSettings_Serialize_WritesEnumsAsNames()
        {
            var settings = new AssetManagerAppSettings
            {
                PlanAndBuildJobPrompt = SlcAsset_Management.Enums.Planandbuildjobpromptenum.Enabled,
            };

            string json = InfraOpsJsonConvert.SerializeObject(settings);

            json.Should().Contain("\"PlanAndBuildJobPrompt\":\"Enabled\"");
        }
    }
}
