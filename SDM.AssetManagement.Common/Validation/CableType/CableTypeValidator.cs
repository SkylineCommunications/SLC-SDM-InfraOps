namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Common.Services;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Public validator service for CableType validation with comprehensive error handling.
    /// </summary>
    public class CableTypeValidator : ValidatorBase<CableType>
    {
        private readonly SdmEntityLoader _entityLoader;
        private readonly Validator<CableType> _validationPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="CableTypeValidator"/> class.
        /// </summary>
        /// <param name="entityLoader">The entity loader for querying cable types.</param>
        public CableTypeValidator(SdmEntityLoader entityLoader)
        {
            _entityLoader = entityLoader ?? throw new ArgumentNullException(nameof(entityLoader));
            _validationPipeline = BuildValidationPipeline();
        }

        /// <summary>
        /// Validates a CableType and returns a ValidationResult.
        /// Collects all errors without throwing exceptions.
        /// <para><b>Not suitable for bulk scenarios</b>: issues one DB query per item. Use <see cref="ValidateBulk"/> instead.</para>
        /// </summary>
        protected override ValidationResult Validate(CableType cableType)
        {
            if (cableType == null)
            {
                throw new ArgumentNullException(nameof(cableType));
            }

            return _validationPipeline.Validate(cableType);
        }

        /// <summary>
        /// Validates a CableType and throws a ValidationException if invalid.
        /// Use this when you want fail-fast behavior.
        /// <para><b>Not suitable for bulk scenarios</b>: issues one DB query per item. Use <see cref="ValidateBulk"/> instead.</para>
        /// </summary>
        public void ValidateAndThrow(CableType cableType)
        {
            _validationPipeline.ValidateAndThrow(cableType);
        }

        /// <summary>
        /// Validates with a custom error handling callback.
        /// <para><b>Not suitable for bulk scenarios</b>: issues one DB query per item. Use <see cref="ValidateBulk"/> instead.</para>
        /// </summary>
        public ValidationResult ValidateWithHandler(CableType cableType, Action<ValidationResult> onError)
        {
            return _validationPipeline.ValidateWithHandler(cableType, onError);
        }

        /// <summary>
        /// Validates name uniqueness — used for real-time UI validation.
        /// <para><b>Not suitable for bulk scenarios</b>: issues one DB query per call. Use <see cref="ValidateBulk"/> instead.</para>
        /// </summary>
        public ValidationResult IsCableTypeNameValid(string name, string exceptIdentifier = null)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(name))
            {
                result.AddFailReason(CableTypeValidationHandler.CableTypeValidationField.Name,
                    "Cable Type Name cannot be empty or whitespace.");
                return result;
            }

            if (IsNameInUse(name, exceptIdentifier))
            {
                result.AddFailReason(CableTypeValidationHandler.CableTypeValidationField.Name,
                    $"Cable Type Name '{name}' is already in use.");
            }

            return result;
        }

        /// <summary>
        /// Validates name uniqueness for the specified <see cref="CableType"/> instance.
        /// Excludes the current cable type identifier from the uniqueness check.
        /// <para><b>Not suitable for bulk scenarios</b>: issues one DB query per call. Use <see cref="ValidateBulk"/> instead.</para>
        /// </summary>
        public ValidationResult IsCableTypeNameValid(CableType cableType)
        {
            return IsCableTypeNameValid(cableType.Name, cableType.Identifier);
        }

        #region Pipeline Construction

        private Validator<CableType> BuildValidationPipeline()
        {
            // Critical validations - stop on failure
            var criticalValidations = Validator<CableType>
                .Create(ValidateCriticalFields)
                .StopOnFailure();

            // Standard validations - collect all errors
            var standardValidations = Validator<CableType>
                .Create(ValidateCategories);

            return criticalValidations.AndThen(standardValidations);
        }

        #endregion

        #region Validation Methods

        private ValidationResult ValidateCriticalFields(CableType cableType)
        {
            return IsCableTypeNameValid(cableType);
        }

        private ValidationResult ValidateCategories(CableType cableType)
        {
            var result = new ValidationResult();

            if (!CableTypeValidationHandler.IsCableTypeCategoriesValid(cableType, out var categoriesResult))
            {
                result.AddFailuresFrom(categoriesResult);
            }

            return result;
        }

        #endregion

        #region Helper Methods

        private bool IsNameInUse(string name, string exceptIdentifier = null)
        {
            return _entityLoader.CountCableTypesByName(name, exceptIdentifier) > 0;
        }

        #endregion

        #region Bulk Validation

        /// <summary>
        /// Validates multiple CableTypes in bulk with optimized performance.
        /// Returns validation results in the same order as the input cable types.
        /// Result at index i corresponds to cable type at index i.
        /// </summary>
        protected override List<ValidationResult> ValidateBulk(List<CableType> cableTypes)
        {
            if (cableTypes == null || !cableTypes.Any())
            {
                return new List<ValidationResult>();
            }

            var results = cableTypes.Select(_ => new ValidationResult()).ToList();

            // ============================================================
            // PHASE 1: NO DATABASE ACCESS CHECKS (BUSINESS RULES)
            // Name format (not empty) + categories — no DB calls.
            // ============================================================
            for (int i = 0; i < cableTypes.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(cableTypes[i].Name))
                {
                    results[i].AddFailReason(
                        CableTypeValidationHandler.CableTypeValidationField.Name,
                        "Cable Type Name cannot be empty or whitespace.");
                }

                results[i].AddFailuresFrom(ValidateCategories(cableTypes[i]));
            }

            if (results.AnyInvalid())
            {
                return results;
            }

            // ============================================================
            // PHASE 2: IN-MEMORY BATCH CONFLICT DETECTION
            // ============================================================
            var batchConflicts = ValidateNameDuplicatesInBatch(cableTypes);
            results.MergeFrom(batchConflicts);

            if (results.AnyInvalid())
            {
                return results;
            }

            // ============================================================
            // PHASE 2.5: BULK NAME UNIQUENESS CHECK AGAINST DATABASE
            // One OR-based query via Tools.RetrieveBigOrFilter — no large AND filter.
            // ============================================================
            var nameDbConflicts = ValidateBulkNamesAgainstDatabase(cableTypes);
            results.MergeFrom(nameDbConflicts);

            return results;
        }

        protected override ValidationResult ValidateForDelete(CableType cableType)
        {
            if (cableType == null)
            {
                throw new ArgumentNullException(nameof(cableType));
            }

            return ValidateNotInUseWhenDeleted(new List<CableType> { cableType })[0];
        }

        protected override List<ValidationResult> ValidateBulkForDelete(List<CableType> cableTypes)
        {
            if (cableTypes == null || !cableTypes.Any())
            {
                return new List<ValidationResult>();
            }

            return ValidateNotInUseWhenDeleted(cableTypes);
        }

        private List<ValidationResult> ValidateNotInUseWhenDeleted(List<CableType> cableTypes)
        {
            var results = cableTypes.Select(_ => new ValidationResult()).ToList();

            var identifiers = cableTypes
                .Select(c => c.Identifier)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            var cableTypeIdsUsedByConnections = _entityLoader.GetConnectionsByCableTypeIds(identifiers)
                .Where(connection => connection.CableType.HasValue())
                .Select(connection => connection.CableType.Identifier)
                .ToHashSet();

            for (int i = 0; i < cableTypes.Count; i++)
            {
                if (cableTypeIdsUsedByConnections.Contains(cableTypes[i].Identifier))
                {
                    results[i].AddFailReason(
                        CableTypeValidationHandler.CableTypeValidationField.Connection,
                        "There are still connections using this cable type. Please remove them first.");
                }
            }

            var remainingIdentifiers = cableTypes
                .Select((cableType, index) => new { CableType = cableType, Index = index })
                .Where(x => results[x.Index].IsValid)
                .Select(x => x.CableType.Identifier)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            if (!remainingIdentifiers.Any())
            {
                return results;
            }

            var cableTypeIdsUsedByPortTypes = _entityLoader.GetPortTypesByCableTypeIds(remainingIdentifiers)
                .SelectMany(portType => portType.CableFKs.CableTypeFks ?? new List<SdmObjectReference<CableType>>())
                .Where(reference => reference.HasValue())
                .Select(reference => reference.Identifier)
                .ToHashSet();

            for (int i = 0; i < cableTypes.Count; i++)
            {
                if (results[i].IsValid && cableTypeIdsUsedByPortTypes.Contains(cableTypes[i].Identifier))
                {
                    results[i].AddFailReason(
                        CableTypeValidationHandler.CableTypeValidationField.PortType,
                        "There are still port types using this cable type as compatibility. Please remove them first.");
                }
            }

            return results;
        }

        private List<ValidationResult> ValidateBulkNamesAgainstDatabase(List<CableType> cableTypes)
        {
            var results = cableTypes.Select(_ => new ValidationResult()).ToList();

            var uniqueNames = cableTypes
                .Select(c => c.Name)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!uniqueNames.Any())
            {
                return results;
            }

            var batchIds = new HashSet<string>(
                cableTypes.Select(c => c.Identifier).Where(id => !string.IsNullOrWhiteSpace(id)));

            var dbMatches = _entityLoader.GetCableTypesByNames(uniqueNames);

            var externalConflictNames = dbMatches
                .Where(r => !batchIds.Contains(r.Identifier))
                .Select(r => r.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < cableTypes.Count; i++)
            {
                var name = cableTypes[i].Name;
                if (!string.IsNullOrWhiteSpace(name) && externalConflictNames.Contains(name))
                {
                    results[i].AddFailReason(
                        CableTypeValidationHandler.CableTypeValidationField.Name,
                        $"Cable Type Name '{name}' is already in use.");
                }
            }

            return results;
        }

        private static List<ValidationResult> ValidateNameDuplicatesInBatch(List<CableType> cableTypes)
        {
            var results = cableTypes.Select(_ => new ValidationResult()).ToList();

            var duplicateNames = cableTypes
                .Select((ct, idx) => new { ct.Name, Index = idx })
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1);

            foreach (var group in duplicateNames)
            {
                foreach (var item in group)
                {
                    results[item.Index].AddFailReason(
                        CableTypeValidationHandler.CableTypeValidationField.Name,
                        $"Cable Type Name '{item.Name}' is duplicated within the batch.");
                }
            }

            return results;
        }

        #endregion
    }
}
