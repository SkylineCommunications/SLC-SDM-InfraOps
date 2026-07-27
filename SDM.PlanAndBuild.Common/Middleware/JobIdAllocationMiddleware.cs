namespace Skyline.DataMiner.SDM.PlanAndBuild.Middleware
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.PlanAndBuild.Helpers;
    using Skyline.DataMiner.SDM.PlanAndBuild.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Middleware;

    using SLDataGateway.API.Types.Querying;

    /// <summary>
    /// Middleware that allocates a system-generated <see cref="PlanAndBuildJob.JobID"/> before a job
    /// reaches the repository. Handles single and bulk creates, and the new-job subset of
    /// create-or-update. Mirrors the pattern of <c>IdentifierMiddleware&lt;T&gt;</c> in Core.
    /// </summary>
    internal class JobIdAllocationMiddleware : IBulkRepositoryMiddleware<PlanAndBuildJob>
    {
        private readonly IPlanAndBuildApiHelper _helper;

        internal JobIdAllocationMiddleware(IPlanAndBuildApiHelper helper)
        {
            _helper = helper ?? throw new ArgumentNullException(nameof(helper));
        }

        public PlanAndBuildJob OnCreate(PlanAndBuildJob oToCreate, Func<PlanAndBuildJob, PlanAndBuildJob> next)
        {
            oToCreate.JobID = JobIdAllocator.AllocateNextJobId(_helper);
            return next(oToCreate);
        }

        public IReadOnlyCollection<PlanAndBuildJob> OnCreate(
            IEnumerable<PlanAndBuildJob> oToCreate,
            Func<IEnumerable<PlanAndBuildJob>, IReadOnlyCollection<PlanAndBuildJob>> next)
        {
            var jobs = oToCreate.ToList();
            foreach (var job in jobs)
            {
                job.JobID = JobIdAllocator.AllocateNextJobId(_helper);
            }

            return next(jobs);
        }

        public IReadOnlyCollection<PlanAndBuildJob> OnCreateOrUpdate(
            IEnumerable<PlanAndBuildJob> oToCreateOrUpdate,
            Func<IEnumerable<PlanAndBuildJob>, IReadOnlyCollection<PlanAndBuildJob>> next)
        {
            var jobs = oToCreateOrUpdate.ToList();
            foreach (var job in jobs.Where(j => j.IsNew))
            {
                job.JobID = JobIdAllocator.AllocateNextJobId(_helper);
            }

            return next(jobs);
        }

        public IReadOnlyCollection<PlanAndBuildJob> OnUpdate(
            IEnumerable<PlanAndBuildJob> oToUpdate,
            Func<IEnumerable<PlanAndBuildJob>, IReadOnlyCollection<PlanAndBuildJob>> next)
        {
            return next(oToUpdate);
        }

        public PlanAndBuildJob OnUpdate(PlanAndBuildJob oToUpdate, Func<PlanAndBuildJob, PlanAndBuildJob> next)
        {
            return next(oToUpdate);
        }

        public void OnDelete(IEnumerable<PlanAndBuildJob> oToDelete, Action<IEnumerable<PlanAndBuildJob>> next)
        {
            next(oToDelete);
        }

        public void OnDelete(PlanAndBuildJob oToDelete, Action<PlanAndBuildJob> next)
        {
            next(oToDelete);
        }

        public IEnumerable<PlanAndBuildJob> OnRead(
            FilterElement<PlanAndBuildJob> filter,
            Func<FilterElement<PlanAndBuildJob>, IEnumerable<PlanAndBuildJob>> next)
        {
            return next(filter);
        }

        public IEnumerable<PlanAndBuildJob> OnRead(
            IQuery<PlanAndBuildJob> query,
            Func<IQuery<PlanAndBuildJob>, IEnumerable<PlanAndBuildJob>> next)
        {
            return next(query);
        }

        public long OnCount(
            FilterElement<PlanAndBuildJob> filter,
            Func<FilterElement<PlanAndBuildJob>, long> next)
        {
            return next(filter);
        }

        public long OnCount(
            IQuery<PlanAndBuildJob> query,
            Func<IQuery<PlanAndBuildJob>, long> next)
        {
            return next(query);
        }

        public IEnumerable<IPagedResult<PlanAndBuildJob>> OnReadPaged(
            FilterElement<PlanAndBuildJob> filter,
            Func<FilterElement<PlanAndBuildJob>, IEnumerable<IPagedResult<PlanAndBuildJob>>> next)
        {
            return next(filter);
        }

        public IEnumerable<IPagedResult<PlanAndBuildJob>> OnReadPaged(
            IQuery<PlanAndBuildJob> query,
            Func<IQuery<PlanAndBuildJob>, IEnumerable<IPagedResult<PlanAndBuildJob>>> next)
        {
            return next(query);
        }

        public IEnumerable<IPagedResult<PlanAndBuildJob>> OnReadPaged(
            FilterElement<PlanAndBuildJob> filter,
            int pageSize,
            Func<FilterElement<PlanAndBuildJob>, int, IEnumerable<IPagedResult<PlanAndBuildJob>>> next)
        {
            return next(filter, pageSize);
        }

        public IEnumerable<IPagedResult<PlanAndBuildJob>> OnReadPaged(
            IQuery<PlanAndBuildJob> query,
            int pageSize,
            Func<IQuery<PlanAndBuildJob>, int, IEnumerable<IPagedResult<PlanAndBuildJob>>> next)
        {
            return next(query, pageSize);
        }
    }
}
