namespace Skyline.DataMiner.SDM.FacilityManagement.Validation
{
    using System;

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

            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (string.IsNullOrWhiteSpace(entity.ZoneId))
            {
                result.AddFailReason(ZoneValidationField.ZoneId, "Zone Id cannot be empty or whitespace.");
            }

            return result.IsValid;
        }

        public static bool IsZoneNameValid(Zone entity, out ValidationResult result)
        {
            result = new ValidationResult();

            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (string.IsNullOrWhiteSpace(entity.Name))
            {
                result.AddFailReason(ZoneValidationField.Name, "Zone Name cannot be empty or whitespace.");
            }

            return result.IsValid;
        }

        public static bool IsCoolingCapacityValid(Zone entity, out ValidationResult result)
        {
            result = new ValidationResult();

            if(entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (!entity.ZoneCapacity.CoolingCapacity.HasValue)
            {
                result.AddFailReason(ZoneValidationField.CoolingCapacity, "Zone cooling capacity must be defined.");
            }
            else if (!NumericValidators.ValidateNonNegative(entity.ZoneCapacity.CoolingCapacity.Value, ZoneValidationField.CoolingCapacity, out var coolingCapacityValidationResult))
            {
                result.AddFrom(coolingCapacityValidationResult);
            }

            return result.IsValid;
        }
    }
}
