namespace Skyline.DataMiner.SDM.InfraOpsProperties.Extensions
{
    using System;
    using System.Linq;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.SDM.InfraOpsProperties.Models;

    /// <summary>
    /// Extension methods for <see cref="PropertyValue"/> mirroring the old repo's PropertyValueWrapper
    /// navigation (resolving the linked Property) and IsCustom flag.
    /// </summary>
    public static class PropertyValueExtensions
    {
        /// <summary>
        /// Gets a value indicating whether this PropertyValue is a custom (ad-hoc) value,
        /// i.e. not backed by a Property definition.
        /// </summary>
        public static bool IsCustom(this PropertyValue value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return !value.PropertyId.HasValue();
        }

        /// <summary>
        /// Resolves the Property definition linked to this PropertyValue, or null if this is a custom value.
        /// </summary>
        public static Property GetProperty(this PropertyValue value, IBulkRepository<Property> repository)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            if (value.IsCustom())
            {
                return null;
            }

            return repository.Read(PropertyExposers.Identifier.Equal(value.PropertyId.Identifier)).SingleOrDefault();
        }
    }
}
