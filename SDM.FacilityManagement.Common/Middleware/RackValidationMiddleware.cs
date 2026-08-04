namespace Skyline.DataMiner.SDM.FacilityManagement.Common.Middleware
{
    using System;
    using System.Collections.Generic;

    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM.FacilityManagement.Validation;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Middleware;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    internal class RackValidationMiddleware : ValidationMiddleware<Rack>
    {
        internal RackValidationMiddleware(RackValidator validator)
            : base(validator)
        {
        }

        protected override Exception BuildBulkValidationException(List<Rack> entities, List<ValidationResult> results)
            => new BulkValidationException<Rack>(
                entities,
                results,
                r => string.IsNullOrEmpty(r.RackId) ? $"Rack '{r.Identifier}'" : $"Rack '{r.RackId}'");
    }
}
