namespace Skyline.DataMiner.SDM.PlanAndBuild.Middleware
{
    using System;
    using System.Collections.Generic;

    using Skyline.DataMiner.SDM.PlanAndBuild.Models;
    using Skyline.DataMiner.SDM.PlanAndBuild.Validation;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Middleware;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    internal class PlanAndBuildJobValidationMiddleware : ValidationMiddleware<PlanAndBuildJob>
    {
        internal PlanAndBuildJobValidationMiddleware(PlanAndBuildJobValidator validator)
            : base(validator)
        {
        }

        protected override Exception BuildBulkValidationException(List<PlanAndBuildJob> entities, List<ValidationResult> results)
            => new BulkValidationException<PlanAndBuildJob>(
                entities,
                results,
                job => string.IsNullOrEmpty(job.JobName) ? $"Job '{job.Identifier}'" : $"Job '{job.JobName}'");
    }
}
