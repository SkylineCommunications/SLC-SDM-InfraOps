namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    using System;

    using SharedCommonLibrary.FacilityManagement.State_Management;
    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.SDM;

    [AllowSdmMiddleware]
    public interface IDeskRepository : IBulkRepository<Desk>
    {
        Desk TransitionTo(Desk desk, SlcFacility_Management.Behaviors.Desk_Behaviour.StatusesEnum newState);

        Desk UpdateAndTransitionTo(Desk desk, SlcFacility_Management.Behaviors.Desk_Behaviour.StatusesEnum newState);

        Desk TransitionAndUpdate(Desk desk, SlcFacility_Management.Behaviors.Desk_Behaviour.StatusesEnum newState);
    }

    internal partial class DeskDomRepository : IDeskRepository
    {
        public Desk TransitionTo(Desk desk, SlcFacility_Management.Behaviors.Desk_Behaviour.StatusesEnum newState)
        {
            if (desk == null) throw new ArgumentNullException(nameof(desk));

            if (!DeskStateMachine.IsTransitionAllowed(desk.State, newState))
            {
                throw new InvalidOperationException($"State transition from {desk.State} to {newState} is not allowed.");
            }

            return ExecuteStateTransition(desk, newState);
        }

        public Desk UpdateAndTransitionTo(Desk desk, SlcFacility_Management.Behaviors.Desk_Behaviour.StatusesEnum newState)
        {
            if (desk == null) throw new ArgumentNullException(nameof(desk));

            if (!DeskStateMachine.IsTransitionAllowed(desk.State, newState))
            {
                throw new InvalidOperationException($"State transition from {desk.State} to {newState} is not allowed.");
            }

            var updated = Update(desk);

            return ExecuteStateTransition(updated, newState);
        }

        public Desk TransitionAndUpdate(Desk desk, SlcFacility_Management.Behaviors.Desk_Behaviour.StatusesEnum newState)
        {
            if (desk == null) throw new ArgumentNullException(nameof(desk));

            if (!DeskStateMachine.IsTransitionAllowed(desk.State, newState))
            {
                throw new InvalidOperationException($"State transition from {desk.State} to {newState} is not allowed.");
            }

            var transitioned = ExecuteStateTransition(desk, newState);

            return Update(transitioned);
        }

        private Desk ExecuteStateTransition(
            Desk desk,
            SlcFacility_Management.Behaviors.Desk_Behaviour.StatusesEnum toState)
        {
            if (desk == null) throw new ArgumentNullException(nameof(desk));

            try
            {
                var transitions = DeskStateMachine.GetTransitionPath(desk.State, toState);

                if (transitions.Count == 0)
                {
                    throw new InvalidOperationException($"No valid transition path found from {desk.State} to {toState}.");
                }

                var instanceId = new DomInstanceId(Guid.Parse(desk.Identifier))
                {
                    ModuleId = DeskDomMapper.ModuleId
                };

                DomInstance currentInstance = null;
                foreach (var transitionId in transitions)
                {
                    currentInstance = helper.DomInstances.DoStatusTransition(instanceId, SlcFacility_Management.Behaviors.Desk_Behaviour.Transitions.ToValue(transitionId));
                }

                if (currentInstance == null)
                {
                    throw new InvalidOperationException($"State transition failed for desk '{desk.Identifier}' to {toState}.");
                }

                desk.State = SlcFacility_Management.Behaviors.Desk_Behaviour.Statuses.ToEnum(currentInstance.StatusId);
                return desk;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to transition desk '{desk.Identifier}' from {desk.State} to {toState}: {ex.Message}",
                    ex);
            }
        }
    }
}
