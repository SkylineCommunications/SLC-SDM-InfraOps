namespace SDM.AssetManagement.Tests
{
	using System;
	using System.Diagnostics;
	using System.Linq;
	using FluentAssertions;
	using FluentAssertions.Execution;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Newtonsoft.Json;
	using SDM.AssetManagement.Tests.Setup;
	//using Skyline.DataMiner.Analytics.GenericInterface.JoinFilter;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.SDM.AssetManagement;
	using Skyline.DataMiner.SDM.AssetManagement.Helpers;
	using Skyline.DataMiner.SDM.AssetManagement.Models;

	[TestClass]
	public partial class PowerPortDomStorageProviderTests
	{
		private PowerPort referencePowerPort;

		[TestInitialize]
		public void Init()
		{
			referencePowerPort = new PowerPort
			{
				Identifier = Guid.NewGuid().ToString(),
				PowerPortInfo = new PowerPortInfo
				{
					Identifier = Guid.NewGuid().ToString(),
					Name = "Test PowerPort",
					PortNumber = 1,
					OutputType = SlcAssetManagement.Enums.Outputtype.IO,
					PortExposure = SlcAssetManagement.Enums.PortExposure.Front,
					Label = "Power Port 1",
				},
				Asset = new SdmObjectReference<Asset>(Guid.NewGuid().ToString()),
				PrimaryPortRelation = new PrimaryPortRelation
				{
					IsPrimaryIpv4 = true,
					IsPrimaryIpv6 = false,
				},
			};
		}

		[TestMethod]
		public void PowerPortDomStorageProvider_EmptyDOM_Create()
		{
			var helper = RepositoryInitialize.InitializeEmptyRepositories();

			helper.PowerPorts.Create(referencePowerPort);

			AssertCreated(helper);
		}

		[TestMethod]
		public void PowerPortDomStorageProvider_EmptyDOM_CreateOrUpdate_Create()
		{
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PowerPorts.CreateOrUpdate([referencePowerPort]);

			AssertCreated(helper);
		}

		[TestMethod]
		public void PowerPortDomStorageProvider_EmptyDOM_CreateOrUpdate_Update()
		{
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PowerPorts.Create(referencePowerPort);

			var updatedPowerPort = new PowerPort
			{
				Identifier = referencePowerPort.Identifier,
				PowerPortInfo = new PowerPortInfo
				{
					Identifier = referencePowerPort.PowerPortInfo.Identifier,
					Name = "Updated PowerPort Name",
					PortNumber = 2,
					OutputType = SlcAssetManagement.Enums.Outputtype.Out,
					PortExposure = SlcAssetManagement.Enums.PortExposure.Back,
					Label = "Power Port 2",
				},
				Asset = referencePowerPort.Asset,
				PrimaryPortRelation = new PrimaryPortRelation
				{
					IsPrimaryIpv4 = true,
					IsPrimaryIpv6 = false,
				},
			};

			helper.PowerPorts.CreateOrUpdate([updatedPowerPort]);

			AssertPowerPortUpdateDifferences(referencePowerPort, updatedPowerPort);
		}

		[TestMethod]
		public void PowerPortDomStorageProvider_ReadPaged()
		{
			const int pageCount = 3;
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulatePowerPorts();

			FilterElement<PowerPort> allFilter = new TRUEFilterElement<PowerPort>();
			var pagedResult = helper.PowerPorts.ReadPaged(allFilter, pageCount);
			var powerPortCount = helper.PowerPorts.Count(allFilter);

			using (new AssertionScope())
			{
				pagedResult.Should().NotBeNull();
				pagedResult.Should().HaveCountGreaterThanOrEqualTo((int)(powerPortCount / pageCount));
				pagedResult.Should().AllSatisfy(page => page.Should().HaveCountLessThanOrEqualTo(pageCount));
			}
		}

		[TestMethod]
		public void PowerPortDomStorageProvider_DeleteBulk()
		{
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulatePowerPorts();

			var filter = new ORFilterElement<PowerPort>(
				PowerPortExposers.PowerPortInfo.Name.Equal("Power Port 3"),
				PowerPortExposers.PowerPortInfo.Label.Equal("Power Port Label 7"),
				PowerPortExposers.Asset.UncheckedEqual(DemoData.Assets[9]));

			var powerPortsToDelete = helper.PowerPorts.Read(filter);

			helper.PowerPorts.Delete(powerPortsToDelete);

			using (new AssertionScope())
			{
				helper.PowerPorts.Count(new TRUEFilterElement<PowerPort>()).Should().Be(DemoData.PowerPorts.Count - powerPortsToDelete.Count());
				helper.PowerPorts.Count(PowerPortExposers.PowerPortInfo.Name.Equal("Power Port 3")).Should().Be(0);
				helper.PowerPorts.Count(PowerPortExposers.PowerPortInfo.Label.Equal("Power Port Label 7")).Should().Be(0);
			}
		}

		[TestMethod]
		public void PowerPortDomStorageProvider_EmptyDOM_DeleteSingle()
		{
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulatePowerPorts();

			var powerPortToDelete = helper.PowerPorts.Read(PowerPortExposers.PowerPortInfo.Name.Equal("Power Port 3")).First();

			helper.PowerPorts.Delete(powerPortToDelete);

			helper.PowerPorts.Count(new TRUEFilterElement<PowerPort>()).Should().Be(DemoData.PowerPorts.Count - 1);
			helper.PowerPorts.Count(PowerPortExposers.Identifier.Equal(powerPortToDelete.Identifier)).Should().Be(0);
		}

		private static void AssertPowerPortUpdateDifferences(PowerPort original, PowerPort updated)
		{
			using (new AssertionScope())
			{
				updated.Identifier.Should().Be(original.Identifier);

				// Name
				updated.PowerPortInfo.Name.Should().NotBe(original.PowerPortInfo.Name);
				updated.PowerPortInfo.Name.Should().Be("Updated PowerPort Name");

				// PortNumber
				updated.PowerPortInfo.PortNumber.Should().NotBe(original.PowerPortInfo.PortNumber);
				updated.PowerPortInfo.PortNumber.Should().Be(2);

				// OutputType
				updated.PowerPortInfo.OutputType.Should().NotBe(original.PowerPortInfo.OutputType);
				updated.PowerPortInfo.OutputType.Should().Be(SlcAssetManagement.Enums.Outputtype.Out);

				// PortExposure
				updated.PowerPortInfo.PortExposure.Should().NotBe(original.PowerPortInfo.PortExposure);
				updated.PowerPortInfo.PortExposure.Should().Be(SlcAssetManagement.Enums.PortExposure.Back);

				// PortType
				updated.PowerPortInfo.PortType.Should().Be(original.PowerPortInfo.PortType);

				// Label
				updated.PowerPortInfo.Label.Should().NotBe(original.PowerPortInfo.Label);
				updated.PowerPortInfo.Label.Should().Be("Power Port 2");

				// Asset
				updated.Asset.Should().Be(original.Asset);
			}
		}

		private void AssertCreated(IAssetManagementApiHelper helper)
		{
			using (new AssertionScope())
			{
				helper.PowerPorts.Count(new TRUEFilterElement<PowerPort>()).Should().Be(1);

				var createdPowerPort = helper.PowerPorts.Read(new TRUEFilterElement<PowerPort>()).First();
				createdPowerPort.Should().NotBeNull();

				createdPowerPort.PowerPortInfo.Equals(referencePowerPort.PowerPortInfo).Should().BeTrue();

				createdPowerPort.Asset.Should().NotBeNull();
				createdPowerPort.Asset.Should().BeAssignableTo<SdmObjectReference<Asset>>();

				createdPowerPort.PrimaryPortRelation.Should().NotBeNull();
				createdPowerPort.PrimaryPortRelation.Equals(referencePowerPort.PrimaryPortRelation).Should().BeTrue();
			}
		}
	}
}