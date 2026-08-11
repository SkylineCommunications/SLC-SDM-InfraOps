namespace Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Convenience base class for validators that implement <see cref="IValidator{T}"/>.
    /// <para>
    /// Provides two levels of abstraction:
    /// <list type="bullet">
    ///   <item><description>
    ///     <b>Create/Update rules (most validators):</b> override <see cref="Validate(T)"/> and
    ///     <see cref="ValidateBulk(List{T})"/>. The base class handles the
    ///     <see cref="RepositoryAction"/> routing and the null-guard automatically.
    ///   </description></item>
    ///   <item><description>
    ///     <b>Delete rules (opt-in, e.g. orphan/in-use guards):</b> override
    ///     <see cref="ValidateForDelete(T)"/> and/or <see cref="ValidateBulkForDelete(List{T})"/>.
    ///     By default deletion is <b>not</b> validated (returns a passing result), because a delete
    ///     carries an already-persisted entity that need not satisfy create/update rules. Override
    ///     these hooks only to add delete-specific guards (e.g. "cannot delete while referenced").
    ///   </description></item>
    /// </list>
    /// </para>
    /// </summary>
    public abstract class ValidatorBase<T> : IValidator<T>
        where T : class
    {
        /// <summary>
        /// Validates a single entity for Create/Update.
        /// Override this for the rules that apply when an entity is created or updated.
        /// </summary>
        protected abstract ValidationResult Validate(T entity);

        /// <summary>
        /// Validates a batch of entities for Create/Update.
        /// Override this for the bulk rules that apply when entities are created or updated.
        /// </summary>
        protected abstract List<ValidationResult> ValidateBulk(List<T> entities);

        /// <summary>
        /// Validates a single entity for <see cref="RepositoryAction.Delete"/>.
        /// Default: no validation (returns a passing result). Override to add delete-specific
        /// guards, e.g. preventing deletion of an entity still referenced by children.
        /// </summary>
        protected virtual ValidationResult ValidateForDelete(T entity)
        {
            return new ValidationResult();
        }

        /// <summary>
        /// Validates a batch of entities for <see cref="RepositoryAction.Delete"/>.
        /// Default: no validation (one passing result per entity, in the same order).
        /// Override to add delete-specific guards for bulk deletes.
        /// </summary>
        protected virtual List<ValidationResult> ValidateBulkForDelete(List<T> entities)
        {
            return entities == null
                ? new List<ValidationResult>()
                : entities.Select(_ => new ValidationResult()).ToList();
        }

        /// <summary>
        /// Validates a single entity for the given action.
        /// Null-guards, then routes <see cref="RepositoryAction.Delete"/> to
        /// <see cref="ValidateForDelete(T)"/> and every other action to <see cref="Validate(T)"/>.
        /// </summary>
        public virtual ValidationResult Validate(T entity, RepositoryAction action)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            return action == RepositoryAction.Delete
                ? ValidateForDelete(entity)
                : Validate(entity);
        }

        /// <summary>
        /// Validates a batch of entities for the given action.
        /// Routes <see cref="RepositoryAction.Delete"/> to <see cref="ValidateBulkForDelete(List{T})"/>
        /// and every other action to <see cref="ValidateBulk(List{T})"/>.
        /// </summary>
        public virtual List<ValidationResult> ValidateBulk(List<T> entities, RepositoryAction action)
        {
            return action == RepositoryAction.Delete
                ? ValidateBulkForDelete(entities)
                : ValidateBulk(entities);
        }
    }
}
