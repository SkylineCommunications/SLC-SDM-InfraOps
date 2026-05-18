namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using SharedMappers.DomIds;

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
            throw new NotImplementedException();
        }

        public Asset UpdateAndTransitionTo(Asset asset, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum newState)
        {
            throw new NotImplementedException();
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
            // 1. Validate state transition
            // 2. Transition state
            // 3. Validate field changes against new state
            // 4. Update fields
            return asset;
        }
    }
}
