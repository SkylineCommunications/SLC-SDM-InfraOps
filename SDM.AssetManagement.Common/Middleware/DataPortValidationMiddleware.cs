namespace Skyline.DataMiner.SDM.AssetManagement.Common.Middleware
{
    using System;
    using System.Collections.Generic;

    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.AssetManagement.Validation;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Middleware;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    internal class DataPortValidationMiddleware : ValidationMiddleware<DataPort>
    {
        internal DataPortValidationMiddleware(DataPortValidator validator)
            : base(validator)
        {
        }

        protected override Exception BuildBulkValidationException(List<DataPort> entities, List<ValidationResult> results)
            => new BulkValidationException<DataPort>(
                entities,
                results,
                p => string.IsNullOrEmpty(p.DataPortInfo?.Name) ? $"DataPort '{p.Identifier}'" : $"DataPort '{p.DataPortInfo.Name}'");
    }
}
