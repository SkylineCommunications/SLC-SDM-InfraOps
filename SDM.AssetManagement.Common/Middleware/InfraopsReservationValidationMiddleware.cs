namespace Skyline.DataMiner.SDM.AssetManagement.Common.Middleware
{
    using Skyline.DataMiner.SDM.AssetManagement.Models;

    using Skyline.DataMiner.SDM.AssetManagement.Validation;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Middleware;

    internal class InfraopsReservationValidationMiddleware : ValidationMiddleware<InfraopsReservation>
    {
        internal InfraopsReservationValidationMiddleware(InfraopsReservationValidator validator)
            : base(validator)
        {
        }
    }
}
