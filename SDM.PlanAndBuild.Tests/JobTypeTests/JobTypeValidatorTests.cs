namespace SDM.PlanAndBuild.Tests.JobTypeTests
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
	/// Tests for JobTypeValidator, which validates JobType business rules including Name uniqueness,
	/// rename-in-use blocking and delete-in-use blocking.
	/// </summary>
	[TestClass]
	public class JobTypeValidatorTests : BaseRepositoryTest
	{
		private JobTypeValidator _validator = null!;

		[TestInitialize]
		public void Setup()
		{
			_validator = new JobTypeValidator(Helper);
		}

		#region Validate - Happy Path

		[TestMethod]
		public void Validate_WithValidJobType_ShouldReturnValid()
		{
			var jobType = new JobType { Name = "Installation" };

			var result = _validator.Validate(jobType);

			using (new AssertionScope())
			{
				result.IsValid.Should().BeTrue();
				result.FailureReasons.Should().BeEmpty();
			}
		}

		[TestMethod]
		public void Validate_WithNullJobType_ShouldThrowArgumentNullException()
		{
			_validator.Invoking(v => v.Validate(null!))
				.Should().Throw<ArgumentNullException>();
		}

		#endregion

		#region Name Uniqueness

		[TestMethod]
		public void Validate_WithDuplicateName_ShouldReturnInvalid()
		{
			Helper.JobTypes.Create(new JobType { Name = "Installation" });

			var newJobType = new JobType { Name = "Installation" };

			var result = _validator.Validate(newJobType);

			using (new AssertionScope())
			{
				result.IsValid.Should().BeFalse();
				result.TryGetFailReason(JobTypeValidationHandler.JobTypeValidationField.Name, out var reason).Should().BeTrue();
				reason.Should().Contain("already in use");
			}
		}

		[TestMethod]
		public void Validate_WithUniqueName_ShouldReturnValid()
		{
			Helper.JobTypes.Create(new JobType { Name = "Installation" });

			var newJobType = new JobType { Name = "Maintenance" };

			var result = _validator.Validate(newJobType);

			result.IsValid.Should().BeTrue();
		}

		[TestMethod]
		public void Validate_ExistingJobTypeUnchanged_ShouldNotConflictWithItself()
		{
			var created = Helper.JobTypes.Create(new JobType { Name = "Installation" });

			var result = _validator.Validate(created);

			result.IsValid.Should().BeTrue("the uniqueness check must exclude the JobType's own identifier");
		}

		#endregion

		#region Rename In-Use Blocking

		[TestMethod]
		public void Validate_RenameToUnusedName_ShouldReturnValid()
		{
			var jobType = Helper.JobTypes.Create(new JobType { Name = "Installation" });

			jobType.Name = "Renamed Installation";

			var result = _validator.Validate(jobType);

			result.IsValid.Should().BeTrue();
		}

		[TestMethod]
		public void Validate_RenameWhenInUseByExistingJobs_ShouldReturnInvalid()
		{
			var jobType = Helper.JobTypes.Create(new JobType { Name = "Installation" });
			Helper.Jobs.Create(new PlanAndBuildJob
			{
				JobName = "Some Job",
				JobType = new SdmObjectReference<JobType>(jobType.Identifier),
			});

			jobType.Name = "Renamed Installation";

			var result = _validator.Validate(jobType);

			using (new AssertionScope())
			{
				result.IsValid.Should().BeFalse();
				result.TryGetFailReason(JobTypeValidationHandler.JobTypeValidationField.Name, out var reason).Should().BeTrue();
				reason.Should().Contain("in use by existing Jobs");
			}
		}

		[TestMethod]
		public void Validate_UnchangedNameWhenInUseByExistingJobs_ShouldReturnValid()
		{
			// Rename-block only applies when the Name actually changed.
			var jobType = Helper.JobTypes.Create(new JobType { Name = "Installation" });
			Helper.Jobs.Create(new PlanAndBuildJob
			{
				JobName = "Some Job",
				JobType = new SdmObjectReference<JobType>(jobType.Identifier),
			});

			jobType.Description = "Updated description only";

			var result = _validator.Validate(jobType);

			result.IsValid.Should().BeTrue();
		}

		#endregion

		#region Delete In-Use Blocking

		[TestMethod]
		public void ValidateDeletion_WhenNotInUse_ShouldReturnValid()
		{
			var jobType = Helper.JobTypes.Create(new JobType { Name = "Installation" });

			var result = _validator.ValidateDeletion(jobType);

			result.IsValid.Should().BeTrue();
		}

		[TestMethod]
		public void ValidateDeletion_WhenInUseByExistingJobs_ShouldReturnInvalid()
		{
			var jobType = Helper.JobTypes.Create(new JobType { Name = "Installation" });
			Helper.Jobs.Create(new PlanAndBuildJob
			{
				JobName = "Some Job",
				JobType = new SdmObjectReference<JobType>(jobType.Identifier),
			});

			var result = _validator.ValidateDeletion(jobType);

			using (new AssertionScope())
			{
				result.IsValid.Should().BeFalse();
				result.TryGetFailReason(JobTypeValidationHandler.JobTypeValidationField.JobType, out var reason).Should().BeTrue();
				reason.Should().Contain("in use by existing Jobs");
			}
		}

		[TestMethod]
		public void ValidateDeletion_WithNullJobType_ShouldThrowArgumentNullException()
		{
			_validator.Invoking(v => v.ValidateDeletion(null!))
				.Should().Throw<ArgumentNullException>();
		}

		#endregion
	}
}
