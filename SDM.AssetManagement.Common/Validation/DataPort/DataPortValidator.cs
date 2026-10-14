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

    using static Skyline.DataMiner.SDM.AssetManagement.Common.Validation.DataPortValidationHandler;

    /// <summary>
    /// Public validator service for DataPort validation.
    /// DataPorts are validated in context of their parent Asset.
    /// </summary>
    public class DataPortValidator : ValidatorBase<DataPort>
    {
        private readonly DataPortValidationCore _validationCore;
        private readonly SdmEntityLoader _entityLoader;

        public DataPortValidator(SdmEntityLoader entityLoader)
        {
            _entityLoader = entityLoader ?? throw new ArgumentNullException(nameof(entityLoader));
            _validationCore = new DataPortValidationCore(entityLoader);
        }

        protected override List<ValidationResult> ValidateBulkNoDatabaseBusinessRules(List<DataPort> entities)
        {
            var results = entities.Select(_ => new ValidationResult()).ToList();

            ValidateBusinessRules(entities, results);

            return results;
        }

        protected override List<ValidationResult> ValidateBulkNoDatabaseBatchConflicts(List<DataPort> entities)
        {
            return entities.Select(_ => new ValidationResult()).ToList();
        }

        protected override List<ValidationResult> ValidateBulkWithDatabase(List<DataPort> entities)
        {
            var results = entities.Select(_ => new ValidationResult()).ToList();

            ValidatePortTypesInBulk(entities, results);

            // Map identifier -> positional index so DB-phase results (keyed by identifier)
            // can be written back to the correct slot.
            var indexByIdentifier = new Dictionary<string, int>();
            for (int i = 0; i < entities.Count; i++)
            {
                indexByIdentifier[entities[i].Identifier] = i;
            }

            ValidateAssetContext(entities, results, indexByIdentifier);
            ValidateConnectionConflicts(entities, results);
            return results;
        }

        private void ValidateConnectionConflicts(List<DataPort> dataPorts, List<ValidationResult> results)
        {
            var changedIndexes = Enumerable.Range(0, dataPorts.Count)
                .Where(i => !dataPorts[i].IsNew
                    && dataPorts[i].DataPortInfo.OutputTypeField.Changed == true
                    && !string.IsNullOrWhiteSpace(dataPorts[i].Identifier))
                .ToList();

            if (!changedIndexes.Any())
            {
                return;
            }

            var portIds = changedIndexes.Select(i => dataPorts[i].Identifier).Distinct().ToList();
            var connections = _entityLoader.GetConnectionsByPortIds(portIds);

            foreach (var i in changedIndexes)
            {
                DataPortValidationHandler.IsOutputTypeCompatibleWithConnections(dataPorts[i], connections, out var conflictResult);
                results[i].AddFailuresFrom(conflictResult);
            }
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
                .Where(p => p.ShouldValidate(p.DataPortInfo.PortTypeField) && p.DataPortInfo.PortType != null && p.DataPortInfo.PortType.HasValue())
                .Select(p => p.DataPortInfo.PortType.Identifier)
                .Distinct()
                .ToList();

            var portTypeMap = _entityLoader.GetPortTypesByDomIds(distinctPortTypeIds)
                .ToDictionary(pt => pt.Identifier);

            for (int idx = 0; idx < dataPorts.Count; idx++)
            {
                var port = dataPorts[idx];

                if (!port.ShouldValidate(port.DataPortInfo.PortTypeField))
                {
                    continue;
                }

                PortType loadedPortType = null;
                if (port.DataPortInfo.PortType != null && port.DataPortInfo.PortType.HasValue())
                {
                    portTypeMap.TryGetValue(port.DataPortInfo.PortType.Identifier, out loadedPortType);
                }

                results[idx].AddFailuresFrom(_validationCore.ValidatePortTypeAgainst(port, loadedPortType));
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
    }
}