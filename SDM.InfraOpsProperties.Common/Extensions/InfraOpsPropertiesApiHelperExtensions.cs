namespace Skyline.DataMiner.SDM.InfraOpsProperties.Extensions
{
    using System;
    using System.Linq;

    using Skyline.DataMiner.SDM.InfraOpsProperties.Helpers;
    using Skyline.DataMiner.SDM.InfraOpsProperties.Models;

    /// <summary>
    /// Extension methods for <see cref="IInfraOpsPropertiesApiHelper"/> combining the Property and
    /// PropertyValues repositories for cross-entity operations.
    /// </summary>
    public static class InfraOpsPropertiesApiHelperExtensions
    {
        /// <summary>
        /// Deletes a Property and cascades the deletion by removing any PropertyValue entries
        /// referencing it from all PropertyValues instances.
        /// Note: does not create any audit history entries.
        /// </summary>
        public static void DeletePropertyWithCascade(this IInfraOpsPropertiesApiHelper helper, Property property)
        {
            if (helper == null)
            {
                throw new ArgumentNullException(nameof(helper));
            }

            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            if (property.IsNew)
            {
                throw new ArgumentException("Property can't be new", nameof(property));
            }

            var affectedPropertyValues = helper.PropertyValues.GetByPropertyID(property).ToList();

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
                helper.PropertyValues.Update(propertyValues);
            }

            helper.Properties.Delete(property);
        }
    }
}
