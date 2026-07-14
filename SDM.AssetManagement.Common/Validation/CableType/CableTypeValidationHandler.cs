namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using System.Linq;

    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Static validation handler for CableType business rules.
    /// Contains pure validation logic without data access.
    /// </summary>
    public static class CableTypeValidationHandler
    {
        public enum CableTypeValidationField
        {
            Name,
            Category,
        }

        #region Name Validation

        /// <summary>
        /// Validates that the CableType name is not empty or whitespace.
        /// </summary>
        public static bool IsCableTypeNameValid(string name, out ValidationResult result)
        {
            result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(name))
            {
                result.AddFailReason(CableTypeValidationField.Name, "Cable Type Name cannot be empty or whitespace.");
                return result.IsValid;
            }

            return result.IsValid;
        }

        #endregion

        #region Category Validation

        /// <summary>
        /// Validates that the CableType has at least one category assigned.
        /// </summary>
        public static bool IsCableTypeCategoriesValid(CableType cableType, out ValidationResult result)
        {
            result = new ValidationResult();

            if (cableType == null)
            {
                result.AddFailReason(CableTypeValidationField.Category, "Cable Type must be provided.");
                return result.IsValid;
            }

            if (cableType.CategoryLinks?.Categories == null || !cableType.CategoryLinks.Categories.Any())
            {
                result.AddFailReason(CableTypeValidationField.Category, "Cable Type must have at least one category.");
            }

            return result.IsValid;
        }

        #endregion
    }
}