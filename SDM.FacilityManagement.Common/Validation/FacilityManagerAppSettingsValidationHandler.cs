namespace Skyline.DataMiner.SDM.FacilityManagement.Validation
{
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Static validation handler for FacilityManagerAppSettings business rules.
    /// Mirrors InfraOpsShared FacilityManagerAppSettingsValidationHandler,
    /// which currently defines no validation rules.
    /// </summary>
    public static class FacilityManagerAppSettingsValidationHandler
    {
        public enum FacilityManagerAppSettingsValidationField
        {
            FacilityManagerAppSettings,
        }

        /// <summary>
        /// No business rules are enforced for FacilityManagerAppSettings in production. Kept as a placeholder
        /// so future rules have a natural home and this module follows the same Validator/Handler pairing
        /// as the rest of the codebase.
        /// </summary>
        public static bool IsValid(FacilityManagerAppSettings appSettings, out ValidationResult result)
        {
            result = new ValidationResult();

            if (appSettings == null)
            {
                result.AddFailReason(FacilityManagerAppSettingsValidationField.FacilityManagerAppSettings, "FacilityManagerAppSettings cannot be null.");
            }

            return result.IsValid;
        }
    }
}
