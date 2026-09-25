namespace Skyline.DataMiner.SDM.AssetManagement.Common.Middleware
{
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.AssetManagement.Validation;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Middleware;

    internal class HistoryValidationMiddleware : ValidationMiddleware<History>
    {
        internal HistoryValidationMiddleware(HistoryValidator validator)
            : base(validator)
        {
        }
    }
}