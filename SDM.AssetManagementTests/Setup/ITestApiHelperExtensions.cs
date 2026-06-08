using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharedMappers.DomIds;

using Skyline.DataMiner.SDM.AssetManagement.Validation;
using Skyline.DataMiner.SDM.Common.Services;

namespace SDM.AssetManagement.Tests.Setup
{
    public static class ITestApiHelperExtensions
    {
        #region Validator Helpers

        /// <summary>
        /// Creates an AssetValidator from the test helper repositories.
        /// Convenient for test validation scenarios.
        /// </summary>
        public static AssetValidator CreateAssetValidator(this ITestApiHelper helper)
        {
            if (helper == null)
            {
                throw new ArgumentNullException(nameof(helper));
            }

            var entityLoader = new SdmEntityLoader(
                assetRepository: helper.AssetManagement.Assets,
                assetClassRepository: helper.AssetManagement.AssetClasses,
                deviceTypeRepository: helper.AssetManagement.DeviceTypes,
                dataPortRepository: helper.AssetManagement.DataPorts,
                powerPortRepository: helper.AssetManagement.PowerPorts,
                rackRepository: helper.FacilityManagement.Racks,
                reservationRepository: null,
                portTypeRepository: null
            );

            return new AssetValidator(entityLoader);
        }

        /// <summary>
        /// Creates an AssetClassValidator from the test helper repositories.
        /// Convenient for test validation scenarios.
        /// </summary>
        public static AssetClassValidator CreateAssetClassValidator(this ITestApiHelper helper)
        {
            if (helper == null)
            {
                throw new ArgumentNullException(nameof(helper));
            }

            var entityLoader = new SdmEntityLoader(
                assetRepository: helper.AssetManagement.Assets,
                assetClassRepository: helper.AssetManagement.AssetClasses,
                deviceTypeRepository: helper.AssetManagement.DeviceTypes,
                dataPortRepository: helper.AssetManagement.DataPorts,
                powerPortRepository: helper.AssetManagement.PowerPorts,
                rackRepository: helper.FacilityManagement.Racks,
                reservationRepository: null,
                portTypeRepository: null
            );

            return new AssetClassValidator(entityLoader);
        }

        /// <summary>
        /// Creates an SdmEntityLoader from the test helper repositories.
        /// Use this if you need the entity loader directly for custom validators.
        /// </summary>
        public static SdmEntityLoader CreateEntityLoader(this ITestApiHelper helper)
        {
            if (helper == null)
            {
                throw new ArgumentNullException(nameof(helper));
            }

            return new SdmEntityLoader(
                assetRepository: helper.AssetManagement.Assets,
                assetClassRepository: helper.AssetManagement.AssetClasses,
                deviceTypeRepository: helper.AssetManagement.DeviceTypes,
                dataPortRepository: helper.AssetManagement.DataPorts,
                powerPortRepository: helper.AssetManagement.PowerPorts,
                rackRepository: helper.FacilityManagement.Racks,
                reservationRepository: null,
                portTypeRepository: null
            );
        }

        #endregion

        #region Test Cleanup Helpers

