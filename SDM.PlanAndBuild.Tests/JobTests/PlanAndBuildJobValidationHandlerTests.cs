namespace SDM.PlanAndBuild.Tests.JobTests
{
	using System;

	using FluentAssertions;
	using FluentAssertions.Execution;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.SDM.PlanAndBuild.Models;
	using Skyline.DataMiner.SDM.PlanAndBuild.Validation;

	using static Skyline.DataMiner.SDM.PlanAndBuild.Validation.PlanAndBuildJobValidationHandler;

	/// <summary>
	/// Tests for the pure PlanAndBuildJobValidationHandler business rules (JobName, JobType, End time).
	/// </summary>
	[TestClass]
	public class PlanAndBuildJobValidationHandlerTests
	{
		#region IsJobNameValid

		[TestMethod]
		public void IsJobNameValid_WithNullJob_ShouldReturnInvalid()
		{
			var isValid = IsJobNameValid(null!, out var result);

			using (new AssertionScope())
			{
				isValid.Should().BeFalse();
				result.TryGetFailReason(PlanAndBuildJobValidationField.Job, out var reason).Should().BeTrue();
				reason.Should().Contain("cannot be null");
			}
		}

		[DataTestMethod]
		[DataRow("")]
		[DataRow(" ")]
		[DataRow(null)]
		public void IsJobNameValid_WithEmptyOrWhitespaceName_ShouldReturnInvalid(string jobName)
		{
			var job = new PlanAndBuildJob { JobName = jobName };

			var isValid = IsJobNameValid(job, out var result);

			using (new AssertionScope())
			{
				isValid.Should().BeFalse();
				result.TryGetFailReason(PlanAndBuildJobValidationField.JobName, out var reason).Should().BeTrue();
				reason.Should().Contain("cannot be empty or whitespace");
			}
		}

		[TestMethod]
		public void IsJobNameValid_WithValidName_ShouldReturnValid()
		{
			var job = new PlanAndBuildJob { JobName = "Install Rack 1 Equipment" };

			var isValid = IsJobNameValid(job, out var result);

			using (new AssertionScope())
			{
				isValid.Should().BeTrue();
				result.FailureReasons.Should().BeEmpty();
			}
		}

		#endregion

		#region IsJobTypeValid

		[TestMethod]
		public void IsJobTypeValid_WithNullJob_ShouldReturnInvalid()
		{
			var isValid = IsJobTypeValid(null!, out var result);

			using (new AssertionScope())
			{
				isValid.Should().BeFalse();
				result.TryGetFailReason(PlanAndBuildJobValidationField.Job, out var reason).Should().BeTrue();
				reason.Should().Contain("cannot be null");
			}
		}

		[TestMethod]
		public void IsJobTypeValid_WithNoJobTypeSelected_ShouldReturnInvalid()
		{
			var job = new PlanAndBuildJob { JobName = "Some Job", Type = null };

			var isValid = IsJobTypeValid(job, out var result);

			using (new AssertionScope())
			{
				isValid.Should().BeFalse();
				result.TryGetFailReason(PlanAndBuildJobValidationField.JobType, out var reason).Should().BeTrue();
				reason.Should().Contain("must be selected");
			}
		}

		[TestMethod]
		public void IsJobTypeValid_WithJobTypeSelected_ShouldReturnValid()
		{
			var job = new PlanAndBuildJob { JobName = "Some Job", Type = new SdmObjectReference<JobType>(Guid.NewGuid().ToString()) };

			var isValid = IsJobTypeValid(job, out var result);

			using (new AssertionScope())
			{
				isValid.Should().BeTrue();
				result.FailureReasons.Should().BeEmpty();
			}
		}

		#endregion

		#region IsEndTimeValid

		[TestMethod]
		public void IsEndTimeValid_WithNullJob_ShouldReturnInvalid()
		{
			var isValid = IsEndTimeValid(null!, out var result);

			using (new AssertionScope())
			{
				isValid.Should().BeFalse();
				result.TryGetFailReason(PlanAndBuildJobValidationField.Job, out var reason).Should().BeTrue();
				reason.Should().Contain("cannot be null");
			}
		}

		[TestMethod]
		public void IsEndTimeValid_WithNoEndTime_ShouldReturnValid()
		{
			var job = new PlanAndBuildJob { Start = DateTime.UtcNow, End = null };

			var isValid = IsEndTimeValid(job, out var result);

			isValid.Should().BeTrue();
		}

		[TestMethod]
		public void IsEndTimeValid_WithEndAfterStart_ShouldReturnValid()
		{
			var start = new DateTime(2026, 1, 1);
			var job = new PlanAndBuildJob { Start = start, End = start.AddDays(1) };

			var isValid = IsEndTimeValid(job, out var result);

			isValid.Should().BeTrue();
		}

		[TestMethod]
		public void IsEndTimeValid_WithEndEqualToStart_ShouldReturnInvalid()
		{
			var moment = new DateTime(2026, 1, 1);
			var job = new PlanAndBuildJob { Start = moment, End = moment };

			var isValid = IsEndTimeValid(job, out var result);

			using (new AssertionScope())
			{
				isValid.Should().BeFalse();
				result.TryGetFailReason(PlanAndBuildJobValidationField.End, out var reason).Should().BeTrue();
				reason.Should().Contain("End time must be higher than Start time");
			}
		}

		[TestMethod]
		public void IsEndTimeValid_WithEndBeforeStart_ShouldReturnInvalid()
		{
			var start = new DateTime(2026, 1, 10);
			var job = new PlanAndBuildJob { Start = start, End = start.AddDays(-1) };

			var isValid = IsEndTimeValid(job, out var result);

			isValid.Should().BeFalse();
		}

		#endregion
	}
}
