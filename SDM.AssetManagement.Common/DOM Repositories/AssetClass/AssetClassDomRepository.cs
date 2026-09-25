namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SharedCommonLibrary.AssetManagement.State_Management;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.Net.Helper;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Extensions;

    [AllowSdmMiddleware]
    public interface IAssetClassRepository : IBulkRepository<AssetClass>
    {
        /// <summary>
        /// Reads the object matching the given identifier. Recommended to use <see cref="ReadByIdentifiers(IEnumerable{string})"/> when retrieving multiple values.
        /// </summary>
        /// <param name="id">The identifier to match.</param>
        /// <returns>The matching object, or <see langword="null"/> if none exists.</returns>
        AssetClass ReadByIdentifier(string id);

        /// <summary>
        /// Reads objects matching any supplied identifiers in one combined retrieval.
        /// </summary>
        /// <param name="identifiers">The identifiers to match.</param>
        /// <returns>The matching objects, or <see langword="null"/> when no values are supplied.</returns>
        IEnumerable<AssetClass> ReadByIdentifiers(IEnumerable<string> identifiers);

        AssetClass TransitionTo(AssetClass assetClass, SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum newState);
    }

    internal partial class AssetClassDomRepository : IAssetClassRepository
    {
        public AssetClass ReadByIdentifier(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            return ReadByIdentifiers(new[] { id }).SingleOrDefault();
        }

        public IEnumerable<AssetClass> ReadByIdentifiers(IEnumerable<string> identifiers)
        {
            if (identifiers.IsNullOrEmpty())
            {
                return Array.Empty<AssetClass>();
            }

            return RepositoryQueryExtensions.ReadByBigOrFilter(this, identifiers, value => AssetClassExposers.Identifier.Equal(value));
        }

        public AssetClass TransitionTo(
            AssetClass assetClass,
            SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum newState)
        {
            if (assetClass == null)
            {
                throw new ArgumentNullException(nameof(assetClass));
            }

            if (!StateMachine.IsTransitionAllowed(assetClass.State, newState))
            {
                throw new InvalidOperationException($"State transition from {assetClass.State} to {newState} is not allowed.");
            }

            return ExecuteStateTransition(assetClass, newState);
        }

        private AssetClass ExecuteStateTransition(
            AssetClass assetClass,
            SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum toState)
        {
            if (assetClass == null)
            {
                throw new ArgumentNullException(nameof(assetClass));
            }

            try
            {
                var transitions = StateMachine.GetTransitionPath(assetClass.State, toState);
                if (transitions.Count == 0)
                {
                    throw new InvalidOperationException($"No valid transition path found from {assetClass.State} to {toState}.");
                }

                var instanceId = new DomInstanceId(Guid.Parse(assetClass.Identifier))
                {
                    ModuleId = AssetClassDomMapper.ModuleId,
                };

                DomInstance currentInstance = null;
                foreach (var transitionId in transitions)
                {
                    currentInstance = helper.DomInstances.DoStatusTransition(
                        instanceId,
                        SlcAsset_Management.Behaviors.Asset_Class_Behavior.Transitions.ToValue(transitionId));
                }

                if (currentInstance == null)
                {
                    throw new InvalidOperationException($"State transition failed for asset class '{assetClass.Identifier}' to {toState}.");
                }

                assetClass.State = SlcAsset_Management.Behaviors.Asset_Class_Behavior.Statuses.ToEnum(currentInstance.StatusId);
                return assetClass;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to transition asset class '{assetClass.Identifier}' from {assetClass.State} to {toState}: {ex.Message}",
                    ex);
            }
        }
    }
}