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
    /// <para>
    /// <b>Null entities:</b> validators never throw on a <c>null</c> entity. A <c>null</c> entity
    /// (single or as an item of a bulk list) yields a failed <see cref="ValidationResult"/> created by
    /// <see cref="CreateNullEntityResult"/>, keyed by the validator's entity-level field with the
    /// message <c>"&lt;Entity&gt; cannot be null."</c>. <see cref="ValidateAndThrow(T, RepositoryAction)"/>
    /// throws that failure as a <see cref="ValidationException"/>. Bulk validation returns one result
    /// per input item, in the original order; only non-null items reach the validation phases.
    /// </para>
    /// </summary>
    public abstract class ValidatorBase<T> : IValidator<T>
        where T : class
    {
        /// <summary>
        /// Validates a single entity for the given action.
        /// Routes <see cref="RepositoryAction.Delete"/> to
        /// <see cref="ValidateBulkForDelete(List{T})"/> and every other action to the Create/Update phases.
        /// A <c>null</c> entity does not throw; it returns the failed result of <see cref="CreateNullEntityResult"/>.
        /// </summary>
        public ValidationResult Validate(T entity, RepositoryAction action)
        {
            if (entity == null)
            {
                return CreateNullEntityResult();
            }

            return ValidateBulk(new List<T> { entity }, action).Single();
        }

        /// <summary>
        /// Validates a single entity for Create/Update.
        /// A <c>null</c> entity does not throw; it returns the failed result of <see cref="CreateNullEntityResult"/>.
        /// </summary>
        protected ValidationResult Validate(T entity)
        {
            if (entity == null)
            {
                return CreateNullEntityResult();
            }

            return ValidateBulk(new List<T> { entity }).Single();
        }

        /// <summary>
        /// Validates a single entity for the given action and throws <see cref="ValidationException"/> if invalid.
        /// A <c>null</c> entity throws the <see cref="ValidationException"/> of <see cref="CreateNullEntityResult"/>
        /// (never <see cref="ArgumentNullException"/>).
        /// </summary>
        /// <exception cref="ValidationException">Thrown if the entity is <c>null</c> or invalid.</exception>
        public void ValidateAndThrow(T entity, RepositoryAction action)
        {
            if (entity == null)
            {
                throw CreateNullEntityResult().ToException();
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
        /// <para>
        /// Returns one result per input item, in the original order. <c>null</c> items get the failed
        /// result of <see cref="CreateNullEntityResult"/>; only the non-null items are passed to the
        /// validation phases. A <c>null</c> or empty list returns an empty list.
        /// </para>
        /// </summary>
        public List<ValidationResult> ValidateBulk(List<T> entities, RepositoryAction action)
        {
            if (entities == null || !entities.Any(e => e == null))
            {
                return ValidateBulkForAction(entities, action);
            }

            var nonNullEntities = entities.Where(e => e != null).ToList();
            var nonNullResults = nonNullEntities.Any()
                ? ValidateBulkForAction(nonNullEntities, action)
                : new List<ValidationResult>();

            var results = new List<ValidationResult>(entities.Count);
            int nonNullIndex = 0;
            foreach (var entity in entities)
            {
                results.Add(entity == null ? CreateNullEntityResult() : nonNullResults[nonNullIndex++]);
            }

            return results;
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
        /// Creates the failed result returned for a <c>null</c> entity.
        /// Implementations add a fail reason keyed by the validator's entity-level field
        /// (e.g. <c>RackValidationField.Rack</c>) with the message <c>"&lt;Entity&gt; cannot be null."</c>
        /// (e.g. <c>"Rack cannot be null."</c>). A new instance must be returned on every call.
        /// </summary>
        /// <returns>A failed <see cref="ValidationResult"/> describing the <c>null</c> entity.</returns>
        protected abstract ValidationResult CreateNullEntityResult();

        /// <summary>
        /// Validates a batch of entities for Create/Update.
        /// The list is expected to contain no <c>null</c> items; the public
        /// <see cref="ValidateBulk(List{T}, RepositoryAction)"/> filters them out before calling this.
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

        private List<ValidationResult> ValidateBulkForAction(List<T> entities, RepositoryAction action)
        {
            return action == RepositoryAction.Delete
                ? ValidateBulkForDelete(entities)
                : ValidateBulk(entities);
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
        /// A <c>null</c> entity does not throw; it returns the failed result of <see cref="CreateNullEntityResult"/>.
        /// </summary>
        protected ValidationResult ValidateForDelete(T entity)
        {
            if (entity == null)
            {
                return CreateNullEntityResult();
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
