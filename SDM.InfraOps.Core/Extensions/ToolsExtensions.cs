namespace Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Extensions
{
    using System;
    using System.Collections.Generic;

    using Skyline.DataMiner.Net;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;

    /// <summary>
    /// Low-level, repository-agnostic building block for batched big-OR queries. Splits a set of keys into
    /// batches (via <see cref="Tools.SplitIntoChunks{T}"/>), combines each batch's per-key filters into a single
    /// <see cref="FilterElement{T}"/>, and lets the caller supply how that combined filter is resolved (e.g. a
    /// repository read or count) and evaluated.
    /// </summary>
    /// <remarks>
    /// This class has no knowledge of <c>IReadableRepository{T}</c>/<c>ICountableRepository{T}</c> or key
    /// deduplication; it only implements the chunk-then-OR-then-resolve loop. The repository-aware overloads in
    /// <see cref="RepositoryQueryExtensions"/> (e.g. <c>HasAnyBigOrFilter</c>) build on top of this class to add
    /// deduplication and repository <c>Read</c>/<c>Count</c> calls.
    /// </remarks>
    public static class ToolsExtensions
    {
        /// <summary>
        /// Evaluates every <paramref name="allIds"/> entry against a big-OR filter, split into batches of 512 via
        /// <see cref="Tools.SplitIntoChunks{T}"/>, and short-circuits as soon as a batch satisfies
        /// <paramref name="postFilterBatchResolver"/>.
        /// </summary>
        /// <typeparam name="T">The entity type the filter is built for.</typeparam>
        /// <typeparam name="R">The type produced by <paramref name="filterResolver"/> for a batch filter (e.g. a list of entities or a count).</typeparam>
        /// <typeparam name="ID">The key type used to build a per-item filter.</typeparam>
        /// <param name="allIds">The keys to look up.</param>
        /// <param name="filterProvider">Builds the <see cref="FilterElement{T}"/> for a single key.</param>
        /// <param name="filterResolver">Resolves the combined batch filter (e.g. reads or counts against a repository).</param>
        /// <param name="postFilterBatchResolver">Determines, for a resolved batch, whether the overall result should be <see langword="true"/>.</param>
        /// <returns><see langword="true"/> if any batch satisfies <paramref name="postFilterBatchResolver"/>; otherwise <see langword="false"/>.</returns>
        public static bool HasAnyBigOrFilter<T, R, ID>(List<ID> allIds, Func<ID, FilterElement<T>> filterProvider, Func<FilterElement<T>, R> filterResolver, Func<R, bool> postFilterBatchResolver)
        {
            List<List<ID>> list2;
            try
            {
                list2 = Tools.SplitIntoChunks(allIds, 512);
            }
            catch (ArgumentException)
            {
                return false;
            }

            foreach (List<ID> item in list2)
            {
                FilterElement<T> filterElement = null;
                foreach (ID item2 in item)
                {
                    filterElement = ((filterElement != null) ? filterElement.OR(filterProvider(item2)) : filterProvider(item2));
                }

                if (filterElement != null && postFilterBatchResolver(filterResolver(filterElement)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
