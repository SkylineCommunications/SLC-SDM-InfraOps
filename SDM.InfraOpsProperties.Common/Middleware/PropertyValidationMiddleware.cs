namespace Skyline.DataMiner.SDM.InfraOpsProperties.Middleware
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.InfraOpsProperties.Extensions;
    using Skyline.DataMiner.SDM.InfraOpsProperties.Helpers;
    using Skyline.DataMiner.SDM.InfraOpsProperties.Models;
    using Skyline.DataMiner.SDM.InfraOpsProperties.Validation;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    using SLDataGateway.API.Types.Querying;

    internal class PropertyValidationMiddleware : IBulkRepositoryMiddleware<Property>
    {
        private readonly PropertyValidator _validator;
        private readonly IInfraOpsPropertiesApiHelper _helper;
        private readonly bool _cascadeDeletes;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyValidationMiddleware"/> class.
        /// </summary>
        /// <param name="validator">The Property validator.</param>
        /// <param name="helper">
        /// The InfraOps Properties API helper used to cascade-delete PropertyValue entries referencing a deleted
        /// Property. Note: this is captured by reference during <see cref="InfraOpsPropertiesApiHelper"/>
        /// construction, before its repositories are wired up. Only <see cref="OnDelete(Property, Action{Property})"/>
        /// / <see cref="OnDelete(IEnumerable{Property}, Action{IEnumerable{Property}})"/> (called after construction
        /// completes) access <paramref name="helper"/>'s repositories.
        /// </param>
        /// <param name="cascadeDeletes">
        /// When <c>true</c> (default), deleting a Property removes any PropertyValue entries referencing it from
        /// all PropertyValues instances first, preventing orphaned references. Set to <c>false</c> to opt out and
        /// perform a plain delete instead.
        /// </param>
        internal PropertyValidationMiddleware(PropertyValidator validator, IInfraOpsPropertiesApiHelper helper, bool cascadeDeletes = true)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _helper = helper ?? throw new ArgumentNullException(nameof(helper));
            _cascadeDeletes = cascadeDeletes;
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

            var properties = oToDelete.ToList();

            if (_cascadeDeletes)
            {
                foreach (var property in properties)
                {
                    CascadeDeleteReferencingValues(property);
                }
            }

            next(properties);
        }

        public void OnDelete(Property oToDelete, Action<Property> next)
        {
            if (oToDelete is null)
            {
                throw new ArgumentNullException(nameof(oToDelete), "The property to delete cannot be null.");
            }

            if (_cascadeDeletes)
            {
                CascadeDeleteReferencingValues(oToDelete);
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
            // Validates each property individually and detects (Scope, Name) conflicts within the batch itself.
            return _validator.ValidateBulk(properties);
        }

        /// <summary>
        /// Removes any PropertyValue entries referencing <paramref name="property"/> from all PropertyValues
        /// instances that carry them, before the Property itself is deleted - preventing orphaned references.
        /// Mirrors the legacy PropertyWrapper.BeforeDelete() cascade behavior (minus audit history logging,
        /// since no History module exists in this codebase yet).
        /// </summary>
        private void CascadeDeleteReferencingValues(Property property)
        {
            if (property == null || property.IsNew)
            {
                // Nothing can reference a Property that was never persisted.
                return;
            }

            var affectedPropertyValues = _helper.PropertyValues.GetByPropertyID(property).ToList();

            foreach (var propertyValues in affectedPropertyValues)
            {
                var remainingValues = propertyValues.Values
                    .Where(v => v == null || v.PropertyId == null || v.PropertyId.Identifier != property.Identifier)
                    .ToList();

                if (remainingValues.Count == propertyValues.Values.Count)
                {
                    continue;
                }

                propertyValues.Values = remainingValues;
                _helper.PropertyValues.Update(propertyValues);
            }
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
