namespace SDM.PlanAndBuild.Tests.JobTests
{
	using System;
	using System.Collections.Generic;

	using FluentAssertions;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using Skyline.DataMiner.SDM.PlanAndBuild.Extensions;
	using Skyline.DataMiner.SDM.PlanAndBuild.Models;

	/// <summary>
	/// Unit tests for the JobAttachment convenience methods on <see cref="PlanAndBuildJob"/>
	/// (AddJobAttachment/RemoveItemFromJobAttachments),
	/// mirroring InfraOpsShared's JobWrapper API.
	/// </summary>
	[TestClass]
	public class PlanAndBuildJobAttachmentsTests
	{
		[TestMethod]
		public void AddJobAttachment_NewAttachment_ShouldBeAdded()
		{
			var job = new PlanAndBuildJob();
			var attachment = new JobAttachment { FilePath = @"C:\file1.pdf" };

			job.AddJobAttachment(attachment);

			job.Attachments.Should().ContainSingle().Which.Should().Be(attachment);
		}

		[TestMethod]
		public void AddJobAttachment_Null_ShouldThrow()
		{
			var job = new PlanAndBuildJob();

			Action act = () => job.AddJobAttachment(null);

			act.Should().Throw<ArgumentNullException>();
		}

		[TestMethod]
		public void AddJobAttachment_DuplicateFilePath_ShouldThrow()
		{
			var job = new PlanAndBuildJob();
			job.AddJobAttachment(new JobAttachment { FilePath = @"C:\file1.pdf" });

			Action act = () => job.AddJobAttachment(new JobAttachment { FilePath = @"C:\file1.pdf" });

			act.Should().Throw<InvalidOperationException>();
		}

		[TestMethod]
		public void AddJobAttachment_DuplicateFilePathDifferentCase_ShouldThrow()
		{
			var job = new PlanAndBuildJob();
			job.AddJobAttachment(new JobAttachment { FilePath = @"C:\File1.pdf" });

			Action act = () => job.AddJobAttachment(new JobAttachment { FilePath = @"c:\FILE1.PDF" });

			act.Should().Throw<InvalidOperationException>();
		}

		[TestMethod]
		public void JobAttachment_FilePathDifferingOnlyInCase_ShouldBeEqualWithSameHashCode()
		{
			var at = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			var left = new JobAttachment { FilePath = @"C:\File1.pdf", AttachedAt = at, AttachedBy = "user" };
			var right = new JobAttachment { FilePath = @"c:\FILE1.PDF", AttachedAt = at, AttachedBy = "user" };

			left.Should().Be(right);
			left.GetHashCode().Should().Be(right.GetHashCode());
		}

		[TestMethod]
		public void RemoveItemFromJobAttachments_DifferentCase_ShouldBeRemoved()
		{
			var job = new PlanAndBuildJob();
			job.AddJobAttachment(new JobAttachment { FilePath = @"C:\File1.pdf" });

			job.RemoveItemFromJobAttachments(new JobAttachment { FilePath = @"c:\FILE1.PDF" });

			job.Attachments.Should().BeEmpty();
		}

		[TestMethod]
		public void RemoveItemFromJobAttachments_ExistingAttachment_ShouldBeRemoved()
		{
			var job = new PlanAndBuildJob();
			var attachment = new JobAttachment { FilePath = @"C:\file1.pdf" };
			job.AddJobAttachment(attachment);

			job.RemoveItemFromJobAttachments(attachment);

			job.Attachments.Should().BeEmpty();
		}

		[TestMethod]
		public void RemoveItemFromJobAttachments_Null_ShouldThrow()
		{
			var job = new PlanAndBuildJob();

			Action act = () => job.RemoveItemFromJobAttachments(null);

			act.Should().Throw<ArgumentNullException>();
		}

		[TestMethod]
		public void RemoveItemFromJobAttachments_NotFound_ShouldThrow()
		{
			var job = new PlanAndBuildJob();
			var attachment = new JobAttachment { FilePath = @"C:\file1.pdf" };

			Action act = () => job.RemoveItemFromJobAttachments(attachment);

			act.Should().Throw<ArgumentException>();
		}

		[TestMethod]
		public void SettingAttachments_ShouldReplaceExistingList()
		{
			var job = new PlanAndBuildJob();
			job.AddJobAttachment(new JobAttachment { FilePath = @"C:\file1.pdf" });

			var replacement = new List<JobAttachment>
			{
				new JobAttachment { FilePath = @"C:\file2.pdf" },
				new JobAttachment { FilePath = @"C:\file3.pdf" },
			};
			job.Attachments = replacement;

			job.Attachments.Should().BeEquivalentTo(replacement);
		}

		[TestMethod]
		public void ClearingAttachments_ShouldEmptyList()
		{
			var job = new PlanAndBuildJob();
			job.AddJobAttachment(new JobAttachment { FilePath = @"C:\file1.pdf" });

			job.Attachments.Clear();

			job.Attachments.Should().BeEmpty();
		}
	}
}
