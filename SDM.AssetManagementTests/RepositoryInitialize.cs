namespace SDM.AssetManagement.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SDM.AssetManagement.Tests.Setup;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;

    public static partial class RepositoryInitialize
    {
        public static ITestApiHelper InitializeEmptyRepositories()
        {
            var connection = ConnectionHelper.CreateConnection();
            var facilityHelper = connection.GetMockedFacilityManagementHelper();
            var assetHelper = connection.GetMockedAssetManagementHelper(facilityHelper);

            return new TestApiHelper(assetHelper, facilityHelper);
        }

        #region Assets

        public static ITestApiHelper PopulateAssets(this ITestApiHelper helper, IEnumerable<Asset> assets)
        {
            if (assets == null)
            {
                throw new ArgumentNullException(nameof(assets));
            }

            helper.AssetManagement.Assets.Create(assets);
            return helper;
        }

        public static ITestApiHelper PopulateAssets(this ITestApiHelper helper)
        {
            var persistedAssetClasses = helper.AssetManagement.AssetClasses
                .Read(new TRUEFilterElement<AssetClass>()).ToList();

            // Validate prerequisites - AssetClasses are required
            if (persistedAssetClasses.Count == 0)
            {
                throw new InvalidOperationException(
                    "Cannot populate assets: No AssetClasses found. Call PopulateAssetClasses() first.");
            }

            // Racks are optional - check if any exist
            var persistedRacks = helper.FacilityManagement.Racks
                .Read(new TRUEFilterElement<Rack>()).ToList();
            bool hasRacks = persistedRacks.Count > 0;

            var assets = new List<Asset>();
            for (int i = 0; i < DemoData.BaseAssets.Count; i++)
            {
                var baseAsset = DemoData.BaseAssets[i];
                var assetClassIndex = i % persistedAssetClasses.Count;
                var assetClass = persistedAssetClasses[assetClassIndex];

                var asset = CloneAsset(baseAsset);
                asset.AssetClassId = new SdmObjectReference<AssetClass>(assetClass.Identifier);

                // Only assign rack if racks are available
                if (hasRacks)
                {
                    var rackIndex = i % persistedRacks.Count;
                    var rack = persistedRacks[rackIndex];
                    asset.Location.RackId = new SdmObjectReference<Rack>(rack.Identifier);
                    // RackPosition is already set in CloneAsset from DemoData
                }
                else
                {
                    // Clear rack-related location data if no racks available
                    asset.Location = null;
                }

                assets.Add(asset);
            }

            helper.AssetManagement.Assets.Create(assets);
            return helper;
        }

        #endregion

        #region AssetClasses

        public static ITestApiHelper PopulateAssetClasses(this ITestApiHelper helper, IEnumerable<AssetClass> assetClasses)
        {
            if (assetClasses == null)
            {
                throw new ArgumentNullException(nameof(assetClasses));
            }

            helper.AssetManagement.AssetClasses.Create(assetClasses);
            return helper;
        }

        public static ITestApiHelper PopulateAssetClasses(this ITestApiHelper helper)
        {
            var persistedDeviceTypes = helper.AssetManagement.DeviceTypes
                .Read(new TRUEFilterElement<DeviceType>()).ToList();

            // Validate prerequisites - DeviceTypes are required
            if (persistedDeviceTypes.Count == 0)
            {
                throw new InvalidOperationException(
                    "Cannot populate asset classes: No DeviceTypes found. Call PopulateDeviceTypes() first.");
            }

            var assetClasses = new List<AssetClass>();
            for (int i = 0; i < DemoData.BaseAssetClasses.Count; i++)
            {
                var baseClass = DemoData.BaseAssetClasses[i];

                // Clone the object and set reference
                var deviceTypeIndex = i % persistedDeviceTypes.Count;

                var assetClass = CloneAssetClass(baseClass);
                assetClass.DeviceTypeId = new SdmObjectReference<DeviceType>(persistedDeviceTypes[deviceTypeIndex].Identifier);

                assetClasses.Add(assetClass);
            }

            helper.AssetManagement.AssetClasses.Create(assetClasses);
            return helper;
        }

        #endregion

        #region DataPorts

        public static ITestApiHelper PopulateDataPorts(this ITestApiHelper helper, IEnumerable<DataPort> dataPorts)
        {
            if (dataPorts == null)
            {
                throw new ArgumentNullException(nameof(dataPorts));
            }

            helper.AssetManagement.DataPorts.Create(dataPorts);
            return helper;
        }

        public static ITestApiHelper PopulateDataPorts(this ITestApiHelper helper)
        {
            var persistedAssets = helper.AssetManagement.Assets
                .Read(new TRUEFilterElement<Asset>()).ToList();

            // Validate prerequisites - Assets are required
            if (persistedAssets.Count == 0)
            {
                throw new InvalidOperationException(
                    "Cannot populate data ports: No Assets found. Call PopulateAssets() first.");
            }

            var dataPorts = new List<DataPort>();
            for (int i = 0; i < DemoData.BaseDataPorts.Count; i++)
            {
                var basePort = DemoData.BaseDataPorts[i];

                // Clone the object and set reference
                var assetIndex = i % persistedAssets.Count;

                var dataPort = CloneDataPort(basePort);
                dataPort.AssetFk = new AssetRelation
                {
                    Asset = new SdmObjectReference<Asset>(persistedAssets[assetIndex].Identifier),
                };

                dataPorts.Add(dataPort);
            }

            helper.AssetManagement.DataPorts.Create(dataPorts);
            return helper;
        }

        #endregion

        #region PowerPorts

        public static ITestApiHelper PopulatePowerPorts(this ITestApiHelper helper, IEnumerable<PowerPort> powerPorts)
        {
            if (powerPorts == null)
            {
                throw new ArgumentNullException(nameof(powerPorts));
            }

            helper.AssetManagement.PowerPorts.Create(powerPorts);
            return helper;
        }

        public static ITestApiHelper PopulatePowerPorts(this ITestApiHelper helper)
        {
            var persistedAssets = helper.AssetManagement.Assets
                .Read(new TRUEFilterElement<Asset>()).ToList();

            // Validate prerequisites - Assets are required
            if (persistedAssets.Count == 0)
            {
                throw new InvalidOperationException(
                    "Cannot populate power ports: No Assets found. Call PopulateAssets() first.");
            }

            var powerPorts = new List<PowerPort>();
            for (int i = 0; i < DemoData.BasePowerPorts.Count; i++)
            {
                var basePort = DemoData.BasePowerPorts[i];

                // Clone the object and set reference
                var assetIndex = i % persistedAssets.Count;

                var powerPort = ClonePowerPort(basePort);
                powerPort.Asset = new SdmObjectReference<Asset>(persistedAssets[assetIndex].Identifier);

                powerPorts.Add(powerPort);
            }

            helper.AssetManagement.PowerPorts.Create(powerPorts);
            return helper;
        }

        #endregion

        #region DeviceTypes

        public static ITestApiHelper PopulateDeviceTypes(this ITestApiHelper helper, IEnumerable<DeviceType> deviceTypes)
        {
            if (deviceTypes == null)
            {
                throw new ArgumentNullException(nameof(deviceTypes));
            }

            helper.AssetManagement.DeviceTypes.Create(deviceTypes);
            return helper;
        }

        public static ITestApiHelper PopulateDeviceTypes(this ITestApiHelper helper)
        {
            // DeviceTypes have no dependencies, so we can use them directly
            helper.AssetManagement.DeviceTypes.Create(DemoData.DeviceTypes);
            return helper;
        }

        #endregion

        #region Racks

        public static ITestApiHelper PopulateRacks(this ITestApiHelper helper, IEnumerable<Rack> racks)
        {
            if (racks == null)
            {
                throw new ArgumentNullException(nameof(racks));
            }

            helper.FacilityManagement.Racks.Create(racks);
            return helper;
        }

        public static ITestApiHelper PopulateRacks(this ITestApiHelper helper)
        {
            // Racks have no dependencies, so we can use them directly
            helper.FacilityManagement.Racks.Create(DemoData.Racks);
            return helper;
        }

        #endregion

        #region Cloning Methods

        private static AssetClass CloneAssetClass(AssetClass source)
        {
            return new AssetClass
            {
                Name = source.Name,
                Description = source.Description,
                Manufacturer = source.Manufacturer,
                Depth = source.Depth,
                Height = source.Height,
                Width = source.Width,
                HeightU = source.HeightU,
                Weight = source.Weight,
                FrontImage = source.FrontImage,
                BackImage = source.BackImage,
                TypicalPowerConsumption = source.TypicalPowerConsumption,
                MaximumPowerConsumption = source.MaximumPowerConsumption,
                PowerSupply = source.PowerSupply,
                Lifecycle = source.Lifecycle != null ? new AssetClassLifecycle
                {
                    EndOfLife = source.Lifecycle.EndOfLife,
                    EndOfService = source.Lifecycle.EndOfService,
                    NominalLifetime = source.Lifecycle.NominalLifetime,
                } : null,
                DataPorts = source.DataPorts != null ? new List<DataPortInfo>(source.DataPorts) : new List<DataPortInfo>(),
                PowerPorts = source.PowerPorts != null ? new List<PowerPortInfo>(source.PowerPorts) : new List<PowerPortInfo>(),
                Holders = source.Holders != null ? new List<AssetHolder>(source.Holders) : new List<AssetHolder>(),
                // DeviceTypeId will be set by caller
            };
        }

        private static Asset CloneAsset(Asset source)
        {
            return new Asset
            {
                State = source.State,
                AssetID = source.AssetID,
                Name = source.Name,
                Description = source.Description,
                FW_OS = source.FW_OS,
                SerialNumber = source.SerialNumber,
                HardwareVersion = source.HardwareVersion,
                MacAddress = source.MacAddress,
                Location = source.Location != null ? new AssetLocation
                {
                    RackPosition = source.Location.RackPosition,
                    Side = source.Location.Side,
                    // RackId will be set by caller
                } : new AssetLocation(),
                PurchaseDate = source.PurchaseDate,
                FirstUseDate = source.FirstUseDate,
                EndOfWarrantyDate = source.EndOfWarrantyDate,
                InstallationDate = source.InstallationDate,
                InstallationUserId = source.InstallationUserId,
                ModificationDate = source.ModificationDate,
                ModificationUserId = source.ModificationUserId,
                EndOfLifeDate = source.EndOfLifeDate,
                Ownership = source.Ownership != null ? new AssetOwnership
                {
                    Organization = source.Ownership.Organization,
                    ContactPerson = source.Ownership.ContactPerson,
                    ContactPersonRole = source.Ownership.ContactPersonRole,
                    Team = source.Ownership.Team,
                } : null,
                Custody = source.Custody != null ? new AssetCustody
                {
                    From = source.Custody.From,
                    Till = source.Custody.Till,
                    ContactPerson = source.Custody.ContactPerson,
                    Team = source.Custody.Team,
                    Organization = source.Custody.Organization,
                    ContactPersonRole = source.Custody.ContactPersonRole,
                } : null,
                Holders = source.Holders != null ? new List<AssetHolder>(source.Holders) : new List<AssetHolder>(),
                ElementLinks = source.ElementLinks != null ? new List<ElementLink>(source.ElementLinks) : new List<ElementLink>(),
                // AssetClassId will be set by caller
            };
        }

        private static DataPort CloneDataPort(DataPort source)
        {
            return new DataPort
            {
                DataPortInfo = source.DataPortInfo != null ? new DataPortInfo
                {
                    Name = source.DataPortInfo.Name,
                    PortNumber = source.DataPortInfo.PortNumber,
                    OutputType = source.DataPortInfo.OutputType,
                    PortExposure = source.DataPortInfo.PortExposure,
                    Label = source.DataPortInfo.Label,
                } : new DataPortInfo(),
                AddressInfo = source.AddressInfo != null ? new AddressInfo
                {
                    Ipv4Address = source.AddressInfo.Ipv4Address,
                    Ipv6Address = source.AddressInfo.Ipv6Address,
                    Hostname = source.AddressInfo.Hostname,
                    DNS = source.AddressInfo.DNS,
                } : null,
                PrimaryPortRelation = source.PrimaryPortRelation != null ? new PrimaryPortRelation
                {
                    IsPrimaryIpv4 = source.PrimaryPortRelation.IsPrimaryIpv4,
                    IsPrimaryIpv6 = source.PrimaryPortRelation.IsPrimaryIpv6,
                } : null,
                // AssetFk will be set by caller
            };
        }

        private static PowerPort ClonePowerPort(PowerPort source)
        {
            return new PowerPort
            {
                PowerPortInfo = source.PowerPortInfo != null ? new PowerPortInfo
                {
                    Name = source.PowerPortInfo.Name,
                    PortNumber = source.PowerPortInfo.PortNumber,
                    PortExposure = source.PowerPortInfo.PortExposure,
                    OutputType = source.PowerPortInfo.OutputType,
                    Label = source.PowerPortInfo.Label,
                } : new PowerPortInfo(),
                // Asset will be set by caller
            };
        }

        #endregion
    }
}