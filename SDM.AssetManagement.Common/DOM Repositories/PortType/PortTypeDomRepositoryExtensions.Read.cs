namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using System.Collections.Generic;

    using Skyline.DataMiner.SDM.AssetManagement.Extensions;

    public static partial class PortTypeDomRepositoryExtensions
    {
        /// <summary>
        /// Reads all Data Port Types using a server-side filter (see <see cref="PortTypeFilters.DataPortTypes"/>).
        /// Returns the same set as filtering all port types with <see cref="PortTypeExtensions.IsDataPortType(PortType)"/>.
        /// </summary>
        /// <param name="repository">The repository to read from.</param>
        /// <returns>All Data Port Types.</returns>
        public static IEnumerable<PortType> ReadDataPortTypes(this IReadableRepository<PortType> repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            return repository.Read(PortTypeFilters.DataPortTypes());
        }

        /// <summary>
        /// Reads all Power Port Types using a server-side filter (see <see cref="PortTypeFilters.PowerPortTypes"/>).
        /// Returns the same set as filtering all port types with <see cref="PortTypeExtensions.IsPowerPortType(PortType)"/>.
        /// </summary>
        /// <param name="repository">The repository to read from.</param>
        /// <returns>All Power Port Types.</returns>
        public static IEnumerable<PortType> ReadPowerPortTypes(this IReadableRepository<PortType> repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            return repository.Read(PortTypeFilters.PowerPortTypes());
        }
    }
}