        /// <summary>
        /// Cleans up all test data from repositories.
        /// Deletes entities in reverse dependency order to avoid foreign key violations.
        /// Call this in [TestInitialize] to ensure clean state before each test.
        /// </summary>
        public static void CleanupAllTestData(this ITestApiHelper helper)
        {
            if (helper == null)
            {
                throw new ArgumentNullException(nameof(helper));
            }

            // Delete in reverse dependency order
            SafeDelete(() =>
            {
                var portTypes = helper.AssetManagement.PortTypes.Read(new Skyline.DataMiner.Net.Messages.SLDataGateway.TRUEFilterElement<Skyline.DataMiner.SDM.AssetManagement.Models.PortType>());
                if (portTypes.Any())
                {
                    helper.AssetManagement.PortTypes.Delete(portTypes);
                }
            });

            SafeDelete(() =>
            {
                var powerPorts = helper.AssetManagement.PowerPorts.Read(new Skyline.DataMiner.Net.Messages.SLDataGateway.TRUEFilterElement<Skyline.DataMiner.SDM.AssetManagement.Models.PowerPort>());
                if (powerPorts.Any())
                {
                    helper.AssetManagement.PowerPorts.Delete(powerPorts);
                }
            });

            SafeDelete(() =>
            {
                var dataPorts = helper.AssetManagement.DataPorts.Read(new Skyline.DataMiner.Net.Messages.SLDataGateway.TRUEFilterElement<Skyline.DataMiner.SDM.AssetManagement.Models.DataPort>());
                if (dataPorts.Any())
                {
                    helper.AssetManagement.DataPorts.Delete(dataPorts);
                }
            });

            SafeDelete(() =>
            {
                var assets = helper.AssetManagement.Assets.Read(new Skyline.DataMiner.Net.Messages.SLDataGateway.TRUEFilterElement<Skyline.DataMiner.SDM.AssetManagement.Models.Asset>());
                if (assets.Any())
                {
                    helper.AssetManagement.Assets.Delete(assets);
                }
            });

            SafeDelete(() =>
            {
                var assetClasses = helper.AssetManagement.AssetClasses.Read(new Skyline.DataMiner.Net.Messages.SLDataGateway.TRUEFilterElement<Skyline.DataMiner.SDM.AssetManagement.Models.AssetClass>());
                if (assetClasses.Any())
                {
                    helper.AssetManagement.AssetClasses.Delete(assetClasses);
                }
            });

            SafeDelete(() =>
            {
                var racks = helper.FacilityManagement.Racks.Read(new Skyline.DataMiner.Net.Messages.SLDataGateway.TRUEFilterElement<Skyline.DataMiner.SDM.FacilityManagement.Models.Rack>());
                if (racks.Any())
                {
                    helper.FacilityManagement.Racks.Delete(racks);
                }
            });

            SafeDelete(() =>
            {
                var deviceTypes = helper.AssetManagement.DeviceTypes.Read(new Skyline.DataMiner.Net.Messages.SLDataGateway.TRUEFilterElement<Skyline.DataMiner.SDM.AssetManagement.Models.DeviceType>());
                if (deviceTypes.Any())
                {
                    helper.AssetManagement.DeviceTypes.Delete(deviceTypes);
                }
            });

            // Clear the test data cache
            var cache = helper.TestData as TestDataCache;
            if (cache != null)
            {
                cache.DeviceTypes = Array.Empty<Skyline.DataMiner.SDM.AssetManagement.Models.DeviceType>();
                cache.AssetClasses = Array.Empty<Skyline.DataMiner.SDM.AssetManagement.Models.AssetClass>();
                cache.Assets = Array.Empty<Skyline.DataMiner.SDM.AssetManagement.Models.Asset>();
                cache.DataPorts = Array.Empty<Skyline.DataMiner.SDM.AssetManagement.Models.DataPort>();
                cache.PowerPorts = Array.Empty<Skyline.DataMiner.SDM.AssetManagement.Models.PowerPort>();
                cache.PortTypes = Array.Empty<Skyline.DataMiner.SDM.AssetManagement.Models.PortType>();
                cache.Racks = Array.Empty<Skyline.DataMiner.SDM.FacilityManagement.Models.Rack>();
            }
        }

        private static void SafeDelete(Action deleteAction)
        {
            try
            {
                deleteAction();
            }
            catch
            {
                // Ignore cleanup errors - may fail if entity doesn't exist or has dependencies
            }
        }

        #endregion

        #region Repository State Verification

        /// <summary>
        /// Checks if all repositories are empty (no data).
        /// Useful for verifying clean state before/after tests.
        /// </summary>
        /// <param name="helper">The test API helper.</param>
        /// <returns>True if all repositories are empty; otherwise false.</returns>
        public static bool AreRepositoriesEmpty(this ITestApiHelper helper)
        {
            if (helper == null)
            {
                throw new ArgumentNullException(nameof(helper));
            }

            return helper.AssetManagement.Assets.Count(new Skyline.DataMiner.Net.Messages.SLDataGateway.TRUEFilterElement<Skyline.DataMiner.SDM.AssetManagement.Models.Asset>()) == 0
                && helper.AssetManagement.AssetClasses.Count(new Skyline.DataMiner.Net.Messages.SLDataGateway.TRUEFilterElement<Skyline.DataMiner.SDM.AssetManagement.Models.AssetClass>()) == 0
                && helper.AssetManagement.DeviceTypes.Count(new Skyline.DataMiner.Net.Messages.SLDataGateway.TRUEFilterElement<Skyline.DataMiner.SDM.AssetManagement.Models.DeviceType>()) == 0
                && helper.AssetManagement.DataPorts.Count(new Skyline.DataMiner.Net.Messages.SLDataGateway.TRUEFilterElement<Skyline.DataMiner.SDM.AssetManagement.Models.DataPort>()) == 0
                && helper.AssetManagement.PowerPorts.Count(new Skyline.DataMiner.Net.Messages.SLDataGateway.TRUEFilterElement<Skyline.DataMiner.SDM.AssetManagement.Models.PowerPort>()) == 0
                && helper.FacilityManagement.Racks.Count(new Skyline.DataMiner.Net.Messages.SLDataGateway.TRUEFilterElement<Skyline.DataMiner.SDM.FacilityManagement.Models.Rack>()) == 0;
        }

