namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    using System;

    using SharedCommonLibrary.FacilityManagement.State_Management;
    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.SDM;

    [AllowSdmMiddleware]
    public interface IFloorRepository : IBulkRepository<Floor>
    {
        Floor TransitionTo(Floor floor, SlcFacility_Management.Behaviors.Floor_Behaviour.StatusesEnum newState);

        Floor UpdateAndTransitionTo(Floor floor, SlcFacility_Management.Behaviors.Floor_Behaviour.StatusesEnum newState);

        Floor TransitionAndUpdate(Floor floor, SlcFacility_Management.Behaviors.Floor_Behaviour.StatusesEnum newState);
    }

    internal partial class FloorDomRepository : IFloorRepository
    {
        public Floor TransitionTo(Floor floor, SlcFacility_Management.Behaviors.Floor_Behaviour.StatusesEnum newState)
        {
            if (floor == null) throw new ArgumentNullException(nameof(floor));

            if (!FloorStateMachine.IsTransitionAllowed(floor.State, newState))
            {
                throw new InvalidOperationException($"State transition from {floor.State} to {newState} is not allowed.");
            }

            return ExecuteStateTransition(floor, newState);
        }

        public Floor UpdateAndTransitionTo(Floor floor, SlcFacility_Management.Behaviors.Floor_Behaviour.StatusesEnum newState)
        {
            if (floor == null) throw new ArgumentNullException(nameof(floor));

            if (!FloorStateMachine.IsTransitionAllowed(floor.State, newState))
            {
                throw new InvalidOperationException($"State transition from {floor.State} to {newState} is not allowed.");
            }

            var updated = Update(floor);

            return ExecuteStateTransition(updated, newState);
        }

        public Floor TransitionAndUpdate(Floor floor, SlcFacility_Management.Behaviors.Floor_Behaviour.StatusesEnum newState)
        {
            if (floor == null) throw new ArgumentNullException(nameof(floor));

            if (!FloorStateMachine.IsTransitionAllowed(floor.State, newState))
            {
                throw new InvalidOperationException($"State transition from {floor.State} to {newState} is not allowed.");
            }

            var transitioned = ExecuteStateTransition(floor, newState);

            return Update(transitioned);
        }

        private Floor ExecuteStateTransition(
            Floor floor,
            SlcFacility_Management.Behaviors.Floor_Behaviour.StatusesEnum toState)
        {
            if (floor == null) throw new ArgumentNullException(nameof(floor));

            try
            {
                var transitions = FloorStateMachine.GetTransitionPath(floor.State, toState);

                if (transitions.Count == 0)
                {
                    throw new InvalidOperationException($"No valid transition path found from {floor.State} to {toState}.");
                }

                var instanceId = new DomInstanceId(Guid.Parse(floor.Identifier))
                {
                    ModuleId = FloorDomMapper.ModuleId
                };

                DomInstance currentInstance = null;
                foreach (var transitionId in transitions)
                {
                    currentInstance = helper.DomInstances.DoStatusTransition(instanceId, SlcFacility_Management.Behaviors.Floor_Behaviour.Transitions.ToValue(transitionId));
                }

                if (currentInstance == null)
                {
                    throw new InvalidOperationException($"State transition failed for floor '{floor.Identifier}' to {toState}.");
                }

                floor.State = SlcFacility_Management.Behaviors.Floor_Behaviour.Statuses.ToEnum(currentInstance.StatusId);
                return floor;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to transition floor '{floor.Identifier}' from {floor.State} to {toState}: {ex.Message}",
                    ex);
            }
        }
    }
}
