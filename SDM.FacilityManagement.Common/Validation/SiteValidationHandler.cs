namespace Skyline.DataMiner.SDM.FacilityManagement.Validation
{
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Static validation handler for Site business rules.
    /// Contains pure validation logic without data access.
    /// </summary>
    public static class SiteValidationHandler
    {
        public enum SiteValidationField
        {
            SiteId,
            Name,
            Latitude,
            Longitude,
        }

        /// <summary>
        /// Validates that the Site id is not empty or whitespace.
        /// </summary>
        public static bool IsSiteIdValid(Site site, out ValidationResult result)
        {
            result = new ValidationResult();

            if (site == null || string.IsNullOrWhiteSpace(site.SiteId))
            {
                result.AddFailReason(SiteValidationField.SiteId, "Site Id cannot be empty or whitespace.");
            }

            return result.IsValid;
        }

        public static bool IsSiteNameValid(Site entity, out ValidationResult result)
        {
            result = new ValidationResult();

            if (entity == null || string.IsNullOrWhiteSpace(entity.Name))
            {
                result.AddFailReason(SiteValidationField.Name, "Site Name cannot be empty or whitespace.");
            }

            return result.IsValid;
        }

        /// <summary>
        /// Validates that, when defined, the Site latitude is between -90 and 90.
        /// </summary>
        public static bool IsSiteLatitudeValid(Site entity, out ValidationResult result)
        {
            if (entity?.Latitude == null)
            {
                result = new ValidationResult();
                return true;
            }

            return NumericValidators.ValidateRange(entity.Latitude.Value, -90d, 90d, SiteValidationField.Latitude, out result);
        }

        /// <summary>
        /// Validates that, when defined, the Site longitude is between -180 and 180.
        /// </summary>
        public static bool IsSiteLongitudeValid(Site entity, out ValidationResult result)
        {
            if (entity?.Longitude == null)
            {
                result = new ValidationResult();
                return true;
            }

            return NumericValidators.ValidateRange(entity.Longitude.Value, -180d, 180d, SiteValidationField.Longitude, out result);
        }

        /// <summary>
        /// Validates the Site name together with its coordinates.
        /// </summary>
        internal static bool IsSiteNameAndCoordinatesValid(Site entity, out ValidationResult result)
        {
            result = new ValidationResult();

            if (!IsSiteNameValid(entity, out var nameResult))
            {
                result.AddFailuresFrom(nameResult);
            }

            if (!IsSiteLatitudeValid(entity, out var latitudeResult))
            {
                result.AddFailuresFrom(latitudeResult);
            }

            if (!IsSiteLongitudeValid(entity, out var longitudeResult))
            {
                result.AddFailuresFrom(longitudeResult);
            }

            return result.IsValid;
        }
    }
}
