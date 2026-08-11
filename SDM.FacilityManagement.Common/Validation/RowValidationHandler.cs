namespace Skyline.DataMiner.SDM.FacilityManagement.Validation
{
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Static validation handler for Row business rules.
    /// Contains pure validation logic without data access.
    /// </summary>
    public static class RowValidationHandler
    {
        public enum RowValidationField
        {
            RowId,
        }

        /// <summary>
        /// Validates that the Row id is not empty or whitespace.
        /// </summary>
        public static bool IsRowIdValid(Row entity, out ValidationResult result)
        {
            result = new ValidationResult();

            if (entity == null || string.IsNullOrWhiteSpace(entity.RowId))
            {
                result.AddFailReason(RowValidationField.RowId, "Row Id cannot be empty or whitespace.");
            }

            return result.IsValid;
        }
    }
}
