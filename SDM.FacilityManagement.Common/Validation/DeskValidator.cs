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

        protected override List<ValidationResult> ValidateBulkNoDatabaseBusinessRules(List<Desk> entities)
        {
            var results = entities.Select(_ => new ValidationResult()).ToList();

            for (int i = 0; i < entities.Count; i++)
            {
                if (entities[i].ShouldValidate(entities[i].DeskIdField) && !DeskValidationHandler.IsDeskIdValid(entities[i], out var idResult))
                {
                    results[i].AddFailuresFrom(idResult);
                }

                if (entities[i].ShouldValidate(entities[i].NameField) && !DeskValidationHandler.IsDeskNameValid(entities[i], out var nameResult))
                {
                    results[i].AddFailuresFrom(nameResult);
                }
            }

            return results;
        }

        protected override List<ValidationResult> ValidateBulkNoDatabaseBatchConflicts(List<Desk> entities)
        {
            return ValidateIdDuplicatesInBatch(entities);
        }

        protected override List<ValidationResult> ValidateBulkWithDatabase(List<Desk> entities)
        {
            var results = ValidateBulkIdsAgainstDatabase(entities);
            results.MergeFrom(ValidateReferencesAgainstDatabase(entities));

            return results;
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
                DeskValidationHandler.DeskValidationField.RoomId,
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
