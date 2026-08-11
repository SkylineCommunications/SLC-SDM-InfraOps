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
    }
}
