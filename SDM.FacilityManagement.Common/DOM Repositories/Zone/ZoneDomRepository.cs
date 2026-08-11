namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    using System;

    using SharedCommonLibrary.FacilityManagement.State_Management;
    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.SDM;

    [AllowSdmMiddleware]
    public interface IZoneRepository : IBulkRepository<Zone>
    {
        Zone TransitionTo(Zone zone, SlcFacility_Management.Behaviors.Zone_Behaviour.StatusesEnum newState);

        Zone UpdateAndTransitionTo(Zone zone, SlcFacility_Management.Behaviors.Zone_Behaviour.StatusesEnum newState);

        Zone TransitionAndUpdate(Zone zone, SlcFacility_Management.Behaviors.Zone_Behaviour.StatusesEnum newState);
    }

    internal partial class ZoneDomRepository : IZoneRepository
    {
        public Zone TransitionTo(Zone zone, SlcFacility_Management.Behaviors.Zone_Behaviour.StatusesEnum newState)
        {
            if (zone == null) throw new ArgumentNullException(nameof(zone));

            if (!ZoneStateMachine.IsTransitionAllowed(zone.State, newState))
            {
                throw new InvalidOperationException($"State transition from {zone.State} to {newState} is not allowed.");
            }

            return ExecuteStateTransition(zone, newState);
        }

        public Zone UpdateAndTransitionTo(Zone zone, SlcFacility_Management.Behaviors.Zone_Behaviour.StatusesEnum newState)
        {
            if (zone == null) throw new ArgumentNullException(nameof(zone));

            if (!ZoneStateMachine.IsTransitionAllowed(zone.State, newState))
            {
                throw new InvalidOperationException($"State transition from {zone.State} to {newState} is not allowed.");
            }

            var updated = Update(zone);

            return ExecuteStateTransition(updated, newState);
        }

        public Zone TransitionAndUpdate(Zone zone, SlcFacility_Management.Behaviors.Zone_Behaviour.StatusesEnum newState)
        {
            if (zone == null) throw new ArgumentNullException(nameof(zone));

            if (!ZoneStateMachine.IsTransitionAllowed(zone.State, newState))
            {
                throw new InvalidOperationException($"State transition from {zone.State} to {newState} is not allowed.");
            }

            var transitioned = ExecuteStateTransition(zone, newState);

            return Update(transitioned);
        }

        private Zone ExecuteStateTransition(
            Zone zone,
            SlcFacility_Management.Behaviors.Zone_Behaviour.StatusesEnum toState)
        {
            if (zone == null) throw new ArgumentNullException(nameof(zone));

            try
            {
                var transitions = ZoneStateMachine.GetTransitionPath(zone.State, toState);

                if (transitions.Count == 0)
                {
                    throw new InvalidOperationException($"No valid transition path found from {zone.State} to {toState}.");
                }

                var instanceId = new DomInstanceId(Guid.Parse(zone.Identifier))
                {
                    ModuleId = ZoneDomMapper.ModuleId
                };

                DomInstance currentInstance = null;
                foreach (var transitionId in transitions)
                {
                    currentInstance = helper.DomInstances.DoStatusTransition(instanceId, SlcFacility_Management.Behaviors.Zone_Behaviour.Transitions.ToValue(transitionId));
                }

                if (currentInstance == null)
                {
                    throw new InvalidOperationException($"State transition failed for zone '{zone.Identifier}' to {toState}.");
                }

                zone.State = SlcFacility_Management.Behaviors.Zone_Behaviour.Statuses.ToEnum(currentInstance.StatusId);
                return zone;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to transition zone '{zone.Identifier}' from {zone.State} to {toState}: {ex.Message}",
                    ex);
            }
        }
    }
}
