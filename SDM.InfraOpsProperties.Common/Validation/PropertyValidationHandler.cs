namespace Skyline.DataMiner.SDM.InfraOpsProperties.Validation
{
    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM.InfraOpsProperties.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Static validation handler for Property business rules.
    /// Contains pure validation logic without data access.
    /// </summary>
    public static class PropertyValidationHandler
    {
        public enum PropertyValidationField
        {
            Property,
            Name,
            Scope,
            StringSizeLimit,
            Options,
        }

        #region Info Validation

        public static bool IsNameValid(Property property, out ValidationResult result)
        {
            result = new ValidationResult();

            if (property == null)
            {
                result.AddFailReason(PropertyValidationField.Property, "Property cannot be null.");
                return result.IsValid;
            }

            if (string.IsNullOrWhiteSpace(property.Name))
            {
                result.AddFailReason(PropertyValidationField.Name, "Property Name cannot be empty or whitespace.");
            }

            return result.IsValid;
        }

        public static bool IsScopeValid(Property property, out ValidationResult result)
        {
            result = new ValidationResult();

            if (property == null)
            {
                result.AddFailReason(PropertyValidationField.Property, "Property cannot be null.");
                return result.IsValid;
            }

            if (string.IsNullOrWhiteSpace(property.Scope))
            {
                result.AddFailReason(PropertyValidationField.Scope, "Property Scope cannot be empty or whitespace.");
            }

            return result.IsValid;
        }

        #endregion

        #region String Constraints Validation

        public static bool IsStringSizeLimitValid(Property property, out ValidationResult result)
        {
            result = new ValidationResult();

            if (property == null)
            {
                result.AddFailReason(PropertyValidationField.Property, "Property cannot be null.");
                return result.IsValid;
            }

            var stringSizeLimit = property.StringSizeLimit;

            if (stringSizeLimit != null && stringSizeLimit <= 0)
            {
                result.AddFailReason(PropertyValidationField.StringSizeLimit, "Property String Size Limit must be greater than 0 when defined.");
            }

            return result.IsValid;
        }

        #endregion

        #region Discrete Constraints Validation

        public static bool IsOptionsValid(Property property, out ValidationResult result)
        {
            result = new ValidationResult();

            if (property == null)
            {
                result.AddFailReason(PropertyValidationField.Property, "Property cannot be null.");
                return result.IsValid;
            }

            var isDiscrete = property.PropertyType == InfraopsProperties.Enums.PropertyTypeEnum.Discrete;
            var hasOptions = property.Options != null && property.Options.Count > 0;

            if (isDiscrete && !hasOptions)
            {
                result.AddFailReason(PropertyValidationField.Options, "Property Options cannot be empty when Property Type is 'Discrete'.");
            }
            else if (!isDiscrete && hasOptions)
            {
                result.AddFailReason(PropertyValidationField.Options, "Property Options must be empty when Property Type is not 'Discrete'.");
            }

            return result.IsValid;
        }

        #endregion
    }
}
