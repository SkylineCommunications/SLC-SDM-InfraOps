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
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Extensions;

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

        public override void OnDelete(Asset item, Action<Asset> next)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            EnsureConfigured();

            if (string.IsNullOrWhiteSpace(item.Identifier))
            {
                throw new ArgumentException("Asset identifier cannot be empty.", nameof(item));
            }

            var existingAsset = _assetManagement.Assets.ReadByIdentifier(item.Identifier);

            var context = existingAsset == null ? null : CaptureAssetContext(
                existingAsset,
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    [existingAsset.Identifier] = existingAsset.Name,
                });

            try
            {
                next(item);
            }
            catch (Exception domException)
            {
                throw new AssetDeletionCascadeException(
                    new[]
                    {
                        new AssetDeletionOutcome(
                            item.Identifier,
                            false,
                            "Asset DOM delete",
                            domException),
                    },
                    domException);
            }

            if (context != null)
            {
                var outcome = CleanupDeletedAsset(context);
                if (!string.IsNullOrEmpty(outcome.FailedStage))
                {
                    throw new AssetDeletionCascadeException(
                        new[] { outcome },
                        null);
                }
            }
        }

        public override void OnDelete(IEnumerable<Asset> items, Action<IEnumerable<Asset>> next)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            var itemList = items.ToList();
            if (itemList.Count == 0)
            {
                next(itemList);
                return;
            }

            EnsureConfigured();


            if (itemList.Any(asset => asset != null && string.IsNullOrWhiteSpace(asset.Identifier)))
            {
                throw new ArgumentException("Asset identifier cannot be empty.", nameof(items));
            }

            var identifiers = itemList
                .Where(asset => asset != null)
                .Select(asset => asset.Identifier)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var contexts = CaptureDeletionContexts(identifiers);

            Exception domException = null;

            try
            {
                next(itemList);
            }
            catch (Exception exception)
            {
                domException = exception;
            }

            CompleteBulkDeletion(identifiers, contexts, domException);
        }

        /// <summary>
        /// Retries dependency cleanup using only the identifier of an Asset whose DOM deletion has succeeded.
        /// </summary>
        /// <param name="assetIdentifier">The deleted Asset DOM identifier.</param>
        /// <remarks>
        /// Only the default deletion policy supports identifier-only recovery. Original Asset metadata is
        /// unnecessary because this policy removes Asset Job entries. Connection snapshots use cable tags
        /// from Connections that still exist; snapshots of already-deleted Connections cannot be reconstructed.
        /// Prefer <see cref="RecoverAssetDeletion(AssetDeletionRecoveryContext)"/> with the original failure
        /// context whenever available. Custom policies require that context.
        /// </remarks>
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

        /// <summary>
        /// Retries dependency cleanup after successful Asset DOM deletion using captured recovery data.
        /// </summary>
        /// <param name="context">The original recovery context from the failed deletion outcome.</param>
        /// <remarks>
        /// Preferred for both default and custom policies because captured Asset metadata and Connection
        /// snapshots remain available even after their source objects have been deleted.
        /// The context policy must match this middleware's deletion policy, and the Asset must no longer exist.
        /// If recovery fails, use the context from the new failure outcome for the next attempt.
        /// </remarks>
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

            var outcome = CleanupDeletedAsset(context);
            if (!string.IsNullOrEmpty(outcome.FailedStage))
            {
                throw new AssetDeletionCascadeException(
                    new[] { outcome },
                    null);
            }
        }

        private Dictionary<string, AssetDeletionRecoveryContext> CaptureDeletionContexts(
            IEnumerable<string> identifiers)
        {
            var existingAssets = _assetManagement.Assets.ReadByIdentifiers(identifiers);
            var names = existingAssets.ToDictionary(item => item.Identifier, item => item.Name, StringComparer.OrdinalIgnoreCase);
            return existingAssets.ToDictionary(
                item => item.Identifier,
                item => CaptureAssetContext(item, names),
                StringComparer.OrdinalIgnoreCase);
        }

        private void CompleteBulkDeletion(
            List<string> identifiers,
            Dictionary<string, AssetDeletionRecoveryContext> contexts,
            Exception domException)
        {
            var successfulIdentifiers = new HashSet<string>(contexts.Keys, StringComparer.OrdinalIgnoreCase);

            if (domException != null)
            {
                try
                {
                    successfulIdentifiers = DiscoverSuccessfulAssetDeletions(identifiers, contexts.Keys);
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

            var outcomes = identifiers
                .Where(identifier => !successfulIdentifiers.Contains(identifier))
                .Select(identifier => new AssetDeletionOutcome(
                    identifier,
                    false,
                    "Asset DOM delete",
                    domException))
                .ToList();

            foreach (var identifier in successfulIdentifiers)
            {
                outcomes.Add(CleanupDeletedAsset(contexts[identifier]));
            }

            if (domException != null || outcomes.Any(outcome => !string.IsNullOrEmpty(outcome.FailedStage)))
            {
                throw new AssetDeletionCascadeException(outcomes, domException);
            }
        }

        private HashSet<string> DiscoverSuccessfulAssetDeletions(
            List<string> identifiers,
            IEnumerable<string> existingIdentifiers)
        {
            var successfulIdentifiers = new HashSet<string>(existingIdentifiers, StringComparer.OrdinalIgnoreCase);
            var existingAfterDelete = _assetManagement.Assets.ReadByIdentifiers(identifiers)
                .Select(asset => asset.Identifier);
            successfulIdentifiers.ExceptWith(existingAfterDelete);

            return successfulIdentifiers;
        }

        private AssetDeletionOutcome CleanupDeletedAsset(AssetDeletionRecoveryContext context)
        {
            try
            {
                var preparation = PrepareCleanup(context);
                context = preparation.RecoveryContext;
                Cleanup(preparation);
                return new AssetDeletionOutcome(context.AssetIdentifier, true, null, null);
            }
            catch (AssetDeletionStageFailureException failure)
            {
                return new AssetDeletionOutcome(
                    context.AssetIdentifier,
                    true,
                    failure.Stage,
                    failure.InnerException,
                    context);
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
                        .ReadByIdentifiers(new[] { asset.AssetClassId.Identifier })
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
                endpointNames = ReadConnectionEndpointNames(connections, retainedSnapshots, context);
            }

            var snapshots = connections
                .Select(connection => CreateConnectionSnapshot(connection, retainedSnapshots, endpointNames, useEndpointNames))
                .ToList();

            return new AssetDeletionSnapshot(connections, snapshots);
        }

        private Dictionary<string, string> ReadConnectionEndpointNames(
            IEnumerable<Connection> connections,
            IReadOnlyDictionary<string, AssetDeletionConnectionSnapshot> retainedSnapshots,
            AssetDeletionRecoveryContext context)
        {
            var endpointIdentifiers = connections
                .Where(connection => !retainedSnapshots.ContainsKey(connection.Identifier))
                .SelectMany(connection =>
                new[] { connection.Source.Port, connection.Destination.Port })
                .Where(reference => !ReferenceEquals(reference, null))
                .Select(reference => reference.Identifier)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var portsByIdentifier = _assetManagement.Ports.ReadByBigOrFilter(
                endpointIdentifiers,
                identifier => PortExposers.Identifier.Equal(identifier))
                .GroupBy(port => port.Identifier, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);
            var endpointPorts = endpointIdentifiers
                .Select(identifier => portsByIdentifier.TryGetValue(identifier, out var matches)
                    ? matches.Single()
                    : throw new InvalidOperationException($"Connection endpoint port '{identifier}' was not found."))
                .ToList();

            var assetIdentifiers = endpointPorts
                .Select(port => port.Asset.Identifier)
                .Where(identifier => !context.DeletedAssetNames.ContainsKey(identifier))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            var assetsByIdentifier = _assetManagement.Assets.ReadByIdentifiers(assetIdentifiers)
                .GroupBy(asset => asset.Identifier, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);

            var endpointNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var port in endpointPorts)
            {
                if (!context.DeletedAssetNames.TryGetValue(port.Asset.Identifier, out var name))
                {
                    name = assetsByIdentifier.TryGetValue(port.Asset.Identifier, out var matches)
                        ? matches.Single().Name
                        : throw new InvalidOperationException($"Connection endpoint Asset '{port.Asset.Identifier}' was not found.");
                }

                endpointNames[port.Identifier] = $"{name} - {port.PortInfo.Name}";
            }

            return endpointNames;
        }

        private static AssetDeletionConnectionSnapshot CreateConnectionSnapshot(
            Connection connection,
            IReadOnlyDictionary<string, AssetDeletionConnectionSnapshot> retainedSnapshots,
            IReadOnlyDictionary<string, string> endpointNames,
            bool useEndpointNames)
        {
            if (retainedSnapshots.TryGetValue(connection.Identifier, out var retained))
            {
                return retained;
            }

            var source = useEndpointNames
                ? endpointNames[connection.Source.Port.Identifier]
                : connection.Source.CableTag;
            var destination = useEndpointNames
                ? endpointNames[connection.Destination.Port.Identifier]
                : connection.Destination.CableTag;

            return new AssetDeletionConnectionSnapshot(
                connection.Identifier,
                source,
                destination,
                connection.CableType.Identifier,
                connection.CableLength);
        }

        private List<Connection> ReadConnectionsByPortIds(IReadOnlyCollection<Guid> portIds)
        {
            if (portIds.Count == 0)
            {
                return new List<Connection>();
            }

            return _assetManagement.Connections.ReadByBigOrFilter(
                portIds,
                portId => ConnectionExposers.Source.Port.Equal(new ISdmObjectReference<IPort>(portId.ToString()))
                .OR(ConnectionExposers.Destination.Port.Equal(new ISdmObjectReference<IPort>(portId.ToString()))))
                .GroupBy(connection => connection.Identifier, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList();
        }

        private AssetDeletionCleanupPreparation PrepareCleanup(AssetDeletionRecoveryContext context)
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

            var recoveryContext = context.WithConnectionSnapshots(context.ConnectionSnapshots
                .Concat(currentSnapshot.JobConnectionSnapshots)
                .GroupBy(snapshot => snapshot.ConnectionIdentifier, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList());

            return new AssetDeletionCleanupPreparation(recoveryContext, currentSnapshot, linkedObjectId);
        }

        private void Cleanup(AssetDeletionCleanupPreparation preparation)
        {
            var cleanupContext = preparation.RecoveryContext;
            var assetIdentifier = cleanupContext.AssetIdentifier;
            var currentSnapshot = preparation.Snapshot;

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
                    .GetByLinkedObjectID(preparation.LinkedObjectId, "Asset", "*")
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

    }
}
