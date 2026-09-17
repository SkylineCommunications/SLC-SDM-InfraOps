namespace Skyline.DataMiner.SDM.InfraOpsProperties.Models
{
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Extensions;

    /// <summary>
    /// Extends bulk operations for <see cref="Property"/> with batched lookups used by
    /// <see cref="Skyline.DataMiner.SDM.InfraOpsProperties.Validation.PropertyValidator"/> to validate a whole
    /// batch of Properties without issuing one database query per Property.
    /// </summary>
    [AllowSdmMiddleware]
    public interface IPropertyRepository : IBulkRepository<Property>
    {
        /// <summary>
        /// Reads all Properties matching any of the given (Scope, Name) pairs, using a single batched big-OR
        /// query (see <see cref="RepositoryQueryExtensions.ReadByBigOrFilter{T, TKey}"/>) instead of one
        /// query per pair. Use this for bulk (Scope, Name) uniqueness checks instead of looping
        /// <see cref="ICountableRepository{Property}.Count(FilterElement{Property})"/> once per candidate Property.
        /// </summary>
        /// <param name="scopeNamePairs">The (Scope, Name) pairs to look up. Duplicates are handled gracefully.</param>
        List<Property> GetByScopeAndNames(IEnumerable<(string Scope, string Name)> scopeNamePairs);

        /// <summary>
        /// Reads all Properties matching any of the given identifiers, using a single batched big-OR query.
        /// </summary>
        /// <param name="identifiers">The identifiers to look up. Duplicates are handled gracefully.</param>
        List<Property> GetByIdentifiers(IEnumerable<string> identifiers);
    }

    internal partial class PropertyDomRepository : IPropertyRepository
    {
        public List<Property> GetByScopeAndNames(IEnumerable<(string Scope, string Name)> scopeNamePairs)
        {
            var keys = scopeNamePairs?.Distinct().ToList() ?? new List<(string Scope, string Name)>();

            return this.ReadByBigOrFilter(
                keys,
                key => PropertyExposers.Scope.Equal(key.Scope).AND(PropertyExposers.Name.Equal(key.Name)));
        }

        public List<Property> GetByIdentifiers(IEnumerable<string> identifiers)
        {
            var keys = identifiers?.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList() ?? new List<string>();
            if (keys.Count == 0)
            {
                return new List<Property>();
            }

            return this.ReadByBigOrFilter(keys, identifier => PropertyExposers.Identifier.Equal(identifier));
        }
    }
}
