namespace SDM.PlanAndBuild.Tests.JobTests
{
	using System;
	using System.Linq;

	using FluentAssertions;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using SDM.PlanAndBuild.Tests.Setup;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.SDM.PlanAndBuild.Helpers;
	using Skyline.DataMiner.SDM.PlanAndBuild.Models;
	using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Exceptions;

	using Statuses = SharedMappers.DomIds.SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum;

	/// <summary>
	/// Integration-level tests for the <see cref="IPlanAndBuildJobRepository"/> state transition methods
	/// (<c>TransitionTo</c>, <c>UpdateAndTransitionTo</c>, <c>TransitionAndUpdate</c>), exercised against a mocked
	/// DOM engine with a real <see cref="Skyline.DataMiner.Net.Apps.DataMinerObjectModel.DomBehaviorDefinition"/>
	/// registered (see <see cref="JobBehaviorFixture"/>), so that <c>DoStatusTransition</c> calls are actually
	/// validated by the engine rather than only by <see cref="SharedCommonLibrary.PlanAndBuild.State_Management.StateMachine"/>.
	/// </summary>
	[TestClass]
	public class PlanAndBuildJobDomRepositoryTransitionTests
	{
		private IPlanAndBuildApiHelper _helper;

		[TestInitialize]
		public void TestInitialize()
		{
			_helper = RepositoryInitialize.InitializeWithJobBehavior();
		}

		private JobType _sharedJobType;

		private PlanAndBuildJob CreateJobAt(Statuses status)
		{
			return CreateJobAt(status, "TestJob");
		}

		private PlanAndBuildJob CreateJobAt(Statuses status, string jobName)
		{
			// Reuse a single JobType across calls within a test so multiple CreateJobAt calls in the same test
			// don't collide with JobTypeValidator's name-uniqueness check.
			_sharedJobType ??= _helper.JobTypes.Create(new JobType { Name = "TestType" });
			return _helper.Jobs.Create(new PlanAndBuildJob { JobName = jobName, JobType = _sharedJobType, State = status });
		}

		#region TransitionTo - valid single-hop and multi-hop transitions

		[DataTestMethod]
		[DataRow(Statuses.New, Statuses.Assigned, DisplayName = "New -> Assigned")]
		[DataRow(Statuses.New, Statuses.Active, DisplayName = "New -> Active (composite)")]
		[DataRow(Statuses.New, Statuses.Review, DisplayName = "New -> Review (composite)")]
		[DataRow(Statuses.New, Statuses.Resolved, DisplayName = "New -> Resolved (composite)")]
		[DataRow(Statuses.New, Statuses.Canceled, DisplayName = "New -> Canceled")]
		[DataRow(Statuses.Assigned, Statuses.Active, DisplayName = "Assigned -> Active")]
		[DataRow(Statuses.Assigned, Statuses.Review, DisplayName = "Assigned -> Review (composite)")]
		[DataRow(Statuses.Assigned, Statuses.Resolved, DisplayName = "Assigned -> Resolved (composite)")]
		[DataRow(Statuses.Assigned, Statuses.Canceled, DisplayName = "Assigned -> Canceled")]
		[DataRow(Statuses.Active, Statuses.Review, DisplayName = "Active -> Review")]
		[DataRow(Statuses.Active, Statuses.Resolved, DisplayName = "Active -> Resolved (composite)")]
		[DataRow(Statuses.Active, Statuses.Canceled, DisplayName = "Active -> Canceled")]
		[DataRow(Statuses.Review, Statuses.Resolved, DisplayName = "Review -> Resolved")]
		public void TransitionTo_ValidPath_ShouldUpdateEngineAndReturnedState(Statuses from, Statuses to)
		{
			var job = CreateJobAt(from);

			var result = _helper.Jobs.TransitionTo(job, to);

			result.State.Should().Be(to);

			// Re-read from the repository to confirm the engine-level state actually persisted, not just the
			// in-memory object returned by TransitionTo.
			var reread = _helper.Jobs.Read(PlanAndBuildJobExposers.Identifier.Equal(result.Identifier)).Single();
			reread.State.Should().Be(to);
		}

		#endregion

		#region TransitionTo - invalid (unreachable / backwards / terminal) transitions

