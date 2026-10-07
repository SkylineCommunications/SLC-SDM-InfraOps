namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Static validation handler for History business rules.
    /// No business rules are currently defined for this entity. Kept as a placeholder
    /// so future rules have a natural home and this module follows the same Validator/Handler
    /// pairing as the rest of the codebase.
    /// </summary>
    public static class HistoryValidationHandler
    {
        public enum HistoryValidationField
        {
            History,
        }

        /// <summary>
        /// No business rules are enforced for History in production.
        /// </summary>
        public static bool IsValid(History history, out ValidationResult result)
        {
            result = new ValidationResult();

            if (history == null)
            {
                result.AddFailReason(HistoryValidationField.History, "History cannot be null.");
            }

            return result.IsValid;
        }
    }
}