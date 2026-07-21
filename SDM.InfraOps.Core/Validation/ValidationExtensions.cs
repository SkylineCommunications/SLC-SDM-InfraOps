namespace Skyline.DataMiner.SDM.InfraOps.Common.Validation
{
    using System.Linq;

    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    /// <summary>
    /// Extension methods for validation logic across all change-tracked entities.
    /// </summary>
    internal static class ValidationExtensions
    {
        /// <summary>
        /// Determines if a field should be validated based on whether the entity is new or the field has changed.
        /// Use this in validators to consistently check if validation is needed.
        /// </summary>
        /// <typeparam name="T">The entity type that implements IChangeTracking.</typeparam>
        /// <param name="entity">The entity to check.</param>
        /// <param name="field">The change tracking field to check.</param>
        /// <returns>True if the entity is new or the field has changed; otherwise false.</returns>
        public static bool ShouldValidate<T>(this T entity, IChangeTrackingField field)
            where T : IEntityTracking
        {
            return entity.IsNew || field.Changed;
        }

        /// <summary>
        /// Determines if a nested change-tracking object should be validated.
        /// Use this for complex nested objects (like Ownership, Custody) that implement IChangeTracking.
        /// </summary>
        /// <typeparam name="T">The entity type that implements IEntityTracking.</typeparam>
        /// <param name="entity">The entity to check.</param>
        /// <param name="nested">The nested change-tracking object to check.</param>
        /// <returns>True if the entity is new or the nested object has changed; otherwise false.</returns>
        public static bool ShouldValidate<T>(this T entity, IChangeTracking nested)
            where T : IEntityTracking
        {
            return entity.IsNew || (nested?.Changed ?? false);
        }

        /// <summary>
        /// Determines if any of the specified fields should be validated.
        /// </summary>
        public static bool ShouldValidateAny<T>(this T entity, params IChangeTrackingField[] fields)
            where T : IEntityTracking
        {
            return entity.IsNew || fields.Any(f => f.Changed);
        }
    }
}
