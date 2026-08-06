namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Common.Services;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    public class PortTypeValidator : ValidatorBase<PortType>
    {
        private readonly SdmEntityLoader _entityLoader;

        public PortTypeValidator(SdmEntityLoader entityLoader)
        {
            _entityLoader = entityLoader ?? throw new ArgumentNullException(nameof(entityLoader));
        }

        protected override ValidationResult Validate(PortType entity)
        {
            return ValidateCableTypeReferences(entity);
        }

        protected override List<ValidationResult> ValidateBulk(List<PortType> entities)
        {
            if (entities == null || !entities.Any())
            {
                return new List<ValidationResult>();
            }

            var results = entities.Select(_ => new ValidationResult()).ToList();
            var cableTypeIds = entities
                .SelectMany(pt => pt.CableFKs?.CableTypeFks ?? new List<SdmObjectReference<CableType>>())
                .Where(reference => reference != null && reference.HasValue())
                .Select(reference => reference.Identifier)
                .Distinct()
                .ToList();
            var existingCableTypeIds = _entityLoader.GetCableTypesByDomIds(cableTypeIds).Select(ct => ct.Identifier).ToHashSet();

            for (int i = 0; i < entities.Count; i++)
            {
                foreach (var reference in entities[i].CableFKs?.CableTypeFks ?? new List<SdmObjectReference<CableType>>())
                {
                    if (reference != null && reference.HasValue() && !existingCableTypeIds.Contains(reference.Identifier))
                    {
                        results[i].AddFailReason("PortType.CableFKs.CableTypeFks", "CableTypeFks", $"Referenced Cable Type '{reference.Identifier}' does not exist.");
                    }
                }
            }

            return results;
        }

        protected override ValidationResult ValidateForDelete(PortType portType)
        {
            if (portType == null)
            {
                throw new ArgumentNullException(nameof(portType));
            }

            return ValidateNotInUseWhenDeleted(new List<PortType> { portType })[0];
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
                .Where(port => port.DataPortInfo?.Type != null && port.DataPortInfo.Type.HasValue())
                .Select(port => port.DataPortInfo.Type.Identifier)
                .Concat(_entityLoader.GetPowerPortsByPortTypeIds(identifiers)
                    .Where(port => port.PowerPortInfo?.PortType != null && port.PowerPortInfo.PortType.HasValue())
                    .Select(port => port.PowerPortInfo.PortType.Identifier))
                .ToHashSet();

            var portTypeIdsUsedByAssetClassPorts = _entityLoader.GetAssetClassesByDataPortTypeIds(identifiers)
                .SelectMany(assetClass => assetClass.DataPorts ?? new List<DataPortInfo>())
                .Where(port => port?.Type != null && port.Type.HasValue())
                .Select(port => port.Type.Identifier)
                .Concat(_entityLoader.GetAssetClassesByPowerPortTypeIds(identifiers)
                    .SelectMany(assetClass => assetClass.PowerPorts ?? new List<PowerPortInfo>())
                    .Where(port => port?.PortType != null && port.PortType.HasValue())
                    .Select(port => port.PortType.Identifier))
                .ToHashSet();

            for (int i = 0; i < portTypes.Count; i++)
            {
                if (portTypeIdsUsedByAssetPorts.Contains(portTypes[i].Identifier))
                {
                    results[i].AddFailReason(
                        "PortType.AssetPorts",
                        "AssetPorts",
                        "There are still asset with ports using this port type. Please remove them first.");
                }

                if (portTypeIdsUsedByAssetClassPorts.Contains(portTypes[i].Identifier))
                {
                    results[i].AddFailReason(
                        "PortType.AssetClassPorts",
                        "AssetClassPorts",
                        "There are still asset classes with ports using this port type. Please remove them first.");
                }
            }

            return results;
        }

        private ValidationResult ValidateCableTypeReferences(PortType portType)
        {
            var result = new ValidationResult();
            var references = portType?.CableFKs?.CableTypeFks ?? new List<SdmObjectReference<CableType>>();
            var identifiers = references
                .Where(reference => reference != null && reference.HasValue())
                .Select(reference => reference.Identifier)
                .Distinct()
                .ToList();

            var existingCableTypeIds = _entityLoader.GetCableTypesByDomIds(identifiers).Select(ct => ct.Identifier).ToHashSet();
            foreach (var reference in references)
            {
                if (reference != null && reference.HasValue() && !existingCableTypeIds.Contains(reference.Identifier))
                {
                    result.AddFailReason("PortType.CableFKs.CableTypeFks", "CableTypeFks", $"Referenced Cable Type '{reference.Identifier}' does not exist.");
                }
            }

            return result;
        }
    }
}
