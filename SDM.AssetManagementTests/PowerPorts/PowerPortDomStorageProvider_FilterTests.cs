namespace SDM.AssetManagement.Tests
{
	using System.Diagnostics;
	using System.Linq;
	using FluentAssertions;
	using FluentAssertions.Execution;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using SDM.AssetManagement.Tests.Setup;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.SDM.AssetManagement.Models;

	public partial class PowerPortDomStorageProviderTests
	{
		[TestMethod]
		public void PowerPortDomStorageProvider_NestedReadFilter_LinkedAsset()
		{
			// 10 assets
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateAssets().PopulatePowerPorts();

			// Link more ports to one asset.
			var asset = DemoData.Assets[6];
			DemoData.PowerPorts[2].Asset = new SdmObjectReference<Asset>(asset.Identifier);
			DemoData.PowerPorts[5].Asset = new SdmObjectReference<Asset>(asset.Identifier);

			helper.PowerPorts.Update(DemoData.PowerPorts);

			var filter = PowerPortExposers.Asset.Equal(new SdmObjectReference<Asset>(asset.Identifier));

			var powerPortsRetrieved = helper.PowerPorts.Read(filter);
			var expected = DemoData.PowerPorts.Where(filter.getLambda());

			using (new AssertionScope())
			{
				powerPortsRetrieved.Should().NotBeNull();
				powerPortsRetrieved.Count().Should().Be(3);

				powerPortsRetrieved.Should().BeEquivalentTo(expected);
			}
		}

		[TestMethod]
		public void PowerPortDomStorageProvider_ReadFilter_PortName_Equal()
		{
			// 10 power ports
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulatePowerPorts();

			var portName = DemoData.PowerPorts[3].PowerPortInfo.Name;
			var filter = PowerPortExposers.PowerPortInfo.Name.Equal(portName);

			var powerPortsRetrieved = helper.PowerPorts.Read(filter);

			using (new AssertionScope())
			{
				powerPortsRetrieved.Should().NotBeNull();
				powerPortsRetrieved.Count().Should().Be(1);
				PowerPort powerPort = powerPortsRetrieved.First();

				powerPort.PowerPortInfo.Name.Should().Be(DemoData.PowerPorts[3].PowerPortInfo.Name);
				powerPort.Identifier.Should().Be(DemoData.PowerPorts[3].Identifier);

				powerPort.PowerPortInfo.Should().Be(DemoData.PowerPorts[3].PowerPortInfo);

				powerPort.PrimaryPortRelation.IsPrimaryIpv4.Should().Be(DemoData.PowerPorts[3].PrimaryPortRelation.IsPrimaryIpv4);
				powerPort.PrimaryPortRelation.IsPrimaryIpv6.Should().Be(DemoData.PowerPorts[3].PrimaryPortRelation.IsPrimaryIpv6);
			}
		}

		[TestMethod]
		public void PowerPortDomStorageProvider_ReadFilter_PortExposure_Equal()
		{
			// 10 power ports
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulatePowerPorts();

			var portExposure = Skyline.DataMiner.SDM.AssetManagement.SlcAssetManagement.Enums.PortExposure.Back;
			var filter = PowerPortExposers.PowerPortInfo.PortExposure.UncheckedEqual(portExposure);

			var powerPortsRetrieved = helper.PowerPorts.Read(filter);
			var expected = DemoData.PowerPorts.Where(filter.getLambda());

			using (new AssertionScope())
			{
				powerPortsRetrieved.Should().NotBeNull();
				powerPortsRetrieved.Should().BeEquivalentTo(expected);
				powerPortsRetrieved.Should().AllSatisfy(port => port.PowerPortInfo.PortExposure.Should().Be(portExposure));
			}
		}

		[TestMethod]
		public void PowerPortDomStorageProvider_ReadFilter_OutputType_Equal()
		{
			// 10 power ports
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulatePowerPorts();

			var outputType = Skyline.DataMiner.SDM.AssetManagement.SlcAssetManagement.Enums.Outputtype.IO;
			var filter = PowerPortExposers.PowerPortInfo.OutputType.UncheckedEqual(outputType);

			var powerPortsRetrieved = helper.PowerPorts.Read(filter);
			var expected = DemoData.PowerPorts.Where(filter.getLambda());

			using (new AssertionScope())
			{
				powerPortsRetrieved.Should().NotBeNull();
				powerPortsRetrieved.Should().BeEquivalentTo(expected);
				powerPortsRetrieved.Should().AllSatisfy(port => port.PowerPortInfo.OutputType.Should().Be(outputType));
			}
		}

		[TestMethod]
		public void PowerPortDomStorageProvider_NestedReadFilter_PrimaryPortRelation_IsIpV6_Equal()
		{
			// 10 power ports
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulatePowerPorts();

			var filter = PowerPortExposers.PrimaryPortRelation.IsPrimaryIpv6.Equal(true);

			var powerPortsRetrieved = helper.PowerPorts.Read(filter);
			var expected = DemoData.PowerPorts.Where(filter.getLambda());

			using (new AssertionScope())
			{
				powerPortsRetrieved.Should().NotBeNull();
				powerPortsRetrieved.Should().BeEquivalentTo(expected);
				powerPortsRetrieved.Should().AllSatisfy(port => port.PrimaryPortRelation.IsPrimaryIpv6.Should().BeTrue());
			}
		}

		[TestMethod]
		public void PowerPortDomStorageProvider_ReadFilter_PortNumber_Equal()
		{
			// 10 power ports
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulatePowerPorts();

			var portNumber = DemoData.PowerPorts[7].PowerPortInfo.PortNumber;
			var filter = PowerPortExposers.PowerPortInfo.PortNumber.Equal(portNumber);

			var powerPortsRetrieved = helper.PowerPorts.Read(filter);

			using (new AssertionScope())
			{
				powerPortsRetrieved.Should().NotBeNull();
				powerPortsRetrieved.Should().HaveCount(1);

				var powerPort = powerPortsRetrieved.First();

				powerPort.Should().Be(DemoData.PowerPorts[7]);
			}
		}

		[TestMethod]
		public void PowerPortDomStorageProvider_ReadFilter_Label_Contains()
		{
			// 10 power ports
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulatePowerPorts();

			var filter = PowerPortExposers.PowerPortInfo.Label.Contains("Power");

			var powerPortsRetrieved = helper.PowerPorts.Read(filter);
			var expected = DemoData.PowerPorts.Where(filter.getLambda());

			using (new AssertionScope())
			{
				powerPortsRetrieved.Should().NotBeNull();
				powerPortsRetrieved.Should().BeEquivalentTo(expected);
				powerPortsRetrieved.Should().AllSatisfy(port => port.PowerPortInfo.Label.Should().Contain("Power"));
			}
		}

		[TestMethod]
		public void PowerPortDomStorageProvider_NestedReadFilter_IsIpV4_Equal_True()
		{
			// 10 power ports
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulatePowerPorts();

			var filter = PowerPortExposers.PrimaryPortRelation.IsPrimaryIpv4.Equal(true);

			var powerPortsRetrieved = helper.PowerPorts.Read(filter);
			var expected = DemoData.PowerPorts.Where(pp => pp.PrimaryPortRelation.IsPrimaryIpv4);

			using (new AssertionScope())
			{
				powerPortsRetrieved.Should().NotBeNull();
				powerPortsRetrieved.Should().BeEmpty();
			}
		}
	}
}
