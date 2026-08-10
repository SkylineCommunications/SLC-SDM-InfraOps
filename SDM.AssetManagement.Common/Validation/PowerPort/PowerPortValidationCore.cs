namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.SDM.AssetManagement.Common.Validation;
    using Skyline.DataMiner.SDM.AssetManagement.Extensions;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Common.Services;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    using static Skyline.DataMiner.SDM.AssetManagement.Common.Validation.PowerPortValidationHandler;

    /// <summary>
    /// Central validation logic shared between single and bulk PowerPort validation.
    /// Separated into no-database and database-access methods for optimal performance.
    /// </summary>
    internal class PowerPortValidationCore
    {
        private readonly SdmEntityLoader _entityLoader;

        public PowerPortValidationCore(SdmEntityLoader entityLoader)
        {
            _entityLoader = entityLoader;
        }

        #region No Database Access Validation

        /// <summary>
        /// Validates PowerPort without database access (business rules only).
        /// </summary>
        public ValidationResult ValidateWithoutDatabaseAccess(PowerPort powerPort)
        {
            var result = new ValidationResult();

            // Mandatory fields
            if (!PowerPortValidationHandler.AreMandatoryFieldsValid(powerPort, out var mandatoryResult))
            {
                result.AddFailuresFrom(mandatoryResult);
                return result; // Stop if mandatory fields fail
            }

            // Asset link
            if (!PowerPortValidationHandler.IsAssetLinkValid(powerPort, out var assetLinkResult))
                result.AddFailuresFrom(assetLinkResult);

            return result;
        }

        #endregion

        #region Database Access Validation

        /// <summary>
        /// Validates PowerPort with database access (PortType, Asset context).
        /// Only called after no-database checks pass.
        /// </summary>
        public ValidationResult ValidateWithDatabaseAccess(PowerPort powerPort)
        {
            var result = new ValidationResult();

            // Port Type validation
            result.AddFailuresFrom(ValidatePortType(powerPort));
            if (!result.IsValid) return result;

            // Asset context validation (port number uniqueness)
            if (powerPort.Asset.HasValue())
            {
                result.AddFailuresFrom(ValidateAssetContext(powerPort));
            }

            return result;
        }

        /// <summary>
        /// Validates Port Type (must exist and be a Power Port Type).
        /// </summary>
        private ValidationResult ValidatePortType(PowerPort powerPort)
        {
            if (!powerPort.PowerPortInfo.PortType.HasValue())
            {
                // Passing null intentionally routes through the required-field failure in ValidatePortTypeAgainst.
                return ValidatePortTypeAgainst(powerPort, null);
            }

            try
            {
                var portType = _entityLoader.LoadPortType(powerPort.PowerPortInfo.PortType);
                return ValidatePortTypeAgainst(powerPort, portType);
            }
            catch (Exception ex)
            {
                var result = new ValidationResult();
                result.AddFailReason(PowerPortValidationField.PortType,
                    $"Error validating Port Type: {ex.Message}");
                return result;
            }
        }

        /// <summary>
        /// Validates a PowerPort's Port Type reference against an already-loaded PortType
        /// (or null when the referenced type could not be found). Pure in-memory checks,
        /// so it can be reused by the bulk path after a batched port-type load.
        /// </summary>
        public ValidationResult ValidatePortTypeAgainst(PowerPort powerPort, PortType loadedPortType)
        {
            var result = new ValidationResult();

            if (!powerPort.PowerPortInfo.PortType.HasValue())
            {
                result.AddFailReason(PowerPortValidationField.PortType,
                    "Port Type cannot be empty.");
                return result;
            }

            if (loadedPortType == null)
            {
                result.AddFailReason(PowerPortValidationField.PortType,
                    $"Port Type not found. Referenced Port Type '{powerPort.PowerPortInfo.PortType.Identifier}' does not exist.");
                return result;
            }

            if (!loadedPortType.IsPowerPortType())
            {
                result.AddFailReason(PowerPortValidationField.PortType,
                    "Port Type must be a Power Port Type.");
                return result;
            }

            return result;
        }

        /// <summary>
        /// Validates PowerPort in context of its Asset.
        /// </summary>
        private ValidationResult ValidateAssetContext(PowerPort powerPort)
        {
            var result = new ValidationResult();

            try
            {
                var asset = _entityLoader.LoadAsset(powerPort.Asset);
                if (asset == null)
                {
                    result.AddFailReason(PowerPortValidationField.Asset,
                        $"Referenced Asset '{powerPort.Asset.Identifier}' does not exist.");
                    return result;
                }

                // Load all ports for asset
                var allPorts = _entityLoader.LoadPowerPorts(asset)
                    .Where(p => p.Identifier != powerPort.Identifier)
                    .ToList();

                allPorts.Add(powerPort);

                // Validate collection (uniqueness)
                result.AddFailuresFrom(ValidatePowerPortCollection(allPorts));
            }
            catch (Exception ex)
            {
                result.AddFailReason(PowerPortValidationField.Asset,
                    $"Error validating Asset context: {ex.Message}");
            }

            return result;
        }

        #endregion

        #region Collection Validation

        /// <summary>
        /// Validates collection of PowerPorts (fail-fast).
        /// All ports must belong to the same asset.
        /// Checks: negative numbers, duplicates.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when ports belong to different assets.</exception>
        public ValidationResult ValidatePowerPortCollection(List<PowerPort> powerPorts)
        {
            if (powerPorts == null || !powerPorts.Any())
            {
                return new ValidationResult();
            }

            // Defensive check: ensure all ports belong to the same asset
            var distinctAssets = powerPorts
                .Select(p => p.Asset.Identifier)
                .Where(id => id != null)
                .Distinct()
                .ToList();

            if (distinctAssets.Count > 1)
            {
                throw new ArgumentException(
                    $"All PowerPorts must belong to the same Asset. Found ports from {distinctAssets.Count} different assets.",
                    nameof(powerPorts));
            }

            // Basic checks: negative and duplicate port numbers
            return PortNumberValidator.ValidateCollection(
                powerPorts, p => p.PowerPortInfo.PortNumber, PowerPortValidationField.PortNumber, "Power Port");
        }

        #endregion

        #region Bulk-Specific Validation

        /// <summary>
        /// Validates multiple PowerPorts for a single Asset (bulk optimization).
        /// All ports must belong to the specified asset.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when ports don't belong to the asset.</exception>
        public Dictionary<string, ValidationResult> ValidatePowerPortsForAsset(
            List<PowerPort> portsToValidate, Asset asset)
        {
            return PortBulkValidationHelper.ValidatePortsForAsset(
                portsToValidate,
                asset,
                "PowerPorts",
                p => p.Identifier,
                p => p.Asset.Identifier,
                a => _entityLoader.LoadPowerPorts(a),
                ValidatePowerPortCollection);
        }

        #endregion
    }
}
