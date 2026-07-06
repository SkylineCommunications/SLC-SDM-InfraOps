namespace Skyline.DataMiner.SDM.InfraOpsProperties.Middleware
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;

    using SLDataGateway.API.Types.Querying;

    /// <summary>
    /// Middleware that ensures all SDM objects have identifiers before they reach the repository.
    /// Assigns GUIDs to objects with null/empty identifiers.
    /// </summary>
    public class IdentifierMiddleware<T> : IBulkRepositoryMiddleware<T>
        where T : SdmObject<T>
    {
        public IReadOnlyCollection<T> OnCreate(
            IEnumerable<T> oToCreate,
            Func<IEnumerable<T>, IReadOnlyCollection<T>> next)
        {
            var itemsList = oToCreate.ToList();
            EnsureIdentifiers(itemsList);
            return next(itemsList);
        }

        public T OnCreate(T oToCreate, Func<T, T> next)
        {
            EnsureIdentifier(oToCreate);
            return next(oToCreate);
        }

        public IReadOnlyCollection<T> OnCreateOrUpdate(
            IEnumerable<T> oToCreateOrUpdate,
            Func<IEnumerable<T>, IReadOnlyCollection<T>> next)
        {
            var itemsList = oToCreateOrUpdate.ToList();
            EnsureIdentifiers(itemsList);
            return next(itemsList);
        }

        public IReadOnlyCollection<T> OnUpdate(
            IEnumerable<T> oToUpdate,
            Func<IEnumerable<T>, IReadOnlyCollection<T>> next)
        {
            // Updates should already have identifiers, but ensure anyway
            var itemsList = oToUpdate.ToList();
            EnsureIdentifiers(itemsList);
            return next(itemsList);
        }

        public T OnUpdate(T oToUpdate, Func<T, T> next)
        {
            EnsureIdentifier(oToUpdate);
            return next(oToUpdate);
        }

        public void OnDelete(IEnumerable<T> oToDelete, Action<IEnumerable<T>> next)
        {
            // Deletes don't need new identifiers
            next(oToDelete);
        }

        public void OnDelete(T oToDelete, Action<T> next)
        {
            // Deletes don't need new identifiers
            next(oToDelete);
        }

        public IEnumerable<T> OnRead(FilterElement<T> filter, Func<FilterElement<T>, IEnumerable<T>> next)
        {
            return next(filter);
        }

        public IEnumerable<T> OnRead(IQuery<T> query, Func<IQuery<T>, IEnumerable<T>> next)
        {
            return next(query);
        }

        public long OnCount(FilterElement<T> filter, Func<FilterElement<T>, long> next)
        {
            return next(filter);
        }

        public long OnCount(IQuery<T> query, Func<IQuery<T>, long> next)
        {
            return next(query);
        }

        public IEnumerable<IPagedResult<T>> OnReadPaged(
            FilterElement<T> filter,
            Func<FilterElement<T>, IEnumerable<IPagedResult<T>>> next)
        {
            return next(filter);
        }

        public IEnumerable<IPagedResult<T>> OnReadPaged(
            IQuery<T> query,
            Func<IQuery<T>, IEnumerable<IPagedResult<T>>> next)
        {
            return next(query);
        }

        public IEnumerable<IPagedResult<T>> OnReadPaged(
            FilterElement<T> filter,
            int pageSize,
            Func<FilterElement<T>, int, IEnumerable<IPagedResult<T>>> next)
        {
            return next(filter, pageSize);
        }

        public IEnumerable<IPagedResult<T>> OnReadPaged(
            IQuery<T> query,
            int pageSize,
            Func<IQuery<T>, int, IEnumerable<IPagedResult<T>>> next)
        {
            return next(query, pageSize);
        }

        /// <summary>
        /// Ensures a single item has an identifier.
        /// </summary>
        private static void EnsureIdentifier(T item)
        {
            if (item != null && string.IsNullOrWhiteSpace(item.Identifier))
            {
                item.Identifier = Guid.NewGuid().ToString();
            }
        }

        /// <summary>
        /// Ensures all items in the collection have identifiers.
        /// </summary>
        private static void EnsureIdentifiers(List<T> items)
        {
            if (items == null) return;

            foreach (var item in items)
            {
                EnsureIdentifier(item);
            }
        }
    }
}
