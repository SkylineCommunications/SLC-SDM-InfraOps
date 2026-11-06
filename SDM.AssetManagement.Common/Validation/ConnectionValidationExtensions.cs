namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using System.Collections.Generic;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM.AssetManagement.Common.Validation;
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

        /// <summary>
        /// Counts the connections that would become invalid if the given port had the given output type:
        /// connections using the port as source while it is Input-only, and as destination while it is Output-only.
        /// Uses the same direction rule as <see cref="ConnectionValidationHandler.IsPortDirectionValid"/>.
        /// </summary>
        public static void CountDirectionConflicts(
            this IEnumerable<Connection> connections,
            string portId,
            SlcAsset_Management.Enums.Outputtype? outputType,
            out int asSource,
            out int asDestination)
        {
            asSource = 0;
            asDestination = 0;

            if (connections == null || string.IsNullOrWhiteSpace(portId))
            {
                return;
            }

            foreach (var connection in connections)
            {
                if (connection == null)
                {
                    continue;
                }

                if (connection.Source.Port.HasValue()
                    && connection.Source.Port.Identifier == portId
                    && !ConnectionValidationHandler.IsPortDirectionValid(outputType, true, out _))
                {
                    asSource++;
                }

                if (connection.Destination.Port.HasValue()
                    && connection.Destination.Port.Identifier == portId
                    && !ConnectionValidationHandler.IsPortDirectionValid(outputType, false, out _))
                {
                    asDestination++;
                }
            }
        }
    }
}