namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using System.Collections.Generic;

    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Extensions;

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
            if (connection != null && connection.Source.Port.HasValue())
            {
                yield return connection.Source.Port.Identifier;
            }

            if (connection != null && connection.Destination.Port.HasValue())
            {
                yield return connection.Destination.Port.Identifier;
            }
        }
    }
}