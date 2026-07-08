namespace SDM.PlanAndBuild.Tests.Setup
{
	using System.Linq;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM.PlanAndBuild.Models;

	/// <summary>
	/// Tests to validate DemoData templates and the population process.
	/// If these tests fail, fix the demo data templates in DemoData.cs.
	/// </summary>
	[TestClass]
	public class DemoDataValidationTests : BaseRepositoryTest
	{
		/// <summary>
		/// Main validation test - ensures all demo data can be populated through validation middleware.
		/// If this fails, validation middleware caught an issue during Create().
		/// </summary>
		[TestMethod]
		public void DemoData_ShouldPopulateWithoutValidationErrors()
		{
			// Act - This will throw if validation middleware fails.
			Helper.PopulateJobTypes();
			Helper.PopulateJobs();
			Helper.PopulateAppSettings();

			// Assert
			Assert.IsTrue(Helper.JobTypes.Read(new TRUEFilterElement<JobType>()).Any(), "JobTypes should be populated");
			Assert.IsTrue(Helper.Jobs.Read(new TRUEFilterElement<PlanAndBuildJob>()).Any(), "Jobs should be populated");
			Assert.IsTrue(Helper.AppSettings.Read(new TRUEFilterElement<PlanAndBuildAppSettings>()).Any(), "AppSettings should be populated");
		}

		/// <summary>
		/// Validates JobType templates have required fields before population.
		/// </summary>
		[TestMethod]
		public void DemoData_JobTypes_ShouldHaveRequiredFields()
		{
			var jobTypes = DemoData.JobTypes.ToList();

			if (!jobTypes.Any())
			{
				Assert.Inconclusive("No JobTypes in DemoData to validate");
				return;
			}

			for (int i = 0; i < jobTypes.Count; i++)
			{
				var jobType = jobTypes[i];
				Assert.IsFalse(string.IsNullOrWhiteSpace(jobType.Name), $"JobType at index {i}: Name should not be empty");
			}
		}

		/// <summary>
		/// Checks for duplicate Names in JobType templates, which would fail uniqueness validation
		/// when populated through the validation middleware.
		/// </summary>
		[TestMethod]
		public void DemoData_JobTypes_ShouldHaveUniqueNames()
		{
			var jobTypes = DemoData.JobTypes.ToList();

			var duplicates = jobTypes
				.GroupBy(jt => jt.Name, System.StringComparer.OrdinalIgnoreCase)
				.Where(g => g.Count() > 1)
				.ToList();

			if (duplicates.Any())
			{
				var duplicateList = string.Join(", ", duplicates.Select(g => $"'{g.Key}' ({g.Count()}x)"));
				Assert.Fail($"Found {duplicates.Count} duplicate JobType Name(s) in demo data templates: {duplicateList}");
			}
		}

		/// <summary>
		/// Validates Job templates have required fields before population.
		/// </summary>
		[TestMethod]
		public void DemoData_Jobs_ShouldHaveRequiredFields()
		{
			var jobs = DemoData.Jobs.ToList();

			if (!jobs.Any())
			{
				Assert.Inconclusive("No Jobs in DemoData to validate");
				return;
			}

			for (int i = 0; i < jobs.Count; i++)
			{
				var job = jobs[i];
				Assert.IsFalse(string.IsNullOrWhiteSpace(job.JobName), $"Job at index {i}: JobName should not be empty");
				Assert.IsNotNull(job.JobType, $"Job at index {i}: JobType should not be null");
			}
		}

		/// <summary>
		/// Checks for duplicate JobNames in Job templates, which would fail uniqueness validation
		/// when populated through the validation middleware.
		/// </summary>
		[TestMethod]
		public void DemoData_Jobs_ShouldHaveUniqueJobNames()
		{
			var jobs = DemoData.Jobs.ToList();

			var duplicates = jobs
				.GroupBy(j => j.JobName, System.StringComparer.OrdinalIgnoreCase)
				.Where(g => g.Count() > 1)
				.ToList();

			if (duplicates.Any())
			{
				var duplicateList = string.Join(", ", duplicates.Select(g => $"'{g.Key}' ({g.Count()}x)"));
				Assert.Fail($"Found {duplicates.Count} duplicate Job JobName(s) in demo data templates: {duplicateList}");
			}
		}

		/// <summary>
		/// Ensures every Job's JobType reference in the Jobs demo data points to a JobType that
		/// actually exists in the JobTypes demo data - otherwise the fixture would silently
		/// reference a JobType that doesn't exist.
		/// </summary>
		[TestMethod]
		public void DemoData_Jobs_MustReferenceExistingJobTypes()
		{
			var jobTypeIds = DemoData.JobTypes.Select(jt => jt.Identifier).ToHashSet();
			var jobs = DemoData.Jobs.ToList();

			var errors = jobs
				.Where(j => j.JobType == null || !jobTypeIds.Contains(j.JobType.Identifier))
				.Select(j => $"Job '{j.JobName}' references unknown JobType '{j.JobType.Identifier}'")
				.ToList();

			if (errors.Any())
			{
				Assert.Fail($"Found {errors.Count} Job(s) referencing unknown JobTypes:\n{string.Join("\n", errors)}");
			}
		}

		/// <summary>
		/// Ensures End is only set when Start is also set, and when both are set, End is strictly
		/// after Start - otherwise DemoData_ShouldPopulateWithoutValidationErrors would fail.
		/// </summary>
		[TestMethod]
		public void DemoData_Jobs_EndMustBeAfterStartWhenBothSet()
		{
			var jobs = DemoData.Jobs.ToList();

			var invalid = jobs
				.Where(j => j.Start.HasValue && j.End.HasValue && j.Start.Value >= j.End.Value)
				.Select(j => $"Job '{j.JobName}' has Start ({j.Start}) >= End ({j.End})")
				.ToList();

			if (invalid.Any())
			{
				Assert.Fail($"Found {invalid.Count} Job(s) with invalid Start/End combination:\n{string.Join("\n", invalid)}");
			}
		}
	}
}
