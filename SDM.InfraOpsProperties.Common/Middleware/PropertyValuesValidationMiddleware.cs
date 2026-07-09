namespace Skyline.DataMiner.SDM.InfraOpsProperties.Middleware
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.InfraOpsProperties.Models;
    using Skyline.DataMiner.SDM.InfraOpsProperties.Validation;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    using SLDataGateway.API.Types.Querying;

    internal class PropertyValuesValidationMiddleware : IBulkRepositoryMiddleware<PropertyValues>
    {
        private readonly PropertyValuesValidator _validator;

        internal PropertyValuesValidationMiddleware(PropertyValuesValidator validator)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public long OnCount(FilterElement<PropertyValues> filter, Func<FilterElement<PropertyValues>, long> next)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter), "Filter cannot be null.");
            }

            return next(filter);
        }

        public long OnCount(IQuery<PropertyValues> query, Func<IQuery<PropertyValues>, long> next)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), "Query cannot be null.");
            }

            return next(query);
        }

        public IReadOnlyCollection<PropertyValues> OnCreate(IEnumerable<PropertyValues> oToCreate, Func<IEnumerable<PropertyValues>, IReadOnlyCollection<PropertyValues>> next)
        {
            var propertyValues = oToCreate.ToList();
            var results = ValidateBulk(propertyValues);

            if (results.AnyInvalid())
            {
                throw BuildBulkValidationException(propertyValues, results);
            }

            return next(oToCreate);
        }

        public PropertyValues OnCreate(PropertyValues oToCreate, Func<PropertyValues, PropertyValues> next)
        {
            var result = Validate(oToCreate);
            if (!result.IsValid)
            {
                throw result.ToException();
            }

            return next(oToCreate);
        }

        public IReadOnlyCollection<PropertyValues> OnCreateOrUpdate(IEnumerable<PropertyValues> oToCreateOrUpdate, Func<IEnumerable<PropertyValues>, IReadOnlyCollection<PropertyValues>> next)
        {
            var propertyValues = oToCreateOrUpdate.ToList();
            var results = ValidateBulk(propertyValues);

            if (results.AnyInvalid())
            {
                throw BuildBulkValidationException(propertyValues, results);
            }

            return next(oToCreateOrUpdate);
        }

        public void OnDelete(IEnumerable<PropertyValues> oToDelete, Action<IEnumerable<PropertyValues>> next)
        {
            if (oToDelete is null)
            {
                throw new ArgumentNullException(nameof(oToDelete), "The collection of property values to delete cannot be null.");
            }

            next(oToDelete);
        }

        public void OnDelete(PropertyValues oToDelete, Action<PropertyValues> next)
        {
            if (oToDelete is null)
            {
                throw new ArgumentNullException(nameof(oToDelete), "The property values to delete cannot be null.");
            }

            next(oToDelete);
        }

        public IEnumerable<PropertyValues> OnRead(FilterElement<PropertyValues> filter, Func<FilterElement<PropertyValues>, IEnumerable<PropertyValues>> next)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter), "Filter cannot be null.");
            }

            return next(filter);
        }

        public IEnumerable<PropertyValues> OnRead(IQuery<PropertyValues> query, Func<IQuery<PropertyValues>, IEnumerable<PropertyValues>> next)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), "Query cannot be null.");
            }

            return next(query);
        }

        public IEnumerable<IPagedResult<PropertyValues>> OnReadPaged(FilterElement<PropertyValues> filter, Func<FilterElement<PropertyValues>, IEnumerable<IPagedResult<PropertyValues>>> next)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter), "Filter cannot be null.");
            }

            return next(filter);
        }

        public IEnumerable<IPagedResult<PropertyValues>> OnReadPaged(IQuery<PropertyValues> query, Func<IQuery<PropertyValues>, IEnumerable<IPagedResult<PropertyValues>>> next)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), "Query cannot be null.");
            }

            return next(query);
        }

        public IEnumerable<IPagedResult<PropertyValues>> OnReadPaged(FilterElement<PropertyValues> filter, int pageSize, Func<FilterElement<PropertyValues>, int, IEnumerable<IPagedResult<PropertyValues>>> next)
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

        public IEnumerable<IPagedResult<PropertyValues>> OnReadPaged(IQuery<PropertyValues> query, int pageSize, Func<IQuery<PropertyValues>, int, IEnumerable<IPagedResult<PropertyValues>>> next)
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

        public IReadOnlyCollection<PropertyValues> OnUpdate(IEnumerable<PropertyValues> oToUpdate, Func<IEnumerable<PropertyValues>, IReadOnlyCollection<PropertyValues>> next)
        {
            var propertyValues = oToUpdate.ToList();
            var results = ValidateBulk(propertyValues);

            if (results.AnyInvalid())
            {
                throw BuildBulkValidationException(propertyValues, results);
            }

            return next(oToUpdate);
        }

        public PropertyValues OnUpdate(PropertyValues oToUpdate, Func<PropertyValues, PropertyValues> next)
        {
            var result = Validate(oToUpdate);

            if (!result.IsValid)
            {
                throw result.ToException();
            }

            return next(oToUpdate);
        }

        private ValidationResult Validate(PropertyValues propertyValues)
        {
            return _validator.Validate(propertyValues);
        }

        private List<ValidationResult> ValidateBulk(List<PropertyValues> propertyValues)
        {
            return _validator.ValidateBulk(propertyValues);
        }

        /// <summary>
        /// Builds a comprehensive exception from bulk validation results.
        /// Uses the generic BulkValidationException with entity references.
        /// </summary>
        private Exception BuildBulkValidationException(List<PropertyValues> propertyValues, List<ValidationResult> results)
        {
            return new BulkValidationException<PropertyValues>(
                propertyValues,
                results,
                pv => $"PropertyValues '{pv.Identifier}'");
        }
    }
}
