namespace Skyline.DataMiner.SDM.InfraOpsProperties.Middleware
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.InfraOpsProperties.Extensions;
    using Skyline.DataMiner.SDM.InfraOpsProperties.Helpers;
    using Skyline.DataMiner.SDM.InfraOpsProperties.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Middleware;

    using SLDataGateway.API.Types.Querying;

    using static SLDataGateway.API.Util.ReferenceManager;

    /// <summary>
    /// Middleware that cascade-deletes <see cref="PropertyValues"/> entries referencing a <see cref="Property"/>
    /// before the property itself is removed. Prevents orphaned references. Mirrors the legacy
    /// PropertyWrapper.BeforeDelete() behavior. Mirrors the pattern of <c>JobIdAllocationMiddleware</c>.
    /// </summary>
    internal class PropertyCascadeDeleteMiddleware : IBulkRepositoryMiddleware<Property>
    {
        private readonly IInfraOpsPropertiesApiHelper _helper;
        private readonly bool _cascadeDeletes;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyCascadeDeleteMiddleware"/> class.
        /// </summary>
        /// <param name="helper">The InfraOps Properties API helper used to read/update PropertyValues.</param>
        /// <param name="cascadeDeletes">
        /// When <c>true</c> (default), deleting a Property removes any PropertyValue entries referencing it.
        /// Set to <c>false</c> to perform a plain delete without cascade.
        /// </param>
        internal PropertyCascadeDeleteMiddleware(IInfraOpsPropertiesApiHelper helper, bool cascadeDeletes = true)
        {
            _helper = helper ?? throw new ArgumentNullException(nameof(helper));
            _cascadeDeletes = cascadeDeletes;
        }

        public Property OnCreate(Property oToCreate, Func<Property, Property> next)
        {
            return next(oToCreate);
        }

        public IReadOnlyCollection<Property> OnCreate(
            IEnumerable<Property> oToCreate,
            Func<IEnumerable<Property>, IReadOnlyCollection<Property>> next)
        {
            return next(oToCreate);
        }

        public IReadOnlyCollection<Property> OnCreateOrUpdate(
            IEnumerable<Property> oToCreateOrUpdate,
            Func<IEnumerable<Property>, IReadOnlyCollection<Property>> next)
        {
            return next(oToCreateOrUpdate);
        }

        public Property OnUpdate(Property oToUpdate, Func<Property, Property> next)
        {
            return next(oToUpdate);
        }

        public IReadOnlyCollection<Property> OnUpdate(
            IEnumerable<Property> oToUpdate,
            Func<IEnumerable<Property>, IReadOnlyCollection<Property>> next)
        {
            return next(oToUpdate);
        }

        public void OnDelete(Property oToDelete, Action<Property> next)
        {
            if (oToDelete is null)
            {
                throw new ArgumentNullException(nameof(oToDelete));
            }

            if (_cascadeDeletes)
            {
                CascadeDeleteReferencingValues(oToDelete);
            }

            next(oToDelete);
        }

        public void OnDelete(IEnumerable<Property> oToDelete, Action<IEnumerable<Property>> next)
        {
            if (oToDelete is null)
            {
                throw new ArgumentNullException(nameof(oToDelete));
            }

            var properties = oToDelete.ToList();

            if (_cascadeDeletes)
            {
                CascadeDeleteReferencingValues(properties.ToArray());
            }

            next(properties);
        }

        public IEnumerable<Property> OnRead(
            FilterElement<Property> filter,
            Func<FilterElement<Property>, IEnumerable<Property>> next)
        {
            return next(filter);
        }

        public IEnumerable<Property> OnRead(
            IQuery<Property> query,
            Func<IQuery<Property>, IEnumerable<Property>> next)
        {
            return next(query);
        }

        public long OnCount(
            FilterElement<Property> filter,
            Func<FilterElement<Property>, long> next)
        {
            return next(filter);
        }

        public long OnCount(
            IQuery<Property> query,
            Func<IQuery<Property>, long> next)
        {
            return next(query);
        }

        public IEnumerable<IPagedResult<Property>> OnReadPaged(
            FilterElement<Property> filter,
            Func<FilterElement<Property>, IEnumerable<IPagedResult<Property>>> next)
        {
            return next(filter);
        }

        public IEnumerable<IPagedResult<Property>> OnReadPaged(
            IQuery<Property> query,
            Func<IQuery<Property>, IEnumerable<IPagedResult<Property>>> next)
        {
            return next(query);
        }

        public IEnumerable<IPagedResult<Property>> OnReadPaged(
            FilterElement<Property> filter,
            int pageSize,
            Func<FilterElement<Property>, int, IEnumerable<IPagedResult<Property>>> next)
        {
            return next(filter, pageSize);
        }

        public IEnumerable<IPagedResult<Property>> OnReadPaged(
            IQuery<Property> query,
            int pageSize,
            Func<IQuery<Property>, int, IEnumerable<IPagedResult<Property>>> next)
        {
            return next(query, pageSize);
        }

        /// <summary>
        /// Removes any PropertyValue entries referencing <paramref name="property"/> from all PropertyValues
        /// instances that carry them, before the Property itself is deleted - preventing orphaned references.
        /// </summary>
        private void CascadeDeleteReferencingValues(params Property[] properties)
        {
            if (properties == null)
            {
                return;
            }

            var propertyIdentifiers = properties.Select(p => p.Identifier).ToHashSet();
            var affectedPropertyValues = _helper.PropertyValues.ReadByBigOrFilter(propertyIdentifiers, id => PropertyValuesExposers.Values.PropertyId.Equal(new SdmObjectReference<Property>(id)));

            var toUpdate = new List<PropertyValues>();

            foreach (var propertyValues in affectedPropertyValues)
            {
                var remainingValues = propertyValues.Values
                    .Where(v => v == null || v.PropertyId == null || !propertyIdentifiers.Contains(v.PropertyId.Identifier))
                    .ToList();

                if (remainingValues.Count == propertyValues.Values.Count)
                {
                    continue;
                }

                propertyValues.Values = remainingValues;
                toUpdate.Add(propertyValues);
            }

            if (toUpdate.Count > 0)
            {
                _helper.PropertyValues.Update(toUpdate);
            }
        }
    }
}
