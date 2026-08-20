namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System.Collections.Generic;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using SLDataGateway.API.Types.Querying;

    /// <summary>
    /// Read-only access to ports across the DataPort and PowerPort DOM definitions.
    /// Filters are built with <see cref="PortExposers"/> and are applied to each definition
    /// using its own field descriptors. Both definitions are read in a single query and the
    /// results are split per definition.
    /// When a filter uses definition-exclusive fields (<see cref="PortExposers.DataPortOnly"/> /
    /// <see cref="PortExposers.PowerPortOnly"/>), the opposite definition is useless to filter and
    /// is skipped. When exclusive fields of both definitions are combined in one filter, no instance
    /// can match and an empty result is returned without querying.
    /// </summary>
    public interface IPortRepository
    {
        /// <summary>
        /// Reads all ports from both the DataPort and PowerPort definitions.
        /// </summary>
        /// <returns>All ports, split per definition.</returns>
        PortReadResult Read();

        /// <summary>
        /// Reads the ports matching the given filter from both the DataPort and PowerPort definitions.
        /// </summary>
        /// <param name="filter">A filter built with <see cref="PortExposers"/>.</param>
        /// <returns>The matching ports, split per definition.</returns>
        PortReadResult Read(FilterElement<IPort> filter);

        /// <summary>
        /// Reads the ports matching the given query from both the DataPort and PowerPort definitions.
        /// The query's order-by is built with <see cref="PortExposers"/>; each ordering on a field whose
        /// descriptor differs between the definitions is applied as OrderBy(DataPort field).ThenBy(PowerPort field),
        /// so instances of one definition are grouped before the other on that field while each group is sorted.
        /// Ordering on <see cref="PortExposers.Identifier"/> or <see cref="PortExposers.Asset"/> sorts both
        /// definitions together.
        /// </summary>
        /// <param name="query">A query whose filter and order-by are built with <see cref="PortExposers"/>.</param>
        /// <returns>The matching ports in query order, also split per definition.</returns>
        PortReadResult Read(IQuery<IPort> query);

        /// <summary>
        /// Reads all ports from both the DataPort and PowerPort definitions, one page at a time.
        /// </summary>
        /// <param name="pageSize">The maximum number of ports (across both definitions) per page.</param>
        /// <returns>A lazy sequence of pages, each split per definition.</returns>
        IEnumerable<PortReadResult> ReadPaged(int pageSize = 500);

        /// <summary>
        /// Reads the ports matching the given filter, one page at a time.
        /// </summary>
        /// <param name="filter">A filter built with <see cref="PortExposers"/>.</param>
        /// <param name="pageSize">The maximum number of ports (across both definitions) per page.</param>
        /// <returns>A lazy sequence of pages, each split per definition.</returns>
        IEnumerable<PortReadResult> ReadPaged(FilterElement<IPort> filter, int pageSize = 500);

        /// <summary>
        /// Reads the ports matching the given query, one page at a time.
        /// The query's order-by is expanded per definition as described on <see cref="Read(IQuery{IPort})"/>.
        /// </summary>
        /// <param name="query">A query whose filter and order-by are built with <see cref="PortExposers"/>.</param>
        /// <param name="pageSize">The maximum number of ports (across both definitions) per page.</param>
        /// <returns>A lazy sequence of pages in query order, each also split per definition.</returns>
        IEnumerable<PortReadResult> ReadPaged(IQuery<IPort> query, int pageSize = 500);
    }
}
