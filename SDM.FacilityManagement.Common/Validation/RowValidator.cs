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
    /// Public validator service for Row validation.
    /// Enforces that the Row id is present and unique.
    /// </summary>
    public class RowValidator : ValidatorBase<Row>
    {
        private readonly FacilityEntityLoader _entityLoader;

        public RowValidator(FacilityEntityLoader entityLoader)
        {
            _entityLoader = entityLoader ?? throw new ArgumentNullException(nameof(entityLoader));
        }

        /// <summary>
        /// Validates a single Row.
        /// <para><b>Not suitable for bulk scenarios</b>: issues one DB query per item. Use <see cref="ValidateBulk"/> instead.</para>
        /// </summary>
        protected override ValidationResult Validate(Row entity)
        {
            var result = new ValidationResult();

            if (entity.ShouldValidate(entity.RowIdField))
            {
                if (!RowValidationHandler.IsRowIdValid(entity, out var idResult))
                {
                    result.AddFailuresFrom(idResult);
                    return result;
                }

                if (IsIdInUse(entity.RowId, entity.Identifier))
                {
                    result.AddFailReason(RowValidationHandler.RowValidationField.RowId,
                        $"Row Id '{entity.RowId}' is already in use.");
                }
            }

            result.AddFailuresFrom(ValidateReferencesAgainstDatabase(new List<Row> { entity })[0]);

            return result;
        }

        protected override ValidationResult ValidateForDelete(Row row)
        {
            if (row == null)
            {
                throw new ArgumentNullException(nameof(row));
            }

            return ValidateNoRacksAssigned(new List<Row> { row })[0];
        }

        protected override List<ValidationResult> ValidateBulkForDelete(List<Row> rows)
        {
            if (rows == null || !rows.Any())
            {
                return new List<ValidationResult>();
            }

            return ValidateNoRacksAssigned(rows);
        }

        /// <summary>
        /// Validates id uniqueness for real-time UI validation.
        /// <para><b>Not suitable for bulk scenarios</b>: issues one DB query per call. Use <see cref="ValidateBulk"/> instead.</para>
        /// </summary>
        public ValidationResult IsRowIdValid(string id, string exceptIdentifier = null)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(id))
            {
                result.AddFailReason(RowValidationHandler.RowValidationField.RowId,
                    "Row Id cannot be empty or whitespace.");
                return result;
            }

            if (IsIdInUse(id, exceptIdentifier))
            {
                result.AddFailReason(RowValidationHandler.RowValidationField.RowId,
                    $"Row Id '{id}' is already in use.");
            }

            return result;
        }

        /// <summary>
        /// Validates a batch of Row entities in three phases:
        /// 1. Non-database checks per item (id not empty).
        /// 2. In-memory batch conflict detection (id uniqueness within batch).
        /// 3. Database uniqueness check (id uniqueness vs DB).
        /// Results are returned in the same order as the input list.
        /// </summary>
        protected override List<ValidationResult> ValidateBulk(List<Row> entities)
        {
            return FacilityBulkValidationHelper.RunBulkValidation(
                entities,
                RowValidationHandler.IsRowIdValid,
                ValidateIdDuplicatesInBatch,
                ValidateBulkIdsAgainstDatabase,
                ValidateReferencesAgainstDatabase);
        }

        private bool IsIdInUse(string id, string exceptIdentifier = null)
        {
            return _entityLoader.CountRowsByRowId(id, exceptIdentifier) > 0;
        }

        private List<ValidationResult> ValidateNoRacksAssigned(List<Row> rows)
        {
            var results = rows.Select(_ => new ValidationResult()).ToList();
            var identifiers = rows.Select(r => r.Identifier).Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
            if (!identifiers.Any())
            {
                return results;
            }

            var referencedIdentifiers = _entityLoader.GetRacksByRowIdentifiers(identifiers)
                .Select(r => r.RowFk.IsEmpty ? null : r.RowFk.Row.Identifier)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .ToHashSet();

            for (int i = 0; i < rows.Count; i++)
            {
                if (referencedIdentifiers.Contains(rows[i].Identifier))
                {
                    results[i].AddFailReason(
                        RowValidationHandler.RowValidationField.RowId,
                        "Can't remove row, since it still has racks assigned to it. Please remove all the racks assigned to this row before removing it.");
                }
            }

            return results;
        }

        private List<ValidationResult> ValidateBulkIdsAgainstDatabase(List<Row> entities)
        {
            var results = entities.Select(_ => new ValidationResult()).ToList();

            var uniqueIds = entities
                .Select(e => e.RowId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!uniqueIds.Any())
            {
                return results;
            }

            var batchIdentifiers = new HashSet<string>(
                entities.Select(e => e.Identifier).Where(id => !string.IsNullOrWhiteSpace(id)));

            var dbMatches = _entityLoader.GetRowsByRowIds(uniqueIds);

            var externalConflictIds = dbMatches
                .Where(r => !batchIdentifiers.Contains(r.Identifier))
                .Select(r => r.RowId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < entities.Count; i++)
            {
                var id = entities[i].RowId;
                if (!string.IsNullOrWhiteSpace(id) && externalConflictIds.Contains(id))
                {
                    results[i].AddFailReason(
                        RowValidationHandler.RowValidationField.RowId,
                        $"Row Id '{id}' is already in use.");
                }
            }

            return results;
        }

        private List<ValidationResult> ValidateReferencesAgainstDatabase(List<Row> entities)
        {
            return FacilityReferenceValidationHelper.ValidateRoomReferences(
                entities,
                RowValidationHandler.RowValidationField.RowId,
                entity => entity.RoomFk.IsEmpty ? null : ReferenceValidationHelper.GetId(entity.RoomFk.Room),
                ids => ReferenceValidationHelper.ToIdentifierSet(_entityLoader.GetRoomsByIdentifiers(ids)));
        }

        private static List<ValidationResult> ValidateIdDuplicatesInBatch(List<Row> entities)
        {
            var results = entities.Select(_ => new ValidationResult()).ToList();

            var duplicateIds = entities
                .Select((e, idx) => new { Id = e.RowId, Index = idx })
                .Where(x => !string.IsNullOrWhiteSpace(x.Id))
                .GroupBy(x => x.Id, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1);

            foreach (var group in duplicateIds)
            {
                foreach (var item in group)
                {
                    results[item.Index].AddFailReason(
                        RowValidationHandler.RowValidationField.RowId,
                        $"Row Id '{item.Id}' is duplicated within the batch.");
                }
            }

            return results;
        }
    }
}
