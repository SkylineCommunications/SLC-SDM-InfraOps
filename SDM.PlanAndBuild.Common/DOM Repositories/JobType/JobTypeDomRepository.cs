namespace Skyline.DataMiner.SDM.PlanAndBuild.Models
{
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Extensions;

    /// <summary>
    /// Extends bulk operations for <see cref="JobType"/> with batched lookups used by
    /// <see cref="Skyline.DataMiner.SDM.PlanAndBuild.Validation.JobTypeValidator"/> to validate a whole batch of
    /// JobTypes without issuing one database query per JobType.
    /// </summary>
    [AllowSdmMiddleware]
    public interface IJobTypeRepository : IBulkRepository<JobType>
    {
        /// <summary>
        /// Reads all JobTypes matching any of the given Names, using a single batched big-OR query (see
        /// <see cref="BulkRepositoryQueryExtensions.ReadByBigOrFilter{T, TKey}"/>) instead of one query per
        /// Name. Use this for bulk Name uniqueness checks instead of looping
        /// <see cref="ICountableRepository{JobType}.Count(FilterElement{JobType})"/> once per candidate JobType.
        /// </summary>
        /// <param name="names">The Names to look up. Duplicates are handled gracefully.</param>
        List<JobType> GetByNames(IEnumerable<string> names);

        /// <summary>
        /// Reads all JobTypes matching any of the given identifiers, using a single batched big-OR query.
        /// </summary>
        /// <param name="identifiers">The identifiers to look up. Duplicates are handled gracefully.</param>
        List<JobType> GetByIdentifiers(IEnumerable<string> identifiers);
    }

    internal partial class JobTypeDomRepository : IJobTypeRepository
    {
        public List<JobType> GetByNames(IEnumerable<string> names)
        {
            var keys = names?.Distinct().ToList() ?? new List<string>();

            return this.ReadByBigOrFilter(keys, name => JobTypeExposers.Name.Equal(name));
        }

        public List<JobType> GetByIdentifiers(IEnumerable<string> identifiers)
        {
            var keys = identifiers?.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList() ?? new List<string>();
            if (keys.Count == 0)
            {
                return new List<JobType>();
            }

            return this.ReadByBigOrFilter(keys, identifier => JobTypeExposers.Identifier.Equal(identifier));
        }
    }
}
