namespace SDM.AssetManagement.Tests.Setup
{
    using System;
    using System.Collections.Generic;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;

    internal static class DemoData
    {
        private const string RackId001 = "RACK-001";
        private const string RackId002 = "RACK-002";
        private const string RackId003 = "RACK-003";
        private const string RackId004 = "RACK-004";
        private const string SN123456 = "SN123456";
        private const string SN123457 = "SN123457";
        private const string SN123458 = "SN123458";
        private const string SN123459 = "SN123459";
        private const string SN123460 = "SN123460";
        private const string SN123461 = "SN123461";
        private const string SN123462 = "SN123462";
        private const string SN123463 = "SN123463";
        private const string SN123464 = "SN123464";
        private const string SN123465 = "SN123465";

        private const string AssetClassName_Router = "Router";
        private const string AssetClassName_Switch = "Switch";
        private const string AssetClassName_Firewall = "Firewall";
        private const string AssetClassName_Server = "Server";
        private const string AssetClassName_Storage = "Storage";
        private const string AssetClassName_UPS = "UPS";
        private const string AssetClassName_KVMSwitch = "KVM Switch";
        private const string AssetClassName_PatchPanel = "Patch Panel";
        private const string AssetClassName_WirelessAccessPoint = "Wireless Access Point";
        private const string AssetClassName_MediaConverter = "Media Converter";

        private const string DeviceType_Decoder = "Decoder";
        private const string DeviceType_Encoder = "Encoder";
        private const string DeviceType_NetworkInterfaceCard = "Network Interface Card";
        private const string DeviceType_Software = "Software";
        private const string DeviceType_Firewall = "Firewall";
        private const string DeviceType_PSU = "PSU";
        private const string DeviceType_OpticsModule = "Optics Module";
        private const string DeviceType_CoolingFan = "Cooling Fan";
        private const string DeviceType_UPSSystem = "UPS System";
        private const string DeviceType_EnterpriseStorageDrive = "Enterprise Storage Drive";

        private static readonly Random _random = new Random();

        // Facilities (Racks, in this case) - no direct references in Assets or AssetClasses yet
        public static readonly List<Rack> Racks =
        [
            CreateRack(RackId001, "Main Server Rack", 42),
            CreateRack(RackId002, "Network Equipment Rack", 42),
            CreateRack(RackId003, "Storage Rack", 42),
            CreateRack(RackId004, "45 U Rack", 45, top: true),
        ];

        // DeviceTypes must be defined first since they have no dependencies
        public static readonly List<DeviceType> DeviceTypes =
        [
            CreateDeviceType(DeviceType_Decoder,  SlcAsset_Management.Enums.HierarchyRoleEnum.Chassis, SlcAsset_Management.Enums.TagOption.AcceptsDataConnection, SlcAsset_Management.Enums.TagOption.RackUnitConsumer),
            CreateDeviceType(DeviceType_Encoder, SlcAsset_Management.Enums.HierarchyRoleEnum.Chassis, SlcAsset_Management.Enums.TagOption.AcceptsDataConnection, SlcAsset_Management.Enums.TagOption.RackUnitConsumer),
            CreateDeviceType(DeviceType_NetworkInterfaceCard, SlcAsset_Management.Enums.HierarchyRoleEnum.Card, SlcAsset_Management.Enums.TagOption.AcceptsDataConnection, SlcAsset_Management.Enums.TagOption.RackUnitConsumer),
            CreateDeviceType(DeviceType_Software, SlcAsset_Management.Enums.HierarchyRoleEnum.None),
            CreateDeviceType(DeviceType_Firewall, SlcAsset_Management.Enums.HierarchyRoleEnum.Module, SlcAsset_Management.Enums.TagOption.AcceptsDataConnection, SlcAsset_Management.Enums.TagOption.RackUnitConsumer),
            CreateDeviceType(DeviceType_PSU, SlcAsset_Management.Enums.HierarchyRoleEnum.PowerSupply, SlcAsset_Management.Enums.TagOption.PowerProvider),
            CreateDeviceType(DeviceType_OpticsModule, SlcAsset_Management.Enums.HierarchyRoleEnum.PowerSupply, SlcAsset_Management.Enums.TagOption.PowerProvider),
            CreateDeviceType(DeviceType_CoolingFan, SlcAsset_Management.Enums.HierarchyRoleEnum.Fan),
            CreateDeviceType(DeviceType_UPSSystem, SlcAsset_Management.Enums.HierarchyRoleEnum.Chassis, SlcAsset_Management.Enums.TagOption.PowerProvider, SlcAsset_Management.Enums.TagOption.RackUnitConsumer),
            CreateDeviceType(DeviceType_EnterpriseStorageDrive, SlcAsset_Management.Enums.HierarchyRoleEnum.Module, SlcAsset_Management.Enums.TagOption.AcceptsDataConnection, SlcAsset_Management.Enums.TagOption.RackUnitConsumer),
        ];

