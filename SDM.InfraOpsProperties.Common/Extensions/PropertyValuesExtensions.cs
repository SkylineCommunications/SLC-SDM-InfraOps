namespace Skyline.DataMiner.SDM.InfraOpsProperties.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.SDM.InfraOpsProperties.Models;

    /// <summary>
    /// Extension methods for <see cref="PropertyValues"/> mirroring InfraOpsShared's PropertyValuesWrapper
    /// API (GetPropertyValue/AddPropertyValue/RemovePropertyValue) plus the old repo's
    /// PropertyValues.Duplicate feature (copying a value-set to another linked object).
    /// </summary>
    public static class PropertyValuesExtensions
    {
        /// <summary>
        /// Finds the <see cref="PropertyValue"/> in <see cref="PropertyValues.Values"/> that is linked to
        /// <paramref name="property"/>, or <c>null</c> if none is set.
        /// </summary>
        public static PropertyValue GetPropertyValue(this PropertyValues source, Property property)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            return source.Values.FirstOrDefault(value => value.PropertyId != null && value.PropertyId.Identifier == property.Identifier);
        }

        /// <summary>
        /// Adds a <see cref="PropertyValue"/> to <see cref="PropertyValues.Values"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="propertyValue"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">A value for the same <see cref="PropertyValue.PropertyId"/> already exists.</exception>
        public static void AddPropertyValue(this PropertyValues source, PropertyValue propertyValue)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (propertyValue == null)
            {
                throw new ArgumentNullException(nameof(propertyValue));
            }

            var list = source.Values;

            if (list.Any(value => value.PropertyId == propertyValue.PropertyId))
            {
                throw new InvalidOperationException("A PropertyValue for the same Property already exists.");
            }

            list.Add(propertyValue);
            source.Values = list;
        }

        /// <summary>
        /// Removes the <see cref="PropertyValue"/> matching <paramref name="propertyValue"/>'s
        /// <see cref="PropertyValue.PropertyId"/> from <see cref="PropertyValues.Values"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="propertyValue"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">No matching value was found.</exception>
        public static void RemovePropertyValue(this PropertyValues source, PropertyValue propertyValue)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (propertyValue == null)
            {
                throw new ArgumentNullException(nameof(propertyValue));
            }

            var list = source.Values;
            var found = list.FirstOrDefault(value => value.PropertyId == propertyValue.PropertyId);

            if (found == null)
            {
                throw new ArgumentException("The specified PropertyValue was not found.");
            }

            list.Remove(found);
            source.Values = list;
        }

        /// <summary>
        /// Creates a new, unsaved PropertyValues instance with the same Scope and Values as the source,
        /// but linked to a different object (and optionally a different SubID).
        /// </summary>
        public static PropertyValues Duplicate(this PropertyValues source, Guid linkedObjectId, string subId = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (linkedObjectId == Guid.Empty)
            {
                throw new ArgumentException("The linked object id can't be empty", nameof(linkedObjectId));
            }

            var duplicate = new PropertyValues
            {
                LinkedObjectID = linkedObjectId,
                Scope = source.Scope,
                SubID = subId,
            };

            var values = new List<PropertyValue>();
            foreach (var value in source.Values)
            {
                if (value == null)
                {
                    continue;
                }

                values.Add(new PropertyValue
                {
                    PropertyName = value.PropertyName,
                    Value = value.Value,
                    PropertyId = value.PropertyId,
                });
            }

            duplicate.Values = values;

            return duplicate;
        }
    }
}
