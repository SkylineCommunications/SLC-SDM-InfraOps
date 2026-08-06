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
    /// Public validator service for Room validation.
    /// Enforces that the Room id is present and unique.
    /// </summary>
    public class RoomValidator : ValidatorBase<Room>
    {
        private readonly FacilityEntityLoader _entityLoader;

        public RoomValidator(FacilityEntityLoader entityLoader)
        {
            _entityLoader = entityLoader ?? throw new ArgumentNullException(nameof(entityLoader));
        }

        /// <summary>
        /// Validates a single Room.
        /// <para><b>Not suitable for bulk scenarios</b>: issues one DB query per item. Use <see cref="ValidateBulk"/> instead.</para>
        /// </summary>
        protected override ValidationResult Validate(Room entity)
        {
            var result = new ValidationResult();

            if (!RoomValidationHandler.IsRoomIdValid(entity, out var idResult))
            {
                result.AddFailuresFrom(idResult);
                return result;
            }

            if (entity.ShouldValidate(entity.RoomIdField) && IsIdInUse(entity.RoomId, entity.Identifier))
            {
                result.AddFailReason(RoomValidationHandler.RoomValidationField.RoomId,
                    $"Room Id '{entity.RoomId}' is already in use.");
            }

            result.AddFailuresFrom(ValidateReferencesAgainstDatabase(new List<Room> { entity })[0]);

            return result;
        }

        protected override ValidationResult ValidateForDelete(Room room)
        {
            if (room == null)
            {
                throw new ArgumentNullException(nameof(room));
            }

            return ValidateNotInUseWhenDeleted(new List<Room> { room })[0];
        }

        protected override List<ValidationResult> ValidateBulkForDelete(List<Room> rooms)
        {
            if (rooms == null || !rooms.Any())
            {
                return new List<ValidationResult>();
            }

            return ValidateNotInUseWhenDeleted(rooms);
        }

        /// <summary>
        /// Validates id uniqueness for real-time UI validation.
        /// <para><b>Not suitable for bulk scenarios</b>: issues one DB query per call. Use <see cref="ValidateBulk"/> instead.</para>
        /// </summary>
        public ValidationResult IsRoomIdValid(string id, string exceptIdentifier = null)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(id))
            {
                result.AddFailReason(RoomValidationHandler.RoomValidationField.RoomId,
                    "Room Id cannot be empty or whitespace.");
                return result;
            }

            if (IsIdInUse(id, exceptIdentifier))
            {
                result.AddFailReason(RoomValidationHandler.RoomValidationField.RoomId,
                    $"Room Id '{id}' is already in use.");
            }

            return result;
        }

        /// <summary>
        /// Validates a batch of Room entities in three phases:
        /// 1. Non-database checks per item (id not empty).
        /// 2. In-memory batch conflict detection (id uniqueness within batch).
        /// 3. Database uniqueness check (id uniqueness vs DB).
        /// Results are returned in the same order as the input list.
        /// </summary>
        protected override List<ValidationResult> ValidateBulk(List<Room> entities)
        {
            if (entities == null || !entities.Any())
            {
                return new List<ValidationResult>();
            }

            var results = entities.Select(_ => new ValidationResult()).ToList();

            for (int i = 0; i < entities.Count; i++)
            {
                if (!RoomValidationHandler.IsRoomIdValid(entities[i], out var idResult))
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
            return _entityLoader.CountRoomsByRoomId(id, exceptIdentifier) > 0;
        }

        private List<ValidationResult> ValidateNotInUseWhenDeleted(List<Room> rooms)
        {
            var results = rooms.Select(_ => new ValidationResult()).ToList();
            var identifiers = rooms.Select(r => r.Identifier).Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
            if (!identifiers.Any())
            {
                return results;
            }

            var roomsWithRows = _entityLoader.GetRowsByRoomIdentifiers(identifiers)
                .Select(r => r.RoomFk == null ? null : r.RoomFk.Room.Identifier)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .ToHashSet();
            var roomsWithZones = _entityLoader.GetZonesByRoomIdentifiers(identifiers)
                .Select(z => z.RoomFk == null ? null : z.RoomFk.Room.Identifier)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .ToHashSet();
            var roomsWithDesks = _entityLoader.GetDesksByRoomIdentifiers(identifiers)
                .Select(d => d.RoomFk == null ? null : d.RoomFk.Room.Identifier)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .ToHashSet();
            var roomsWithAssets = GetIdentifiersWithAssets(FacilityManagementEntityType.Room, identifiers);

            for (int i = 0; i < rooms.Count; i++)
            {
                var identifier = rooms[i].Identifier;
                AddDeleteBlockIfPresent(roomsWithRows, identifier, results[i],
                    "Can't remove room, since it still has rows assigned to it. Please remove all the rows assigned to this room before removing it.");
                AddDeleteBlockIfPresent(roomsWithZones, identifier, results[i],
                    "Can't remove room, since it still has zones assigned to it. Please remove all the zones assigned to this room before removing it.");
                AddDeleteBlockIfPresent(roomsWithDesks, identifier, results[i],
                    "Can't remove room, since it still has desks assigned to it. Please remove all the desks assigned to this room before removing it.");
                AddDeleteBlockIfPresent(roomsWithAssets, identifier, results[i],
                    "Can't remove room, since it still has assets assigned to it. Please remove all the assets assigned to this room before removing it.");
            }

            return results;
        }

        private static void AddDeleteBlockIfPresent(HashSet<string> usedIds, string identifier, ValidationResult result, string message)
        {
            if (usedIds.Contains(identifier))
            {
                result.AddFailReason(RoomValidationHandler.RoomValidationField.RoomId, message);
            }
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

        private List<ValidationResult> ValidateBulkIdsAgainstDatabase(List<Room> entities)
        {
            var results = entities.Select(_ => new ValidationResult()).ToList();

            var uniqueIds = entities
                .Select(e => e.RoomId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!uniqueIds.Any())
            {
                return results;
            }

            var batchIdentifiers = new HashSet<string>(
                entities.Select(e => e.Identifier).Where(id => !string.IsNullOrWhiteSpace(id)));

            var dbMatches = _entityLoader.GetRoomsByRoomIds(uniqueIds);

            var externalConflictIds = dbMatches
                .Where(r => !batchIdentifiers.Contains(r.Identifier))
                .Select(r => r.RoomId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < entities.Count; i++)
            {
                var id = entities[i].RoomId;
                if (!string.IsNullOrWhiteSpace(id) && externalConflictIds.Contains(id))
                {
                    results[i].AddFailReason(
                        RoomValidationHandler.RoomValidationField.RoomId,
                        $"Room Id '{id}' is already in use.");
                }
            }

            return results;
        }

        private List<ValidationResult> ValidateReferencesAgainstDatabase(List<Room> entities)
        {
            var results = entities.Select(_ => new ValidationResult()).ToList();
            var floorCandidates = entities
                .Select((entity, index) => new
                {
                    Entity = entity,
                    Index = index,
                    FloorIdentifier = entity.FloorFk == null ? null : FacilityReferenceValidationHelper.GetId(entity.FloorFk.Floor),
                })
                .Where(x => FacilityReferenceValidationHelper.ShouldValidateReferences(x.Entity) &&
                    FacilityReferenceValidationHelper.HasId(x.FloorIdentifier))
                .Select(x => (x.Index, x.FloorIdentifier))
                .ToList();
            var ownerCandidates = entities
                .Select((entity, index) => new { Entity = entity, Index = index, OwnerId = entity.Ownership?.Owner ?? Guid.Empty })
                .Where(x => FacilityReferenceValidationHelper.ShouldValidateReferences(x.Entity) &&
                    FacilityReferenceValidationHelper.HasId(x.OwnerId))
                .Select(x => (x.Index, x.OwnerId))
                .ToList();
            var teamCandidates = entities
                .Select((entity, index) => new { Entity = entity, Index = index, TeamId = entity.Ownership?.Team ?? Guid.Empty })
                .Where(x => FacilityReferenceValidationHelper.ShouldValidateReferences(x.Entity) &&
                    FacilityReferenceValidationHelper.HasId(x.TeamId))
                .Select(x => (x.Index, x.TeamId))
                .ToList();
            var resourceCandidates = entities
                .Select((entity, index) => new { Entity = entity, Index = index, ResourceId = entity.ResourceLink?.ResourceId ?? Guid.Empty })
                .Where(x => FacilityReferenceValidationHelper.ShouldValidateReferences(x.Entity) &&
                    FacilityReferenceValidationHelper.HasId(x.ResourceId))
                .Select(x => (x.Index, x.ResourceId))
                .ToList();

            var existingFloorIds = floorCandidates.Any()
                ? FacilityReferenceValidationHelper.ToIdentifierSet(_entityLoader.GetFloorsByIdentifiers(floorCandidates.Select(x => x.FloorIdentifier).Distinct().ToList()))
                : new HashSet<string>();
            var existingPersonIds = GetExistingPersonIds(ownerCandidates.Select(x => x.OwnerId));
            var existingTeamIds = GetExistingTeamIds(teamCandidates.Select(x => x.TeamId));
            var existingResourceIds = GetExistingResourceIds(resourceCandidates.Select(x => x.ResourceId));

            ValidateStringReferences(floorCandidates, existingFloorIds, "Floor", results);
            ValidateGuidReferences(ownerCandidates, existingPersonIds, "Person", results);
            ValidateGuidReferences(teamCandidates, existingTeamIds, "Team", results);
            ValidateGuidReferences(resourceCandidates, existingResourceIds, "Resource", results);

            return results;
        }

        private static void ValidateStringReferences(List<(int Index, string Identifier)> candidates, HashSet<string> existingIds, string referenceType, List<ValidationResult> results)
        {
            foreach (var (index, identifier) in candidates)
            {
                if (!existingIds.Contains(identifier))
                {
                    FacilityReferenceValidationHelper.AddMissingReference(results[index], RoomValidationHandler.RoomValidationField.RoomId, referenceType, identifier);
                }
            }
        }

        private static void ValidateGuidReferences(List<(int Index, Guid Id)> candidates, HashSet<Guid> existingIds, string referenceType, List<ValidationResult> results)
        {
            foreach (var (index, id) in candidates)
            {
                if (!existingIds.Contains(id))
                {
                    FacilityReferenceValidationHelper.AddMissingReference(results[index], RoomValidationHandler.RoomValidationField.RoomId, referenceType, id);
                }
            }
        }

        private HashSet<Guid> GetExistingPersonIds(IEnumerable<Guid> personIds)
        {
            var checker = _entityLoader.ExternalReferenceChecker;
            var keys = personIds?.Where(FacilityReferenceValidationHelper.HasId).Distinct().ToList() ?? new List<Guid>();
            if (checker == null || !keys.Any())
            {
                // No reference checker available: treat all referenced ids as existing so the
                // reference check is effectively skipped instead of reporting false errors.
                return new HashSet<Guid>(keys);
            }

            return FacilityReferenceValidationHelper.ToGuidSet(checker.GetExistingPersonIds(keys));
        }

        private HashSet<Guid> GetExistingTeamIds(IEnumerable<Guid> teamIds)
        {
            var checker = _entityLoader.ExternalReferenceChecker;
            var keys = teamIds?.Where(FacilityReferenceValidationHelper.HasId).Distinct().ToList() ?? new List<Guid>();
            if (checker == null || !keys.Any())
            {
                // No reference checker available: treat all referenced ids as existing so the
                // reference check is effectively skipped instead of reporting false errors.
                return new HashSet<Guid>(keys);
            }

            return FacilityReferenceValidationHelper.ToGuidSet(checker.GetExistingTeamIds(keys));
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

        private static List<ValidationResult> ValidateIdDuplicatesInBatch(List<Room> entities)
        {
            var results = entities.Select(_ => new ValidationResult()).ToList();

            var duplicateIds = entities
                .Select((e, idx) => new { Id = e.RoomId, Index = idx })
                .Where(x => !string.IsNullOrWhiteSpace(x.Id))
                .GroupBy(x => x.Id, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1);

            foreach (var group in duplicateIds)
            {
                foreach (var item in group)
                {
                    results[item.Index].AddFailReason(
                        RoomValidationHandler.RoomValidationField.RoomId,
                        $"Room Id '{item.Id}' is duplicated within the batch.");
                }
            }

            return results;
        }
    }
}
