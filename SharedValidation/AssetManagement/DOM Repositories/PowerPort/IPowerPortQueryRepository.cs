namespace Skyline.DataMiner.SDM.AssetManagement.Repositories
{
    using System.Collections.Generic;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.AssetManagement.Models;

    /// <summary>
    /// Query repository interface for PowerPort entities.
    /// Provides read-only access to PowerPort data.
    /// </summary>
    public interface IPowerPortQueryRepository
    {
        /// <summary>
        /// Reads PowerPort entities matching the specified filter.
        /// </summary>
        /// <param name="filter">Filter to apply when querying PowerPorts.</param>
        /// <returns>Collection of PowerPort entities matching the filter.</returns>
        IEnumerable<PowerPort> Read(FilterElement<PowerPort> filter);

        /// <summary>
        /// Counts the number of PowerPort entities matching the specified filter.
        /// </summary>
        /// <param name="filter">Filter to apply when counting PowerPorts.</param>
        /// <returns>Number of PowerPort entities matching the filter.</returns>
        long Count(FilterElement<PowerPort> filter);
    }
}