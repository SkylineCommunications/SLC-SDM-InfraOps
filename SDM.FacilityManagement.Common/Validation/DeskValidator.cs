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

            if (!DeskValidationHandler.IsDeskIdValid(entity, out var idResult))
            {
                result.AddFailuresFrom(idResult);
                return result;
            }

            if (entity.ShouldValidate(entity.DeskIdField) && IsIdInUse(entity.DeskID, entity.Identifier))
            {
                result.AddFailReason(DeskValidationHandler.DeskValidationField.DeskId,
                    $"Desk Id '{entity.DeskID}' is already in use.");
            }

            result.AddFailuresFrom(ValidateReferencesAgainstDatabase(new List<Desk> { entity })[0]);

            return result;
        }

        protected override ValidationResult ValidateForDelete(Desk desk)
        {
            if (desk == null)
            {
                throw new ArgumentNullException(nameof(desk));
            }

            return ValidateNoAssetsAssigned(new List<Desk> { desk })[0];
        }

        protected override List<ValidationResult> ValidateBulkForDelete(List<Desk> desks)
        {
            if (desks == null || !desks.Any())
            {
                return new List<ValidationResult>();
            }

            return ValidateNoAssetsAssigned(desks);
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

        private List<ValidationResult> ValidateNoAssetsAssigned(List<Desk> desks)
        {
            var results = desks.Select(_ => new ValidationResult()).ToList();
            var identifiers = desks.Select(d => d.Identifier).Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
            if (!identifiers.Any())
            {
                return results;
            }

            var desksWithAssets = GetIdentifiersWithAssets(FacilityManagementEntityType.Desk, identifiers);

            for (int i = 0; i < desks.Count; i++)
            {
                if (desksWithAssets.Contains(desks[i].Identifier))
                {
                    results[i].AddFailReason(
                        DeskValidationHandler.DeskValidationField.DeskId,
                        "Can't remove desk, since it still has assets assigned to it. Please remove all the assets assigned to this desk before removing it.");
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
            var checker = _entityLoader.ExternalReferenceChecker;
            return FacilityReferenceValidationHelper.ValidateRoomAndResourceReferences(
                entities,
                DeskValidationHandler.DeskValidationField.DeskId,
                entity => entity.RoomFk == null ? null : FacilityReferenceValidationHelper.GetId(entity.RoomFk.Room),
                entity => entity.Resource?.ResourceId ?? Guid.Empty,
                ids => FacilityReferenceValidationHelper.ToIdentifierSet(_entityLoader.GetRoomsByIdentifiers(ids)),
                checker == null ? (Func<IReadOnlyCollection<Guid>, IReadOnlyCollection<Guid>>)null : checker.GetExistingResourceIds);
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
