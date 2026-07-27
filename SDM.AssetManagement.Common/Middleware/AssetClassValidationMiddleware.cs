namespace Skyline.DataMiner.SDM.AssetManagement.Common.Middleware
{
    using System;
    using System.Collections.Generic;

    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.AssetManagement.Validation;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Middleware;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    internal class AssetClassValidationMiddleware : ValidationMiddleware<AssetClass>
    {
        internal AssetClassValidationMiddleware(AssetClassValidator validator)
            : base(validator)
        {
        }

        protected override Exception BuildBulkValidationException(List<AssetClass> entities, List<ValidationResult> results)
            => new BulkValidationException<AssetClass>(
                entities,
                results,
                ac => string.IsNullOrEmpty(ac.Name) ? $"AssetClass '{ac.Identifier}'" : $"AssetClass '{ac.Name}'");
    }
}