        // AssetClasses reference DeviceTypes
        public static readonly List<AssetClass> BaseAssetClasses =
        [
            CreateBaseAssetClass(DeviceType_Decoder, AssetClassName_Router, "High performance router", 44.5, 4.4, 43.9, 1, 12.5, "router-front.png", "router-back.jpg", 120, 200),
            CreateBaseAssetClass(DeviceType_Encoder, AssetClassName_Switch, "Layer 2 switch", 30.0, 8.9, 44.0, 2, 3.5, "switch-front.png", "switch-back.png", 40, 60),
            CreateBaseAssetClass(DeviceType_UPSSystem, AssetClassName_Firewall, "Enterprise firewall", 20.0, 13.3, 20.0, 3, 2.0, "fw-front.png", "fw-back.jpg", 30, 50),
            CreateBaseAssetClass(DeviceType_Encoder, AssetClassName_Server, "Rack server", 70.0, 8.9, 44.0, 2, 25.0, "server-front.png", "server-back.png", 350, 500),
            CreateBaseAssetClass(DeviceType_Encoder, AssetClassName_Storage, "NAS storage", 60.0, 8.9, 44.0, 2, 20.0, "nas-front.png", "nas-back.png", 250, 400),
            CreateBaseAssetClass(DeviceType_Encoder, AssetClassName_UPS, "Uninterruptible Power Supply", 40.0, 8.9, 17.0, 2, 28.0, "ups-front.png", "ups-back.jpg", 120, 180),
            CreateBaseAssetClass(DeviceType_Firewall, AssetClassName_KVMSwitch, "Keyboard Video Mouse Switch", 16.0, 31.1, 44.0, 7, 2.2, "kvm-front.png", "kvm-back.png", 15, 25),
            CreateBaseAssetClass(DeviceType_Decoder, AssetClassName_PatchPanel, "24-port Patch Panel", 10.0, 4.4, 48.0, 1, 1.5, "patchpanel-front.png", "patchpanel-back.png", 5, 10),
            CreateBaseAssetClass(DeviceType_UPSSystem, AssetClassName_WirelessAccessPoint, "Dual-band WiFi 6 AP", 20.0, 4.4, 20.0, 1, 0.8, "ap-front.png", "ap-back.jpeg", 12, 18),
            CreateBaseAssetClass(DeviceType_UPSSystem, AssetClassName_MediaConverter, "Fiber to Ethernet Converter", 9.4, 4.4, 7.0, 1, 0.3, "mediaconv-front.png", "mediaconv-back.png", 3, 5),
        ];

