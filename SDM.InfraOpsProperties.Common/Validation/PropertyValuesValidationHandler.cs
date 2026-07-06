namespace Skyline.DataMiner.SDM.InfraOpsProperties.Validation
{
    using System;
    using System.Linq;

    using Skyline.DataMiner.SDM.InfraOpsProperties.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Static validation handler for PropertyValues business rules.
    /// Contains pure validation logic without data access.
    /// </summary>
    public static class PropertyValuesValidationHandler
    {
        public enum PropertyValuesValidationField
        {
            PropertyValues,
            LinkedObjectID,
            Scope,
            Values,
        }

        #region Info Validation

        public static bool IsLinkedObjectIDValid(PropertyValues propertyValues, out ValidationResult result)
        {
            result = new ValidationResult();

            if (propertyValues == null)
            {
                result.AddFailReason(PropertyValuesValidationField.PropertyValues, "PropertyValues cannot be null.");
                return result.IsValid;
            }

            if (propertyValues.LinkedObjectID == Guid.Empty)
            {
                result.AddFailReason(PropertyValuesValidationField.LinkedObjectID, "PropertyValues Linked Object ID cannot be empty.");
            }

            return result.IsValid;
        }

        public static bool IsScopeValid(PropertyValues propertyValues, out ValidationResult result)
        {
            result = new ValidationResult();

            if (propertyValues == null)
            {
                result.AddFailReason(PropertyValuesValidationField.PropertyValues, "PropertyValues cannot be null.");
                return result.IsValid;
            }

            if (string.IsNullOrWhiteSpace(propertyValues.Scope))
            {
                result.AddFailReason(PropertyValuesValidationField.Scope, "PropertyValues Scope cannot be empty or whitespace.");
            }

            return result.IsValid;
        }

        #endregion

        #region Values Validation

        public static bool IsValuesValid(PropertyValues propertyValues, out ValidationResult result)
        {
            result = new ValidationResult();

            if (propertyValues == null)
            {
                result.AddFailReason(PropertyValuesValidationField.PropertyValues, "PropertyValues cannot be null.");
                return result.IsValid;
            }

            var values = propertyValues.Values;
            if (values == null || values.Count == 0)
            {
                return result.IsValid;
            }

            if (values.Any(v => string.IsNullOrWhiteSpace(v?.PropertyName)))
            {
                result.AddFailReason(PropertyValuesValidationField.Values, "Every entry in Values must have a non-empty Property Name.");
                return result.IsValid;
            }

            var duplicateNames = values
                .Select(v => v.PropertyName)
                .GroupBy(name => name, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToList();

            if (duplicateNames.Count > 0)
            {
                result.AddFailReason(PropertyValuesValidationField.Values, $"Duplicate Property Name(s) found in Values: {string.Join(", ", duplicateNames)}.");
            }

            return result.IsValid;
        }

        #endregion
    }
}
