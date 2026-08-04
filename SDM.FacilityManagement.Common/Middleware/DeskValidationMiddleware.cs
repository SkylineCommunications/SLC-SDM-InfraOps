namespace Skyline.DataMiner.SDM.FacilityManagement.Common.Middleware
{
    using System;
    using System.Collections.Generic;

    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM.FacilityManagement.Validation;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Middleware;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    internal class DeskValidationMiddleware : ValidationMiddleware<Desk>
    {
        internal DeskValidationMiddleware(DeskValidator validator)
            : base(validator)
        {
        }

        protected override Exception BuildBulkValidationException(List<Desk> entities, List<ValidationResult> results)
            => new BulkValidationException<Desk>(
                entities,
                results,
                e => string.IsNullOrEmpty(e.DeskID) ? $"Desk '{e.Identifier}'" : $"Desk '{e.DeskID}'");
    }
}
