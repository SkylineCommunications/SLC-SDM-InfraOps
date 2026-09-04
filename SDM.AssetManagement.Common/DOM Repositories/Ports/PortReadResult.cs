namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Result of reading ports across the DataPort and PowerPort DOM definitions,
    /// split per definition.
    /// </summary>
    public sealed class PortReadResult : IPagedResult<IPort>
    {
        IReadOnlyList<DataPort> _dataPorts;
        IReadOnlyList<PowerPort> _powerPorts;

        internal PortReadResult(IReadOnlyList<IPort> domOrderedPorts, int pageNumber = 0, bool hasNextPage = false)
        {
            DomOrderedPorts = domOrderedPorts ?? Array.Empty<IPort>();
            PageNumber = pageNumber;
            HasNextPage = hasNextPage;
        }

        /// <summary>
        /// Gets the port at the given position in DOM query order.
        /// </summary>
        public IPort this[int index] => DomOrderedPorts[index];

        /// <summary>
        /// Gets all ports in the order the DOM instances were returned by the query,
        /// so the result can be used directly for tables.
        /// </summary>
        public IReadOnlyList<IPort> DomOrderedPorts { get; }

        /// <summary>
        /// Gets the ports that were read from the DataPort DOM definition.
        /// </summary>
        public IReadOnlyList<DataPort> DataPorts => _dataPorts ?? (_dataPorts = DomOrderedPorts.OfType<DataPort>().ToList());

        /// <summary>
        /// Gets the ports that were read from the PowerPort DOM definition.
        /// </summary>
        public IReadOnlyList<PowerPort> PowerPorts => _powerPorts ?? (_powerPorts = DomOrderedPorts.OfType<PowerPort>().ToList());

        /// <summary>
        /// Gets the current page number (0-based). Always 0 for non-paged reads.
        /// </summary>
        public int PageNumber { get; }

        /// <summary>
        /// Gets a value indicating whether there is a next page. Always <c>false</c> for non-paged reads.
        /// </summary>
        public bool HasNextPage { get; }

        /// <summary>
        /// Gets the total number of ports across both definitions.
        /// </summary>
        public int Count => DomOrderedPorts.Count;

        /// <summary>
        /// Enumerates all ports in DOM query order.
        /// </summary>
        public IEnumerator<IPort> GetEnumerator()
        {
            return DomOrderedPorts.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
