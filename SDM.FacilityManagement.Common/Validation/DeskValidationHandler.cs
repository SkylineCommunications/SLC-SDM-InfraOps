namespace Skyline.DataMiner.SDM.FacilityManagement.Validation
{
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Static validation handler for Desk business rules.
    /// Contains pure validation logic without data access.
    /// </summary>
    public static class DeskValidationHandler
    {
        public enum DeskValidationField
        {
            DeskId,
        }

        /// <summary>
        /// Validates that the Desk id is not empty or whitespace.
        /// </summary>
        public static bool IsDeskIdValid(Desk entity, out ValidationResult result)
        {
            result = new ValidationResult();

            if (entity == null || string.IsNullOrWhiteSpace(entity.DeskID))
            {
                result.AddFailReason(DeskValidationField.DeskId, "Desk Id cannot be empty or whitespace.");
            }

            return result.IsValid;
        }
    }
}
