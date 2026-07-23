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
	using Skyline.DataMiner.SDM.PlanAndBuild.Models;
	using Skyline.DataMiner.SDM.PlanAndBuild.Validation;
	using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Exceptions;
	using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Middleware;
	using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

	/// <summary>
	/// Tests for <see cref="ValidationMiddleware{T}"/> wired with <see cref="JobTypeValidator"/>, including the delete-in-use guard.
	/// </summary>
	[TestClass]
	public class JobTypeValidationMiddlewareTests : BaseRepositoryTest
	{
		private ValidationMiddleware<JobType> _middleware = null!;

		[TestInitialize]
		public void Setup()
		{
			Helper.PopulateAppSettings();
			var validator = new JobTypeValidator(Helper);
			_middleware = new ValidationMiddleware<JobType>(
				validator,
				jt => string.IsNullOrEmpty(jt.Name) ? $"Job Type '{jt.Identifier}'" : $"Job Type '{jt.Name}'",
				jt => validator.ValidateDeletion(jt));
		}

		private static JobType ValidJobType() => new JobType { Name = "Installation" };

		private static JobType InvalidJobType() => new JobType { Name = string.Empty };

		#region Single Create/Update

		[TestMethod]
		public void OnCreate_Single_WithValidJobType_ShouldCallNext()
		{
			var jobType = ValidJobType();
			var nextCalled = false;

			var result = _middleware.OnCreate(jobType, jt => { nextCalled = true; return jt; });

			using (new AssertionScope())
			{
				nextCalled.Should().BeTrue();
				result.Should().Be(jobType);
			}
		}

		[TestMethod]
		public void OnCreate_Single_WithInvalidJobType_ShouldThrowAndNotCallNext()
		{
			var jobType = InvalidJobType();
			var nextCalled = false;

			Action act = () => _middleware.OnCreate(jobType, jt => { nextCalled = true; return jt; });

			using (new AssertionScope())
			{
				act.Should().Throw<ValidationException>();
				nextCalled.Should().BeFalse();
			}
		}

		#endregion

		#region Bulk Create/Update

		[TestMethod]
		public void OnCreate_Bulk_WithOneInvalidJobType_ShouldThrowBulkValidationException()
		{
			var jobTypes = new List<JobType> { ValidJobType(), InvalidJobType() };

			Action act = () => _middleware.OnCreate(jobTypes, jt => jt.ToList());

			var exception = act.Should().Throw<BulkValidationException<JobType>>().Which;
			exception.FailedCount.Should().Be(1);
		}

		[TestMethod]
		public void OnCreate_Bulk_WithDuplicateNamesInBatch_ShouldThrowBulkValidationException()
		{
			// Regression test: two brand-new JobTypes sharing a Name in the same bulk create call must be
			// rejected even though neither exists in the DOM yet (in-memory batch conflict detection).
			var jobTypes = new List<JobType> { new JobType { Name = "Duplicate Type" }, new JobType { Name = "Duplicate Type" } };
			var nextCalled = false;

			Action act = () => _middleware.OnCreate(jobTypes, jt => { nextCalled = true; return jt.ToList(); });

			using (new AssertionScope())
			{
				var exception = act.Should().Throw<BulkValidationException<JobType>>().Which;
				exception.FailedCount.Should().Be(2);
				nextCalled.Should().BeFalse();
			}
		}

		#endregion

		#region Delete Guard

		[TestMethod]
		public void OnDelete_Single_WhenNotInUse_ShouldCallNext()
		{
			var jobType = Helper.JobTypes.Create(new JobType { Name = "Installation" });
			var nextCalled = false;

			_middleware.OnDelete(jobType, jt => { nextCalled = true; });

			nextCalled.Should().BeTrue();
		}

		[TestMethod]
		public void OnDelete_Single_WhenInUseByExistingJobs_ShouldThrowAndNotCallNext()
		{
			var jobType = Helper.JobTypes.Create(new JobType { Name = "Installation" });
			Helper.Jobs.Create(new PlanAndBuildJob
			{
				JobName = "Some Job",
				Type = new SdmObjectReference<JobType>(jobType.Identifier),
			});
			var nextCalled = false;

			Action act = () => _middleware.OnDelete(jobType, jt => { nextCalled = true; });

			using (new AssertionScope())
			{
				act.Should().Throw<ValidationException>();
				nextCalled.Should().BeFalse();
			}
		}

		[TestMethod]
		public void OnDelete_Single_WithNullJobType_ShouldThrowArgumentNullException()
		{
			Action act = () => _middleware.OnDelete((JobType)null!, jt => { });

			act.Should().Throw<ArgumentNullException>();
		}

		[TestMethod]
		public void OnDelete_Bulk_WhenOneInUseByExistingJobs_ShouldThrowBulkValidationException()
		{
			var freeJobType = Helper.JobTypes.Create(new JobType { Name = "Free Type" });
			var inUseJobType = Helper.JobTypes.Create(new JobType { Name = "In-Use Type" });
			Helper.Jobs.Create(new PlanAndBuildJob
			{
				JobName = "Some Job",
				Type = new SdmObjectReference<JobType>(inUseJobType.Identifier),
			});

			Action act = () => _middleware.OnDelete(new List<JobType> { freeJobType, inUseJobType }, jts => { });

			var exception = act.Should().Throw<BulkValidationException<JobType>>().Which;
			exception.FailedCount.Should().Be(1);
		}

		[TestMethod]
		public void OnDelete_Bulk_WithNullCollection_ShouldThrowArgumentNullException()
		{
			Action act = () => _middleware.OnDelete((IEnumerable<JobType>)null!, jt => { });

			act.Should().Throw<ArgumentNullException>();
		}

		#endregion

		#region Pass-through operations

		[TestMethod]
		public void OnRead_WithNullFilter_ShouldThrowArgumentNullException()
		{
			Action act = () => _middleware.OnRead((Skyline.DataMiner.Net.Messages.SLDataGateway.FilterElement<JobType>)null!, f => Enumerable.Empty<JobType>());

			act.Should().Throw<ArgumentNullException>();
		}

		[TestMethod]
		public void OnCount_WithNullFilter_ShouldThrowArgumentNullException()
		{
			Action act = () => _middleware.OnCount((Skyline.DataMiner.Net.Messages.SLDataGateway.FilterElement<JobType>)null!, f => 0L);

			act.Should().Throw<ArgumentNullException>();
		}

		#endregion
	}
}