		[DataTestMethod]
		[DataRow(Statuses.Assigned, Statuses.New, DisplayName = "Assigned -> New (backwards)")]
		[DataRow(Statuses.Active, Statuses.New, DisplayName = "Active -> New (backwards)")]
		[DataRow(Statuses.Active, Statuses.Assigned, DisplayName = "Active -> Assigned (backwards)")]
		[DataRow(Statuses.Review, Statuses.New, DisplayName = "Review -> New (backwards)")]
		[DataRow(Statuses.Review, Statuses.Assigned, DisplayName = "Review -> Assigned (backwards)")]
		[DataRow(Statuses.Review, Statuses.Active, DisplayName = "Review -> Active (backwards)")]
		[DataRow(Statuses.Review, Statuses.Canceled, DisplayName = "Review -> Canceled (not allowed)")]
		[DataRow(Statuses.Canceled, Statuses.New, DisplayName = "Canceled -> New (terminal)")]
		[DataRow(Statuses.Canceled, Statuses.Assigned, DisplayName = "Canceled -> Assigned (terminal)")]
		[DataRow(Statuses.Canceled, Statuses.Active, DisplayName = "Canceled -> Active (terminal)")]
		[DataRow(Statuses.Canceled, Statuses.Review, DisplayName = "Canceled -> Review (terminal)")]
		[DataRow(Statuses.Canceled, Statuses.Resolved, DisplayName = "Canceled -> Resolved (terminal)")]
		[DataRow(Statuses.Resolved, Statuses.New, DisplayName = "Resolved -> New (terminal)")]
		[DataRow(Statuses.Resolved, Statuses.Assigned, DisplayName = "Resolved -> Assigned (terminal)")]
		[DataRow(Statuses.Resolved, Statuses.Active, DisplayName = "Resolved -> Active (terminal)")]
		[DataRow(Statuses.Resolved, Statuses.Review, DisplayName = "Resolved -> Review (terminal)")]
		[DataRow(Statuses.Resolved, Statuses.Canceled, DisplayName = "Resolved -> Canceled (terminal)")]
		public void TransitionTo_InvalidPath_ShouldThrowInvalidOperationException(Statuses from, Statuses to)
		{
			var job = CreateJobAt(from);

			Action act = () => _helper.Jobs.TransitionTo(job, to);

			act.Should().Throw<InvalidOperationException>()
				.WithMessage($"State transition from {from} to {to} is not allowed.");
		}

		[DataTestMethod]
		[DataRow(Statuses.New)]
		[DataRow(Statuses.Assigned)]
		[DataRow(Statuses.Active)]
		[DataRow(Statuses.Review)]
		[DataRow(Statuses.Canceled)]
		[DataRow(Statuses.Resolved)]
		public void TransitionTo_SameFromAndToStatus_ShouldThrowInvalidOperationException(Statuses status)
		{
			var job = CreateJobAt(status);

			Action act = () => _helper.Jobs.TransitionTo(job, status);

			act.Should().Throw<InvalidOperationException>();
		}

		#endregion

		#region TransitionTo - argument validation

		[TestMethod]
		public void TransitionTo_NullJob_ShouldThrowArgumentNullException()
		{
			Action act = () => _helper.Jobs.TransitionTo(null, Statuses.Assigned);

			act.Should().Throw<ArgumentNullException>();
		}

		#endregion

		#region UpdateAndTransitionTo - field update ordering

		[TestMethod]
		public void UpdateAndTransitionTo_ShouldPersistFieldUpdateBeforeTransitioning()
		{
			var job = CreateJobAt(Statuses.New);
			job.JobDescription = "Updated before transition";

			var result = _helper.Jobs.UpdateAndTransitionTo(job, Statuses.Assigned);

			result.State.Should().Be(Statuses.Assigned);
			result.JobDescription.Should().Be("Updated before transition");

			var reread = _helper.Jobs.Read(PlanAndBuildJobExposers.Identifier.Equal(result.Identifier)).Single();
			reread.State.Should().Be(Statuses.Assigned);
			reread.JobDescription.Should().Be("Updated before transition");
		}

		[TestMethod]
		public void UpdateAndTransitionTo_InvalidPath_ShouldThrowAndNotPersistFieldUpdate()
		{
			var job = CreateJobAt(Statuses.Canceled);
			job.JobDescription = "Should not persist";

			Action act = () => _helper.Jobs.UpdateAndTransitionTo(job, Statuses.New);

			act.Should().Throw<InvalidOperationException>();

			var reread = _helper.Jobs.Read(PlanAndBuildJobExposers.Identifier.Equal(job.Identifier)).Single();
			reread.JobDescription.Should().BeNull();
		}

		[TestMethod]
		public void UpdateAndTransitionTo_NullJob_ShouldThrowArgumentNullException()
		{
			Action act = () => _helper.Jobs.UpdateAndTransitionTo(null, Statuses.Assigned);

			act.Should().Throw<ArgumentNullException>();
		}

		#endregion

		#region TransitionAndUpdate - field update ordering

		[TestMethod]
		public void TransitionAndUpdate_ShouldTransitionThenPersistFieldUpdate()
		{
			var job = CreateJobAt(Statuses.New);
			job.JobDescription = "Updated after transition";

			var result = _helper.Jobs.TransitionAndUpdate(job, Statuses.Assigned);

			result.State.Should().Be(Statuses.Assigned);
			result.JobDescription.Should().Be("Updated after transition");

			var reread = _helper.Jobs.Read(PlanAndBuildJobExposers.Identifier.Equal(result.Identifier)).Single();
			reread.State.Should().Be(Statuses.Assigned);
			reread.JobDescription.Should().Be("Updated after transition");
		}

