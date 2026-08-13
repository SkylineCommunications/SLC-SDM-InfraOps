namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Common.Validation;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Common.Services;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

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
            var indexesRequiringDatabaseAccess = new List<int>();

            for (int i = 0; i < connections.Count; i++)
            {
                ValidateWithoutDatabaseAccess(connections[i], results[i]);
                if (results[i].IsValid)
                {
                    indexesRequiringDatabaseAccess.Add(i);
                }
            }

            if (!indexesRequiringDatabaseAccess.Any())
            {
                return results;
            }

            var connectionsRequiringDatabaseAccess = indexesRequiringDatabaseAccess.Select(index => connections[index]).ToList();
            var lookups = BuildLookups(connectionsRequiringDatabaseAccess);

            foreach (var index in indexesRequiringDatabaseAccess)
            {
                ValidateWithDatabaseAccess(connections[index], lookups, results[index]);
            }

            return results;
        }

        private static void ValidateWithoutDatabaseAccess(Connection connection, ValidationResult result)
        {
            if (ConnectionValidationHandler.IsCableLengthValid(connection.CableLength, out var cableLengthResult) == false)
            {
                result.AddFailuresFrom(cableLengthResult);
            }

            var sourcePort = connection.Source.Port;
            var destinationPort = connection.Destination.Port;

            if (ConnectionValidationHandler.IsNotSelfConnection(sourcePort, destinationPort, out var selfResult) == false)
            {
                result.AddFailuresFrom(selfResult);
                return;
            }
        }

        private void ValidateWithDatabaseAccess(Connection connection, Lookups lookups, ValidationResult result)
        {
            ValidateCableType(connection, lookups, result);
            ValidatePortTypeReferences(connection, lookups, result);

            var connectionType = ResolveConnectionType(connection, lookups);

            ValidateEndpoint(connection, isSource: true, connectionType, lookups, result);
            ValidateEndpoint(connection, isSource: false, connectionType, lookups, result);
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
            if (portType.HasValue() && !lookups.PortTypeIds.Contains(portType.Identifier))
            {
                result.AddFailReason(
                    isSource ? ConnectionValidationHandler.ConnectionValidationField.SourcePortType : ConnectionValidationHandler.ConnectionValidationField.DestinationPortType,
                    $"Referenced Port Type '{portType.Identifier}' does not exist.");
            }
        }

        private void ValidateEndpoint(Connection connection, bool isSource, SlcAsset_Management.Enums.ConnectionType? connectionType, Lookups lookups, ValidationResult result)
        {
            var endpointPort = isSource ? connection.Source.Port : connection.Destination.Port;
            var field = isSource ? ConnectionValidationHandler.ConnectionValidationField.SourcePort : ConnectionValidationHandler.ConnectionValidationField.DestinationPort;

            // Connections may legitimately be patched on a single end. Because the Connection model does not
            // track per-field changes (unlike the legacy application, which gated the "port must be selected"
            // check on the port field being changed), an unset endpoint is treated as "not being connected"
            // and skipped rather than rejected. Endpoints that are set are still fully validated.
            if (endpointPort == Guid.Empty)
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

            if (!connectionType.HasValue)
            {
                return;
            }

            var chain = ResolveAssetChain(resolved.Asset, lookups);
            if (ConnectionValidationHandler.IsEndpointAssetValid(chain.Asset, chain.AssetClass, chain.DeviceType, connectionType.Value, isSource, out var assetResult) == false)
            {
                result.AddFailuresFrom(assetResult);
            }
        }

        private bool IsPortAlreadyInUse(Connection connection, Guid endpointPort, Lookups lookups)
        {
            if (!lookups.PortUsage.TryGetValue(endpointPort.ToString(), out var connectionIds))
            {
                return false;
            }

            var usageCount = connectionIds.Count(id => !string.Equals(id, connection.Identifier, StringComparison.Ordinal));
            return usageCount >= MaxConnectionsPerPort;
        }

        private static SlcAsset_Management.Enums.ConnectionType? ResolveConnectionType(Connection connection, Lookups lookups)
        {
            if (connection.ConnectionType.HasValue)
            {
                return connection.ConnectionType;
            }

            var sourcePort = connection.Source.Port;
            if (sourcePort != Guid.Empty)
            {
                if (lookups.DataPorts.ContainsKey(sourcePort.ToString()))
                {
                    return SlcAsset_Management.Enums.ConnectionType.Data;
                }

                if (lookups.PowerPorts.ContainsKey(sourcePort.ToString()))
                {
                    return SlcAsset_Management.Enums.ConnectionType.Power;
                }
            }

            return null;
        }

        private static ResolvedPort ResolvePort(Guid portGuid, Lookups lookups)
        {
            var key = portGuid.ToString();

            if (lookups.DataPorts.TryGetValue(key, out var dataPort))
            {
                return new ResolvedPort
                {
                    Found = true,
                    Asset = dataPort.Asset,
                    OutputType = dataPort.DataPortInfo.OutputType,
                    PortNumber = dataPort.DataPortInfo.PortNumber,
                };
            }

            if (lookups.PowerPorts.TryGetValue(key, out var powerPort))
            {
                return new ResolvedPort
                {
                    Found = true,
                    Asset = powerPort.Asset,
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
            lookups.PortTypeIds = _entityLoader.GetPortTypesByDomIds(portTypeIds).Select(pt => pt.Identifier).ToHashSet();

            var portIds = connections
                .SelectMany(c => new[] { c.Source.Port, c.Destination.Port })
                .Where(id => id != Guid.Empty)
                .Select(id => id.ToString())
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

        private static void AddPortUsage(Dictionary<string, List<string>> usage, Guid port, string connectionId)
        {
            if (port == Guid.Empty)
            {
                return;
            }

            var key = port.ToString();
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

            public HashSet<string> PortTypeIds { get; set; } = new HashSet<string>();

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

            public SdmObjectReference<Asset> Asset { get; set; }

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
