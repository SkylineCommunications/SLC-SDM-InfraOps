namespace Skyline.DataMiner.SDM.FacilityManagement.Validation
{
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Static validation handler for Zone business rules.
    /// Contains pure validation logic without data access.
    /// </summary>
    public static class ZoneValidationHandler
    {
        public enum ZoneValidationField
        {
            ZoneId,
            Name,
            CoolingCapacity,
            RoomId,
        }

        /// <summary>
        /// Validates that the Zone id is not empty or whitespace.
        /// </summary>
        public static bool IsZoneIdValid(Zone entity, out ValidationResult result)
        {
            result = new ValidationResult();

            if (entity == null || string.IsNullOrWhiteSpace(entity.ZoneId))
            {
                result.AddFailReason(ZoneValidationField.ZoneId, "Zone Id cannot be empty or whitespace.");
            }

            return result.IsValid;
        }

        public static bool IsZoneNameValid(Zone entity, out ValidationResult result)
        {
            result = new ValidationResult();

            if (entity == null || string.IsNullOrWhiteSpace(entity.Name))
            {
                result.AddFailReason(ZoneValidationField.Name, "Zone Name cannot be empty or whitespace.");
            }

            return result.IsValid;
        }

        public static bool IsCoolingCapacityValid(Zone entity, out ValidationResult result)
        {
            result = new ValidationResult();
            if (entity == null || entity.ZoneCapacity?.CoolingCapacity == null || entity.ZoneCapacity.CoolingCapacity < 0)
            {
                result.AddFailReason(ZoneValidationField.CoolingCapacity, "Cooling Capacity must have a value greater than or equal to 0.");
            }
            return result.IsValid;
        }
    }
}
