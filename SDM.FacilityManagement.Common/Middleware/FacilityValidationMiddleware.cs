namespace Skyline.DataMiner.SDM.FacilityManagement.Common.Middleware
{
    using System;
    using System.Collections.Generic;

    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM.FacilityManagement.Validation;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Middleware;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    internal class FacilityValidationMiddleware : ValidationMiddleware<Facility>
    {
        internal FacilityValidationMiddleware(FacilityValidator validator)
            : base(validator)
        {
        }

        protected override Exception BuildBulkValidationException(List<Facility> entities, List<ValidationResult> results)
            => new BulkValidationException<Facility>(
                entities,
                results,
                e => string.IsNullOrEmpty(e.FacilityId) ? $"Facility '{e.Identifier}'" : $"Facility '{e.FacilityId}'");
    }
}
