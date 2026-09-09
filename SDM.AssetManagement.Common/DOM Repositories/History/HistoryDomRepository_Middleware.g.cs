namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Skyline.DataMiner.Net;
    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.Net.Apps.Sections.Sections;
    using Skyline.DataMiner.Net.Helper;
    using Skyline.DataMiner.Net.ManagerStore;
    using Skyline.DataMiner.Net.Messages;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.Net.Sections;
    using Skyline.DataMiner.Net.SubscriptionFilters;
    using Skyline.DataMiner.SDM;
    using SLDataGateway.API.Querying;
    using SLDataGateway.API.Types.Querying;

    internal sealed partial class HistoryDomRepository_Middleware : IBulkRepository<AssetManagement.Models.History>
    {
        private readonly IBulkRepository<AssetManagement.Models.History> _inner;
        private readonly IMiddlewareMarker<AssetManagement.Models.History> _middleware;

        public HistoryDomRepository_Middleware(IBulkRepository<AssetManagement.Models.History> inner, IMiddlewareMarker<AssetManagement.Models.History> middleware)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _middleware = middleware;
        }

        public IEnumerable<IPagedResult<AssetManagement.Models.History>> ReadPaged(FilterElement<AssetManagement.Models.History> filter)
        {
            if (_middleware is IPageableMiddleware<AssetManagement.Models.History> middleware)
            {
                return middleware.OnReadPaged(filter, _inner.ReadPaged);
            }
            else
            {
                return _inner.ReadPaged(filter);
            }
        }

        public IEnumerable<IPagedResult<AssetManagement.Models.History>> ReadPaged(IQuery<AssetManagement.Models.History> query)
        {
            if (_middleware is IPageableMiddleware<AssetManagement.Models.History> middleware)
            {
                return middleware.OnReadPaged(query, _inner.ReadPaged);
            }
            else
            {
                return _inner.ReadPaged(query);
            }
        }

        public IEnumerable<IPagedResult<AssetManagement.Models.History>> ReadPaged(FilterElement<AssetManagement.Models.History> filter, int pageSize)
        {
            if (_middleware is IPageableMiddleware<AssetManagement.Models.History> middleware)
            {
                return middleware.OnReadPaged(filter, pageSize, _inner.ReadPaged);
            }
            else
            {
                return _inner.ReadPaged(filter, pageSize);
            }
        }

        public IEnumerable<IPagedResult<AssetManagement.Models.History>> ReadPaged(IQuery<AssetManagement.Models.History> query, int pageSize)
        {
            if (_middleware is IPageableMiddleware<AssetManagement.Models.History> middleware)
            {
                return middleware.OnReadPaged(query, pageSize, _inner.ReadPaged);
            }
            else
            {
                return _inner.ReadPaged(query, pageSize);
            }
        }

        public IEnumerable<AssetManagement.Models.History> Read(FilterElement<AssetManagement.Models.History> filter)
        {
            if (_middleware is IReadableMiddleware<AssetManagement.Models.History> middleware)
            {
                return middleware.OnRead(filter, _inner.Read);
            }
            else
            {
                return _inner.Read(filter);
            }
        }

        public IEnumerable<AssetManagement.Models.History> Read(IQuery<AssetManagement.Models.History> query)
        {
            if (_middleware is IReadableMiddleware<AssetManagement.Models.History> middleware)
            {
                return middleware.OnRead(query, _inner.Read);
            }
            else
            {
                return _inner.Read(query);
            }
        }

        public long Count(FilterElement<AssetManagement.Models.History> filter)
        {
            if (_middleware is ICountableMiddleware<AssetManagement.Models.History> middleware)
            {
                return middleware.OnCount(filter, _inner.Count);
            }
            else
            {
                return _inner.Count(filter);
            }
        }

        public long Count(IQuery<AssetManagement.Models.History> query)
        {
            if (_middleware is ICountableMiddleware<AssetManagement.Models.History> middleware)
            {
                return middleware.OnCount(query, _inner.Count);
            }
            else
            {
                return _inner.Count(query);
            }
        }

        public IReadOnlyCollection<AssetManagement.Models.History> Create(IEnumerable<AssetManagement.Models.History> oToCreate)
        {
            if (_middleware is IBulkCreatableMiddleware<AssetManagement.Models.History> middleware)
            {
                return middleware.OnCreate(oToCreate, _inner.Create);
            }
            else
            {
                return _inner.Create(oToCreate);
            }
        }

        public AssetManagement.Models.History Create(AssetManagement.Models.History oToCreate)
        {
            if (_middleware is ICreatableMiddleware<AssetManagement.Models.History> middleware)
            {
                return middleware.OnCreate(oToCreate, _inner.Create);
            }
            else
            {
                return _inner.Create(oToCreate);
            }
        }

        public IReadOnlyCollection<AssetManagement.Models.History> Update(IEnumerable<AssetManagement.Models.History> oToUpdate)
        {
            if (_middleware is IBulkUpdatableMiddleware<AssetManagement.Models.History> middleware)
            {
                return middleware.OnUpdate(oToUpdate, _inner.Update);
            }
            else
            {
                return _inner.Update(oToUpdate);
            }
        }

        public AssetManagement.Models.History Update(AssetManagement.Models.History oToUpdate)
        {
            if (_middleware is IUpdatableMiddleware<AssetManagement.Models.History> middleware)
            {
                return middleware.OnUpdate(oToUpdate, _inner.Update);
            }
            else
            {
                return _inner.Update(oToUpdate);
            }
        }

        public void Delete(IEnumerable<AssetManagement.Models.History> oToDelete)
        {
            if (_middleware is IBulkDeletableMiddleware<AssetManagement.Models.History> middleware)
            {
                middleware.OnDelete(oToDelete, _inner.Delete);
            }
            else
            {
                _inner.Delete(oToDelete);
            }
        }

        public void Delete(AssetManagement.Models.History oToDelete)
        {
            if (_middleware is IDeletableMiddleware<AssetManagement.Models.History> middleware)
            {
                middleware.OnDelete(oToDelete, _inner.Delete);
            }
            else
            {
                _inner.Delete(oToDelete);
            }
        }
        public System.Collections.Generic.IReadOnlyCollection<AssetManagement.Models.History> CreateOrUpdate(System.Collections.Generic.IEnumerable<AssetManagement.Models.History> oToCreateOrUpdate)
        {
            if (_middleware is IBulkRepositoryMiddleware<AssetManagement.Models.History> middleware)
            {
                return middleware.OnCreateOrUpdate(oToCreateOrUpdate, _inner.CreateOrUpdate);
            }
            else
            {
                return _inner.CreateOrUpdate(oToCreateOrUpdate);
            }
        }

    }
}