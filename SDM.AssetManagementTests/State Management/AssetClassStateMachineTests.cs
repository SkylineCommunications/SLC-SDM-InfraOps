namespace SDM.AssetManagement.Tests.StateManagement
{
    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SharedCommonLibrary.AssetManagement.State_Management;

    using Statuses = SharedMappers.DomIds.SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum;
    using Transitions = SharedMappers.DomIds.SlcAsset_Management.Behaviors.Asset_Class_Behavior.TransitionsEnum;

    [TestClass]
    public class AssetClassStateMachineTests
    {
        [TestMethod]
        public void IsTransitionAllowed_ShouldAllowTheThreeForwardPaths()
        {
            StateMachine.IsTransitionAllowed(Statuses.Draft, Statuses.Active).Should().BeTrue();
            StateMachine.IsTransitionAllowed(Statuses.Active, Statuses.Deprecated).Should().BeTrue();
            StateMachine.IsTransitionAllowed(Statuses.Draft, Statuses.Deprecated).Should().BeTrue();
        }

        [TestMethod]
        public void GetTransitionPath_ShouldReturnTheExpectedOrderedTransitions()
        {
            StateMachine.GetTransitionPath(Statuses.Draft, Statuses.Active)
                .Should().Equal(Transitions.Draft_Active);

            StateMachine.GetTransitionPath(Statuses.Active, Statuses.Deprecated)
                .Should().Equal(Transitions.Active_Deprecated);

            StateMachine.GetTransitionPath(Statuses.Draft, Statuses.Deprecated)
                .Should().Equal(Transitions.Draft_Active, Transitions.Active_Deprecated);
        }

        [TestMethod]
        public void IsTransitionAllowed_ShouldRejectBackwardsAndSelfTransitions()
        {
            StateMachine.IsTransitionAllowed(Statuses.Active, Statuses.Draft).Should().BeFalse();
            StateMachine.IsTransitionAllowed(Statuses.Deprecated, Statuses.Active).Should().BeFalse();
            StateMachine.IsTransitionAllowed(Statuses.Draft, Statuses.Draft).Should().BeFalse();
            StateMachine.IsTransitionAllowed(Statuses.Deprecated, Statuses.Deprecated).Should().BeFalse();
        }

        [TestMethod]
        public void GetTransitionPath_ShouldReturnAnIndependentCopy()
        {
            var firstPath = StateMachine.GetTransitionPath(Statuses.Draft, Statuses.Deprecated);
            firstPath.Clear();

            StateMachine.GetTransitionPath(Statuses.Draft, Statuses.Deprecated)
                .Should().Equal(Transitions.Draft_Active, Transitions.Active_Deprecated);
        }
    }
}