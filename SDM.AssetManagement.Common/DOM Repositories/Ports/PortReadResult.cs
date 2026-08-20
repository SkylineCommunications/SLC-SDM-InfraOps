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
        internal PortReadResult(IReadOnlyList<IPort> domOrderedPorts)
        {
            DomOrderedPorts = domOrderedPorts ?? Array.Empty<IPort>();
            DataPorts = DomOrderedPorts.OfType<DataPort>().ToList();
            PowerPorts = DomOrderedPorts.OfType<PowerPort>().ToList();
        }

        public IPort this[int index] => throw new NotImplementedException();

        /// <summary>
        /// Gets all ports in the order the DOM instances were returned by the query,
        /// so the result can be used directly for tables.
        /// </summary>
        public IReadOnlyList<IPort> DomOrderedPorts { get; }

        /// <summary>
        /// Gets the ports that were read from the DataPort DOM definition.
        /// </summary>
        public IReadOnlyList<DataPort> DataPorts { get; }

        /// <summary>
        /// Gets the ports that were read from the PowerPort DOM definition.
        /// </summary>
        public IReadOnlyList<PowerPort> PowerPorts { get; }

        public int PageNumber => throw new NotImplementedException();

        public bool HasNextPage => throw new NotImplementedException();

        public int Count => throw new NotImplementedException();

        public IEnumerator<IPort> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