		[TestMethod]
		public void TransitionAndUpdate_InvalidPath_ShouldThrowAndNotPersistFieldUpdate()
		{
			var job = CreateJobAt(Statuses.Canceled);
			job.JobDescription = "Should not persist";

			Action act = () => _helper.Jobs.TransitionAndUpdate(job, Statuses.New);

			act.Should().Throw<InvalidOperationException>();

			var reread = _helper.Jobs.Read(PlanAndBuildJobExposers.Identifier.Equal(job.Identifier)).Single();
			reread.JobDescription.Should().BeNull();
		}

		[TestMethod]
		public void TransitionAndUpdate_NullJob_ShouldThrowArgumentNullException()
		{
			Action act = () => _helper.Jobs.TransitionAndUpdate(null, Statuses.Assigned);

			act.Should().Throw<ArgumentNullException>();
		}

		#endregion

		#region Composite path - engine-level intermediate hops

		[TestMethod]
		public void TransitionTo_NewToResolved_ShouldChainThroughAllIntermediateStatuses()
		{
			var job = CreateJobAt(Statuses.New);

			var result = _helper.Jobs.TransitionTo(job, Statuses.Resolved);

			result.State.Should().Be(Statuses.Resolved);

			// Once resolved (terminal), no further transition should be possible - confirms the engine
			// genuinely walked New -> Assigned -> Active -> Review -> Resolved rather than skipping validation.
			Action act = () => _helper.Jobs.TransitionTo(result, Statuses.Canceled);
			act.Should().Throw<InvalidOperationException>();
		}

		#endregion

		#region Corner cases - stale in-memory state vs. actual engine state

		[TestMethod]
		public void TransitionTo_StaleInMemoryState_SingleHopMismatch_ShouldThrowAndLeaveEngineUnchanged()
		{
			// Job is created at New, then advanced to Assigned "behind the back" of our in-memory reference
			// (simulating another caller/process moving it forward). Our local `job` variable still says New.
			var job = CreateJobAt(Statuses.New);
			_helper.Jobs.TransitionTo(_helper.Jobs.Read(PlanAndBuildJobExposers.Identifier.Equal(job.Identifier)).Single(), Statuses.Assigned);

			// job.State is still (stale) New, so IsTransitionAllowed(New, Active) succeeds and a 2-hop path is
			// computed, but the engine's actual FromStatusId is Assigned, not New - the first hop must fail.
			Action act = () => _helper.Jobs.TransitionTo(job, Statuses.Active);

			act.Should().Throw<InvalidOperationException>();

			// Confirm the engine is still exactly where the external transition left it (Assigned), not
			// corrupted or partially advanced by the failed attempt.
			var reread = _helper.Jobs.Read(PlanAndBuildJobExposers.Identifier.Equal(job.Identifier)).Single();
			reread.State.Should().Be(Statuses.Assigned);
		}

		[TestMethod]
		public void TransitionTo_StaleInMemoryState_MultiHopDrift_ShouldThrowOnFirstHopAndLeaveEngineUnchanged()
		{
			// Job is created at New, then advanced two hops (New -> Assigned -> Active) "behind the back" of
			// our in-memory reference, which still says New.
			var job = CreateJobAt(Statuses.New);
			var freshlyRead = _helper.Jobs.Read(PlanAndBuildJobExposers.Identifier.Equal(job.Identifier)).Single();
			_helper.Jobs.TransitionTo(freshlyRead, Statuses.Active);

			// job.State is still (stale) New, so a 4-hop path (New -> ... -> Resolved) is computed, but the
			// engine's actual FromStatusId is Active - the first hop (New_Assigned) must fail immediately.
			Action act = () => _helper.Jobs.TransitionTo(job, Statuses.Resolved);

			act.Should().Throw<InvalidOperationException>();

			var reread = _helper.Jobs.Read(PlanAndBuildJobExposers.Identifier.Equal(job.Identifier)).Single();
			reread.State.Should().Be(Statuses.Active);
		}

		#endregion

		#region Corner cases - identifier edge cases

		[TestMethod]
		public void TransitionTo_JobWithNullIdentifier_ShouldThrowInvalidOperationException()
		{
			// A job that was never Create()'d has no Identifier yet.
			var job = new PlanAndBuildJob { JobName = "NeverCreated", State = Statuses.New };

			Action act = () => _helper.Jobs.TransitionTo(job, Statuses.Assigned);

			act.Should().Throw<InvalidOperationException>();
		}

