namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Common.Services;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    using static Skyline.DataMiner.SDM.AssetManagement.Common.Validation.PowerPortValidationHandler;

    /// <summary>
    /// Public validator service for PowerPort validation.
    /// PowerPorts are validated in context of their parent Asset.
    /// </summary>
    public class PowerPortValidator : ValidatorBase<PowerPort>
    {
        private readonly PowerPortValidationCore _validationCore;
        private readonly SdmEntityLoader _entityLoader;
        private readonly Validator<PowerPort> _validationPipeline;

        public PowerPortValidator(SdmEntityLoader entityLoader)
        {
            _entityLoader = entityLoader ?? throw new ArgumentNullException(nameof(entityLoader));
            _validationCore = new PowerPortValidationCore(entityLoader);
            _validationPipeline = BuildValidationPipeline();
        }

        #region Single PowerPort Validation

        /// <summary>
        /// Validates a single PowerPort and returns ValidationResult.
        /// Collects all errors without throwing exceptions.
        /// </summary>
        protected override ValidationResult Validate(PowerPort powerPort)
        {
            if (powerPort == null)
            {
                throw new ArgumentNullException(nameof(powerPort));
            }

            return _validationPipeline.Validate(powerPort);
        }

        /// <summary>
        /// Validates a PowerPort and throws ValidationException if invalid.
        /// Use this when you want fail-fast behavior.
        /// </summary>
        public void ValidateAndThrow(PowerPort powerPort)
        {
            _validationPipeline.ValidateAndThrow(powerPort);
        }

        /// <summary>
        /// Validates with custom error handling callback.
        /// </summary>
        public ValidationResult ValidateWithHandler(PowerPort powerPort, Action<ValidationResult> onError)
        {
            return _validationPipeline.ValidateWithHandler(powerPort, onError);
        }

        #endregion

        #region Bulk PowerPort Validation

        /// <summary>
        /// Validates multiple PowerPorts in bulk with optimized performance.
        /// Groups ports by parent Asset and loads each Asset's existing ports once,
        /// reducing N DB reads to K (one per unique Asset in the batch).
        /// Returns one result per input port, in the same order.
        /// </summary>
        protected override List<ValidationResult> ValidateBulk(List<PowerPort> powerPorts)
        {
            if (powerPorts == null || !powerPorts.Any())
            {
                return new List<ValidationResult>();
            }

            var results = powerPorts.Select(_ => new ValidationResult()).ToList();

            // Map identifier -> positional index so DB-phase results (keyed by identifier)
            // can be written back to the correct slot.
            var indexByIdentifier = new Dictionary<string, int>();
            for (int i = 0; i < powerPorts.Count; i++)
            {
                indexByIdentifier[powerPorts[i].Identifier] = i;
            }

            ValidateBusinessRules(powerPorts, results);
            if (results.AnyInvalid())
            {
                return results;
            }

            ValidatePortTypesInBulk(powerPorts, results);
            if (results.AnyInvalid())
            {
                return results;
            }

            ValidateAssetContext(powerPorts, results, indexByIdentifier);

            return results;
        }

        private void ValidateBusinessRules(List<PowerPort> powerPorts, List<ValidationResult> results)
        {
            for (int i = 0; i < powerPorts.Count; i++)
            {
                results[i].AddFailuresFrom(_validationCore.ValidateWithoutDatabaseAccess(powerPorts[i]));
            }
        }

        private void ValidatePortTypesInBulk(List<PowerPort> powerPorts, List<ValidationResult> results)
        {
            var distinctPortTypeIds = powerPorts
                .Where(p => p.PowerPortInfo?.PortType != null && p.PowerPortInfo.PortType.HasValue())
                .Select(p => p.PowerPortInfo.PortType.Identifier)
                .Distinct()
                .ToList();

            var portTypeMap = _entityLoader.GetPortTypesByDomIds(distinctPortTypeIds)
                .ToDictionary(pt => pt.Identifier);

            for (int i = 0; i < powerPorts.Count; i++)
            {
                var port = powerPorts[i];

                PortType loadedPortType = null;
                if (port.PowerPortInfo?.PortType != null && port.PowerPortInfo.PortType.HasValue())
                {
                    portTypeMap.TryGetValue(port.PowerPortInfo.PortType.Identifier, out loadedPortType);
                }

                results[i].AddFailuresFrom(_validationCore.ValidatePortTypeAgainst(port, loadedPortType));
            }
        }

        private void ValidateAssetContext(List<PowerPort> powerPorts, List<ValidationResult> results, Dictionary<string, int> indexByIdentifier)
        {
            var distinctAssetIds = powerPorts
                .Select(p => p.Asset.Identifier)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            var assetMap = _entityLoader.GetAssetsByDomIds(distinctAssetIds)
                .ToDictionary(a => a.Identifier);

            var portsByAsset = powerPorts
                .Where(p => !string.IsNullOrWhiteSpace(p.Asset.Identifier))
                .GroupBy(p => p.Asset.Identifier);

            foreach (var group in portsByAsset)
            {
                ValidateAssetGroup(group, assetMap, results, indexByIdentifier);
            }
        }

        private void ValidateAssetGroup(IGrouping<string, PowerPort> group, Dictionary<string, Asset> assetMap, List<ValidationResult> results, Dictionary<string, int> indexByIdentifier)
        {
            if (!assetMap.TryGetValue(group.Key, out var asset))
            {
                foreach (var port in group)
                {
                    results[indexByIdentifier[port.Identifier]].AddFailReason(
                        PowerPortValidationField.Asset,
                        $"Referenced Asset '{group.Key}' does not exist.");
                }

                return;
            }

            var groupResults = _validationCore.ValidatePowerPortsForAsset(group.ToList(), asset);
            foreach (var kvp in groupResults)
            {
                results[indexByIdentifier[kvp.Key]].AddFailuresFrom(kvp.Value);
            }
        }

        protected override ValidationResult ValidateForDelete(PowerPort powerPort)
        {
            if (powerPort == null)
            {
                throw new ArgumentNullException(nameof(powerPort));
            }

            return ValidateNotAssignedToConnections(new List<PowerPort> { powerPort })[0];
        }

        protected override List<ValidationResult> ValidateBulkForDelete(List<PowerPort> powerPorts)
        {
            if (powerPorts == null || !powerPorts.Any())
            {
                return new List<ValidationResult>();
            }

            return ValidateNotAssignedToConnections(powerPorts);
        }

        private List<ValidationResult> ValidateNotAssignedToConnections(List<PowerPort> powerPorts)
        {
            var results = powerPorts.Select(_ => new ValidationResult()).ToList();

            var portIds = powerPorts
                .Select(p => p.Identifier)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            var connectedPortIds = _entityLoader.GetConnectionsByPortIds(portIds)
                .SelectMany(GetConnectionPortIds)
                .ToHashSet();

            for (int i = 0; i < powerPorts.Count; i++)
            {
                if (connectedPortIds.Contains(powerPorts[i].Identifier))
                {
                    results[i].AddFailReason(
                        PowerPortValidationField.PowerPort,
                        "This port has connections assigned. Please delete all of the connections first.");
                }
            }

            return results;
        }

        private static IEnumerable<string> GetConnectionPortIds(Connection connection)
        {
            if (connection?.Source != null && connection.Source.Port != Guid.Empty)
            {
                yield return connection.Source.Port.ToString();
            }

            if (connection?.Destination != null && connection.Destination.Port != Guid.Empty)
            {
                yield return connection.Destination.Port.ToString();
            }
        }

        #endregion

        #region Pipeline Construction

        private Validator<PowerPort> BuildValidationPipeline()
        {
            // Critical validations (business rules) - stop on failure
            var criticalValidations = Validator<PowerPort>
                .Create(ValidateCriticalFields)
                .StopOnFailure();

            // Database validations - collect all errors
            var databaseValidations = Validator<PowerPort>
                .Create(ValidateDatabaseFields);

            // Combine: critical first, then database
            return criticalValidations.AndThen(databaseValidations);
        }

        #endregion

        #region Validation Methods

        private ValidationResult ValidateCriticalFields(PowerPort powerPort)
        {
            // Phase 1: No-database validation (mandatory fields, business rules)
            return _validationCore.ValidateWithoutDatabaseAccess(powerPort);
        }

        private ValidationResult ValidateDatabaseFields(PowerPort powerPort)
        {
            // Phase 2: Database validation (Port Type, Asset context)
            return _validationCore.ValidateWithDatabaseAccess(powerPort);
        }

        #endregion
    }
}
