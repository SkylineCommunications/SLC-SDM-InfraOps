namespace Skyline.DataMiner.SDM.FacilityManagement.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Shared orchestration for the Facility Management validators' bulk Create/Update flow.
    /// Centralizes the four-phase pipeline so each entity validator only supplies the
    /// entity-specific steps instead of repeating the identical control flow.
    /// </summary>
    internal static class FacilityBulkValidationHelper
    {
        /// <summary>
        /// Attempts to validate a single entity's id, returning whether it is valid and, when not,
        /// the failure details.
        /// </summary>
        /// <typeparam name="TEntity">The entity type being validated.</typeparam>
        /// <param name="entity">The entity whose id is validated.</param>
        /// <param name="result">The failure details when the id is invalid.</param>
        /// <returns><c>true</c> when the id is valid; otherwise <c>false</c>.</returns>
        internal delegate bool TryValidateId<in TEntity>(TEntity entity, out ValidationResult result);

        /// <summary>
        /// Runs the standard four-phase bulk validation flow shared by all Facility Management
        /// validators: per-item id checks, in-batch duplicate detection, database id-uniqueness
        /// checks and reference-integrity checks. Short-circuits after phase 1 or 2 when failures
        /// are already present. Results are returned in the same order as <paramref name="entities"/>.
        /// </summary>
        /// <typeparam name="TEntity">The entity type being validated.</typeparam>
        /// <param name="entities">The batch of entities to validate.</param>
        /// <param name="validateId">Per-item id validity check.</param>
        /// <param name="validateBatchDuplicates">In-memory duplicate-id detection within the batch.</param>
        /// <param name="validateDatabaseIds">Database id-uniqueness check.</param>
        /// <param name="validateReferences">Reference-integrity check.</param>
        /// <returns>One <see cref="ValidationResult"/> per entity, in input order.</returns>
        internal static List<ValidationResult> RunBulkValidation<TEntity>(
            List<TEntity> entities,
            TryValidateId<TEntity> validateId,
            Func<List<TEntity>, List<ValidationResult>> validateBatchDuplicates,
            Func<List<TEntity>, List<ValidationResult>> validateDatabaseIds,
            Func<List<TEntity>, List<ValidationResult>> validateReferences)
        {
            if (entities == null || !entities.Any())
            {
                return new List<ValidationResult>();
            }

            var results = entities.Select(_ => new ValidationResult()).ToList();

            for (int i = 0; i < entities.Count; i++)
            {
                if (!validateId(entities[i], out var idResult))
                {
                    results[i].AddFailuresFrom(idResult);
                }
            }

            if (results.AnyInvalid())
            {
                return results;
            }

            results.MergeFrom(validateBatchDuplicates(entities));

            if (results.AnyInvalid())
            {
                return results;
            }

            results.MergeFrom(validateDatabaseIds(entities));
            results.MergeFrom(validateReferences(entities));

            return results;
        }
    }
}
