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

        /// <summary>
        /// Validates a single Floor.
        /// <para><b>Not suitable for bulk scenarios</b>: issues one DB query per item. Use <see cref="ValidateBulk"/> instead.</para>
        /// </summary>
        protected override ValidationResult Validate(Floor entity)
        {
            var result = new ValidationResult();

            if (entity.ShouldValidate(entity.FloorIdField))
            {
                if (!FloorValidationHandler.IsFloorIdValid(entity, out var idResult))
                {
                    result.AddFailuresFrom(idResult);
                    return result;
                }

                if (IsIdInUse(entity.FloorId, entity.Identifier))
                {
                    result.AddFailReason(FloorValidationHandler.FloorValidationField.FloorId,
                        $"Floor Id '{entity.FloorId}' is already in use.");
                }
            }

            result.AddFailuresFrom(ValidateReferencesAgainstDatabase(new List<Floor> { entity })[0]);

            return result;
        }

        protected override ValidationResult ValidateForDelete(Floor floor)
        {
            if (floor == null)
            {
                throw new ArgumentNullException(nameof(floor));
            }

            return ValidateNoRoomsAssigned(new List<Floor> { floor })[0];
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

        /// <summary>
        /// Validates a batch of Floor entities in three phases:
        /// 1. Non-database checks per item (id not empty).
        /// 2. In-memory batch conflict detection (id uniqueness within batch).
        /// 3. Database uniqueness check (id uniqueness vs DB).
        /// Results are returned in the same order as the input list.
        /// </summary>
        protected override List<ValidationResult> ValidateBulk(List<Floor> entities)
        {
            return FacilityBulkValidationHelper.RunBulkValidation(
                entities,
                FloorValidationHandler.IsFloorIdValid,
                ValidateIdDuplicatesInBatch,
                ValidateBulkIdsAgainstDatabase,
                ValidateReferencesAgainstDatabase);
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
                .Select(r => r.FloorFk == null ? null : r.FloorFk.Floor.Identifier)
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
            var candidates = entities
                .Select((entity, index) => new
                {
                    Entity = entity,
                    Index = index,
                    FacilityIdentifier = entity.FacilityFk == null ? null : ReferenceValidationHelper.GetId(entity.FacilityFk.Facility),
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
