namespace Skyline.DataMiner.SDM.FacilityManagement.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM.FacilityManagement.Services;
    using Skyline.DataMiner.SDM.InfraOps.Common.Validation;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Public validator service for Facility validation.
    /// Enforces that the Facility id is present and unique.
    /// </summary>
    public class FacilityValidator : ValidatorBase<Facility>
    {
        private readonly FacilityEntityLoader _entityLoader;

        public FacilityValidator(FacilityEntityLoader entityLoader)
        {
            _entityLoader = entityLoader ?? throw new ArgumentNullException(nameof(entityLoader));
        }

        /// <summary>
        /// Validates a single Facility.
        /// <para><b>Not suitable for bulk scenarios</b>: issues one DB query per item. Use <see cref="ValidateBulk"/> instead.</para>
        /// </summary>
        protected override ValidationResult Validate(Facility entity)
        {
            var result = new ValidationResult();

            if (!FacilityValidationHandler.IsFacilityIdValid(entity, out var idResult))
            {
                result.AddFailuresFrom(idResult);
                return result;
            }

            if (entity.ShouldValidate(entity.FacilityIdField) && IsIdInUse(entity.FacilityId, entity.Identifier))
            {
                result.AddFailReason(FacilityValidationHandler.FacilityValidationField.FacilityId,
                    $"Facility Id '{entity.FacilityId}' is already in use.");
            }

            return result;
        }

        /// <summary>
        /// Validates id uniqueness for real-time UI validation.
        /// <para><b>Not suitable for bulk scenarios</b>: issues one DB query per call. Use <see cref="ValidateBulk"/> instead.</para>
        /// </summary>
        public ValidationResult IsFacilityIdValid(string id, string exceptIdentifier = null)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(id))
            {
                result.AddFailReason(FacilityValidationHandler.FacilityValidationField.FacilityId,
                    "Facility Id cannot be empty or whitespace.");
                return result;
            }

            if (IsIdInUse(id, exceptIdentifier))
            {
                result.AddFailReason(FacilityValidationHandler.FacilityValidationField.FacilityId,
                    $"Facility Id '{id}' is already in use.");
            }

            return result;
        }

        /// <summary>
        /// Validates a batch of Facility entities in three phases:
        /// 1. Non-database checks per item (id not empty).
        /// 2. In-memory batch conflict detection (id uniqueness within batch).
        /// 3. Database uniqueness check (id uniqueness vs DB).
        /// Results are returned in the same order as the input list.
        /// </summary>
        protected override List<ValidationResult> ValidateBulk(List<Facility> entities)
        {
            if (entities == null || !entities.Any())
            {
                return new List<ValidationResult>();
            }

            var results = entities.Select(_ => new ValidationResult()).ToList();

            for (int i = 0; i < entities.Count; i++)
            {
                if (!FacilityValidationHandler.IsFacilityIdValid(entities[i], out var idResult))
                {
                    results[i].AddFailuresFrom(idResult);
                }
            }

            if (results.AnyInvalid())
            {
                return results;
            }

            var batchConflicts = ValidateIdDuplicatesInBatch(entities);
            results.MergeFrom(batchConflicts);

            if (results.AnyInvalid())
            {
                return results;
            }

            var dbConflicts = ValidateBulkIdsAgainstDatabase(entities);
            results.MergeFrom(dbConflicts);

            return results;
        }

        private bool IsIdInUse(string id, string exceptIdentifier = null)
        {
            return _entityLoader.CountFacilitiesByFacilityId(id, exceptIdentifier) > 0;
        }

        private List<ValidationResult> ValidateBulkIdsAgainstDatabase(List<Facility> entities)
        {
            var results = entities.Select(_ => new ValidationResult()).ToList();

            var uniqueIds = entities
                .Select(e => e.FacilityId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!uniqueIds.Any())
            {
                return results;
            }

            var batchIdentifiers = new HashSet<string>(
                entities.Select(e => e.Identifier).Where(id => !string.IsNullOrWhiteSpace(id)));

            var dbMatches = _entityLoader.GetFacilitiesByFacilityIds(uniqueIds);

            var externalConflictIds = dbMatches
                .Where(r => !batchIdentifiers.Contains(r.Identifier))
                .Select(r => r.FacilityId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < entities.Count; i++)
            {
                var id = entities[i].FacilityId;
                if (!string.IsNullOrWhiteSpace(id) && externalConflictIds.Contains(id))
                {
                    results[i].AddFailReason(
                        FacilityValidationHandler.FacilityValidationField.FacilityId,
                        $"Facility Id '{id}' is already in use.");
                }
            }

            return results;
        }

        private static List<ValidationResult> ValidateIdDuplicatesInBatch(List<Facility> entities)
        {
            var results = entities.Select(_ => new ValidationResult()).ToList();

            var duplicateIds = entities
                .Select((e, idx) => new { Id = e.FacilityId, Index = idx })
                .Where(x => !string.IsNullOrWhiteSpace(x.Id))
                .GroupBy(x => x.Id, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1);

            foreach (var group in duplicateIds)
            {
                foreach (var item in group)
                {
                    results[item.Index].AddFailReason(
                        FacilityValidationHandler.FacilityValidationField.FacilityId,
                        $"Facility Id '{item.Id}' is duplicated within the batch.");
                }
            }

            return results;
        }
    }
}
