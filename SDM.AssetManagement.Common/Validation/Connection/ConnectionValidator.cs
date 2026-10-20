namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Common.Validation;
    using Skyline.DataMiner.SDM.AssetManagement.Extensions;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Common.Services;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.SDM.InfraOps.Common.Validation;
    using Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    using static Skyline.DataMiner.SDM.AssetManagement.Common.Validation.ConnectionValidationHandler;

    public class ConnectionValidator : ValidatorBase<Connection>
    {
        /// <summary>
        /// Maximum number of connections allowed on a single port. The legacy application raised this to 2
        /// for ports on "Connection Panel" device types, but the SDM tag set has no ConnectionPanel tag,
        /// so a single connection per port is enforced uniformly.
        /// </summary>
        private const int MaxConnectionsPerPort = 1;

        private readonly SdmEntityLoader _entityLoader;

        public ConnectionValidator(SdmEntityLoader entityLoader)
        {
            _entityLoader = entityLoader ?? throw new ArgumentNullException(nameof(entityLoader));
        }

        protected override List<ValidationResult> ValidateBulkNoDatabaseBusinessRules(List<Connection> entities)
        {
            var results = entities.Select(_ => new ValidationResult()).ToList();

            for (int idx = 0; idx < entities.Count; idx++)
            {
                ValidateWithoutDatabaseAccess(entities[idx], results[idx]);
            }
            return results;
        }

        protected override List<ValidationResult> ValidateBulkNoDatabaseBatchConflicts(List<Connection> entities)
        {
            return entities.Select(_ => new ValidationResult()).ToList();
        }

        protected override List<ValidationResult> ValidateBulkWithDatabase(List<Connection> entities)
        {
            var results = entities.Select(_ => new ValidationResult()).ToList();

            var lookups = BuildLookups(entities);

            for (int idx = 0; idx < entities.Count; idx++)
            {
                ValidateWithDatabaseAccess(entities[idx], lookups, results[idx]);
            }

            return results;
        }

        private static void ValidateWithoutDatabaseAccess(Connection connection, ValidationResult result)
        {
            if (connection.ShouldValidate(connection.ConnectionTypeField) && !connection.ConnectionType.HasValue)
            {
                result.AddFailReason(ConnectionValidationField.ConnectionType, "Connection type cannot be empty.");
            }

            if (connection.ShouldValidate(connection.CableTypeField) && !connection.CableType.HasValue())
            {
                result.AddFailReason(ConnectionValidationField.CableType, "Cable type cannot be empty.");
            }

            if (connection.ShouldValidate(connection.Source.PortField) && !connection.Source.Port.HasValue())
            {
                result.AddFailReason(ConnectionValidationField.SourcePort, "Source port cannot be empty.");
            }

            if (connection.ShouldValidate(connection.Source.PortTypeField) && !connection.Source.PortType.HasValue())
            {
                result.AddFailReason(ConnectionValidationField.SourcePortType, "Source port type cannot be empty.");
            }

            if (connection.ShouldValidate(connection.Destination.PortField) && !connection.Destination.Port.HasValue())
            {
                result.AddFailReason(ConnectionValidationField.DestinationPort, "Destination port cannot be empty.");
            }

            if (connection.ShouldValidate(connection.Destination.PortTypeField) && !connection.Destination.PortType.HasValue())
            {
                result.AddFailReason(ConnectionValidationField.DestinationPortType, "Destination port type cannot be empty.");
            }

            if (!ConnectionValidationHandler.IsCableLengthValid(connection.CableLength, out var cableLengthResult))
            {
                result.AddFailuresFrom(cableLengthResult);
            }

            var sourcePort = connection.Source.Port;
            var destinationPort = connection.Destination.Port;

            if (!ConnectionValidationHandler.IsNotSelfConnection(sourcePort, destinationPort, out var selfResult))
            {
                result.AddFailuresFrom(selfResult);
            }
        }

        private void ValidateWithDatabaseAccess(Connection connection, Lookups lookups, ValidationResult result)
        {
            ValidateCableType(connection, lookups, result);
            ValidatePortTypeReferences(connection, lookups, result);

            ValidateConnectionType(connection, lookups, result);

            ValidateEndpoint(connection, isSource: true, lookups, result);
            ValidateEndpoint(connection, isSource: false, lookups, result);
        }

        private void ValidateCableType(Connection connection, Lookups lookups, ValidationResult result)
        {
            if (connection.CableType.HasValue() && !lookups.CableTypeIds.Contains(connection.CableType.Identifier))
            {
                result.AddFailReason(
                    ConnectionValidationHandler.ConnectionValidationField.CableType,
                    $"Referenced Cable Type '{connection.CableType.Identifier}' does not exist.");
            }
        }

        private void ValidatePortTypeReferences(Connection connection, Lookups lookups, ValidationResult result)
        {
            if (!connection.Source.IsEmpty)
            {
                ValidatePortTypeReference(connection.Source.PortType, isSource: true, lookups, result);
            }

            if (!connection.Destination.IsEmpty)
            {
                ValidatePortTypeReference(connection.Destination.PortType, isSource: false, lookups, result);
            }
        }

        private void ValidatePortTypeReference(SdmObjectReference<PortType> portType, bool isSource, Lookups lookups, ValidationResult result)
        {
            if (portType.HasValue() && !lookups.PortTypes.ContainsKey(portType.Identifier))
            {
                result.AddFailReason(
                    isSource ? ConnectionValidationHandler.ConnectionValidationField.SourcePortType : ConnectionValidationHandler.ConnectionValidationField.DestinationPortType,
                    $"Referenced Port Type '{portType.Identifier}' does not exist.");
            }
        }

        private void ValidateEndpoint(Connection connection, bool isSource, Lookups lookups, ValidationResult result)
        {
            var endpointPort = isSource ? connection.Source.Port : connection.Destination.Port;
            var field = isSource ? ConnectionValidationHandler.ConnectionValidationField.SourcePort : ConnectionValidationHandler.ConnectionValidationField.DestinationPort;

            if (!endpointPort.HasValue())
            {
                return;
            }

            var resolved = ResolvePort(endpointPort, lookups);
            if (!resolved.Found)
            {
                // The referenced port does not exist in either the data-port or power-port store.
                result.AddFailReason(field, $"Referenced Port '{endpointPort}' does not exist.");
                return;
            }

            if (ConnectionValidationHandler.IsPortDirectionValid(resolved.OutputType, isSource, out var directionResult) == false)
            {
                result.AddFailuresFrom(directionResult);
                return;
            }

            if (IsPortAlreadyInUse(connection, endpointPort, lookups))
            {
                result.AddFailReason(field, $"Port {resolved.PortNumber} is already in use.");
                return;
            }

            if (!connection.ConnectionType.HasValue)
            {
                return;
            }

            var chain = ResolveAssetChain(resolved.Asset, lookups);
            if (ConnectionValidationHandler.IsEndpointAssetValid(chain.Asset, chain.AssetClass, chain.DeviceType, connection.ConnectionType.Value, isSource, out var assetResult) == false)
            {
                result.AddFailuresFrom(assetResult);
            }
        }

        private bool IsPortAlreadyInUse(Connection connection, ISdmObjectReference<IPort> endpointPort, Lookups lookups)
        {
            if (!lookups.PortUsage.TryGetValue(endpointPort.Identifier, out var connectionIds))
            {
                return false;
            }

            var usageCount = connectionIds.Count(id => !string.Equals(id, connection.Identifier, StringComparison.Ordinal));
            return usageCount >= MaxConnectionsPerPort;
        }

        private static void ValidateConnectionType(Connection connection, Lookups lookups, ValidationResult result)
        {
            if (!connection.ConnectionType.HasValue)
            {
                return;
            }

            ValidateEndpointConnectionType(connection, isSource: true, lookups, result);
            ValidateEndpointConnectionType(connection, isSource: false, lookups, result);
        }

        private static void ValidateEndpointConnectionType(Connection connection, bool isSource, Lookups lookups, ValidationResult result)
        {
            var endpointLabel = isSource ? "Source" : "Destination";
            var portField = isSource ? ConnectionValidationField.SourcePort : ConnectionValidationField.DestinationPort;
            var portTypeField = isSource ? ConnectionValidationField.SourcePortType : ConnectionValidationField.DestinationPortType;
            var endpointPort = isSource ? connection.Source.Port : connection.Destination.Port;
            var endpointPortType = isSource ? connection.Source.PortType : connection.Destination.PortType;
            var connectionType = connection.ConnectionType.Value;

            if (endpointPortType.HasValue() && lookups.PortTypes.TryGetValue(endpointPortType.Identifier, out var loadedPortType))
            {
                if (connectionType == SlcAsset_Management.Enums.ConnectionType.Data && !loadedPortType.IsDataPortType())
                {
                    result.AddFailReason(portTypeField, $"{endpointLabel} Port Type must be a Data Port Type for a Data connection.");
                }
                else if (connectionType == SlcAsset_Management.Enums.ConnectionType.Power && !loadedPortType.IsPowerPortType())
                {
                    result.AddFailReason(portTypeField, $"{endpointLabel} Port Type must be a Power Port Type for a Power connection.");
                }
            }

            if (!endpointPort.HasValue())
            {
                return;
            }

            var resolved = ResolvePort(endpointPort, lookups);
            if (!resolved.Found)
            {
                return;
            }

            if (connectionType == SlcAsset_Management.Enums.ConnectionType.Data && !resolved.IsDataPort)
            {
                result.AddFailReason(portField, $"{endpointLabel} port must be a Data Port for a Data connection.");
            }
            else if (connectionType == SlcAsset_Management.Enums.ConnectionType.Power && !resolved.IsPowerPort)
            {
                result.AddFailReason(portField, $"{endpointLabel} port must be a Power Port for a Power connection.");
            }

            if (endpointPortType.HasValue()
                && resolved.PortType.HasValue()
                && !string.Equals(endpointPortType.Identifier, resolved.PortType.Identifier, StringComparison.Ordinal))
            {
                result.AddFailReason(portTypeField, $"{endpointLabel} port type does not match the connected port's port type.");
            }
        }

        private static ResolvedPort ResolvePort(ISdmObjectReference<IPort> portGuid, Lookups lookups)
        {
            var key = portGuid.Identifier;

            if (lookups.DataPorts.TryGetValue(key, out var dataPort))
            {
                return new ResolvedPort
                {
                    Found = true,
                    IsDataPort = true,
                    Asset = dataPort.Asset,
                    PortType = dataPort.DataPortInfo.PortType,
                    OutputType = dataPort.DataPortInfo.OutputType,
                    PortNumber = dataPort.DataPortInfo.PortNumber,
                };
            }

            if (lookups.PowerPorts.TryGetValue(key, out var powerPort))
            {
                return new ResolvedPort
                {
                    Found = true,
                    IsPowerPort = true,
                    Asset = powerPort.Asset,
                    PortType = powerPort.PowerPortInfo.PortType,
                    OutputType = powerPort.PowerPortInfo.OutputType,
                    PortNumber = powerPort.PowerPortInfo.PortNumber,
                };
            }

            return new ResolvedPort { Found = false };
        }

        private static AssetChain ResolveAssetChain(SdmObjectReference<Asset> assetReference, Lookups lookups)
        {
            var chain = new AssetChain();

            if (assetReference.HasValue() && lookups.Assets.TryGetValue(assetReference.Identifier, out var asset))
            {
                chain.Asset = asset;
            }

            if (chain.Asset != null && chain.Asset.AssetClassId.HasValue() && lookups.AssetClasses.TryGetValue(chain.Asset.AssetClassId.Identifier, out var assetClass))
            {
                chain.AssetClass = assetClass;
            }

            if (chain.AssetClass != null && chain.AssetClass.DeviceTypeId.HasValue() && lookups.DeviceTypes.TryGetValue(chain.AssetClass.DeviceTypeId.Identifier, out var deviceType))
            {
                chain.DeviceType = deviceType;
            }

            return chain;
        }

        private Lookups BuildLookups(List<Connection> connections)
        {
            var lookups = new Lookups();

            var cableTypeIds = connections
                .Where(c => c.CableType.HasValue())
                .Select(c => c.CableType.Identifier)
                .Distinct()
                .ToList();
            lookups.CableTypeIds = _entityLoader.GetCableTypesByDomIds(cableTypeIds).Select(ct => ct.Identifier).ToHashSet();

            var portTypeIds = connections
                .SelectMany(GetEndpointPortTypes)
                .Where(reference => reference.HasValue())
                .Select(reference => reference.Identifier)
                .Distinct()
                .ToList();
            lookups.PortTypes = _entityLoader.GetPortTypesByDomIds(portTypeIds)
                .GroupBy(pt => pt.Identifier)
                .ToDictionary(g => g.Key, g => g.First());

            var portIds = connections
                .SelectMany(c => new[] { c.Source.Port, c.Destination.Port })
                .Where(reference => reference.HasValue())
                .Select(reference => reference.Identifier)
                .Distinct()
                .ToList();

            lookups.DataPorts = _entityLoader.GetDataPortsByDomIds(portIds)
                .GroupBy(p => p.Identifier)
                .ToDictionary(g => g.Key, g => g.First());
            lookups.PowerPorts = _entityLoader.GetPowerPortsByDomIds(portIds)
                .GroupBy(p => p.Identifier)
                .ToDictionary(g => g.Key, g => g.First());

            var assetIds = lookups.DataPorts.Values.Select(p => p.Asset)
                .Concat(lookups.PowerPorts.Values.Select(p => p.Asset))
                .Where(reference => reference.HasValue())
                .Select(reference => reference.Identifier)
                .Distinct()
                .ToList();
            lookups.Assets = _entityLoader.GetAssetsByDomIds(assetIds)
                .GroupBy(a => a.Identifier)
                .ToDictionary(g => g.Key, g => g.First());

            var assetClassIds = lookups.Assets.Values
                .Where(a => a.AssetClassId.HasValue())
                .Select(a => a.AssetClassId.Identifier)
                .Distinct()
                .ToList();
            lookups.AssetClasses = _entityLoader.GetAssetClassesByDomIds(assetClassIds)
                .GroupBy(c => c.Identifier)
                .ToDictionary(g => g.Key, g => g.First());

            var deviceTypeIds = lookups.AssetClasses.Values
                .Where(c => c.DeviceTypeId.HasValue())
                .Select(c => c.DeviceTypeId.Identifier)
                .Distinct()
                .ToList();
            lookups.DeviceTypes = _entityLoader.GetDeviceTypesByDomIds(deviceTypeIds)
                .GroupBy(d => d.Identifier)
                .ToDictionary(g => g.Key, g => g.First());

            lookups.PortUsage = BuildPortUsage(portIds);

            return lookups;
        }

        private Dictionary<string, List<string>> BuildPortUsage(List<string> portIds)
        {
            var usage = new Dictionary<string, List<string>>();

            foreach (var existing in _entityLoader.GetConnectionsByPortIds(portIds))
            {
                AddPortUsage(usage, existing.Source.Port, existing.Identifier);
                AddPortUsage(usage, existing.Destination.Port, existing.Identifier);
            }

            return usage;
        }

        private static IEnumerable<SdmObjectReference<PortType>> GetEndpointPortTypes(Connection connection)
        {
            if (!connection.Source.IsEmpty)
            {
                yield return connection.Source.PortType;
            }

            if (!connection.Destination.IsEmpty)
            {
                yield return connection.Destination.PortType;
            }
        }

        private static void AddPortUsage(Dictionary<string, List<string>> usage, ISdmObjectReference<IPort> port, string connectionId)
        {
            if (port == default(ISdmObjectReference<IPort>))
            {
                return;
            }

            var key = port.Identifier;
            if (!usage.TryGetValue(key, out var ids))
            {
                ids = new List<string>();
                usage[key] = ids;
            }

            ids.Add(connectionId);
        }

        private sealed class Lookups
        {
            public HashSet<string> CableTypeIds { get; set; } = new HashSet<string>();

            public Dictionary<string, PortType> PortTypes { get; set; } = new Dictionary<string, PortType>();

            public Dictionary<string, DataPort> DataPorts { get; set; } = new Dictionary<string, DataPort>();

            public Dictionary<string, PowerPort> PowerPorts { get; set; } = new Dictionary<string, PowerPort>();

            public Dictionary<string, Asset> Assets { get; set; } = new Dictionary<string, Asset>();

            public Dictionary<string, AssetClass> AssetClasses { get; set; } = new Dictionary<string, AssetClass>();

            public Dictionary<string, DeviceType> DeviceTypes { get; set; } = new Dictionary<string, DeviceType>();

            public Dictionary<string, List<string>> PortUsage { get; set; } = new Dictionary<string, List<string>>();
        }

        private struct ResolvedPort
        {
            public bool Found { get; set; }

            public bool IsDataPort { get; set; }

            public bool IsPowerPort { get; set; }

            public SdmObjectReference<Asset> Asset { get; set; }

            public SdmObjectReference<PortType> PortType { get; set; }

            public SlcAsset_Management.Enums.Outputtype? OutputType { get; set; }

            public long? PortNumber { get; set; }
        }

        private sealed class AssetChain
        {
            public Asset Asset { get; set; }

            public AssetClass AssetClass { get; set; }

            public DeviceType DeviceType { get; set; }
        }
    }
}