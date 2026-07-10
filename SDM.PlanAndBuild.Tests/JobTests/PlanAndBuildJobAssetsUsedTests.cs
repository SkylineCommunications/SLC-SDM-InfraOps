namespace SDM.PlanAndBuild.Tests.JobTests
{
	using System;
	using System.Collections.Generic;

	using FluentAssertions;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.SDM.AssetManagement.Models;
	using Skyline.DataMiner.SDM.PlanAndBuild.Extensions;
	using Skyline.DataMiner.SDM.PlanAndBuild.Models;

	/// <summary>
	/// Unit tests for the AssetsUsed convenience methods on <see cref="PlanAndBuildJob"/>
	/// (AddAssetsUsedItem/RemoveItemFromAssetsUsed/SetAssetsUsed/ClearAssetsUsed),
	/// mirroring InfraOpsShared's JobWrapper API.
	/// </summary>
	[TestClass]
	public class PlanAndBuildJobAssetsUsedTests
	{
		[TestMethod]
		public void AddAssetsUsedItem_NewAsset_ShouldBeAdded()
		{
			var job = new PlanAndBuildJob();
			var asset = new JobAsset { AssetId = new SdmObjectReference<Asset>(Guid.NewGuid().ToString()) };

			job.AddAssetsUsedItem(asset);

			job.AssetsUsed.Should().ContainSingle().Which.Should().Be(asset);
		}

		[TestMethod]
		public void AddAssetsUsedItem_Null_ShouldThrow()
		{
			var job = new PlanAndBuildJob();

			Action act = () => job.AddAssetsUsedItem(null);

			act.Should().Throw<ArgumentNullException>();
		}

		[TestMethod]
		public void AddAssetsUsedItem_DuplicateAssetId_ShouldThrow()
		{
			var job = new PlanAndBuildJob();
			var assetId = new SdmObjectReference<Asset>(Guid.NewGuid().ToString());
			job.AddAssetsUsedItem(new JobAsset { AssetId = assetId });

			Action act = () => job.AddAssetsUsedItem(new JobAsset { AssetId = assetId });

			act.Should().Throw<InvalidOperationException>();
		}

		[TestMethod]
		public void RemoveItemFromAssetsUsed_ExistingAsset_ShouldBeRemoved()
		{
			var job = new PlanAndBuildJob();
			var asset = new JobAsset { AssetId = new SdmObjectReference<Asset>(Guid.NewGuid().ToString()) };
			job.AddAssetsUsedItem(asset);

			job.RemoveItemFromAssetsUsed(asset);

			job.AssetsUsed.Should().BeEmpty();
		}

		[TestMethod]
		public void RemoveItemFromAssetsUsed_Null_ShouldThrow()
		{
			var job = new PlanAndBuildJob();

			Action act = () => job.RemoveItemFromAssetsUsed(null);

			act.Should().Throw<ArgumentNullException>();
		}

		[TestMethod]
		public void RemoveItemFromAssetsUsed_NotFound_ShouldThrow()
		{
			var job = new PlanAndBuildJob();
			var asset = new JobAsset { AssetId = new SdmObjectReference<Asset>(Guid.NewGuid().ToString()) };

			Action act = () => job.RemoveItemFromAssetsUsed(asset);

			act.Should().Throw<ArgumentException>();
		}

		[TestMethod]
		public void SetAssetsUsed_ShouldReplaceExistingList()
		{
			var job = new PlanAndBuildJob();
			job.AddAssetsUsedItem(new JobAsset { AssetId = new SdmObjectReference<Asset>(Guid.NewGuid().ToString()) });

			var replacement = new List<JobAsset>
			{
				new JobAsset { AssetId = new SdmObjectReference<Asset>(Guid.NewGuid().ToString()) },
				new JobAsset { AssetId = new SdmObjectReference<Asset>(Guid.NewGuid().ToString()) },
			};
			job.SetAssetsUsed(replacement);

			job.AssetsUsed.Should().BeEquivalentTo(replacement);
		}

		[TestMethod]
		public void ClearAssetsUsed_ShouldEmptyList()
		{
			var job = new PlanAndBuildJob();
			job.AddAssetsUsedItem(new JobAsset { AssetId = new SdmObjectReference<Asset>(Guid.NewGuid().ToString()) });

			job.ClearAssetsUsed();

			job.AssetsUsed.Should().BeEmpty();
		}
	}
}
