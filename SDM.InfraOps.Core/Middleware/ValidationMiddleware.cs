namespace Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Middleware
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    using SLDataGateway.API.Types.Querying;

    /// <summary>
    /// Generic validation middleware that delegates write-operation validation to an <see cref="IValidator{T}"/>.
    /// The action performed (Create, Update, Delete, CreateOrUpdate) is forwarded to the validator so
    /// action-specific rules (e.g. delete-in-use guards) can be applied without extra constructor parameters.
    /// All read and count operations are passed through unchanged after a null-guard.
    /// </summary>
    internal class ValidationMiddleware<T> : IBulkRepositoryMiddleware<T>
        where T : class
    {
        private readonly IValidator<T> _validator;

        /// <summary>
        /// Initializes a new instance of <see cref="ValidationMiddleware{T}"/>.
        /// </summary>
        /// <param name="validator">The validator used for all write operations.</param>
        internal ValidationMiddleware(IValidator<T> validator)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        // ── Private helpers ─────────────────────────────────────────────────

        /// <summary>Validates a single entity for the given action. Throws if invalid.</summary>
        private ValidationResult ValidateSingle(T entity, RepositoryAction action)
            => _validator.Validate(entity, action);

        /// <summary>Validates a batch of entities for the given action.</summary>
        private List<ValidationResult> ValidateBulk(List<T> entities, RepositoryAction action)
            => _validator.ValidateBulk(entities, action);

        /// <summary>Builds the exception thrown when bulk validation fails.</summary>
        protected virtual Exception BuildBulkValidationException(List<T> entities, List<ValidationResult> results)
            => new BulkValidationException<T>(entities, results);

        // ── Write operations ────────────────────────────────────────────────

        public T OnCreate(T oToCreate, Func<T, T> next)
        {
            var result = ValidateSingle(oToCreate, RepositoryAction.Create);
            if (!result.IsValid)
                throw result.ToException();
            return next(oToCreate);
        }

        public IReadOnlyCollection<T> OnCreate(IEnumerable<T> oToCreate, Func<IEnumerable<T>, IReadOnlyCollection<T>> next)
        {
            var items = oToCreate.ToList();
            var results = ValidateBulk(items, RepositoryAction.Create);
            if (results.AnyInvalid())
                throw BuildBulkValidationException(items, results);
            return next(items);
        }

        public IReadOnlyCollection<T> OnCreateOrUpdate(IEnumerable<T> oToCreateOrUpdate, Func<IEnumerable<T>, IReadOnlyCollection<T>> next)
        {
            var items = oToCreateOrUpdate.ToList();
            var results = ValidateBulk(items, RepositoryAction.CreateOrUpdate);
            if (results.AnyInvalid())
                throw BuildBulkValidationException(items, results);
            return next(items);
        }

        public T OnUpdate(T oToUpdate, Func<T, T> next)
        {
            var result = ValidateSingle(oToUpdate, RepositoryAction.Update);
            if (!result.IsValid)
                throw result.ToException();
            return next(oToUpdate);
        }

        public IReadOnlyCollection<T> OnUpdate(IEnumerable<T> oToUpdate, Func<IEnumerable<T>, IReadOnlyCollection<T>> next)
        {
            var items = oToUpdate.ToList();
            var results = ValidateBulk(items, RepositoryAction.Update);
            if (results.AnyInvalid())
                throw BuildBulkValidationException(items, results);
            return next(items);
        }

        public void OnDelete(T oToDelete, Action<T> next)
        {
            if (oToDelete is null)
                throw new ArgumentNullException(nameof(oToDelete));

            var result = ValidateSingle(oToDelete, RepositoryAction.Delete);
            if (!result.IsValid)
                throw result.ToException();

            next(oToDelete);
        }

        public void OnDelete(IEnumerable<T> oToDelete, Action<IEnumerable<T>> next)
        {
            if (oToDelete is null)
                throw new ArgumentNullException(nameof(oToDelete));

            var items = oToDelete.ToList();
            var results = ValidateBulk(items, RepositoryAction.Delete);
            if (results.AnyInvalid())
                throw BuildBulkValidationException(items, results);

            next(items);
        }

        // ── Pass-through operations (null-guarded) ──────────────────────────

        public IEnumerable<T> OnRead(FilterElement<T> filter, Func<FilterElement<T>, IEnumerable<T>> next)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter), "Filter cannot be null.");
            return next(filter);
        }

        public IEnumerable<T> OnRead(IQuery<T> query, Func<IQuery<T>, IEnumerable<T>> next)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query), "Query cannot be null.");
            return next(query);
        }

        public long OnCount(FilterElement<T> filter, Func<FilterElement<T>, long> next)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter), "Filter cannot be null.");
            return next(filter);
        }

        public long OnCount(IQuery<T> query, Func<IQuery<T>, long> next)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query), "Query cannot be null.");
            return next(query);
        }

        public IEnumerable<IPagedResult<T>> OnReadPaged(FilterElement<T> filter, Func<FilterElement<T>, IEnumerable<IPagedResult<T>>> next)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter), "Filter cannot be null.");
            return next(filter);
        }

        public IEnumerable<IPagedResult<T>> OnReadPaged(IQuery<T> query, Func<IQuery<T>, IEnumerable<IPagedResult<T>>> next)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query), "Query cannot be null.");
            return next(query);
        }

        public IEnumerable<IPagedResult<T>> OnReadPaged(FilterElement<T> filter, int pageSize, Func<FilterElement<T>, int, IEnumerable<IPagedResult<T>>> next)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter), "Filter cannot be null.");
            if (pageSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");
            return next(filter, pageSize);
        }

        public IEnumerable<IPagedResult<T>> OnReadPaged(IQuery<T> query, int pageSize, Func<IQuery<T>, int, IEnumerable<IPagedResult<T>>> next)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query), "Query cannot be null.");
            if (pageSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");
            return next(query, pageSize);
        }
    }
}
