namespace Skyline.DataMiner.SDM.InfraOpsProperties.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.InfraOps.Core.Models;
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
        /// Gets all Properties defined within the scope declared on <typeparamref name="T"/> via <see cref="PropertyScopeAttribute"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException"><typeparamref name="T"/> has no <see cref="PropertyScopeAttribute"/>.</exception>
        public static IEnumerable<Property> GetByScope<T>(this IBulkRepository<Property> repository) where T : SdmObject<T>
        {
            return repository.GetByScope(PropertyScopeAttribute.GetScope<T>());
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

        /// <summary>
        /// Gets the single Property with the given name, defined within the scope declared on <typeparamref name="T"/>
        /// via <see cref="PropertyScopeAttribute"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException"><typeparamref name="T"/> has no <see cref="PropertyScopeAttribute"/>.</exception>
        public static Property GetByScopeAndName<T>(this IBulkRepository<Property> repository, string propertyName) where T : SdmObject<T>
        {
            return repository.GetByScopeAndName(PropertyScopeAttribute.GetScope<T>(), propertyName);
        }
    }
}
