namespace Skyline.DataMiner.SDM.PlanAndBuild.Middleware
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.PlanAndBuild.Models;
    using Skyline.DataMiner.SDM.PlanAndBuild.Validation;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    using SLDataGateway.API.Types.Querying;

    internal class JobTypeValidationMiddleware : IBulkRepositoryMiddleware<JobType>
    {
        private readonly JobTypeValidator _validator;

        internal JobTypeValidationMiddleware(JobTypeValidator validator)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public long OnCount(FilterElement<JobType> filter, Func<FilterElement<JobType>, long> next)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter), "Filter cannot be null.");
            }

            return next(filter);
        }

        public long OnCount(IQuery<JobType> query, Func<IQuery<JobType>, long> next)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), "Query cannot be null.");
            }

            return next(query);
        }

        public IReadOnlyCollection<JobType> OnCreate(IEnumerable<JobType> oToCreate, Func<IEnumerable<JobType>, IReadOnlyCollection<JobType>> next)
        {
            var jobTypes = oToCreate.ToList();
            var results = ValidateBulk(jobTypes);

            if (results.AnyInvalid())
            {
                throw BuildBulkValidationException(jobTypes, results);
            }

            return next(oToCreate);
        }

        public JobType OnCreate(JobType oToCreate, Func<JobType, JobType> next)
        {
            var result = Validate(oToCreate);
            if (!result.IsValid)
            {
                throw result.ToException();
            }

            return next(oToCreate);
        }

        public IReadOnlyCollection<JobType> OnCreateOrUpdate(IEnumerable<JobType> oToCreateOrUpdate, Func<IEnumerable<JobType>, IReadOnlyCollection<JobType>> next)
        {
            var jobTypes = oToCreateOrUpdate.ToList();
            var results = ValidateBulk(jobTypes);

            if (results.AnyInvalid())
            {
                throw BuildBulkValidationException(jobTypes, results);
            }

            return next(oToCreateOrUpdate);
        }

        public void OnDelete(IEnumerable<JobType> oToDelete, Action<IEnumerable<JobType>> next)
        {
            if (oToDelete is null)
            {
                throw new ArgumentNullException(nameof(oToDelete), "The collection of job types to delete cannot be null.");
            }

            var jobTypes = oToDelete.ToList();
            var results = jobTypes.Select(jt => _validator.ValidateDeletion(jt)).ToList();

            if (results.AnyInvalid())
            {
                throw BuildBulkValidationException(jobTypes, results);
            }

            next(oToDelete);
        }

        public void OnDelete(JobType oToDelete, Action<JobType> next)
        {
            if (oToDelete is null)
            {
                throw new ArgumentNullException(nameof(oToDelete), "The job type to delete cannot be null.");
            }

            var result = _validator.ValidateDeletion(oToDelete);
            if (!result.IsValid)
            {
                throw result.ToException();
            }

            next(oToDelete);
        }

        public IEnumerable<JobType> OnRead(FilterElement<JobType> filter, Func<FilterElement<JobType>, IEnumerable<JobType>> next)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter), "Filter cannot be null.");
            }

            return next(filter);
        }

        public IEnumerable<JobType> OnRead(IQuery<JobType> query, Func<IQuery<JobType>, IEnumerable<JobType>> next)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), "Query cannot be null.");
            }

            return next(query);
        }

        public IEnumerable<IPagedResult<JobType>> OnReadPaged(FilterElement<JobType> filter, Func<FilterElement<JobType>, IEnumerable<IPagedResult<JobType>>> next)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter), "Filter cannot be null.");
            }

            return next(filter);
        }

        public IEnumerable<IPagedResult<JobType>> OnReadPaged(IQuery<JobType> query, Func<IQuery<JobType>, IEnumerable<IPagedResult<JobType>>> next)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), "Query cannot be null.");
            }

            return next(query);
        }

        public IEnumerable<IPagedResult<JobType>> OnReadPaged(FilterElement<JobType> filter, int pageSize, Func<FilterElement<JobType>, int, IEnumerable<IPagedResult<JobType>>> next)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter), "Filter cannot be null.");
            }

            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");
            }

            return next(filter, pageSize);
        }

        public IEnumerable<IPagedResult<JobType>> OnReadPaged(IQuery<JobType> query, int pageSize, Func<IQuery<JobType>, int, IEnumerable<IPagedResult<JobType>>> next)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), "Query cannot be null.");
            }

            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");
            }

            return next(query, pageSize);
        }

        public IReadOnlyCollection<JobType> OnUpdate(IEnumerable<JobType> oToUpdate, Func<IEnumerable<JobType>, IReadOnlyCollection<JobType>> next)
        {
            var jobTypes = oToUpdate.ToList();
            var results = ValidateBulk(jobTypes);

            if (results.AnyInvalid())
            {
                throw BuildBulkValidationException(jobTypes, results);
            }

            return next(oToUpdate);
        }

        public JobType OnUpdate(JobType oToUpdate, Func<JobType, JobType> next)
        {
            var result = Validate(oToUpdate);

            if (!result.IsValid)
            {
                throw result.ToException();
            }

            return next(oToUpdate);
        }

        private ValidationResult Validate(JobType jobType)
        {
            return _validator.Validate(jobType);
        }

        private List<ValidationResult> ValidateBulk(List<JobType> jobTypes)
        {
            // Validates each job type individually and detects Name conflicts within the batch itself.
            return _validator.ValidateBulk(jobTypes);
        }

        /// <summary>
        /// Builds a comprehensive exception from bulk validation results.
        /// Uses the generic BulkValidationException with entity references.
        /// </summary>
        private Exception BuildBulkValidationException(List<JobType> jobTypes, List<ValidationResult> results)
        {
            return new BulkValidationException<JobType>(
                jobTypes,
                results,
                jobType => string.IsNullOrEmpty(jobType.Name)
                    ? $"Job Type '{jobType.Identifier}'"
                    : $"Job Type '{jobType.Name}'");
        }
    }
}
