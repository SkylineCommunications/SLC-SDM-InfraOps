namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    using System;

    using SharedCommonLibrary.FacilityManagement.State_Management;
    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.SDM;

    [AllowSdmMiddleware]
    public interface IRoomRepository : IBulkRepository<Room>
    {
        Room TransitionTo(Room room, SlcFacility_Management.Behaviors.Room_Behaviour.StatusesEnum newState);

        Room UpdateAndTransitionTo(Room room, SlcFacility_Management.Behaviors.Room_Behaviour.StatusesEnum newState);

        Room TransitionAndUpdate(Room room, SlcFacility_Management.Behaviors.Room_Behaviour.StatusesEnum newState);
    }

    internal partial class RoomDomRepository : IRoomRepository
    {
        public Room TransitionTo(Room room, SlcFacility_Management.Behaviors.Room_Behaviour.StatusesEnum newState)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));

            if (!RoomStateMachine.IsTransitionAllowed(room.State, newState))
            {
                throw new InvalidOperationException($"State transition from {room.State} to {newState} is not allowed.");
            }

            return ExecuteStateTransition(room, newState);
        }

        public Room UpdateAndTransitionTo(Room room, SlcFacility_Management.Behaviors.Room_Behaviour.StatusesEnum newState)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));

            if (!RoomStateMachine.IsTransitionAllowed(room.State, newState))
            {
                throw new InvalidOperationException($"State transition from {room.State} to {newState} is not allowed.");
            }

            var updated = Update(room);

            return ExecuteStateTransition(updated, newState);
        }

        public Room TransitionAndUpdate(Room room, SlcFacility_Management.Behaviors.Room_Behaviour.StatusesEnum newState)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));

            if (!RoomStateMachine.IsTransitionAllowed(room.State, newState))
            {
                throw new InvalidOperationException($"State transition from {room.State} to {newState} is not allowed.");
            }

            var transitioned = ExecuteStateTransition(room, newState);

            return Update(transitioned);
        }

        private Room ExecuteStateTransition(
            Room room,
            SlcFacility_Management.Behaviors.Room_Behaviour.StatusesEnum toState)
        {
            if (room == null) throw new ArgumentNullException(nameof(room));

            try
            {
                var transitions = RoomStateMachine.GetTransitionPath(room.State, toState);

                if (transitions.Count == 0)
                {
                    throw new InvalidOperationException($"No valid transition path found from {room.State} to {toState}.");
                }

                var instanceId = new DomInstanceId(Guid.Parse(room.Identifier))
                {
                    ModuleId = RoomDomMapper.ModuleId
                };

                DomInstance currentInstance = null;
                foreach (var transitionId in transitions)
                {
                    currentInstance = helper.DomInstances.DoStatusTransition(instanceId, SlcFacility_Management.Behaviors.Room_Behaviour.Transitions.ToValue(transitionId));
                }

                if (currentInstance == null)
                {
                    throw new InvalidOperationException($"State transition failed for room '{room.Identifier}' to {toState}.");
                }

                room.State = SlcFacility_Management.Behaviors.Room_Behaviour.Statuses.ToEnum(currentInstance.StatusId);
                return room;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to transition room '{room.Identifier}' from {room.State} to {toState}: {ex.Message}",
                    ex);
            }
        }
    }
}
