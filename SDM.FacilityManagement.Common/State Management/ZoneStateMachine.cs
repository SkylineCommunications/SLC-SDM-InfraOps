namespace SharedCommonLibrary.FacilityManagement.State_Management
{
    using System.Collections.Generic;

    using SharedMappers.DomIds;

    internal static class ZoneStateMachine
    {
        private static readonly IDictionary<(SlcFacility_Management.Behaviors.Zone_Behaviour.StatusesEnum startStatus, SlcFacility_Management.Behaviors.Zone_Behaviour.StatusesEnum endStatus), List<SlcFacility_Management.Behaviors.Zone_Behaviour.TransitionsEnum>> ZoneStatusToStatusTransitions = new Dictionary<(SlcFacility_Management.Behaviors.Zone_Behaviour.StatusesEnum startStatus, SlcFacility_Management.Behaviors.Zone_Behaviour.StatusesEnum endStatus), List<SlcFacility_Management.Behaviors.Zone_Behaviour.TransitionsEnum>>
        {
            [(SlcFacility_Management.Behaviors.Zone_Behaviour.StatusesEnum.Draft, SlcFacility_Management.Behaviors.Zone_Behaviour.StatusesEnum.Active)] = new List<SlcFacility_Management.Behaviors.Zone_Behaviour.TransitionsEnum> { SlcFacility_Management.Behaviors.Zone_Behaviour.TransitionsEnum.Draft_Active },
            [(SlcFacility_Management.Behaviors.Zone_Behaviour.StatusesEnum.Active, SlcFacility_Management.Behaviors.Zone_Behaviour.StatusesEnum.Deprecated)] = new List<SlcFacility_Management.Behaviors.Zone_Behaviour.TransitionsEnum> { SlcFacility_Management.Behaviors.Zone_Behaviour.TransitionsEnum.Active_Deprecated },
        };

        public static bool IsTransitionAllowed(SlcFacility_Management.Behaviors.Zone_Behaviour.StatusesEnum fromStatus, SlcFacility_Management.Behaviors.Zone_Behaviour.StatusesEnum toStatus)
        {
            return ZoneStatusToStatusTransitions.ContainsKey((fromStatus, toStatus));
        }

        public static List<SlcFacility_Management.Behaviors.Zone_Behaviour.TransitionsEnum> GetTransitionPath(SlcFacility_Management.Behaviors.Zone_Behaviour.StatusesEnum fromStatus, SlcFacility_Management.Behaviors.Zone_Behaviour.StatusesEnum toStatus)
        {
            if (ZoneStatusToStatusTransitions.TryGetValue((fromStatus, toStatus), out var transitions))
            {
                return new List<SlcFacility_Management.Behaviors.Zone_Behaviour.TransitionsEnum>(transitions);
            }

            return new List<SlcFacility_Management.Behaviors.Zone_Behaviour.TransitionsEnum>();
        }
    }
}
