namespace Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.Net;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;

    /// <summary>
    /// Generic batched-read helper built on <see cref="Tools.RetrieveBigOrFilter{T, ID}"/>. Fetches every entity
    /// matching any of a set of keys in one (internally batched) big-OR query, instead of issuing a separate
    /// database round-trip per key.
    /// </summary>
    /// <remarks>
    /// Use this instead of a <c>foreach</c>/<c>for</c> loop that calls <c>repository.Read(filter)</c> or
    /// <c>repository.Count(filter)</c> once per item (e.g. per-item uniqueness checks in a bulk validator).
    /// Each per-module DOM repository exposes a typed wrapper around this method (e.g.
    /// <c>IPropertyRepository.GetByScopeAndNames</c>) so callers don't need to build the <c>FilterElement</c>
    /// themselves.
    /// </remarks>
    public static class RepositoryQueryExtensions
    {
        /// <summary>
        /// Reads all entities matching any of the given <paramref name="keys"/>, using a single big-OR filter
        /// (internally batched by <see cref="Tools.RetrieveBigOrFilter{T, ID}"/> to avoid oversized queries)
        /// instead of one query per key.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <typeparam name="TKey">The key type used to build a per-item filter (e.g. a name, id, or tuple).</typeparam>
        /// <param name="repository">The repository to read from.</param>
        /// <param name="keys">The keys to look up. Duplicates and empty input are handled gracefully.</param>
        /// <param name="filterProvider">Builds the <see cref="FilterElement{T}"/> for a single key.</param>
        public static List<T> ReadByBigOrFilter<T, TKey>(
            this IReadableRepository<T> repository,
            IEnumerable<TKey> keys,
            Func<TKey, FilterElement<T>> filterProvider)
            where T : class
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            if (filterProvider == null)
            {
                throw new ArgumentNullException(nameof(filterProvider));
            }

            var keyList = keys?.ToList() ?? new List<TKey>();

            if (keyList.Count == 0)
            {
                return new List<T>();
            }

            return Tools.RetrieveBigOrFilter<T, TKey>(keyList, key => filterProvider(key), filter => repository.Read(filter).ToList());
        }
    }
}
