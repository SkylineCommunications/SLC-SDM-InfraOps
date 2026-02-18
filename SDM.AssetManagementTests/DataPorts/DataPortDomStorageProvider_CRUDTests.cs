namespace SDM.AssetManagement.Tests
{
	using System;
	using System.Linq;
	using FluentAssertions;
	using FluentAssertions.Execution;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using SDM.AssetManagement.Tests.Setup;
	//using Skyline.DataMiner.Analytics.GenericInterface.JoinFilter;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.SDM.AssetManagement;
	using Skyline.DataMiner.SDM.AssetManagement.Helpers;
	using Skyline.DataMiner.SDM.AssetManagement.Models;

	[TestClass]
	public partial class DataPortDomStorageProviderTests
	{
		private DataPort referenceDataPort;

		[TestInitialize]
		public void Init()
		{
			referenceDataPort = new DataPort
			{
				Identifier = Guid.NewGuid().ToString(),
				DataPortInfo = new DataPortInfo
				{
					Identifier = Guid.NewGuid().ToString(),
					Name = "Test DataPort",
					PortNumber = 1,
					OutputType = SlcAssetManagement.Enums.Outputtype.IO,
					PortExposure = SlcAssetManagement.Enums.PortExposure.Front,
					Type = Guid.NewGuid(),
					Label = "Ethernet Port 1",
				},
				Asset = new SdmObjectReference<Asset>(Guid.NewGuid().ToString()),
				AddressInfo = new AddressInfo
				{
					Ipv4Address = "192.168.1.100",
					Ipv6Address = "2001:0db8:85a3:0000:0000:8a2e:0370:7334",
					Hostname = "test-hostname",
					DNS = true,
				},
				PrimaryPortRelation = new PrimaryPortRelation
				{
					IsPrimaryIpv4 = true,
					IsPrimaryIpv6 = false,
				},
			};
		}

		[TestMethod]
		public void DataPortDomStorageProvider_EmptyDOM_Create()
		{
			var helper = RepositoryInitialize.InitializeEmptyRepositories();

			helper.DataPorts.Create(referenceDataPort);

			AssertCreated(helper);
		}

		[TestMethod]
		public void DataPortDomStorageProvider_EmptyDOM_CreateOrUpdate_Create()
		{
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.DataPorts.CreateOrUpdate([referenceDataPort]);

			AssertCreated(helper);
		}

		[TestMethod]
		public void DataPortDomStorageProvider_EmptyDOM_CreateOrUpdate_Update()
		{
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.DataPorts.Create(referenceDataPort);

			var updatedDataPort = new DataPort
			{
				Identifier = referenceDataPort.Identifier,
				DataPortInfo = new DataPortInfo
				{
					Identifier = Guid.NewGuid().ToString(),
					Name = "Updated DataPort Name",
					PortNumber = 2,
					OutputType = SlcAssetManagement.Enums.Outputtype.Out,
					PortExposure = SlcAssetManagement.Enums.PortExposure.Back,
					Label = "Fiber Port 2",
				},
				Asset = referenceDataPort.Asset,
				AddressInfo = new AddressInfo
				{
					Ipv4Address = "10.0.0.50",
					Ipv6Address = "",
					Hostname = "updated-hostname",
					DNS = false,
				},
				PrimaryPortRelation = new PrimaryPortRelation
				{
					IsPrimaryIpv4 = false,
					IsPrimaryIpv6 = true,
				},
			};

			helper.DataPorts.CreateOrUpdate([updatedDataPort]);

			AssertDataPortUpdateDifferences(referenceDataPort, updatedDataPort);
		}

		[TestMethod]
		public void DataPortDomStorageProvider_ReadPaged()
		{
			const int pageCount = 2;
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateDataPorts();

			FilterElement<DataPort> allFilter = new ORFilterElement<DataPort>();
			var pagedResult = helper.DataPorts.ReadPaged(allFilter, pageCount);
			var dataPortCount = helper.DataPorts.Count(allFilter);

			using (new AssertionScope())
			{
				pagedResult.Should().NotBeNull();
				pagedResult.Should().HaveCount((int)(dataPortCount / pageCount));
				pagedResult.Should().AllSatisfy(page => page.Should().HaveCount(pageCount));
			}
		}

		[TestMethod]
		public void DataPortDomStorageProvider_DeleteBulk()
		{
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateDataPorts();

			var filter = new ORFilterElement<DataPort>(
				DataPortExposers.DataPortInfo.Name.Equal("Data Port 3"),
				DataPortExposers.DataPortInfo.Label.Equal("Data Port Label 7"));
			var dataPortsToDelete = helper.DataPorts.Read(filter);

			helper.DataPorts.Delete(dataPortsToDelete);

			using (new AssertionScope())
			{
				helper.DataPorts.Count(new TRUEFilterElement<DataPort>()).Should().Be(DemoData.DataPorts.Count - 2);
				helper.DataPorts.Count(DataPortExposers.DataPortInfo.Name.Equal("Data Port 3")).Should().Be(0);
				helper.DataPorts.Count(DataPortExposers.DataPortInfo.Label.Equal("Data Port Label 7")).Should().Be(0);
			}
		}

		[TestMethod]
		public void DataPortDomStorageProvider_EmptyDOM_DeleteSingle()
		{
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateDataPorts();

			var dataPortToDelete = helper.DataPorts.Read(DataPortExposers.DataPortInfo.Name.Equal("Data Port 3")).First();

			helper.DataPorts.Delete(dataPortToDelete);

			helper.DataPorts.Count(new TRUEFilterElement<DataPort>()).Should().Be(DemoData.DataPorts.Count - 1);
			helper.DataPorts.Count(DataPortExposers.Identifier.Equal(dataPortToDelete.Identifier)).Should().Be(0);
		}

		private static void AssertDataPortUpdateDifferences(DataPort original, DataPort updated)
		{
			using (new AssertionScope())
			{
				updated.Identifier.Should().Be(original.Identifier);

				// Name
				updated.DataPortInfo.Name.Should().NotBe(original.DataPortInfo.Name);
				updated.DataPortInfo.Name.Should().Be("Updated DataPort Name");

				// PortNumber
				updated.DataPortInfo.PortNumber.Should().NotBe(original.DataPortInfo.PortNumber);
				updated.DataPortInfo.PortNumber.Should().Be(2);

				// OutputType
				updated.DataPortInfo.OutputType.Should().NotBe(original.DataPortInfo.OutputType);
				updated.DataPortInfo.OutputType.Should().Be(SlcAssetManagement.Enums.Outputtype.Out);

				// PortExposure
				updated.DataPortInfo.PortExposure.Should().NotBe(original.DataPortInfo.PortExposure);
				updated.DataPortInfo.PortExposure.Should().Be(SlcAssetManagement.Enums.PortExposure.Back);

				// PortType
				updated.DataPortInfo.Type.Should().NotBe(original.DataPortInfo.Type);

				// Label
				updated.DataPortInfo.Label.Should().NotBe(original.DataPortInfo.Label);
				updated.DataPortInfo.Label.Should().Be("Fiber Port 2");

				// Asset
				updated.Asset.Should().Be(original.Asset);

				// AddressInfo
				updated.AddressInfo.Should().NotBeNull();
				updated.AddressInfo.Ipv4Address.Should().Be("10.0.0.50");
				updated.AddressInfo.Ipv6Address.Should().BeNullOrEmpty();
				updated.AddressInfo.Hostname.Should().Be("updated-hostname");
				updated.AddressInfo.DNS.Should().BeFalse();

				// PrimaryPortRelation
				updated.PrimaryPortRelation.Should().NotBeNull();
				updated.PrimaryPortRelation.IsPrimaryIpv4.Should().BeFalse();
				updated.PrimaryPortRelation.IsPrimaryIpv6.Should().BeTrue();
			}
		}

		private void AssertCreated(IAssetManagementApiHelper helper)
		{
			using (new AssertionScope())
			{
				helper.DataPorts.Count(new TRUEFilterElement<DataPort>()).Should().Be(1);

				var createdDataPort = helper.DataPorts.Read(new TRUEFilterElement<DataPort>()).First();
				createdDataPort.Should().NotBeNull();
				createdDataPort.DataPortInfo.Name.Should().Be("Test DataPort");
				createdDataPort.DataPortInfo.PortNumber.Should().Be(1);
				createdDataPort.DataPortInfo.OutputType.Should().Be(SlcAssetManagement.Enums.Outputtype.IO);
				createdDataPort.DataPortInfo.PortExposure.Should().Be(SlcAssetManagement.Enums.PortExposure.Front);
				createdDataPort.DataPortInfo.Label.Should().Be("Ethernet Port 1");

				createdDataPort.Asset.Should().NotBeNull();
				createdDataPort.Asset.Should().BeAssignableTo<SdmObjectReference<Asset>>();

				createdDataPort.AddressInfo.Should().NotBeNull();
				createdDataPort.AddressInfo.Ipv4Address.Should().Be("192.168.1.100");
				createdDataPort.AddressInfo.Ipv6Address.Should().Be("2001:0db8:85a3:0000:0000:8a2e:0370:7334");
				createdDataPort.AddressInfo.Hostname.Should().Be("test-hostname");
				createdDataPort.AddressInfo.DNS.Should().BeTrue();

				createdDataPort.PrimaryPortRelation.Should().NotBeNull();
				createdDataPort.PrimaryPortRelation.IsPrimaryIpv4.Should().BeTrue();
				createdDataPort.PrimaryPortRelation.IsPrimaryIpv6.Should().BeFalse();
			}
		}
	}
}
