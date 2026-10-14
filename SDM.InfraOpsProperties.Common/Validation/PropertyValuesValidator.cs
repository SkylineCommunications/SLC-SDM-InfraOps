namespace Skyline.DataMiner.SDM.InfraOpsProperties.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.SDM.InfraOps.Common.Validation;
    using Skyline.DataMiner.SDM.InfraOpsProperties.Helpers;
    using Skyline.DataMiner.SDM.InfraOpsProperties.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Public validator service for PropertyValues validation, including data access for
    /// (LinkedObjectID, Scope, SubID) uniqueness checks.
    /// </summary>
    public class PropertyValuesValidator : ValidatorBase<PropertyValues>
    {
        private readonly IInfraOpsPropertiesApiHelper _helper;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyValuesValidator"/> class.
        /// </summary>
        /// <param name="helper">
        /// The InfraOps Properties API helper used to query existing PropertyValues for
        /// (LinkedObjectID, Scope, SubID) uniqueness checks.
        /// Note: this is captured by reference during <see cref="InfraOpsPropertiesApiHelper"/> construction, before
        /// its repositories are wired up.
        /// </param>
        public PropertyValuesValidator(IInfraOpsPropertiesApiHelper helper)
        {
            _helper = helper ?? throw new ArgumentNullException(nameof(helper));
        }

        #region PropertyValues Validation

        protected override List<ValidationResult> ValidateBulkNoDatabaseBusinessRules(List<PropertyValues> entities)
        {
            // ============================================================
            // PHASE 1: NO DATABASE ACCESS CHECKS (BUSINESS RULES)
            // ============================================================
            var results = entities.Select(ValidateInfo).ToList();

            for (int i = 0; i < entities.Count; i++)
            {
                results[i].AddFailuresFrom(ValidateValues(entities[i]));
            }

            return results;
        }

        protected override List<ValidationResult> ValidateBulkNoDatabaseBatchConflicts(List<PropertyValues> entities)
        {
            // ============================================================
            // PHASE 2: IN-MEMORY BATCH CONFLICT DETECTION (NO DATABASE)
            // ============================================================
            return ValidateBatchConflicts(entities);
        }

        protected override List<ValidationResult> ValidateBulkWithDatabase(List<PropertyValues> entities)
        {
            // Initialize results - same order as input
            var results = entities.Select(pv => new ValidationResult()).ToList();

            // ============================================================
            // PHASE 3: DATABASE ACCESS CHECKS (UNIQUENESS) + REMAINING RULES
            // ============================================================
            // Batch-fetch every (LinkedObjectID, Scope) combination that needs a uniqueness check in a single
            // big-OR query, instead of issuing one Read() query per entry in the loop below.
            var linkedObjectScopeKeys = entities
                .Where(pv => pv.ShouldValidateAny(pv.LinkedObjectIDField, pv.ScopeField, pv.SubIDField) &&
                             pv.LinkedObjectID != Guid.Empty &&
                             !string.IsNullOrWhiteSpace(pv.Scope))
                .Select(pv => (pv.LinkedObjectID, pv.Scope))
                .Distinct()
                .ToList();

            var existingByLinkedObjectAndScope = _helper.PropertyValues
                .GetByLinkedObjectIDsAndScopes(linkedObjectScopeKeys)
                .GroupBy(pv => (pv.LinkedObjectID, pv.Scope))
                .ToDictionary(g => g.Key, g => g.ToList());

            var existingPropertyIds = GetExistingPropertyIds(CollectReferencedPropertyIds(entities));

            for (int i = 0; i < entities.Count; i++)
            {
                results[i].AddFailuresFrom(ValidateUniqueness(entities[i], existingByLinkedObjectAndScope));
                results[i].AddFailuresFrom(ValidatePropertyReferences(entities[i], existingPropertyIds));
            }

            return results;
        }

        private ValidationResult ValidateInfo(PropertyValues propertyValues)
        {
            var result = new ValidationResult();

            if (propertyValues.ShouldValidate(propertyValues.LinkedObjectIDField) && !PropertyValuesValidationHandler.IsLinkedObjectIDValid(propertyValues, out var linkedObjectIdResult))
            {
                result.AddFailuresFrom(linkedObjectIdResult);
            }

            if (propertyValues.ShouldValidate(propertyValues.ScopeField) && !PropertyValuesValidationHandler.IsScopeValid(propertyValues, out var scopeResult))
            {
                result.AddFailuresFrom(scopeResult);
            }

            return result;
        }

        private ValidationResult ValidateValues(PropertyValues propertyValues)
        {
            var result = new ValidationResult();

            if (propertyValues.ShouldValidate(propertyValues.ValuesField) && !PropertyValuesValidationHandler.IsValuesValid(propertyValues, out var valuesResult))
            {
                result.AddFailuresFrom(valuesResult);
            }

            return result;
        }

        /// <summary>
        /// Validates that (LinkedObjectID, Scope, SubID) is unique across the persisted PropertyValues.
        /// A missing/null SubID is treated as its own distinct bucket (i.e. not equal to any specific SubID value),
        /// matching the legacy behavior where PropertyValues without a SubID were filtered separately from
        /// PropertyValues carrying a specific SubID.
        /// </summary>
        private ValidationResult ValidateUniqueness(PropertyValues propertyValues)
        {
            var result = new ValidationResult();

            if (!propertyValues.ShouldValidateAny(propertyValues.LinkedObjectIDField, propertyValues.ScopeField, propertyValues.SubIDField))
            {
                return result;
            }

            if (propertyValues.LinkedObjectID == Guid.Empty || string.IsNullOrWhiteSpace(propertyValues.Scope))
            {
                // Missing required key parts - already reported by ValidateInfo.
                return result;
            }

            if (IsComboInUse(propertyValues.LinkedObjectID, propertyValues.Scope, propertyValues.SubID, propertyValues.Identifier))
            {
                result.AddFailReason(PropertyValuesValidationHandler.PropertyValuesValidationField.PropertyValues,
                    $"PropertyValues for Linked Object '{propertyValues.LinkedObjectID}', Scope '{propertyValues.Scope}'" +
                    (propertyValues.SubID == null ? " (no SubID)" : $", SubID '{propertyValues.SubID}'") +
                    " already exist.");
            }

            return result;
        }

        private bool IsComboInUse(Guid linkedObjectId, string scope, string subId, string exceptIdentifier)
        {
            FilterElement<PropertyValues> filter = PropertyValuesExposers.LinkedObjectID.Equal(linkedObjectId)
                .AND(PropertyValuesExposers.Scope.Equal(scope));

            var candidates = _helper.PropertyValues.Read(filter);

            return candidates.Any(pv =>
                string.Equals(pv.SubID, subId, StringComparison.Ordinal) &&
                (string.IsNullOrEmpty(exceptIdentifier) || !string.Equals(pv.Identifier, exceptIdentifier, StringComparison.Ordinal)));
        }

        /// <summary>
        /// Batch variant of <see cref="ValidateUniqueness(PropertyValues)"/>, used by <see cref="ValidateBulk"/>.
        /// Checks the pre-fetched <paramref name="existingByLinkedObjectAndScope"/> lookup (built once for the
        /// whole batch via <see cref="IPropertyValuesRepository.GetByLinkedObjectIDsAndScopes"/>) instead of
        /// issuing its own DB query.
        /// </summary>
        private ValidationResult ValidateUniqueness(PropertyValues propertyValues, Dictionary<(Guid LinkedObjectID, string Scope), List<PropertyValues>> existingByLinkedObjectAndScope)
        {
            var result = new ValidationResult();

            if (!propertyValues.ShouldValidateAny(propertyValues.LinkedObjectIDField, propertyValues.ScopeField, propertyValues.SubIDField))
            {
                return result;
            }

            if (propertyValues.LinkedObjectID == Guid.Empty || string.IsNullOrWhiteSpace(propertyValues.Scope))
            {
                // Missing required key parts - already reported by ValidateInfo.
                return result;
            }

            if (IsComboInUse(propertyValues.LinkedObjectID, propertyValues.Scope, propertyValues.SubID, propertyValues.Identifier, existingByLinkedObjectAndScope))
            {
                result.AddFailReason(PropertyValuesValidationHandler.PropertyValuesValidationField.PropertyValues,
                    $"PropertyValues for Linked Object '{propertyValues.LinkedObjectID}', Scope '{propertyValues.Scope}'" +
                    (propertyValues.SubID == null ? " (no SubID)" : $", SubID '{propertyValues.SubID}'") +
                    " already exist.");
            }

            return result;
        }

        private static bool IsComboInUse(Guid linkedObjectId, string scope, string subId, string exceptIdentifier, Dictionary<(Guid LinkedObjectID, string Scope), List<PropertyValues>> existingByLinkedObjectAndScope)
        {
            if (existingByLinkedObjectAndScope == null || !existingByLinkedObjectAndScope.TryGetValue((linkedObjectId, scope), out var candidates))
            {
                return false;
            }

            return candidates.Any(pv =>
                string.Equals(pv.SubID, subId, StringComparison.Ordinal) &&
                (string.IsNullOrEmpty(exceptIdentifier) || !string.Equals(pv.Identifier, exceptIdentifier, StringComparison.Ordinal)));
        }

        private ValidationResult ValidatePropertyReferences(PropertyValues propertyValues)
        {
            return ValidatePropertyReferences(propertyValues, GetExistingPropertyIds(CollectReferencedPropertyIds(new List<PropertyValues> { propertyValues })));
        }

        private ValidationResult ValidatePropertyReferences(PropertyValues propertyValues, HashSet<string> existingPropertyIds)
        {
            var result = new ValidationResult();

            if (!propertyValues.ShouldValidate(propertyValues.ValuesField) || propertyValues.Values == null)
            {
                return result;
            }

            foreach (var propertyId in propertyValues.Values
                .Where(value => value != null && IsReferenceSet(value.PropertyId))
                .Select(value => value.PropertyId.Identifier))
            {
                if (existingPropertyIds?.Contains(propertyId) != true)
                {
                    result.AddFailReason(PropertyValuesValidationHandler.PropertyValuesValidationField.Values, $"Referenced Property '{propertyId}' does not exist.");
                }
            }

            return result;
        }

        private HashSet<string> GetExistingPropertyIds(IEnumerable<string> propertyIds)
        {
            var keys = propertyIds?.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList() ?? new List<string>();

            return _helper.Properties.GetByIdentifiers(keys)
                .Select(property => property.Identifier)
                .ToHashSet();
        }

        private static List<string> CollectReferencedPropertyIds(List<PropertyValues> propertyValuesList)
        {
            return propertyValuesList
                .Where(pv => pv.ShouldValidate(pv.ValuesField) && pv.Values != null)
                .SelectMany(pv => pv.Values)
                .Where(value => value != null && IsReferenceSet(value.PropertyId))
                .Select(value => value.PropertyId.Identifier)
                .ToList();
        }

        /// <summary>
        /// Detects (LinkedObjectID, Scope, SubID) conflicts among the PropertyValues of a single batch
        /// (in-memory only, no database access). A missing/null SubID is treated as its own distinct bucket,
        /// consistent with <see cref="IsComboInUse(Guid, string, string, string)"/>. Result at index i corresponds to entry at index i.
        /// </summary>
        public List<ValidationResult> ValidateBatchConflicts(List<PropertyValues> propertyValuesList)
        {
            var results = propertyValuesList.Select(pv => new ValidationResult()).ToList();

            var comboGroups = propertyValuesList
                .Select((propertyValues, index) => new { propertyValues, index })
                .Where(x => x.propertyValues.ShouldValidateAny(x.propertyValues.LinkedObjectIDField, x.propertyValues.ScopeField, x.propertyValues.SubIDField) &&
                            x.propertyValues.LinkedObjectID != Guid.Empty &&
                            !string.IsNullOrWhiteSpace(x.propertyValues.Scope))
                .GroupBy(x => new
                {
                    x.propertyValues.LinkedObjectID,
                    Scope = x.propertyValues.Scope.ToUpperInvariant(),
                    x.propertyValues.SubID,
                })
                .Where(g => g.Count() > 1);

            foreach (var group in comboGroups)
            {
                foreach (var item in group)
                {
                    results[item.index].AddFailReason(PropertyValuesValidationHandler.PropertyValuesValidationField.PropertyValues,
                        $"PropertyValues for Linked Object '{item.propertyValues.LinkedObjectID}', Scope '{item.propertyValues.Scope}'" +
                        (item.propertyValues.SubID == null ? " (no SubID)" : $", SubID '{item.propertyValues.SubID}'") +
                        " is duplicated within the validation batch.");
                }
            }

            return results;
        }

        private static bool IsReferenceSet(SdmObjectReference<Property> reference)
        {
            return reference.HasValue();
        }

        #endregion
    }
}
