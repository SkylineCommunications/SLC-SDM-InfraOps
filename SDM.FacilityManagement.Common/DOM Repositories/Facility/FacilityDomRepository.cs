namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    using System;

    using SharedCommonLibrary.FacilityManagement.State_Management;
    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.SDM;

    [AllowSdmMiddleware]
    public interface IFacilityRepository : IBulkRepository<Facility>
    {
        Facility TransitionTo(Facility facility, SlcFacility_Management.Behaviors.Facility_Behaviour.StatusesEnum newState);

        Facility UpdateAndTransitionTo(Facility facility, SlcFacility_Management.Behaviors.Facility_Behaviour.StatusesEnum newState);

        Facility TransitionAndUpdate(Facility facility, SlcFacility_Management.Behaviors.Facility_Behaviour.StatusesEnum newState);
    }

    internal partial class FacilityDomRepository : IFacilityRepository
    {
        public Facility TransitionTo(Facility facility, SlcFacility_Management.Behaviors.Facility_Behaviour.StatusesEnum newState)
        {
            if (facility == null) throw new ArgumentNullException(nameof(facility));

            if (!FacilityStateMachine.IsTransitionAllowed(facility.State, newState))
            {
                throw new InvalidOperationException($"State transition from {facility.State} to {newState} is not allowed.");
            }

            return ExecuteStateTransition(facility, newState);
        }

        public Facility UpdateAndTransitionTo(Facility facility, SlcFacility_Management.Behaviors.Facility_Behaviour.StatusesEnum newState)
        {
            if (facility == null) throw new ArgumentNullException(nameof(facility));

            if (!FacilityStateMachine.IsTransitionAllowed(facility.State, newState))
            {
                throw new InvalidOperationException($"State transition from {facility.State} to {newState} is not allowed.");
            }

            var updated = Update(facility);

            return ExecuteStateTransition(updated, newState);
        }

        public Facility TransitionAndUpdate(Facility facility, SlcFacility_Management.Behaviors.Facility_Behaviour.StatusesEnum newState)
        {
            if (facility == null) throw new ArgumentNullException(nameof(facility));

            if (!FacilityStateMachine.IsTransitionAllowed(facility.State, newState))
            {
                throw new InvalidOperationException($"State transition from {facility.State} to {newState} is not allowed.");
            }

            var transitioned = ExecuteStateTransition(facility, newState);

            return Update(transitioned);
        }

        private Facility ExecuteStateTransition(
            Facility facility,
            SlcFacility_Management.Behaviors.Facility_Behaviour.StatusesEnum toState)
        {
            if (facility == null) throw new ArgumentNullException(nameof(facility));

            try
            {
                var transitions = FacilityStateMachine.GetTransitionPath(facility.State, toState);

                if (transitions.Count == 0)
                {
                    throw new InvalidOperationException($"No valid transition path found from {facility.State} to {toState}.");
                }

                var instanceId = new DomInstanceId(Guid.Parse(facility.Identifier))
                {
                    ModuleId = FacilityDomMapper.ModuleId
                };

                DomInstance currentInstance = null;
                foreach (var transitionId in transitions)
                {
                    currentInstance = helper.DomInstances.DoStatusTransition(instanceId, SlcFacility_Management.Behaviors.Facility_Behaviour.Transitions.ToValue(transitionId));
                }

                if (currentInstance == null)
                {
                    throw new InvalidOperationException($"State transition failed for facility '{facility.Identifier}' to {toState}.");
                }

                facility.State = SlcFacility_Management.Behaviors.Facility_Behaviour.Statuses.ToEnum(currentInstance.StatusId);
                return facility;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to transition facility '{facility.Identifier}' from {facility.State} to {toState}: {ex.Message}",
                    ex);
            }
        }
    }
}
