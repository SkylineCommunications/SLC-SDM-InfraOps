namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Common.Services;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    using static Skyline.DataMiner.SDM.AssetManagement.Common.Validation.DataPortValidationHandler;

    /// <summary>
    /// Public validator service for DataPort validation.
    /// DataPorts are validated in context of their parent Asset.
    /// </summary>
    public class DataPortValidator
    {
        private readonly DataPortValidationCore _validationCore;
        private readonly SdmEntityLoader _entityLoader;
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
        /// Groups ports by parent Asset and loads each Asset's existing ports once,
        /// reducing N DB reads to K (one per unique Asset in the batch).
        /// Returns a dictionary keyed by port identifier.
        /// </summary>
        public Dictionary<string, ValidationResult> ValidateBulk(List<DataPort> dataPorts)
        {
            if (dataPorts == null || !dataPorts.Any())
            {
                return new Dictionary<string, ValidationResult>();
            }

            var results = dataPorts.ToDictionary(p => p.Identifier, _ => new ValidationResult());

            // ============================================================
            // PHASE 1: NO DATABASE ACCESS CHECKS (BUSINESS RULES)
            // ============================================================
            foreach (var port in dataPorts)
            {
                var nonDbResult = _validationCore.ValidateWithoutDatabaseAccess(port);
                results[port.Identifier].AddFailuresFrom(nonDbResult);
            }

            var failedIds = results.Where(r => !r.Value.IsValid).Select(r => r.Key).ToHashSet();
            var validPorts = dataPorts.Where(p => !failedIds.Contains(p.Identifier)).ToList();

            if (!validPorts.Any())
            {
                return results;
            }

            // ============================================================
            // PHASE 2: ASSET-CONTEXT VALIDATION (grouped by parent Asset)
            // Bulk-load all distinct parent Assets in one OR query, then
            // call ValidateDataPortsForAsset per group — K DB calls instead of N.
            // ============================================================
            var distinctAssetIds = validPorts
                .Select(p => p.Asset.Identifier)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            var assetMap = _entityLoader.GetAssetsByDomIds(distinctAssetIds)
                .ToDictionary(a => a.Identifier);

            var portsByAsset = validPorts
                .Where(p => !string.IsNullOrWhiteSpace(p.Asset.Identifier))
                .GroupBy(p => p.Asset.Identifier);

            foreach (var group in portsByAsset)
            {
                if (!assetMap.TryGetValue(group.Key, out var asset))
                {
                    foreach (var port in group)
                    {
                        results[port.Identifier].AddFailReason(
                            DataPortValidationField.Asset,
                            $"Parent Asset '{group.Key}' not found.");
                    }

                    continue;
                }

                var groupResults = _validationCore.ValidateDataPortsForAsset(group.ToList(), asset);
                foreach (var kvp in groupResults)
                {
                    results[kvp.Key].AddFailuresFrom(kvp.Value);
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