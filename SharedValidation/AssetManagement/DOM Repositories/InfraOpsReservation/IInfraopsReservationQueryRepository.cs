namespace Skyline.DataMiner.SDM.AssetManagement.Repositories
{
    using System.Collections.Generic;

    using global::Skyline.DataMiner.Net.Messages.SLDataGateway;

    using SharedCommonLibrary.AssetManagement.Models;

    /// <summary>
    /// Query repository interface for InfraopsReservation entities.
    /// Provides read-only access to Reservation data.
    /// </summary>
    public interface IInfraopsReservationQueryRepository
    {
        /// <summary>
        /// Reads InfraopsReservation entities matching the specified filter.
        /// </summary>
        /// <param name="filter">Filter to apply when querying Reservations.</param>
        /// <returns>Collection of InfraopsReservation entities matching the filter.</returns>
        IEnumerable<InfraopsReservation> Read(FilterElement<InfraopsReservation> filter);

        /// <summary>
        /// Counts the number of InfraopsReservation entities matching the specified filter.
        /// </summary>
        /// <param name="filter">Filter to apply when counting Reservations.</param>
        /// <returns>Number of InfraopsReservation entities matching the filter.</returns>
        long Count(FilterElement<InfraopsReservation> filter);
    }
}

