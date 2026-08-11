namespace Skyline.DataMiner.SDM.FacilityManagement.Common.Middleware
{
    using System;
    using System.Collections.Generic;

    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM.FacilityManagement.Validation;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Middleware;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    internal class ZoneValidationMiddleware : ValidationMiddleware<Zone>
    {
        internal ZoneValidationMiddleware(ZoneValidator validator)
            : base(validator)
        {
        }

        protected override Exception BuildBulkValidationException(List<Zone> entities, List<ValidationResult> results)
            => new BulkValidationException<Zone>(
                entities,
                results,
                e => string.IsNullOrEmpty(e.ZoneId) ? $"Zone '{e.Identifier}'" : $"Zone '{e.ZoneId}'");
    }
}
