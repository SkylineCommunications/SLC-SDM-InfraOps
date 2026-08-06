namespace Skyline.DataMiner.SDM.AssetManagement.Common.Middleware
{
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.AssetManagement.Validation;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Middleware;

    internal class PortTypeValidationMiddleware : ValidationMiddleware<PortType>
    {
        internal PortTypeValidationMiddleware(PortTypeValidator validator)
            : base(validator)
        {
        }
    }
}
