namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    using System;

    using SharedCommonLibrary.FacilityManagement.State_Management;
    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.SDM;

    [AllowSdmMiddleware]
    public interface IRackRepository : IBulkRepository<Rack>
    {
        Rack TransitionTo(Rack rack, SlcFacility_Management.Behaviors.Rack_Behaviour.StatusesEnum newState);

        Rack UpdateAndTransitionTo(Rack rack, SlcFacility_Management.Behaviors.Rack_Behaviour.StatusesEnum newState);

        Rack TransitionAndUpdate(Rack rack, SlcFacility_Management.Behaviors.Rack_Behaviour.StatusesEnum newState);
    }

    internal partial class RackDomRepository : IRackRepository
    {
        public Rack TransitionTo(Rack rack, SlcFacility_Management.Behaviors.Rack_Behaviour.StatusesEnum newState)
        {
            if (rack == null) throw new ArgumentNullException(nameof(rack));

            if (!RackStateMachine.IsTransitionAllowed(rack.State, newState))
            {
                throw new InvalidOperationException($"State transition from {rack.State} to {newState} is not allowed.");
            }

            return ExecuteStateTransition(rack, newState);
        }

        public Rack UpdateAndTransitionTo(Rack rack, SlcFacility_Management.Behaviors.Rack_Behaviour.StatusesEnum newState)
        {
            if (rack == null) throw new ArgumentNullException(nameof(rack));

            if (!RackStateMachine.IsTransitionAllowed(rack.State, newState))
            {
                throw new InvalidOperationException($"State transition from {rack.State} to {newState} is not allowed.");
            }

            var updated = Update(rack);

            return ExecuteStateTransition(updated, newState);
        }

        public Rack TransitionAndUpdate(Rack rack, SlcFacility_Management.Behaviors.Rack_Behaviour.StatusesEnum newState)
        {
            if (rack == null) throw new ArgumentNullException(nameof(rack));

            if (!RackStateMachine.IsTransitionAllowed(rack.State, newState))
            {
                throw new InvalidOperationException($"State transition from {rack.State} to {newState} is not allowed.");
            }

            var transitioned = ExecuteStateTransition(rack, newState);

            return Update(transitioned);
        }

        private Rack ExecuteStateTransition(
            Rack rack,
            SlcFacility_Management.Behaviors.Rack_Behaviour.StatusesEnum toState)
        {
            if (rack == null) throw new ArgumentNullException(nameof(rack));

            try
            {
                var transitions = RackStateMachine.GetTransitionPath(rack.State, toState);

                if (transitions.Count == 0)
                {
                    throw new InvalidOperationException($"No valid transition path found from {rack.State} to {toState}.");
                }

                var instanceId = new DomInstanceId(Guid.Parse(rack.Identifier))
                {
                    ModuleId = RackDomMapper.ModuleId
                };

                DomInstance currentInstance = null;
                foreach (var transitionId in transitions)
                {
                    currentInstance = helper.DomInstances.DoStatusTransition(instanceId, SlcFacility_Management.Behaviors.Rack_Behaviour.Transitions.ToValue(transitionId));
                }

                if (currentInstance == null)
                {
                    throw new InvalidOperationException($"State transition failed for rack '{rack.Identifier}' to {toState}.");
                }

                rack.State = SlcFacility_Management.Behaviors.Rack_Behaviour.Statuses.ToEnum(currentInstance.StatusId);
                return rack;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to transition rack '{rack.Identifier}' from {rack.State} to {toState}: {ex.Message}",
                    ex);
            }
        }
    }
}
