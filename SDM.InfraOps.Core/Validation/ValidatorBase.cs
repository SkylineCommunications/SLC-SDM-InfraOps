namespace Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Exceptions;

    /// <summary>
    /// Convenience base class for validators that implement <see cref="IValidator{T}"/>.
    /// <para>
    /// Provides Create/Update phases and optional Delete rules:
    /// <list type="bullet">
    ///   <item><description>
    ///     <b>Create/Update rules:</b> override <see cref="ValidateBulkNoDatabaseBusinessRules(List{T})"/>,
    ///     <see cref="ValidateBulkNoDatabaseBatchConflicts(List{T})"/>, and
    ///     <see cref="ValidateBulkWithDatabase(List{T})"/>. Single-entity validation uses the same phases.
    ///   </description></item>
    ///   <item><description>
    ///     <b>Delete rules (opt-in, e.g. orphan/in-use guards):</b> override
    ///     <see cref="ValidateBulkForDelete(List{T})"/>.
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
        /// Validates a single entity for the given action.
        /// Null-guards, then routes <see cref="RepositoryAction.Delete"/> to
        /// <see cref="ValidateBulkForDelete(List{T})"/> and every other action to the Create/Update phases.
        /// </summary>
        public ValidationResult Validate(T entity, RepositoryAction action)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            return ValidateBulk(new List<T> { entity }, action).Single();
        }

        /// <summary>
        /// Validates a single entity for Create/Update.
        /// </summary>
        protected ValidationResult Validate(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            return ValidateBulk(new List<T> { entity }).Single();
        }

        /// <summary>
        /// Validates a single entity for the given action and throws <see cref="ValidationException"/> if invalid.
        /// </summary>
        public void ValidateAndThrow(T entity, RepositoryAction action)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            try
            {
                ValidateBulkAndThrow(new List<T> { entity }, action);
            }
            catch(ValidationAggregateException ex)
            {
                throw ex.InnerExceptions.Single();
            }
        }

        public void ValidateWithHandler(T entity, RepositoryAction action, Action<ValidationResult> onError)
        {
            var result = Validate(entity, action);
            if (!result.IsValid)
            {
                onError?.Invoke(result);
            }
        }

        /// <summary>
        /// Validates a batch of entities for the given action.
        /// Routes <see cref="RepositoryAction.Delete"/> to <see cref="ValidateBulkForDelete(List{T})"/>
        /// and every other action to <see cref="ValidateBulk(List{T})"/>.
        /// </summary>
        public List<ValidationResult> ValidateBulk(List<T> entities, RepositoryAction action)
        {
            return action == RepositoryAction.Delete
                ? ValidateBulkForDelete(entities)
                : ValidateBulk(entities);
        }

        /// <summary>
        /// Validates a batch of entities for the given action and throws <see cref="ValidationAggregateException"/> if any are invalid.
        /// </summary>
        /// <param name="entities">The list of entities to validate.</param>
        /// <param name="action">The repository action to validate for.</param>
        /// <exception cref="ValidationAggregateException">Thrown if any of the validation results are invalid.</exception>
        public void ValidateBulkAndThrow(List<T> entities, RepositoryAction action)
        {
            var results = ValidateBulk(entities, action);
            if (results.AnyInvalid())
            {
                throw new ValidationAggregateException(results.Where(r => !r.IsValid).Select(r => r.ToException()));
            }
        }

        /// <summary>
        /// Validates a batch of entities for the given action and invokes the provided error handler if any are invalid.
        /// </summary>
        /// <param name="entities">The list of entities to validate.</param>
        /// <param name="action">The repository action to validate for.</param>
        /// <param name="onError">The error handler to invoke if any validation results are invalid.</param>
        public void ValidateBulkWithHandler(List<T> entities, RepositoryAction action, Action<List<ValidationResult>> onError)
        {
            var result = ValidateBulk(entities, action);
            if (result.AnyInvalid())
            {
                onError?.Invoke(result);
            }
        }

        /// <summary>
        /// Validates a batch of entities for Create/Update.
        /// </summary>
        protected List<ValidationResult> ValidateBulk(List<T> entities)
        {
            if (entities == null || !entities.Any())
            {
                return new List<ValidationResult>();
            }

            List<ValidationResult> results = ValidateBulkNoDatabase(entities);

            // Fast-fail if 'no database' validations fails
            if (results.AnyInvalid())
            {
                return results;
            }

            // Validate rules that require database access
            var databaseResults = ValidateBulkWithDatabase(entities);
            results.MergeFrom(databaseResults);

            return results;
        }

        private List<ValidationResult> ValidateBulkNoDatabase(List<T> entities)
        {
            // Validate business logic rules first (no database access)
            var results = ValidateBulkNoDatabaseBusinessRules(entities);

            // Fast-fail if business rules fail
            if (results.AnyInvalid())
            {
                return results;
            }

            // Validate rules that require no database access
            var noDatabaseResults = ValidateBulkNoDatabaseBatchConflicts(entities);
            results.MergeFrom(noDatabaseResults);

            return results;
        }

        /// <summary>
        /// Validates a batch of entities for business rules that do not require database access.
        /// </summary>
        /// <param name="entities">The list of entities to validate.</param>
        /// <returns>A list of validation results.</returns>
        protected abstract List<ValidationResult> ValidateBulkNoDatabaseBusinessRules(List<T> entities);

        /// <summary>
        /// Validates a batch of entities for rules that do not require database access.
        /// </summary>
        /// <param name="entities">The list of entities to validate.</param>
        /// <returns>A list of validation results.</returns>
        protected abstract List<ValidationResult> ValidateBulkNoDatabaseBatchConflicts(List<T> entities);

        /// <summary>
        /// Validates a batch of entities for rules that require database access.
        /// </summary>
        /// <param name="entities">The list of entities to validate.</param>
        /// <returns>A list of validation results.</returns>
        protected abstract List<ValidationResult> ValidateBulkWithDatabase(List<T> entities);

        /// <summary>
        /// Validates a single entity for <see cref="RepositoryAction.Delete"/>.
        /// Default: no validation (returns a passing result). Override to add delete-specific
        /// guards, e.g. preventing deletion of an entity still referenced by children.
        /// </summary>
        protected ValidationResult ValidateForDelete(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            return ValidateBulkForDelete(new List<T> { entity}).Single();
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
    }
}