        /// <summary>
        /// Asserts that all repositories are empty, throwing an exception with details if not.
        /// </summary>
        /// <param name="helper">The test API helper.</param>
        /// <exception cref="InvalidOperationException">Thrown when repositories are not empty, with details.</exception>
        public static void AssertRepositoriesEmpty(this ITestApiHelper helper)
        {
            if (helper == null)
            {
                throw new ArgumentNullException(nameof(helper));
            }

            var nonEmptyRepositories = new List<string>();

            var assetCount = helper.AssetManagement.Assets.Count(new Skyline.DataMiner.Net.Messages.SLDataGateway.TRUEFilterElement<Skyline.DataMiner.SDM.AssetManagement.Models.Asset>());
            if (assetCount > 0) nonEmptyRepositories.Add($"Assets ({assetCount})");

            var assetClassCount = helper.AssetManagement.AssetClasses.Count(new Skyline.DataMiner.Net.Messages.SLDataGateway.TRUEFilterElement<Skyline.DataMiner.SDM.AssetManagement.Models.AssetClass>());
            if (assetClassCount > 0) nonEmptyRepositories.Add($"AssetClasses ({assetClassCount})");

            var deviceTypeCount = helper.AssetManagement.DeviceTypes.Count(new Skyline.DataMiner.Net.Messages.SLDataGateway.TRUEFilterElement<Skyline.DataMiner.SDM.AssetManagement.Models.DeviceType>());
            if (deviceTypeCount > 0) nonEmptyRepositories.Add($"DeviceTypes ({deviceTypeCount})");

            var dataPortCount = helper.AssetManagement.DataPorts.Count(new Skyline.DataMiner.Net.Messages.SLDataGateway.TRUEFilterElement<Skyline.DataMiner.SDM.AssetManagement.Models.DataPort>());
            if (dataPortCount > 0) nonEmptyRepositories.Add($"DataPorts ({dataPortCount})");

            var powerPortCount = helper.AssetManagement.PowerPorts.Count(new Skyline.DataMiner.Net.Messages.SLDataGateway.TRUEFilterElement<Skyline.DataMiner.SDM.AssetManagement.Models.PowerPort>());
            if (powerPortCount > 0) nonEmptyRepositories.Add($"PowerPorts ({powerPortCount})");

            var rackCount = helper.FacilityManagement.Racks.Count(new Skyline.DataMiner.Net.Messages.SLDataGateway.TRUEFilterElement<Skyline.DataMiner.SDM.FacilityManagement.Models.Rack>());
            if (rackCount > 0) nonEmptyRepositories.Add($"Racks ({rackCount})");

            if (nonEmptyRepositories.Any())
            {
                throw new InvalidOperationException(
                    $"Repositories are not empty. Non-empty repositories: {string.Join(", ", nonEmptyRepositories)}");
            }
        }

        /// <summary>
        /// Gets a dictionary of repository counts for diagnostic purposes.
        /// </summary>
        /// <param name="helper">The test API helper.</param>
        /// <returns>Dictionary with repository names and their item counts.</returns>
        public static Dictionary<string, long> GetRepositoryCounts(this ITestApiHelper helper)
        {
            if (helper == null)
            {
                throw new ArgumentNullException(nameof(helper));
            }

            return new Dictionary<string, long>
            {
                ["Assets"] = helper.AssetManagement.Assets.Count(new Skyline.DataMiner.Net.Messages.SLDataGateway.TRUEFilterElement<Skyline.DataMiner.SDM.AssetManagement.Models.Asset>()),
                ["AssetClasses"] = helper.AssetManagement.AssetClasses.Count(new Skyline.DataMiner.Net.Messages.SLDataGateway.TRUEFilterElement<Skyline.DataMiner.SDM.AssetManagement.Models.AssetClass>()),
                ["DeviceTypes"] = helper.AssetManagement.DeviceTypes.Count(new Skyline.DataMiner.Net.Messages.SLDataGateway.TRUEFilterElement<Skyline.DataMiner.SDM.AssetManagement.Models.DeviceType>()),
                ["DataPorts"] = helper.AssetManagement.DataPorts.Count(new Skyline.DataMiner.Net.Messages.SLDataGateway.TRUEFilterElement<Skyline.DataMiner.SDM.AssetManagement.Models.DataPort>()),
                ["PowerPorts"] = helper.AssetManagement.PowerPorts.Count(new Skyline.DataMiner.Net.Messages.SLDataGateway.TRUEFilterElement<Skyline.DataMiner.SDM.AssetManagement.Models.PowerPort>()),
                ["Racks"] = helper.FacilityManagement.Racks.Count(new Skyline.DataMiner.Net.Messages.SLDataGateway.TRUEFilterElement<Skyline.DataMiner.SDM.FacilityManagement.Models.Rack>())
            };
        }

        #endregion

        #region Test Data Helpers

        /// <summary>
        /// Returns the first device type in the cache that does not carry the PowerProvider tag.
        /// Use this instead of <c>DeviceTypes.First()</c> when creating asset classes in tests,
        /// because repo ordering is non-deterministic and a PowerProvider device type requires
        /// PowerSupply to be set on the asset class or validation will fail.
        /// </summary>
        public static Skyline.DataMiner.SDM.AssetManagement.Models.DeviceType NonPowerProviderDeviceType(this ITestDataCache testData)
            => testData.DeviceTypes.First(d => !d.TagsInfo.Tags.Contains(SlcAsset_Management.Enums.TagOption.PowerProvider));

        #endregion
    }
}
