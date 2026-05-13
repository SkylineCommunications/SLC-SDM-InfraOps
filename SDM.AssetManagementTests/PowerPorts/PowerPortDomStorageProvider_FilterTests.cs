namespace SDM.AssetManagement.Tests
{
	using System.Diagnostics;
	using System.Linq;
	using FluentAssertions;
	using FluentAssertions.Execution;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using SDM.AssetManagement.Tests.Setup;

    using SharedMappers.DomIds;

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

			helper.AssetManagement.PowerPorts.Update(DemoData.PowerPorts);

			var filter = PowerPortExposers.Asset.Equal(new SdmObjectReference<Asset>(asset.Identifier));

			var powerPortsRetrieved = helper.AssetManagement.PowerPorts.Read(filter);
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

			var powerPortsRetrieved = helper.AssetManagement.PowerPorts.Read(filter);

			using (new AssertionScope())
			{
				powerPortsRetrieved.Should().NotBeNull();
				powerPortsRetrieved.Count().Should().Be(1);
				PowerPort powerPort = powerPortsRetrieved.First();

				powerPort.PowerPortInfo.Name.Should().Be(DemoData.PowerPorts[3].PowerPortInfo.Name);
				powerPort.Identifier.Should().Be(DemoData.PowerPorts[3].Identifier);

				powerPort.PowerPortInfo.Should().Be(DemoData.PowerPorts[3].PowerPortInfo);				
			}
		}

		[TestMethod]
		public void PowerPortDomStorageProvider_ReadFilter_PortExposure_Equal()
		{
			// 10 power ports
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulatePowerPorts();

			var portExposure = SlcAsset_Management.Enums.PortExposureEnum.Back;
			var filter = PowerPortExposers.PowerPortInfo.PortExposure.UncheckedEqual(portExposure);

			var powerPortsRetrieved = helper.AssetManagement.PowerPorts.Read(filter);
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

			var outputType = SlcAsset_Management.Enums.Outputtype.IO;
			var filter = PowerPortExposers.PowerPortInfo.OutputType.UncheckedEqual(outputType);

			var powerPortsRetrieved = helper.AssetManagement.PowerPorts.Read(filter);
			var expected = DemoData.PowerPorts.Where(filter.getLambda());

			using (new AssertionScope())
			{
				powerPortsRetrieved.Should().NotBeNull();
				powerPortsRetrieved.Should().BeEquivalentTo(expected);
				powerPortsRetrieved.Should().AllSatisfy(port => port.PowerPortInfo.OutputType.Should().Be(outputType));
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

			var powerPortsRetrieved = helper.AssetManagement.PowerPorts.Read(filter);

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

			var powerPortsRetrieved = helper.AssetManagement.PowerPorts.Read(filter);
			var expected = DemoData.PowerPorts.Where(filter.getLambda());

			using (new AssertionScope())
			{
				powerPortsRetrieved.Should().NotBeNull();
				powerPortsRetrieved.Should().BeEquivalentTo(expected);
				powerPortsRetrieved.Should().AllSatisfy(port => port.PowerPortInfo.Label.Should().Contain("Power"));
			}
		}
	}
}
