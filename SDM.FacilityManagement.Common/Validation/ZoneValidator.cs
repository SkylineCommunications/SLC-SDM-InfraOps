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

            if (entity.ShouldValidate(entity.ZoneIdField))
            {
                if (!ZoneValidationHandler.IsZoneIdValid(entity, out var idResult))
                {
                    result.AddFailuresFrom(idResult);
                    return result;
                }

                if (IsIdInUse(entity.ZoneId, entity.Identifier))
                {
                    result.AddFailReason(ZoneValidationHandler.ZoneValidationField.ZoneId,
                        $"Zone Id '{entity.ZoneId}' is already in use.");
                }
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
            return FacilityBulkValidationHelper.RunBulkValidation(
                entities,
                ZoneValidationHandler.IsZoneIdValid,
                ValidateIdDuplicatesInBatch,
                ValidateBulkIdsAgainstDatabase,
                ValidateReferencesAgainstDatabase);
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
                .Select(r => r.ZoneFk.IsEmpty ? null : r.ZoneFk.Zone.Identifier)
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
            return FacilityReferenceValidationHelper.ValidateRoomReferences(
                entities,
                ZoneValidationHandler.ZoneValidationField.ZoneId,
                entity => entity.RoomFk.IsEmpty ? null : ReferenceValidationHelper.GetId(entity.RoomFk.Room),
                ids => ReferenceValidationHelper.ToIdentifierSet(_entityLoader.GetRoomsByIdentifiers(ids)));
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
