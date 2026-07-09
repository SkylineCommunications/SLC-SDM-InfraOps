namespace SDM.PlanAndBuild.Tests.Middleware
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using FluentAssertions;
	using FluentAssertions.Execution;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using SDM.PlanAndBuild.Tests.Setup;

	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.SDM.PlanAndBuild.Middleware;
	using Skyline.DataMiner.SDM.PlanAndBuild.Models;
	using Skyline.DataMiner.SDM.PlanAndBuild.Validation;
	using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Exceptions;
	using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

	/// <summary>
	/// Tests for <see cref="PlanAndBuildJobValidationMiddleware"/>.
	/// </summary>
	[TestClass]
	public class PlanAndBuildJobValidationMiddlewareTests : BaseRepositoryTest
	{
		private PlanAndBuildJobValidationMiddleware _middleware = null!;
		private JobType _jobType = null!;

		[TestInitialize]
		public void Setup()
		{
			_middleware = new PlanAndBuildJobValidationMiddleware(new PlanAndBuildJobValidator(Helper));
			_jobType = Helper.JobTypes.Create(new JobType { Name = "Installation" });
		}

		private PlanAndBuildJob ValidJob() => new PlanAndBuildJob
		{
			JobName = "Valid Job",
			JobType = new SdmObjectReference<JobType>(_jobType.Identifier),
		};

		private static PlanAndBuildJob InvalidJob() => new PlanAndBuildJob
		{
			JobName = string.Empty,
			JobType = null,
		};

		#region Single Create/Update

		[TestMethod]
		public void OnCreate_Single_WithValidJob_ShouldCallNext()
		{
			var job = ValidJob();
			var nextCalled = false;

			var result = _middleware.OnCreate(job, j => { nextCalled = true; return j; });

			using (new AssertionScope())
			{
				nextCalled.Should().BeTrue();
				result.Should().Be(job);
			}
		}

		[TestMethod]
		public void OnCreate_Single_WithInvalidJob_ShouldThrowAndNotCallNext()
		{
			var job = InvalidJob();
			var nextCalled = false;

			Action act = () => _middleware.OnCreate(job, j => { nextCalled = true; return j; });

			using (new AssertionScope())
			{
				act.Should().Throw<ValidationException>();
				nextCalled.Should().BeFalse();
			}
		}

		[TestMethod]
		public void OnUpdate_Single_WithInvalidJob_ShouldThrow()
		{
			var job = InvalidJob();

			Action act = () => _middleware.OnUpdate(job, j => j);

			act.Should().Throw<ValidationException>();
		}

		#endregion

		#region Bulk Create/Update

		[TestMethod]
		public void OnCreate_Bulk_WithAllValidJobs_ShouldCallNext()
		{
			var jobs = new List<PlanAndBuildJob> { ValidJob(), new PlanAndBuildJob { JobName = "Another Valid Job", JobType = new SdmObjectReference<JobType>(_jobType.Identifier) } };
			var nextCalled = false;

			_middleware.OnCreate(jobs, j => { nextCalled = true; return j.ToList(); });

			nextCalled.Should().BeTrue();
		}

		[TestMethod]
		public void OnCreate_Bulk_WithOneInvalidJob_ShouldThrowBulkValidationException()
		{
			var jobs = new List<PlanAndBuildJob> { ValidJob(), InvalidJob() };

			Action act = () => _middleware.OnCreate(jobs, j => j.ToList());

			var exception = act.Should().Throw<BulkValidationException<PlanAndBuildJob>>().Which;
			exception.FailedCount.Should().Be(1);
		}

		[TestMethod]
		public void OnCreateOrUpdate_Bulk_WithOneInvalidJob_ShouldThrowBulkValidationException()
		{
			var jobs = new List<PlanAndBuildJob> { ValidJob(), InvalidJob(), InvalidJob() };

			Action act = () => _middleware.OnCreateOrUpdate(jobs, j => j.ToList());

			var exception = act.Should().Throw<BulkValidationException<PlanAndBuildJob>>().Which;
			exception.FailedCount.Should().Be(2);
		}

		[TestMethod]
		public void OnCreate_Bulk_WithDuplicateJobNamesInBatch_ShouldThrowBulkValidationException()
		{
			// Regression test: two brand-new jobs sharing a JobName in the same bulk create call must be
			// rejected even though neither exists in the DOM yet (in-memory batch conflict detection).
			var jobs = new List<PlanAndBuildJob>
			{
				new PlanAndBuildJob { JobName = "Duplicate Job Name", JobType = new SdmObjectReference<JobType>(_jobType.Identifier) },
				new PlanAndBuildJob { JobName = "Duplicate Job Name", JobType = new SdmObjectReference<JobType>(_jobType.Identifier) },
			};
			var nextCalled = false;

			Action act = () => _middleware.OnCreate(jobs, j => { nextCalled = true; return j.ToList(); });

			using (new AssertionScope())
			{
				var exception = act.Should().Throw<BulkValidationException<PlanAndBuildJob>>().Which;
				exception.FailedCount.Should().Be(2);
				nextCalled.Should().BeFalse();
			}
		}

		#endregion

		#region Pass-through operations

		[TestMethod]
		public void OnRead_WithNullFilter_ShouldThrowArgumentNullException()
		{
			Action act = () => _middleware.OnRead((Skyline.DataMiner.Net.Messages.SLDataGateway.FilterElement<PlanAndBuildJob>)null!, f => Enumerable.Empty<PlanAndBuildJob>());

			act.Should().Throw<ArgumentNullException>();
		}

		[TestMethod]
		public void OnCount_WithNullFilter_ShouldThrowArgumentNullException()
		{
			Action act = () => _middleware.OnCount((Skyline.DataMiner.Net.Messages.SLDataGateway.FilterElement<PlanAndBuildJob>)null!, f => 0L);

			act.Should().Throw<ArgumentNullException>();
		}

		#endregion
	}
}
