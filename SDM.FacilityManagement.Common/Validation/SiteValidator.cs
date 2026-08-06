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
    /// Public validator service for Site validation.
    /// Enforces that the Site id is present and unique.
    /// </summary>
    public class SiteValidator : ValidatorBase<Site>
    {
        private readonly FacilityEntityLoader _entityLoader;

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteValidator"/> class.
        /// </summary>
        /// <param name="entityLoader">The entity loader for querying sites.</param>
        public SiteValidator(FacilityEntityLoader entityLoader)
        {
            _entityLoader = entityLoader ?? throw new ArgumentNullException(nameof(entityLoader));
        }

        /// <summary>
        /// Validates a single Site.
        /// <para><b>Not suitable for bulk scenarios</b>: issues one DB query per item. Use <see cref="ValidateBulk"/> instead.</para>
        /// </summary>
        protected override ValidationResult Validate(Site site)
        {
            var result = new ValidationResult();

            if (!SiteValidationHandler.IsSiteIdValid(site, out var idResult))
            {
                result.AddFailuresFrom(idResult);
                return result;
            }

            if (site.ShouldValidate(site.SiteIdField) && IsSiteIdInUse(site.SiteId, site.Identifier))
            {
                result.AddFailReason(SiteValidationHandler.SiteValidationField.SiteId,
                    $"Site Id '{site.SiteId}' is already in use.");
            }

            return result;
        }

        protected override ValidationResult ValidateForDelete(Site site)
        {
            if (site == null)
            {
                throw new ArgumentNullException(nameof(site));
            }

            return ValidateNoFacilitiesAssigned(new List<Site> { site })[0];
        }

        protected override List<ValidationResult> ValidateBulkForDelete(List<Site> sites)
        {
            if (sites == null || !sites.Any())
            {
                return new List<ValidationResult>();
            }

            return ValidateNoFacilitiesAssigned(sites);
        }

        /// <summary>
        /// Validates name uniqueness for real-time UI validation.
        /// <para><b>Not suitable for bulk scenarios</b>: issues one DB query per call. Use <see cref="ValidateBulk"/> instead.</para>
        /// </summary>
        public ValidationResult IsSiteIdValid(string siteId, string exceptIdentifier = null)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(siteId))
            {
                result.AddFailReason(SiteValidationHandler.SiteValidationField.SiteId,
                    "Site Id cannot be empty or whitespace.");
                return result;
            }

            if (IsSiteIdInUse(siteId, exceptIdentifier))
            {
                result.AddFailReason(SiteValidationHandler.SiteValidationField.SiteId,
                    $"Site Id '{siteId}' is already in use.");
            }

            return result;
        }

        /// <summary>
        /// Validates a batch of Sites in three phases:
        /// 1. Non-database checks per item (id not empty).
        /// 2. In-memory batch conflict detection (id uniqueness within batch).
        /// 3. Database uniqueness check (id uniqueness vs DB).
        /// Results are returned in the same order as the input list.
        /// </summary>
        protected override List<ValidationResult> ValidateBulk(List<Site> sites)
        {
            if (sites == null || !sites.Any())
            {
                return new List<ValidationResult>();
            }

            var results = sites.Select(_ => new ValidationResult()).ToList();

            // ============================================================
            // PHASE 1: NO DATABASE ACCESS CHECKS (BUSINESS RULES)
            // ============================================================
            for (int i = 0; i < sites.Count; i++)
            {
                if (!SiteValidationHandler.IsSiteIdValid(sites[i], out var idResult))
                {
                    results[i].AddFailuresFrom(idResult);
                }
            }

            if (results.AnyInvalid())
            {
                return results;
            }

            // ============================================================
            // PHASE 2: IN-MEMORY BATCH CONFLICT DETECTION
            // ============================================================
            var batchConflicts = ValidateSiteIdDuplicatesInBatch(sites);
            results.MergeFrom(batchConflicts);

            if (results.AnyInvalid())
            {
                return results;
            }

            // ============================================================
            // PHASE 2.5: BULK ID UNIQUENESS CHECK AGAINST DATABASE
            // One OR-based query via Tools.RetrieveBigOrFilter — no large AND filter.
            // ============================================================
            var dbConflicts = ValidateBulkSiteIdsAgainstDatabase(sites);
            results.MergeFrom(dbConflicts);

            return results;
        }

        private bool IsSiteIdInUse(string siteId, string exceptIdentifier = null)
        {
            return _entityLoader.CountSitesBySiteId(siteId, exceptIdentifier) > 0;
        }

        private List<ValidationResult> ValidateNoFacilitiesAssigned(List<Site> sites)
        {
            var results = sites.Select(_ => new ValidationResult()).ToList();
            var identifiers = sites.Select(s => s.Identifier).Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
            if (!identifiers.Any())
            {
                return results;
            }

            var referencedIdentifiers = _entityLoader.GetFacilitiesBySiteIdentifiers(identifiers)
                .Select(f => f.SiteFk == null ? null : f.SiteFk.Site.Identifier)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .ToHashSet();

            for (int i = 0; i < sites.Count; i++)
            {
                if (referencedIdentifiers.Contains(sites[i].Identifier))
                {
                    results[i].AddFailReason(
                        SiteValidationHandler.SiteValidationField.SiteId,
                        "Can't remove site, since it still has facilities assigned to it. Please remove all the facilities assigned to this site before removing it.");
                }
            }

            return results;
        }

        private List<ValidationResult> ValidateBulkSiteIdsAgainstDatabase(List<Site> sites)
        {
            var results = sites.Select(_ => new ValidationResult()).ToList();

            var uniqueIds = sites
                .Select(s => s.SiteId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!uniqueIds.Any())
            {
                return results;
            }

            var batchIdentifiers = new HashSet<string>(
                sites.Select(s => s.Identifier).Where(id => !string.IsNullOrWhiteSpace(id)));

            var dbMatches = _entityLoader.GetSitesBySiteIds(uniqueIds);

            var externalConflictIds = dbMatches
                .Where(r => !batchIdentifiers.Contains(r.Identifier))
                .Select(r => r.SiteId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < sites.Count; i++)
            {
                var siteId = sites[i].SiteId;
                if (!string.IsNullOrWhiteSpace(siteId) && externalConflictIds.Contains(siteId))
                {
                    results[i].AddFailReason(
                        SiteValidationHandler.SiteValidationField.SiteId,
                        $"Site Id '{siteId}' is already in use.");
                }
            }

            return results;
        }

        private static List<ValidationResult> ValidateSiteIdDuplicatesInBatch(List<Site> sites)
        {
            var results = sites.Select(_ => new ValidationResult()).ToList();

            var duplicateIds = sites
                .Select((s, idx) => new { s.SiteId, Index = idx })
                .Where(x => !string.IsNullOrWhiteSpace(x.SiteId))
                .GroupBy(x => x.SiteId, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1);

            foreach (var group in duplicateIds)
            {
                foreach (var item in group)
                {
                    results[item.Index].AddFailReason(
                        SiteValidationHandler.SiteValidationField.SiteId,
                        $"Site Id '{item.SiteId}' is duplicated within the batch.");
                }
            }

            return results;
        }
    }
}

