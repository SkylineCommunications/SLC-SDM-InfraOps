namespace Skyline.DataMiner.SDM.AssetManagement.Common.Middleware
{
    using System;
    using System.Collections.Generic;

    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.AssetManagement.Validation;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Middleware;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    internal class AssetValidationMiddleware : ValidationMiddleware<Asset>
    {
        internal AssetValidationMiddleware(AssetValidator validator)
            : base(validator)
        {
        }

        protected override Exception BuildBulkValidationException(List<Asset> entities, List<ValidationResult> results)
            => new BulkValidationException<Asset>(
                entities,
                results,
                a => string.IsNullOrEmpty(a.Name) ? $"Asset with ID '{a.AssetID}'" : $"Asset '{a.Name}'");
    }
}
