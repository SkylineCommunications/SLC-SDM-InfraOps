namespace Skyline.DataMiner.SDM.AssetManagement.Common.Middleware
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
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
            var assetClasses = oToCreate.ToList();
            var results = ValidateBulk(assetClasses);

            if (results.AnyInvalid())
            {
                throw BuildBulkValidationException(assetClasses, results);
            }

            return next(oToCreate);
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
            var assetClasses = oToCreateOrUpdate.ToList();
            var results = ValidateBulk(assetClasses);

            if (results.AnyInvalid())
            {
                throw BuildBulkValidationException(assetClasses, results);
            }

            return next(oToCreateOrUpdate);
        }

        public void OnDelete(IEnumerable<AssetClass> oToDelete, Action<IEnumerable<AssetClass>> next)
        {
            if (oToDelete is null)
            {
                throw new ArgumentNullException(nameof(oToDelete), "The collection of asset classes to delete cannot be null.");
            }

            next(oToDelete);
        }

        public void OnDelete(AssetClass oToDelete, Action<AssetClass> next)
        {
            if (oToDelete is null)
            {
                throw new ArgumentNullException(nameof(oToDelete), "The asset class to delete cannot be null.");
            }

            next(oToDelete);
        }

        public IEnumerable<AssetClass> OnRead(FilterElement<AssetClass> filter, Func<FilterElement<AssetClass>, IEnumerable<AssetClass>> next)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter), "Filter cannot be null.");
            }

            return next(filter);
        }

        public IEnumerable<AssetClass> OnRead(IQuery<AssetClass> query, Func<IQuery<AssetClass>, IEnumerable<AssetClass>> next)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), "Query cannot be null.");
            }

            return next(query);
        }

        public IEnumerable<IPagedResult<AssetClass>> OnReadPaged(FilterElement<AssetClass> filter, Func<FilterElement<AssetClass>, IEnumerable<IPagedResult<AssetClass>>> next)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter), "Filter cannot be null.");
            }

            return next(filter);
        }

        public IEnumerable<IPagedResult<AssetClass>> OnReadPaged(IQuery<AssetClass> query, Func<IQuery<AssetClass>, IEnumerable<IPagedResult<AssetClass>>> next)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), "Query cannot be null.");
            }

            return next(query);
        }

        public IEnumerable<IPagedResult<AssetClass>> OnReadPaged(FilterElement<AssetClass> filter, int pageSize, Func<FilterElement<AssetClass>, int, IEnumerable<IPagedResult<AssetClass>>> next)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter), "Filter cannot be null.");
            }

            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");
            }

            return next(filter, pageSize);
        }

        public IEnumerable<IPagedResult<AssetClass>> OnReadPaged(IQuery<AssetClass> query, int pageSize, Func<IQuery<AssetClass>, int, IEnumerable<IPagedResult<AssetClass>>> next)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), "Query cannot be null.");
            }

            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");
            }

            return next(query, pageSize);
        }

        public IReadOnlyCollection<AssetClass> OnUpdate(IEnumerable<AssetClass> oToUpdate, Func<IEnumerable<AssetClass>, IReadOnlyCollection<AssetClass>> next)
        {
            var assetClasses = oToUpdate.ToList();
            var results = ValidateBulk(assetClasses);

            if (results.AnyInvalid())
            {
                throw BuildBulkValidationException(assetClasses, results);
            }

            return next(oToUpdate);
        }

        public AssetClass OnUpdate(AssetClass oToUpdate, Func<AssetClass, AssetClass> next)
        {
            var result = Validate(oToUpdate);
            
            if (!result.IsValid)
            {
                throw result.ToException();
            }

            return next(oToUpdate);
        }

        private ValidationResult Validate(AssetClass assetClass)
        {
            return _validator.Validate(assetClass);
        }

        private List<ValidationResult> ValidateBulk(List<AssetClass> assetClasses)
        {
            // Validate each asset class individually
            return assetClasses.Select(ac => _validator.Validate(ac)).ToList();
        }

        /// <summary>
        /// Builds a comprehensive exception from bulk validation results.
        /// Uses the generic BulkValidationException with entity references.
        /// </summary>
        private Exception BuildBulkValidationException(List<AssetClass> assetClasses, List<ValidationResult> results)
        {
            return new BulkValidationException<AssetClass>(
                assetClasses, 
                results, 
                assetClass => string.IsNullOrEmpty(assetClass.Name) 
                    ? $"AssetClass '{assetClass.Identifier}'" 
                    : $"AssetClass '{assetClass.Name}'");
        }
    }
}