        // Base Assets - will update AssetClassId and RackId at runtime
        public static readonly List<Asset> BaseAssets =
        [
            CreateBaseAsset(1, SN123456, "00-14-22-01-23-41", AssetClassName_Router),   // Router (1U)
            CreateBaseAsset(2, SN123457, "00-14-22-01-23-42", AssetClassName_Switch),   // Switch (2U)
            CreateBaseAsset(3, SN123458, "00-14-22-01-23-43", AssetClassName_Firewall),   // Firewall (3U)
            CreateBaseAsset(4, SN123459, "00-14-22-01-23-44", AssetClassName_Server),   // Server (2U)
            CreateBaseAsset(5, SN123460, "00-14-22-01-23-45", AssetClassName_Storage),   // Storage (2U)
            CreateBaseAsset(6, SN123461, "00-14-22-01-23-46", AssetClassName_UPS),  // UPS (2U)
            CreateBaseAsset(7, SN123462, "00-14-22-01-23-47", AssetClassName_KVMSwitch),  // KVM (7U)
            CreateBaseAsset(8, SN123463, "00-14-22-01-23-48", AssetClassName_PatchPanel),  // Patch Panel (1U)
            CreateBaseAsset(9, SN123464, "00-14-22-01-23-49", AssetClassName_WirelessAccessPoint),  // AP (1U)
            CreateBaseAsset(10, SN123465, "00-14-22-01-23-50", AssetClassName_MediaConverter), // Media Converter (1U)
        ];

        /// <summary>
        /// Rack assignment for a single asset.
        /// </summary>
        public struct AssetRackPlacement
        {
            public string RackId { get; set; }
            public int Position { get; set; }
            public string Comment { get; set; }

            public AssetRackPlacement(string rackId, int position, string comment = null)
            {
                RackId = rackId;
                Position = position;
                Comment = comment;
            }
        }

        /// <summary>
        /// Explicit rack assignments for demo assets.
        /// Maps asset serial number to rack placement for deterministic, order-independent placement.
        /// </summary>
        public static readonly Dictionary<string, AssetRackPlacement> AssetRackAssignments = new Dictionary<string, AssetRackPlacement>
        {
            // Rack 0 (RACK-001)
            { SN123456, new AssetRackPlacement(RackId001, 1, "Router - 1U") },
            { SN123457, new AssetRackPlacement(RackId001, 2, "Switch - 2U") },
            { SN123458, new AssetRackPlacement(RackId001, 4, "Firewall - 3U") },
            { SN123459, new AssetRackPlacement(RackId001, 7, "Server - 2U") },
            
            // Rack 1 (RACK-002)
            { SN123460, new AssetRackPlacement(RackId002, 9, "Storage - 2U") },
            { SN123461, new AssetRackPlacement(RackId002, 11, "UPS - 2U") },
            { SN123462, new AssetRackPlacement(RackId002, 13, "KVM - 7U") },
            
            // Rack 2 (RACK-003)
            { SN123463, new AssetRackPlacement(RackId003, 20, "Patch Panel - 1U") },
            { SN123464, new AssetRackPlacement(RackId003, 21, "Access Point - 1U") },
            { SN123465, new AssetRackPlacement(RackId003, 22, "Media Converter - 1U") },
        };

        // Base DataPorts - will update Asset reference at runtime
        public static readonly List<DataPort> BaseDataPorts =
        [
            CreateBaseDataPort(0),
            CreateBaseDataPort(1),
            CreateBaseDataPort(2),
            CreateBaseDataPort(3),
            CreateBaseDataPort(4),
            CreateBaseDataPort(5),
            CreateBaseDataPort(6),
            CreateBaseDataPort(7),
            CreateBaseDataPort(8),
            CreateBaseDataPort(9),
        ];

        public static readonly List<PowerPort> BasePowerPorts =
        [
            CreateBasePowerPort(0),
            CreateBasePowerPort(1),
            CreateBasePowerPort(2),
            CreateBasePowerPort(3),
            CreateBasePowerPort(4),
            CreateBasePowerPort(5),
            CreateBasePowerPort(6),
            CreateBasePowerPort(7),
            CreateBasePowerPort(8),
            CreateBasePowerPort(9),
        ];

        #region Asset Port Instances (DataPort, PowerPort)

