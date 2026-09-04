namespace Skyline.DataMiner.SDM.InfraOpsProperties.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Extensions;

    /// <summary>
    /// Extends bulk operations for <see cref="PropertyValues"/> with batched lookups used by
    /// <see cref="Skyline.DataMiner.SDM.InfraOpsProperties.Validation.PropertyValuesValidator"/> to validate a
    /// whole batch of PropertyValues without issuing one database query per entry.
    /// </summary>
    [AllowSdmMiddleware]
    public interface IPropertyValuesRepository : IBulkRepository<PropertyValues>
    {
        /// <summary>
        /// Reads all PropertyValues matching any of the given (LinkedObjectID, Scope) pairs, using a single
        /// batched big-OR query (see <see cref="RepositoryQueryExtensions.ReadByBigOrFilter{T, TKey}"/>)
        /// instead of one query per pair. Use this for bulk (LinkedObjectID, Scope, SubID) uniqueness checks
        /// instead of looping <see cref="IReadableRepository{PropertyValues}.Read(FilterElement{PropertyValues})"/> once per candidate entry.
        /// </summary>
        /// <param name="linkedObjectIdAndScopePairs">
        /// The (LinkedObjectID, Scope) pairs to look up. Duplicates are handled gracefully.
        /// </param>
        List<PropertyValues> GetByLinkedObjectIDsAndScopes(IEnumerable<(Guid LinkedObjectID, string Scope)> linkedObjectIdAndScopePairs);
    }

    internal partial class PropertyValuesDomRepository : IPropertyValuesRepository
    {
        public List<PropertyValues> GetByLinkedObjectIDsAndScopes(IEnumerable<(Guid LinkedObjectID, string Scope)> linkedObjectIdAndScopePairs)
        {
            var keys = linkedObjectIdAndScopePairs?.Distinct().ToList() ?? new List<(Guid LinkedObjectID, string Scope)>();

            return this.ReadByBigOrFilter(
                keys,
                key => PropertyValuesExposers.LinkedObjectID.Equal(key.LinkedObjectID).AND(PropertyValuesExposers.Scope.Equal(key.Scope)));
        }
    }
}
