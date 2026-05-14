namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.SDM.AssetManagement.Common.Validation;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Common.Services;
    using Skyline.DataMiner.SDM.Extensions;
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
        private readonly Validator<DataPort> _validationPipeline;

        public DataPortValidator(SdmEntityLoader entityLoader)
        {
            _entityLoader = entityLoader ?? throw new ArgumentNullException(nameof(entityLoader));
            _validationCore = new DataPortValidationCore(entityLoader);
            _validationPipeline = BuildValidationPipeline();
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

            return _validationPipeline.Validate(dataPort);
        }

        /// <summary>
        /// Validates a DataPort and throws ValidationException if invalid.
        /// Use this when you want fail-fast behavior.
        /// </summary>
        public void ValidateAndThrow(DataPort dataPort)
        {
            _validationPipeline.ValidateAndThrow(dataPort);
        }

        /// <summary>
        /// Validates with custom error handling callback.
        /// </summary>
        public ValidationResult ValidateWithHandler(DataPort dataPort, Action<ValidationResult> onError)
        {
            return _validationPipeline.ValidateWithHandler(dataPort, onError);
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
                .Where(p => p.Asset != null && p.Asset.HasValue() && results[p.Identifier].IsValid)
                .GroupBy(p => p.Asset.Identifier);

            foreach (var group in portsByAsset)
            {
                var assetId = group.Key;
                var assetFk = group.First().Asset;
                var portsForAsset = group.ToList();

                try
                {
                    var asset = _entityLoader.LoadAsset(assetFk);
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

        #region Pipeline Construction

        private Validator<DataPort> BuildValidationPipeline()
        {
            // Critical validations (business rules) - stop on failure
            var criticalValidations = Validator<DataPort>
                .Create(ValidateCriticalFields)
                .StopOnFailure();

            // Database validations - collect all errors
            var databaseValidations = Validator<DataPort>
                .Create(ValidateDatabaseFields);

            // Combine: critical first, then database
            return criticalValidations.AndThen(databaseValidations);
        }

        #endregion

        #region Validation Methods

        private ValidationResult ValidateCriticalFields(DataPort dataPort)
        {
            // Phase 1: No-database validation (mandatory fields, business rules)
            return _validationCore.ValidateWithoutDatabaseAccess(dataPort);
        }

        private ValidationResult ValidateDatabaseFields(DataPort dataPort)
        {
            // Phase 2: Database validation (Port Type, Asset context)
            return _validationCore.ValidateWithDatabaseAccess(dataPort);
        }

        #endregion
    }
}