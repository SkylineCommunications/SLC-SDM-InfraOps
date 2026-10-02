namespace Skyline.DataMiner.SDM.InfraOps.Orchestration.AssetDeletion
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Deletion;
    using Skyline.DataMiner.SDM.AssetManagement.Helpers;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences;
    using Skyline.DataMiner.SDM.InfraOpsProperties.Extensions;
    using Skyline.DataMiner.SDM.InfraOpsProperties.Helpers;
    using Skyline.DataMiner.SDM.PlanAndBuild.Deletion;

    using Connection = Skyline.DataMiner.SDM.AssetManagement.Models.Connection;

    /// <summary>
    /// Performs the shared Asset dependency cascade after the Asset DOM delete succeeds.
    /// </summary>
    public sealed class AssetDeletionMiddleware : AssetDeletionMiddlewareBase
    {
        private readonly AssetDeletionPolicy _policy;
        private IAssetManagementApiHelper _assetManagement;
        private IAssetDeletionJobCleanup _jobCleanup;
        private IInfraOpsPropertiesApiHelper _properties;

        public AssetDeletionMiddleware()
            : this(AssetDeletionPolicy.Default)
        {
        }

        public AssetDeletionMiddleware(AssetDeletionPolicy policy)
        {
            _policy = policy ?? throw new ArgumentNullException(nameof(policy));
        }

        internal void Configure(
            IAssetManagementApiHelper assetManagement,
            IAssetDeletionJobCleanup jobCleanup,
            IInfraOpsPropertiesApiHelper properties)
        {
            if (assetManagement == null)
            {
                throw new ArgumentNullException(nameof(assetManagement));
            }

            if (jobCleanup == null)
            {
                throw new ArgumentNullException(nameof(jobCleanup));
            }

            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            if (_assetManagement != null || _jobCleanup != null || _properties != null)
            {
                throw new InvalidOperationException("Asset deletion cascade participants were already configured.");
            }

            _assetManagement = assetManagement;
            _jobCleanup = jobCleanup;
            _properties = properties;
        }

        public override void OnDelete(Asset asset, Action<Asset> next)
        {
            if (asset == null)
            {
                throw new ArgumentNullException(nameof(asset));
            }

            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            EnsureConfigured();
            if (string.IsNullOrWhiteSpace(asset.Identifier))
            {
                throw new ArgumentException("Asset identifier cannot be empty.", nameof(asset));
            }

            var existingAsset = _assetManagement.Assets
                .Read(AssetExposers.Identifier.Equal(asset.Identifier))
                .SingleOrDefault();
            var context = existingAsset == null ? null : CaptureAssetContext(
                existingAsset,
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    [existingAsset.Identifier] = existingAsset.Name,
                });
            try
            {
                next(asset);
            }
            catch (Exception domException)
            {
                throw new AssetDeletionCascadeException(
                    new[]
                    {
                        new AssetDeletionOutcome(
                            asset.Identifier,
                            false,
                            "Asset DOM delete",
                            domException),
                    },
                    domException);
            }

            if (context != null)
            {
                try
                {
                    Cleanup(ref context);
                }
                catch (AssetDeletionStageFailureException failure)
                {
                    throw new AssetDeletionCascadeException(
                        new[]
                        {
                            new AssetDeletionOutcome(
                                asset.Identifier,
                                true,
                                failure.Stage,
                                failure.InnerException,
                                context),
                        },
                        null);
                }
            }
        }

        public override void OnDelete(IEnumerable<Asset> assets, Action<IEnumerable<Asset>> next)
        {
            if (assets == null)
            {
                throw new ArgumentNullException(nameof(assets));
            }

            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            var items = assets.ToList();
            if (items.Count == 0)
            {
                next(items);
                return;
            }

            EnsureConfigured();
            var identifiers = items
                .Where(asset => asset != null)
                .GroupBy(asset => asset.Identifier, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.Key)
                .ToList();
            if (identifiers.Any(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException("Asset identifier cannot be empty.", nameof(assets));
            }

            var existingAssets = ReadExistingAssets(identifiers);
            var names = existingAssets.ToDictionary(item => item.Identifier, item => item.Name, StringComparer.OrdinalIgnoreCase);
            var contexts = existingAssets.ToDictionary(
                item => item.Identifier,
                item => CaptureAssetContext(item, names),
                StringComparer.OrdinalIgnoreCase);
            var existingBeforeDelete = new HashSet<string>(contexts.Keys, StringComparer.OrdinalIgnoreCase);

            Exception domException = null;
            try
            {
                next(items);
            }
            catch (Exception exception)
            {
                domException = exception;
            }

            var successfulIdentifiers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (domException == null)
            {
                successfulIdentifiers.UnionWith(existingBeforeDelete);
            }
            else
            {
                try
                {
                    var existingAfterDelete = ReadExistingAssetIdentifiers(identifiers);
                    foreach (var identifier in existingBeforeDelete)
                    {
                        if (!existingAfterDelete.Contains(identifier))
                        {
                            successfulIdentifiers.Add(identifier);
                        }
                    }
                }
                catch (Exception discoveryException)
                {
                    var combinedException = new AggregateException(domException, discoveryException);
                    throw new AssetDeletionCascadeException(
                        identifiers.Select(identifier => new AssetDeletionOutcome(
                            identifier,
                            false,
                            "Asset DOM result discovery",
                            combinedException)).ToList(),
                        domException);
                }
            }

            var outcomes = new List<AssetDeletionOutcome>();
            foreach (var identifier in identifiers)
            {
                if (successfulIdentifiers.Contains(identifier))
                {
                    continue;
                }

                outcomes.Add(new AssetDeletionOutcome(
                    identifier,
                    false,
                    "Asset DOM delete",
                    domException));
            }

            foreach (var identifier in successfulIdentifiers)
            {
                var context = contexts[identifier];
                try
                {
                    Cleanup(ref context);
                    outcomes.Add(new AssetDeletionOutcome(identifier, true, null, null));
                }
                catch (AssetDeletionStageFailureException failure)
                {
                    outcomes.Add(new AssetDeletionOutcome(
                        identifier,
                        true,
                        failure.Stage,
                        failure.InnerException,
                        context));
                }
            }

            if (domException != null || outcomes.Any(outcome => !string.IsNullOrEmpty(outcome.FailedStage)))
            {
                throw new AssetDeletionCascadeException(outcomes, domException);
            }
        }

        public override void RecoverAssetDeletion(string assetIdentifier)
        {
            if (string.IsNullOrWhiteSpace(assetIdentifier))
            {
                throw new ArgumentException("Asset identifier cannot be empty.", nameof(assetIdentifier));
            }

            if (!_policy.Matches(AssetDeletionPolicy.Default))
            {
                throw new InvalidOperationException("Custom-policy recovery requires the original AssetDeletionRecoveryContext.");
            }

            RecoverAssetDeletion(new AssetDeletionRecoveryContext(
                assetIdentifier, _policy, null, null, null,
                new Dictionary<string, string>(), Array.Empty<AssetDeletionConnectionSnapshot>()));
        }

        public override void RecoverAssetDeletion(AssetDeletionRecoveryContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!_policy.Matches(context.Policy))
            {
                throw new InvalidOperationException("The recovery context policy must match this composition's deletion policy.");
            }

            EnsureConfigured();
            var assetIdentifier = context.AssetIdentifier;
            if (_assetManagement.Assets.Read(AssetExposers.Identifier.Equal(assetIdentifier)).Any())
            {
                throw new InvalidOperationException(
                    $"Asset '{assetIdentifier}' still exists; recovery is only valid after its DOM deletion succeeds.");
            }

            try
            {
                Cleanup(ref context);
            }
            catch (AssetDeletionStageFailureException failure)
            {
                throw new AssetDeletionCascadeException(
                    new[] { new AssetDeletionOutcome(assetIdentifier, true, failure.Stage, failure.InnerException, context) },
                    null);
            }
        }

        private AssetDeletionRecoveryContext CaptureAssetContext(
            Asset asset,
            IReadOnlyDictionary<string, string> deletedAssetNames)
        {
            string className = null;
            string ipAddress = null;
            if (_policy.AssetJobEntryMode == JobEntryRemovalMode.KeepRemovedSnapshot)
            {
                if (!string.IsNullOrWhiteSpace(asset.AssetClassId.Identifier))
                {
                    className = _assetManagement.AssetClasses
                        .Read(AssetClassExposers.Identifier.Equal(asset.AssetClassId.Identifier))
                        .SingleOrDefault()?.Name;
                }

                ipAddress = ReadDataPorts(asset.Identifier)
                    .FirstOrDefault(port => port.PrimaryPortRelation.IsPrimaryIpv4)?.AddressInfo.Ipv4Address;
            }

            return new AssetDeletionRecoveryContext(
                asset.Identifier, _policy, asset.Name, className, ipAddress, deletedAssetNames,
                Array.Empty<AssetDeletionConnectionSnapshot>());
        }

        private AssetDeletionSnapshot CaptureConnectionDependencies(AssetDeletionRecoveryContext context)
        {
            var assetIdentifier = context.AssetIdentifier;
            var assetReference = new SdmObjectReference<Asset>(assetIdentifier);
            var dataPorts = _assetManagement.DataPorts
                .Read(DataPortExposers.Asset.Equal(assetReference))
                .ToList();
            var powerPorts = _assetManagement.PowerPorts
                .Read(PowerPortExposers.Asset.Equal(assetReference))
                .ToList();

            var portIds = dataPorts.Concat<IPort>(powerPorts)
                .Select(port => Guid.Parse(port.Identifier))
                .Distinct()
                .ToList();

            var connections = ReadConnectionsByPortIds(portIds);
            var retainedSnapshots = context.ConnectionSnapshots
                .GroupBy(snapshot => snapshot.ConnectionIdentifier, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
            var endpointNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var useEndpointNames = _policy.ConnectionJobEntryMode == JobEntryRemovalMode.KeepRemovedSnapshot &&
                _policy.ConnectionSnapshotFormat == ConnectionSnapshotFormat.AssetAndPortNames;
            if (useEndpointNames)
            {
                foreach (var portReference in connections
                    .Where(connection => !retainedSnapshots.ContainsKey(connection.Identifier))
                    .SelectMany(connection =>
                    new[] { connection.Source.Port, connection.Destination.Port })
                    .Where(reference => !ReferenceEquals(reference, null))
                    .GroupBy(reference => reference.Identifier, StringComparer.OrdinalIgnoreCase)
                    .Select(group => group.First()))
                {
                    var port = _assetManagement.Ports.Read(PortExposers.Identifier.Equal(portReference.Identifier)).Single();
                    if (!context.DeletedAssetNames.TryGetValue(port.Asset.Identifier, out var name))
                    {
                        name = _assetManagement.Assets
                            .Read(AssetExposers.Identifier.Equal(port.Asset.Identifier)).Single().Name;
                    }

                    endpointNames[port.Identifier] = $"{name} - {port.PortInfo.Name}";
                }
            }

            var snapshots = connections
                .Select(connection => retainedSnapshots.TryGetValue(connection.Identifier, out var retained)
                    ? retained
                    : new AssetDeletionConnectionSnapshot(
                    connection.Identifier,
                    useEndpointNames ? endpointNames[connection.Source.Port.Identifier] : connection.Source.CableTag,
                    useEndpointNames ? endpointNames[connection.Destination.Port.Identifier] : connection.Destination.CableTag,
                    connection.CableType.Identifier,
                    connection.CableLength))
                .ToList();

            return new AssetDeletionSnapshot(connections, snapshots);
        }

        private List<Connection> ReadConnectionsByPortIds(IReadOnlyCollection<Guid> portIds)
        {
            if (portIds.Count == 0)
            {
                return new List<Connection>();
            }

            var filters = portIds
                .SelectMany(portId => new FilterElement<Connection>[]
                {
                    ConnectionExposers.Source.Port.Equal(new ISdmObjectReference<IPort>(portId.ToString())),
                    ConnectionExposers.Destination.Port.Equal(new ISdmObjectReference<IPort>(portId.ToString())),
                })
                .ToArray();

            var portIdSet = new HashSet<Guid>(portIds);
            return _assetManagement.Connections
                .Read(new ORFilterElement<Connection>(filters))
                .Where(connection =>
                    IsReferencedPort(connection.Source.Port, portIdSet) ||
                    IsReferencedPort(connection.Destination.Port, portIdSet))
                .GroupBy(connection => connection.Identifier, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList();
        }

        private static bool IsReferencedPort(
            ISdmObjectReference<IPort> portReference,
            ISet<Guid> portIds)
        {
            return !ReferenceEquals(portReference, null) &&
                Guid.TryParse(portReference.Identifier, out var portId) &&
                portIds.Contains(portId);
        }

        private void Cleanup(ref AssetDeletionRecoveryContext context)
        {
            var assetIdentifier = context.AssetIdentifier;
            Guid linkedObjectId;
            try
            {
                linkedObjectId = Guid.Parse(assetIdentifier);
            }
            catch (Exception exception)
            {
                throw new AssetDeletionStageFailureException(assetIdentifier, "PropertyValues identifier", exception);
            }

            AssetDeletionSnapshot currentSnapshot;
            try
            {
                currentSnapshot = CaptureConnectionDependencies(context);
            }
            catch (Exception exception)
            {
                throw new AssetDeletionStageFailureException(assetIdentifier, "Dependency snapshot", exception);
            }

            context = context.WithConnectionSnapshots(context.ConnectionSnapshots
                .Concat(currentSnapshot.JobConnectionSnapshots)
                .GroupBy(snapshot => snapshot.ConnectionIdentifier, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList());
            var cleanupContext = context;
            ExecuteStage(assetIdentifier, "PlanAndBuild Jobs", () =>
                _jobCleanup.RemoveDeletedAssetReferences(cleanupContext));

            ExecuteStage(assetIdentifier, "Connections", () =>
            {
                if (currentSnapshot.Connections.Count > 0)
                {
                    _assetManagement.Connections.Delete(currentSnapshot.Connections);
                }
            });

            ExecuteStage(assetIdentifier, "DataPorts", () =>
            {
                var dataPorts = ReadDataPorts(assetIdentifier);
                if (dataPorts.Count > 0)
                {
                    _assetManagement.DataPorts.Delete(dataPorts);
                }
            });

            ExecuteStage(assetIdentifier, "PowerPorts", () =>
            {
                var powerPorts = ReadPowerPorts(assetIdentifier);
                if (powerPorts.Count > 0)
                {
                    _assetManagement.PowerPorts.Delete(powerPorts);
                }
            });

            ExecuteStage(assetIdentifier, "Child Assets", () =>
            {
                var children = ReadDirectChildren(assetIdentifier);
                foreach (var child in children)
                {
                    ClearLocationIfParentMatches(child.Location, assetIdentifier);
                    ClearLocationIfParentMatches(child.DestinationLocation, assetIdentifier);
                }

                if (children.Count > 0)
                {
                    _assetManagement.Assets.Update(children);
                }
            });

            ExecuteStage(assetIdentifier, "PropertyValues", () =>
            {
                var values = _properties.PropertyValues
                    .GetByLinkedObjectID(linkedObjectId, "Asset", "*")
                    .ToList();
                if (values.Count > 0)
                {
                    _properties.PropertyValues.Delete(values);
                }
            });
        }

        // TODO: Reuse the Asset port query extensions when the other development branches are merged.
        private List<DataPort> ReadDataPorts(string assetIdentifier)
        {
            return _assetManagement.DataPorts
                .Read(DataPortExposers.Asset.Equal(new SdmObjectReference<Asset>(assetIdentifier)))
                .ToList();
        }

        private List<PowerPort> ReadPowerPorts(string assetIdentifier)
        {
            return _assetManagement.PowerPorts
                .Read(PowerPortExposers.Asset.Equal(new SdmObjectReference<Asset>(assetIdentifier)))
                .ToList();
        }

        private List<Asset> ReadDirectChildren(string assetIdentifier)
        {
            var parentReference = new SdmObjectReference<Asset>(assetIdentifier);
            var filter = new ORFilterElement<Asset>(
                AssetExposers.Location.ParentAsset.Equal(parentReference),
                AssetExposers.DestinationLocation.ParentAsset.Equal(parentReference));

            return _assetManagement.Assets.Read(filter)
                .Where(child =>
                    child.Identifier != assetIdentifier &&
                    (child.Location.ParentAsset.Identifier == assetIdentifier ||
                     child.DestinationLocation.ParentAsset.Identifier == assetIdentifier))
                .GroupBy(child => child.Identifier, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList();
        }

        private HashSet<string> ReadExistingAssetIdentifiers(IEnumerable<string> identifiers)
        {
            return new HashSet<string>(
                ReadExistingAssets(identifiers).Select(asset => asset.Identifier),
                StringComparer.OrdinalIgnoreCase);
        }

        private List<Asset> ReadExistingAssets(IEnumerable<string> identifiers)
        {
            var keys = identifiers.Where(identifier => !string.IsNullOrWhiteSpace(identifier)).Distinct().ToList();
            if (keys.Count == 0)
            {
                return new List<Asset>();
            }

            var filter = new ORFilterElement<Asset>(
                keys.Select(identifier => AssetExposers.Identifier.Equal(identifier)).ToArray());

            return _assetManagement.Assets.Read(filter)
                .ToList();
        }

        private static void ClearLocationIfParentMatches(AssetLocation location, string assetIdentifier)
        {
            if (!string.Equals(location.ParentAsset.Identifier, assetIdentifier, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            location.ParentAsset = default;
            location.HolderNumber = null;
            location.RackId = default;
            location.RackPosition = null;
            location.Side = null;
            location.DeskId = default;
            location.ContainerId = default;
            location.RoomId = default;
        }

        private static void ExecuteStage(string assetIdentifier, string stage, Action action)
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                throw new AssetDeletionStageFailureException(assetIdentifier, stage, exception);
            }
        }

        private void EnsureConfigured()
        {
            if (_assetManagement == null || _jobCleanup == null || _properties == null)
            {
                throw new InvalidOperationException(
                    "Asset deletion cascade requires Asset Management, Plan & Build, and InfraOps Properties participants.");
            }
        }

        private sealed class AssetDeletionSnapshot
        {
            public AssetDeletionSnapshot(
                List<Connection> connections,
                List<AssetDeletionConnectionSnapshot> connectionSnapshots)
            {
                Connections = connections;
                JobConnectionSnapshots = connectionSnapshots;
            }

            public List<Connection> Connections { get; }

            public List<AssetDeletionConnectionSnapshot> JobConnectionSnapshots { get; }
        }
    }
}
