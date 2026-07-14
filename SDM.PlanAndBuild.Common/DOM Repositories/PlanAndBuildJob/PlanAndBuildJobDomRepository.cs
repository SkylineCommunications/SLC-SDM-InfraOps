namespace Skyline.DataMiner.SDM.PlanAndBuild.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SharedCommonLibrary.PlanAndBuild.State_Management;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Extensions;

    /// <summary>
    /// Defines methods for managing <see cref="PlanAndBuildJob"/> state transitions in a repository. Extends bulk
    /// operations for jobs.
    /// </summary>
    /// <remarks>This interface provides operations to change a job's workflow state, supporting scenarios
    /// where field updates and state transitions must occur in a specific order. Implementations should ensure
    /// that combined operations are performed atomically to maintain data consistency. The interface is intended
    /// for use in Plan &amp; Build where jobs have lifecycle states and validation rules that may depend on the
    /// current state.</remarks>
    [AllowSdmMiddleware]
    public interface IPlanAndBuildJobRepository : IBulkRepository<PlanAndBuildJob>
    {
        /// <summary>
        /// Transitions the job to a new state.
        /// Use this AFTER updating fields if the new state has different validation rules.
        /// </summary>
        /// <param name="job">The job to transition.</param>
        /// <param name="newState">The new state to transition the job to.</param>
        PlanAndBuildJob TransitionTo(PlanAndBuildJob job, SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum newState);

        /// <summary>
        /// Updates fields and transitions state in a single atomic operation.
        /// Order: Fields are updated first, then state transition occurs.
        /// Use when you need to prepare the job for the new state.
        /// </summary>
        /// <param name="job">The job to update and transition.</param>
        /// <param name="newState">The new state to transition the job to.</param>
        PlanAndBuildJob UpdateAndTransitionTo(PlanAndBuildJob job, SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum newState);

        /// <summary>
        /// Transitions state first, then updates fields.
        /// Order: State transition occurs, then fields are updated.
        /// Use when the new state enables certain field changes.
        /// </summary>
        /// <param name="job">The job to transition and update.</param>
        /// <param name="newState">The new state to transition the job to.</param>
        PlanAndBuildJob TransitionAndUpdate(PlanAndBuildJob job, SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum newState);

        /// <summary>
        /// Reads all Jobs matching any of the given JobNames, using a single batched big-OR query (see
        /// <see cref="BulkRepositoryQueryExtensions.ReadByBigOrFilter{T, TKey}"/>) instead of one query per
        /// JobName. Use this for bulk JobName uniqueness checks instead of looping
        /// <see cref="ICountableRepository{PlanAndBuildJob}.Count(FilterElement{PlanAndBuildJob})"/> once per candidate Job.
        /// </summary>
        /// <param name="jobNames">The JobNames to look up. Duplicates are handled gracefully.</param>
        List<PlanAndBuildJob> GetByJobNames(IEnumerable<string> jobNames);

        /// <summary>
        /// Reads all Jobs referencing any of the given JobType identifiers, using a single batched big-OR query
        /// (see <see cref="BulkRepositoryQueryExtensions.ReadByBigOrFilter{T, TKey}"/>) instead of one query per
        /// JobType. Used by <see cref="Skyline.DataMiner.SDM.PlanAndBuild.Validation.JobTypeValidator"/> for bulk
        /// "JobType in use" checks instead of looping <see cref="ICountableRepository{PlanAndBuildJob}.Count(FilterElement{PlanAndBuildJob})"/> once
        /// per candidate JobType.
        /// </summary>
        /// <param name="jobTypeIdentifiers">The JobType identifiers to look up. Duplicates are handled gracefully.</param>
        List<PlanAndBuildJob> GetByJobTypes(IEnumerable<string> jobTypeIdentifiers);
    }

    internal partial class PlanAndBuildJobDomRepository : IPlanAndBuildJobRepository
    {
        /// <summary>
        /// Optional validator used to enforce business-rule validation (e.g. JobName uniqueness) on the field
        /// updates performed by <see cref="UpdateAndTransitionTo"/> and <see cref="TransitionAndUpdate"/>.
        /// </summary>
        /// <remarks>
        /// These methods call this repository's own internal <see cref="Update(PlanAndBuildJob)"/> directly, which
        /// bypasses <c>PlanAndBuildJobValidationMiddleware</c> (that middleware only wraps the decorator exposed
        /// through <see cref="Skyline.DataMiner.SDM.PlanAndBuild.Helpers.IPlanAndBuildApiHelper.Jobs"/>). Wiring
        /// this property (done by <see cref="Skyline.DataMiner.SDM.PlanAndBuild.Helpers.PlanAndBuildApiHelper"/>)
        /// restores validation for the combined transition+update operations. It is intentionally nullable so this
        /// repository remains usable standalone (e.g. in tests) without validation.
        /// </remarks>
        internal Skyline.DataMiner.SDM.PlanAndBuild.Validation.PlanAndBuildJobValidator Validator { get; set; }

        public PlanAndBuildJob TransitionTo(PlanAndBuildJob job, SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum newState)
        {
            if (job == null) throw new ArgumentNullException(nameof(job));

            if (!StateMachine.IsTransitionAllowed(job.State, newState))
            {
                throw new InvalidOperationException($"State transition from {job.State} to {newState} is not allowed.");
            }

            return ExecuteStateTransition(job, newState);
        }

        public PlanAndBuildJob UpdateAndTransitionTo(PlanAndBuildJob job, SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum newState)
        {
            if (job == null) throw new ArgumentNullException(nameof(job));

            if (!StateMachine.IsTransitionAllowed(job.State, newState))
            {
                throw new InvalidOperationException($"State transition from {job.State} to {newState} is not allowed.");
            }

            // Validate before mutating anything, so an invalid job is rejected atomically: neither the field
            // update nor the transition is applied.
            Validator?.ValidateAndThrow(job);

            var updated = Update(job);

            return ExecuteStateTransition(updated, newState);
        }

        /// <summary>
        /// Transitions state first, then updates fields.
        /// Order: State transition occurs, then fields are updated.
        /// Use when the new state enables certain field changes.
        /// </summary>
        public PlanAndBuildJob TransitionAndUpdate(PlanAndBuildJob job, SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum newState)
        {
            if (job == null) throw new ArgumentNullException(nameof(job));

            if (!StateMachine.IsTransitionAllowed(job.State, newState))
            {
                throw new InvalidOperationException($"State transition from {job.State} to {newState} is not allowed.");
            }

            // Validate before mutating anything, so an invalid job is rejected atomically: the transition is
            // never executed if the pending field values wouldn't pass validation.
            Validator?.ValidateAndThrow(job);

            var transitioned = ExecuteStateTransition(job, newState);

            return Update(transitioned);
        }

        public List<PlanAndBuildJob> GetByJobNames(IEnumerable<string> jobNames)
        {
            var keys = jobNames?.Distinct().ToList() ?? new List<string>();

            return this.ReadByBigOrFilter(keys, jobName => PlanAndBuildJobExposers.JobName.Equal(jobName));
        }

        public List<PlanAndBuildJob> GetByJobTypes(IEnumerable<string> jobTypeIdentifiers)
        {
            var keys = jobTypeIdentifiers?.Distinct().ToList() ?? new List<string>();

            return this.ReadByBigOrFilter(
                keys,
                jobTypeIdentifier => PlanAndBuildJobExposers.Type.Equal(new SdmObjectReference<JobType>(jobTypeIdentifier)));
        }

        private PlanAndBuildJob ExecuteStateTransition(
            PlanAndBuildJob job,
            SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum toState)
        {
            if (job == null) throw new ArgumentNullException(nameof(job));

            try
            {
                var transitions = StateMachine.GetTransitionPath(job.State, toState);

                if (transitions.Count == 0)
                {
                    throw new InvalidOperationException($"No valid transition path found from {job.State} to {toState}.");
                }

                var instanceId = new DomInstanceId(Guid.Parse(job.Identifier))
                {
                    ModuleId = PlanAndBuildJobDomMapper.ModuleId
                };

                DomInstance currentInstance = null;
                foreach (var transitionId in transitions)
                {
                    currentInstance = helper.DomInstances.DoStatusTransition(instanceId, SlcPlan_And_Build.Behaviors.Job_Behavior.Transitions.ToValue(transitionId));
                }

                if (currentInstance == null)
                {
                    throw new InvalidOperationException($"State transition failed for job '{job.Identifier}' to {toState}.");
                }

                // return back Job with updated state
                job.State = SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.ToEnum(currentInstance.StatusId);
                return job;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to transition job '{job.Identifier}' from {job.State} to {toState}: {ex.Message}",
                    ex);
            }
        }
    }
}
