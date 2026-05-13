namespace SDM.AssetManagement.Tests
{
	using System.Diagnostics;
	using System.Linq;
	using FluentAssertions;
	using FluentAssertions.Execution;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using SDM.AssetManagement.Tests;
	using SDM.AssetManagement.Tests.Setup;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.SDM.AssetManagement.Models;

	public partial class AssetDomStorageProviderTests
	{
		[TestMethod]
		public void AssetDomStorageProvider_ReadFilter_AssetName()
		{
			// 11 assets
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.AssetManagement.Assets.CreateOrUpdate([referenceAsset]);
			helper.PopulateAssets();

			var nameFilter = AssetExposers.AssetName.Equal(referenceAsset.Name);

			var assetsRetrieved = helper.AssetManagement.Assets.Read(nameFilter);

			using (new AssertionScope())
			{
				assetsRetrieved.Should().NotBeNull();
				assetsRetrieved.Count().Should().Be(1);
				Asset asset = assetsRetrieved.First();

				asset.Name.Should().Be(referenceAsset.Name);
				asset.AssetID.Should().Be(referenceAsset.AssetID);
				asset.FirstUseDate.Should().Be(referenceAsset.FirstUseDate);
				asset.Holders.Should().NotBeEmpty();
				asset.Holders.Should().HaveCount(3);
			}
		}

		[TestMethod]
		public void AssetDomStorageProvider_ReadFilter_AssetDescription()
		{
			// 11 assets
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.AssetManagement.Assets.CreateOrUpdate([referenceAsset]);
			helper.PopulateAssets();

			var descriptionFilter = AssetExposers.AssetDescription.Equal(DemoData.Assets[3].Description);

			var assetsRetrieved = helper.AssetManagement.Assets.Read(descriptionFilter);

			using (new AssertionScope())
			{
				assetsRetrieved.Should().NotBeNull();
				assetsRetrieved.Count().Should().Be(1);
				Asset asset = assetsRetrieved.First();

				asset.Name.Should().Be(DemoData.Assets[3].Name);
				asset.AssetID.Should().Be(DemoData.Assets[3].AssetID);
				asset.FW_OS.Should().Be(DemoData.Assets[3].FW_OS);
				asset.Custody.Team.Should().Be(DemoData.Assets[3].Custody.Team);
			}
		}

		[TestMethod]
		public void AssetDomStorageProvider_ReadFilter_AssetClass()
		{
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateAssets().PopulateAssetClasses();

			var random = new Random();
			var randomIndex = random.Next(0, 9);

			var assetClass = DemoData.AssetClasses[randomIndex];
			referenceAsset.AssetClassId = new SdmObjectReference<AssetClass>(assetClass.Identifier);

			helper.PopulateAssets([referenceAsset]);
			Debug.WriteLine(helper.AssetManagement.Assets.Count(new TRUEFilterElement<Asset>()));

			var filter = AssetExposers.AssetClass.Equal(new SdmObjectReference<AssetClass>(assetClass.Identifier));
			var assetsRetrieved = helper.AssetManagement.Assets.Read(filter);

			using (new AssertionScope())
			{
				assetsRetrieved.Should().NotBeNull();
				assetsRetrieved.Count().Should().Be(1);
				Asset asset = assetsRetrieved.First();

				asset.Name.Should().Be(referenceAsset.Name);
				asset.AssetID.Should().Be(referenceAsset.AssetID);
				asset.AssetClassId.Identifier.Should().NotBe(Guid.Empty.ToString());
				asset.AssetClassId.Identifier.Should().Be(assetClass.Identifier);
			}
		}

		[TestMethod]
		public void AssetDomStorageProvider_ReadFilter_FwOs()
		{
			// 11 assets
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.AssetManagement.Assets.CreateOrUpdate([referenceAsset]);
			helper.PopulateAssets();

			var firmwareFilter = AssetExposers.FwOs.Equal(DemoData.Assets[1].FW_OS);

			var assetsRetrieved = helper.AssetManagement.Assets.Read(firmwareFilter);

			using (new AssertionScope())
			{
				assetsRetrieved.Should().NotBeNull();
				assetsRetrieved.Count().Should().Be(1);
				Asset asset = assetsRetrieved.First();

				asset.Name.Should().Be(DemoData.Assets[1].Name);
				asset.AssetID.Should().Be(DemoData.Assets[1].AssetID);
				asset.FW_OS.Should().Be(DemoData.Assets[1].FW_OS);
				asset.MacAddress.Should().Be(DemoData.Assets[1].MacAddress);
				asset.Custody.Till.Should().Be(DemoData.Assets[1].Custody.Till);
				asset.Holders.Should().BeEmpty();
			}
		}

		[TestMethod]
		public void AssetDomStorageProvider_ReadFilter_SerialNumber_Equal()
		{
			// 11 assets
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.AssetManagement.Assets.CreateOrUpdate([referenceAsset]);
			helper.PopulateAssets();

			var firmwareFilter = AssetExposers.SerialNumber.Equal(DemoData.Assets[9].SerialNumber);

			var assetsRetrieved = helper.AssetManagement.Assets.Read(firmwareFilter);

			using (new AssertionScope())
			{
				assetsRetrieved.Should().NotBeNull();
				assetsRetrieved.Count().Should().Be(1);
				Asset asset = assetsRetrieved.First();

				asset.Name.Should().Be(DemoData.Assets[9].Name);
				asset.AssetID.Should().Be(DemoData.Assets[9].AssetID);
				asset.Description.Should().Be(DemoData.Assets[9].Description);
				asset.Location.Side.Should().Be(DemoData.Assets[9].Location.Side);
				asset.Custody.From.Should().Be(DemoData.Assets[9].Custody.From);
				asset.ModificationDate.Should().Be(DemoData.Assets[9].ModificationDate);
				asset.EndOfWarrantyDate.Should().Be(DemoData.Assets[9].EndOfWarrantyDate);
			}
		}

		[TestMethod]
		public void AssetDomStorageProvider_ReadFilter_HardwareVersion_Equal()
		{
			// 11 assets
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.AssetManagement.Assets.CreateOrUpdate([referenceAsset]);
			helper.PopulateAssets();

			var firmwareFilter = AssetExposers.HardwareVersion.Equal(DemoData.Assets[4].HardwareVersion);

			var assetsRetrieved = helper.AssetManagement.Assets.Read(firmwareFilter);

			using (new AssertionScope())
			{
				assetsRetrieved.Should().NotBeNull();
				assetsRetrieved.Count().Should().Be(1);
				Asset asset = assetsRetrieved.First();

				asset.Name.Should().Be(DemoData.Assets[4].Name);
				asset.AssetID.Should().Be(DemoData.Assets[4].AssetID);
				asset.SerialNumber.Should().Be(DemoData.Assets[4].SerialNumber);
				asset.Location.RackPosition.Should().Be(DemoData.Assets[4].Location.RackPosition);
				asset.InstallationDate.Should().Be(DemoData.Assets[4].InstallationDate);
			}
		}

		[TestMethod]
		public void AssetDomStorageProvider_NestedReadFilter_NetworkDetails_MACAddress_Equal()
		{
			// 11 assets
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.AssetManagement.Assets.CreateOrUpdate([referenceAsset]);
			helper.PopulateAssets();

			var macAddress = DemoData.Assets[5].MacAddress;
			var macFilter = AssetExposers.NetworkDetails.MACAddress.Equal(macAddress);

			var assetsRetrieved = helper.AssetManagement.Assets.Read(macFilter);

			using (new AssertionScope())
			{
				assetsRetrieved.Should().NotBeNull();
				assetsRetrieved.Count().Should().Be(1);
				Asset asset = assetsRetrieved.First();

				asset.Name.Should().Be(DemoData.Assets[5].Name);
				asset.AssetID.Should().Be(DemoData.Assets[5].AssetID);
				asset.Description.Should().Be(DemoData.Assets[5].Description);
				asset.FW_OS.Should().Be(DemoData.Assets[5].FW_OS);
				asset.MacAddress.Should().Be(macAddress);
				asset.Custody.Team.Should().Be(DemoData.Assets[5].Custody.Team);
				asset.PurchaseDate.Should().Be(DemoData.Assets[5].PurchaseDate);
			}
		}

		[TestMethod]
		public void AssetDomStorageProvider_NestedReadFilter_Lifecycle_FirstUseDate_LessThanOrEqual()
		{
			// 10 assets
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateAssets();

			var firstUseDate = DateTime.UtcNow.AddYears(-5);
			var filter = AssetExposers.Lifecycle.FirstUseDate.LessThanOrEqual(firstUseDate);

			var assetsRetrieved = helper.AssetManagement.Assets.Read(filter);
			var expected = DemoData.Assets.Where(asset => asset.FirstUseDate <= firstUseDate).ToList();

			using (new AssertionScope())
			{
				assetsRetrieved.Should().NotBeNull();
				assetsRetrieved.Count().Should().Be(expected.Count);
			}
		}

		[TestMethod]
		public void AssetDomStorageProvider_NestedReadFilter_Lifecycle_EndOfWarrantyDate_LessThan()
		{
			// 10 assets
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateAssets();

			var endOfWarrantyDate = DateTime.UtcNow.AddYears(5);
			var filter = AssetExposers.Lifecycle.EndOfWarrantyDate.LessThan(endOfWarrantyDate);

			var assetsRetrieved = helper.AssetManagement.Assets.Read(filter);
			var expected = DemoData.Assets.Where(asset => asset.EndOfWarrantyDate < endOfWarrantyDate).ToList();

			using (new AssertionScope())
			{
				assetsRetrieved.Should().NotBeNull();
				assetsRetrieved.Count().Should().Be(expected.Count);
			}
		}

		[TestMethod]
		public void AssetDomStorageProvider_NestedReadFilter_Lifecycle_InstallationDate_InBetween()
		{
			// 10 assets
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateAssets();

			var installationDateStart = DateTime.UtcNow.AddYears(-6);
			var installationDateEnd = DateTime.UtcNow.AddYears(-3);

			var filter = AssetExposers.Lifecycle.InstallationDate.LessThan(installationDateEnd)
				.AND(AssetExposers.Lifecycle.InstallationDate.GreaterThan(installationDateStart));

			var assetsRetrieved = helper.AssetManagement.Assets.Read(filter);
			var expected = DemoData.Assets
				.Where(asset =>
					asset.InstallationDate > installationDateStart
					&& asset.InstallationDate < installationDateEnd)
				.ToList();

			using (new AssertionScope())
			{
				assetsRetrieved.Should().NotBeNull();
				assetsRetrieved.Count().Should().Be(expected.Count);
			}
		}

		[TestMethod]
		public void AssetDomStorageProvider_NestedReadFilter_Location_RackPosition_NotEqual()
		{
			// 10 assets
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateAssets();

			var filter = AssetExposers.Location.RackPosition.NotEqual(7);

			var assetsRetrieved = helper.AssetManagement.Assets.Read(filter);
			var expected = DemoData.Assets.Where(asset => asset.Location.RackPosition != 7);

			using (new AssertionScope())
			{
				assetsRetrieved.Should().NotBeNull();
				assetsRetrieved.Count().Should().Be(DemoData.Assets.Count - 1);

				assetsRetrieved.Should().BeEquivalentTo(expected);
				assetsRetrieved.Should().AllSatisfy(asset => asset.Location.RackPosition.Should().NotBe(7));
			}
		}

		[TestMethod]
		public void AssetDomStorageProvider_NestedReadFilter_ElementLink_ElementID_Equal()
		{
			// 10 assets
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateAssets();

			string elementId = "101/4";
			var filter = AssetExposers.ElementLinks.ElementID.Equal(elementId);

			var assetsRetrieved = helper.AssetManagement.Assets.Read(filter);
			var expected = DemoData.Assets.Where(filter.getLambda());

			using (new AssertionScope())
			{
				assetsRetrieved.Should().NotBeNull();
				assetsRetrieved.Should().HaveCount(1);

				assetsRetrieved.Should().BeEquivalentTo(expected);
				assetsRetrieved.Should().AllSatisfy(asset => asset.ElementLinks[0].ElementID.Should().Be(elementId));
			}
		}
	}
}