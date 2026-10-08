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
            Name,
            SiteId,
            Latitude,
            Longitude,
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

        public static bool IsFacilityNameValid(Facility entity, out ValidationResult result)
        {
            result = new ValidationResult();

            if (entity == null || string.IsNullOrWhiteSpace(entity.Name))
            {
                result.AddFailReason(FacilityValidationField.Name, "Facility Name cannot be empty or whitespace.");
            }

            return result.IsValid;
        }

        /// <summary>
        /// Validates that, when defined, the Facility latitude is between -90 and 90.
        /// </summary>
        public static bool IsFacilityLatitudeValid(Facility entity, out ValidationResult result)
        {
            if (entity?.Latitude == null)
            {
                result = new ValidationResult();
                return true;
            }

            return NumericValidators.ValidateRange(entity.Latitude.Value, -90d, 90d, FacilityValidationField.Latitude, out result);
        }

        /// <summary>
        /// Validates that, when defined, the Facility longitude is between -180 and 180.
        /// </summary>
        public static bool IsFacilityLongitudeValid(Facility entity, out ValidationResult result)
        {
            if (entity?.Longitude == null)
            {
                result = new ValidationResult();
                return true;
            }

            return NumericValidators.ValidateRange(entity.Longitude.Value, -180d, 180d, FacilityValidationField.Longitude, out result);
        }

        /// <summary>
        /// Validates the Facility name together with its coordinates.
        /// </summary>
        internal static bool IsFacilityNameAndCoordinatesValid(Facility entity, out ValidationResult result)
        {
            result = new ValidationResult();

            if (!IsFacilityNameValid(entity, out var nameResult))
            {
                result.AddFailuresFrom(nameResult);
            }

            if (!IsFacilityLatitudeValid(entity, out var latitudeResult))
            {
                result.AddFailuresFrom(latitudeResult);
            }

            if (!IsFacilityLongitudeValid(entity, out var longitudeResult))
            {
                result.AddFailuresFrom(longitudeResult);
            }

            return result.IsValid;
        }
    }
}
