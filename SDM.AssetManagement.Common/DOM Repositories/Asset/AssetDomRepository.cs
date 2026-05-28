namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;

    using SharedCommonLibrary.AssetManagement.State_Management;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;

    using static SLDataGateway.API.Types.Tasks.TaskStatus;

    /// <summary>
    /// Defines methods for updating asset fields and managing asset state transitions in a repository. Extends bulk
    /// operations for assets.
    /// </summary>
    /// <remarks>This interface provides operations to update asset properties and change their workflow
    /// state, supporting scenarios where field updates and state transitions must occur in a specific order.
    /// Implementations should ensure that combined operations are performed atomically to maintain data consistency.
    /// The interface is intended for use in asset management systems where assets have lifecycle states and validation
    /// rules that may depend on the current state.</remarks>
    public interface IAssetRepository : IBulkRepository<Asset>
    {
        /// <summary>
        /// Transitions asset to a new state.
        /// Use this AFTER updating fields if the new state has different validation rules.
        /// </summary>
        /// <param name="asset">The asset to transition.</param>
        /// <param name="newState">The new state to transition the asset to.</param>
        Asset TransitionTo(Asset asset, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum newState);

        /// <summary>
        /// Updates fields and transitions state in a single atomic operation.
        /// Order: Fields are updated first, then state transition occurs.
        /// Use when you need to prepare the asset for the new state.
        /// </summary>
        /// <param name="asset">The asset to update and transition.</param>
        /// <param name="newState">The new state to transition the asset to.</param>
        Asset UpdateAndTransitionTo(Asset asset, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum newState);

        /// <summary>
        /// Transitions state first, then updates fields.
        /// Order: State transition occurs, then fields are updated.
        /// Use when the new state enables certain field changes.
        /// 
        /// Example: Transitioning to Disposed before clearing Location.
        /// </summary>
        /// <param name="asset">The asset to transition and update.</param>
        /// <param name="newState">The new state to transition the asset to.</param>
        Asset TransitionAndUpdate(Asset asset, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum newState);
    }

    internal partial class AssetDomRepository : IAssetRepository
    {
        public Asset TransitionTo(Asset asset, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum newState)
        {
            if(asset == null) throw new ArgumentNullException(nameof(asset));

            if(!StateMachine.IsTransitionAllowed(asset.State, newState))
            {
                throw new InvalidOperationException($"State transition from {asset.State} to {newState} is not allowed.");
            }

            return ExecuteStateTransition(asset, newState);
        }

        public Asset UpdateAndTransitionTo(Asset asset, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum newState)
        {
            if (asset == null) throw new ArgumentNullException(nameof(asset));

            if (!StateMachine.IsTransitionAllowed(asset.State, newState))
            {
                throw new InvalidOperationException($"State transition from {asset.State} to {newState} is not allowed.");
            }

            var updated = Update(asset);

            return ExecuteStateTransition(updated, newState);
        }

        /// <summary>
        /// Transitions state first, then updates fields.
        /// Order: State transition occurs, then fields are updated.
        /// Use when the new state enables certain field changes.
        /// 
        /// Example: Transitioning to Disposed before clearing Location.
        /// </summary>
        public Asset TransitionAndUpdate(Asset asset, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum newState)
        {
            if (asset == null) throw new ArgumentNullException(nameof(asset));

            if (!StateMachine.IsTransitionAllowed(asset.State, newState))
            {
                throw new InvalidOperationException($"State transition from {asset.State} to {newState} is not allowed.");
            }

            var transitioned = ExecuteStateTransition(asset, newState);

            return Update(transitioned);
        }

        private Asset ExecuteStateTransition(
            Asset asset,
            SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum toState)
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

                if(currentInstance == null)
                {
                    throw new InvalidOperationException($"State transition failed for asset '{asset.Identifier}' to {toState}.");
                }

                // return back Asset with updated state
                asset.State = SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.ToEnum(currentInstance.StatusId);
                return asset;
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
