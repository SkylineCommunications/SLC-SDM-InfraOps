namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.SDM.AssetManagement.Common.Validation;
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
            if (dataPort.DataPortInfoField.Changed)
            {
                if (!DataPortValidationHandler.AreMandatoryFieldsValid(dataPort, out var mandatoryResult))
                {
                    result.AddFailuresFrom(mandatoryResult);
                    return result; // Stop if mandatory fields fail
                }
            }

            // Asset link
            if (dataPort.AssetFkField.Changed)
            {
                if (!DataPortValidationHandler.IsAssetLinkValid(dataPort, out var assetLinkResult))
                {
                    result.AddFailuresFrom(assetLinkResult);
                }
            }

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
            if (dataPort.AssetFk.Asset.HasValue())
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
            var result = new ValidationResult();

            if (dataPort.DataPortInfo.Type == null || !dataPort.DataPortInfo.Type.HasValue())
            {
                result.AddFailReason(DataPortValidationField.PortType,
                    "Port Type cannot be empty.");
                return result;
            }

            try
            {
                var portType = _entityLoader.LoadPortType(dataPort.DataPortInfo.Type);
                if (portType == null)
                {
                    result.AddFailReason(DataPortValidationField.PortType,
                        "Port Type not found.");
                    return result;
                }

                if (!portType.IsDataPortType)
                {
                    result.AddFailReason(DataPortValidationField.PortType,
                        "Port Type must be a Data Port Type.");
                    return result;
                }
            }
            catch (Exception ex)
            {
                result.AddFailReason(DataPortValidationField.PortType,
                    $"Error validating Port Type: {ex.Message}");
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
                var asset = _entityLoader.LoadAsset(dataPort.AssetFk.Asset);
                if (asset == null)
                {
                    result.AddFailReason(DataPortValidationField.Asset,
                        $"Parent Asset '{dataPort.AssetFk.Asset.Identifier}' not found.");
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
                .Select(p => p.AssetFk?.Asset.Identifier)
                .Where(id => id != null)
                .Distinct()
                .ToList();

            if (distinctAssets.Count > 1)
            {
                throw new ArgumentException(
                    $"All DataPorts must belong to the same Asset. Found ports from {distinctAssets.Count} different assets.",
                    nameof(dataPorts));
            }

            var result = new ValidationResult();

            var seenPortNumbers = new HashSet<long>();
            int primaryIPv4Count = 0;
            int primaryIPv6Count = 0;

            foreach (var port in dataPorts)
            {
                // Check for negative port numbers
                if (port.DataPortInfo.PortNumber < 0)
                {
                    result.AddFailReason(DataPortValidationField.PortNumber,
                        $"Data Port number cannot be negative. Found: {port.DataPortInfo.PortNumber}");
                    return result;
                }

                // Check for duplicate port numbers
                if (!seenPortNumbers.Add(port.DataPortInfo.PortNumber))
                {
                    result.AddFailReason(DataPortValidationField.PortNumber,
                        $"Port number {port.DataPortInfo.PortNumber} is already in use on the asset.");
                    return result;
                }

                // Count primary IPv4 ports
                if (port.PrimaryPortRelation.IsPrimaryIpv4)
                {
                    primaryIPv4Count++;
                    if (primaryIPv4Count > 1)
                    {
                        result.AddFailReason(DataPortValidationField.PrimaryPort,
                            "Only one Data Port can be marked as Primary IPv4.");
                        return result;
                    }
                }

                // Count primary IPv6 ports
                if (port.PrimaryPortRelation.IsPrimaryIpv6)
                {
                    primaryIPv6Count++;
                    if (primaryIPv6Count > 1)
                    {
                        result.AddFailReason(DataPortValidationField.PrimaryPort,
                            "Only one Data Port can be marked as Primary IPv6.");
                        return result;
                    }
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
            if (asset == null)
            {
                throw new ArgumentNullException(nameof(asset));
            }

            if (portsToValidate == null || !portsToValidate.Any())
            {
                return new Dictionary<string, ValidationResult>();
            }

            // ✅ DEFENSIVE CHECK: Ensure all ports belong to this asset
            var mismatchedPorts = portsToValidate
                .Where(p => p.AssetFk?.Asset.Identifier != asset.Identifier)
                .ToList();

            if (mismatchedPorts.Any())
            {
                throw new ArgumentException(
                    $"All DataPorts must belong to Asset '{asset.Identifier}'. Found {mismatchedPorts.Count} port(s) belonging to different assets. ",
                    nameof(portsToValidate));
            }

            var results = portsToValidate.ToDictionary(p => p.Identifier, p => new ValidationResult());

            var validatedIds = portsToValidate.Select(p => p.Identifier).ToList();
            var existingPorts = _entityLoader.LoadDataPorts(asset)
                .Where(p => !validatedIds.Contains(p.Identifier))
                .ToList();

            var allPorts = existingPorts.Concat(portsToValidate).ToList();

            var collectionResult = ValidateDataPortCollection(allPorts);

            if (!collectionResult.IsValid)
            {
                foreach (var port in portsToValidate)
                {
                    results[port.Identifier].AddFailuresFrom(collectionResult);
                }
            }

            return results;
        }

        #endregion
    }
}