namespace SharedCommonLibrary.FacilityManagement.State_Management
{
    using System.Collections.Generic;

    using SharedMappers.DomIds;

    internal static class RackStateMachine
    {
        private static readonly IDictionary<(SlcFacility_Management.Behaviors.Rack_Behaviour.StatusesEnum startStatus, SlcFacility_Management.Behaviors.Rack_Behaviour.StatusesEnum endStatus), List<SlcFacility_Management.Behaviors.Rack_Behaviour.TransitionsEnum>> RackStatusToStatusTransitions = new Dictionary<(SlcFacility_Management.Behaviors.Rack_Behaviour.StatusesEnum startStatus, SlcFacility_Management.Behaviors.Rack_Behaviour.StatusesEnum endStatus), List<SlcFacility_Management.Behaviors.Rack_Behaviour.TransitionsEnum>>
        {
            [(SlcFacility_Management.Behaviors.Rack_Behaviour.StatusesEnum.Draft, SlcFacility_Management.Behaviors.Rack_Behaviour.StatusesEnum.Active)] = new List<SlcFacility_Management.Behaviors.Rack_Behaviour.TransitionsEnum> { SlcFacility_Management.Behaviors.Rack_Behaviour.TransitionsEnum.Draft_Active },
            [(SlcFacility_Management.Behaviors.Rack_Behaviour.StatusesEnum.Active, SlcFacility_Management.Behaviors.Rack_Behaviour.StatusesEnum.Deprecated)] = new List<SlcFacility_Management.Behaviors.Rack_Behaviour.TransitionsEnum> { SlcFacility_Management.Behaviors.Rack_Behaviour.TransitionsEnum.Active_Deprecated },
        };

        public static bool IsTransitionAllowed(SlcFacility_Management.Behaviors.Rack_Behaviour.StatusesEnum fromStatus, SlcFacility_Management.Behaviors.Rack_Behaviour.StatusesEnum toStatus)
        {
            return RackStatusToStatusTransitions.ContainsKey((fromStatus, toStatus));
        }

        public static List<SlcFacility_Management.Behaviors.Rack_Behaviour.TransitionsEnum> GetTransitionPath(SlcFacility_Management.Behaviors.Rack_Behaviour.StatusesEnum fromStatus, SlcFacility_Management.Behaviors.Rack_Behaviour.StatusesEnum toStatus)
        {
            if (RackStatusToStatusTransitions.TryGetValue((fromStatus, toStatus), out var transitions))
            {
                return new List<SlcFacility_Management.Behaviors.Rack_Behaviour.TransitionsEnum>(transitions);
            }

            return new List<SlcFacility_Management.Behaviors.Rack_Behaviour.TransitionsEnum>();
        }
    }
}