		[TestMethod]
		public void TransitionTo_JobWithNonExistentIdentifier_ShouldThrowInvalidOperationException()
		{
			// Well-formed GUID, but no DOM instance was ever created for it (e.g. deleted, or fabricated).
			var job = new PlanAndBuildJob { Identifier = Guid.NewGuid().ToString(), JobName = "Ghost", State = Statuses.New };

			Action act = () => _helper.Jobs.TransitionTo(job, Statuses.Assigned);

			act.Should().Throw<InvalidOperationException>();
		}

		#endregion

		#region Corner cases - sequential reuse of the same in-memory object

		[TestMethod]
		public void TransitionTo_ChainedCallsOnSameReturnedObject_ShouldSucceedForEachHop()
		{
			// ExecuteStateTransition mutates job.State in place and returns the same reference. Verify chaining
			// TransitionTo calls directly on that returned object (without re-reading from the repository in
			// between) works correctly for each subsequent hop.
			var job = CreateJobAt(Statuses.New);

			var afterFirst = _helper.Jobs.TransitionTo(job, Statuses.Assigned);
			afterFirst.State.Should().Be(Statuses.Assigned);

			var afterSecond = _helper.Jobs.TransitionTo(afterFirst, Statuses.Active);
			afterSecond.State.Should().Be(Statuses.Active);

			var afterThird = _helper.Jobs.TransitionTo(afterSecond, Statuses.Review);
			afterThird.State.Should().Be(Statuses.Review);

			var reread = _helper.Jobs.Read(PlanAndBuildJobExposers.Identifier.Equal(job.Identifier)).Single();
			reread.State.Should().Be(Statuses.Review);
		}

		#endregion

		#region Corner cases - field update validation scope during combined operations

		[TestMethod]
		public void TransitionAndUpdate_FieldUpdateWithDuplicateJobName_ShouldThrowAndNotPersistOrTransition()
		{
			// PlanAndBuildJobValidator enforces JobName uniqueness. TransitionAndUpdate calls the repository's own
			// internal Update() directly (not through the validation middleware that wraps _helper.Jobs), so the
			// repository's Validator (wired by PlanAndBuildApiHelper) must be the one enforcing this - confirming
			// the fix for the validation-bypass gap found earlier.
			var existing = CreateJobAt(Statuses.New);
			existing.JobName = "TakenName";
			_helper.Jobs.Update(existing);

			var job = CreateJobAt(Statuses.New);
			job.JobName = "TakenName";

			Action act = () => _helper.Jobs.TransitionAndUpdate(job, Statuses.Assigned);

			act.Should().Throw<ValidationException>();

			// Neither the field update nor the transition should have been applied.
			var reread = _helper.Jobs.Read(PlanAndBuildJobExposers.Identifier.Equal(job.Identifier)).Single();
			reread.JobName.Should().Be("TestJob");
			reread.State.Should().Be(Statuses.New);
		}

		[TestMethod]
		public void UpdateAndTransitionTo_FieldUpdateWithDuplicateJobName_ShouldThrowAndNotPersistOrTransition()
		{
			var existing = CreateJobAt(Statuses.New);
			existing.JobName = "TakenName2";
			_helper.Jobs.Update(existing);

			var job = CreateJobAt(Statuses.New);
			job.JobName = "TakenName2";

			Action act = () => _helper.Jobs.UpdateAndTransitionTo(job, Statuses.Assigned);

			act.Should().Throw<ValidationException>();

			var reread = _helper.Jobs.Read(PlanAndBuildJobExposers.Identifier.Equal(job.Identifier)).Single();
			reread.JobName.Should().Be("TestJob");
			reread.State.Should().Be(Statuses.New);
		}

		[TestMethod]
		public void PlanAndBuildJobDomRepository_WithNoValidatorWired_ShouldStillPerformCombinedOperations()
		{
			// Repositories constructed directly (bypassing PlanAndBuildApiHelper, e.g. in lower-level tests) have
			// no Validator wired at all. Confirms the null-conditional guard means this doesn't throw a
			// NullReferenceException and the combined operation still works, just without validation.
			var connection = ConnectionHelper.CreateConnectionWithJobBehavior();
			var rawRepository = new PlanAndBuildJobDomRepository(connection);

			var job = rawRepository.Create(new PlanAndBuildJob { JobName = "StandaloneJob", State = Statuses.New });
			job.JobDescription = "Updated with no validator wired";

			Action act = () => rawRepository.UpdateAndTransitionTo(job, Statuses.Assigned);

			act.Should().NotThrow();

			var reread = rawRepository.Read(PlanAndBuildJobExposers.Identifier.Equal(job.Identifier)).Single();
			reread.JobDescription.Should().Be("Updated with no validator wired");
			reread.State.Should().Be(Statuses.Assigned);
		}

		#endregion
	}
}