        private static DataPort CreateBaseDataPort(int i)
        {
            return new DataPort
            {
                DataPortInfo = new DataPortInfo
                {
                    Name = $"Data Port {i}",
                    PortNumber = i,
                    OutputType = SlcAsset_Management.Enums.Outputtype.Out,
                    PortExposure = (SlcAsset_Management.Enums.PortExposureEnum)(i % 2),
                    Label = $"Data Port Label {i}",
                },
                AddressInfo = new AddressInfo
                {
                    Ipv4Address = $"192.168.1.{i}",
                    Ipv6Address = $"2001:0db8:85a3:0000:0000:8a2e:0370:7{i:D3}",
                    Hostname = $"device{i}.example.com",
                    DNS = true,
                },
                // AssetFk will be set at runtime
                PrimaryPortRelation = new PrimaryPortRelation
                {
                    IsPrimaryIpv4 = true,
                    IsPrimaryIpv6 = false,
                },
            };
        }

        private static PowerPort CreateBasePowerPort(int i)
        {
            return new PowerPort
            {
                PowerPortInfo = new PowerPortInfo
                {
                    Name = $"Power Port {i}",
                    PortNumber = i,
                    PortExposure = (SlcAsset_Management.Enums.PortExposureEnum)(i % 2),
                    OutputType = (SlcAsset_Management.Enums.Outputtype)(i % 3),
                    Label = $"Power Port Label {i}",
                },
                // Asset will be set at runtime
            };
        }

        #endregion

        #region AssetClass Port Templates (DataPortInfo, PowerPortInfo)

        /// <summary>
        /// Generates random DataPortInfo templates for AssetClass.
        /// These are specifications/templates, not actual port instances.
        /// </summary>
        private static List<DataPortInfo> GenerateRandomDataPortInfos()
        {
            int portCount = _random.Next(1, 6); // 1 to 5 port templates
            var portInfos = new List<DataPortInfo>();

            for (int i = 1; i <= portCount; i++)
            {
                portInfos.Add(new DataPortInfo
                {
                    Name = $"Port {i}",
                    PortNumber = i,
                    OutputType = (SlcAsset_Management.Enums.Outputtype)(i % 3),
                    PortExposure = (SlcAsset_Management.Enums.PortExposureEnum)(i % 2),
                    Label = $"ETH{i}",
                });
            }

            return portInfos;
        }

        /// <summary>
        /// Generates random PowerPortInfo templates for AssetClass.
        /// These are specifications/templates, not actual port instances.
        /// </summary>
        private static List<PowerPortInfo> GenerateRandomPowerPortInfos()
        {
            int portCount = _random.Next(1, 4); // 1 to 3 power port templates
            var portInfos = new List<PowerPortInfo>();

            for (int i = 1; i <= portCount; i++)
            {
                portInfos.Add(new PowerPortInfo
                {
                    Name = $"PWR {i}",
                    PortNumber = i,
                    PortExposure = (SlcAsset_Management.Enums.PortExposureEnum)(i % 2),
                    OutputType = (SlcAsset_Management.Enums.Outputtype)(i % 3),
                    Label = $"PWR-{i}",
                });
            }

            return portInfos;
        }

        #endregion

        private static AssetClass CreateBaseAssetClass(
       string deviceTypeName,
       string deviceName,
       string deviceDescription,
       double depth,
       double height,
       double width,
       double heightU,
       double weight,
       string frontImage,
       string backImage,
       double typicalPowerConsumption,
       double maximumPowerConsumption)
        {
            return new AssetClass
            {
                DeviceTypeId = new SdmObjectReference<DeviceType>(deviceTypeName), // Will be updated at runtime to reference the correct DeviceType
                Name = deviceName,
                // DeviceTypeId will be set at runtime
                Description = deviceDescription,
                Manufacturer = Guid.NewGuid(),
                Depth = depth,
                Height = height,
                Width = width,
                HeightU = heightU,
                Weight = weight,
                FrontImage = frontImage,
                BackImage = backImage,
                TypicalPowerConsumption = typicalPowerConsumption,
                MaximumPowerConsumption = maximumPowerConsumption,
                PowerSupply = SlcAsset_Management.Enums.PowerSupplyEnum.AC,
                Lifecycle = new AssetClassLifecycle
                {
                    EndOfLife = DateTime.UtcNow.AddYears(10),
                    EndOfService = DateTime.UtcNow.AddYears(7),
                    NominalLifetime = TimeSpan.FromDays(365 * 7),
                },
                DataPorts = GenerateRandomDataPortInfos(),
                PowerPorts = GenerateRandomPowerPortInfos(),
                Holders = new List<AssetHolder>
            {
                new AssetHolder
                {
                    HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Module,
                    SlotNumber = 1,
                },
            },
            };
        }

