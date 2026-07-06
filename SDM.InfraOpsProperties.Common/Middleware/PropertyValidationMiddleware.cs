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

    internal class PropertyValidationMiddleware : IBulkRepositoryMiddleware<Property>
    {
        private readonly PropertyValidator _validator;

        internal PropertyValidationMiddleware(PropertyValidator validator)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public long OnCount(FilterElement<Property> filter, Func<FilterElement<Property>, long> next)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter), "Filter cannot be null.");
            }

            return next(filter);
        }

        public long OnCount(IQuery<Property> query, Func<IQuery<Property>, long> next)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), "Query cannot be null.");
            }

            return next(query);
        }

        public IReadOnlyCollection<Property> OnCreate(IEnumerable<Property> oToCreate, Func<IEnumerable<Property>, IReadOnlyCollection<Property>> next)
        {
            var properties = oToCreate.ToList();
            var results = ValidateBulk(properties);

            if (results.AnyInvalid())
            {
                throw BuildBulkValidationException(properties, results);
            }

            return next(oToCreate);
        }

        public Property OnCreate(Property oToCreate, Func<Property, Property> next)
        {
            var result = Validate(oToCreate);
            if (!result.IsValid)
            {
                throw result.ToException();
            }

            return next(oToCreate);
        }

        public IReadOnlyCollection<Property> OnCreateOrUpdate(IEnumerable<Property> oToCreateOrUpdate, Func<IEnumerable<Property>, IReadOnlyCollection<Property>> next)
        {
            var properties = oToCreateOrUpdate.ToList();
            var results = ValidateBulk(properties);

            if (results.AnyInvalid())
            {
                throw BuildBulkValidationException(properties, results);
            }

            return next(oToCreateOrUpdate);
        }

        public void OnDelete(IEnumerable<Property> oToDelete, Action<IEnumerable<Property>> next)
        {
            if (oToDelete is null)
            {
                throw new ArgumentNullException(nameof(oToDelete), "The collection of properties to delete cannot be null.");
            }

            next(oToDelete);
        }

        public void OnDelete(Property oToDelete, Action<Property> next)
        {
            if (oToDelete is null)
            {
                throw new ArgumentNullException(nameof(oToDelete), "The property to delete cannot be null.");
            }

            next(oToDelete);
        }

        public IEnumerable<Property> OnRead(FilterElement<Property> filter, Func<FilterElement<Property>, IEnumerable<Property>> next)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter), "Filter cannot be null.");
            }

            return next(filter);
        }

        public IEnumerable<Property> OnRead(IQuery<Property> query, Func<IQuery<Property>, IEnumerable<Property>> next)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), "Query cannot be null.");
            }

            return next(query);
        }

        public IEnumerable<IPagedResult<Property>> OnReadPaged(FilterElement<Property> filter, Func<FilterElement<Property>, IEnumerable<IPagedResult<Property>>> next)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter), "Filter cannot be null.");
            }

            return next(filter);
        }

        public IEnumerable<IPagedResult<Property>> OnReadPaged(IQuery<Property> query, Func<IQuery<Property>, IEnumerable<IPagedResult<Property>>> next)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), "Query cannot be null.");
            }

            return next(query);
        }

        public IEnumerable<IPagedResult<Property>> OnReadPaged(FilterElement<Property> filter, int pageSize, Func<FilterElement<Property>, int, IEnumerable<IPagedResult<Property>>> next)
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

        public IEnumerable<IPagedResult<Property>> OnReadPaged(IQuery<Property> query, int pageSize, Func<IQuery<Property>, int, IEnumerable<IPagedResult<Property>>> next)
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

        public IReadOnlyCollection<Property> OnUpdate(IEnumerable<Property> oToUpdate, Func<IEnumerable<Property>, IReadOnlyCollection<Property>> next)
        {
            var properties = oToUpdate.ToList();
            var results = ValidateBulk(properties);

            if (results.AnyInvalid())
            {
                throw BuildBulkValidationException(properties, results);
            }

            return next(oToUpdate);
        }

        public Property OnUpdate(Property oToUpdate, Func<Property, Property> next)
        {
            var result = Validate(oToUpdate);

            if (!result.IsValid)
            {
                throw result.ToException();
            }

            return next(oToUpdate);
        }

        private ValidationResult Validate(Property property)
        {
            return _validator.Validate(property);
        }

        private List<ValidationResult> ValidateBulk(List<Property> properties)
        {
            // Validate each property individually
            return properties.Select(p => _validator.Validate(p)).ToList();
        }

        /// <summary>
        /// Builds a comprehensive exception from bulk validation results.
        /// Uses the generic BulkValidationException with entity references.
        /// </summary>
        private Exception BuildBulkValidationException(List<Property> properties, List<ValidationResult> results)
        {
            return new BulkValidationException<Property>(
                properties,
                results,
                property => string.IsNullOrEmpty(property.Name)
                    ? $"Property '{property.Identifier}'"
                    : $"Property '{property.Name}'");
        }
    }
}
