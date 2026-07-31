namespace Skyline.DataMiner.SDM.AssetManagement.Common.Middleware
{
    using System;
    using System.Collections.Generic;

    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.AssetManagement.Validation;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Middleware;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    internal class PowerPortValidationMiddleware : ValidationMiddleware<PowerPort>
    {
        internal PowerPortValidationMiddleware(PowerPortValidator validator)
            : base(validator)
        {
        }

        protected override Exception BuildBulkValidationException(List<PowerPort> entities, List<ValidationResult> results)
            => new BulkValidationException<PowerPort>(
                entities,
                results,
                p => string.IsNullOrEmpty(p.PowerPortInfo?.Name) ? $"PowerPort '{p.Identifier}'" : $"PowerPort '{p.PowerPortInfo.Name}'");
    }
}
