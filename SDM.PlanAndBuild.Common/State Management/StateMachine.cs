namespace SharedCommonLibrary.PlanAndBuild.State_Management
{
    using System.Collections.Generic;

    using SharedMappers.DomIds;

    /// <summary>
    /// Defines the allowed status transitions for a <see cref="Skyline.DataMiner.SDM.PlanAndBuild.Models.PlanAndBuildJob"/>
    /// and the ordered list of behavior transitions required to move between any two reachable statuses.
    /// Mirrors the status graph defined on the "(slc)plan_and_build" Job DOM behavior.
    /// </summary>
    internal static class StateMachine
    {
        private static readonly IDictionary<(SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum startStatus, SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum endStatus), List<SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum>> JobStatusToStatusTransitions = new Dictionary<(SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum startStatus, SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum endStatus), List<SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum>>
        {
            #region New To

            [(SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum.New, SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum.Assigned)] = new List<SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum> { SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum.New_Assigned },
            [(SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum.New, SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum.Active)] = new List<SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum>
            {
                SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum.New_Assigned,
                SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum.Assigned_Active,
            },
            [(SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum.New, SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum.Review)] = new List<SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum>
            {
                SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum.New_Assigned,
                SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum.Assigned_Active,
                SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum.Active_Review,
            },
            [(SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum.New, SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum.Resolved)] = new List<SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum>
            {
                SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum.New_Assigned,
                SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum.Assigned_Active,
                SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum.Active_Review,
                SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum.Review_Resolved,
            },
            [(SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum.New, SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum.Canceled)] = new List<SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum> { SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum.New_Canceled },

            #endregion

            #region Assigned To

            [(SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum.Assigned, SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum.Active)] = new List<SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum> { SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum.Assigned_Active },
            [(SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum.Assigned, SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum.Review)] = new List<SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum>
            {
                SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum.Assigned_Active,
                SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum.Active_Review,
            },
            [(SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum.Assigned, SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum.Resolved)] = new List<SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum>
            {
                SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum.Assigned_Active,
                SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum.Active_Review,
                SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum.Review_Resolved,
            },
            [(SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum.Assigned, SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum.Canceled)] = new List<SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum> { SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum.Assigned_Canceled },

            #endregion

            #region Active To

            [(SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum.Active, SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum.Review)] = new List<SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum> { SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum.Active_Review },
            [(SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum.Active, SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum.Resolved)] = new List<SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum>
            {
                SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum.Active_Review,
                SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum.Review_Resolved,
            },
            [(SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum.Active, SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum.Canceled)] = new List<SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum> { SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum.Active_Canceled },

            #endregion

            #region Review To

            [(SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum.Review, SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum.Resolved)] = new List<SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum> { SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum.Review_Resolved },

            #endregion

            // Canceled and Resolved are terminal statuses - no outgoing transitions.
        };

        /// <summary>
        /// Checks if a state transition from the specified start status to end status is allowed.
        /// </summary>
        /// <param name="fromStatus">The starting status.</param>
        /// <param name="toStatus">The target status.</param>
        /// <returns>True if the transition is allowed; otherwise, false.</returns>
        public static bool IsTransitionAllowed(SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum fromStatus, SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum toStatus)
        {
            return JobStatusToStatusTransitions.ContainsKey((fromStatus, toStatus));
        }

        /// <summary>
        /// Gets the required transition path (list of transition steps) to move from one status to another.
        /// </summary>
        /// <param name="fromStatus">The starting status.</param>
        /// <param name="toStatus">The target status.</param>
        /// <returns>A list of transitions required to reach the target status, or an empty list if no valid path exists.</returns>
        public static List<SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum> GetTransitionPath(SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum fromStatus, SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum toStatus)
        {
            if (JobStatusToStatusTransitions.TryGetValue((fromStatus, toStatus), out var transitions))
            {
                return new List<SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum>(transitions);
            }

            return new List<SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum>();
        }
    }
}
