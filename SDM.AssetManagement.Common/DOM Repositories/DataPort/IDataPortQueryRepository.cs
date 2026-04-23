namespace Skyline.DataMiner.SDM.AssetManagement.Repositories
{
    using System.Collections.Generic;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.AssetManagement.Models;

    /// <summary>
    /// Query repository interface for DataPort entities.
    /// Provides read-only access to DataPort data.
    /// </summary>
    public interface IDataPortQueryRepository
    {
        /// <summary>
        /// Reads DataPort entities matching the specified filter.
        /// </summary>
        /// <param name="filter">Filter to apply when querying DataPorts.</param>
        /// <returns>Collection of DataPort entities matching the filter.</returns>
        IEnumerable<DataPort> Read(FilterElement<DataPort> filter);

        /// <summary>
        /// Counts the number of DataPort entities matching the specified filter.
        /// </summary>
        /// <param name="filter">Filter to apply when counting DataPorts.</param>
        /// <returns>Number of DataPort entities matching the filter.</returns>
        long Count(FilterElement<DataPort> filter);
    }
}