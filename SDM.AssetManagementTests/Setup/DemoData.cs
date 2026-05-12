namespace SDM.AssetManagement.Tests.Setup
{
    using System;
    using System.Collections.Generic;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;

    internal class DemoData
    {
        private static readonly Random _random = new Random();

        // DeviceTypes must be defined first since they have no dependencies
        public static readonly List<DeviceType> DeviceTypes =
        [
            CreateDeviceType("Decoder",  SlcAsset_Management.Enums.HierarchyRoleEnum.Chassis, SlcAsset_Management.Enums.TagOption.AcceptsDataConnection),
            CreateDeviceType("Encoder", SlcAsset_Management.Enums.HierarchyRoleEnum.Chassis, SlcAsset_Management.Enums.TagOption.AcceptsDataConnection),
            CreateDeviceType("Network Interface Card", SlcAsset_Management.Enums.HierarchyRoleEnum.Card, SlcAsset_Management.Enums.TagOption.AcceptsDataConnection, SlcAsset_Management.Enums.TagOption.RackUnitConsumer),
            CreateDeviceType("Software", SlcAsset_Management.Enums.HierarchyRoleEnum.None),
            CreateDeviceType("Firewall", SlcAsset_Management.Enums.HierarchyRoleEnum.Module, SlcAsset_Management.Enums.TagOption.AcceptsDataConnection),
            CreateDeviceType("PSU", SlcAsset_Management.Enums.HierarchyRoleEnum.PowerSupply, SlcAsset_Management.Enums.TagOption.PowerProvider),
            CreateDeviceType("Optics Module", SlcAsset_Management.Enums.HierarchyRoleEnum.PowerSupply, SlcAsset_Management.Enums.TagOption.PowerProvider),
            CreateDeviceType("Cooling Fan", SlcAsset_Management.Enums.HierarchyRoleEnum.Fan),
            CreateDeviceType("UPS System", SlcAsset_Management.Enums.HierarchyRoleEnum.Chassis, SlcAsset_Management.Enums.TagOption.PowerProvider, SlcAsset_Management.Enums.TagOption.RackUnitConsumer),
            CreateDeviceType("Enterprise Storage Drive", SlcAsset_Management.Enums.HierarchyRoleEnum.Module, SlcAsset_Management.Enums.TagOption.AcceptsDataConnection),
        ];

        // AssetClasses reference DeviceTypes
        public static readonly List<AssetClass> AssetClasses =
        [
            CreateAssetClass(1, "Router", "High performance router", DeviceTypes[0].Identifier, 44.5, 4.4, 43.9, 1, 12.5, "router-front.png", "router-back.jpg", 120, 200),
            CreateAssetClass(2, "Switch", "Layer 2 switch", DeviceTypes[1].Identifier, 30.0, 8.9, 44.0, 2, 3.5, "switch-front.png", "switch-back.png", 40, 60),
            CreateAssetClass(3, "Firewall", "Enterprise firewall", DeviceTypes[4].Identifier, 20.0, 13.3, 20.0, 3, 2.0, "fw-front.png", "fw-back.jpg", 30, 50),
            CreateAssetClass(4, "Server", "Rack server", DeviceTypes[0].Identifier, 70.0, 8.9, 44.0, 2, 25.0, "server-front.png", "server-back.png", 350, 500),
            CreateAssetClass(5, "Storage", "NAS storage", DeviceTypes[9].Identifier, 60.0, 8.9, 44.0, 2, 20.0, "nas-front.png", "nas-back.png", 250, 400),
            CreateAssetClass(6, "UPS", "Uninterruptible Power Supply", DeviceTypes[8].Identifier, 40.0, 8.9, 17.0, 2, 28.0, "ups-front.png", "ups-back.jpg", 120, 180),
            CreateAssetClass(7, "KVM Switch", "Keyboard Video Mouse Switch", DeviceTypes[3].Identifier, 16.0, 31.1, 44.0, 7, 2.2, "kvm-front.png", "kvm-back.png", 15, 25),
            CreateAssetClass(8, "Patch Panel", "24-port Patch Panel", DeviceTypes[2].Identifier, 10.0, 4.4, 48.0, 1, 1.5, "patchpanel-front.png", "patchpanel-back.png", 5, 10),
            CreateAssetClass(9, "Wireless Access Point", "Dual-band WiFi 6 AP", DeviceTypes[2].Identifier, 20.0, 4.4, 20.0, 1, 0.8, "ap-front.png", "ap-back.jpeg", 12, 18),
            CreateAssetClass(10, "Media Converter", "Fiber to Ethernet Converter", DeviceTypes[6].Identifier, 9.4, 4.4, 7.0, 1, 0.3, "mediaconv-front.png", "mediaconv-back.png", 3, 5),
        ];

        // Assets reference AssetClasses
        public static readonly List<Asset> Assets =
        [
            // Assets get the name "Test Asset {i}"
            CreateAsset(1, AssetClasses[0].Identifier, "SN123456", "00-14-22-01-23-41"),
            CreateAsset(2, AssetClasses[1].Identifier, "SN123457", "00-14-22-01-23-42"),
            CreateAsset(3, AssetClasses[2].Identifier, "SN123458", "00-14-22-01-23-43"),
            CreateAsset(4, AssetClasses[3].Identifier, "SN123459", "00-14-22-01-23-44"),
            CreateAsset(5, AssetClasses[4].Identifier, "SN123460", "00-14-22-01-23-45"),
            CreateAsset(6, AssetClasses[5].Identifier, "SN123461", "00-14-22-01-23-46"),
            CreateAsset(7, AssetClasses[6].Identifier, "SN123462", "00-14-22-01-23-47"),
            CreateAsset(8, AssetClasses[7].Identifier, "SN123463", "00-14-22-01-23-48"),
            CreateAsset(9, AssetClasses[8].Identifier, "SN123464", "00-14-22-01-23-49"),
            CreateAsset(10, AssetClasses[9].Identifier, "SN123465", "00-14-22-01-23-50"),
        ];

        // Ports reference Assets (actual port instances)
        public static readonly List<DataPort> DataPorts =
        [
            CreateDataPort(0),
            CreateDataPort(1),
            CreateDataPort(2),
            CreateDataPort(3),
            CreateDataPort(4),
            CreateDataPort(5),
            CreateDataPort(6),
            CreateDataPort(7),
            CreateDataPort(8),
            CreateDataPort(9),
        ];

        public static readonly List<PowerPort> PowerPorts =
        [
            CreatePowerPort(0),
            CreatePowerPort(1),
            CreatePowerPort(2),
            CreatePowerPort(3),
            CreatePowerPort(4),
            CreatePowerPort(5),
            CreatePowerPort(6),
            CreatePowerPort(7),
            CreatePowerPort(8),
            CreatePowerPort(9),
        ];

        // Facilities (Racks, in this case) - no direct references in Assets or AssetClasses yet
        public static readonly List<Rack> Racks =
        [
            CreateRack("RACK-001", "Main Server Rack", 42),
            CreateRack("RACK-002", "Network Equipment Rack", 42),
            CreateRack("RACK-003", "Storage Rack", 42),
        ];

        #region Asset Port Instances (DataPort, PowerPort)

        private static PowerPort CreatePowerPort(int i)
        {
            return new PowerPort
            {
                Identifier = Guid.NewGuid().ToString(),
                PowerPortInfo = new PowerPortInfo
                {
                    Identifier = Guid.NewGuid().ToString(),
                    Name = $"Power Port {i}",
                    PortNumber = i,
                    PortExposure = (SlcAsset_Management.Enums.PortExposureEnum)(i % 2),
                    OutputType = (SlcAsset_Management.Enums.Outputtype)(i % 3),
                    Label = $"Power Port Label {i}",
                },
                Asset = new SdmObjectReference<Asset>(Assets[i].Identifier),
            };
        }

        private static DataPort CreateDataPort(int i)
        {
            return new DataPort
            {
                Identifier = Guid.NewGuid().ToString(),
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
                AssetFk = new AssetRelation
                {
                    Asset = new SdmObjectReference<Asset>(Assets[i].Identifier),
                },
                PrimaryPortRelation = new PrimaryPortRelation
                {
                    IsPrimaryIpv4 = true,
                    IsPrimaryIpv6 = false,
                },
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
                    Identifier = Guid.NewGuid().ToString(),
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

        private static Asset CreateAsset(
            int orderNo,
            string assetClassIdentifier,
            string serialNumber,
            string macAddress)
        {
            var assetId = Guid.NewGuid();
            
            // Use one of the defined racks (cycle through them)
            var rackIndex = (orderNo - 1) % Racks.Count;
            var selectedRack = Racks[rackIndex];
            
            return new Asset
            {
                State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available,
                AssetID = assetId.ToString(),
                Name = $"Test Asset {orderNo}",
                AssetClassId = new SdmObjectReference<AssetClass>(assetClassIdentifier),
                Description = $"Sample asset {orderNo}",
                FW_OS = $"FW1.{orderNo}",
                SerialNumber = serialNumber,
                HardwareVersion = $"HW1.{orderNo}",
                MacAddress = macAddress,
                Location = new AssetLocation
                {
                    RackId = new SdmObjectReference<Rack>(selectedRack.Identifier),
                    RackPosition = orderNo,
                    Side = SlcAsset_Management.Enums.SideEnum.Back,
                },
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

        private static AssetClass CreateAssetClass(
            int orderNo,
            string deviceName,
            string deviceDescription,
            string deviceTypeIdentifier,
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
                Name = deviceName,
                DeviceTypeId = new SdmObjectReference<DeviceType>(deviceTypeIdentifier),
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
                // Port templates (specifications) - NOT actual instances
                DataPorts = GenerateRandomDataPortInfos(),
                PowerPorts = GenerateRandomPowerPortInfos(),
                Holders = new List<AssetHolder>
                {
                    new AssetHolder
                    {
                        HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Module,
                        SlotNumber = orderNo,
                    },
                },
            };
        }

        private static DeviceType CreateDeviceType(string name, SlcAsset_Management.Enums.HierarchyRoleEnum role, params SlcAsset_Management.Enums.TagOption[] tags)
        {
            var tagsList = tags.Length == 0 ? new List<SlcAsset_Management.Enums.TagOption>() : [.. tags];
            return new DeviceType
            {
                Identifier = Guid.NewGuid().ToString(),
                Description = $"Device type for {name}",
                Name = name,
                HierarchyInfo = new HierarchyInfo
                {
                    HierarchyRole = role,
                },
                TagsInfo = new TagsInfo
                {
                    Identifier = Guid.NewGuid().ToString(),
                    Tags = tagsList,
                },
            };
        }

        private static Rack CreateRack(string rackId, string name, int heightu)
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
            };
        }
    }
}