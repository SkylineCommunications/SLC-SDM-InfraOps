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

            // Id not empty is critical - stop if invalid.
            if (!RackValidationHandler.IsRackIdValid(rack, out var idResult))
            {
                result.AddFailuresFrom(idResult);
                return result;
            }

            AddBusinessRuleFailures(rack, result);

            if (rack.ShouldValidate(rack.RackIdField) && IsRackIdInUse(rack.RackId, rack.Identifier))
            {
                result.AddFailReason(RackValidationHandler.RackValidationField.RackId,
                    $"Rack Id '{rack.RackId}' is already in use.");
            }

            result.AddFailuresFrom(ValidateReferencesAgainstDatabase(new List<Rack> { rack })[0]);

            return result;
        }

        protected override ValidationResult ValidateForDelete(Rack rack)
        {
            if (rack == null)
            {
                throw new ArgumentNullException(nameof(rack));
            }

            return ValidateNoAssetsAssigned(new List<Rack> { rack })[0];
        }

        protected override List<ValidationResult> ValidateBulkForDelete(List<Rack> racks)
        {
            if (racks == null || !racks.Any())
            {
                return new List<ValidationResult>();
            }

            return ValidateNoAssetsAssigned(racks);
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

        private List<ValidationResult> ValidateNoAssetsAssigned(List<Rack> racks)
        {
            var results = racks.Select(_ => new ValidationResult()).ToList();
            var identifiers = racks.Select(r => r.Identifier).Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
            if (!identifiers.Any())
            {
                return results;
            }

            var racksWithAssets = GetIdentifiersWithAssets(FacilityManagementEntityType.Rack, identifiers);

            for (int i = 0; i < racks.Count; i++)
            {
                if (racksWithAssets.Contains(racks[i].Identifier))
                {
                    results[i].AddFailReason(
                        RackValidationHandler.RackValidationField.RackId,
                        "Can't remove rack, since it still has assets assigned to it. Please remove all the assets assigned to this rack before removing it.");
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
                    RowIdentifier = rack.RowFk == null ? null : FacilityReferenceValidationHelper.GetId(rack.RowFk.Row),
                })
                .Where(x => FacilityReferenceValidationHelper.ShouldValidateReferences(x.Rack) &&
                    FacilityReferenceValidationHelper.HasId(x.RowIdentifier))
                .Select(x => (x.Index, x.RowIdentifier))
                .ToList();
            var zoneCandidates = racks
                .Select((rack, index) => new
                {
                    Rack = rack,
                    Index = index,
                    ZoneIdentifier = rack.ZoneFk == null ? null : FacilityReferenceValidationHelper.GetId(rack.ZoneFk.Zone),
                })
                .Where(x => FacilityReferenceValidationHelper.ShouldValidateReferences(x.Rack) &&
                    FacilityReferenceValidationHelper.HasId(x.ZoneIdentifier))
                .Select(x => (x.Index, x.ZoneIdentifier))
                .ToList();
            var resourceCandidates = racks
                .Select((rack, index) => new
                {
                    Rack = rack,
                    Index = index,
                    ResourceId = rack.Resource?.ResourceId ?? Guid.Empty,
                })
                .Where(x => FacilityReferenceValidationHelper.ShouldValidateReferences(x.Rack) &&
                    FacilityReferenceValidationHelper.HasId(x.ResourceId))
                .Select(x => (x.Index, x.ResourceId))
                .ToList();

            var existingRowIds = rowCandidates.Any()
                ? FacilityReferenceValidationHelper.ToIdentifierSet(_entityLoader.GetRowsByIdentifiers(rowCandidates.Select(x => x.RowIdentifier).Distinct().ToList()))
                : new HashSet<string>();
            var existingZoneIds = zoneCandidates.Any()
                ? FacilityReferenceValidationHelper.ToIdentifierSet(_entityLoader.GetZonesByIdentifiers(zoneCandidates.Select(x => x.ZoneIdentifier).Distinct().ToList()))
                : new HashSet<string>();
            var existingResourceIds = GetExistingResourceIds(resourceCandidates.Select(x => x.ResourceId));

            ValidateStringReferences(rowCandidates, existingRowIds, "Row", results);
            ValidateStringReferences(zoneCandidates, existingZoneIds, "Zone", results);
            ValidateResourceReferences(resourceCandidates, existingResourceIds, results);

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
                    FacilityReferenceValidationHelper.AddMissingReference(
                        results[index],
                        RackValidationHandler.RackValidationField.RackId,
                        referenceType,
                        identifier);
                }
            }
        }

        private static void ValidateResourceReferences(
            List<(int Index, Guid ResourceId)> candidates,
            HashSet<Guid> existingIds,
            List<ValidationResult> results)
        {
            foreach (var (index, resourceId) in candidates)
            {
                if (!existingIds.Contains(resourceId))
                {
                    FacilityReferenceValidationHelper.AddMissingReference(
                        results[index],
                        RackValidationHandler.RackValidationField.RackId,
                        "Resource",
                        resourceId);
                }
            }
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
