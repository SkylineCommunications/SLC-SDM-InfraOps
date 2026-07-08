namespace SDM.PlanAndBuild.Tests.JobTests
{
	using System.Collections.Generic;
	using System.Linq;

	using FluentAssertions;
	using FluentAssertions.Execution;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using SharedCommonLibrary.PlanAndBuild.State_Management;

	using SharedMappers.DomIds;

	using Statuses = SharedMappers.DomIds.SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum;
	using Transitions = SharedMappers.DomIds.SlcPlan_And_Build.Behaviors.Job_Behavior.TransitionsEnum;

	/// <summary>
	/// Tests for the <see cref="StateMachine"/> that governs allowed status transitions for a
	/// <see cref="Skyline.DataMiner.SDM.PlanAndBuild.Models.PlanAndBuildJob"/>.
	/// Status graph: New -> Assigned -> Active -> Review -> Resolved, with New/Assigned/Active able to
	/// short-circuit directly to Canceled.
	/// </summary>
	[TestClass]
	public class StateMachineTests
	{
		#region IsTransitionAllowed - valid single-hop and multi-hop transitions

		[DataTestMethod]
		[DataRow(Statuses.New, Statuses.Assigned, DisplayName = "New -> Assigned")]
		[DataRow(Statuses.New, Statuses.Active, DisplayName = "New -> Active")]
		[DataRow(Statuses.New, Statuses.Review, DisplayName = "New -> Review")]
		[DataRow(Statuses.New, Statuses.Resolved, DisplayName = "New -> Resolved")]
		[DataRow(Statuses.New, Statuses.Canceled, DisplayName = "New -> Canceled")]
		[DataRow(Statuses.Assigned, Statuses.Active, DisplayName = "Assigned -> Active")]
		[DataRow(Statuses.Assigned, Statuses.Review, DisplayName = "Assigned -> Review")]
		[DataRow(Statuses.Assigned, Statuses.Resolved, DisplayName = "Assigned -> Resolved")]
		[DataRow(Statuses.Assigned, Statuses.Canceled, DisplayName = "Assigned -> Canceled")]
		[DataRow(Statuses.Active, Statuses.Review, DisplayName = "Active -> Review")]
		[DataRow(Statuses.Active, Statuses.Resolved, DisplayName = "Active -> Resolved")]
		[DataRow(Statuses.Active, Statuses.Canceled, DisplayName = "Active -> Canceled")]
		[DataRow(Statuses.Review, Statuses.Resolved, DisplayName = "Review -> Resolved")]
		public void IsTransitionAllowed_WithValidPath_ShouldReturnTrue(Statuses from, Statuses to)
		{
			StateMachine.IsTransitionAllowed(from, to).Should().BeTrue();
		}

		#endregion

		#region IsTransitionAllowed - invalid (unreachable / backwards) transitions

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
		public void IsTransitionAllowed_WithInvalidPath_ShouldReturnFalse(Statuses from, Statuses to)
		{
			StateMachine.IsTransitionAllowed(from, to).Should().BeFalse();
		}

		#endregion

		#region IsTransitionAllowed - self-transitions

		[DataTestMethod]
		[DataRow(Statuses.New)]
		[DataRow(Statuses.Assigned)]
		[DataRow(Statuses.Active)]
		[DataRow(Statuses.Review)]
		[DataRow(Statuses.Canceled)]
		[DataRow(Statuses.Resolved)]
		public void IsTransitionAllowed_WithSameFromAndToStatus_ShouldReturnFalse(Statuses status)
		{
			StateMachine.IsTransitionAllowed(status, status).Should().BeFalse();
		}

		#endregion

		#region IsTransitionAllowed - terminal statuses have no outgoing transitions

		[DataTestMethod]
		[DataRow(Statuses.Canceled)]
		[DataRow(Statuses.Resolved)]
		public void IsTransitionAllowed_FromTerminalStatus_ShouldNeverAllowAnyTransition(Statuses terminalStatus)
		{
			var allStatuses = new[] { Statuses.New, Statuses.Assigned, Statuses.Active, Statuses.Canceled, Statuses.Review, Statuses.Resolved };

			using (new AssertionScope())
			{
				foreach (var target in allStatuses)
				{
					StateMachine.IsTransitionAllowed(terminalStatus, target).Should().BeFalse($"'{terminalStatus}' is terminal and should never transition to '{target}'");
				}
			}
		}

		#endregion

		#region GetTransitionPath - valid paths return the correct ordered transition steps

		[TestMethod]
		public void GetTransitionPath_NewToAssigned_ShouldReturnSingleStep()
		{
			var path = StateMachine.GetTransitionPath(Statuses.New, Statuses.Assigned);

			path.Should().Equal(Transitions.New_Assigned);
		}

		[TestMethod]
		public void GetTransitionPath_NewToActive_ShouldReturnTwoSteps()
		{
			var path = StateMachine.GetTransitionPath(Statuses.New, Statuses.Active);

			path.Should().Equal(Transitions.New_Assigned, Transitions.Assigned_Active);
		}

		[TestMethod]
		public void GetTransitionPath_NewToReview_ShouldReturnThreeSteps()
		{
			var path = StateMachine.GetTransitionPath(Statuses.New, Statuses.Review);

			path.Should().Equal(Transitions.New_Assigned, Transitions.Assigned_Active, Transitions.Active_Review);
		}

		[TestMethod]
		public void GetTransitionPath_NewToResolved_ShouldReturnFourSteps()
		{
			var path = StateMachine.GetTransitionPath(Statuses.New, Statuses.Resolved);

			path.Should().Equal(Transitions.New_Assigned, Transitions.Assigned_Active, Transitions.Active_Review, Transitions.Review_Resolved);
		}

		[TestMethod]
		public void GetTransitionPath_NewToCanceled_ShouldReturnSingleDirectStep()
		{
			var path = StateMachine.GetTransitionPath(Statuses.New, Statuses.Canceled);

			path.Should().Equal(Transitions.New_Canceled);
		}

		[TestMethod]
		public void GetTransitionPath_AssignedToActive_ShouldReturnSingleStep()
		{
			var path = StateMachine.GetTransitionPath(Statuses.Assigned, Statuses.Active);

			path.Should().Equal(Transitions.Assigned_Active);
		}

		[TestMethod]
		public void GetTransitionPath_AssignedToReview_ShouldReturnTwoSteps()
		{
			var path = StateMachine.GetTransitionPath(Statuses.Assigned, Statuses.Review);

			path.Should().Equal(Transitions.Assigned_Active, Transitions.Active_Review);
		}

		[TestMethod]
		public void GetTransitionPath_AssignedToResolved_ShouldReturnThreeSteps()
		{
			var path = StateMachine.GetTransitionPath(Statuses.Assigned, Statuses.Resolved);

			path.Should().Equal(Transitions.Assigned_Active, Transitions.Active_Review, Transitions.Review_Resolved);
		}

		[TestMethod]
		public void GetTransitionPath_AssignedToCanceled_ShouldReturnSingleDirectStep()
		{
			var path = StateMachine.GetTransitionPath(Statuses.Assigned, Statuses.Canceled);

			path.Should().Equal(Transitions.Assigned_Canceled);
		}

		[TestMethod]
		public void GetTransitionPath_ActiveToReview_ShouldReturnSingleStep()
		{
			var path = StateMachine.GetTransitionPath(Statuses.Active, Statuses.Review);

			path.Should().Equal(Transitions.Active_Review);
		}

		[TestMethod]
		public void GetTransitionPath_ActiveToResolved_ShouldReturnTwoSteps()
		{
			var path = StateMachine.GetTransitionPath(Statuses.Active, Statuses.Resolved);

			path.Should().Equal(Transitions.Active_Review, Transitions.Review_Resolved);
		}

		[TestMethod]
		public void GetTransitionPath_ActiveToCanceled_ShouldReturnSingleDirectStep()
		{
			var path = StateMachine.GetTransitionPath(Statuses.Active, Statuses.Canceled);

			path.Should().Equal(Transitions.Active_Canceled);
		}

		[TestMethod]
		public void GetTransitionPath_ReviewToResolved_ShouldReturnSingleStep()
		{
			var path = StateMachine.GetTransitionPath(Statuses.Review, Statuses.Resolved);

			path.Should().Equal(Transitions.Review_Resolved);
		}

		#endregion

		#region GetTransitionPath - invalid paths return an empty (never null) list

		[DataTestMethod]
		[DataRow(Statuses.Assigned, Statuses.New)]
		[DataRow(Statuses.Active, Statuses.New)]
		[DataRow(Statuses.Active, Statuses.Assigned)]
		[DataRow(Statuses.Review, Statuses.New)]
		[DataRow(Statuses.Review, Statuses.Assigned)]
		[DataRow(Statuses.Review, Statuses.Active)]
		[DataRow(Statuses.Review, Statuses.Canceled)]
		[DataRow(Statuses.Canceled, Statuses.New)]
		[DataRow(Statuses.Canceled, Statuses.Assigned)]
		[DataRow(Statuses.Canceled, Statuses.Active)]
		[DataRow(Statuses.Canceled, Statuses.Review)]
		[DataRow(Statuses.Canceled, Statuses.Resolved)]
		[DataRow(Statuses.Resolved, Statuses.New)]
		[DataRow(Statuses.Resolved, Statuses.Assigned)]
		[DataRow(Statuses.Resolved, Statuses.Active)]
		[DataRow(Statuses.Resolved, Statuses.Review)]
		[DataRow(Statuses.Resolved, Statuses.Canceled)]
		public void GetTransitionPath_WithInvalidPath_ShouldReturnEmptyList(Statuses from, Statuses to)
		{
			var path = StateMachine.GetTransitionPath(from, to);

			using (new AssertionScope())
			{
				path.Should().NotBeNull();
				path.Should().BeEmpty();
			}
		}

		[DataTestMethod]
		[DataRow(Statuses.New)]
		[DataRow(Statuses.Assigned)]
		[DataRow(Statuses.Active)]
		[DataRow(Statuses.Review)]
		[DataRow(Statuses.Canceled)]
		[DataRow(Statuses.Resolved)]
		public void GetTransitionPath_WithSameFromAndToStatus_ShouldReturnEmptyList(Statuses status)
		{
			var path = StateMachine.GetTransitionPath(status, status);

			path.Should().BeEmpty();
		}

		#endregion

		#region GetTransitionPath - defensive copy semantics

		[TestMethod]
		public void GetTransitionPath_ReturnedList_ShouldBeIndependentCopy()
		{
			var firstCall = StateMachine.GetTransitionPath(Statuses.New, Statuses.Resolved);
			firstCall.Add(Transitions.New_Canceled);

			var secondCall = StateMachine.GetTransitionPath(Statuses.New, Statuses.Resolved);

			using (new AssertionScope())
			{
				secondCall.Should().HaveCount(4, "mutating a previously returned list should not affect the internal state machine data");
				secondCall.Should().Equal(Transitions.New_Assigned, Transitions.Assigned_Active, Transitions.Active_Review, Transitions.Review_Resolved);
			}
		}

		[TestMethod]
		public void GetTransitionPath_CalledTwiceForSamePair_ShouldReturnDistinctListInstances()
		{
			var firstCall = StateMachine.GetTransitionPath(Statuses.Assigned, Statuses.Resolved);
			var secondCall = StateMachine.GetTransitionPath(Statuses.Assigned, Statuses.Resolved);

			using (new AssertionScope())
			{
				firstCall.Should().NotBeSameAs(secondCall);
				firstCall.Should().Equal(secondCall);
			}
		}

		#endregion

		#region Consistency between IsTransitionAllowed and GetTransitionPath

		[TestMethod]
		public void IsTransitionAllowed_ShouldBeConsistentWithGetTransitionPath_ForEveryStatusCombination()
		{
			var allStatuses = new[] { Statuses.New, Statuses.Assigned, Statuses.Active, Statuses.Canceled, Statuses.Review, Statuses.Resolved };

			using (new AssertionScope())
			{
				foreach (var from in allStatuses)
				{
					foreach (var to in allStatuses)
					{
						var isAllowed = StateMachine.IsTransitionAllowed(from, to);
						var path = StateMachine.GetTransitionPath(from, to);

						if (isAllowed)
						{
							path.Should().NotBeEmpty($"'{from}' -> '{to}' is reported as allowed, so a transition path should exist");
						}
						else
						{
							path.Should().BeEmpty($"'{from}' -> '{to}' is reported as not allowed, so no transition path should exist");
						}
					}
				}
			}
		}

		#endregion

		#region Completeness - total number of reachable (from, to) combinations

		[TestMethod]
		public void StateMachine_ShouldExposeExactlyThirteenValidStatusCombinations()
		{
			var allStatuses = new[] { Statuses.New, Statuses.Assigned, Statuses.Active, Statuses.Canceled, Statuses.Review, Statuses.Resolved };

			var validCombinations = allStatuses
				.SelectMany(from => allStatuses.Select(to => (from, to)))
				.Where(pair => pair.from != pair.to)
				.Where(pair => StateMachine.IsTransitionAllowed(pair.from, pair.to))
				.ToList();

			validCombinations.Should().HaveCount(13);
		}

		[TestMethod]
		public void StateMachine_EveryValidPath_ShouldOnlyContainTransitionsReachableFromStartStatus()
		{
			// The first transition in any path from a given status must be one of the direct
			// (single-hop) transitions defined for that status, guarding against a mismatched/typo'd path.
			var directFirstTransitionsByStatus = new Dictionary<Statuses, Transitions>
			{
				[Statuses.New] = Transitions.New_Assigned,
				[Statuses.Assigned] = Transitions.Assigned_Active,
				[Statuses.Active] = Transitions.Active_Review,
				[Statuses.Review] = Transitions.Review_Resolved,
			};

			var allStatuses = new[] { Statuses.New, Statuses.Assigned, Statuses.Active, Statuses.Canceled, Statuses.Review, Statuses.Resolved };

			using (new AssertionScope())
			{
				foreach (var from in allStatuses)
				{
					foreach (var to in allStatuses)
					{
						if (from == to)
						{
							continue;
						}

						var path = StateMachine.GetTransitionPath(from, to);
						if (path.Count == 0)
						{
							continue;
						}

						// Direct cancellation transitions bypass the "linear happy path" first step.
						bool isDirectCancellation = to == Statuses.Canceled;
						if (isDirectCancellation)
						{
							continue;
						}

						if (directFirstTransitionsByStatus.TryGetValue(from, out var expectedFirstTransition))
						{
							path.First().Should().Be(expectedFirstTransition, $"path from '{from}' to '{to}' should start with the direct transition out of '{from}'");
						}
					}
				}
			}
		}

		#endregion
	}
}
