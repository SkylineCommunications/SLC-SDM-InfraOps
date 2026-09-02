namespace SDM.AssetManagement.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SDM.AssetManagement.Tests.Setup;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Extensions;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;

    /// <summary>
    /// Defines the data layers that can be populated in the test environment.
    /// The order represents the dependency hierarchy.
    /// </summary>
    public enum DemoDataLayer
    {
        /// <summary>
        /// Device types - no dependencies.
        /// </summary>
        DeviceTypes = 1,

        /// <summary>
        /// Racks - no dependencies (optional for Assets).
        /// </summary>
        Racks = 2,

        /// <summary>
        /// Asset classes - depends on DeviceTypes.
        /// </summary>
        AssetClasses = 3,

        /// <summary>
        /// Assets - depends on AssetClasses (and optionally Racks).
        /// </summary>
        Assets = 4,

        /// <summary>
        /// Data ports - depends on Assets.
        /// </summary>
        DataPorts = 5,

        /// <summary>
        /// Power ports - depends on Assets.
        /// </summary>
        PowerPorts = 6,

        /// <summary>
        /// Port types - no dependencies.
        /// </summary>
        PortTypes = 7,
    }

    public static partial class RepositoryInitialize
    {
        public static ITestApiHelper InitializeEmptyRepositories()
        {
            var connection = ConnectionHelper.CreateConnection();
            return new TestApiHelper(connection);
        }

        /// <summary>
        /// Populates demo data up to and including the specified data layer.
        /// Automatically populates all required dependencies.
        /// </summary>
        /// <param name="helper">The test API helper.</param>
        /// <param name="upTo">The highest data layer to populate.</param>
        /// <param name="includeRacks">Whether to include racks when populating assets.</param>
        /// <returns>The test API helper for method chaining.</returns>
        public static ITestApiHelper PopulateWithDemoData(
            this ITestApiHelper helper,
            DemoDataLayer upTo)
        {
            // Layer 1: DeviceTypes (required for AssetClasses and above)
            if (upTo >= DemoDataLayer.DeviceTypes)
            {
                PopulateDeviceTypes(helper);
            }

            // Layer 2: Racks (optional, but populate if requested and needed for Assets)
            if (upTo >= DemoDataLayer.Racks)
            {
                PopulateRacks(helper);
            }

            // Layer 3: AssetClasses (required for Assets and above)
            if (upTo >= DemoDataLayer.AssetClasses)
            {
                PopulateAssetClasses(helper);
            }

            // Layer 4: Assets (required for Ports)
            if (upTo >= DemoDataLayer.Assets)
            {
                PopulateAssets(helper);
            }

            // Layer 5 & 6: Ports (independent of each other)
            if (upTo >= DemoDataLayer.DataPorts)
            {
                PopulateDataPorts(helper);
            }

            if (upTo >= DemoDataLayer.PowerPorts)
            {
                PopulatePowerPorts(helper);
            }

            if (upTo >= DemoDataLayer.PortTypes)
            {
                PopulatePortTypes(helper);
            }

            return helper;
        }

        /// <summary>
        /// Populates all demo data including all ports.
        /// </summary>
        public static ITestApiHelper PopulateWithDemoData(this ITestApiHelper helper)
        {
            return PopulateWithDemoData(helper, DemoDataLayer.PortTypes);
        }

        #region Assets

        public static ITestApiHelper PopulateAssets(this ITestApiHelper helper, IEnumerable<Asset> assets)
        {
            if (assets == null)
            {
                throw new ArgumentNullException(nameof(assets));
            }

            helper.AssetManagement.Assets.Create(assets);

            // Refresh cache from database to ensure consistency
            RefreshAssetsCache(helper);

            return helper;
        }

        public static ITestApiHelper PopulateAssets(this ITestApiHelper helper)
        {
            // If already populated, return existing
            if (helper.TestData.Assets.Any())
            {
                return helper;
            }

            // Ensure AssetClasses exist (will use cached if available)
            helper.PopulateAssetClasses();
            
            var persistedAssetClasses = helper.TestData.AssetClasses;
            if (!persistedAssetClasses.Any())
            {
                throw new InvalidOperationException(
                    "Cannot populate assets: No AssetClasses found. Call PopulateAssetClasses() first.");
            }

            // Racks are optional
            var assetClasses = persistedAssetClasses.ToDictionary(ac => ac.Name);
            var persistedRacks = helper.TestData.Racks;
            bool hasRacks = persistedRacks.Any();

            // Sort racks by RackId to ensure deterministic order
            Dictionary<string,Rack> racks = new Dictionary<string, Rack>();
            if (hasRacks)
            {
                racks = persistedRacks.ToDictionary(r => r.RackId);
            }

            var assets = new List<Asset>();
            for (int i = 0; i < DemoData.BaseAssets.Count; i++)
            {
                var baseAsset = DemoData.BaseAssets[i];
                string assetClassName = baseAsset.AssetClassId.Identifier;

                if (!assetClasses.TryGetValue(assetClassName, out var assetClass))
                {
                    throw new InvalidOperationException(
                        $"Asset '{baseAsset.SerialNumber}': Asset class with name '{assetClassName}' not found in persisted asset classes. Only asset classes available {String.Format(";", assetClasses.Keys)}.");
                }

                var asset = CloneAsset(baseAsset);
                asset.AssetClassId = new SdmObjectReference<AssetClass>(assetClass.Identifier);

                if (hasRacks)
                {
                    // Use serial number from the asset to lookup rack assignment
                    //cant for the moment do any rack assignment do tue the non existent of nullables so it will complain about having multiple locations?
                    if (DemoData.AssetRackAssignments.TryGetValue(asset.SerialNumber, out var assignment))
                    {
                        string rackid = assignment.RackId;

                        // Validate rack index is within bounds
                        if (racks.TryGetValue(rackid, out Rack rack))
                        {
                            asset.Location.RackId = new SdmObjectReference<Rack>(rack.Identifier);
                            asset.Location.RackPosition = assignment.Position;
                            asset.Location.Side = SlcAsset_Management.Enums.SideEnum.Front;
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                $"Asset '{asset.SerialNumber}': Rack id {rackid} not found in persisted racks. Only rack ids available {String.Format(";", racks.Keys)}.");
                        }
                    }
                }
                else
                {
                    asset.Location.ParentAsset = default;
                    asset.Location.HolderNumber = default;
                    asset.Location.RackId = default;
                    asset.Location.RackPosition = default;
                    asset.Location.Side = default;
                    asset.Location.DeskId = default;
                    asset.Location.ContainerId = default;
                    asset.Location.RoomId = default;
                }

                assets.Add(asset);
            }

            helper.AssetManagement.Assets.Create(assets);
            
            // Refresh cache from database to ensure consistency
            RefreshAssetsCache(helper);
            
            return helper;
        }

        private static void RefreshAssetsCache(ITestApiHelper helper)
        {
            var allAssets = helper.AssetManagement.Assets
                .Read(new TRUEFilterElement<Asset>())
                .ToList();
            helper.TestData.Assets = allAssets.AsReadOnly();
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

            // Refresh cache from database to ensure consistency
            RefreshAssetClassesCache(helper);

            return helper;
        }

        private static ITestApiHelper PopulateAssetClasses(this ITestApiHelper helper)
        {
            // If already populated, return existing
            if (helper.TestData.AssetClasses.Any())
            {
                return helper;
            }

            var persistedDeviceTypes = helper.TestData.DeviceTypes;
            if (!persistedDeviceTypes.Any())
            {
                throw new InvalidOperationException(
                    "Cannot populate asset classes: No DeviceTypes found. Call PopulateDeviceTypes() first.");
            }

            var deviceTypes = persistedDeviceTypes.ToDictionary(dt => dt.Name);
            var assetClasses = new List<AssetClass>();
            for (int i = 0; i < DemoData.BaseAssetClasses.Count; i++)
            {
                var baseClass = DemoData.BaseAssetClasses[i];
                var deviceTypeName = baseClass.DeviceTypeId.Identifier;

                var assetClass = CloneAssetClass(baseClass);
                assetClass.DeviceTypeId = new SdmObjectReference<DeviceType>(deviceTypes[deviceTypeName].Identifier);

                assetClasses.Add(assetClass);
            }

            helper.AssetManagement.AssetClasses.Create(assetClasses);

            // Refresh cache from database to ensure consistency
            RefreshAssetClassesCache(helper);

            return helper;
        }

        private static void RefreshAssetClassesCache(ITestApiHelper helper)
        {
            var allAssetClasses = helper.AssetManagement.AssetClasses
                .Read(new TRUEFilterElement<AssetClass>())
                .ToList();
            helper.TestData.AssetClasses = allAssetClasses.AsReadOnly();
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

            // Refresh cache from database to ensure consistency
            RefreshDataPortsCache(helper);

            return helper;
        }

        private static ITestApiHelper PopulateDataPorts(this ITestApiHelper helper)
        {
            // If already populated, return existing
            if (helper.TestData.DataPorts.Any())
            {
                return helper;
            }

            // Ensure Assets exist (will use cached if available)
            helper.PopulateAssets();

            var persistedAssets = helper.TestData.Assets;
            if (!persistedAssets.Any())
            {
                throw new InvalidOperationException(
                    "Cannot populate data ports: No Assets found. Call PopulateAssets() first.");
            }

            // Ensure PortTypes exist and pick a data-compatible one (validation requires a valid Port Type).
            helper.PopulatePortTypes();
            var dataPortType = helper.TestData.PortTypes.First(pt => pt.IsDataPortType());

            var dataPorts = new List<DataPort>();
            for (int i = 0; i < DemoData.BaseDataPorts.Count; i++)
            {
                var basePort = DemoData.BaseDataPorts[i];
                var assetIndex = i % persistedAssets.Count;

                var dataPort = CloneDataPort(basePort);
                dataPort.Asset = new SdmObjectReference<Asset>(persistedAssets[assetIndex].Identifier);
                dataPort.DataPortInfo.PortType = new SdmObjectReference<PortType>(dataPortType.Identifier);

                dataPorts.Add(dataPort);
            }

            helper.AssetManagement.DataPorts.Create(dataPorts);

            // Refresh cache from database to ensure consistency
            RefreshDataPortsCache(helper);

            return helper;
        }

        private static void RefreshDataPortsCache(ITestApiHelper helper)
        {
            var allDataPorts = helper.AssetManagement.DataPorts
                .Read(new TRUEFilterElement<DataPort>())
                .ToList();
            helper.TestData.DataPorts = allDataPorts.AsReadOnly();
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

            // Refresh cache from database to ensure consistency
            RefreshPowerPortsCache(helper);

            return helper;
        }

        private static ITestApiHelper PopulatePowerPorts(this ITestApiHelper helper)
        {
            // If already populated, return existing
            if (helper.TestData.PowerPorts.Any())
            {
                return helper;
            }

            // Ensure Assets exist (will use cached if available)
            helper.PopulateAssets();

            var persistedAssets = helper.TestData.Assets;
            if (!persistedAssets.Any())
            {
                throw new InvalidOperationException(
                    "Cannot populate power ports: No Assets found. Call PopulateAssets() first.");
            }

            // Ensure PortTypes exist and pick a power-compatible one (validation requires a valid Port Type).
            helper.PopulatePortTypes();
            var powerPortType = helper.TestData.PortTypes.First(pt => pt.IsPowerPortType());

            var powerPorts = new List<PowerPort>();
            for (int i = 0; i < DemoData.BasePowerPorts.Count; i++)
            {
                var basePort = DemoData.BasePowerPorts[i];
                var assetIndex = i % persistedAssets.Count;

                var powerPort = ClonePowerPort(basePort);
                powerPort.Asset = new SdmObjectReference<Asset>(persistedAssets[assetIndex].Identifier);
                powerPort.PowerPortInfo.PortType = new SdmObjectReference<PortType>(powerPortType.Identifier);

                powerPorts.Add(powerPort);
            }

            helper.AssetManagement.PowerPorts.Create(powerPorts);

            // Refresh cache from database to ensure consistency
            RefreshPowerPortsCache(helper);

            return helper;
        }

        private static void RefreshPowerPortsCache(ITestApiHelper helper)
        {
            var allPowerPorts = helper.AssetManagement.PowerPorts
                .Read(new TRUEFilterElement<PowerPort>())
                .ToList();
            helper.TestData.PowerPorts = allPowerPorts.AsReadOnly();
        }

        #endregion

        #region PortTypes

        public static ITestApiHelper PopulatePortTypes(this ITestApiHelper helper, IEnumerable<PortType> portTypes)
        {
            if (portTypes == null)
            {
                throw new ArgumentNullException(nameof(portTypes));
            }

            helper.AssetManagement.PortTypes.Create(portTypes);

            // Refresh cache from database to ensure consistency
            RefreshPortTypesCache(helper);

            return helper;
        }

        private static ITestApiHelper PopulatePortTypes(this ITestApiHelper helper)
        {
            // If already populated, return existing
            if (helper.TestData.PortTypes.Any())
            {
                return helper;
            }

            helper.AssetManagement.PortTypes.Create(DemoData.PortTypes);

            // Refresh cache from database to ensure consistency
            RefreshPortTypesCache(helper);

            return helper;
        }

        private static void RefreshPortTypesCache(ITestApiHelper helper)
        {
            var allPortTypes = helper.AssetManagement.PortTypes
                .Read(new TRUEFilterElement<PortType>())
                .ToList();
            helper.TestData.PortTypes = allPortTypes.AsReadOnly();
        }

        #endregion

        #region AssetManagerAppSettings

        public static ITestApiHelper PopulateAssetManagerAppSettings(this ITestApiHelper helper)
        {
            helper.AssetManagement.AppSettings.Create(DemoData.AssetManagerAppSettings);

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

            // Refresh cache from database to ensure consistency
            RefreshDeviceTypesCache(helper);

            return helper;
        }

        private static ITestApiHelper PopulateDeviceTypes(this ITestApiHelper helper)
        {
            // If already populated, return existing
            if (helper.TestData.DeviceTypes.Any())
            {
                return helper;
            }

            helper.AssetManagement.DeviceTypes.Create(DemoData.DeviceTypes);

            // Refresh cache from database to ensure consistency
            RefreshDeviceTypesCache(helper);

            return helper;
        }

        private static void RefreshDeviceTypesCache(ITestApiHelper helper)
        {
            var allDeviceTypes = helper.AssetManagement.DeviceTypes
                .Read(new TRUEFilterElement<DeviceType>())
                .ToList();
            helper.TestData.DeviceTypes = allDeviceTypes.AsReadOnly();
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

            // Refresh cache from database to ensure consistency
            RefreshRacksCache(helper);

            return helper;
        }

        private static ITestApiHelper PopulateRacks(this ITestApiHelper helper)
        {
            // If already populated, return existing
            if (helper.TestData.Racks.Any())
            {
                return helper;
            }

            helper.FacilityManagement.Racks.Create(DemoData.Racks);

            // Refresh cache from database to ensure consistency
            RefreshRacksCache(helper);

            return helper;
        }

        private static void RefreshRacksCache(ITestApiHelper helper)
        {
            var allRacks = helper.FacilityManagement.Racks
                .Read(new TRUEFilterElement<Rack>())
                .ToList();
            helper.TestData.Racks = allRacks.AsReadOnly();
        }

        #endregion

        #region Cloning Methods

        private static AssetClass CloneAssetClass(AssetClass source)
        {
            var clone = new AssetClass
            {
                Name = source.Name,
                State = source.State,
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
                DataPorts = source.DataPorts != null ? new List<DataPortInfo>(source.DataPorts) : new List<DataPortInfo>(),
                PowerPorts = source.PowerPorts != null ? new List<PowerPortInfo>(source.PowerPorts) : new List<PowerPortInfo>(),
                Holders = source.Holders != null ? new List<AssetHolder>(source.Holders) : new List<AssetHolder>(),
                Attachments = source.Attachments != null ? new List<Attachment>(source.Attachments) : new List<Attachment>(),
                // DeviceTypeId will be set by caller
            };

            clone.Lifecycle.EndOfLife = source.Lifecycle.EndOfLife;
            clone.Lifecycle.EndOfService = source.Lifecycle.EndOfService;
            clone.Lifecycle.NominalLifetime = source.Lifecycle.NominalLifetime;

            return clone;
        }

        private static Asset CloneAsset(Asset source)
        {
            var clone = new Asset
            {
                State = source.State,
                AssetID = source.AssetID,
                Name = source.Name,
                Description = source.Description,
                FW_OS = source.FW_OS,
                SerialNumber = source.SerialNumber,
                HardwareVersion = source.HardwareVersion,
                MacAddress = source.MacAddress,
                PurchaseDate = source.PurchaseDate,
                FirstUseDate = source.FirstUseDate,
                EndOfWarrantyDate = source.EndOfWarrantyDate,
                InstallationDate = source.InstallationDate,
                InstallationUserId = source.InstallationUserId,
                ModificationDate = source.ModificationDate,
                ModificationUserId = source.ModificationUserId,
                EndOfLifeDate = source.EndOfLifeDate,
                Holders = source.Holders != null ? new List<AssetHolder>(source.Holders) : null,
                ElementLinks = source.ElementLinks != null ? new List<ElementLink>(source.ElementLinks) : new List<ElementLink>(),
                // AssetClassId will be set by caller
            };

            clone.Location.RackPosition = source.Location.RackPosition;
            clone.Location.Side = source.Location.Side;
            clone.Location.ParentAsset = source.Location.ParentAsset;
            clone.Location.HolderNumber = source.Location.HolderNumber;
            clone.Location.RackId = source.Location.RackId;
            clone.Location.DeskId = source.Location.DeskId;
            clone.Location.ContainerId = source.Location.ContainerId;
            clone.Location.RoomId = source.Location.RoomId;
            // RackId will be set by caller

            clone.DestinationLocation.RackPosition = source.DestinationLocation.RackPosition;
            clone.DestinationLocation.Side = source.DestinationLocation.Side;
            clone.DestinationLocation.ParentAsset = source.DestinationLocation.ParentAsset;
            clone.DestinationLocation.HolderNumber = source.DestinationLocation.HolderNumber;
            clone.DestinationLocation.RackId = source.DestinationLocation.RackId;
            clone.DestinationLocation.DeskId = source.DestinationLocation.DeskId;
            clone.DestinationLocation.ContainerId = source.DestinationLocation.ContainerId;
            clone.DestinationLocation.RoomId = source.DestinationLocation.RoomId;

            clone.Ownership.Organization = source.Ownership.Organization;
            clone.Ownership.ContactPerson = source.Ownership.ContactPerson;
            clone.Ownership.ContactPersonRole = source.Ownership.ContactPersonRole;
            clone.Ownership.Team = source.Ownership.Team;

            clone.Custody.From = source.Custody.From;
            clone.Custody.Till = source.Custody.Till;
            clone.Custody.ContactPerson = source.Custody.ContactPerson;
            clone.Custody.Team = source.Custody.Team;
            clone.Custody.Organization = source.Custody.Organization;
            clone.Custody.ContactPersonRole = source.Custody.ContactPersonRole;

            return clone;
        }

        private static DataPort CloneDataPort(DataPort source)
        {
            var clone = new DataPort();
            // AssetFk will be set by caller

            clone.DataPortInfo.Name = source.DataPortInfo.Name;
            clone.DataPortInfo.PortNumber = source.DataPortInfo.PortNumber;
            clone.DataPortInfo.OutputType = source.DataPortInfo.OutputType;
            clone.DataPortInfo.PortExposure = source.DataPortInfo.PortExposure;
            clone.DataPortInfo.PortType = source.DataPortInfo.PortType;
            clone.DataPortInfo.Label = source.DataPortInfo.Label;

            clone.AddressInfo.Ipv4Address = source.AddressInfo.Ipv4Address;
            clone.AddressInfo.Ipv6Address = source.AddressInfo.Ipv6Address;
            clone.AddressInfo.Hostname = source.AddressInfo.Hostname;
            clone.AddressInfo.DNS = source.AddressInfo.DNS;

            clone.PrimaryPortRelation.IsPrimaryIpv4 = source.PrimaryPortRelation.IsPrimaryIpv4;
            clone.PrimaryPortRelation.IsPrimaryIpv6 = source.PrimaryPortRelation.IsPrimaryIpv6;

            return clone;
        }

        private static PowerPort ClonePowerPort(PowerPort source)
        {
            var clone = new PowerPort();
            // Asset will be set by caller

            clone.PowerPortInfo.Name = source.PowerPortInfo.Name;
            clone.PowerPortInfo.PortNumber = source.PowerPortInfo.PortNumber;
            clone.PowerPortInfo.PortExposure = source.PowerPortInfo.PortExposure;
            clone.PowerPortInfo.OutputType = source.PowerPortInfo.OutputType;
            clone.PowerPortInfo.Label = source.PowerPortInfo.Label;

            return clone;
        }

        #endregion
    }
}