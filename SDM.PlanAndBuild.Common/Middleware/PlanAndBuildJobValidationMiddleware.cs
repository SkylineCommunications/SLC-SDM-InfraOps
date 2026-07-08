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

    internal class PlanAndBuildJobValidationMiddleware : IBulkRepositoryMiddleware<PlanAndBuildJob>
    {
        private readonly PlanAndBuildJobValidator _validator;

        internal PlanAndBuildJobValidationMiddleware(PlanAndBuildJobValidator validator)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public long OnCount(FilterElement<PlanAndBuildJob> filter, Func<FilterElement<PlanAndBuildJob>, long> next)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter), "Filter cannot be null.");
            }

            return next(filter);
        }

        public long OnCount(IQuery<PlanAndBuildJob> query, Func<IQuery<PlanAndBuildJob>, long> next)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), "Query cannot be null.");
            }

            return next(query);
        }

        public IReadOnlyCollection<PlanAndBuildJob> OnCreate(IEnumerable<PlanAndBuildJob> oToCreate, Func<IEnumerable<PlanAndBuildJob>, IReadOnlyCollection<PlanAndBuildJob>> next)
        {
            var jobs = oToCreate.ToList();
            var results = ValidateBulk(jobs);

            if (results.AnyInvalid())
            {
                throw BuildBulkValidationException(jobs, results);
            }

            return next(oToCreate);
        }

        public PlanAndBuildJob OnCreate(PlanAndBuildJob oToCreate, Func<PlanAndBuildJob, PlanAndBuildJob> next)
        {
            var result = Validate(oToCreate);
            if (!result.IsValid)
            {
                throw result.ToException();
            }

            return next(oToCreate);
        }

        public IReadOnlyCollection<PlanAndBuildJob> OnCreateOrUpdate(IEnumerable<PlanAndBuildJob> oToCreateOrUpdate, Func<IEnumerable<PlanAndBuildJob>, IReadOnlyCollection<PlanAndBuildJob>> next)
        {
            var jobs = oToCreateOrUpdate.ToList();
            var results = ValidateBulk(jobs);

            if (results.AnyInvalid())
            {
                throw BuildBulkValidationException(jobs, results);
            }

            return next(oToCreateOrUpdate);
        }

        public void OnDelete(IEnumerable<PlanAndBuildJob> oToDelete, Action<IEnumerable<PlanAndBuildJob>> next)
        {
            if (oToDelete is null)
            {
                throw new ArgumentNullException(nameof(oToDelete), "The collection of jobs to delete cannot be null.");
            }

            next(oToDelete);
        }

        public void OnDelete(PlanAndBuildJob oToDelete, Action<PlanAndBuildJob> next)
        {
            if (oToDelete is null)
            {
                throw new ArgumentNullException(nameof(oToDelete), "The job to delete cannot be null.");
            }

            next(oToDelete);
        }

        public IEnumerable<PlanAndBuildJob> OnRead(FilterElement<PlanAndBuildJob> filter, Func<FilterElement<PlanAndBuildJob>, IEnumerable<PlanAndBuildJob>> next)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter), "Filter cannot be null.");
            }

            return next(filter);
        }

        public IEnumerable<PlanAndBuildJob> OnRead(IQuery<PlanAndBuildJob> query, Func<IQuery<PlanAndBuildJob>, IEnumerable<PlanAndBuildJob>> next)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), "Query cannot be null.");
            }

            return next(query);
        }

        public IEnumerable<IPagedResult<PlanAndBuildJob>> OnReadPaged(FilterElement<PlanAndBuildJob> filter, Func<FilterElement<PlanAndBuildJob>, IEnumerable<IPagedResult<PlanAndBuildJob>>> next)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter), "Filter cannot be null.");
            }

            return next(filter);
        }

        public IEnumerable<IPagedResult<PlanAndBuildJob>> OnReadPaged(IQuery<PlanAndBuildJob> query, Func<IQuery<PlanAndBuildJob>, IEnumerable<IPagedResult<PlanAndBuildJob>>> next)
        {
            if (query == null)
            {
                throw new ArgumentNullException(nameof(query), "Query cannot be null.");
            }

            return next(query);
        }

        public IEnumerable<IPagedResult<PlanAndBuildJob>> OnReadPaged(FilterElement<PlanAndBuildJob> filter, int pageSize, Func<FilterElement<PlanAndBuildJob>, int, IEnumerable<IPagedResult<PlanAndBuildJob>>> next)
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

        public IEnumerable<IPagedResult<PlanAndBuildJob>> OnReadPaged(IQuery<PlanAndBuildJob> query, int pageSize, Func<IQuery<PlanAndBuildJob>, int, IEnumerable<IPagedResult<PlanAndBuildJob>>> next)
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

        public IReadOnlyCollection<PlanAndBuildJob> OnUpdate(IEnumerable<PlanAndBuildJob> oToUpdate, Func<IEnumerable<PlanAndBuildJob>, IReadOnlyCollection<PlanAndBuildJob>> next)
        {
            var jobs = oToUpdate.ToList();
            var results = ValidateBulk(jobs);

            if (results.AnyInvalid())
            {
                throw BuildBulkValidationException(jobs, results);
            }

            return next(oToUpdate);
        }

        public PlanAndBuildJob OnUpdate(PlanAndBuildJob oToUpdate, Func<PlanAndBuildJob, PlanAndBuildJob> next)
        {
            var result = Validate(oToUpdate);

            if (!result.IsValid)
            {
                throw result.ToException();
            }

            return next(oToUpdate);
        }

        private ValidationResult Validate(PlanAndBuildJob job)
        {
            return _validator.Validate(job);
        }

        private List<ValidationResult> ValidateBulk(List<PlanAndBuildJob> jobs)
        {
            // Validate each job individually
            return jobs.Select(j => _validator.Validate(j)).ToList();
        }

        /// <summary>
        /// Builds a comprehensive exception from bulk validation results.
        /// Uses the generic BulkValidationException with entity references.
        /// </summary>
        private Exception BuildBulkValidationException(List<PlanAndBuildJob> jobs, List<ValidationResult> results)
        {
            return new BulkValidationException<PlanAndBuildJob>(
                jobs,
                results,
                job => string.IsNullOrEmpty(job.JobName)
                    ? $"Job '{job.Identifier}'"
                    : $"Job '{job.JobName}'");
        }
    }
}
