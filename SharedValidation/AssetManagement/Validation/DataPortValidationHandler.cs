namespace Skyline.DataMiner.SDM.AssetManagement.Common.Validation
{
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Static validation handler for DataPort business rules.
    /// Contains pure validation logic without data access.
    /// </summary>
    public static class DataPortValidationHandler
    {
        public enum DataPortValidationField
        {
            DataPort,
            Name,
            PortNumber,
            PortType,
            Asset,
            AddressInfo,
            PrimaryPort,
        }

        #region Info Validation

        /// <summary>
        /// Validates mandatory fields (Name, Port Number).
        /// </summary>
        public static bool AreMandatoryFieldsValid(DataPort dataPort, out ValidationResult result)
        {
            result = new ValidationResult();

            if (dataPort == null)
            {
                result.AddFailReason(DataPortValidationField.DataPort, "DataPort cannot be null.");
                return result.IsValid;
            }

            if (string.IsNullOrWhiteSpace(dataPort.DataPortInfo?.Name))
            {
                result.AddFailReason(DataPortValidationField.Name,
                    "DataPort Name cannot be empty.");
                return result.IsValid;
            }

            if (dataPort.DataPortInfo?.PortNumber == null)
            {
                result.AddFailReason(DataPortValidationField.PortNumber,
                    "DataPort Number must be provided.");
                return result.IsValid;
            }

            if (dataPort.DataPortInfo.PortNumber < 0)
            {
                result.AddFailReason(DataPortValidationField.PortNumber,
                    $"DataPort Number cannot be negative. Found: {dataPort.DataPortInfo.PortNumber}");
                return result.IsValid;
            }

            return result.IsValid;
        }

        /// <summary>
        /// Validates that DataPort has a valid Asset reference.
        /// </summary>
        public static bool IsAssetLinkValid(DataPort dataPort, out ValidationResult result)
        {
            result = new ValidationResult();

            if (dataPort == null)
            {
                result.AddFailReason(DataPortValidationField.DataPort, "DataPort cannot be null.");
                return result.IsValid;
            }

            if (dataPort.AssetFk?.Asset == null || !dataPort.AssetFk.Asset.HasValue())
            {
                result.AddFailReason(DataPortValidationField.Asset,
                    "DataPort must be linked to an Asset.");
                return result.IsValid;
            }

            return result.IsValid;
        }

        #endregion
    }
}