        private static DeviceType CreateDeviceType(string name, SlcAsset_Management.Enums.HierarchyRoleEnum role, params SlcAsset_Management.Enums.TagOption[] tags)
        {
            var tagsList = tags.Length == 0 ? new List<SlcAsset_Management.Enums.TagOption>() : [.. tags];
            return new DeviceType
            {
                Description = $"Device type for {name}",
                Name = name,
                HierarchyInfo = new HierarchyInfo
                {
                    HierarchyRole = role,
                },
                TagsInfo = new TagsInfo
                {
                    Tags = tagsList,
                },
            };
        }

        private static Rack CreateRack(string rackId, string name, int heightu, bool top = false)
        {
            return new Rack
            {
                Identifier = Guid.NewGuid().ToString(),
                RackId = rackId,
                Name = name,
                Capacity = new RackCapacity
                {
                    MaximumRackCapacity = heightu,
                },
                Position = top == true ? SlcFacility_Management.Enums.RackpositionenumEnum.Top : SlcFacility_Management.Enums.RackpositionenumEnum.Bottom,
            };
        }

        private static Asset CreateBaseAsset(
            int orderNo,
            string serialNumber,
            string macAddress,
            string assetClassName)
        {
            var assetId = Guid.NewGuid();

            return new Asset
            {
                State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available,
                AssetClassId = new SdmObjectReference<AssetClass>(assetClassName), // Will be updated at runtime to reference the correct AssetClass
                AssetID = assetId.ToString(),
                Name = $"Test Asset {orderNo}",
                Description = $"Sample asset {orderNo}",
                FW_OS = $"FW1.{orderNo}",
                SerialNumber = serialNumber,
                HardwareVersion = $"HW1.{orderNo}",
                MacAddress = macAddress,
                PurchaseDate = DateTime.UtcNow.AddYears(-orderNo),
                FirstUseDate = DateTime.UtcNow.AddYears(-orderNo).AddMonths(2),
                EndOfWarrantyDate = DateTime.UtcNow.AddYears(-orderNo + 10),
                InstallationDate = DateTime.UtcNow.AddYears(-orderNo).AddMonths(1),
                InstallationUserId = Guid.NewGuid(),
                ModificationDate = DateTime.UtcNow,
                ModificationUserId = Guid.NewGuid(),
                EndOfLifeDate = DateTime.UtcNow.AddYears(-orderNo + 15),
                Ownership = new AssetOwnership
                {
                    Organization = Guid.NewGuid(),
                    ContactPerson = Guid.NewGuid(),
                    ContactPersonRole = Guid.NewGuid(),
                    Team = Guid.NewGuid(),
                },
                Custody = new AssetCustody
                {
                    From = DateTime.UtcNow.AddMonths(-6),
                    Till = DateTime.UtcNow.AddMonths(6),
                    ContactPerson = Guid.NewGuid(),
                    Team = Guid.NewGuid(),
                    Organization = Guid.NewGuid(),
                    ContactPersonRole = Guid.NewGuid(),
                },
                Holders = new List<AssetHolder>(),
                ElementLinks = new List<ElementLink>
                {
                    new ElementLink
                    {
                        IsPrimary = true,
                        ElementID = $"101/{orderNo}",
                    },
                },
            };
        }
    }
}