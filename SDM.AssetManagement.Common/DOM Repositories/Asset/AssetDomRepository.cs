namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using System.ComponentModel;
    using SharedCommonLibrary.AssetManagement.State_Management;
    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.SDM.AssetManagement.Common.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    [AllowSdmMiddleware]
    public interface IAssetRepository : IBulkRepository<Asset>
    {
        /// <summary>
        /// Infrastructure hook used by <c>AssetDomRepositoryExtensions.TransitionTo</c>. Do not call directly:
        /// it validates and executes the transition but does not preserve pending edits or prepare state history tracking.
        /// Use the <c>TransitionTo</c> extension method instead.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        Asset TransitionToInternal(Asset asset, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum newState);

        /// <summary>
        /// Infrastructure hook used by <c>AssetDomRepositoryExtensions.UpdateAndTransitionTo</c> to prevalidate the
        /// transition (path and attached transition rules) before persisting field updates. Throws when not allowed.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        void EnsureTransitionAllowedInternal(Asset asset, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum newState);
    }

    internal partial class AssetDomRepository : IAssetRepository
    {
        // Optional: null keeps raw repositories used for system writes unvalidated (CAN-003 V8).
        internal TransitionValidatorBase<Asset, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum> TransitionValidator { get; set; }

        public Asset TransitionToInternal(Asset asset, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum newState)
        {
            EnsureTransitionAllowedInternal(asset, newState);

            return ExecuteStateTransition(asset, newState);
        }

        public void EnsureTransitionAllowedInternal(Asset asset, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum newState)
        {
            if (asset == null) throw new ArgumentNullException(nameof(asset));

            if (!StateMachine.IsTransitionAllowed(asset.State, newState))
            {
                throw new InvalidOperationException($"State transition from {asset.State} to {newState} is not allowed.");
            }

            asset.ValidateTransitionPath(newState);
            TransitionValidator?.EnsureTransitionAllowed(asset, newState);
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