namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Result of reading ports across the DataPort and PowerPort DOM definitions,
    /// split per definition.
    /// </summary>
    public sealed class PortReadResult
    {
        internal PortReadResult(IReadOnlyList<DataPort> dataPorts, IReadOnlyList<PowerPort> powerPorts)
        {
            DataPorts = dataPorts ?? Array.Empty<DataPort>();
            PowerPorts = powerPorts ?? Array.Empty<PowerPort>();
        }

        /// <summary>
        /// Gets the ports that were read from the DataPort DOM definition.
        /// </summary>
        public IReadOnlyList<DataPort> DataPorts { get; }

        /// <summary>
        /// Gets the ports that were read from the PowerPort DOM definition.
        /// </summary>
        public IReadOnlyList<PowerPort> PowerPorts { get; }
    }
}
