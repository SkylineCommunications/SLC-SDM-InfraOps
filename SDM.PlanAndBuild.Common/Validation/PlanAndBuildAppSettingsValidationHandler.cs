namespace Skyline.DataMiner.SDM.PlanAndBuild.Validation
{
    using Skyline.DataMiner.SDM.PlanAndBuild.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Static validation handler for PlanAndBuildAppSettings business rules.
    /// Mirrors InfraOpsShared.DOM_Classes.DOM.Applications.Plan_And_Build.Validation.AppSettingsValidationHandler,
    /// which currently defines no validation rules.
    /// </summary>
    public static class PlanAndBuildAppSettingsValidationHandler
    {
        public enum PlanAndBuildAppSettingsValidationField
        {
            PlanAndBuildAppSettings,
        }

        /// <summary>
        /// No business rules are enforced for PlanAndBuildAppSettings in production. Kept as a placeholder
        /// so future rules have a natural home and this module follows the same Validator/Handler pairing
        /// as the rest of the codebase.
        /// </summary>
        public static bool IsValid(PlanAndBuildAppSettings appSettings, out ValidationResult result)
        {
            result = new ValidationResult();

            if (appSettings == null)
            {
                result.AddFailReason(PlanAndBuildAppSettingsValidationField.PlanAndBuildAppSettings, "PlanAndBuildAppSettings cannot be null.");
            }

            return result.IsValid;
        }
    }
}
