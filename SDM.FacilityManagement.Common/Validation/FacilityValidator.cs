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

            result.AddFailuresFrom(ValidateReferencesAgainstDatabase(new List<Facility> { entity })[0]);

            return result;
        }

        protected override ValidationResult ValidateForDelete(Facility facility)
        {
            if (facility == null)
            {
                throw new ArgumentNullException(nameof(facility));
            }

            return ValidateNotInUseWhenDeleted(new List<Facility> { facility })[0];
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

        /// <summary>
        /// Validates a batch of Facility entities in three phases:
        /// 1. Non-database checks per item (id not empty).
        /// 2. In-memory batch conflict detection (id uniqueness within batch).
        /// 3. Database uniqueness check (id uniqueness vs DB).
        /// Results are returned in the same order as the input list.
        /// </summary>
        protected override List<ValidationResult> ValidateBulk(List<Facility> entities)
        {
            return FacilityBulkValidationHelper.RunBulkValidation(
                entities,
                FacilityValidationHandler.IsFacilityIdValid,
                ValidateIdDuplicatesInBatch,
                ValidateBulkIdsAgainstDatabase,
                ValidateReferencesAgainstDatabase);
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
                .Select(f => f.FacilityFk == null ? null : f.FacilityFk.Facility.Identifier)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .ToHashSet();

            var facilitiesWithAssets = GetIdentifiersWithAssets(FacilityManagementEntityType.Facility, identifiers);

            for (int i = 0; i < facilities.Count; i++)
            {
                if (facilitiesWithFloors.Contains(facilities[i].Identifier))
                {
                    results[i].AddFailReason(
                        FacilityValidationHandler.FacilityValidationField.FacilityId,
                        "Can't remove facility, since it still has floors assigned to it. Please remove all the floors assigned to this facility before removing it.");
                }

                if (facilitiesWithAssets.Contains(facilities[i].Identifier))
                {
                    results[i].AddFailReason(
                        FacilityValidationHandler.FacilityValidationField.FacilityId,
                        "Can't remove facility, since it still has assets assigned to it. Please remove all the assets assigned to this facility before removing it.");
                }
            }

            return results;
        }

        private HashSet<string> GetIdentifiersWithAssets(FacilityManagementEntityType entityType, List<string> identifiers)
        {
            var checker = _entityLoader.ExternalReferenceChecker;
            if (checker == null)
            {
                return new HashSet<string>();
            }

            return (checker.GetIdentifiersWithAssets(entityType, identifiers) ?? new List<string>())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .ToHashSet();
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
                    SiteIdentifier = entity.SiteFk == null ? null : FacilityReferenceValidationHelper.GetId(entity.SiteFk.Site),
                })
                .Where(x => FacilityReferenceValidationHelper.ShouldValidateReferences(x.Entity) &&
                    FacilityReferenceValidationHelper.HasId(x.SiteIdentifier))
                .ToList();

            if (!candidates.Any())
            {
                return results;
            }

            var existingSiteIds = FacilityReferenceValidationHelper.ToIdentifierSet(
                _entityLoader.GetSitesByIdentifiers(candidates.Select(x => x.SiteIdentifier).Distinct().ToList()));

            foreach (var candidate in candidates)
            {
                if (!existingSiteIds.Contains(candidate.SiteIdentifier))
                {
                    FacilityReferenceValidationHelper.AddMissingReference(
                        results[candidate.Index],
                        FacilityValidationHandler.FacilityValidationField.FacilityId,
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
