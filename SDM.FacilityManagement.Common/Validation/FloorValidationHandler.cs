namespace Skyline.DataMiner.SDM.FacilityManagement.Validation
{
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Static validation handler for Floor business rules.
    /// Contains pure validation logic without data access.
    /// </summary>
    public static class FloorValidationHandler
    {
        public enum FloorValidationField
        {
            FloorId,
        }

        /// <summary>
        /// Validates that the Floor id is not empty or whitespace.
        /// </summary>
        public static bool IsFloorIdValid(Floor entity, out ValidationResult result)
        {
            result = new ValidationResult();

            if (entity == null || string.IsNullOrWhiteSpace(entity.FloorId))
            {
                result.AddFailReason(FloorValidationField.FloorId, "Floor Id cannot be empty or whitespace.");
            }

            return result.IsValid;
        }
    }
}
