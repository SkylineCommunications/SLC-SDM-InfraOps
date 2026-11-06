namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using System.Linq;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM.AssetManagement.Common.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public static partial class AssetDomRepositoryExtensions
    {
        /// <summary>
        /// Transitions asset to a new state.
        /// Use this AFTER updating fields if the new state has different validation rules.
        /// </summary>
        /// <param name="repository">The asset repository.</param>
        /// <param name="asset">The asset to transition.</param>
        /// <param name="newState">The new state to transition the asset to.</param>
        public static Asset TransitionTo(this IAssetRepository repository, Asset asset, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum newState)
        {
            if (asset == null) throw new ArgumentNullException(nameof(asset));

            if(asset.State == newState)
            {
                return asset;
            }

            var initialState = asset.State;
            var changes = SdmChangeTracking.RevertFieldChanges(asset);
            ApplyChangesResult result;

            try
            {
                asset = repository.TransitionToInternal(asset, newState);
            }
            finally
            {

                result = SdmChangeTracking.ApplyFieldChanges(asset, changes);
            }

            MarkStateChange(asset, initialState);

            if (result?.Skipped.Any() ?? false)
            {
                var skippedFields = string.Join(Environment.NewLine, result.Skipped.Select(s => $"{s.Change.FieldName} - Reason: {s.Reason}"));
                throw new InvalidOperationException($"Failed to apply changes to the asset after transitioning to {newState}.{Environment.NewLine}Skipped fields:{Environment.NewLine}{skippedFields}");
            }

            //TODO: ADD ASSERT OF destination location as location when move of extensions from solution is made
            //if in transit save destination to location 
            // logic to asset state transition from InTransit to another state, clear the location and set it to the destination location needs access to the facility manager
            //if (initialState == SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InTransit && newState != SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InTransit)
            //{
            //    ClearLocation(asset.Location);
            //    asset.Location = asset.DestinationLocation;
            //    asset.Destination = null;
            //}

            return asset;
        }

        // The transition returns a freshly loaded asset whose state is already the original value.
        // Re-baseline the field on the previous state so GetChanges reports previous -> new.
        private static void MarkStateChange(Asset asset, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum previousState)
        {
            var currentState = asset.State;
            if (currentState == previousState || !(asset.StateField is IDomInstanceFieldApplyChanges stateField))
            {
                return;
            }

            asset.State = previousState;
            stateField.ApplyChanges();
            asset.State = currentState;
        }

        /// <summary>
        /// Updates fields first, then transitions state. If the transition fails, the field update remains persisted.
        /// Prevalidates the transition against the proposed field values before updating.
        /// Use when you need to prepare the asset for the new state.
        /// </summary>
        /// <param name="repository">The asset repository.</param>
        /// <param name="asset">The asset to update and transition.</param>
        /// <param name="newState">The new state to transition the asset to.</param>
        public static Asset UpdateAndTransitionTo(this IAssetRepository repository, Asset asset, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum newState)
        {
            if (asset == null) throw new ArgumentNullException(nameof(asset));

            if (asset.State != newState)
            {
                if (!asset.IsTransitionAllowed(newState))
                {
                    throw new InvalidOperationException($"State transition from {asset.State} to {newState} is not allowed.");
                }

                asset.ValidateTransitionPath(newState);
            }

            var updated = repository.Update(asset);

            return AssetDomRepositoryExtensions.TransitionTo(repository, updated, newState);
        }

        /// <summary>
        /// Transitions state first, then updates fields.
        /// Order: State transition occurs, then fields are updated.
        /// If the field update fails, the state transition remains persisted.
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

            asset = AssetDomRepositoryExtensions.TransitionTo(repository, asset, newState);

            return repository.Update(asset);
        }

    }
}
