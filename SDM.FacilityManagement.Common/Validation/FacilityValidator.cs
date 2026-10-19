namespace Skyline.DataMiner.SDM.FacilityManagement.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

        protected override List<ValidationResult> ValidateBulkNoDatabaseBusinessRules(List<Facility> entities)
        {
            var results = entities.Select(_ => new ValidationResult()).ToList();

            for (int i = 0; i < entities.Count; i++)
            {
                if (entities[i].ShouldValidate(entities[i].FacilityIdField) && !FacilityValidationHandler.IsFacilityIdValid(entities[i], out var idResult))
                {
                    results[i].AddFailuresFrom(idResult);
                }

                if (entities[i].ShouldValidate(entities[i].NameField) && !FacilityValidationHandler.IsFacilityNameValid(entities[i], out var nameResult))
                {
                    results[i].AddFailuresFrom(nameResult);
                }

                if (entities[i].ShouldValidate(entities[i].LatitudeField) && !FacilityValidationHandler.IsFacilityLatitudeValid(entities[i], out var latitudeResult))
                {
                    results[i].AddFailuresFrom(latitudeResult);
                }

                if (entities[i].ShouldValidate(entities[i].LongitudeField) && !FacilityValidationHandler.IsFacilityLongitudeValid(entities[i], out var longitudeResult))
                {
                    results[i].AddFailuresFrom(longitudeResult);
                }
            }

            return results;
        }

        protected override List<ValidationResult> ValidateBulkNoDatabaseBatchConflicts(List<Facility> entities)
        {
            return ValidateIdDuplicatesInBatch(entities);
        }

        protected override List<ValidationResult> ValidateBulkWithDatabase(List<Facility> entities)
        {
            var results = ValidateBulkIdsAgainstDatabase(entities);
            results.MergeFrom(ValidateReferencesAgainstDatabase(entities));
            return results;
        }

        protected override List<ValidationResult> ValidateBulkForDelete(List<Facility> facilities)
        {
            if (facilities == null || !facilities.Any())
            {
                return new List<ValidationResult>();
            }

            return ValidateNotInUseWhenDeleted(facilities);
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

        private bool IsIdInUse(string id, string exceptIdentifier = null)
        {
            return _entityLoader.CountFacilitiesByFacilityId(id, exceptIdentifier) > 0;
        }

        private List<ValidationResult> ValidateNotInUseWhenDeleted(List<Facility> facilities)
        {
            var results = facilities.Select(_ => new ValidationResult()).ToList();
            var identifiers = facilities.Select(f => f.Identifier).Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
            if (!identifiers.Any())
            {
                return results;
            }

            var facilitiesWithFloors = _entityLoader.GetFloorsByFacilityIdentifiers(identifiers)
                .Select(f => f.FacilityFk.IsEmpty ? null : f.FacilityFk.Facility.Identifier)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .ToHashSet();

            for (int i = 0; i < facilities.Count; i++)
            {
                if (facilitiesWithFloors.Contains(facilities[i].Identifier))
                {
                    results[i].AddFailReason(
                        FacilityValidationHandler.FacilityValidationField.FacilityId,
                        "Can't remove facility, since it still has floors assigned to it. Please remove all the floors assigned to this facility before removing it.");
                }
            }

            return results;
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

        private List<ValidationResult> ValidateReferencesAgainstDatabase(List<Facility> entities)
        {
            var results = entities.Select(_ => new ValidationResult()).ToList();
            var candidates = entities
                .Select((entity, index) => new
                {
                    Entity = entity,
                    Index = index,
                    SiteIdentifier = entity.SiteFk.IsEmpty ? null : ReferenceValidationHelper.GetId(entity.SiteFk.Site),
                })
                .Where(x => ReferenceValidationHelper.ShouldValidateReferences(x.Entity) &&
                    ReferenceValidationHelper.HasId(x.SiteIdentifier))
                .ToList();

            if (!candidates.Any())
            {
                return results;
            }

            var existingSiteIds = ReferenceValidationHelper.ToIdentifierSet(
                _entityLoader.GetSitesByIdentifiers(candidates.Select(x => x.SiteIdentifier).Distinct().ToList()));

            foreach (var candidate in candidates)
            {
                if (!existingSiteIds.Contains(candidate.SiteIdentifier))
                {
                    ReferenceValidationHelper.AddMissingReference(
                        results[candidate.Index],
                        FacilityValidationHandler.FacilityValidationField.SiteId,
                        "Site",
                        candidate.SiteIdentifier);
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
