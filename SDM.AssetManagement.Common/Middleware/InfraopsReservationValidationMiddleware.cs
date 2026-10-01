namespace Skyline.DataMiner.SDM.AssetManagement.Common.Middleware
{
    using Skyline.DataMiner.SDM.AssetManagement.Common.Validation.Reservations;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Middleware;

    internal class InfraopsReservationValidationMiddleware : ValidationMiddleware<InfraopsReservation>
    {
        internal InfraopsReservationValidationMiddleware(InfraopsReservationValidator validator)
            : base(validator)
        {
        }
    }
}
