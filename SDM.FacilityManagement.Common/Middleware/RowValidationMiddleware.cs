namespace Skyline.DataMiner.SDM.FacilityManagement.Common.Middleware
{
    using System;
    using System.Collections.Generic;

    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM.FacilityManagement.Validation;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Middleware;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    internal class RowValidationMiddleware : ValidationMiddleware<Row>
    {
        internal RowValidationMiddleware(RowValidator validator)
            : base(validator)
        {
        }

        protected override Exception BuildBulkValidationException(List<Row> entities, List<ValidationResult> results)
            => new BulkValidationException<Row>(
                entities,
                results,
                e => string.IsNullOrEmpty(e.RowId) ? $"Row '{e.Identifier}'" : $"Row '{e.RowId}'");
    }
}
