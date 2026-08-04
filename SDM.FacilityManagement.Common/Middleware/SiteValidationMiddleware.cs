namespace Skyline.DataMiner.SDM.FacilityManagement.Common.Middleware
{
    using System;
    using System.Collections.Generic;

    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM.FacilityManagement.Validation;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Middleware;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    internal class SiteValidationMiddleware : ValidationMiddleware<Site>
    {
        internal SiteValidationMiddleware(SiteValidator validator)
            : base(validator)
        {
        }

        protected override Exception BuildBulkValidationException(List<Site> entities, List<ValidationResult> results)
            => new BulkValidationException<Site>(
                entities,
                results,
                s => string.IsNullOrEmpty(s.SiteId) ? $"Site '{s.Identifier}'" : $"Site '{s.SiteId}'");
    }
}
