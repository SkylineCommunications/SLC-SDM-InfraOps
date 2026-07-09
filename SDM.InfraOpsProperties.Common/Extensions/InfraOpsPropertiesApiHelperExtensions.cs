namespace Skyline.DataMiner.SDM.InfraOpsProperties.Extensions
{
    using System;

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
        /// </summary>
        /// <remarks>
        /// Kept for backward compatibility. Cascading is now built into <c>Properties.Delete(...)</c> itself
        /// (see <see cref="Skyline.DataMiner.SDM.InfraOpsProperties.Middleware.PropertyValidationMiddleware"/>),
        /// enabled by default. This method simply delegates to it and no longer duplicates the cascade logic.
        /// If the helper was constructed with cascading disabled, calling this method still performs a plain
        /// delete without cascading. Note: does not create any audit history entries.
        /// </remarks>
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

            helper.Properties.Delete(property);
        }
    }
}
