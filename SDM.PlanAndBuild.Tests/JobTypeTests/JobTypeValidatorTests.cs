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

		#region ValidateBulk

		[TestMethod]
		public void ValidateBulk_WithNullList_ShouldReturnEmptyResults()
		{
			var results = _validator.ValidateBulk(null!);

			results.Should().BeEmpty();
		}

		[TestMethod]
		public void ValidateBulk_WithEmptyList_ShouldReturnEmptyResults()
		{
			var results = _validator.ValidateBulk(new System.Collections.Generic.List<JobType>());

			results.Should().BeEmpty();
		}

		[TestMethod]
		public void ValidateBulk_WithAllValidJobTypes_ShouldReturnAllValid()
		{
			var jobTypes = new System.Collections.Generic.List<JobType>
			{
				new JobType { Name = "Installation" },
				new JobType { Name = "Maintenance" },
			};

			var results = _validator.ValidateBulk(jobTypes);

			using (new AssertionScope())
			{
				results.Should().HaveCount(2);
				results.Should().OnlyContain(r => r.IsValid);
			}
		}

		[TestMethod]
		public void ValidateBulk_WithDuplicateNamesWithinBatch_ShouldFlagBothAsInvalid()
		{
			// Two JobTypes being created together share a Name. Neither is persisted yet, so a
			// single-JobType DB uniqueness query alone would miss this - the in-memory batch check must catch it.
			var jobTypes = new System.Collections.Generic.List<JobType>
			{
				new JobType { Name = "Installation" },
				new JobType { Name = "Installation" },
			};

			var results = _validator.ValidateBulk(jobTypes);

			using (new AssertionScope())
			{
				results.Should().HaveCount(2);
				results[0].IsValid.Should().BeFalse();
				results[1].IsValid.Should().BeFalse();
				results[0].TryGetFailReason(JobTypeValidationHandler.JobTypeValidationField.Name, out var reason0).Should().BeTrue();
				reason0.Should().Contain("duplicated within the validation batch");
				results[1].TryGetFailReason(JobTypeValidationHandler.JobTypeValidationField.Name, out var reason1).Should().BeTrue();
				reason1.Should().Contain("duplicated within the validation batch");
			}
		}

		[TestMethod]
		public void ValidateBulk_WithDuplicateNamesDifferentCasing_ShouldFlagBothAsInvalid()
		{
			var jobTypes = new System.Collections.Generic.List<JobType>
			{
				new JobType { Name = "Installation" },
				new JobType { Name = "INSTALLATION" },
			};

			var results = _validator.ValidateBulk(jobTypes);

			results.Should().OnlyContain(r => !r.IsValid);
		}

		[TestMethod]
		public void ValidateBulk_WithUniqueNamesWithinBatch_ShouldNotFlagBatchConflict()
		{
			var jobTypes = new System.Collections.Generic.List<JobType>
			{
				new JobType { Name = "Installation" },
				new JobType { Name = "Maintenance" },
				new JobType { Name = "Decommission" },
			};

			var results = _validator.ValidateBulk(jobTypes);

			results.Should().OnlyContain(r => r.IsValid);
		}

		[TestMethod]
		public void ValidateBulk_WithBatchNameDuplicatingExistingDomJobType_ShouldFlagOnlyThatEntry()
		{
			// One of the batch entries collides with an already-persisted JobType (DB check), while the batch
			// itself has no in-memory duplicates - only the DB-colliding entry should be invalid.
			Helper.JobTypes.Create(new JobType { Name = "Installation" });

			var jobTypes = new System.Collections.Generic.List<JobType>
			{
				new JobType { Name = "Installation" },
				new JobType { Name = "Brand New Type" },
			};

			var results = _validator.ValidateBulk(jobTypes);

			using (new AssertionScope())
			{
				results.Should().HaveCount(2);
				results[0].IsValid.Should().BeFalse();
				results[0].TryGetFailReason(JobTypeValidationHandler.JobTypeValidationField.Name, out var reason).Should().BeTrue();
				reason.Should().Contain("already in use");
				results[1].IsValid.Should().BeTrue();
			}
		}

		[TestMethod]
		public void ValidateBulk_WithOneJobTypeMissingName_ShouldFastFailBeforeBatchOrDbChecks()
		{
			// Phase 1 (business rules, no DB) fails fast: an empty Name should short-circuit before
			// batch conflict detection or DB uniqueness checks even run.
			var jobTypes = new System.Collections.Generic.List<JobType>
			{
				new JobType { Name = string.Empty },
				new JobType { Name = "Valid Type" },
			};

			var results = _validator.ValidateBulk(jobTypes);

			using (new AssertionScope())
			{
				results.Should().HaveCount(2);
				results[0].IsValid.Should().BeFalse();
				results[0].TryGetFailReason(JobTypeValidationHandler.JobTypeValidationField.Name, out _).Should().BeTrue();
				results[1].IsValid.Should().BeTrue("only the invalid entry should fail; well-formed entries in the same batch are unaffected");
			}
		}

		#endregion

		#region ValidateBatchConflicts

		[TestMethod]
		public void ValidateBatchConflicts_WithDuplicateNames_ShouldFlagBothEntries()
		{
			var jobTypes = new System.Collections.Generic.List<JobType>
			{
				new JobType { Name = "Duplicate Name" },
				new JobType { Name = "Duplicate Name" },
				new JobType { Name = "Unique Name" },
			};

			var results = _validator.ValidateBatchConflicts(jobTypes);

			using (new AssertionScope())
			{
				results.Should().HaveCount(3);
				results[0].IsValid.Should().BeFalse();
				results[1].IsValid.Should().BeFalse();
				results[2].IsValid.Should().BeTrue();
			}
		}

		[TestMethod]
		public void ValidateBatchConflicts_WithBlankNames_ShouldIgnoreThem()
		{
			// Blank names are covered by the info/presence check, not the batch-duplicate check.
			var jobTypes = new System.Collections.Generic.List<JobType>
			{
				new JobType { Name = string.Empty },
				new JobType { Name = "   " },
			};

			var results = _validator.ValidateBatchConflicts(jobTypes);

			results.Should().OnlyContain(r => r.IsValid);
		}

		#endregion
	}
}
