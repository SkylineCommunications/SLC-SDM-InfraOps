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
			helper.Assets.CreateOrUpdate([referenceAsset]);
			helper.PopulateAssets();

			var nameFilter = AssetExposers.AssetName.Equal(referenceAsset.AssetName);

			var assetsRetrieved = helper.Assets.Read(nameFilter);

			using (new AssertionScope())
			{
				assetsRetrieved.Should().NotBeNull();
				assetsRetrieved.Count().Should().Be(1);
				Asset asset = assetsRetrieved.First();

				asset.AssetName.Should().Be(referenceAsset.AssetName);
				asset.AssetId.Should().Be(referenceAsset.AssetId);
				asset.Lifecycle.FirstUseDate.Should().Be(referenceAsset.Lifecycle.FirstUseDate);
				asset.Holders.Should().NotBeEmpty();
				asset.Holders.Should().HaveCount(3);
			}
		}

		[TestMethod]
		public void AssetDomStorageProvider_ReadFilter_AssetDescription()
		{
			// 11 assets
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.Assets.CreateOrUpdate([referenceAsset]);
			helper.PopulateAssets();

			var descriptionFilter = AssetExposers.AssetDescription.Equal(DemoData.Assets[3].AssetDescription);

			var assetsRetrieved = helper.Assets.Read(descriptionFilter);

			using (new AssertionScope())
			{
				assetsRetrieved.Should().NotBeNull();
				assetsRetrieved.Count().Should().Be(1);
				Asset asset = assetsRetrieved.First();

				asset.AssetName.Should().Be(DemoData.Assets[3].AssetName);
				asset.AssetId.Should().Be(DemoData.Assets[3].AssetId);
				asset.FwOs.Should().Be(DemoData.Assets[3].FwOs);
				asset.Notes.Should().Be(DemoData.Assets[3].Notes);
				asset.Custody.TeamId.Should().Be(DemoData.Assets[3].Custody.TeamId);
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
			referenceAsset.AssetClass = new SdmObjectReference<AssetClass>(assetClass.Id.ToString());

			helper.PopulateAssets([referenceAsset]);
			Debug.WriteLine(helper.Assets.Count(new TRUEFilterElement<Asset>()));

			var filter = AssetExposers.AssetClass.Equal(new SdmObjectReference<AssetClass>(assetClass.Identifier));
			var assetsRetrieved = helper.Assets.Read(filter);

			using (new AssertionScope())
			{
				assetsRetrieved.Should().NotBeNull();
				assetsRetrieved.Count().Should().Be(1);
				Asset asset = assetsRetrieved.First();

				asset.AssetName.Should().Be(referenceAsset.AssetName);
				asset.AssetId.Should().Be(referenceAsset.AssetId);
				asset.AssetClass.Identifier.Should().NotBe(Guid.Empty.ToString());
				asset.AssetClass.Identifier.Should().Be(assetClass.Id.ToString());
			}
		}

		[TestMethod]
		public void AssetDomStorageProvider_ReadFilter_FwOs()
		{
			// 11 assets
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.Assets.CreateOrUpdate([referenceAsset]);
			helper.PopulateAssets();

			var firmwareFilter = AssetExposers.FwOs.Equal(DemoData.Assets[1].FwOs);

			var assetsRetrieved = helper.Assets.Read(firmwareFilter);

			using (new AssertionScope())
			{
				assetsRetrieved.Should().NotBeNull();
				assetsRetrieved.Count().Should().Be(1);
				Asset asset = assetsRetrieved.First();

				asset.AssetName.Should().Be(DemoData.Assets[1].AssetName);
				asset.AssetId.Should().Be(DemoData.Assets[1].AssetId);
				asset.FwOs.Should().Be(DemoData.Assets[1].FwOs);
				asset.NetworkDetails.MACAddress.Should().Be(DemoData.Assets[1].NetworkDetails.MACAddress);
				asset.Custody.Till.Should().Be(DemoData.Assets[1].Custody.Till);
				asset.Holders.Should().BeEmpty();
			}
		}

		[TestMethod]
		public void AssetDomStorageProvider_ReadFilter_SerialNumber_Equal()
		{
			// 11 assets
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.Assets.CreateOrUpdate([referenceAsset]);
			helper.PopulateAssets();

			var firmwareFilter = AssetExposers.SerialNumber.Equal(DemoData.Assets[9].SerialNumber);

			var assetsRetrieved = helper.Assets.Read(firmwareFilter);

			using (new AssertionScope())
			{
				assetsRetrieved.Should().NotBeNull();
				assetsRetrieved.Count().Should().Be(1);
				Asset asset = assetsRetrieved.First();

				asset.AssetName.Should().Be(DemoData.Assets[9].AssetName);
				asset.AssetId.Should().Be(DemoData.Assets[9].AssetId);
				asset.AssetDescription.Should().Be(DemoData.Assets[9].AssetDescription);
				asset.Notes.Should().Be(DemoData.Assets[9].Notes);
				asset.Location.Side.Should().Be(DemoData.Assets[9].Location.Side);
				asset.Custody.From.Should().Be(DemoData.Assets[9].Custody.From);
				asset.Lifecycle.ModificationDate.Should().Be(DemoData.Assets[9].Lifecycle.ModificationDate);
				asset.Lifecycle.EndOfWarrantyDate.Should().Be(DemoData.Assets[9].Lifecycle.EndOfWarrantyDate);
			}
		}

		[TestMethod]
		public void AssetDomStorageProvider_ReadFilter_HardwareVersion_Equal()
		{
			// 11 assets
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.Assets.CreateOrUpdate([referenceAsset]);
			helper.PopulateAssets();

			var firmwareFilter = AssetExposers.HardwareVersion.Equal(DemoData.Assets[4].HardwareVersion);

			var assetsRetrieved = helper.Assets.Read(firmwareFilter);

			using (new AssertionScope())
			{
				assetsRetrieved.Should().NotBeNull();
				assetsRetrieved.Count().Should().Be(1);
				Asset asset = assetsRetrieved.First();

				asset.AssetName.Should().Be(DemoData.Assets[4].AssetName);
				asset.AssetId.Should().Be(DemoData.Assets[4].AssetId);
				asset.SerialNumber.Should().Be(DemoData.Assets[4].SerialNumber);
				asset.Location.RackPosition.Should().Be(DemoData.Assets[4].Location.RackPosition);
				asset.Lifecycle.InstallationDate.Should().Be(DemoData.Assets[4].Lifecycle.InstallationDate);
			}
		}

		[TestMethod]
		public void AssetDomStorageProvider_NestedReadFilter_NetworkDetails_MACAddress_Equal()
		{
			// 11 assets
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.Assets.CreateOrUpdate([referenceAsset]);
			helper.PopulateAssets();

			var macAddress = DemoData.Assets[5].NetworkDetails.MACAddress;
			var macFilter = AssetExposers.NetworkDetails.MACAddress.Equal(macAddress);

			var assetsRetrieved = helper.Assets.Read(macFilter);

			using (new AssertionScope())
			{
				assetsRetrieved.Should().NotBeNull();
				assetsRetrieved.Count().Should().Be(1);
				Asset asset = assetsRetrieved.First();

				asset.AssetName.Should().Be(DemoData.Assets[5].AssetName);
				asset.AssetId.Should().Be(DemoData.Assets[5].AssetId);
				asset.AssetDescription.Should().Be(DemoData.Assets[5].AssetDescription);
				asset.FwOs.Should().Be(DemoData.Assets[5].FwOs);
				asset.NetworkDetails.MACAddress.Should().Be(macAddress);
				asset.Custody.TeamId.Should().Be(DemoData.Assets[5].Custody.TeamId);
				asset.Lifecycle.PurchaseDate.Should().Be(DemoData.Assets[5].Lifecycle.PurchaseDate);
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

			var assetsRetrieved = helper.Assets.Read(filter);
			var expected = DemoData.Assets.Where(asset => asset.Lifecycle.FirstUseDate <= firstUseDate).ToList();

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

			var assetsRetrieved = helper.Assets.Read(filter);
			var expected = DemoData.Assets.Where(asset => asset.Lifecycle.EndOfWarrantyDate < endOfWarrantyDate).ToList();

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

			var assetsRetrieved = helper.Assets.Read(filter);
			var expected = DemoData.Assets
				.Where(asset =>
					asset.Lifecycle.InstallationDate > installationDateStart
					&& asset.Lifecycle.InstallationDate < installationDateEnd)
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

			var assetsRetrieved = helper.Assets.Read(filter);
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

			var assetsRetrieved = helper.Assets.Read(filter);
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