namespace Skyline.DataMiner.SDM.PlanAndBuild.Middleware
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.SDM.PlanAndBuild.Helpers;
    using Skyline.DataMiner.SDM.PlanAndBuild.Models;
    using Skyline.DataMiner.SDM.PlanAndBuild.Validation;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Middleware;

    internal class PlanAndBuildJobValidationMiddleware : ValidationMiddleware<PlanAndBuildJob>
    {
        private readonly IPlanAndBuildApiHelper _helper;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanAndBuildJobValidationMiddleware"/> class.
        /// </summary>
        /// <param name="validator">The PlanAndBuildJob validator.</param>
        /// <param name="helper">
        /// The Plan &amp; Build API helper used to allocate a system-generated JobID (via <see cref="JobIdAllocator"/>)
        /// on every create. Note: this is captured by reference during <see cref="PlanAndBuildApiHelper"/>
        /// construction, before its repositories are wired up. Only <see cref="OnCreate(PlanAndBuildJob, Func{PlanAndBuildJob, PlanAndBuildJob})"/>
        /// / <see cref="OnCreate(IEnumerable{PlanAndBuildJob}, Func{IEnumerable{PlanAndBuildJob}, IReadOnlyCollection{PlanAndBuildJob}})"/>
        /// (called after construction completes) access <paramref name="helper"/>'s repositories.
        /// </param>
        internal PlanAndBuildJobValidationMiddleware(PlanAndBuildJobValidator validator, IPlanAndBuildApiHelper helper)
            : base(
                validator,
                job => string.IsNullOrEmpty(job.JobName) ? $"Job '{job.Identifier}'" : $"Job '{job.JobName}'")
        {
            _helper = helper ?? throw new ArgumentNullException(nameof(helper));
        }

        public override IReadOnlyCollection<PlanAndBuildJob> OnCreate(IEnumerable<PlanAndBuildJob> oToCreate, Func<IEnumerable<PlanAndBuildJob>, IReadOnlyCollection<PlanAndBuildJob>> next)
        {
            var jobs = oToCreate.ToList();

            foreach (var job in jobs)
            {
                job.JobID = JobIdAllocator.AllocateNextJobId(_helper);
            }

            return base.OnCreate(jobs, next);
        }

        public override PlanAndBuildJob OnCreate(PlanAndBuildJob oToCreate, Func<PlanAndBuildJob, PlanAndBuildJob> next)
        {
            oToCreate.JobID = JobIdAllocator.AllocateNextJobId(_helper);

            return base.OnCreate(oToCreate, next);
        }

        public override IReadOnlyCollection<PlanAndBuildJob> OnCreateOrUpdate(IEnumerable<PlanAndBuildJob> oToCreateOrUpdate, Func<IEnumerable<PlanAndBuildJob>, IReadOnlyCollection<PlanAndBuildJob>> next)
        {
            var jobs = oToCreateOrUpdate.ToList();

            foreach (var job in jobs.Where(j => j.IsNew))
            {
                job.JobID = JobIdAllocator.AllocateNextJobId(_helper);
            }

            return base.OnCreateOrUpdate(jobs, next);
        }
    }
}
