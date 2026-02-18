namespace SDM.AssetManagement.Tests
{
	using System.Linq;
	using FluentAssertions;
	using FluentAssertions.Execution;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using SDM.AssetManagement.Tests.Setup;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.SDM.AssetManagement.Models;

	public partial class AssetClassDomStorageProvider
	{
		[TestMethod]
		public void AssetClassDomStorageProvider_ReadFilter_DeviceName_Equals()
		{
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateAssetClasses();

			string deviceName = "KVM Switch";
			var nameFilter = AssetClassExposers.DeviceName.Equal(deviceName);
			var expected = DemoData.AssetClasses.Single(asset => asset.DeviceName.Equals(deviceName));

			var classesRetrieved = helper.AssetClasses.Read(nameFilter);

			using (new AssertionScope())
			{
				classesRetrieved.Should().NotBeNull();
				classesRetrieved.Count().Should().Be(1);
				var assetClass = classesRetrieved.First();

				assetClass.DeviceName.Should().Be(expected.DeviceName);
				assetClass.DeviceTypeId.Should().Be(expected.DeviceTypeId);
				assetClass.Manufacturer.Should().Be(expected.Manufacturer);
				assetClass.Depth.Should().Be(expected.Depth);
				assetClass.Height.Should().Be(expected.Height);
				assetClass.Weight.Should().Be(expected.Weight);
				assetClass.HeightU.Should().Be(expected.HeightU);
				assetClass.Width.Should().Be(expected.Width);
				assetClass.Lifecycle.Should().NotBeNull();

				assetClass.DataPorts.Should().NotBeNull();
				assetClass.DataPorts.Should().HaveCount(expected.DataPorts.Count);

				assetClass.PowerPorts.Should().NotBeNull();
				assetClass.PowerPorts.Should().HaveCount(expected.PowerPorts.Count);

				assetClass.Holders.Should().NotBeNull();
				assetClass.Holders.Should().HaveCount(expected.Holders.Count);
			}
		}

		[TestMethod]
		public void AssetClassDomStorageProvider_ReadFilter_DeviceDescription_Contains()
		{
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateAssetClasses();

			var descriptionFilter = AssetClassExposers.DeviceDescription.Contains("Panel", StringComparison.OrdinalIgnoreCase); // DemoData.AssetClasses[7]
			var expected = DemoData.AssetClasses[7];

			var classesRetrieved = helper.AssetClasses.Read(descriptionFilter);

			using (new AssertionScope())
			{
				classesRetrieved.Should().NotBeNull();
				classesRetrieved.Count().Should().Be(1);
				var assetClass = classesRetrieved.First();

				// Assert all fields and properties
				assetClass.DeviceName.Should().Be(expected.DeviceName);
				assetClass.DeviceDescription.Should().Be(expected.DeviceDescription);
				assetClass.DeviceTypeId.Should().Be(expected.DeviceTypeId);
				assetClass.Manufacturer.Should().Be(expected.Manufacturer);
				assetClass.Depth.Should().Be(expected.Depth);
				assetClass.Height.Should().Be(expected.Height);
				assetClass.HeightU.Should().Be(expected.HeightU);
				assetClass.Width.Should().Be(expected.Width);
				assetClass.Weight.Should().Be(expected.Weight);
				assetClass.FrontImage.Should().Be(expected.FrontImage);
				assetClass.BackImage.Should().Be(expected.BackImage);
				assetClass.MaximumPowerConsumption.Should().Be(expected.MaximumPowerConsumption);
				assetClass.TypicalPowerConsumption.Should().Be(expected.TypicalPowerConsumption);
				assetClass.PowerSupply.Should().Be(expected.PowerSupply);

				assetClass.Lifecycle.Should().NotBeNull();
				assetClass.Lifecycle.Should().Be(expected.Lifecycle);

				assetClass.DataPorts.Should().NotBeNull();
				assetClass.DataPorts.Should().BeEquivalentTo(expected.DataPorts);

				assetClass.PowerPorts.Should().NotBeNull();
				assetClass.PowerPorts.Should().BeEquivalentTo(expected.PowerPorts);

				assetClass.Holders.Should().NotBeNull();
				assetClass.Holders.Should().BeEquivalentTo(expected.Holders);
			}
		}

		public void AssetClassDomStorageProvider_ReadFilter_ManufacturerId()
		{
			// Create test once SdmObjectReference for the ManufacturerId is added.
		}

		public void AssetClassDomStorageProvider_ReadFilter_DeviceTypeId()
		{
			// Create test once SdmObjectReference for the DeviceTypeId is added.
		}

		[TestMethod]
		public void AssetClassDomStorageProvider_ReadFilter_Width_Equal()
		{
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateAssetClasses();

			var width = DemoData.AssetClasses[0].Width;
			var widthFilter = AssetClassExposers.Width.Equal(width);

			var classesRetrieved = helper.AssetClasses.Read(widthFilter);
			var expected = DemoData.AssetClasses[0];

			using (new AssertionScope())
			{
				classesRetrieved.Should().NotBeNull();
				classesRetrieved.Count().Should().Be(1);
				var assetClass = classesRetrieved.First();
				assetClass.Width.Should().Be(width);
				assetClass.DeviceName.Should().Be(expected.DeviceName);
			}
		}

		// Simulating a query to find costly devices.
		[TestMethod]
		public void AssetClassDomStorageProvider_ReadFilter_GreaterThanOrEqual_MaximumPowerConsumption_GreaterThanOrEqual()
		{
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateAssetClasses();

			double powerConsumptionThreshold = 200;
			var maxPowerConsumptionFilter = AssetClassExposers.MaximumPowerConsumption.GreaterThanOrEqual(powerConsumptionThreshold);

			var classesRetrieved = helper.AssetClasses.Read(maxPowerConsumptionFilter);
			var actualArray = DemoData.AssetClasses.Where(assetClass => assetClass.MaximumPowerConsumption >= powerConsumptionThreshold).ToArray();

			using (new AssertionScope())
			{
				classesRetrieved.Should().NotBeNull();
				classesRetrieved.Count().Should().Be(actualArray.Length);
				classesRetrieved.Should().BeEquivalentTo(actualArray);
			}
		}

		// Simulating a query to find devices which fit in an imaginary small rack.
		[TestMethod]
		public void AssetClassDomStorageProvider_ReadFilter_WidthHeightDepth_LessThanOrEqual()
		{
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateAssetClasses();

			double heightThreshold = 2 * 4.45; // 2U height
			double widthThreshold = 40.0; // 40 cm width
			double depthThreshold = 40.0; // 40 cm depth

			var volumeFilter = AssetClassExposers.Height.LessThanOrEqual(heightThreshold)
				.AND(AssetClassExposers.Width.LessThanOrEqual(widthThreshold))
				.AND(AssetClassExposers.Depth.LessThanOrEqual(depthThreshold));
			var classesRetrieved = helper.AssetClasses.Read(volumeFilter);

			var expected = DemoData.AssetClasses.Where(ac => ac.Height <= heightThreshold && ac.Width <= widthThreshold && ac.Depth <= depthThreshold).ToArray();
			using (new AssertionScope())
			{
				classesRetrieved.Should().NotBeNull();
				classesRetrieved.Count().Should().Be(expected.Length);
				classesRetrieved.Should().BeEquivalentTo(expected);
			}
		}

		[TestMethod]
		public void AssetClassDomStorageProvider_ReadFilter_FrontImage_NotNullOrEmpty()
		{
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateAssetClasses();

			var frontImageName = "fw-front.png";
			var frontImageFilter = AssetClassExposers.FrontImage.Equal(frontImageName);
			var classesRetrieved = helper.AssetClasses.Read(frontImageFilter);

			using (new AssertionScope())
			{
				classesRetrieved.Should().NotBeNull();
				classesRetrieved.Count().Should().Be(1);
				var assetClass = classesRetrieved.First();

				assetClass.DeviceName.Should().Be(assetClass.DeviceName);
				assetClass.DeviceDescription.Should().Be(assetClass.DeviceDescription);
				assetClass.DeviceTypeId.Should().Be(assetClass.DeviceTypeId);
				assetClass.Manufacturer.Should().Be(assetClass.Manufacturer);
				assetClass.Depth.Should().Be(assetClass.Depth);
				assetClass.Height.Should().Be(assetClass.Height);
				assetClass.HeightU.Should().Be(assetClass.HeightU);
				assetClass.Width.Should().Be(assetClass.Width);
				assetClass.Weight.Should().Be(assetClass.Weight);
				assetClass.FrontImage.Should().Be(assetClass.FrontImage);
				assetClass.BackImage.Should().Be(assetClass.BackImage);
				assetClass.MaximumPowerConsumption.Should().Be(assetClass.MaximumPowerConsumption);
				assetClass.TypicalPowerConsumption.Should().Be(assetClass.TypicalPowerConsumption);
				assetClass.PowerSupply.Should().Be(assetClass.PowerSupply);

				assetClass.Lifecycle.Should().NotBeNull();
				assetClass.Lifecycle.Should().BeEquivalentTo(assetClass.Lifecycle);

				assetClass.DataPorts.Should().NotBeNull();
				assetClass.DataPorts.Should().BeEquivalentTo(assetClass.DataPorts);

				assetClass.PowerPorts.Should().NotBeNull();
				assetClass.PowerPorts.Should().BeEquivalentTo(assetClass.PowerPorts);

				assetClass.Holders.Should().NotBeNull();
				assetClass.Holders.Should().BeEquivalentTo(assetClass.Holders);
			}
		}

		[TestMethod]
		public void AssetClassDomStorageProvider_ReadFilter_BackImage_NotContains()
		{
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateAssetClasses();

			var backImageFilter = AssetClassExposers.BackImage.NotContains(".png");
			var classesRetrieved = helper.AssetClasses.Read(backImageFilter);

			var expected = DemoData.AssetClasses.Where(backImageFilter.getLambda()).ToArray();

			using (new AssertionScope())
			{
				classesRetrieved.Should().NotBeNull();
				classesRetrieved.Count().Should().Be(expected.Length);
				classesRetrieved.Should().BeEquivalentTo(expected);
			}
		}

		[TestMethod]
		public void AssetClassDomStorageProvider_ReadFilter_TypicalPowerConsumption_LessThanOrEqual()
		{
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateAssetClasses();

			double typicalPowerThreshold = 100.0;
			var typicalPowerFilter = AssetClassExposers.TypicalPowerConsumption.LessThanOrEqual(typicalPowerThreshold);

			var classesRetrieved = helper.AssetClasses.Read(typicalPowerFilter);
			var expected = DemoData.AssetClasses.Where(ac => ac.TypicalPowerConsumption <= typicalPowerThreshold).ToArray();

			using (new AssertionScope())
			{
				classesRetrieved.Should().NotBeNull();
				classesRetrieved.Count().Should().Be(expected.Length);
				classesRetrieved.Should().BeEquivalentTo(expected);
			}
		}

		[TestMethod]
		public void AssetClassDomStorageProvider_NestedReadFilter_PortNumber_Equal()
		{
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateAssetClasses();

			var filter = AssetClassExposers.DataPorts.PortNumber.Equal(4);
			var classesRetrieved = helper.AssetClasses.Read(filter);

			var expected = DemoData.AssetClasses.Where(ac => ac.DataPorts.Any(port => port.PortNumber == 4)).ToArray();

			using (new AssertionScope())
			{
				classesRetrieved.Should().NotBeNull();
				classesRetrieved.Count().Should().Be(expected.Length);
				classesRetrieved.Should().BeEquivalentTo(expected);
			}
		}

		[TestMethod]
		public void AssetClassDomStorageProvider_NestedReadFilter_SlotNumber_Equal()
		{
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateAssetClasses();

			var filter = AssetClassExposers.Holders.SlotNumber.Equal(6); // Should be DemoData.AssetClasses[5]
			var classesRetrieved = helper.AssetClasses.Read(filter);

			var expected = DemoData.AssetClasses.First(ac => ac.Holders.Any(holder => holder.SlotNumber == 6));

			using (new AssertionScope())
			{
				classesRetrieved.Should().NotBeNull();
				classesRetrieved.Count().Should().Be(1);
				var assetClass = classesRetrieved.First();

				assetClass.DeviceName.Should().Be(expected.DeviceName);
				assetClass.DeviceDescription.Should().Be(expected.DeviceDescription);
				assetClass.DeviceTypeId.Should().Be(expected.DeviceTypeId);
				assetClass.Manufacturer.Should().Be(expected.Manufacturer);
				assetClass.Depth.Should().Be(expected.Depth);
				assetClass.Height.Should().Be(expected.Height);
				assetClass.HeightU.Should().Be(expected.HeightU);
				assetClass.Width.Should().Be(expected.Width);
				assetClass.Weight.Should().Be(expected.Weight);
				assetClass.FrontImage.Should().Be(expected.FrontImage);
				assetClass.BackImage.Should().Be(expected.BackImage);
				assetClass.MaximumPowerConsumption.Should().Be(expected.MaximumPowerConsumption);
				assetClass.TypicalPowerConsumption.Should().Be(expected.TypicalPowerConsumption);
				assetClass.PowerSupply.Should().Be(expected.PowerSupply);

				assetClass.Lifecycle.Should().NotBeNull();
				assetClass.Lifecycle.Should().Be(expected.Lifecycle);

				assetClass.DataPorts.Should().NotBeNull();
				assetClass.DataPorts.Should().BeEquivalentTo(expected.DataPorts);

				assetClass.PowerPorts.Should().NotBeNull();
				assetClass.PowerPorts.Should().BeEquivalentTo(expected.PowerPorts);

				assetClass.Holders.Should().NotBeNull();
				assetClass.Holders.Should().BeEquivalentTo(expected.Holders);
			}
		}
	}
}