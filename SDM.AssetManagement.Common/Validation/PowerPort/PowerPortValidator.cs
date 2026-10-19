namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.SDM.AssetManagement.Common.Validation;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Common.Services;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.SDM.InfraOps.Common.Validation;
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

        public PowerPortValidator(SdmEntityLoader entityLoader)
        {
            _entityLoader = entityLoader ?? throw new ArgumentNullException(nameof(entityLoader));
            _validationCore = new PowerPortValidationCore(entityLoader);
        }

        protected override List<ValidationResult> ValidateBulkNoDatabaseBusinessRules(List<PowerPort> entities)
        {
            var results = entities.Select(_ => new ValidationResult()).ToList();

            ValidateBusinessRules(entities, results);

            return results;
        }

        protected override List<ValidationResult> ValidateBulkNoDatabaseBatchConflicts(List<PowerPort> entities)
        {
            return entities.Select(_ => new ValidationResult()).ToList();
        }

        protected override List<ValidationResult> ValidateBulkWithDatabase(List<PowerPort> entities)
        {
            var results = entities.Select(_ => new ValidationResult()).ToList();

            ValidatePortTypesInBulk(entities, results);

            // Map identifier -> positional index so DB-phase results (keyed by identifier)
            // can be written back to the correct slot.
            var indexByIdentifier = new Dictionary<string, int>();
            for (int idx = 0; idx < entities.Count; idx++)
            {
                indexByIdentifier[entities[idx].Identifier] = idx;
            }

            ValidateAssetContext(entities, results, indexByIdentifier);
            ValidateConnectionConflicts(entities, results);

            return results;
        }

        private void ValidateConnectionConflicts(List<PowerPort> powerPorts, List<ValidationResult> results)
        {
            var changedIndexes = Enumerable.Range(0, powerPorts.Count)
                .Where(i => !powerPorts[i].IsNew
                    && powerPorts[i].PowerPortInfo.OutputTypeField.Changed
                    && !string.IsNullOrWhiteSpace(powerPorts[i].Identifier))
                .ToList();

            if (!changedIndexes.Any())
            {
                return;
            }

            var portIds = changedIndexes.Select(i => powerPorts[i].Identifier).Distinct().ToList();
            var connections = _entityLoader.GetConnectionsByPortIds(portIds);

            foreach (var i in changedIndexes)
            {
                PowerPortValidationHandler.IsOutputTypeCompatibleWithConnections(powerPorts[i], connections, out var conflictResult);
                results[i].AddFailuresFrom(conflictResult);
            }
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
                .Where(p => p.ShouldValidate(p.PowerPortInfo.PortTypeField) && p.PowerPortInfo.PortType != null && p.PowerPortInfo.PortType.HasValue())
                .Select(p => p.PowerPortInfo.PortType.Identifier)
                .Distinct()
                .ToList();

            var portTypeMap = _entityLoader.GetPortTypesByDomIds(distinctPortTypeIds)
                .ToDictionary(pt => pt.Identifier);

            for (int idx = 0; idx < powerPorts.Count; idx++)
            {
                var port = powerPorts[idx];

                if (!port.ShouldValidate(port.PowerPortInfo.PortTypeField))
                {
                    continue;
                }

                PortType loadedPortType = null;
                if (port.PowerPortInfo.PortType != null && port.PowerPortInfo.PortType.HasValue())
                {
                    portTypeMap.TryGetValue(port.PowerPortInfo.PortType.Identifier, out loadedPortType);
                }

                results[idx].AddFailuresFrom(_validationCore.ValidatePortTypeAgainst(port, loadedPortType));
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
                ValidateAssetGroup(powerPorts, group, assetMap, results, indexByIdentifier);
            }
        }

        private void ValidateAssetGroup(List<PowerPort> powerPorts, IGrouping<string, PowerPort> group, Dictionary<string, Asset> assetMap, List<ValidationResult> results, Dictionary<string, int> indexByIdentifier)
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

            var groupResults = _validationCore.ValidatePowerPortsForAsset(powerPorts, group.ToList(), asset);
            foreach (var kvp in groupResults)
            {
                results[indexByIdentifier[kvp.Key]].AddFailuresFrom(kvp.Value);
            }
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
                .SelectMany(connection => connection.GetPortIds())
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
    }
}
