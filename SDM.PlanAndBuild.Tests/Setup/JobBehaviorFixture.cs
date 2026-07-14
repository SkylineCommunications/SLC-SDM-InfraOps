namespace SDM.PlanAndBuild.Tests.Setup
{
	using System.Collections.Generic;

	using SharedMappers.DomIds;

	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel.Status;

	/// <summary>
	/// Builds the minimal <see cref="DomDefinition"/> and <see cref="DomBehaviorDefinition"/> objects required for
	/// the mocked DOM engine (<see cref="Skyline.DataMiner.Utils.DOM.UnitTesting.DomSLNetMessageHandler"/>) to
	/// resolve <c>DoStatusTransition</c> calls against <see cref="Skyline.DataMiner.SDM.PlanAndBuild.Models.PlanAndBuildJob"/>
	/// instances. Only what is strictly needed for status transitions is populated (no section/field wiring).
	/// </summary>
	internal static class JobBehaviorFixture
	{
		/// <summary>
		/// The module under which the Job definition/behavior are registered on the mocked message handler.
		/// </summary>
		internal const string ModuleId = "(slc)plan_and_build";

		/// <summary>
		/// Builds the <see cref="DomBehaviorDefinition"/> for the Job behavior, containing every status
		/// (<see cref="SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum"/>) and every transition
		/// (<see cref="SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum"/>) as defined in the real
		/// Plan &amp; Build module.
		/// </summary>
		internal static DomBehaviorDefinition BuildJobBehaviorDefinition()
		{
			return new DomBehaviorDefinition("Job_Behavior")
			{
				ID = SlcPlan_And_Build.Behaviors.Job_Behavior.Id,
				InitialStatusId = SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.New,
				Statuses = new List<DomStatus>
				{
					new DomStatus(SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.New, "New"),
					new DomStatus(SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.Assigned, "Assigned"),
					new DomStatus(SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.Active, "Active"),
					new DomStatus(SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.Review, "Review"),
					new DomStatus(SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.Resolved, "Resolved"),
					new DomStatus(SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.Canceled, "Canceled"),
				},
				StatusTransitions = new List<DomStatusTransition>
				{
					new DomStatusTransition(
						SlcPlan_And_Build.Behaviors.Job_Behavior.Transitions.New_Assigned,
						SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.New,
						SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.Assigned),
					new DomStatusTransition(
						SlcPlan_And_Build.Behaviors.Job_Behavior.Transitions.Assigned_Active,
						SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.Assigned,
						SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.Active),
					new DomStatusTransition(
						SlcPlan_And_Build.Behaviors.Job_Behavior.Transitions.Active_Review,
						SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.Active,
						SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.Review),
					new DomStatusTransition(
						SlcPlan_And_Build.Behaviors.Job_Behavior.Transitions.Review_Resolved,
						SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.Review,
						SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.Resolved),
					new DomStatusTransition(
						SlcPlan_And_Build.Behaviors.Job_Behavior.Transitions.New_Canceled,
						SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.New,
						SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.Canceled),
					new DomStatusTransition(
						SlcPlan_And_Build.Behaviors.Job_Behavior.Transitions.Assigned_Canceled,
						SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.Assigned,
						SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.Canceled),
					new DomStatusTransition(
						SlcPlan_And_Build.Behaviors.Job_Behavior.Transitions.Active_Canceled,
						SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.Active,
						SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.Canceled),
				},
			};
		}

		/// <summary>
		/// Builds the <see cref="DomDefinition"/> for the Job entity, linked to the Job behavior definition so the
		/// mocked message handler can resolve status transitions for job instances.
		/// </summary>
		internal static DomDefinition BuildJobDefinition()
		{
			return new DomDefinition("Job")
			{
				ID = Skyline.DataMiner.SDM.PlanAndBuild.Models.PlanAndBuildJobDomMapper.DomDefinitionId,
				DomBehaviorDefinitionId = SlcPlan_And_Build.Behaviors.Job_Behavior.Id,
			};
		}
	}
}
