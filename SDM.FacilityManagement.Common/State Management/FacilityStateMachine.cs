namespace SharedCommonLibrary.FacilityManagement.State_Management
{
    using System.Collections.Generic;

    using SharedMappers.DomIds;

    internal static class FacilityStateMachine
    {
        private static readonly IDictionary<(SlcFacility_Management.Behaviors.Facility_Behaviour.StatusesEnum startStatus, SlcFacility_Management.Behaviors.Facility_Behaviour.StatusesEnum endStatus), List<SlcFacility_Management.Behaviors.Facility_Behaviour.TransitionsEnum>> FacilityStatusToStatusTransitions = new Dictionary<(SlcFacility_Management.Behaviors.Facility_Behaviour.StatusesEnum startStatus, SlcFacility_Management.Behaviors.Facility_Behaviour.StatusesEnum endStatus), List<SlcFacility_Management.Behaviors.Facility_Behaviour.TransitionsEnum>>
        {
            [(SlcFacility_Management.Behaviors.Facility_Behaviour.StatusesEnum.Draft, SlcFacility_Management.Behaviors.Facility_Behaviour.StatusesEnum.Active)] = new List<SlcFacility_Management.Behaviors.Facility_Behaviour.TransitionsEnum> { SlcFacility_Management.Behaviors.Facility_Behaviour.TransitionsEnum.Draft_Active },
            [(SlcFacility_Management.Behaviors.Facility_Behaviour.StatusesEnum.Active, SlcFacility_Management.Behaviors.Facility_Behaviour.StatusesEnum.Deprecated)] = new List<SlcFacility_Management.Behaviors.Facility_Behaviour.TransitionsEnum> { SlcFacility_Management.Behaviors.Facility_Behaviour.TransitionsEnum.Active_Deprecated },
        };

        public static bool IsTransitionAllowed(SlcFacility_Management.Behaviors.Facility_Behaviour.StatusesEnum fromStatus, SlcFacility_Management.Behaviors.Facility_Behaviour.StatusesEnum toStatus)
        {
            return FacilityStatusToStatusTransitions.ContainsKey((fromStatus, toStatus));
        }

        public static List<SlcFacility_Management.Behaviors.Facility_Behaviour.TransitionsEnum> GetTransitionPath(SlcFacility_Management.Behaviors.Facility_Behaviour.StatusesEnum fromStatus, SlcFacility_Management.Behaviors.Facility_Behaviour.StatusesEnum toStatus)
        {
            if (FacilityStatusToStatusTransitions.TryGetValue((fromStatus, toStatus), out var transitions))
            {
                return new List<SlcFacility_Management.Behaviors.Facility_Behaviour.TransitionsEnum>(transitions);
            }

            return new List<SlcFacility_Management.Behaviors.Facility_Behaviour.TransitionsEnum>();
        }
    }
}
