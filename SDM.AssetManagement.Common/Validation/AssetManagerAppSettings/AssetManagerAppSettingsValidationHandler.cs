namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Static validation handler for AssetManagerAppSettings business rules.
    /// No business rules are currently defined for this entity. Kept as a placeholder
    /// so future rules have a natural home and this module follows the same Validator/Handler
    /// pairing as the rest of the codebase.
    /// </summary>
    public static class AssetManagerAppSettingsValidationHandler
    {
        public enum AssetManagerAppSettingsValidationField
        {
            AssetManagerAppSettings,
        }

        /// <summary>
        /// No business rules are enforced for AssetManagerAppSettings in production.
        /// </summary>
        public static bool IsValid(AssetManagerAppSettings appSettings, out ValidationResult result)
        {
            result = new ValidationResult();

            if (appSettings == null)
            {
                result.AddFailReason(AssetManagerAppSettingsValidationField.AssetManagerAppSettings, "AssetManagerAppSettings cannot be null.");
            }

            return result.IsValid;
        }
    }
}