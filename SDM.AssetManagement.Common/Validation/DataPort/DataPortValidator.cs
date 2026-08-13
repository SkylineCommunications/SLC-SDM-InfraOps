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
    public class DataPortValidator : ValidatorBase<DataPort>
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
        protected override ValidationResult Validate(DataPort dataPort)
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
        /// Returns one result per input port, in the same order.
        /// </summary>
        protected override List<ValidationResult> ValidateBulk(List<DataPort> dataPorts)
        {
            if (dataPorts == null || !dataPorts.Any())
            {
                return new List<ValidationResult>();
            }

            var results = dataPorts.Select(_ => new ValidationResult()).ToList();

            // Map identifier -> positional index so DB-phase results (keyed by identifier)
            // can be written back to the correct slot.
            var indexByIdentifier = new Dictionary<string, int>();
            for (int i = 0; i < dataPorts.Count; i++)
            {
                indexByIdentifier[dataPorts[i].Identifier] = i;
            }

            ValidateBusinessRules(dataPorts, results);
            if (results.AnyInvalid())
            {
                return results;
            }

            ValidatePortTypesInBulk(dataPorts, results);
            if (results.AnyInvalid())
            {
                return results;
            }

            ValidateAssetContext(dataPorts, results, indexByIdentifier);

            return results;
        }

        private void ValidateBusinessRules(List<DataPort> dataPorts, List<ValidationResult> results)
        {
            for (int i = 0; i < dataPorts.Count; i++)
            {
                results[i].AddFailuresFrom(_validationCore.ValidateWithoutDatabaseAccess(dataPorts[i]));
            }
        }

        private void ValidatePortTypesInBulk(List<DataPort> dataPorts, List<ValidationResult> results)
        {
            var distinctPortTypeIds = dataPorts
                .Where(p => p.DataPortInfo.TypeField.Changed == true && p.DataPortInfo.Type != null && p.DataPortInfo.Type.HasValue())
                .Select(p => p.DataPortInfo.Type.Identifier)
                .Distinct()
                .ToList();

            var portTypeMap = _entityLoader.GetPortTypesByDomIds(distinctPortTypeIds)
                .ToDictionary(pt => pt.Identifier);

            for (int i = 0; i < dataPorts.Count; i++)
            {
                var port = dataPorts[i];

                PortType loadedPortType = null;
                if (port.DataPortInfo.Type != null && port.DataPortInfo.Type.HasValue())
                {
                    portTypeMap.TryGetValue(port.DataPortInfo.Type.Identifier, out loadedPortType);
                }

                results[i].AddFailuresFrom(_validationCore.ValidatePortTypeAgainst(port, loadedPortType));
            }
        }

        private void ValidateAssetContext(List<DataPort> dataPorts, List<ValidationResult> results, Dictionary<string, int> indexByIdentifier)
        {
            var distinctAssetIds = dataPorts
                .Where(p => p.AssetField.Changed)
                .Select(p => p.Asset.Identifier)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            var assetMap = _entityLoader.GetAssetsByDomIds(distinctAssetIds)
                .ToDictionary(a => a.Identifier);

            var portsByAsset = dataPorts
                .Where(p => p.AssetField.Changed && !string.IsNullOrWhiteSpace(p.Asset.Identifier))
                .GroupBy(p => p.Asset.Identifier);

            foreach (var group in portsByAsset)
            {
                ValidateAssetGroup(group, assetMap, results, indexByIdentifier);
            }
        }

        private void ValidateAssetGroup(IGrouping<string, DataPort> group, Dictionary<string, Asset> assetMap, List<ValidationResult> results, Dictionary<string, int> indexByIdentifier)
        {
            if (!assetMap.TryGetValue(group.Key, out var asset))
            {
                foreach (var port in group)
                {
                    results[indexByIdentifier[port.Identifier]].AddFailReason(
                        DataPortValidationField.Asset,
                        $"Referenced Asset '{group.Key}' does not exist.");
                }

                return;
            }

            var groupResults = _validationCore.ValidateDataPortsForAsset(group.ToList(), asset);
            foreach (var kvp in groupResults)
            {
                results[indexByIdentifier[kvp.Key]].AddFailuresFrom(kvp.Value);
            }
        }

        protected override ValidationResult ValidateForDelete(DataPort dataPort)
        {
            if (dataPort == null)
            {
                throw new ArgumentNullException(nameof(dataPort));
            }

            return ValidateNotAssignedToConnections(new List<DataPort> { dataPort })[0];
        }

        protected override List<ValidationResult> ValidateBulkForDelete(List<DataPort> dataPorts)
        {
            if (dataPorts == null || !dataPorts.Any())
            {
                return new List<ValidationResult>();
            }

            return ValidateNotAssignedToConnections(dataPorts);
        }

        private List<ValidationResult> ValidateNotAssignedToConnections(List<DataPort> dataPorts)
        {
            var results = dataPorts.Select(_ => new ValidationResult()).ToList();

            var portIds = dataPorts
                .Select(p => p.Identifier)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            var connectedPortIds = _entityLoader.GetConnectionsByPortIds(portIds)
                .SelectMany(connection => connection.GetPortIds())
                .ToHashSet();

            for (int i = 0; i < dataPorts.Count; i++)
            {
                if (connectedPortIds.Contains(dataPorts[i].Identifier))
                {
                    results[i].AddFailReason(
                        DataPortValidationField.DataPort,
                        "This port has connections assigned. Please delete all of the connections first.");
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