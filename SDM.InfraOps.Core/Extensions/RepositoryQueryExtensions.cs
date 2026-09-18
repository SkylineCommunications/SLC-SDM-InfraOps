namespace Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.Net;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.Extensions;

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
        /// Reads all entities matching any of the given <paramref name="keys"/> by combining their typed filters
        /// into big-OR queries. <see cref="Tools.RetrieveBigOrFilter{T, ID}"/> batches oversized queries, while the
        /// repository remains responsible for translating the resulting <see cref="FilterElement{T}"/> and applying
        /// any repository-specific constraints, such as a DOM definition filter.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <typeparam name="TKey">The key type used to build a per-item filter (e.g. a name, id, or tuple).</typeparam>
        /// <param name="repository">The repository to read from.</param>
        /// <param name="keys">
        /// The keys to look up. Duplicates are removed using <see cref="EqualityComparer{T}.Default"/>;
        /// null and empty input are handled gracefully.
        /// </param>
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

            var keyList = keys?.Distinct().ToList() ?? new List<TKey>();

            if (keyList.Count == 0)
            {
                return new List<T>();
            }

            return Tools.RetrieveBigOrFilter(keyList, key => filterProvider(key), filter => repository.Read(filter).ToList()).Distinct().ToList();
        }

        /// <summary>
        /// Reads all entities matching any of the given <paramref name="keys"/> by combining their typed filters
        /// into big-OR queries. <see cref="Tools.RetrieveBigOrFilter{T, ID}"/> batches oversized queries, while the
        /// repository remains responsible for translating the resulting <see cref="FilterElement{T}"/> and applying
        /// any repository-specific constraints, such as a DOM definition filter.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <typeparam name="TKey">The key type used to build a per-item filter (e.g. a name, id, or tuple).</typeparam>
        /// <param name="repository">The repository to read from.</param>
        /// <param name="keys">
        /// The keys to look up. Duplicates are removed using <see cref="EqualityComparer{T}.Default"/>;
        /// null and empty input are handled gracefully.
        /// </param>
        /// <param name="filterProvider">Builds the <see cref="FilterElement{T}"/> for a single key.</param>
        /// <param name="batchFilterResolver">Resolves the batch filter for oversized queries.</param>
        public static List<T> ReadByBigOrFilter<T, TKey>(
            this IReadableRepository<T> repository,
            IEnumerable<TKey> keys,
            Func<TKey, FilterElement<T>> filterProvider,
            Func<FilterElement<T>, FilterElement<T>> batchFilterResolver)
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

            var keyList = keys?.Distinct().ToList() ?? new List<TKey>();

            if (keyList.Count == 0)
            {
                return new List<T>();
            }

            return Tools.RetrieveBigOrFilter(keyList, key => filterProvider(key), filter => repository.Read(batchFilterResolver(filter)).ToList()).Distinct().ToList();
        }

        /// <summary>
        /// Bulk-resolves every <paramref name="references"/> entry that has a value in one (internally batched)
        /// big-OR query, keyed on <see cref="SdmObjectReference{T}.Identifier"/>. Deduplication and the
        /// null/empty-key check happen once here and in the <c>string</c>-keyed overload this delegates to;
        /// callers should not re-<c>Distinct()</c> the references themselves.
        /// </summary>
        /// <remarks>
        /// References without a value (see <see cref="SdmObjectReferenceExtensions.HasValue{T}"/>) are filtered out
        /// before deduplication, because <see cref="SdmObjectReference{T}"/> throws when hashing an unset reference
        /// (its <c>Identifier</c> is <see langword="null"/>).
        /// </remarks>
        /// <typeparam name="T">The entity type referenced by <paramref name="references"/>, and read from <paramref name="repository"/>.</typeparam>
        /// <param name="repository">The repository to read from.</param>
        /// <param name="references">
        /// The references to resolve. References without a value are skipped; null and empty input are handled gracefully.
        /// </param>
        /// <param name="filterProvider">Builds the <see cref="FilterElement{T}"/> for a single identifier.</param>
        public static List<T> ReadByBigOrFilter<T>(
            this IReadableRepository<T> repository,
            IEnumerable<SdmObjectReference<T>> references,
            Func<string, FilterElement<T>> filterProvider)
            where T : SdmObject<T>
        {
            var identifiers = references?.Where(reference => reference.HasValue()).Select(reference => reference.Identifier)
                ?? Enumerable.Empty<string>();

            return repository.ReadByBigOrFilter(identifiers, filterProvider);
        }
    }
}
