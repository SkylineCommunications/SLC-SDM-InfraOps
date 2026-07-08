namespace SDM.PlanAndBuild.Tests.JobTests
{
	using System;

	using FluentAssertions;
	using FluentAssertions.Execution;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using SDM.PlanAndBuild.Tests.Setup;

	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.SDM.PlanAndBuild.Models;
	using Skyline.DataMiner.SDM.PlanAndBuild.Validation;

	/// <summary>
	/// Tests for PlanAndBuildJobValidator, which validates PlanAndBuildJob business rules including
	/// JobName presence/uniqueness, JobType selection and Start/End consistency.
	/// </summary>
	[TestClass]
	public class PlanAndBuildJobValidatorTests : BaseRepositoryTest
	{
		private PlanAndBuildJobValidator _validator = null!;
		private JobType _jobType = null!;

		[TestInitialize]
		public void Setup()
		{
			_validator = new PlanAndBuildJobValidator(Helper);
			_jobType = Helper.JobTypes.Create(new JobType { Name = "Installation" });
		}

		#region Validate - Happy Path

		[TestMethod]
		public void Validate_WithAllValidFields_ShouldReturnValid()
		{
			var job = new PlanAndBuildJob
			{
				JobName = "Install Rack 1 Equipment",
				JobType = new SdmObjectReference<JobType>(_jobType.Identifier),
				Start = new DateTime(2026, 1, 10),
				End = new DateTime(2026, 1, 15),
			};

			var result = _validator.Validate(job);

			using (new AssertionScope())
			{
				result.IsValid.Should().BeTrue();
				result.FailureReasons.Should().BeEmpty();
			}
		}

		[TestMethod]
		public void Validate_WithNullJob_ShouldThrowArgumentNullException()
		{
			_validator.Invoking(v => v.Validate(null!))
				.Should().Throw<ArgumentNullException>();
		}

		#endregion

		#region JobName

		[TestMethod]
		public void Validate_WithEmptyJobName_ShouldReturnInvalid()
		{
			var job = new PlanAndBuildJob
			{
				JobName = string.Empty,
				JobType = new SdmObjectReference<JobType>(_jobType.Identifier),
			};

			var result = _validator.Validate(job);

			using (new AssertionScope())
			{
				result.IsValid.Should().BeFalse();
				result.TryGetFailReason(PlanAndBuildJobValidationHandler.PlanAndBuildJobValidationField.JobName, out var reason).Should().BeTrue();
			}
		}

		[TestMethod]
		public void Validate_WithDuplicateJobName_ShouldReturnInvalid()
		{
			Helper.Jobs.Create(new PlanAndBuildJob
			{
				JobName = "Install Rack 1 Equipment",
				JobType = new SdmObjectReference<JobType>(_jobType.Identifier),
			});

			var newJob = new PlanAndBuildJob
			{
				JobName = "Install Rack 1 Equipment",
				JobType = new SdmObjectReference<JobType>(_jobType.Identifier),
			};

			var result = _validator.Validate(newJob);

			using (new AssertionScope())
			{
				result.IsValid.Should().BeFalse();
				result.TryGetFailReason(PlanAndBuildJobValidationHandler.PlanAndBuildJobValidationField.JobName, out var reason).Should().BeTrue();
				reason.Should().Contain("already in use");
			}
		}

		[TestMethod]
		public void Validate_ExistingJobUnchanged_ShouldNotConflictWithItself()
		{
			var created = Helper.Jobs.Create(new PlanAndBuildJob
			{
				JobName = "Install Rack 1 Equipment",
				JobType = new SdmObjectReference<JobType>(_jobType.Identifier),
			});

			var result = _validator.Validate(created);

			result.IsValid.Should().BeTrue("the uniqueness check must exclude the Job's own identifier");
		}

		#endregion

		#region JobType

		[TestMethod]
		public void Validate_WithNoJobTypeSelected_ShouldReturnInvalid()
		{
			var job = new PlanAndBuildJob { JobName = "Some Job", JobType = null };

			var result = _validator.Validate(job);

			using (new AssertionScope())
			{
				result.IsValid.Should().BeFalse();
				result.TryGetFailReason(PlanAndBuildJobValidationHandler.PlanAndBuildJobValidationField.JobType, out var reason).Should().BeTrue();
			}
		}

		#endregion

		#region Start/End Dates

		[TestMethod]
		public void Validate_WithEndBeforeStart_ShouldReturnInvalid()
		{
			var start = new DateTime(2026, 1, 10);
			var job = new PlanAndBuildJob
			{
				JobName = "Some Job",
				JobType = new SdmObjectReference<JobType>(_jobType.Identifier),
				Start = start,
				End = start.AddDays(-1),
			};

			var result = _validator.Validate(job);

			using (new AssertionScope())
			{
				result.IsValid.Should().BeFalse();
				result.TryGetFailReason(PlanAndBuildJobValidationHandler.PlanAndBuildJobValidationField.End, out var reason).Should().BeTrue();
			}
		}

		[TestMethod]
		public void Validate_WithNoEndDate_ShouldReturnValid()
		{
			var job = new PlanAndBuildJob
			{
				JobName = "Some Job",
				JobType = new SdmObjectReference<JobType>(_jobType.Identifier),
				Start = DateTime.UtcNow,
				End = null,
			};

			var result = _validator.Validate(job);

			result.IsValid.Should().BeTrue();
		}

		#endregion

		#region Change-Tracking Only Validates Changed Fields

		[TestMethod]
		public void Validate_ExistingJobWithOnlyRemarksChanged_ShouldNotRevalidateUnchangedFields()
		{
			// Regression guard: mutating an unrelated field on a fetched Job should not trigger
			// JobName/JobType/date validation for fields that were never touched.
			var created = Helper.Jobs.Create(new PlanAndBuildJob
			{
				JobName = "Install Rack 1 Equipment",
				JobType = new SdmObjectReference<JobType>(_jobType.Identifier),
			});

			created.Remarks = "Updated remarks only";

			var result = _validator.Validate(created);

			result.IsValid.Should().BeTrue();
		}

		#endregion
	}
}
