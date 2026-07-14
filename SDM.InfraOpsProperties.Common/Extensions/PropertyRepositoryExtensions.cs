namespace Skyline.DataMiner.SDM.InfraOpsProperties.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.InfraOpsProperties.Models;

    /// <summary>
    /// Extension methods for <see cref="IBulkRepository{Property}"/> mirroring the scope/name based lookups
    /// previously exposed on PropertyDefinitionHandler.
    /// </summary>
    public static class PropertyRepositoryExtensions
    {
        /// <summary>
        /// Gets all Properties defined within a given scope.
        /// </summary>
        public static IEnumerable<Property> GetByScope(this IBulkRepository<Property> repository, string scope)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            if (string.IsNullOrWhiteSpace(scope))
            {
                throw new ArgumentException("The scope has to be defined", nameof(scope));
            }

            var filter = PropertyExposers.Scope.Equal(scope);
            return repository.Read(filter);
        }

        /// <summary>
        /// Gets the single Property with the given name, defined within a given scope.
        /// </summary>
        public static Property GetByScopeAndName(this IBulkRepository<Property> repository, string scope, string propertyName)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            if (string.IsNullOrWhiteSpace(scope))
            {
                throw new ArgumentException("The scope has to be defined", nameof(scope));
            }

            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new ArgumentException("The property name has to be defined", nameof(propertyName));
            }

            var filter = PropertyExposers.Scope.Equal(scope).AND(PropertyExposers.Name.Equal(propertyName));
            var matches = repository.Read(filter).ToList();

            if (matches.Count > 1)
            {
                throw new InvalidOperationException($"Found {matches.Count} Properties with scope '{scope}' and name '{propertyName}', expected at most one.");
            }

            return matches.SingleOrDefault();
        }
    }
}
