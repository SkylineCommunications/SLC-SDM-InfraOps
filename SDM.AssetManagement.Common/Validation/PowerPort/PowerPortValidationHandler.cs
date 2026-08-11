namespace Skyline.DataMiner.SDM.AssetManagement.Common.Validation
{
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Static validation handler for PowerPort business rules.
    /// Contains pure validation logic without data access.
    /// </summary>
    public static class PowerPortValidationHandler
    {
        public enum PowerPortValidationField
        {
            PowerPort,
            Name,
            PortNumber,
            PortType,
            Asset,
            OutputType,
        }

        #region Info Validation

        /// <summary>
        /// Validates mandatory fields (Name, Port Number).
        /// </summary>
        public static bool AreMandatoryFieldsValid(PowerPort powerPort, out ValidationResult result)
        {
            result = new ValidationResult();

            if (powerPort == null)
            {
                result.AddFailReason(PowerPortValidationField.PowerPort, "PowerPort cannot be null.");
                return result.IsValid;
            }

            if (string.IsNullOrWhiteSpace(powerPort.PowerPortInfo?.Name))
            {
                result.AddFailReason(PowerPortValidationField.Name,
                    "PowerPort Name cannot be empty.");
                return result.IsValid;
            }

            if (powerPort.PowerPortInfo?.PortNumber == null)
            {
                result.AddFailReason(PowerPortValidationField.PortNumber,
                    "PowerPort Number must be provided.");
                return result.IsValid;
            }

            if (powerPort.PowerPortInfo.PortNumber < 0)
            {
                result.AddFailReason(PowerPortValidationField.PortNumber,
                    $"PowerPort Number cannot be negative. Found: {powerPort.PowerPortInfo.PortNumber}");
                return result.IsValid;
            }

            if (powerPort.PowerPortInfo?.OutputType == null)
            {
                result.AddFailReason(PowerPortValidationField.OutputType,
                    "PowerPort Output Type must be provided.");
                return result.IsValid;
            }

            return result.IsValid;
        }

        /// <summary>
        /// Validates that PowerPort has a valid Asset reference.
        /// </summary>
        public static bool IsAssetLinkValid(PowerPort powerPort, out ValidationResult result)
        {
            result = new ValidationResult();

            if (powerPort == null)
            {
                result.AddFailReason(PowerPortValidationField.PowerPort, "PowerPort cannot be null.");
                return result.IsValid;
            }

            if (powerPort.Asset == null || !powerPort.Asset.HasValue())
            {
                result.AddFailReason(PowerPortValidationField.Asset,
                    "PowerPort must be linked to an Asset.");
                return result.IsValid;
            }

            return result.IsValid;
        }

        #endregion
    }
}
