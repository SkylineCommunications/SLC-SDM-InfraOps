namespace Skyline.DataMiner.SDM.FacilityManagement.Validation
{
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Static validation handler for Facility business rules.
    /// Contains pure validation logic without data access.
    /// </summary>
    public static class FacilityValidationHandler
    {
        public enum FacilityValidationField
        {
            FacilityId,
        }

        /// <summary>
        /// Validates that the Facility id is not empty or whitespace.
        /// </summary>
        public static bool IsFacilityIdValid(Facility entity, out ValidationResult result)
        {
            result = new ValidationResult();

            if (entity == null || string.IsNullOrWhiteSpace(entity.FacilityId))
            {
                result.AddFailReason(FacilityValidationField.FacilityId, "Facility Id cannot be empty or whitespace.");
            }

            return result.IsValid;
        }
    }
}
