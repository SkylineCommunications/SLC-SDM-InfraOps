namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using SharedCommonLibrary.AssetManagement.State_Management;
    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.SDM.AssetManagement.Common.Extensions;

    [AllowSdmMiddleware]
    public interface IAssetRepository : IBulkRepository<Asset>
    {
        /// <summary>
        /// Transitions asset to a new state.
        /// Use this AFTER updating fields if the new state has different validation rules.
        /// </summary>
        /// <param name="asset">The asset to transition.</param>
        /// <param name="newState">The new state to transition the asset to.</param>
        Asset TransitionTo(Asset asset, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum newState);
    }

    internal partial class AssetDomRepository : IAssetRepository
    {
        public Asset TransitionTo(Asset asset, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum newState)
        {
            if (asset == null) throw new ArgumentNullException(nameof(asset));

            if (!StateMachine.IsTransitionAllowed(asset.State, newState))
            {
                throw new InvalidOperationException($"State transition from {asset.State} to {newState} is not allowed.");
            }

            asset.ValidateTransitionPath(newState);

            return ExecuteStateTransition(asset, newState);
        }

        private Asset ExecuteStateTransition(Asset asset, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum toState)
        {
            if (asset == null) throw new ArgumentNullException(nameof(asset));

            try
            {
                var transitions = StateMachine.GetTransitionPath(asset.State, toState);

                if (transitions.Count == 0)
                {
                    throw new InvalidOperationException($"No valid transition path found from {asset.State} to {toState}.");
                }

                var instanceId = new DomInstanceId(Guid.Parse(asset.Identifier))
                {
                    ModuleId = AssetDomMapper.ModuleId
                };

                DomInstance currentInstance = null;
                foreach (var transitionId in transitions)
                {
                    currentInstance = helper.DomInstances.DoStatusTransition(instanceId, SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.ToValue(transitionId));
                }

                if (currentInstance == null)
                {
                    throw new InvalidOperationException($"State transition failed for asset '{asset.Identifier}' to {toState}.");
                }

                return FromInstance(currentInstance);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to transition asset '{asset.Identifier}' from {asset.State} to {toState}: {ex.Message}",
                    ex);
            }
        }
    }
}