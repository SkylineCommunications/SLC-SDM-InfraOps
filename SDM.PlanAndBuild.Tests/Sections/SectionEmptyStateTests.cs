namespace SDM.PlanAndBuild.Tests
{
	using System;

	using FluentAssertions;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using SharedMappers.DomIds;

	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.SDM.AssetManagement.Models;
	using Skyline.DataMiner.SDM.PlanAndBuild.Models;

	[TestClass]
	public class SectionEmptyStateTests
	{
		[TestMethod]
		public void JobOwnership_DefaultState_IsEmpty()
		{
			new JobOwnership().IsEmpty.Should().BeTrue();
		}

		[TestMethod]
		public void JobOwnership_AnyFieldSet_IsNotEmpty()
		{
			new JobOwnership().Also(ownership => ownership.AssignedTo = Guid.NewGuid()).IsEmpty.Should().BeFalse();
			new JobOwnership().Also(ownership => ownership.AssignmentGroup = Guid.NewGuid()).IsEmpty.Should().BeFalse();
		}

		[TestMethod]
		public void JobAsset_DefaultState_IsEmpty()
		{
			new JobAsset().IsEmpty.Should().BeTrue();
		}

		[TestMethod]
		public void JobAsset_AnyFieldSet_IsNotEmpty()
		{
			new JobAsset().Also(asset => asset.AssetId = new SdmObjectReference<Asset>(Guid.NewGuid().ToString())).IsEmpty.Should().BeFalse();
			new JobAsset().Also(asset => asset.Action = SlcPlan_And_Build.Enums.ActionforassetenumEnum.Reinstalled).IsEmpty.Should().BeFalse();
		}

		[TestMethod]
		public void JobAttachment_DefaultState_IsEmpty()
		{
			new JobAttachment().IsEmpty.Should().BeTrue();
		}

		[TestMethod]
		public void JobAttachment_AnyFieldSet_IsNotEmpty()
		{
			new JobAttachment().Also(attachment => attachment.FilePath = @"C:\attachments\plan.pdf").IsEmpty.Should().BeFalse();
			new JobAttachment().Also(attachment => attachment.AttachedAt = DateTime.UtcNow).IsEmpty.Should().BeFalse();
			new JobAttachment().Also(attachment => attachment.AttachedBy = Guid.NewGuid()).IsEmpty.Should().BeFalse();
		}

		[TestMethod]
		public void JobConnection_DefaultState_IsEmpty()
		{
			new JobConnection().IsEmpty.Should().BeTrue();
		}

		[TestMethod]
		public void JobConnection_AnyFieldSet_IsNotEmpty()
		{
			new JobConnection().Also(connection => connection.ConnectionId = new SdmObjectReference<Connection>(Guid.NewGuid().ToString())).IsEmpty.Should().BeFalse();
			new JobConnection().Also(connection => connection.Source = "Source").IsEmpty.Should().BeFalse();
			new JobConnection().Also(connection => connection.Destination = "Destination").IsEmpty.Should().BeFalse();
			new JobConnection().Also(connection => connection.Status = "Active").IsEmpty.Should().BeFalse();
			new JobConnection().Also(connection => connection.CableType = new SdmObjectReference<CableType>(Guid.NewGuid().ToString())).IsEmpty.Should().BeFalse();
			new JobConnection().Also(connection => connection.CableLength = 12.5).IsEmpty.Should().BeFalse();
		}
	}

	internal static class ObjectExtensions
	{
		public static T Also<T>(this T obj, Action<T> action)
		{
			action(obj);
			return obj;
		}
	}
}
