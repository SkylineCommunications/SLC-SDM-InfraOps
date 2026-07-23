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
using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

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
			Helper.PopulateAppSettings();
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
				Type = new SdmObjectReference<JobType>(_jobType.Identifier),
				Start = new DateTime(2026, 1, 10),
				End = new DateTime(2026, 1, 15),
			};

			var result = _validator.Validate(job, RepositoryAction.Create);

			using (new AssertionScope())
			{
				result.IsValid.Should().BeTrue();
				result.FailureReasons.Should().BeEmpty();
			}
		}

		[TestMethod]
		public void Validate_WithNullJob_ShouldThrowArgumentNullException()
		{
			_validator.Invoking(v => v.Validate(null!, RepositoryAction.Create))
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
				Type = new SdmObjectReference<JobType>(_jobType.Identifier),
			};

			var result = _validator.Validate(job, RepositoryAction.Create);

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
				Type = new SdmObjectReference<JobType>(_jobType.Identifier),
			});

			var newJob = new PlanAndBuildJob
			{
				JobName = "Install Rack 1 Equipment",
				Type = new SdmObjectReference<JobType>(_jobType.Identifier),
			};

			var result = _validator.Validate(newJob, RepositoryAction.Create);

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
				Type = new SdmObjectReference<JobType>(_jobType.Identifier),
			});

			var result = _validator.Validate(created, RepositoryAction.Create);

			result.IsValid.Should().BeTrue("the uniqueness check must exclude the Job's own identifier");
		}

		#endregion

		#region JobType

		[TestMethod]
		public void Validate_WithNoJobTypeSelected_ShouldReturnInvalid()
		{
			var job = new PlanAndBuildJob { JobName = "Some Job", Type = null };

			var result = _validator.Validate(job, RepositoryAction.Create);

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
				Type = new SdmObjectReference<JobType>(_jobType.Identifier),
				Start = start,
				End = start.AddDays(-1),
			};

			var result = _validator.Validate(job, RepositoryAction.Create);

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
				Type = new SdmObjectReference<JobType>(_jobType.Identifier),
				Start = DateTime.UtcNow,
				End = null,
			};

			var result = _validator.Validate(job, RepositoryAction.Create);

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
				Type = new SdmObjectReference<JobType>(_jobType.Identifier),
			});

			created.Remarks = "Updated remarks only";

			var result = _validator.Validate(created, RepositoryAction.Create);

			result.IsValid.Should().BeTrue();
		}

		#endregion

		#region ValidateBulk

		[TestMethod]
		public void ValidateBulk_WithNullList_ShouldReturnEmptyResults()
		{
			var results = _validator.ValidateBulk(null!, RepositoryAction.Create);

			results.Should().BeEmpty();
		}

		[TestMethod]
		public void ValidateBulk_WithEmptyList_ShouldReturnEmptyResults()
		{
			var results = _validator.ValidateBulk(new System.Collections.Generic.List<PlanAndBuildJob>(), RepositoryAction.Create);

			results.Should().BeEmpty();
		}

		[TestMethod]
		public void ValidateBulk_WithAllValidJobs_ShouldReturnAllValid()
		{
			var jobs = new System.Collections.Generic.List<PlanAndBuildJob>
			{
				new PlanAndBuildJob { JobName = "Job One", Type = new SdmObjectReference<JobType>(_jobType.Identifier) },
				new PlanAndBuildJob { JobName = "Job Two", Type = new SdmObjectReference<JobType>(_jobType.Identifier) },
			};

			var results = _validator.ValidateBulk(jobs, RepositoryAction.Create);

			using (new AssertionScope())
			{
				results.Should().HaveCount(2);
				results.Should().OnlyContain(r => r.IsValid);
			}
		}

		[TestMethod]
		public void ValidateBulk_WithDuplicateJobNamesWithinBatch_ShouldFlagBothAsInvalid()
		{
			// The two jobs being created together share a JobName. Neither is persisted yet, so a
			// single-job DB uniqueness query alone would miss this - the in-memory batch check must catch it.
			var jobs = new System.Collections.Generic.List<PlanAndBuildJob>
			{
				new PlanAndBuildJob { JobName = "Install Rack 1 Equipment", Type = new SdmObjectReference<JobType>(_jobType.Identifier) },
				new PlanAndBuildJob { JobName = "Install Rack 1 Equipment", Type = new SdmObjectReference<JobType>(_jobType.Identifier) },
			};

			var results = _validator.ValidateBulk(jobs, RepositoryAction.Create);

			using (new AssertionScope())
			{
				results.Should().HaveCount(2);
				results[0].IsValid.Should().BeFalse();
				results[1].IsValid.Should().BeFalse();
				results[0].TryGetFailReason(PlanAndBuildJobValidationHandler.PlanAndBuildJobValidationField.JobName, out var reason0).Should().BeTrue();
				reason0.Should().Contain("duplicated within the validation batch");
				results[1].TryGetFailReason(PlanAndBuildJobValidationHandler.PlanAndBuildJobValidationField.JobName, out var reason1).Should().BeTrue();
				reason1.Should().Contain("duplicated within the validation batch");
			}
		}

		[TestMethod]
		public void ValidateBulk_WithDuplicateJobNamesDifferentCasing_ShouldFlagBothAsInvalid()
		{
			var jobs = new System.Collections.Generic.List<PlanAndBuildJob>
			{
				new PlanAndBuildJob { JobName = "Install Rack 1 Equipment", Type = new SdmObjectReference<JobType>(_jobType.Identifier) },
				new PlanAndBuildJob { JobName = "INSTALL RACK 1 EQUIPMENT", Type = new SdmObjectReference<JobType>(_jobType.Identifier) },
			};

			var results = _validator.ValidateBulk(jobs, RepositoryAction.Create);

			results.Should().OnlyContain(r => !r.IsValid);
		}

		[TestMethod]
		public void ValidateBulk_WithUniqueJobNamesWithinBatch_ShouldNotFlagBatchConflict()
		{
			var jobs = new System.Collections.Generic.List<PlanAndBuildJob>
			{
				new PlanAndBuildJob { JobName = "Job One", Type = new SdmObjectReference<JobType>(_jobType.Identifier) },
				new PlanAndBuildJob { JobName = "Job Two", Type = new SdmObjectReference<JobType>(_jobType.Identifier) },
				new PlanAndBuildJob { JobName = "Job Three", Type = new SdmObjectReference<JobType>(_jobType.Identifier) },
			};

			var results = _validator.ValidateBulk(jobs, RepositoryAction.Create);

			results.Should().OnlyContain(r => r.IsValid);
		}

		[TestMethod]
		public void ValidateBulk_WithBatchNameDuplicatingExistingDomJob_ShouldFlagAgainstBothChecks()
		{
			// One of the batch entries collides with an already-persisted Job (DB check), while the batch
			// itself has no in-memory duplicates - only the DB-colliding entry should be invalid.
			Helper.Jobs.Create(new PlanAndBuildJob
			{
				JobName = "Existing Job",
				Type = new SdmObjectReference<JobType>(_jobType.Identifier),
			});

			var jobs = new System.Collections.Generic.List<PlanAndBuildJob>
			{
				new PlanAndBuildJob { JobName = "Existing Job", Type = new SdmObjectReference<JobType>(_jobType.Identifier) },
				new PlanAndBuildJob { JobName = "Brand New Job", Type = new SdmObjectReference<JobType>(_jobType.Identifier) },
			};

			var results = _validator.ValidateBulk(jobs, RepositoryAction.Create);

			using (new AssertionScope())
			{
				results.Should().HaveCount(2);
				results[0].IsValid.Should().BeFalse();
				results[0].TryGetFailReason(PlanAndBuildJobValidationHandler.PlanAndBuildJobValidationField.JobName, out var reason).Should().BeTrue();
				reason.Should().Contain("already in use");
				results[1].IsValid.Should().BeTrue();
			}
		}

		[TestMethod]
		public void ValidateBulk_WithOneJobMissingName_ShouldFastFailBeforeBatchOrDbChecks()
		{
			// Phase 1 (business rules, no DB) fails fast: an empty JobName should short-circuit before
			// batch conflict detection or DB uniqueness checks even run.
			var jobs = new System.Collections.Generic.List<PlanAndBuildJob>
			{
				new PlanAndBuildJob { JobName = string.Empty, Type = new SdmObjectReference<JobType>(_jobType.Identifier) },
				new PlanAndBuildJob { JobName = "Valid Job", Type = new SdmObjectReference<JobType>(_jobType.Identifier) },
			};

			var results = _validator.ValidateBulk(jobs, RepositoryAction.Create);

			using (new AssertionScope())
			{
				results.Should().HaveCount(2);
				results[0].IsValid.Should().BeFalse();
				results[0].TryGetFailReason(PlanAndBuildJobValidationHandler.PlanAndBuildJobValidationField.JobName, out _).Should().BeTrue();
				results[1].IsValid.Should().BeTrue("only the invalid entry should fail; well-formed entries in the same batch are unaffected");
			}
		}

		#endregion

		#region ValidateBatchConflicts

		[TestMethod]
		public void ValidateBatchConflicts_WithDuplicateNames_ShouldFlagBothEntries()
		{
			var jobs = new System.Collections.Generic.List<PlanAndBuildJob>
			{
				new PlanAndBuildJob { JobName = "Duplicate Name" },
				new PlanAndBuildJob { JobName = "Duplicate Name" },
				new PlanAndBuildJob { JobName = "Unique Name" },
			};

			var results = _validator.ValidateBatchConflicts(jobs);

			using (new AssertionScope())
			{
				results.Should().HaveCount(3);
				results[0].IsValid.Should().BeFalse();
				results[1].IsValid.Should().BeFalse();
				results[2].IsValid.Should().BeTrue();
			}
		}

		[TestMethod]
		public void ValidateBatchConflicts_WithBlankJobNames_ShouldIgnoreThem()
		{
			// Blank names are covered by the info/presence check, not the batch-duplicate check.
			var jobs = new System.Collections.Generic.List<PlanAndBuildJob>
			{
				new PlanAndBuildJob { JobName = string.Empty },
				new PlanAndBuildJob { JobName = "   " },
			};

			var results = _validator.ValidateBatchConflicts(jobs);

			results.Should().OnlyContain(r => r.IsValid);
		}

		#endregion

		#region People & Organizations (AssignedTo / AssignmentGroup / AttachedBy)

		[TestMethod]
		public void Validate_WithAssignedToExistingPerson_ShouldReturnValid()
		{
			var job = new PlanAndBuildJob
			{
				JobName = "Some Job",
				Type = new SdmObjectReference<JobType>(_jobType.Identifier),
			};
			job.Ownership.AssignedTo = Guid.NewGuid();

			// Base test Helper is wired with the default People API mock, where any Guid "exists".
			var result = _validator.Validate(job, RepositoryAction.Create);

			result.IsValid.Should().BeTrue();
		}

		[TestMethod]
		public void Validate_WithAssignedToUnknownPerson_ShouldReturnInvalid()
		{
			var helper = ConnectionHelper.CreateConnection()
				.GetMockedHelperWithPeopleApi(exists: false)
				.PopulateAppSettings();
			var jobType = helper.JobTypes.Create(new JobType { Name = "Installation" });
			var validator = new PlanAndBuildJobValidator(helper);

			var job = new PlanAndBuildJob
			{
				JobName = "Some Job",
				Type = new SdmObjectReference<JobType>(jobType.Identifier),
			};
			job.Ownership.AssignedTo = Guid.NewGuid();

			var result = validator.Validate(job, RepositoryAction.Create);

			using (new AssertionScope())
			{
				result.IsValid.Should().BeFalse();
				result.TryGetFailReason(PlanAndBuildJobValidationHandler.PlanAndBuildJobValidationField.AssignedTo, out var reason).Should().BeTrue();
				reason.Should().Contain("does not exist");
			}
		}

		[TestMethod]
		public void Validate_WithAssignmentGroupUnknownTeam_ShouldReturnInvalid()
		{
			var helper = ConnectionHelper.CreateConnection()
				.GetMockedHelperWithPeopleApi(exists: false)
				.PopulateAppSettings();
			var jobType = helper.JobTypes.Create(new JobType { Name = "Installation" });
			var validator = new PlanAndBuildJobValidator(helper);

			var job = new PlanAndBuildJob
			{
				JobName = "Some Job",
				Type = new SdmObjectReference<JobType>(jobType.Identifier),
			};
			job.Ownership.AssignmentGroup = Guid.NewGuid();

			var result = validator.Validate(job, RepositoryAction.Create);

			using (new AssertionScope())
			{
				result.IsValid.Should().BeFalse();
				result.TryGetFailReason(PlanAndBuildJobValidationHandler.PlanAndBuildJobValidationField.AssignmentGroup, out var reason).Should().BeTrue();
				reason.Should().Contain("does not exist");
			}
		}

		[TestMethod]
		public void Validate_WithAttachmentAttachedByUnknownPerson_ShouldReturnInvalid()
		{
			var helper = ConnectionHelper.CreateConnection()
				.GetMockedHelperWithPeopleApi(exists: false)
				.PopulateAppSettings();
			var jobType = helper.JobTypes.Create(new JobType { Name = "Installation" });
			var validator = new PlanAndBuildJobValidator(helper);

			var job = new PlanAndBuildJob
			{
				JobName = "Some Job",
				Type = new SdmObjectReference<JobType>(jobType.Identifier),
				Attachments = new System.Collections.Generic.List<JobAttachment>
				{
					new JobAttachment { FilePath = @"C:\attachments\plan.pdf", AttachedBy = Guid.NewGuid() },
				},
			};

			var result = validator.Validate(job, RepositoryAction.Create);

			using (new AssertionScope())
			{
				result.IsValid.Should().BeFalse();
				result.TryGetFailReason(PlanAndBuildJobValidationHandler.PlanAndBuildJobValidationField.Attachments, out var reason).Should().BeTrue();
				reason.Should().Contain("does not exist");
			}
		}

		[TestMethod]
		public void Validate_WithNoAssignedToOrAttachments_ShouldNotQueryPeopleApi()
		{
			// When AssignedTo/AssignmentGroup/Attachments are left unset, validation must not fail
			// even against a People API mock where nothing "exists".
			var helper = ConnectionHelper.CreateConnection()
				.GetMockedHelperWithPeopleApi(exists: false)
				.PopulateAppSettings();
			var jobType = helper.JobTypes.Create(new JobType { Name = "Installation" });
			var validator = new PlanAndBuildJobValidator(helper);

			var job = new PlanAndBuildJob
			{
				JobName = "Some Job",
				Type = new SdmObjectReference<JobType>(jobType.Identifier),
			};

			var result = validator.Validate(job, RepositoryAction.Create);

			result.IsValid.Should().BeTrue();
		}

		#endregion
	}
}
