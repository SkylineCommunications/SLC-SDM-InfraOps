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
    /// Public validator service for Floor validation.
    /// Enforces that the Floor id is present and unique.
    /// </summary>
    public class FloorValidator : ValidatorBase<Floor>
    {
        private readonly FacilityEntityLoader _entityLoader;

        public FloorValidator(FacilityEntityLoader entityLoader)
        {
            _entityLoader = entityLoader ?? throw new ArgumentNullException(nameof(entityLoader));
        }

        protected override List<ValidationResult> ValidateBulkNoDatabaseBusinessRules(List<Floor> entities)
        {
            var results = entities.Select(_ => new ValidationResult()).ToList();

            for (int i = 0; i < entities.Count; i++)
            {
                if (entities[i].ShouldValidate(entities[i].FloorIdField) && !FloorValidationHandler.IsFloorIdValid(entities[i], out var idResult))
                {
                    results[i].AddFailuresFrom(idResult);
                }

                if (entities[i].ShouldValidate(entities[i].NameField) && !FloorValidationHandler.IsFloorNameValid(entities[i], out var nameResult))
                {
                    results[i].AddFailuresFrom(nameResult);
                }
            }

            return results;
        }

        protected override List<ValidationResult> ValidateBulkNoDatabaseBatchConflicts(List<Floor> entities)
        {
            return ValidateIdDuplicatesInBatch(entities);
        }

        protected override List<ValidationResult> ValidateBulkWithDatabase(List<Floor> entities)
        {
            var results = ValidateBulkIdsAgainstDatabase(entities);
            results.MergeFrom(ValidateReferencesAgainstDatabase(entities));
            return results;
        }

        protected override List<ValidationResult> ValidateBulkForDelete(List<Floor> floors)
        {
            if (floors == null || !floors.Any())
            {
                return new List<ValidationResult>();
            }

            return ValidateNoRoomsAssigned(floors);
        }

        /// <summary>
        /// Validates id uniqueness for real-time UI validation.
        /// <para><b>Not suitable for bulk scenarios</b>: issues one DB query per call. Use <see cref="ValidateBulk"/> instead.</para>
        /// </summary>
        public ValidationResult IsFloorIdValid(string id, string exceptIdentifier = null)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(id))
            {
                result.AddFailReason(FloorValidationHandler.FloorValidationField.FloorId,
                    "Floor Id cannot be empty or whitespace.");
                return result;
            }

            if (IsIdInUse(id, exceptIdentifier))
            {
                result.AddFailReason(FloorValidationHandler.FloorValidationField.FloorId,
                    $"Floor Id '{id}' is already in use.");
            }

            return result;
        }

        private bool IsIdInUse(string id, string exceptIdentifier = null)
        {
            return _entityLoader.CountFloorsByFloorId(id, exceptIdentifier) > 0;
        }

        private List<ValidationResult> ValidateNoRoomsAssigned(List<Floor> floors)
        {
            var results = floors.Select(_ => new ValidationResult()).ToList();
            var identifiers = floors.Select(f => f.Identifier).Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
            if (!identifiers.Any())
            {
                return results;
            }

            var referencedIdentifiers = _entityLoader.GetRoomsByFloorIdentifiers(identifiers)
                .Select(r => r.FloorFk.IsEmpty ? null : r.FloorFk.Floor.Identifier)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .ToHashSet();

            for (int i = 0; i < floors.Count; i++)
            {
                if (referencedIdentifiers.Contains(floors[i].Identifier))
                {
                    results[i].AddFailReason(
                        FloorValidationHandler.FloorValidationField.FloorId,
                        "Can't remove floor, since it still has rooms assigned to it. Please remove all the rooms assigned to this floor before removing it.");
                }
            }

            return results;
        }

        private List<ValidationResult> ValidateBulkIdsAgainstDatabase(List<Floor> entities)
        {
            var results = entities.Select(_ => new ValidationResult()).ToList();

            var uniqueIds = entities
                .Select(e => e.FloorId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!uniqueIds.Any())
            {
                return results;
            }

            var batchIdentifiers = new HashSet<string>(
                entities.Select(e => e.Identifier).Where(id => !string.IsNullOrWhiteSpace(id)));

            var dbMatches = _entityLoader.GetFloorsByFloorIds(uniqueIds);

            var externalConflictIds = dbMatches
                .Where(r => !batchIdentifiers.Contains(r.Identifier))
                .Select(r => r.FloorId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < entities.Count; i++)
            {
                var id = entities[i].FloorId;
                if (!string.IsNullOrWhiteSpace(id) && externalConflictIds.Contains(id))
                {
                    results[i].AddFailReason(
                        FloorValidationHandler.FloorValidationField.FloorId,
                        $"Floor Id '{id}' is already in use.");
                }
            }

            return results;
        }

        private List<ValidationResult> ValidateReferencesAgainstDatabase(List<Floor> entities)
        {
            var results = entities.Select(_ => new ValidationResult()).ToList();
            for (int i = 0; i < entities.Count; i++)
            {
                if (ReferenceValidationHelper.ShouldValidateReferences(entities[i]) &&
                    (entities[i].FacilityFk.IsEmpty || !ReferenceValidationHelper.HasId(ReferenceValidationHelper.GetId(entities[i].FacilityFk.Facility))))
                {
                    ReferenceValidationHelper.AddRequiredReference(results[i], FloorValidationHandler.FloorValidationField.FacilityId, "Facility");
                }
            }

            var candidates = entities
                .Select((entity, index) => new
                {
                    Entity = entity,
                    Index = index,
                    FacilityIdentifier = entity.FacilityFk.IsEmpty ? null : ReferenceValidationHelper.GetId(entity.FacilityFk.Facility),
                })
                .Where(x => ReferenceValidationHelper.ShouldValidateReferences(x.Entity) &&
                    ReferenceValidationHelper.HasId(x.FacilityIdentifier))
                .ToList();

            if (!candidates.Any())
            {
                return results;
            }

            var existingFacilityIds = ReferenceValidationHelper.ToIdentifierSet(
                _entityLoader.GetFacilitiesByIdentifiers(candidates.Select(x => x.FacilityIdentifier).Distinct().ToList()));

            foreach (var candidate in candidates)
            {
                if (!existingFacilityIds.Contains(candidate.FacilityIdentifier))
                {
                    ReferenceValidationHelper.AddMissingReference(
                        results[candidate.Index],
                        FloorValidationHandler.FloorValidationField.FloorId,
                        "Facility",
                        candidate.FacilityIdentifier);
                }
            }

            return results;
        }

        private static List<ValidationResult> ValidateIdDuplicatesInBatch(List<Floor> entities)
        {
            var results = entities.Select(_ => new ValidationResult()).ToList();

            var duplicateIds = entities
                .Select((e, idx) => new { Id = e.FloorId, Index = idx })
                .Where(x => !string.IsNullOrWhiteSpace(x.Id))
                .GroupBy(x => x.Id, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1);

            foreach (var group in duplicateIds)
            {
                foreach (var item in group)
                {
                    results[item.Index].AddFailReason(
                        FloorValidationHandler.FloorValidationField.FloorId,
                        $"Floor Id '{item.Id}' is duplicated within the batch.");
                }
            }

            return results;
        }
    }
}
