namespace Skyline.DataMiner.SDM.AssetManagement.Common.Middleware
{
    using System;
    using System.Collections.Generic;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.Net.SLConfiguration;
    using Skyline.DataMiner.SDM.AssetManagement.Common.Validation;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.AssetManagement.Validation;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    using SLDataGateway.API.Types.Querying;

    internal class AssetClassValidationMiddleware : IBulkRepositoryMiddleware<AssetClass>
    {
        private readonly AssetClassValidator _validator;

        internal AssetClassValidationMiddleware(AssetClassValidator validator)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public long OnCount(FilterElement<AssetClass> filter, Func<FilterElement<AssetClass>, long> next)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter), "Filter cannot be null.");
            }

            return next(filter);
        }

        public long OnCount(IQuery<AssetClass> query, Func<IQuery<AssetClass>, long> next)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), "Query cannot be null.");
            }

            return next(query);
        }

        public IReadOnlyCollection<AssetClass> OnCreate(IEnumerable<AssetClass> oToCreate, Func<IEnumerable<AssetClass>, IReadOnlyCollection<AssetClass>> next)
        {
            throw new NotImplementedException();
        }

        public AssetClass OnCreate(AssetClass oToCreate, Func<AssetClass, AssetClass> next)
        {
            var result = Validate(oToCreate);
            if (!result.IsValid)
            {
                throw result.ToException();
            }

            return next(oToCreate);
        }

        public IReadOnlyCollection<AssetClass> OnCreateOrUpdate(IEnumerable<AssetClass> oToCreateOrUpdate, Func<IEnumerable<AssetClass>, IReadOnlyCollection<AssetClass>> next)
        {
            throw new NotImplementedException();
        }

        public void OnDelete(IEnumerable<AssetClass> oToDelete, Action<IEnumerable<AssetClass>> next)
        {
            throw new NotImplementedException();
        }

        public void OnDelete(AssetClass oToDelete, Action<AssetClass> next)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<AssetClass> OnRead(FilterElement<AssetClass> filter, Func<FilterElement<AssetClass>, IEnumerable<AssetClass>> next)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<AssetClass> OnRead(IQuery<AssetClass> query, Func<IQuery<AssetClass>, IEnumerable<AssetClass>> next)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IPagedResult<AssetClass>> OnReadPaged(FilterElement<AssetClass> filter, Func<FilterElement<AssetClass>, IEnumerable<IPagedResult<AssetClass>>> next)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IPagedResult<AssetClass>> OnReadPaged(IQuery<AssetClass> query, Func<IQuery<AssetClass>, IEnumerable<IPagedResult<AssetClass>>> next)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IPagedResult<AssetClass>> OnReadPaged(FilterElement<AssetClass> filter, int pageSize, Func<FilterElement<AssetClass>, int, IEnumerable<IPagedResult<AssetClass>>> next)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IPagedResult<AssetClass>> OnReadPaged(IQuery<AssetClass> query, int pageSize, Func<IQuery<AssetClass>, int, IEnumerable<IPagedResult<AssetClass>>> next)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<AssetClass> OnUpdate(IEnumerable<AssetClass> oToUpdate, Func<IEnumerable<AssetClass>, IReadOnlyCollection<AssetClass>> next)
        {
            throw new NotImplementedException();
        }

        public AssetClass OnUpdate(AssetClass oToUpdate, Func<AssetClass, AssetClass> next)
        {
            throw new NotImplementedException();
        }

        private ValidationResult Validate(AssetClass assetClass)
        {
            return _validator.Validate(assetClass);
        }
    }
}
