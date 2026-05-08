namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.SDM.AssetManagement.Common.Validation;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Common.Services;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Exceptions;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Public validator service for DataPort validation.
    /// DataPorts are validated in context of their parent Asset.
    /// </summary>
    public class DataPortValidator
    {
        private readonly SdmEntityLoader _entityLoader;
        private readonly DataPortValidationCore _validationCore;

        public DataPortValidator(SdmEntityLoader entityLoader)
        {
            _entityLoader = entityLoader ?? throw new ArgumentNullException(nameof(entityLoader));
            _validationCore = new DataPortValidationCore(entityLoader);
        }

        #region Single DataPort Validation

        /// <summary>
        /// Validates a single DataPort and returns ValidationResult.
        /// Collects all errors without throwing exceptions.
        /// </summary>
        public ValidationResult Validate(DataPort dataPort)
        {
            if (dataPort == null)
            {
                throw new ArgumentNullException(nameof(dataPort));
            }

            // Phase 1: No-database validation
            var result = _validationCore.ValidateWithoutDatabaseAccess(dataPort);
            if (!result.IsValid)
            {
                return result;
            }

            // Phase 2: Database validation
            result = result.CombineWith(_validationCore.ValidateWithDatabaseAccess(dataPort));

            return result;
        }

        /// <summary>
        /// Validates a DataPort and throws ValidationException if invalid.
        /// Use this when you want fail-fast behavior.
        /// </summary>
        public void ValidateAndThrow(DataPort dataPort)
        {
            var result = Validate(dataPort);
            if (!result.IsValid)
            {
                throw new ValidationException(result);
            }
        }

        #endregion

        #region Bulk DataPort Validation

        /// <summary>
        /// Validates multiple DataPorts in bulk with optimized performance.
        /// Groups ports by Asset and validates them together.
        /// </summary>
        public Dictionary<string, ValidationResult> ValidateBulk(List<DataPort> dataPorts)
        {
            var results = new Dictionary<string, ValidationResult>();

            if (dataPorts == null || !dataPorts.Any())
            {
                return results;
            }

            // ============================================================
            // PHASE 1: NO DATABASE ACCESS CHECKS (BUSINESS RULES)
            // ============================================================
            foreach (var port in dataPorts)
            {
                results[port.Identifier] = _validationCore.ValidateWithoutDatabaseAccess(port);
            }

            // Fast-fail if business rules fail
            if (results.Any(r => !r.Value.IsValid))
            {
                return results;
            }

            // ============================================================
            // PHASE 2: DATABASE ACCESS CHECKS (PORT TYPE)
            // ============================================================
            foreach (var port in dataPorts.Where(p => results[p.Identifier].IsValid))
            {
                results[port.Identifier].AddFailuresFrom(
                    _validationCore.ValidateWithDatabaseAccess(port));
            }

            // ============================================================
            // PHASE 3: ASSET CONTEXT VALIDATION (OPTIMIZED BY ASSET)
            // ============================================================
            var portsByAsset = dataPorts
                .Where(p => p.AssetId != null && p.AssetId.HasValue() && results[p.Identifier].IsValid)
                .GroupBy(p => p.AssetId.Identifier);

            foreach (var group in portsByAsset)
            {
                var assetId = group.Key;
                var portsForAsset = group.ToList();

                try
                {
                    var asset = _entityLoader.LoadAsset(assetId);
                    if (asset == null)
                    {
                        foreach (var port in portsForAsset)
                        {
                            results[port.Identifier].AddFailReason(
                                DataPortValidationHandler.DataPortValidationField.Asset,
                                $"Parent Asset '{assetId}' not found.");
                        }
                        continue;
                    }

                    var assetPortResults = _validationCore.ValidateDataPortsForAsset(portsForAsset, asset);
                    foreach (var kvp in assetPortResults)
                    {
                        results[kvp.Key].AddFailuresFrom(kvp.Value);
                    }
                }
                catch (Exception ex)
                {
                    foreach (var port in portsForAsset)
                    {
                        results[port.Identifier].AddFailReason(
                            DataPortValidationHandler.DataPortValidationField.DataPort,
                            $"Error validating DataPort: {ex.Message}");
                    }
                }
            }

            return results;
        }

        #endregion
    }
}