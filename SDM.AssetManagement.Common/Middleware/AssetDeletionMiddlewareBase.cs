namespace Skyline.DataMiner.SDM.AssetManagement.Deletion
{
    using System;
    using System.Collections.Generic;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.AssetManagement.Common.Middleware;
    using Skyline.DataMiner.SDM.AssetManagement.Models;

    using SLDataGateway.API.Types.Querying;

    /// <summary>
    /// Pass-through middleware base for implementations that specialize Asset deletion only.
    /// </summary>
    public abstract class AssetDeletionMiddlewareBase : IAssetDeletionMiddleware
    {
        public virtual Asset OnCreate(Asset item, Func<Asset, Asset> next) => next(item);

        public virtual IReadOnlyCollection<Asset> OnCreate(
            IEnumerable<Asset> items,
            Func<IEnumerable<Asset>, IReadOnlyCollection<Asset>> next) => next(items);

        public virtual IReadOnlyCollection<Asset> OnCreateOrUpdate(
            IEnumerable<Asset> items,
            Func<IEnumerable<Asset>, IReadOnlyCollection<Asset>> next) => next(items);

        public virtual Asset OnUpdate(Asset item, Func<Asset, Asset> next) => next(item);

        public virtual IReadOnlyCollection<Asset> OnUpdate(
            IEnumerable<Asset> items,
            Func<IEnumerable<Asset>, IReadOnlyCollection<Asset>> next) => next(items);

        public abstract void OnDelete(Asset item, Action<Asset> next);

        public abstract void OnDelete(IEnumerable<Asset> items, Action<IEnumerable<Asset>> next);

        public virtual IEnumerable<Asset> OnRead(
            FilterElement<Asset> filter,
            Func<FilterElement<Asset>, IEnumerable<Asset>> next) => next(filter);

        public virtual IEnumerable<Asset> OnRead(
            IQuery<Asset> query,
            Func<IQuery<Asset>, IEnumerable<Asset>> next) => next(query);

        public virtual long OnCount(FilterElement<Asset> filter, Func<FilterElement<Asset>, long> next)
            => next(filter);

        public virtual long OnCount(IQuery<Asset> query, Func<IQuery<Asset>, long> next)
            => next(query);

        public virtual IEnumerable<IPagedResult<Asset>> OnReadPaged(
            FilterElement<Asset> filter,
            Func<FilterElement<Asset>, IEnumerable<IPagedResult<Asset>>> next) => next(filter);

        public virtual IEnumerable<IPagedResult<Asset>> OnReadPaged(
            IQuery<Asset> query,
            Func<IQuery<Asset>, IEnumerable<IPagedResult<Asset>>> next) => next(query);

        public virtual IEnumerable<IPagedResult<Asset>> OnReadPaged(
            FilterElement<Asset> filter,
            int pageSize,
            Func<FilterElement<Asset>, int, IEnumerable<IPagedResult<Asset>>> next) => next(filter, pageSize);

        public virtual IEnumerable<IPagedResult<Asset>> OnReadPaged(
            IQuery<Asset> query,
            int pageSize,
            Func<IQuery<Asset>, int, IEnumerable<IPagedResult<Asset>>> next) => next(query, pageSize);

        public abstract void RecoverAssetDeletion(string assetIdentifier);

        public abstract void RecoverAssetDeletion(AssetDeletionRecoveryContext context);
    }
}
