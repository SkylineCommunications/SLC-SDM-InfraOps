namespace Skyline.DataMiner.SDM.InfraOpsProperties.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.InfraOpsProperties.Models;

    /// <summary>
    /// Extension methods for <see cref="IBulkRepository{PropertyValues}"/> mirroring the scope/linked-object
    /// based lookups previously exposed on PropertyValuesDefinitionHandler.
    /// </summary>
    public static class PropertyValuesRepositoryExtensions
    {
        /// <summary>
        /// Gets the PropertyValues linked to a given object, within a given scope, optionally scoped further by SubID.
        /// </summary>
        /// <param name="repository">The PropertyValues repository.</param>
        /// <param name="linkedObjectId">The linked object identifier.</param>
        /// <param name="scope">The scope to filter by.</param>
        /// <param name="subId">
        /// Controls how the SubID is filtered, matching the legacy PropertyValuesDefinitionHandler behavior:
        /// <list type="bullet">
        /// <item><description><c>null</c> (default) - only matches PropertyValues that have <b>no</b> SubID set.</description></item>
        /// <item><description><c>"*"</c> - wildcard, matches PropertyValues regardless of their SubID (no SubID filter applied).</description></item>
        /// <item><description>any other value - matches PropertyValues with that exact SubID.</description></item>
        /// </list>
        /// </param>
        public static IEnumerable<PropertyValues> GetByLinkedObjectID(this IBulkRepository<PropertyValues> repository, Guid linkedObjectId, string scope, string subId = null)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            if (linkedObjectId == Guid.Empty)
            {
                throw new ArgumentException("The linked object id can't be empty", nameof(linkedObjectId));
            }

            if (string.IsNullOrWhiteSpace(scope))
            {
                throw new ArgumentException("The scope has to be defined", nameof(scope));
            }

            var filter = PropertyValuesExposers.LinkedObjectID.Equal(linkedObjectId).AND(PropertyValuesExposers.Scope.Equal(scope));

            if (subId != "*")
            {
                // subId == null resolves to "no SubID set" (the generated repository translates a null Equal
                // filter into a KeyExists(...) == false check), and any other value is an exact SubID match.
                // Only "*" is treated as a wildcard that skips the SubID filter entirely.
                filter = filter.AND(PropertyValuesExposers.SubID.Equal(subId));
            }

            return repository.Read(filter);
        }

        /// <summary>
        /// Gets the single PropertyValues linked to a given object, within a given scope, optionally scoped further
        /// by SubID. See <see cref="GetByLinkedObjectID"/> for the SubID matching semantics.
        /// </summary>
        public static PropertyValues GetSingleOrDefaultByLinkedObjectID(this IBulkRepository<PropertyValues> repository, Guid linkedObjectId, string scope, string subId = null)
        {
            return repository.GetByLinkedObjectID(linkedObjectId, scope, subId).SingleOrDefault();
        }

        /// <summary>
        /// Gets all PropertyValues instances that have at least one value referencing the given Property.
        /// </summary>
        /// <param name="repository">The PropertyValues repository.</param>
        /// <param name="propertyId">The Property identifier.</param>
        public static IEnumerable<PropertyValues> GetByPropertyID(this IBulkRepository<PropertyValues> repository, Guid propertyId)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            if (propertyId == Guid.Empty)
            {
                throw new ArgumentException("The property id can't be empty", nameof(propertyId));
            }

            var reference = new SdmObjectReference<Property>(propertyId.ToString());
            var filter = PropertyValuesExposers.Values.PropertyId.Contains(reference);

            return repository.Read(filter);
        }

        /// <summary>
        /// Copies the PropertyValues of one object to another within the same scope, overwriting
        /// any PropertyValues already linked to the target object.
        /// </summary>
        /// <remarks>
        /// Unlike the model-level Add/Remove/Set/Clear helpers, this method persists changes directly
        /// to the DOM/database: it either creates the target's PropertyValues or updates it in place,
        /// without requiring a separate save call. If <paramref name="objectIdA"/> has no PropertyValues,
        /// the target is left untouched and <c>null</c> is returned. When the target already has
        /// PropertyValues, they are overwritten via a single <c>Update</c> call (not delete + create),
        /// so a failure never leaves the target with zero or two PropertyValues instances for the same
        /// scope/SubID.
        /// </remarks>
        public static PropertyValues CopyPropertyValues(this IBulkRepository<PropertyValues> repository, string scope, Guid objectIdA, Guid objectIdB, string subIdA = null, string subIdB = null)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            if (string.IsNullOrWhiteSpace(scope))
            {
                throw new ArgumentException("The scope has to be defined", nameof(scope));
            }

            if (objectIdA == Guid.Empty)
            {
                throw new ArgumentException("The source object id can't be empty", nameof(objectIdA));
            }

            if (objectIdB == Guid.Empty)
            {
                throw new ArgumentException("The target object id can't be empty", nameof(objectIdB));
            }

            var source = repository.GetSingleOrDefaultByLinkedObjectID(objectIdA, scope, subIdA);

            if (source == null)
            {
                // Nothing to copy - leave the target untouched instead of wiping its existing values.
                return null;
            }

            var duplicate = source.Duplicate(objectIdB, subIdB);

            var existingTarget = repository.GetSingleOrDefaultByLinkedObjectID(objectIdB, scope, subIdB);

            if (existingTarget != null)
            {
                // Overwrite in place: a single Update call, so a failure can't leave the target with
                // zero (delete succeeded, create failed) or two (create succeeded, delete failed)
                // PropertyValues instances for the same scope/SubID.
                existingTarget.Values = duplicate.Values;
                return repository.Update(existingTarget);
            }

            return repository.Create(duplicate);
        }
    }
}
