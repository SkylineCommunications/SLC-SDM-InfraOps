namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Skyline.DataMiner.Net.Helper;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Common.Services;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    public class PortTypeValidator : ValidatorBase<PortType>
    {
        public enum PortTypeValidationField
        {
            Category,
            CableTypeFks,
            AssetPorts,
            AssetClassPorts,
        }

        private readonly SdmEntityLoader _entityLoader;

        public PortTypeValidator(SdmEntityLoader entityLoader)
        {
            _entityLoader = entityLoader ?? throw new ArgumentNullException(nameof(entityLoader));
        }

        protected override List<ValidationResult> ValidateBulkNoDatabaseBusinessRules(List<PortType> entities)
        {
            var results = entities.Select(_ => new ValidationResult()).ToList();

            for (int idx = 0; idx < entities.Count; idx++)
            {
                if (entities[idx]?.CategoryLinks.Categories.IsNullOrEmpty() ?? true)
                {
                    results[idx].AddFailReason(PortTypeValidationField.Category, "Port Type must have at least one category.");
                }

                if (entities[idx]?.CableFKs.CableTypeFks.IsNullOrEmpty() ?? true)
                {
                    results[idx].AddFailReason(PortTypeValidationField.CableTypeFks, "Port Type must have at least one cable type.");
                }

                var cableTypes = entities[idx]?.CableFKs.CableTypeFks ?? new List<SdmObjectReference<CableType>>();

                foreach (var reference in cableTypes)
                {
                    if (!reference.HasValue())
                    {
                        results[idx].AddFailReason(PortTypeValidationField.CableTypeFks, $"Referenced Cable Type cannot be empty.");
                    }
                }
            }

            return results;
        }

        protected override List<ValidationResult> ValidateBulkNoDatabaseBatchConflicts(List<PortType> entities)
        {
            return entities.Select(_ => new ValidationResult()).ToList();
        }

        protected override List<ValidationResult> ValidateBulkWithDatabase(List<PortType> entities)
        {
            var results = entities.Select(_ => new ValidationResult()).ToList();

            var cableTypeIds = entities
                .SelectMany(pt => pt?.CableFKs.CableTypeFks ?? new List<SdmObjectReference<CableType>>())
                .Where(reference => reference.HasValue())
                .Select(reference => reference.Identifier)
                .Distinct()
                .ToList();

            var existingCableTypeIds = _entityLoader.GetCableTypesByDomIds(cableTypeIds).Select(ct => ct.Identifier).ToHashSet();

            for (int idx = 0; idx < entities.Count; idx++)
            {
                var cableTypes = entities[idx]?.CableFKs.CableTypeFks ?? new List<SdmObjectReference<CableType>>();

                foreach (var reference in cableTypes)
                {
                    if (!existingCableTypeIds.Contains(reference.Identifier))
                    {
                        results[idx].AddFailReason(PortTypeValidationField.CableTypeFks, $"Referenced Cable Type '{reference.Identifier}' does not exist.");
                    }
                }
            }

            return results;
        }

        protected override List<ValidationResult> ValidateBulkForDelete(List<PortType> portTypes)
        {
            if (portTypes == null || !portTypes.Any())
            {
                return new List<ValidationResult>();
            }

            return ValidateNotInUseWhenDeleted(portTypes);
        }

        private List<ValidationResult> ValidateNotInUseWhenDeleted(List<PortType> portTypes)
        {
            var results = portTypes.Select(_ => new ValidationResult()).ToList();

            var identifiers = portTypes
                .Select(pt => pt.Identifier)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            var portTypeIdsUsedByAssetPorts = _entityLoader.GetDataPortsByPortTypeIds(identifiers)
                .Where(port => port.DataPortInfo.PortType.HasValue())
                .Select(port => port.DataPortInfo.PortType.Identifier)
                .Concat(_entityLoader.GetPowerPortsByPortTypeIds(identifiers)
                    .Where(port => port.PowerPortInfo.PortType.HasValue())
                    .Select(port => port.PowerPortInfo.PortType.Identifier))
                .ToHashSet();

            var portTypeIdsUsedByAssetClassPorts = _entityLoader.GetAssetClassesByDataPortTypeIds(identifiers)
                .SelectMany(assetClass => assetClass.DataPorts ?? new List<DataPortInfo>())
                .Where(port => port != null && port.PortType.HasValue())
                .Select(port => port.PortType.Identifier)
                .Concat(_entityLoader.GetAssetClassesByPowerPortTypeIds(identifiers)
                    .SelectMany(assetClass => assetClass.PowerPorts ?? new List<PowerPortInfo>())
                    .Where(port => port != null && port.PortType.HasValue())
                    .Select(port => port.PortType.Identifier))
                .ToHashSet();

            for (int i = 0; i < portTypes.Count; i++)
            {
                if (portTypeIdsUsedByAssetPorts.Contains(portTypes[i].Identifier))
                {
                    results[i].AddFailReason(
                        PortTypeValidationField.AssetPorts,
                        "There are still asset with ports using this port type. Please remove them first.");
                }

                if (portTypeIdsUsedByAssetClassPorts.Contains(portTypes[i].Identifier))
                {
                    results[i].AddFailReason(
                        PortTypeValidationField.AssetClassPorts,
                        "There are still asset classes with ports using this port type. Please remove them first.");
                }
            }

            return results;
        }
    }
}
