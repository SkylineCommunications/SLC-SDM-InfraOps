namespace Skyline.DataMiner.SDM.InfraOpsProperties.Middleware
{
    using System;
    using System.Collections.Generic;

    using Skyline.DataMiner.SDM.InfraOpsProperties.Models;
    using Skyline.DataMiner.SDM.InfraOpsProperties.Validation;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Middleware;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    internal class PropertyValidationMiddleware : ValidationMiddleware<Property>
    {
        internal PropertyValidationMiddleware(PropertyValidator validator)
            : base(validator)
        {
        }

        protected override Exception BuildBulkValidationException(List<Property> entities, List<ValidationResult> results)
            => new BulkValidationException<Property>(
                entities,
                results,
                p => string.IsNullOrEmpty(p.Name) ? $"Property '{p.Identifier}'" : $"Property '{p.Name}'");
    }
}
