namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System.Collections.Generic;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;

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
    }
}
