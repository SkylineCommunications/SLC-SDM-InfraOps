namespace Skyline.DataMiner.SDM.AssetManagement.Common.Middleware
{
    using System;
    using System.Collections.Generic;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.AssetManagement.Validation;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    using SLDataGateway.API.Types.Querying;

    internal class AssetValidationMiddleware : IBulkRepositoryMiddleware<Asset>
    {
        private readonly AssetValidator _validator;

        internal AssetValidationMiddleware(AssetValidator validator)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public long OnCount(FilterElement<Asset> filter, Func<FilterElement<Asset>, long> next)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter), "Filter cannot be null.");
            }

            return next(filter);
        }

        public long OnCount(IQuery<Asset> query, Func<IQuery<Asset>, long> next)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), "Query cannot be null.");
            }

            return next(query);
        }

        public IReadOnlyCollection<Asset> OnCreate(IEnumerable<Asset> oToCreate, Func<IEnumerable<Asset>, IReadOnlyCollection<Asset>> next)
        {
            throw new NotImplementedException();
        }

        public Asset OnCreate(Asset oToCreate, Func<Asset, Asset> next)
        {
            var result = Validate(oToCreate);
            if (!result.IsValid)
            {
                throw result.ToException();
            }

            return next(oToCreate);
        }

        public IReadOnlyCollection<Asset> OnCreateOrUpdate(IEnumerable<Asset> oToCreateOrUpdate, Func<IEnumerable<Asset>, IReadOnlyCollection<Asset>> next)
        {
            throw new NotImplementedException();
        }

        public void OnDelete(IEnumerable<Asset> oToDelete, Action<IEnumerable<Asset>> next)
        {
            throw new NotImplementedException();
        }

        public void OnDelete(Asset oToDelete, Action<Asset> next)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Asset> OnRead(FilterElement<Asset> filter, Func<FilterElement<Asset>, IEnumerable<Asset>> next)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Asset> OnRead(IQuery<Asset> query, Func<IQuery<Asset>, IEnumerable<Asset>> next)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IPagedResult<Asset>> OnReadPaged(FilterElement<Asset> filter, Func<FilterElement<Asset>, IEnumerable<IPagedResult<Asset>>> next)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IPagedResult<Asset>> OnReadPaged(IQuery<Asset> query, Func<IQuery<Asset>, IEnumerable<IPagedResult<Asset>>> next)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IPagedResult<Asset>> OnReadPaged(FilterElement<Asset> filter, int pageSize, Func<FilterElement<Asset>, int, IEnumerable<IPagedResult<Asset>>> next)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IPagedResult<Asset>> OnReadPaged(IQuery<Asset> query, int pageSize, Func<IQuery<Asset>, int, IEnumerable<IPagedResult<Asset>>> next)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<Asset> OnUpdate(IEnumerable<Asset> oToUpdate, Func<IEnumerable<Asset>, IReadOnlyCollection<Asset>> next)
        {
            throw new NotImplementedException();
        }

        public Asset OnUpdate(Asset oToUpdate, Func<Asset, Asset> next)
        {
            throw new NotImplementedException();
        }

        private ValidationResult Validate(Asset asset)
        {
            return _validator.Validate(asset);
        }
    }
}
