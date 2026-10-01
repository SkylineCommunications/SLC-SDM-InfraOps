namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using SharedMappers.DomIds;
    using Skyline.DataMiner.SDM.AssetManagement.Common.Extensions;

    public static partial class AssetDomRepository_Extensions
    {
        /// <summary>
        /// Updates fields and transitions state in a single atomic operation.
        /// Order: Fields are updated first, then state transition occurs.
        /// Use when you need to prepare the asset for the new state.
        /// </summary>
        /// <param name="repository">The asset repository.</param>
        /// <param name="asset">The asset to update and transition.</param>
        /// <param name="newState">The new state to transition the asset to.</param>
        public static Asset UpdateAndTransitionTo(this IAssetRepository repository, Asset asset, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum newState)
        {
            if (asset == null) throw new ArgumentNullException(nameof(asset));

            if (!asset.IsTransitionAllowed(newState))
            {
                throw new InvalidOperationException($"State transition from {asset.State} to {newState} is not allowed.");
            }

            asset.ValidateTransitionPath(newState);

            var updated = repository.Update(asset);

            return repository.TransitionTo(updated, newState);
        }

        /// <summary>
        /// Transitions state first, then updates fields.
        /// Order: State transition occurs, then fields are updated.
        /// Use when the new state enables certain field changes.
        /// 
        /// Example: Transitioning to Disposed before clearing Location.
        /// </summary>
        /// <param name="repository">The asset repository.</param>
        /// <param name="asset">The asset to transition and update.</param>
        /// <param name="newState">The new state to transition the asset to.</param>
        public static Asset TransitionAndUpdate(this IAssetRepository repository, Asset asset, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum newState)
        {
            if (asset == null) throw new ArgumentNullException(nameof(asset));

            if (!asset.IsTransitionAllowed(newState))
            {
                throw new InvalidOperationException($"State transition from {asset.State} to {newState} is not allowed.");
            }

            var transitioned = repository.TransitionTo(asset, newState);
            asset.State = transitioned.State;

            //TODO: apply changes to transision Asset and proceed. This is done to preserve external changes.
            //transitioned.ApplyChanges(asset.GetChanges()); 
            return repository.Update(asset);
        }

    }
}
