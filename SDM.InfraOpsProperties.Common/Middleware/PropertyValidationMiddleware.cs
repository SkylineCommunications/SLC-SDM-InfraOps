namespace Skyline.DataMiner.SDM.InfraOpsProperties.Middleware
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.SDM.InfraOpsProperties.Extensions;
    using Skyline.DataMiner.SDM.InfraOpsProperties.Helpers;
    using Skyline.DataMiner.SDM.InfraOpsProperties.Models;
    using Skyline.DataMiner.SDM.InfraOpsProperties.Validation;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Middleware;

    internal class PropertyValidationMiddleware : ValidationMiddleware<Property>
    {
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
            : base(
                validator,
                p => string.IsNullOrEmpty(p.Name) ? $"Property '{p.Identifier}'" : $"Property '{p.Name}'")
        {
            _helper = helper ?? throw new ArgumentNullException(nameof(helper));
            _cascadeDeletes = cascadeDeletes;
        }

        public override void OnDelete(IEnumerable<Property> oToDelete, Action<IEnumerable<Property>> next)
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

        public override void OnDelete(Property oToDelete, Action<Property> next)
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

            var affectedPropertyValues = _helper.PropertyValues.GetByPropertyID(Guid.Parse(property.Identifier)).ToList();

            var toUpdate = new List<PropertyValues>();

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
                toUpdate.Add(propertyValues);
            }

            if (toUpdate.Count > 0)
            {
                _helper.PropertyValues.Update(toUpdate);
            }
        }
    }
}
