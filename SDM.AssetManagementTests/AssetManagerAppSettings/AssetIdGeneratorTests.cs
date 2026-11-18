namespace SDM.AssetManagement.Tests.AssetManagerAppSettings
{
    using System;
    using System.Linq;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using SDM.AssetManagement.Tests.Setup;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Helpers;
    using Skyline.DataMiner.SDM.AssetManagement.Models;

    [TestClass]
    public class AssetIdGeneratorTests : BaseRepositoryTest
    {
        private static AssetManagerAppSettings CreateSettings(bool enabled, long next = 1, long increment = 1, long minDigits = 4)
        {
            return new AssetManagerAppSettings
            {
                Identifier = Guid.NewGuid().ToString(),
                EnableAutoAssetId = enabled,
                AssetIdNextSequence = next,
                AssetIdIncrement = increment,
                AssetIdMinimumDigits = minDigits,
            };
        }

        private AssetManagerAppSettings Reload(AssetManagerAppSettings settings)
        {
            return Helper.AssetManagement.AppSettings.Read(AssetManagerAppSettingsExposers.Identifier.Equal(settings.Identifier)).Single();
        }

        [TestMethod]
        public void AppSettings_AutoAssetIdFields_RoundTrip()
        {
            var settings = CreateSettings(true, 42, 5, 6);
            Helper.AssetManagement.AppSettings.Create(settings);

            var reloaded = Reload(settings);

            using (new AssertionScope())
            {
                reloaded.EnableAutoAssetId.Should().BeTrue();
                reloaded.AssetIdNextSequence.Should().Be(42);
                reloaded.AssetIdIncrement.Should().Be(5);
                reloaded.AssetIdMinimumDigits.Should().Be(6);
            }
        }

        [TestMethod]
        public void GenerateNextAssetId_Disabled_ReturnsNull()
        {
            Helper.AssetManagement.AppSettings.Create(CreateSettings(false));

            Helper.AssetManagement.GenerateNextAssetId("Switch").Should().BeNull();
            Helper.AssetManagement.IsAutoAssetIdEnabled().Should().BeFalse();
        }

        [TestMethod]
        public void GenerateNextAssetId_Enabled_ReturnsSequentialIdsAndPersistsCounter()
        {
            var settings = CreateSettings(true, 1, 2, 3);
            Helper.AssetManagement.AppSettings.Create(settings);

            var first = Helper.AssetManagement.GenerateNextAssetId("Network Switch");
            var second = Helper.AssetManagement.GenerateNextAssetId("Network Switch");

            using (new AssertionScope())
            {
                first.Should().Be("NetworkSwitch-001");
                second.Should().Be("NetworkSwitch-003");
                Reload(settings).AssetIdNextSequence.Should().Be(5);
            }
        }

        [TestMethod]
        public void GenerateNextAssetId_InvalidSettings_FallBackToDefaults()
        {
            Helper.AssetManagement.AppSettings.Create(CreateSettings(true, 0, 0, 0));

            Helper.AssetManagement.GenerateNextAssetId("Rack").            Should().Be("Rack-0001");
                    }

                    [TestMethod]
                    public void GenerateNextAssetId_EmptyClassName_Throws()
                    {
                        Helper.AssetManagement.AppSettings.Create(CreateSettings(true));

                        Helper.AssetManagement.Invoking(x => x.GenerateNextAssetId(" ")).Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void Compose_And_SanitizePrefix_BehaveAsExpected()
        {
            using (new AssertionScope())
            {
                AssetIdGenerator.SanitizePrefix("Power Dist. Unit/2").Should().Be("PowerDistUnit2");
                AssetIdGenerator.SanitizePrefix(null).Should().BeEmpty();
                AssetIdGenerator.Compose("Rack", 7, 4).Should().Be("Rack-0007");
            }
        }
    }
}
