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
    /// Public validator service for Desk validation.
    /// Enforces that the Desk id is present and unique.
    /// </summary>
    public class DeskValidator : ValidatorBase<Desk>
    {
        private readonly FacilityEntityLoader _entityLoader;

        public DeskValidator(FacilityEntityLoader entityLoader)
        {
            _entityLoader = entityLoader ?? throw new ArgumentNullException(nameof(entityLoader));
        }

        /// <summary>
        /// Validates a single Desk.
        /// <para><b>Not suitable for bulk scenarios</b>: issues one DB query per item. Use <see cref="ValidateBulk"/> instead.</para>
        /// </summary>
        protected override ValidationResult Validate(Desk entity)
        {
            var result = new ValidationResult();

            if (entity.ShouldValidate(entity.DeskIdField))
            {
                if (!DeskValidationHandler.IsDeskIdValid(entity, out var idResult))
                {
                    result.AddFailuresFrom(idResult);
                    return result;
                }

                if (IsIdInUse(entity.DeskID, entity.Identifier))
                {
                    result.AddFailReason(DeskValidationHandler.DeskValidationField.DeskId,
                        $"Desk Id '{entity.DeskID}' is already in use.");
                }
            }

            result.AddFailuresFrom(ValidateReferencesAgainstDatabase(new List<Desk> { entity })[0]);

            return result;
        }

        protected override List<ValidationResult> ValidateBulkForDelete(List<Desk> desks)
        {
            if (desks == null || !desks.Any())
            {
                return new List<ValidationResult>();
            }

            return desks.Select(_ => new ValidationResult()).ToList();
        }

        /// <summary>
        /// Validates id uniqueness for real-time UI validation.
        /// <para><b>Not suitable for bulk scenarios</b>: issues one DB query per call. Use <see cref="ValidateBulk"/> instead.</para>
        /// </summary>
        public ValidationResult IsDeskIdValid(string id, string exceptIdentifier = null)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(id))
            {
                result.AddFailReason(DeskValidationHandler.DeskValidationField.DeskId,
                    "Desk Id cannot be empty or whitespace.");
                return result;
            }

            if (IsIdInUse(id, exceptIdentifier))
            {
                result.AddFailReason(DeskValidationHandler.DeskValidationField.DeskId,
                    $"Desk Id '{id}' is already in use.");
            }

            return result;
        }

        /// <summary>
        /// Validates a batch of Desk entities in three phases:
        /// 1. Non-database checks per item (id not empty).
        /// 2. In-memory batch conflict detection (id uniqueness within batch).
        /// 3. Database uniqueness check (id uniqueness vs DB).
        /// Results are returned in the same order as the input list.
        /// </summary>
        protected override List<ValidationResult> ValidateBulk(List<Desk> entities)
        {
            return FacilityBulkValidationHelper.RunBulkValidation(
                entities,
                DeskValidationHandler.IsDeskIdValid,
                ValidateIdDuplicatesInBatch,
                ValidateBulkIdsAgainstDatabase,
                ValidateReferencesAgainstDatabase);
        }

        private bool IsIdInUse(string id, string exceptIdentifier = null)
        {
            return _entityLoader.CountDesksByDeskId(id, exceptIdentifier) > 0;
        }

        private List<ValidationResult> ValidateBulkIdsAgainstDatabase(List<Desk> entities)
        {
            var results = entities.Select(_ => new ValidationResult()).ToList();

            var uniqueIds = entities
                .Select(e => e.DeskID)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!uniqueIds.Any())
            {
                return results;
            }

            var batchIdentifiers = new HashSet<string>(
                entities.Select(e => e.Identifier).Where(id => !string.IsNullOrWhiteSpace(id)));

            var dbMatches = _entityLoader.GetDesksByDeskIds(uniqueIds);

            var externalConflictIds = dbMatches
                .Where(r => !batchIdentifiers.Contains(r.Identifier))
                .Select(r => r.DeskID)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < entities.Count; i++)
            {
                var id = entities[i].DeskID;
                if (!string.IsNullOrWhiteSpace(id) && externalConflictIds.Contains(id))
                {
                    results[i].AddFailReason(
                        DeskValidationHandler.DeskValidationField.DeskId,
                        $"Desk Id '{id}' is already in use.");
                }
            }

            return results;
        }

        private List<ValidationResult> ValidateReferencesAgainstDatabase(List<Desk> entities)
        {
            return FacilityReferenceValidationHelper.ValidateRoomReferences(
                entities,
                DeskValidationHandler.DeskValidationField.DeskId,
                entity => entity.RoomFk.IsEmpty ? null : ReferenceValidationHelper.GetId(entity.RoomFk.Room),
                ids => ReferenceValidationHelper.ToIdentifierSet(_entityLoader.GetRoomsByIdentifiers(ids)));
        }

        private static List<ValidationResult> ValidateIdDuplicatesInBatch(List<Desk> entities)
        {
            var results = entities.Select(_ => new ValidationResult()).ToList();

            var duplicateIds = entities
                .Select((e, idx) => new { Id = e.DeskID, Index = idx })
                .Where(x => !string.IsNullOrWhiteSpace(x.Id))
                .GroupBy(x => x.Id, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1);

            foreach (var group in duplicateIds)
            {
                foreach (var item in group)
                {
                    results[item.Index].AddFailReason(
                        DeskValidationHandler.DeskValidationField.DeskId,
                        $"Desk Id '{item.Id}' is duplicated within the batch.");
                }
            }

            return results;
        }
    }
}
