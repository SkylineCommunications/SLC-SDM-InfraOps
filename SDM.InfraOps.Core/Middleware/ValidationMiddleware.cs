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
    /// Generic validation middleware that delegates create/update validation to an <see cref="IValidator{T}"/>
    /// and, optionally, delete pre-validation to a caller-supplied function.
    /// All read and count operations are passed through unchanged after a null-guard.
    /// </summary>
    internal class ValidationMiddleware<T> : IBulkRepositoryMiddleware<T>
        where T : class
    {
        private readonly IValidator<T> _validator;
        private readonly Func<T, string> _getDisplayName;
        private readonly Func<T, ValidationResult> _validateOnDelete;

        /// <summary>
        /// Initializes a new instance of <see cref="ValidationMiddleware{T}"/>.
        /// </summary>
        /// <param name="validator">The validator used for create/update operations.</param>
        /// <param name="getDisplayName">
        /// Optional function that returns a human-readable label for an entity, used in
        /// <see cref="BulkValidationException{T}"/> messages.
        /// </param>
        /// <param name="validateOnDelete">
        /// Optional function called before each delete. When the returned <see cref="ValidationResult"/>
        /// is invalid the delete is aborted with a <see cref="ValidationException"/> (single) or
        /// <see cref="BulkValidationException{T}"/> (bulk).
        /// </param>
        internal ValidationMiddleware(
            IValidator<T> validator,
            Func<T, string> getDisplayName = null,
            Func<T, ValidationResult> validateOnDelete = null)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _getDisplayName = getDisplayName;
            _validateOnDelete = validateOnDelete;
        }

        // ── Write operations ────────────────────────────────────────────────

        public virtual T OnCreate(T oToCreate, Func<T, T> next)
        {
            var result = _validator.Validate(oToCreate);
            if (!result.IsValid)
                throw result.ToException();
            return next(oToCreate);
        }

        public virtual IReadOnlyCollection<T> OnCreate(IEnumerable<T> oToCreate, Func<IEnumerable<T>, IReadOnlyCollection<T>> next)
        {
            var items = oToCreate.ToList();
            var results = _validator.ValidateBulk(items);
            if (results.AnyInvalid())
                throw new BulkValidationException<T>(items, results, _getDisplayName);
            return next(oToCreate);
        }

        public virtual IReadOnlyCollection<T> OnCreateOrUpdate(IEnumerable<T> oToCreateOrUpdate, Func<IEnumerable<T>, IReadOnlyCollection<T>> next)
        {
            var items = oToCreateOrUpdate.ToList();
            var results = _validator.ValidateBulk(items);
            if (results.AnyInvalid())
                throw new BulkValidationException<T>(items, results, _getDisplayName);
            return next(oToCreateOrUpdate);
        }

        public virtual T OnUpdate(T oToUpdate, Func<T, T> next)
        {
            var result = _validator.Validate(oToUpdate);
            if (!result.IsValid)
                throw result.ToException();
            return next(oToUpdate);
        }

        public virtual IReadOnlyCollection<T> OnUpdate(IEnumerable<T> oToUpdate, Func<IEnumerable<T>, IReadOnlyCollection<T>> next)
        {
            var items = oToUpdate.ToList();
            var results = _validator.ValidateBulk(items);
            if (results.AnyInvalid())
                throw new BulkValidationException<T>(items, results, _getDisplayName);
            return next(oToUpdate);
        }

        public virtual void OnDelete(T oToDelete, Action<T> next)
        {
            if (oToDelete is null)
                throw new ArgumentNullException(nameof(oToDelete));

            if (_validateOnDelete != null)
            {
                var result = _validateOnDelete(oToDelete);
                if (!result.IsValid)
                    throw result.ToException();
            }

            next(oToDelete);
        }

        public virtual void OnDelete(IEnumerable<T> oToDelete, Action<IEnumerable<T>> next)
        {
            if (oToDelete is null)
                throw new ArgumentNullException(nameof(oToDelete));

            if (_validateOnDelete != null)
            {
                var items = oToDelete.ToList();
                var results = items.Select(_validateOnDelete).ToList();
                if (results.AnyInvalid())
                    throw new BulkValidationException<T>(items, results, _getDisplayName);
                next(items);
                return;
            }

            next(oToDelete);
        }

        // ── Pass-through operations (null-guarded) ──────────────────────────

        public virtual IEnumerable<T> OnRead(FilterElement<T> filter, Func<FilterElement<T>, IEnumerable<T>> next)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter), "Filter cannot be null.");
            return next(filter);
        }

        public virtual IEnumerable<T> OnRead(IQuery<T> query, Func<IQuery<T>, IEnumerable<T>> next)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query), "Query cannot be null.");
            return next(query);
        }

        public virtual long OnCount(FilterElement<T> filter, Func<FilterElement<T>, long> next)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter), "Filter cannot be null.");
            return next(filter);
        }

        public virtual long OnCount(IQuery<T> query, Func<IQuery<T>, long> next)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query), "Query cannot be null.");
            return next(query);
        }

        public virtual IEnumerable<IPagedResult<T>> OnReadPaged(FilterElement<T> filter, Func<FilterElement<T>, IEnumerable<IPagedResult<T>>> next)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter), "Filter cannot be null.");
            return next(filter);
        }

        public virtual IEnumerable<IPagedResult<T>> OnReadPaged(IQuery<T> query, Func<IQuery<T>, IEnumerable<IPagedResult<T>>> next)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query), "Query cannot be null.");
            return next(query);
        }

        public virtual IEnumerable<IPagedResult<T>> OnReadPaged(FilterElement<T> filter, int pageSize, Func<FilterElement<T>, int, IEnumerable<IPagedResult<T>>> next)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter), "Filter cannot be null.");
            if (pageSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");
            return next(filter, pageSize);
        }

        public virtual IEnumerable<IPagedResult<T>> OnReadPaged(IQuery<T> query, int pageSize, Func<IQuery<T>, int, IEnumerable<IPagedResult<T>>> next)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query), "Query cannot be null.");
            if (pageSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");
            return next(query, pageSize);
        }
    }
}
