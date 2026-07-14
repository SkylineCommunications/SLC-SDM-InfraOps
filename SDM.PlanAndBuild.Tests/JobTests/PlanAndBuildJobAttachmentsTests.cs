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
	/// (AddJobAttachment/RemoveItemFromJobAttachments/SetJobAttachments/ClearJobAttachments),
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
		public void SetJobAttachments_ShouldReplaceExistingList()
		{
			var job = new PlanAndBuildJob();
			job.AddJobAttachment(new JobAttachment { FilePath = @"C:\file1.pdf" });

			var replacement = new List<JobAttachment>
			{
				new JobAttachment { FilePath = @"C:\file2.pdf" },
				new JobAttachment { FilePath = @"C:\file3.pdf" },
			};
			job.SetJobAttachments(replacement);

			job.Attachments.Should().BeEquivalentTo(replacement);
		}

		[TestMethod]
		public void ClearJobAttachments_ShouldEmptyList()
		{
			var job = new PlanAndBuildJob();
			job.AddJobAttachment(new JobAttachment { FilePath = @"C:\file1.pdf" });

			job.ClearJobAttachments();

			job.Attachments.Should().BeEmpty();
		}
	}
}
