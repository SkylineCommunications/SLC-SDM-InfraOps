namespace Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Convenience base class for validators that implement <see cref="IValidator{T}"/>.
    /// <para>
    /// Provides two levels of abstraction:
    /// <list type="bullet">
    ///   <item><description>
    ///     <b>Action-unaware (most validators):</b> override <see cref="Validate(T)"/> and
    ///     <see cref="ValidateBulk(List{T})"/>. The base class handles the
    ///     <see cref="RepositoryAction"/> routing and the null-guard automatically.
    ///   </description></item>
    ///   <item><description>
    ///     <b>Action-aware (e.g. delete-in-use guards):</b> override
    ///     <see cref="Validate(T, RepositoryAction)"/> and/or
    ///     <see cref="ValidateBulk(List{T}, RepositoryAction)"/> directly, then call
    ///     <c>base.Validate(entity, action)</c> / <c>base.ValidateBulk(entities, action)</c>
    ///     for the standard path.
    ///   </description></item>
    /// </list>
    /// </para>
    /// </summary>
    public abstract class ValidatorBase<T> : IValidator<T>
        where T : class
    {
        /// <summary>
        /// Validates a single entity with no action context.
        /// Override this for validators that apply the same rules regardless of action.
        /// </summary>
        protected abstract ValidationResult Validate(T entity);

        /// <summary>
        /// Validates a batch of entities with no action context.
        /// Override this for validators that apply the same bulk rules regardless of action.
        /// </summary>
        protected abstract List<ValidationResult> ValidateBulk(List<T> entities);

        /// <summary>
        /// Validates a single entity for the given action.
        /// Default: null-guard then delegates to <see cref="Validate(T)"/>.
        /// Override <b>only</b> when the validator has action-specific rules
        /// (e.g. routing <see cref="RepositoryAction.Delete"/> to a different check).
        /// <para>
        /// <b>Warning:</b> if you override this method for action-specific behavior (e.g. Delete routing),
        /// you almost certainly also need to override <see cref="ValidateBulk(List{T}, RepositoryAction)"/>
        /// for the same action. Failing to do so leaves a bug where bulk operations skip the action-specific rules.
        /// </para>
        /// Call <c>base.Validate(entity, action)</c> for the standard path.
        /// </summary>
        public virtual ValidationResult Validate(T entity, RepositoryAction action)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            return Validate(entity);
        }

        /// <summary>
        /// Validates a batch of entities for the given action.
        /// Default: delegates to <see cref="ValidateBulk(List{T})"/>.
        /// Override <b>only</b> when the validator has action-specific bulk rules.
        /// Call <c>base.ValidateBulk(entities, action)</c> for the standard path.
        /// </summary>
        public virtual List<ValidationResult> ValidateBulk(List<T> entities, RepositoryAction action)
        {
            return ValidateBulk(entities);
        }
    }
}
