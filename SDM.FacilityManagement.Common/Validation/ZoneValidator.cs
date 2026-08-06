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
    /// Public validator service for Zone validation.
    /// Enforces that the Zone id is present and unique.
    /// </summary>
    public class ZoneValidator : ValidatorBase<Zone>
    {
        private readonly FacilityEntityLoader _entityLoader;

        public ZoneValidator(FacilityEntityLoader entityLoader)
        {
            _entityLoader = entityLoader ?? throw new ArgumentNullException(nameof(entityLoader));
        }

        /// <summary>
        /// Validates a single Zone.
        /// <para><b>Not suitable for bulk scenarios</b>: issues one DB query per item. Use <see cref="ValidateBulk"/> instead.</para>
        /// </summary>
        protected override ValidationResult Validate(Zone entity)
        {
            var result = new ValidationResult();

            if (!ZoneValidationHandler.IsZoneIdValid(entity, out var idResult))
            {
                result.AddFailuresFrom(idResult);
                return result;
            }

            if (entity.ShouldValidate(entity.ZoneIdField) && IsIdInUse(entity.ZoneId, entity.Identifier))
            {
                result.AddFailReason(ZoneValidationHandler.ZoneValidationField.ZoneId,
                    $"Zone Id '{entity.ZoneId}' is already in use.");
            }

            result.AddFailuresFrom(ValidateReferencesAgainstDatabase(new List<Zone> { entity })[0]);

            return result;
        }

        protected override ValidationResult ValidateForDelete(Zone zone)
        {
            if (zone == null)
            {
                throw new ArgumentNullException(nameof(zone));
            }

            return ValidateNoRacksAssigned(new List<Zone> { zone })[0];
        }

        protected override List<ValidationResult> ValidateBulkForDelete(List<Zone> zones)
        {
            if (zones == null || !zones.Any())
            {
                return new List<ValidationResult>();
            }

            return ValidateNoRacksAssigned(zones);
        }

        /// <summary>
        /// Validates id uniqueness for real-time UI validation.
        /// <para><b>Not suitable for bulk scenarios</b>: issues one DB query per call. Use <see cref="ValidateBulk"/> instead.</para>
        /// </summary>
        public ValidationResult IsZoneIdValid(string id, string exceptIdentifier = null)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(id))
            {
                result.AddFailReason(ZoneValidationHandler.ZoneValidationField.ZoneId,
                    "Zone Id cannot be empty or whitespace.");
                return result;
            }

            if (IsIdInUse(id, exceptIdentifier))
            {
                result.AddFailReason(ZoneValidationHandler.ZoneValidationField.ZoneId,
                    $"Zone Id '{id}' is already in use.");
            }

            return result;
        }

        /// <summary>
        /// Validates a batch of Zone entities in three phases:
        /// 1. Non-database checks per item (id not empty).
        /// 2. In-memory batch conflict detection (id uniqueness within batch).
        /// 3. Database uniqueness check (id uniqueness vs DB).
        /// Results are returned in the same order as the input list.
        /// </summary>
        protected override List<ValidationResult> ValidateBulk(List<Zone> entities)
        {
            if (entities == null || !entities.Any())
            {
                return new List<ValidationResult>();
            }

            var results = entities.Select(_ => new ValidationResult()).ToList();

            for (int i = 0; i < entities.Count; i++)
            {
                if (!ZoneValidationHandler.IsZoneIdValid(entities[i], out var idResult))
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

            var referenceConflicts = ValidateReferencesAgainstDatabase(entities);
            results.MergeFrom(referenceConflicts);

            return results;
        }

        private bool IsIdInUse(string id, string exceptIdentifier = null)
        {
            return _entityLoader.CountZonesByZoneId(id, exceptIdentifier) > 0;
        }

        private List<ValidationResult> ValidateNoRacksAssigned(List<Zone> zones)
        {
            var results = zones.Select(_ => new ValidationResult()).ToList();
            var identifiers = zones.Select(z => z.Identifier).Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
            if (!identifiers.Any())
            {
                return results;
            }

            var referencedIdentifiers = _entityLoader.GetRacksByZoneIdentifiers(identifiers)
                .Select(r => r.ZoneFk == null ? null : r.ZoneFk.Zone.Identifier)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .ToHashSet();

            for (int i = 0; i < zones.Count; i++)
            {
                if (referencedIdentifiers.Contains(zones[i].Identifier))
                {
                    results[i].AddFailReason(
                        ZoneValidationHandler.ZoneValidationField.ZoneId,
                        "Can't remove zone, since it still has racks assigned to it. Please remove all the racks assigned to this zone before removing it.");
                }
            }

            return results;
        }

        private List<ValidationResult> ValidateBulkIdsAgainstDatabase(List<Zone> entities)
        {
            var results = entities.Select(_ => new ValidationResult()).ToList();

            var uniqueIds = entities
                .Select(e => e.ZoneId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!uniqueIds.Any())
            {
                return results;
            }

            var batchIdentifiers = new HashSet<string>(
                entities.Select(e => e.Identifier).Where(id => !string.IsNullOrWhiteSpace(id)));

            var dbMatches = _entityLoader.GetZonesByZoneIds(uniqueIds);

            var externalConflictIds = dbMatches
                .Where(r => !batchIdentifiers.Contains(r.Identifier))
                .Select(r => r.ZoneId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < entities.Count; i++)
            {
                var id = entities[i].ZoneId;
                if (!string.IsNullOrWhiteSpace(id) && externalConflictIds.Contains(id))
                {
                    results[i].AddFailReason(
                        ZoneValidationHandler.ZoneValidationField.ZoneId,
                        $"Zone Id '{id}' is already in use.");
                }
            }

            return results;
        }

        private List<ValidationResult> ValidateReferencesAgainstDatabase(List<Zone> entities)
        {
            var results = entities.Select(_ => new ValidationResult()).ToList();
            var roomCandidates = entities
                .Select((entity, index) => new
                {
                    Entity = entity,
                    Index = index,
                    RoomIdentifier = entity.RoomFk == null ? null : FacilityReferenceValidationHelper.GetId(entity.RoomFk.Room),
                })
                .Where(x => FacilityReferenceValidationHelper.ShouldValidateReferences(x.Entity) &&
                    FacilityReferenceValidationHelper.HasId(x.RoomIdentifier))
                .ToList();

            var resourceCandidates = entities
                .Select((entity, index) => new
                {
                    Entity = entity,
                    Index = index,
                    ResourceId = entity.Resource?.ResourceId ?? Guid.Empty,
                })
                .Where(x => FacilityReferenceValidationHelper.ShouldValidateReferences(x.Entity) &&
                    FacilityReferenceValidationHelper.HasId(x.ResourceId))
                .ToList();

            var existingRoomIds = roomCandidates.Any()
                ? FacilityReferenceValidationHelper.ToIdentifierSet(_entityLoader.GetRoomsByIdentifiers(roomCandidates.Select(x => x.RoomIdentifier).Distinct().ToList()))
                : new HashSet<string>();
            var existingResourceIds = GetExistingResourceIds(resourceCandidates.Select(x => x.ResourceId));

            foreach (var candidate in roomCandidates)
            {
                if (!existingRoomIds.Contains(candidate.RoomIdentifier))
                {
                    FacilityReferenceValidationHelper.AddMissingReference(
                        results[candidate.Index],
                        ZoneValidationHandler.ZoneValidationField.ZoneId,
                        "Room",
                        candidate.RoomIdentifier);
                }
            }

            foreach (var candidate in resourceCandidates)
            {
                if (existingResourceIds != null && !existingResourceIds.Contains(candidate.ResourceId))
                {
                    FacilityReferenceValidationHelper.AddMissingReference(
                        results[candidate.Index],
                        ZoneValidationHandler.ZoneValidationField.ZoneId,
                        "Resource",
                        candidate.ResourceId);
                }
            }

            return results;
        }

        private HashSet<Guid> GetExistingResourceIds(IEnumerable<Guid> resourceIds)
        {
            var checker = _entityLoader.ExternalReferenceChecker;
            var keys = resourceIds?.Where(FacilityReferenceValidationHelper.HasId).Distinct().ToList() ?? new List<Guid>();
            if (checker == null || !keys.Any())
            {
                // No reference checker available: treat all referenced ids as existing so the
                // reference check is effectively skipped instead of reporting false errors.
                return new HashSet<Guid>(keys);
            }

            return FacilityReferenceValidationHelper.ToGuidSet(checker.GetExistingResourceIds(keys));
        }

        private static List<ValidationResult> ValidateIdDuplicatesInBatch(List<Zone> entities)
        {
            var results = entities.Select(_ => new ValidationResult()).ToList();

            var duplicateIds = entities
                .Select((e, idx) => new { Id = e.ZoneId, Index = idx })
                .Where(x => !string.IsNullOrWhiteSpace(x.Id))
                .GroupBy(x => x.Id, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1);

            foreach (var group in duplicateIds)
            {
                foreach (var item in group)
                {
                    results[item.Index].AddFailReason(
                        ZoneValidationHandler.ZoneValidationField.ZoneId,
                        $"Zone Id '{item.Id}' is duplicated within the batch.");
                }
            }

            return results;
        }
    }
}
