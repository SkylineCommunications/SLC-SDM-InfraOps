namespace Skyline.DataMiner.SDM.InfraOpsProperties.Middleware
{
    using System;
    using System.Collections.Generic;

    using Skyline.DataMiner.SDM.InfraOpsProperties.Models;
    using Skyline.DataMiner.SDM.InfraOpsProperties.Validation;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Middleware;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    internal class PropertyValuesValidationMiddleware : ValidationMiddleware<PropertyValues>
    {
        internal PropertyValuesValidationMiddleware(PropertyValuesValidator validator)
            : base(validator)
        {
        }

        protected override Exception BuildBulkValidationException(List<PropertyValues> entities, List<ValidationResult> results)
            => new BulkValidationException<PropertyValues>(
                entities,
                results,
                pv => $"PropertyValues '{pv.Identifier}'");
    }
}
