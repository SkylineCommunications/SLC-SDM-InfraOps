namespace SDM.AssetManagement.Tests.Setup
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.SDM.AssetManagement;
	using Skyline.DataMiner.SDM.AssetManagement.Models;
	using static Skyline.DataMiner.SDM.AssetManagement.SlcAssetManagement.Enums;

	internal class DemoData
	{
		private static readonly Random _random = new Random();

		public static readonly List<Asset> Assets =
		[
			// Assets get the name "Test Asset {i}"
			CreateAsset(1, "SN123456", "00-14-22-01-23-41"),
			CreateAsset(2, "SN123457", "00-14-22-01-23-42"),
			CreateAsset(3, "SN123458", "00-14-22-01-23-43"),
			CreateAsset(4, "SN123459", "00-14-22-01-23-44"),
			CreateAsset(5, "SN123460", "00-14-22-01-23-45"),
			CreateAsset(6, "SN123461", "00-14-22-01-23-46"),
			CreateAsset(7, "SN123462", "00-14-22-01-23-47"),
			CreateAsset(8, "SN123463", "00-14-22-01-23-48"),
			CreateAsset(9, "SN123464", "00-14-22-01-23-49"),
			CreateAsset(10, "SN123465", "00-14-22-01-23-50"),
		];

		public static readonly List<AssetClass> AssetClasses =
		[
			CreateAssetClass(1, "Router", "High performance router", 44.5, 4.4, 43.9, 1, 12.5, "router-front.png", "router-back.jpg", 120, 200),
			CreateAssetClass(2, "Switch", "Layer 2 switch", 30.0, 8.9, 44.0, 2, 3.5, "switch-front.png", "switch-back.png", 40, 60),
			CreateAssetClass(3, "Firewall", "Enterprise firewall", 20.0, 13.3, 20.0, 3, 2.0, "fw-front.png", "fw-back.jpg", 30, 50),
			CreateAssetClass(4, "Server", "Rack server", 70.0, 8.9, 44.0, 2, 25.0, "server-front.png", "server-back.png", 350, 500),
			CreateAssetClass(5, "Storage", "NAS storage", 60.0, 8.9, 44.0, 2, 20.0, "nas-front.png", "nas-back.png", 250, 400),
			CreateAssetClass(6, "UPS", "Uninterruptible Power Supply", 40.0, 8.9, 17.0, 2, 28.0, "ups-front.png", "ups-back.jpg", 120, 180),
			CreateAssetClass(7, "KVM Switch", "Keyboard Video Mouse Switch", 16.0, 31.1, 44.0, 7, 2.2, "kvm-front.png", "kvm-back.png", 15, 25),
			CreateAssetClass(8, "Patch Panel", "24-port Patch Panel", 10.0, 4.4, 48.0, 1, 1.5, "patchpanel-front.png", "patchpanel-back.png", 5, 10),
			CreateAssetClass(9, "Wireless Access Point", "Dual-band WiFi 6 AP", 20.0, 4.4, 20.0, 1, 0.8, "ap-front.png", "ap-back.jpeg", 12, 18),
			CreateAssetClass(10, "Media Converter", "Fiber to Ethernet Converter", 9.4, 4.4, 7.0, 1, 0.3, "mediaconv-front.png", "mediaconv-back.png", 3, 5),
		];

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

		public static readonly List<DeviceType> DeviceTypes =
		[
			CreateDeviceType("Decoder", HierarchyRole.Chassis, TagOption.AcceptsDataConnection),
			CreateDeviceType("Encoder", HierarchyRole.Chassis, TagOption.AcceptsDataConnection),
			CreateDeviceType("Network Interface Card", HierarchyRole.Card, TagOption.AcceptsDataConnection, TagOption.RackUnitConsumer),
			CreateDeviceType("Software", HierarchyRole.None),
			CreateDeviceType("Firewall", HierarchyRole.Module, TagOption.AcceptsDataConnection),
			CreateDeviceType("PSU", HierarchyRole.PowerSupply, TagOption.PowerProvider),
			CreateDeviceType("Optics Module", HierarchyRole.PowerSupply, TagOption.PowerProvider),
			CreateDeviceType("Cooling Fan", HierarchyRole.Fan),
			CreateDeviceType("UPS System", HierarchyRole.Chassis, TagOption.PowerProvider, TagOption.RackUnitConsumer),
			CreateDeviceType("Enterprise Storage Drive", HierarchyRole.Module, TagOption.AcceptsDataConnection),
		];

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
					PortExposure = (SlcAssetManagement.Enums.PortExposure)(i % 2),
					OutputType = (SlcAssetManagement.Enums.Outputtype)(i % 3),
					Label = $"Power Port Label {i}",
				},
				Asset = new SdmObjectReference<Asset>(Assets[i].Identifier),
				PrimaryPortRelation = new PrimaryPortRelation
				{
					IsPrimaryIpv4 = false,
					IsPrimaryIpv6 = true,
				},
			};
		}

		private static DataPort CreateDataPort(int i)
		{
			return new DataPort
			{
				Identifier = Guid.NewGuid().ToString(),
				DataPortInfo = new DataPortInfo
				{
					Identifier = Guid.NewGuid().ToString(),
					Name = $"Data Port {i}",
					PortNumber = i,
					OutputType = SlcAssetManagement.Enums.Outputtype.Out,
					PortExposure = (SlcAssetManagement.Enums.PortExposure)(i % 2),
					Label = $"Data Port Label {i}",
				},
				AddressInfo = new AddressInfo
				{
					Ipv4Address = $"192.168.1.{i}",
					Ipv6Address = $"2001:0db8:85a3:0000:0000:8a2e:0370:7{i:D3}",
					Hostname = $"device{i}.example.com",
					DNS = true,
				},
				Asset = new SdmObjectReference<Asset>(Assets[i].Identifier),
				PrimaryPortRelation = new PrimaryPortRelation
				{
					IsPrimaryIpv4 = true,
					IsPrimaryIpv6 = false,
				},
			};
		}

		private static List<DataPort> GenerateRandomDataPorts()
		{
			int portCount = _random.Next(0, 6); // 0 to 5 ports
			var ports = new List<DataPort>();
			for (int i = 1; i <= portCount; i++)
			{
				ports.Add(CreateDataPort(i));
			}

			return ports;
		}

		private static List<PowerPort> GenerateRandomPowerPorts()
		{
			int portCount = _random.Next(0, 6); // 0 to 5 ports
			var ports = new List<PowerPort>();
			for (int i = 1; i <= portCount; i++)
			{
				ports.Add(CreatePowerPort(i));
			}

			return ports;
		}

		private static Asset CreateAsset(
			int orderNo,
			string serialNumber,
			string macAddress)
		{
			var assetId = Guid.NewGuid();
			return new Asset
			{
				Identifier = assetId.ToString(),
				AssetId = assetId.ToString(),
				AssetName = $"Test Asset {orderNo}",
				AssetClass = null,
				AssetDescription = $"Sample asset {orderNo}",
				FwOs = $"FW1.{orderNo}",
				Notes = "Test notes",
				SerialNumber = serialNumber,
				HardwareVersion = $"HW1.{orderNo}",
				NetworkDetails = new AssetNetworkDetails
				{
					MACAddress = macAddress,
				},
				Location = new AssetLocation
				{
					ParentAsset = new SdmObjectReference<Asset>(assetId.ToString()),
					RoomId = Guid.NewGuid(),
					RackId = Guid.NewGuid(),
					RackPosition = orderNo,
					ContainerId = Guid.NewGuid(),
					DeskId = Guid.NewGuid(),
					Side = SlcAssetManagement.Enums.Side.Back,
				},
				Lifecycle = new AssetLifecycle
				{
					PurchaseDate = DateTime.UtcNow.AddYears(-orderNo),
					FirstUseDate = DateTime.UtcNow.AddYears(-orderNo).AddMonths(2),
					EndOfWarrantyDate = DateTime.UtcNow.AddYears(-orderNo + 10),
					InstallationDate = DateTime.UtcNow.AddYears(-orderNo).AddMonths(1),
					InstallationUserId = Guid.NewGuid(),
					ModificationDate = DateTime.UtcNow,
					ModificationUserId = Guid.NewGuid(),
					EndOfLife = DateTime.UtcNow.AddYears(-orderNo + 15),
				},
				Ownership = new AssetOwnership
				{
					Organization = Guid.NewGuid(),
					ContactPersonId = Guid.NewGuid(),
					ContactPersonRoleId = Guid.NewGuid(),
					TeamId = Guid.NewGuid(),
				},
				Custody = new AssetCustody
				{
					From = DateTime.UtcNow.AddMonths(-6),
					Till = DateTime.UtcNow.AddMonths(6),
					ContactPersonId = Guid.NewGuid(),
					TeamId = Guid.NewGuid(),
					OrganizationId = Guid.NewGuid(),
					ContactPersonRoleId = Guid.NewGuid(),
				},
				Holders = new List<AssetHolder>(),
				ElementLinks = new List<ElementLink>
				{
					new ElementLink
					{
						Identifier = Guid.NewGuid().ToString(),
						ElementID = $"101/{orderNo}",
					},
				},
			};
		}

		private static AssetClass CreateAssetClass(
		int orderNo,
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
			var newId = Guid.NewGuid();
			return new AssetClass
			{
				Identifier = newId.ToString(),
				Id = newId,
				DeviceName = deviceName,
				DeviceTypeId = new SdmObjectReference<DeviceType>(Guid.NewGuid().ToString()),
				DeviceDescription = deviceDescription,
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
				PowerSupply = SlcAssetManagement.Enums.PowerSupply.AC,
				Lifecycle = new AssetClassLifecycle
				{
					EndOfLife = DateTime.UtcNow.AddYears(10),
					EndOfService = DateTime.UtcNow.AddYears(7),
					NominalLifetime = TimeSpan.FromDays(365 * 7),
				},
				DataPorts = GenerateRandomDataPorts().Select(dp => dp.DataPortInfo).ToList(),
				PowerPorts = GenerateRandomPowerPorts().Select(pp => pp.PowerPortInfo).ToList(),
				Holders = new List<AssetHolder>
				{
					new AssetHolder
					{
						Identifier = Guid.NewGuid().ToString(),
						HierarchyRole = SlcAssetManagement.Enums.HierarchyRole.Module,
						SlotNumber = orderNo,
					},
				},
			};
		}

		private static DeviceType CreateDeviceType(string name, HierarchyRole role, params TagOption[] tags)
		{
			var tagsList = tags.Length == 0 ? new List<TagOption>() : [.. tags];
			return new DeviceType
			{
				Identifier = Guid.NewGuid().ToString(),
				Description = $"Device type for {name}",
				Name = name,
				HierarchyInfo = new HierarchyInfo
				{
					Identifier = Guid.NewGuid().ToString(),
					HierarchyRole = role,
				},
				TagsInfo = new TagsInfo
				{
					Identifier = Guid.NewGuid().ToString(),
					Tags = tagsList,
				},
			};
		}
	}
}