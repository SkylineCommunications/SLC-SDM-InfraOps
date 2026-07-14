namespace Skyline.DataMiner.SDM.InfraOpsProperties.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.SDM.InfraOpsProperties.Models;

    /// <summary>
    /// Convenience extension methods for <see cref="Property"/>, mirroring InfraOpsShared's
    /// PropertyWrapper API (AddDiscrete/RemoveDiscrete/ClearDiscretes). Kept as extension methods
    /// so <see cref="Property"/> stays a plain change-tracked data holder with no embedded business logic.
    /// </summary>
    public static class PropertyExtensions
    {
        /// <summary>
        /// Adds a <see cref="PropertyOption"/> to <see cref="Property.Discreets"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="discrete"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">An option with the same <see cref="PropertyOption.Option"/> already exists.</exception>
        /// <remarks>
        /// This only updates the in-memory <see cref="Property"/> instance. It is not persisted to the
        /// DOM/database until the instance is saved (e.g. via <c>CreateOrUpdate</c>/<c>Update</c> on the
        /// repository or helper).
        /// </remarks>
        public static void AddDiscrete(this Property property, PropertyOption discrete)
        {
            if (discrete == null)
            {
                throw new ArgumentNullException(nameof(discrete));
            }

            var list = property.Discreets;

            if (list.Any(option => option.Option == discrete.Option))
            {
                throw new InvalidOperationException("A Discrete with the same Option already exists.");
            }

            list.Add(discrete);
            property.Discreets = list;
        }

        /// <summary>
        /// Removes the <see cref="PropertyOption"/> matching <paramref name="discrete"/>'s
        /// <see cref="PropertyOption.Option"/> from <see cref="Property.Discreets"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="discrete"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">No matching option was found.</exception>
        /// <remarks>
        /// This only updates the in-memory <see cref="Property"/> instance. It is not persisted to the
        /// DOM/database until the instance is saved (e.g. via <c>CreateOrUpdate</c>/<c>Update</c> on the
        /// repository or helper).
        /// </remarks>
        public static void RemoveDiscrete(this Property property, PropertyOption discrete)
        {
            if (discrete == null)
            {
                throw new ArgumentNullException(nameof(discrete));
            }

            var list = property.Discreets;
            var found = list.FirstOrDefault(option => option.Option == discrete.Option);

            if (found == null)
            {
                throw new ArgumentException("The specified Discrete was not found.");
            }

            list.Remove(found);
            property.Discreets = list;
        }

        /// <summary>
        /// Clears all entries from <see cref="Property.Discreets"/>.
        /// </summary>
        /// <remarks>
        /// This only updates the in-memory <see cref="Property"/> instance. It is not persisted to the
        /// DOM/database until the instance is saved (e.g. via <c>CreateOrUpdate</c>/<c>Update</c> on the
        /// repository or helper).
        /// </remarks>
        public static void ClearDiscretes(this Property property)
        {
            if (property.Discreets.Count == 0)
            {
                return;
            }

            property.Discreets = new List<PropertyOption>();
        }
    }
}
