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

    public class ConnectionValidator : ValidatorBase<Connection>
    {
        private readonly SdmEntityLoader _entityLoader;

        public ConnectionValidator(SdmEntityLoader entityLoader)
        {
            _entityLoader = entityLoader ?? throw new ArgumentNullException(nameof(entityLoader));
        }

        protected override ValidationResult Validate(Connection connection)
        {
            return ValidateBulk(new List<Connection> { connection })[0];
        }

        protected override List<ValidationResult> ValidateBulk(List<Connection> connections)
        {
            if (connections == null || !connections.Any())
            {
                return new List<ValidationResult>();
            }

            var results = connections.Select(_ => new ValidationResult()).ToList();

            var cableTypeIds = connections
                .Where(c => c.CableType != null && c.CableType.HasValue())
                .Select(c => c.CableType.Identifier)
                .Distinct()
                .ToList();
            var existingCableTypeIds = _entityLoader.GetCableTypesByDomIds(cableTypeIds).Select(ct => ct.Identifier).ToHashSet();

            var portTypeIds = connections
                .SelectMany(c => new[] { c.Source == null ? null : c.Source.PortType, c.Destination == null ? null : c.Destination.PortType })
                .Where(reference => reference != null && reference.HasValue())
                .Select(reference => reference.Identifier)
                .Distinct()
                .ToList();
            var existingPortTypeIds = _entityLoader.GetPortTypesByDomIds(portTypeIds).Select(pt => pt.Identifier).ToHashSet();

            var portIds = connections
                .SelectMany(c => new[] { c.Source?.Port ?? Guid.Empty, c.Destination?.Port ?? Guid.Empty })
                .Where(id => id != Guid.Empty)
                .Select(id => id.ToString())
                .Distinct()
                .ToList();
            var existingPortIds = _entityLoader.GetDataPortsByDomIds(portIds).Select(p => p.Identifier)
                .Concat(_entityLoader.GetPowerPortsByDomIds(portIds).Select(p => p.Identifier))
                .ToHashSet();

            for (int i = 0; i < connections.Count; i++)
            {
                var connection = connections[i];
                if (connection.CableType != null && connection.CableType.HasValue() && !existingCableTypeIds.Contains(connection.CableType.Identifier))
                {
                    results[i].AddFailReason(ConnectionValidationHandler.ConnectionValidationField.CableType,
                        $"Referenced Cable Type '{connection.CableType.Identifier}' does not exist.");
                }

                ValidateEndpoint(connection.Source, true, existingPortIds, existingPortTypeIds, results[i]);
                ValidateEndpoint(connection.Destination, false, existingPortIds, existingPortTypeIds, results[i]);
            }

            return results;
        }

        private static void ValidateEndpoint(SourceInfo endpoint, bool isSource, HashSet<string> existingPortIds, HashSet<string> existingPortTypeIds, ValidationResult result)
        {
            if (endpoint == null)
            {
                return;
            }

            if (endpoint.Port != Guid.Empty && !existingPortIds.Contains(endpoint.Port.ToString()))
            {
                result.AddFailReason(
                    isSource ? ConnectionValidationHandler.ConnectionValidationField.SourcePort : ConnectionValidationHandler.ConnectionValidationField.DestinationPort,
                    $"Referenced Port '{endpoint.Port}' does not exist.");
            }

            if (endpoint.PortType != null && endpoint.PortType.HasValue() && !existingPortTypeIds.Contains(endpoint.PortType.Identifier))
            {
                result.AddFailReason(
                    isSource ? ConnectionValidationHandler.ConnectionValidationField.SourcePortType : ConnectionValidationHandler.ConnectionValidationField.DestinationPortType,
                    $"Referenced Port Type '{endpoint.PortType.Identifier}' does not exist.");
            }
        }

        private static void ValidateEndpoint(DestinationInfo endpoint, bool isSource, HashSet<string> existingPortIds, HashSet<string> existingPortTypeIds, ValidationResult result)
        {
            if (endpoint == null)
            {
                return;
            }

            if (endpoint.Port != Guid.Empty && !existingPortIds.Contains(endpoint.Port.ToString()))
            {
                result.AddFailReason(
                    isSource ? ConnectionValidationHandler.ConnectionValidationField.SourcePort : ConnectionValidationHandler.ConnectionValidationField.DestinationPort,
                    $"Referenced Port '{endpoint.Port}' does not exist.");
            }

            if (endpoint.PortType != null && endpoint.PortType.HasValue() && !existingPortTypeIds.Contains(endpoint.PortType.Identifier))
            {
                result.AddFailReason(
                    isSource ? ConnectionValidationHandler.ConnectionValidationField.SourcePortType : ConnectionValidationHandler.ConnectionValidationField.DestinationPortType,
                    $"Referenced Port Type '{endpoint.PortType.Identifier}' does not exist.");
            }
        }
    }
}
