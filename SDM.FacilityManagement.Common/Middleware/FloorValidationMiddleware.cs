namespace Skyline.DataMiner.SDM.FacilityManagement.Common.Middleware
{
    using System;
    using System.Collections.Generic;

    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM.FacilityManagement.Validation;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Middleware;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    internal class FloorValidationMiddleware : ValidationMiddleware<Floor>
    {
        internal FloorValidationMiddleware(FloorValidator validator)
            : base(validator)
        {
        }

        protected override Exception BuildBulkValidationException(List<Floor> entities, List<ValidationResult> results)
            => new BulkValidationException<Floor>(
                entities,
                results,
                e => string.IsNullOrEmpty(e.FloorId) ? $"Floor '{e.Identifier}'" : $"Floor '{e.FloorId}'");
    }
}
