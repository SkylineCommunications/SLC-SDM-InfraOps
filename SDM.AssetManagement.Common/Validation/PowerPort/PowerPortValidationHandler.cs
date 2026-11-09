namespace Skyline.DataMiner.SDM.AssetManagement.Common.Validation
{
    using System.Collections.Generic;

    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.AssetManagement.Validation;
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
            ConnectionConflict,
        }

        #region Connection Validation

        /// <summary>
        /// Validates that a changed output type does not invalidate the existing connections of the port
        /// (source ports cannot be Input-only, destination ports cannot be Output-only).
        /// Failures use <see cref="PowerPortValidationField.ConnectionConflict"/> so callers can tell them
        /// apart from invalid input and, for example, offer to delete the connections first.
        /// </summary>
        public static bool IsOutputTypeCompatibleWithConnections(PowerPort powerPort, IEnumerable<Connection> connections, out ValidationResult result)
        {
            result = new ValidationResult();

            if (powerPort == null || powerPort.IsNew || powerPort.PowerPortInfo.OutputTypeField.Changed != true)
            {
                return true;
            }

            connections.CountDirectionConflicts(powerPort.Identifier, powerPort.PowerPortInfo.OutputType, out var asSource, out var asDestination);

            if (asSource > 0)
            {
                result.AddFailReason(
                    PowerPortValidationField.ConnectionConflict,
                    $"The port is the source of {asSource} connection(s) and cannot be set to Input. Delete the connections first.");
            }

            if (asDestination > 0)
            {
                result.AddFailReason(
                    PowerPortValidationField.ConnectionConflict,
                    $"The port is the destination of {asDestination} connection(s) and cannot be set to Output. Delete the connections first.");
            }

            return result.IsValid;
        }

        #endregion

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

            if (string.IsNullOrWhiteSpace(powerPort.PowerPortInfo.Name))
            {
                result.AddFailReason(PowerPortValidationField.Name,
                    "PowerPort Name cannot be empty.");
                return result.IsValid;
            }

            if (powerPort.PowerPortInfo.PortNumber == null)
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

            if (powerPort.PowerPortInfo.OutputType == null)
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
