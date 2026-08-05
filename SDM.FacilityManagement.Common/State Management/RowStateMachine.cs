namespace SharedCommonLibrary.FacilityManagement.State_Management
{
    using System.Collections.Generic;

    using SharedMappers.DomIds;

    internal static class RowStateMachine
    {
        private static readonly IDictionary<(SlcFacility_Management.Behaviors.Row_Behaviour.StatusesEnum startStatus, SlcFacility_Management.Behaviors.Row_Behaviour.StatusesEnum endStatus), List<SlcFacility_Management.Behaviors.Row_Behaviour.TransitionsEnum>> RowStatusToStatusTransitions = new Dictionary<(SlcFacility_Management.Behaviors.Row_Behaviour.StatusesEnum startStatus, SlcFacility_Management.Behaviors.Row_Behaviour.StatusesEnum endStatus), List<SlcFacility_Management.Behaviors.Row_Behaviour.TransitionsEnum>>
        {
            [(SlcFacility_Management.Behaviors.Row_Behaviour.StatusesEnum.Draft, SlcFacility_Management.Behaviors.Row_Behaviour.StatusesEnum.Active)] = new List<SlcFacility_Management.Behaviors.Row_Behaviour.TransitionsEnum> { SlcFacility_Management.Behaviors.Row_Behaviour.TransitionsEnum.Draft_Active },
            [(SlcFacility_Management.Behaviors.Row_Behaviour.StatusesEnum.Active, SlcFacility_Management.Behaviors.Row_Behaviour.StatusesEnum.Deprecated)] = new List<SlcFacility_Management.Behaviors.Row_Behaviour.TransitionsEnum> { SlcFacility_Management.Behaviors.Row_Behaviour.TransitionsEnum.Active_Deprecated },
        };

        public static bool IsTransitionAllowed(SlcFacility_Management.Behaviors.Row_Behaviour.StatusesEnum fromStatus, SlcFacility_Management.Behaviors.Row_Behaviour.StatusesEnum toStatus)
        {
            return RowStatusToStatusTransitions.ContainsKey((fromStatus, toStatus));
        }

        public static List<SlcFacility_Management.Behaviors.Row_Behaviour.TransitionsEnum> GetTransitionPath(SlcFacility_Management.Behaviors.Row_Behaviour.StatusesEnum fromStatus, SlcFacility_Management.Behaviors.Row_Behaviour.StatusesEnum toStatus)
        {
            if (RowStatusToStatusTransitions.TryGetValue((fromStatus, toStatus), out var transitions))
            {
                return new List<SlcFacility_Management.Behaviors.Row_Behaviour.TransitionsEnum>(transitions);
            }

            return new List<SlcFacility_Management.Behaviors.Row_Behaviour.TransitionsEnum>();
        }
    }
}
