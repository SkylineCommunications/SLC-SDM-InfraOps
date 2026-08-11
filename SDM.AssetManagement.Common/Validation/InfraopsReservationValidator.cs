namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.SDM.AssetManagement.Models;

    using Skyline.DataMiner.SDM.Common.Services;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    public class InfraopsReservationValidator : ValidatorBase<InfraopsReservation>
    {
        private readonly SdmEntityLoader _entityLoader;

        public InfraopsReservationValidator(SdmEntityLoader entityLoader)
        {
            _entityLoader = entityLoader ?? throw new ArgumentNullException(nameof(entityLoader));
        }

        protected override ValidationResult Validate(InfraopsReservation reservation)
        {
            return ValidateBulk(new List<InfraopsReservation> { reservation })[0];
        }

        protected override List<ValidationResult> ValidateBulk(List<InfraopsReservation> reservations)
        {
            if (reservations == null || !reservations.Any())
            {
                return new List<ValidationResult>();
            }

            var results = reservations.Select(_ => new ValidationResult()).ToList();
            var rackIds = reservations
                .Where(r => r.RackFk?.Rack != null && r.RackFk.Rack.HasValue())
                .Select(r => r.RackFk.Rack.Identifier)
                .Distinct()
                .ToList();
            var existingRackIds = _entityLoader.GetRacksByDomIds(rackIds).Select(r => r.Identifier).ToHashSet();

            for (int i = 0; i < reservations.Count; i++)
            {
                var reservation = reservations[i];
                if (reservation.RackFk?.Rack != null && reservation.RackFk.Rack.HasValue() && !existingRackIds.Contains(reservation.RackFk.Rack.Identifier))
                {
                    results[i].AddFailReason("Reservation.Rack", "Rack", $"Referenced Rack '{reservation.RackFk.Rack.Identifier}' does not exist.");
                }
            }

            return results;
        }
    }
}
