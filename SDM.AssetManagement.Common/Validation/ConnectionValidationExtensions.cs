namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using System;
    using System.Collections.Generic;

    using Skyline.DataMiner.SDM.AssetManagement.Models;

    /// <summary>
    /// Shared helpers for extracting information from <see cref="Connection"/> instances during validation.
    /// </summary>
    internal static class ConnectionValidationExtensions
    {
        /// <summary>
        /// Returns the non-empty source and destination port identifiers of the connection.
        /// </summary>
        public static IEnumerable<string> GetPortIds(this Connection connection)
        {
            if (connection != null && connection.Source.Port != Guid.Empty)
            {
                yield return connection.Source.Port.ToString();
            }

            if (connection != null && connection.Destination.Port != Guid.Empty)
            {
                yield return connection.Destination.Port.ToString();
            }
        }
    }
}
