namespace Skyline.DataMiner.SDM.AssetManagement.Common.Middleware
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Common.Services;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences;

    using SLDataGateway.API.Types.Querying;

    /// <summary>
    /// Middleware that derives <see cref="SourceInfo.PortType"/> and <see cref="DestinationInfo.PortType"/>
    /// from the referenced data or power port on create and update, so callers never have to supply them.
    /// Must run before validation so the persisted port type always matches the connected port.
    /// </summary>
    internal class ConnectionPortTypeMiddleware : IBulkRepositoryMiddleware<Connection>
    {
        private readonly SdmEntityLoader _entityLoader;

        internal ConnectionPortTypeMiddleware(SdmEntityLoader entityLoader)
        {
            _entityLoader = entityLoader ?? throw new ArgumentNullException(nameof(entityLoader));
        }

        public Connection OnCreate(Connection oToCreate, Func<Connection, Connection> next)
        {
            ApplyPortTypes(new List<Connection> { oToCreate });
            return next(oToCreate);
        }

        public IReadOnlyCollection<Connection> OnCreate(
            IEnumerable<Connection> oToCreate,
            Func<IEnumerable<Connection>, IReadOnlyCollection<Connection>> next)
        {
            var connections = oToCreate.ToList();
            ApplyPortTypes(connections);
            return next(connections);
        }

        public IReadOnlyCollection<Connection> OnCreateOrUpdate(
            IEnumerable<Connection> oToCreateOrUpdate,
            Func<IEnumerable<Connection>, IReadOnlyCollection<Connection>> next)
        {
            var connections = oToCreateOrUpdate.ToList();
            ApplyPortTypes(connections);
            return next(connections);
        }

        public Connection OnUpdate(Connection oToUpdate, Func<Connection, Connection> next)
        {
            ApplyPortTypes(new List<Connection> { oToUpdate });
            return next(oToUpdate);
        }

        public IReadOnlyCollection<Connection> OnUpdate(
            IEnumerable<Connection> oToUpdate,
            Func<IEnumerable<Connection>, IReadOnlyCollection<Connection>> next)
        {
            var connections = oToUpdate.ToList();
            ApplyPortTypes(connections);
            return next(connections);
        }

        public void OnDelete(Connection oToDelete, Action<Connection> next)
        {
            next(oToDelete);
        }

        public void OnDelete(IEnumerable<Connection> oToDelete, Action<IEnumerable<Connection>> next)
        {
            next(oToDelete);
        }

        public IEnumerable<Connection> OnRead(FilterElement<Connection> filter, Func<FilterElement<Connection>, IEnumerable<Connection>> next)
        {
            return next(filter);
        }

        public IEnumerable<Connection> OnRead(IQuery<Connection> query, Func<IQuery<Connection>, IEnumerable<Connection>> next)
        {
            return next(query);
        }

        public long OnCount(FilterElement<Connection> filter, Func<FilterElement<Connection>, long> next)
        {
            return next(filter);
        }

        public long OnCount(IQuery<Connection> query, Func<IQuery<Connection>, long> next)
        {
            return next(query);
        }

        public IEnumerable<IPagedResult<Connection>> OnReadPaged(
            FilterElement<Connection> filter,
            Func<FilterElement<Connection>, IEnumerable<IPagedResult<Connection>>> next)
        {
            return next(filter);
        }

        public IEnumerable<IPagedResult<Connection>> OnReadPaged(
            IQuery<Connection> query,
            Func<IQuery<Connection>, IEnumerable<IPagedResult<Connection>>> next)
        {
            return next(query);
        }

        public IEnumerable<IPagedResult<Connection>> OnReadPaged(
            FilterElement<Connection> filter,
            int pageSize,
            Func<FilterElement<Connection>, int, IEnumerable<IPagedResult<Connection>>> next)
        {
            return next(filter, pageSize);
        }

        public IEnumerable<IPagedResult<Connection>> OnReadPaged(
            IQuery<Connection> query,
            int pageSize,
            Func<IQuery<Connection>, int, IEnumerable<IPagedResult<Connection>>> next)
        {
            return next(query, pageSize);
        }

        private void ApplyPortTypes(List<Connection> connections)
        {
            var nonNull = connections.Where(c => c != null).ToList();
            if (nonNull.Count == 0)
            {
                return;
            }

            var portIds = nonNull
                .SelectMany(c => new[] { c.Source.Port, c.Destination.Port })
                .Where(reference => reference.HasValue())
                .Select(reference => reference.Identifier)
                .Distinct()
                .ToList();

            var portTypesByPortId = new Dictionary<string, SdmObjectReference<PortType>>();

            foreach (var dataPort in _entityLoader.GetDataPortsByDomIds(portIds))
            {
                portTypesByPortId[dataPort.Identifier] = dataPort.DataPortInfo.PortType;
            }

            foreach (var powerPort in _entityLoader.GetPowerPortsByDomIds(portIds))
            {
                portTypesByPortId[powerPort.Identifier] = powerPort.PowerPortInfo.PortType;
            }

            foreach (var connection in nonNull)
            {
                connection.Source.PortType = ResolvePortType(connection.Source.Port, portTypesByPortId);
                connection.Destination.PortType = ResolvePortType(connection.Destination.Port, portTypesByPortId);
            }
        }

        private static SdmObjectReference<PortType> ResolvePortType(
            ISdmObjectReference<IPort> port,
            Dictionary<string, SdmObjectReference<PortType>> portTypesByPortId)
        {
            // Unknown or empty ports clear the port type; the validator reports the missing or invalid port.
            if (port.HasValue() && portTypesByPortId.TryGetValue(port.Identifier, out var portType))
            {
                return portType;
            }

            return default;
        }
    }
}
