namespace Skyline.DataMiner.SDM.InfraOpsProperties.Extensions
{
    using System;
    using System.Collections.Generic;

    using Skyline.DataMiner.SDM.InfraOpsProperties.Models;

    /// <summary>
    /// Extension methods for <see cref="PropertyValues"/> mirroring the old repo's
    /// PropertyValues.Duplicate feature (copying a value-set to another linked object).
    /// </summary>
    public static class PropertyValuesExtensions
    {
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
