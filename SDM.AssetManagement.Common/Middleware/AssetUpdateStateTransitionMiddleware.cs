namespace Skyline.DataMiner.SDM.AssetManagement.Common.Middleware
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SharedCommonLibrary.AssetManagement.State_Management;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net;
    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.AssetManagement.Models;

    using SLDataGateway.API.Types.Querying;

    internal class AssetUpdateStateTransitionMiddleware : IBulkRepositoryMiddleware<Asset>
    {
        private readonly DomHelper helper;

        internal AssetUpdateStateTransitionMiddleware(IConnection connection)
        {
            this.helper = new DomHelper(connection.HandleMessages, AssetDomMapper.ModuleId);
        }

        public long OnCount(FilterElement<Asset> filter, Func<FilterElement<Asset>, long> next)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter), "Filter cannot be null.");
            }

            return next(filter);
        }

        public long OnCount(IQuery<Asset> query, Func<IQuery<Asset>, long> next)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), "Query cannot be null.");
            }

            return next(query);
        }

        public IReadOnlyCollection<Asset> OnCreate(IEnumerable<Asset> oToCreate, Func<IEnumerable<Asset>, IReadOnlyCollection<Asset>> next)
        {
            if (oToCreate == null)
            {
                throw new ArgumentNullException(nameof(oToCreate), "The assets to create cannot be null.");
            }

            return next(oToCreate);
        }

        public Asset OnCreate(Asset oToCreate, Func<Asset, Asset> next)
        {
            if (oToCreate == null)
            {
                throw new ArgumentNullException(nameof(oToCreate), "The asset to create cannot be null.");
            }

            return next(oToCreate);
        }

        public IReadOnlyCollection<Asset> OnCreateOrUpdate(
           IEnumerable<Asset> assets,
           Func<IEnumerable<Asset>, IReadOnlyCollection<Asset>> next)
        {
            if (assets == null || !assets.Any())
            {
                return Array.Empty<Asset>();
            }

            // Track new states (only for existing assets)
            var newStates = assets
                .Where(a => !string.IsNullOrEmpty(a.Identifier))
                .ToDictionary(a => a.Identifier, a => a.State);

            // 1. Execute the CreateOrUpdate (data fields)
            var updated = next(assets);

            // 2. Handle state transitions for updated assets
            // Note: New assets already have their state set via StatusId in ToInstance
            return ProcessStateTransitions(updated, newStates);
        }

        public void OnDelete(IEnumerable<Asset> oToDelete, Action<IEnumerable<Asset>> next)
        {
            if (oToDelete == null)
            {
                throw new ArgumentNullException(nameof(oToDelete), "The assets to delete cannot be null.");
            }

            next(oToDelete);
        }

        public void OnDelete(Asset oToDelete, Action<Asset> next)
        {
            if (oToDelete == null)
            {
                throw new ArgumentNullException(nameof(oToDelete), "The asset to delete cannot be null.");
            }

            next(oToDelete);
        }

        public IEnumerable<Asset> OnRead(FilterElement<Asset> filter, Func<FilterElement<Asset>, IEnumerable<Asset>> next)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter), "Filter cannot be null.");
            }

            return next(filter);
        }

        public IEnumerable<Asset> OnRead(IQuery<Asset> query, Func<IQuery<Asset>, IEnumerable<Asset>> next)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), "Query cannot be null.");
            }

            return next(query);
        }

        public IEnumerable<IPagedResult<Asset>> OnReadPaged(FilterElement<Asset> filter, Func<FilterElement<Asset>, IEnumerable<IPagedResult<Asset>>> next)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter), "Filter cannot be null.");
            }

            return next(filter);
        }

        public IEnumerable<IPagedResult<Asset>> OnReadPaged(IQuery<Asset> query, Func<IQuery<Asset>, IEnumerable<IPagedResult<Asset>>> next)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), "Query cannot be null.");
            }

            return next(query);
        }

        public IEnumerable<IPagedResult<Asset>> OnReadPaged(FilterElement<Asset> filter, int pageSize, Func<FilterElement<Asset>, int, IEnumerable<IPagedResult<Asset>>> next)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter), "Filter cannot be null.");
            }

            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");
            }

            return next(filter, pageSize);
        }

        public IEnumerable<IPagedResult<Asset>> OnReadPaged(IQuery<Asset> query, int pageSize, Func<IQuery<Asset>, int, IEnumerable<IPagedResult<Asset>>> next)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), "Query cannot be null.");
            }

            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");
            }

            return next(query, pageSize);
        }

        public Asset OnUpdate(Asset asset, Func<Asset, Asset> next)
        {
            if (asset == null)
            {
                throw new ArgumentNullException(nameof(asset));
            }

            var newState = asset.State;

            // 1. Execute the update (data fields)
            var updated = next(asset);

            // 2. Handle state transitions if state changed
            if (newState != updated.State)
            {
                updated = ExecuteStateTransitions(updated, newState);
            }

            return updated;
        }

        public IReadOnlyCollection<Asset> OnUpdate(
            IEnumerable<Asset> assets,
            Func<IEnumerable<Asset>, IReadOnlyCollection<Asset>> next)
        {
            if (assets == null || !assets.Any())
            {
                return Array.Empty<Asset>();
            }

            // Track new states
            var newStates = assets.ToDictionary(a => a.Identifier, a => a.State);

            // 1. Execute the update (data fields)
            var updated = next(assets);

            // 2. Handle state transitions for assets with changed states
            return ProcessStateTransitions(updated, newStates);
        }

        private IReadOnlyCollection<Asset> ProcessStateTransitions(
            IReadOnlyCollection<Asset> updatedAssets,
            Dictionary<string, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum> newStates)
        {
            var finalResults = new List<Asset>();

            foreach (var asset in updatedAssets)
            {
                // Check if this was an update (not a create) and if state changed
                if (newStates.TryGetValue(asset.Identifier, out var newState))
                {
                    if (newState != asset.State)
                    {
                        var transitioned = ExecuteStateTransitions(asset, newState);
                        finalResults.Add(transitioned);
                    }
                    else
                    {
                        finalResults.Add(asset);
                    }
                }
                else
                {
                    // This was a newly created asset, state already set
                    finalResults.Add(asset);
                }
            }

            return finalResults;
        }

        private Asset ExecuteStateTransitions(
            Asset asset,
            SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum toState)
        {
            try
            {
                var transitions = StateMachine.GetTransitionPath(asset.State, toState);

                var instanceId = new DomInstanceId(Guid.Parse(asset.Identifier))
                {
                    ModuleId = AssetDomMapper.ModuleId
                };

                DomInstance currentInstance = null;
                foreach (var transitionId in transitions)
                {
                    currentInstance = helper.DomInstances.DoStatusTransition(instanceId, SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.ToValue(transitionId));
                    asset.State = SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.ToEnum(currentInstance.StatusId);
                }

                // return back Asset with updated state
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
