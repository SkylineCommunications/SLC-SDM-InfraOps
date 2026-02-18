namespace SDM.AssetManagement.Tests
{
	using System.Linq;
	using FluentAssertions;
	using FluentAssertions.Execution;
	using SDM.AssetManagement.Tests.Setup;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.SDM.AssetManagement.Models;

	public partial class DataPortDomStorageProviderTests
	{
		[TestMethod]
		public void DataPortDomStorageProvider_NestedReadFilter_LinkedAsset()
		{
			// 10 assets
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateAssets().PopulateDataPorts();

			// Link more ports to one asset.
			var asset = DemoData.Assets[6];
			DemoData.DataPorts[2].Asset = new SdmObjectReference<Asset>(asset.Identifier);
			DemoData.DataPorts[5].Asset = new SdmObjectReference<Asset>(asset.Identifier);

			helper.DataPorts.Update(DemoData.DataPorts);

			var filter = DataPortExposers.Asset.Equal(new SdmObjectReference<Asset>(asset.Identifier));

			var dataPortsRetrieved = helper.DataPorts.Read(filter);
			var expected = DemoData.DataPorts.Where(filter.getLambda());

			using (new AssertionScope())
			{
				dataPortsRetrieved.Should().NotBeNull();
				dataPortsRetrieved.Count().Should().Be(3);

				dataPortsRetrieved.Should().BeEquivalentTo(expected);
			}
		}

		[TestMethod]
		public void DataPortDomStorageProvider_ReadFilter_PortName_Equal()
		{
			// 10 data ports
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateDataPorts();

			var portName = DemoData.DataPorts[3].DataPortInfo.Name;
			var filter = DataPortExposers.DataPortInfo.Name.Equal(portName);

			var dataPortsRetrieved = helper.DataPorts.Read(filter);

			using (new AssertionScope())
			{
				dataPortsRetrieved.Should().NotBeNull();
				dataPortsRetrieved.Count().Should().Be(1);
				DataPort dataPort = dataPortsRetrieved.First();

				dataPort.DataPortInfo.Name.Should().Be(DemoData.DataPorts[3].DataPortInfo.Name);
				dataPort.Identifier.Should().Be(DemoData.DataPorts[3].Identifier);

				dataPort.AddressInfo.DNS.Should().Be(DemoData.DataPorts[3].AddressInfo.DNS);
				dataPort.AddressInfo.Hostname.Should().Be(DemoData.DataPorts[3].AddressInfo.Hostname);
				dataPort.AddressInfo.Ipv4Address.Should().Be(DemoData.DataPorts[3].AddressInfo.Ipv4Address);
				dataPort.AddressInfo.Ipv6Address.Should().Be(DemoData.DataPorts[3].AddressInfo.Ipv6Address);

				dataPort.DataPortInfo.Should().Be(DemoData.DataPorts[3].DataPortInfo);

				dataPort.PrimaryPortRelation.IsPrimaryIpv4.Should().Be(DemoData.DataPorts[3].PrimaryPortRelation.IsPrimaryIpv4);
				dataPort.PrimaryPortRelation.IsPrimaryIpv6.Should().Be(DemoData.DataPorts[3].PrimaryPortRelation.IsPrimaryIpv6);
			}
		}

		[TestMethod]
		public void DataPortDomStorageProvider_ReadFilter_PortExposure_Equal()
		{
			// 10 data ports
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateDataPorts();

			var portExposure = Skyline.DataMiner.SDM.AssetManagement.SlcAssetManagement.Enums.PortExposure.Back;
			var filter = DataPortExposers.DataPortInfo.PortExposure.UncheckedEqual(portExposure);

			var dataPortsRetrieved = helper.DataPorts.Read(filter);
			var expected = DemoData.DataPorts.Where(filter.getLambda());

			using (new AssertionScope())
			{
				dataPortsRetrieved.Should().NotBeNull();
				dataPortsRetrieved.Should().BeEquivalentTo(expected);
				dataPortsRetrieved.Should().AllSatisfy(port => port.DataPortInfo.PortExposure.Should().Be(portExposure));
			}
		}

		[TestMethod]
		public void DataPortDomStorageProvider_NestedReadFilter_PrimaryPortRelation_IsIpV4_Equal()
		{
			// 10 data ports
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateDataPorts();

			var filter = DataPortExposers.PrimaryPortRelation.IsPrimaryIpv4.Equal(true);

			var dataPortsRetrieved = helper.DataPorts.Read(filter);
			var expected = DemoData.DataPorts.Where(filter.getLambda());

			using (new AssertionScope())
			{
				dataPortsRetrieved.Should().NotBeNull();
				dataPortsRetrieved.Should().BeEquivalentTo(expected);
				dataPortsRetrieved.Should().AllSatisfy(port => port.PrimaryPortRelation.IsPrimaryIpv4.Should().BeTrue());
			}
		}

		[TestMethod]
		public void DataPortDomStorageProvider_NestedReadFilter_AddressInfo_Hostname_Equal()
		{
			// 10 data ports
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateDataPorts();

			var filter = DataPortExposers.AddressInfo.Hostname.Equal("device4.example.com");

			var dataPortsRetrieved = helper.DataPorts.Read(filter);

			using (new AssertionScope())
			{
				dataPortsRetrieved.Should().NotBeNull();
				dataPortsRetrieved.Should().HaveCount(1);

				var dataPort = dataPortsRetrieved.First();

				dataPort.Should().Be(DemoData.DataPorts[4]);
			}
		}

		[TestMethod]
		public void DataPortDomStorageProvider_NestedReadFilter_AddressInfo_Ipv4Address_Contains()
		{
			// 10 data ports
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateDataPorts();

			// Match the last two octets of the IPv4 address of data port 9:
			var filter = DataPortExposers.AddressInfo.Ipv4Address.Contains("1.9");

			var dataPortsRetrieved = helper.DataPorts.Read(filter);

			using (new AssertionScope())
			{
				dataPortsRetrieved.Should().NotBeNull();
				dataPortsRetrieved.Should().HaveCount(1);

				var dataPort = dataPortsRetrieved.First();

				dataPort.Should().Be(DemoData.DataPorts[9]);
			}
		}

		[TestMethod]
		public void DataPortDomStorageProvider_ReadFilter_PortNumber_Equal()
		{
			// 10 data ports
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateDataPorts();

			var portNumber = DemoData.DataPorts[8].DataPortInfo.PortNumber;
			var filter = DataPortExposers.DataPortInfo.PortNumber.Equal(portNumber);

			var dataPortsRetrieved = helper.DataPorts.Read(filter);
			var expected = DemoData.DataPorts.Where(filter.getLambda());

			using (new AssertionScope())
			{
				dataPortsRetrieved.Should().NotBeNull();
				dataPortsRetrieved.Should().HaveCount(1);
				var dataPort = dataPortsRetrieved.First();

				dataPort.Should().Be(expected.First());
			}
		}

		[TestMethod]
		public void DataPortDomStorageProvider_ReadFilter_Label_Equal()
		{
			// 10 data ports
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateDataPorts();

			var label = DemoData.DataPorts[5].DataPortInfo.Label;
			var filter = DataPortExposers.DataPortInfo.Label.Equal(label);

			var dataPortsRetrieved = helper.DataPorts.Read(filter);
			var expected = DemoData.DataPorts.Where(filter.getLambda());

			using (new AssertionScope())
			{
				dataPortsRetrieved.Should().NotBeNull();
				dataPortsRetrieved.Should().HaveCount(1);

				var dataPort = dataPortsRetrieved.First();
				dataPort.Should().Be(expected.First());
			}
		}
	}
}
