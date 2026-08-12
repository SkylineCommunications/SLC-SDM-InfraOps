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
    /// Public validator service for Rack validation.
    /// Enforces Rack Id presence and uniqueness alongside the dimension and capacity business rules.
    /// </summary>
    public class RackValidator : ValidatorBase<Rack>
    {
        private readonly FacilityEntityLoader _entityLoader;

        /// <summary>
        /// Initializes a new instance of the <see cref="RackValidator"/> class.
        /// </summary>
        /// <param name="entityLoader">The entity loader for querying racks.</param>
        public RackValidator(FacilityEntityLoader entityLoader)
        {
            _entityLoader = entityLoader ?? throw new ArgumentNullException(nameof(entityLoader));
        }

        /// <summary>
        /// Validates a single Rack.
        /// <para><b>Not suitable for bulk scenarios</b>: issues one DB query per item. Use <see cref="ValidateBulk"/> instead.</para>
        /// </summary>
        protected override ValidationResult Validate(Rack rack)
        {
            if (rack == null)
            {
                throw new ArgumentNullException(nameof(rack));
            }

            var result = new ValidationResult();

            if (rack.ShouldValidate(rack.RackIdField))
            {
                // Id not empty is critical - stop if invalid.
                if (!RackValidationHandler.IsRackIdValid(rack, out var idResult))
                {
                    result.AddFailuresFrom(idResult);
                    return result;
                }

                if (IsRackIdInUse(rack.RackId, rack.Identifier))
                {
                    result.AddFailReason(RackValidationHandler.RackValidationField.RackId,
                        $"Rack Id '{rack.RackId}' is already in use.");
                }
            }

            AddBusinessRuleFailures(rack, result);

            result.AddFailuresFrom(ValidateReferencesAgainstDatabase(new List<Rack> { rack })[0]);

            return result;
        }

        protected override List<ValidationResult> ValidateBulkForDelete(List<Rack> racks)
        {
            if (racks == null || !racks.Any())
            {
                return new List<ValidationResult>();
            }

            return racks.Select(_ => new ValidationResult()).ToList();
        }

        /// <summary>
        /// Validates Rack Id uniqueness for real-time UI validation.
        /// <para><b>Not suitable for bulk scenarios</b>: issues one DB query per call. Use <see cref="ValidateBulk"/> instead.</para>
        /// </summary>
        public ValidationResult IsRackIdValid(string rackId, string exceptIdentifier = null)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(rackId))
            {
                result.AddFailReason(RackValidationHandler.RackValidationField.RackId,
                    "Rack Id cannot be empty or whitespace.");
                return result;
            }

            if (IsRackIdInUse(rackId, exceptIdentifier))
            {
                result.AddFailReason(RackValidationHandler.RackValidationField.RackId,
                    $"Rack Id '{rackId}' is already in use.");
            }

            return result;
        }

        /// <summary>
        /// Validates a batch of Racks in three phases:
        /// 1. Non-database checks per item (id not empty + dimension/capacity rules).
        /// 2. In-memory batch conflict detection (id uniqueness within batch).
        /// 3. Database uniqueness check (id uniqueness vs DB).
        /// Results are returned in the same order as the input list.
        /// </summary>
        protected override List<ValidationResult> ValidateBulk(List<Rack> racks)
        {
            if (racks == null || !racks.Any())
            {
                return new List<ValidationResult>();
            }

            var results = racks.Select(_ => new ValidationResult()).ToList();

            // ============================================================
            // PHASE 1: NO DATABASE ACCESS CHECKS (BUSINESS RULES)
            // ============================================================
            for (int i = 0; i < racks.Count; i++)
            {
                if (!RackValidationHandler.IsRackIdValid(racks[i], out var idResult))
                {
                    results[i].AddFailuresFrom(idResult);
                }

                AddBusinessRuleFailures(racks[i], results[i]);
            }

            if (results.AnyInvalid())
            {
                return results;
            }

            // ============================================================
            // PHASE 2: IN-MEMORY BATCH CONFLICT DETECTION
            // ============================================================
            var batchConflicts = ValidateRackIdDuplicatesInBatch(racks);
            results.MergeFrom(batchConflicts);

            if (results.AnyInvalid())
            {
                return results;
            }

            // ============================================================
            // PHASE 2.5: BULK ID UNIQUENESS CHECK AGAINST DATABASE
            // One OR-based query via Tools.RetrieveBigOrFilter — no large AND filter.
            // ============================================================
            var dbConflicts = ValidateBulkRackIdsAgainstDatabase(racks);
            results.MergeFrom(dbConflicts);

            var referenceConflicts = ValidateReferencesAgainstDatabase(racks);
            results.MergeFrom(referenceConflicts);

            return results;
        }

        private static void AddBusinessRuleFailures(Rack rack, ValidationResult result)
        {
            if (!RackValidationHandler.IsRackHeightValid(rack, out var heightResult))
            {
                result.AddFailuresFrom(heightResult);
            }

            if (!RackValidationHandler.IsRackDepthValid(rack, out var depthResult))
            {
                result.AddFailuresFrom(depthResult);
            }

            if (!RackValidationHandler.IsRackWidthValid(rack, out var widthResult))
            {
                result.AddFailuresFrom(widthResult);
            }

            if (!RackValidationHandler.IsRackUnitCapacityValid(rack, out var unitResult))
            {
                result.AddFailuresFrom(unitResult);
            }

            if (!RackValidationHandler.IsRackPowerCapacityValid(rack, out var powerResult))
            {
                result.AddFailuresFrom(powerResult);
            }
        }

        private bool IsRackIdInUse(string rackId, string exceptIdentifier = null)
        {
            return _entityLoader.CountRacksByRackId(rackId, exceptIdentifier) > 0;
        }

        private List<ValidationResult> ValidateBulkRackIdsAgainstDatabase(List<Rack> racks)
        {
            var results = racks.Select(_ => new ValidationResult()).ToList();

            var uniqueIds = racks
                .Select(r => r.RackId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!uniqueIds.Any())
            {
                return results;
            }

            var batchIdentifiers = new HashSet<string>(
                racks.Select(r => r.Identifier).Where(id => !string.IsNullOrWhiteSpace(id)));

            var dbMatches = _entityLoader.GetRacksByRackIds(uniqueIds);

            var externalConflictIds = dbMatches
                .Where(r => !batchIdentifiers.Contains(r.Identifier))
                .Select(r => r.RackId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < racks.Count; i++)
            {
                var rackId = racks[i].RackId;
                if (!string.IsNullOrWhiteSpace(rackId) && externalConflictIds.Contains(rackId))
                {
                    results[i].AddFailReason(
                        RackValidationHandler.RackValidationField.RackId,
                        $"Rack Id '{rackId}' is already in use.");
                }
            }

            return results;
        }

        private List<ValidationResult> ValidateReferencesAgainstDatabase(List<Rack> racks)
        {
            var results = racks.Select(_ => new ValidationResult()).ToList();
            var rowCandidates = racks
                .Select((rack, index) => new
                {
                    Rack = rack,
                    Index = index,
                    RowIdentifier = rack.RowFk.IsEmpty ? null : ReferenceValidationHelper.GetId(rack.RowFk.Row),
                })
                .Where(x => ReferenceValidationHelper.ShouldValidateReferences(x.Rack) &&
                    ReferenceValidationHelper.HasId(x.RowIdentifier))
                .Select(x => (x.Index, x.RowIdentifier))
                .ToList();
            var zoneCandidates = racks
                .Select((rack, index) => new
                {
                    Rack = rack,
                    Index = index,
                    ZoneIdentifier = rack.ZoneFk.IsEmpty ? null : ReferenceValidationHelper.GetId(rack.ZoneFk.Zone),
                })
                .Where(x => ReferenceValidationHelper.ShouldValidateReferences(x.Rack) &&
                    ReferenceValidationHelper.HasId(x.ZoneIdentifier))
                .Select(x => (x.Index, x.ZoneIdentifier))
                .ToList();

            var existingRowIds = rowCandidates.Any()
                ? ReferenceValidationHelper.ToIdentifierSet(_entityLoader.GetRowsByIdentifiers(rowCandidates.Select(x => x.RowIdentifier).Distinct().ToList()))
                : new HashSet<string>();
            var existingZoneIds = zoneCandidates.Any()
                ? ReferenceValidationHelper.ToIdentifierSet(_entityLoader.GetZonesByIdentifiers(zoneCandidates.Select(x => x.ZoneIdentifier).Distinct().ToList()))
                : new HashSet<string>();

            ValidateStringReferences(rowCandidates, existingRowIds, "Row", results);
            ValidateStringReferences(zoneCandidates, existingZoneIds, "Zone", results);

            return results;
        }

        private static void ValidateStringReferences(
            List<(int Index, string Identifier)> candidates,
            HashSet<string> existingIds,
            string referenceType,
            List<ValidationResult> results)
        {
            foreach (var (index, identifier) in candidates)
            {
                if (!existingIds.Contains(identifier))
                {
                    ReferenceValidationHelper.AddMissingReference(
                        results[index],
                        RackValidationHandler.RackValidationField.RackId,
                        referenceType,
                        identifier);
                }
            }
        }

        private static List<ValidationResult> ValidateRackIdDuplicatesInBatch(List<Rack> racks)
        {
            var results = racks.Select(_ => new ValidationResult()).ToList();

            var duplicateIds = racks
                .Select((r, idx) => new { r.RackId, Index = idx })
                .Where(x => !string.IsNullOrWhiteSpace(x.RackId))
                .GroupBy(x => x.RackId, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1);

            foreach (var group in duplicateIds)
            {
                foreach (var item in group)
                {
                    results[item.Index].AddFailReason(
                        RackValidationHandler.RackValidationField.RackId,
                        $"Rack Id '{item.RackId}' is duplicated within the batch.");
                }
            }

            return results;
        }
    }
}
