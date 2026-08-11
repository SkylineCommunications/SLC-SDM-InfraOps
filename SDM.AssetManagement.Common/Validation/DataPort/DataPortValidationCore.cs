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

    using static Skyline.DataMiner.SDM.AssetManagement.Common.Validation.DataPortValidationHandler;

    /// <summary>
    /// Central validation logic shared between single and bulk DataPort validation.
    /// Separated into no-database and database-access methods for optimal performance.
    /// </summary>
    internal class DataPortValidationCore
    {
        private readonly SdmEntityLoader _entityLoader;

        public DataPortValidationCore(SdmEntityLoader entityLoader)
        {
            _entityLoader = entityLoader;
        }

        #region No Database Access Validation

        /// <summary>
        /// Validates DataPort without database access (business rules only).
        /// </summary>
        public ValidationResult ValidateWithoutDatabaseAccess(DataPort dataPort)
        {
            var result = new ValidationResult();

            // Mandatory fields
            if (dataPort.DataPortInfo.Changed
                && !DataPortValidationHandler.AreMandatoryFieldsValid(dataPort, out var mandatoryResult))
            {
                result.AddFailuresFrom(mandatoryResult);
                return result; // Stop if mandatory fields fail
            }

            // Asset link
            if (dataPort.AssetField.Changed
                && !DataPortValidationHandler.IsAssetLinkValid(dataPort, out var assetLinkResult))
                result.AddFailuresFrom(assetLinkResult);

            if ((dataPort.AddressInfo.Changed || dataPort.PrimaryPortRelation.Changed)
                && !DataPortValidationHandler.IsAddressInfoValid(dataPort, out var addressResult))
                result.AddFailuresFrom(addressResult);

            return result;
        }

        #endregion

        #region Database Access Validation

        /// <summary>
        /// Validates DataPort with database access (PortType, Asset context).
        /// Only called after no-database checks pass.
        /// </summary>
        public ValidationResult ValidateWithDatabaseAccess(DataPort dataPort)
        {
            var result = new ValidationResult();

            // Port Type validation
            result.AddFailuresFrom(ValidatePortType(dataPort));
            if (!result.IsValid) return result;


            // Asset context validation (port number uniqueness, primary ports)
            if (dataPort.Asset.HasValue())
            {
                result.AddFailuresFrom(ValidateAssetContext(dataPort));
            }

            return result;
        }

        /// <summary>
        /// Validates Port Type (must exist and be DataPortType).
        /// </summary>
        private ValidationResult ValidatePortType(DataPort dataPort)
        {
            if (dataPort.DataPortInfo.Type == null || !dataPort.DataPortInfo.Type.HasValue())
            {
                return PortTypeRequiredFailure();
            }

            try
            {
                var portType = _entityLoader.LoadPortType(dataPort.DataPortInfo.Type);
                return ValidatePortTypeAgainst(dataPort, portType);
            }
            catch (Exception ex)
            {
                var result = new ValidationResult();
                result.AddFailReason(DataPortValidationField.PortType,
                    $"Error validating Port Type: {ex.Message}");
                return result;
            }
        }

        /// <summary>
        /// Builds the required-field failure for a missing Port Type reference.
        /// Shared by the single-item and bulk validation paths so the message stays consistent.
        /// </summary>
        private static ValidationResult PortTypeRequiredFailure()
        {
            var result = new ValidationResult();
            result.AddFailReason(DataPortValidationField.PortType,
                "Port Type cannot be empty.");
            return result;
        }

        /// <summary>
        /// Validates a DataPort's Port Type reference against an already-loaded PortType
        /// (or null when the referenced type could not be found). Pure in-memory checks,
        /// so it can be reused by the bulk path after a batched port-type load.
        /// </summary>
        public ValidationResult ValidatePortTypeAgainst(DataPort dataPort, PortType loadedPortType)
        {
            var result = new ValidationResult();

            if (dataPort.DataPortInfo.Type == null || !dataPort.DataPortInfo.Type.HasValue())
            {
                return PortTypeRequiredFailure();
            }

            if (loadedPortType == null)
            {
                result.AddFailReason(DataPortValidationField.PortType,
                    $"Port Type not found. Referenced Port Type '{dataPort.DataPortInfo.Type.Identifier}' does not exist.");
                return result;
            }

            if (!loadedPortType.IsDataPortType())
            {
                result.AddFailReason(DataPortValidationField.PortType,
                    "Port Type must be a Data Port Type.");
                return result;
            }

            return result;
        }

        /// <summary>
        /// Validates DataPort in context of its Asset.
        /// </summary>
        private ValidationResult ValidateAssetContext(DataPort dataPort)
        {
            var result = new ValidationResult();

            try
            {
                var asset = _entityLoader.LoadAsset(dataPort.Asset);
                if (asset == null)
                {
                    result.AddFailReason(DataPortValidationField.Asset,
                        $"Referenced Asset '{dataPort.Asset.Identifier}' does not exist.");
                    return result;
                }

                // Load all ports for asset
                var allPorts = _entityLoader.LoadDataPorts(asset)
                    .Where(p => p.Identifier != dataPort.Identifier)
                    .ToList();

                allPorts.Add(dataPort);

                // Validate collection (uniqueness + primary ports)
                result.AddFailuresFrom(ValidateDataPortCollection(allPorts));
            }
            catch (Exception ex)
            {
                result.AddFailReason(DataPortValidationField.Asset,
                    $"Error validating Asset context: {ex.Message}");
            }

            return result;
        }

        #endregion

        #region Collection Validation

        /// <summary>
        /// Validates collection of DataPorts (fail-fast).
        /// All ports must belong to the same asset.
        /// Checks: negative numbers, duplicates, primary ports.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when ports belong to different assets.</exception>
        public ValidationResult ValidateDataPortCollection(List<DataPort> dataPorts)
        {
            if (dataPorts == null || !dataPorts.Any())
            {
                return new ValidationResult();
            }

            // ✅ DEFENSIVE CHECK: Ensure all ports belong to the same asset
            var distinctAssets = dataPorts
                .Select(p => p.Asset.Identifier)
                .Where(id => id != null)
                .Distinct()
                .ToList();

            if (distinctAssets.Count > 1)
            {
                throw new ArgumentException(
                    $"All DataPorts must belong to the same Asset. Found ports from {distinctAssets.Count} different assets.",
                    nameof(dataPorts));
            }

            // Basic checks: negative and duplicate port numbers
            var result = PortNumberValidator.ValidateCollection(
                dataPorts, p => p.DataPortInfo.PortNumber, DataPortValidationField.PortNumber, "Data Port");
            if (!result.IsValid) return result;

            // DataPort-specific: primary IPv4/IPv6 uniqueness
            int primaryIPv4Count = 0;
            int primaryIPv6Count = 0;

            foreach (var port in dataPorts)
            {
                if (port.PrimaryPortRelation.IsPrimaryIpv4 && ++primaryIPv4Count > 1)
                {
                    result.AddFailReason(DataPortValidationField.PrimaryPort,
                        "Only one Data Port can be marked as Primary IPv4.");
                    return result;
                }

                if (port.PrimaryPortRelation.IsPrimaryIpv6 && ++primaryIPv6Count > 1)
                {
                    result.AddFailReason(DataPortValidationField.PrimaryPort,
                        "Only one Data Port can be marked as Primary IPv6.");
                    return result;
                }
            }

            return result;
        }

        #endregion

        #region Bulk-Specific Validation

        /// <summary>
        /// Validates multiple DataPorts for a single Asset (bulk optimization).
        /// All ports must belong to the specified asset.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when ports don't belong to the asset.</exception>
        public Dictionary<string, ValidationResult> ValidateDataPortsForAsset(
            List<DataPort> portsToValidate, Asset asset)
        {
            return PortBulkValidationHelper.ValidatePortsForAsset(
                portsToValidate,
                asset,
                "DataPorts",
                p => p.Identifier,
                p => p.Asset.Identifier,
                a => _entityLoader.LoadDataPorts(a),
                ValidateDataPortCollection);
        }

        #endregion
    }
}