namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    using System;

    using SharedCommonLibrary.FacilityManagement.State_Management;
    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.SDM;

    [AllowSdmMiddleware]
    public interface ISiteRepository : IBulkRepository<Site>
    {
        Site TransitionTo(Site site, SlcFacility_Management.Behaviors.Site_Behaviour.StatusesEnum newState);

        Site UpdateAndTransitionTo(Site site, SlcFacility_Management.Behaviors.Site_Behaviour.StatusesEnum newState);

        Site TransitionAndUpdate(Site site, SlcFacility_Management.Behaviors.Site_Behaviour.StatusesEnum newState);
    }

    internal partial class SiteDomRepository : ISiteRepository
    {
        public Site TransitionTo(Site site, SlcFacility_Management.Behaviors.Site_Behaviour.StatusesEnum newState)
        {
            if (site == null) throw new ArgumentNullException(nameof(site));

            if (!SiteStateMachine.IsTransitionAllowed(site.State, newState))
            {
                throw new InvalidOperationException($"State transition from {site.State} to {newState} is not allowed.");
            }

            return ExecuteStateTransition(site, newState);
        }

        public Site UpdateAndTransitionTo(Site site, SlcFacility_Management.Behaviors.Site_Behaviour.StatusesEnum newState)
        {
            if (site == null) throw new ArgumentNullException(nameof(site));

            if (!SiteStateMachine.IsTransitionAllowed(site.State, newState))
            {
                throw new InvalidOperationException($"State transition from {site.State} to {newState} is not allowed.");
            }

            var updated = Update(site);

            return ExecuteStateTransition(updated, newState);
        }

        public Site TransitionAndUpdate(Site site, SlcFacility_Management.Behaviors.Site_Behaviour.StatusesEnum newState)
        {
            if (site == null) throw new ArgumentNullException(nameof(site));

            if (!SiteStateMachine.IsTransitionAllowed(site.State, newState))
            {
                throw new InvalidOperationException($"State transition from {site.State} to {newState} is not allowed.");
            }

            var transitioned = ExecuteStateTransition(site, newState);

            return Update(transitioned);
        }

        private Site ExecuteStateTransition(
            Site site,
            SlcFacility_Management.Behaviors.Site_Behaviour.StatusesEnum toState)
        {
            if (site == null) throw new ArgumentNullException(nameof(site));

            try
            {
                var transitions = SiteStateMachine.GetTransitionPath(site.State, toState);

                if (transitions.Count == 0)
                {
                    throw new InvalidOperationException($"No valid transition path found from {site.State} to {toState}.");
                }

                var instanceId = new DomInstanceId(Guid.Parse(site.Identifier))
                {
                    ModuleId = SiteDomMapper.ModuleId
                };

                DomInstance currentInstance = null;
                foreach (var transitionId in transitions)
                {
                    currentInstance = helper.DomInstances.DoStatusTransition(instanceId, SlcFacility_Management.Behaviors.Site_Behaviour.Transitions.ToValue(transitionId));
                }

                if (currentInstance == null)
                {
                    throw new InvalidOperationException($"State transition failed for site '{site.Identifier}' to {toState}.");
                }

                site.State = SlcFacility_Management.Behaviors.Site_Behaviour.Statuses.ToEnum(currentInstance.StatusId);
                return site;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to transition site '{site.Identifier}' from {site.State} to {toState}: {ex.Message}",
                    ex);
            }
        }
    }
}
