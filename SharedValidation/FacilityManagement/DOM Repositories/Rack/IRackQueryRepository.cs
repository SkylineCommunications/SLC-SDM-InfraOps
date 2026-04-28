namespace Skyline.DataMiner.SDM.FacilityManagement.Repositories
{
    using System.Collections.Generic;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;

    /// <summary>
    /// Query repository interface for Rack entities.
    /// Provides read-only access to Rack data.
    /// </summary>
    public interface IRackQueryRepository
    {
        /// <summary>
        /// Reads Rack entities matching the specified filter.
        /// </summary>
        /// <param name="filter">Filter to apply when querying Racks.</param>
        /// <returns>Collection of Rack entities matching the filter.</returns>
        IEnumerable<Rack> Read(FilterElement<Rack> filter);

        /// <summary>
        /// Counts the number of Rack entities matching the specified filter.
        /// </summary>
        /// <param name="filter">Filter to apply when counting Racks.</param>
        /// <returns>Number of Rack entities matching the filter.</returns>
        long Count(FilterElement<Rack> filter);
    }
}