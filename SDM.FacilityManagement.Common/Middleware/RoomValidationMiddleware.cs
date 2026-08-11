namespace Skyline.DataMiner.SDM.FacilityManagement.Common.Middleware
{
    using System;
    using System.Collections.Generic;

    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM.FacilityManagement.Validation;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Middleware;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    internal class RoomValidationMiddleware : ValidationMiddleware<Room>
    {
        internal RoomValidationMiddleware(RoomValidator validator)
            : base(validator)
        {
        }

        protected override Exception BuildBulkValidationException(List<Room> entities, List<ValidationResult> results)
            => new BulkValidationException<Room>(
                entities,
                results,
                e => string.IsNullOrEmpty(e.RoomId) ? $"Room '{e.Identifier}'" : $"Room '{e.RoomId}'");
    }
}
