namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Skyline.DataMiner.Net.Helper;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Extensions;

    public static partial class AssetClassDomRepositoryExtensions
    {
        /// <summary>
        /// Reads the object matching the given identifier. Recommended to use <see cref="ReadByIdentifiers(IReadableRepository{AssetClass}, IEnumerable{string})"/> when retrieving multiple values.
        /// </summary>
        /// <param name="repository">The repository to read from.</param>
        /// <param name="id">The identifier to match.</param>
        /// <returns>The matching object, or <see langword="null"/> if none exists.</returns>
        public static AssetClass ReadByIdentifier(this IReadableRepository<AssetClass> repository, string id)
        {
            return ReadByIdentifiers(repository, new[] { id }).SingleOrDefault();
        }

        /// <summary>
        /// Reads objects matching any supplied identifiers in one combined retrieval.
        /// </summary>
        /// <param name="repository">The repository to read from.</param>
        /// <param name="identifiers">The identifiers to match.</param>
        /// <returns>The matching objects, or <see langword="null"/> when no values are supplied.</returns>
        public static IEnumerable<AssetClass> ReadByIdentifiers(this IReadableRepository<AssetClass> repository, IEnumerable<string> identifiers)
        {
            var notNullIdentifiers = identifiers?.Where(identifier => Guid.TryParse(identifier, out _));
            if (notNullIdentifiers.IsNullOrEmpty())
            {
                return Array.Empty<AssetClass>();
            }

            return RepositoryQueryExtensions.ReadByBigOrFilter(repository, notNullIdentifiers, value => AssetClassExposers.Identifier.Equal(value));
        }

    }
}
