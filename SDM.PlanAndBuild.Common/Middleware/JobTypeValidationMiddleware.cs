namespace Skyline.DataMiner.SDM.PlanAndBuild.Middleware
{
    using System;
    using System.Collections.Generic;

    using Skyline.DataMiner.SDM.PlanAndBuild.Models;
    using Skyline.DataMiner.SDM.PlanAndBuild.Validation;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Middleware;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    internal class JobTypeValidationMiddleware : ValidationMiddleware<JobType>
    {
        internal JobTypeValidationMiddleware(JobTypeValidator validator)
            : base(validator)
        {
        }

        protected override Exception BuildBulkValidationException(List<JobType> entities, List<ValidationResult> results)
            => new BulkValidationException<JobType>(
                entities,
                results,
                jt => string.IsNullOrEmpty(jt.Name) ? $"Job Type '{jt.Identifier}'" : $"Job Type '{jt.Name}'");
    }
}
