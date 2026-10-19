using Skyline.DataMiner.SDM.AssetManagement.Validation;

namespace Skyline.DataMiner.SDM.AssetManagement.Common.Validation.Reservations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.SDM.AssetManagement.Models;

    using Skyline.DataMiner.SDM.Common.Services;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    using static Skyline.DataMiner.SDM.FacilityManagement.Validation.RackValidationHandler;

    /// <summary>
    /// Public validator service for InfraopsReservation validation with comprehensive error handling.
    /// </summary>
    public class InfraopsReservationValidator : ValidatorBase<InfraopsReservation>
    {
        public enum InfraopsReservationValidationField
        {
            Rack,
            ReservedPositions,
        }

        private readonly SdmEntityLoader _entityLoader;
        private readonly AssetValidationCore _validationCore;

        /// <summary>
        /// Initializes a new instance of the <see cref="InfraopsReservationValidator"/> class.
        /// </summary>
        /// <param name="entityLoader">Shared entity loader service.</param>
        public InfraopsReservationValidator(SdmEntityLoader entityLoader)
        {
            _entityLoader = entityLoader ?? throw new ArgumentNullException(nameof(entityLoader));
            _validationCore = new AssetValidationCore(_entityLoader);
        }


        protected override List<ValidationResult> ValidateBulkNoDatabaseBusinessRules(List<InfraopsReservation> entities)
        {
            // ============================================================
            // PHASE 1: NO DATABASE ACCESS CHECKS (BUSINESS RULES)
            // ============================================================
            return entities.Select(ValidateWithoutDatabaseAccess).ToList();
        }

        protected override List<ValidationResult> ValidateBulkNoDatabaseBatchConflicts(List<InfraopsReservation> entities)
        {
            return entities.Select(_ => new ValidationResult()).ToList();
        }

        protected override List<ValidationResult> ValidateBulkWithDatabase(List<InfraopsReservation> entities)
        {
            var results = entities.Select(_ => new ValidationResult()).ToList();

            // ============================================================
            // PHASE 2: RACK EXISTENCE CHECK (BULK, AGAINST DATABASE)
            // ============================================================
            var rackIds = entities
                .Where(r => r.RackFk.Rack != null && r.RackFk.Rack.HasValue())
                .Select(r => r.RackFk.Rack.Identifier)
                .Distinct()
                .ToList();

            var rackMap = _entityLoader.GetRacksByDomIds(rackIds).ToDictionary(r => r.Identifier);

            for (int idx = 0; idx < entities.Count; idx++)
            {
                var rackFk = entities[idx].RackFk.Rack;
                if (rackFk == null || !rackFk.HasValue())
                {
                    continue;
                }

                if (!rackMap.ContainsKey(rackFk.Identifier))
                {
                    results[idx].AddFailReason(InfraopsReservationValidationField.Rack, $"Referenced Rack '{rackFk.Identifier}' does not exist.");
                }
            }

            if (results.AnyInvalid())
            {
                return results;
            }

            // ============================================================
            // PHASE 3: PLACEMENT / OCCUPANCY VALIDATION (DATABASE)
            // ============================================================
            for (int idx = 0; idx < entities.Count; idx++)
            {
                var reservation = entities[idx];
                var rack = rackMap[reservation.RackFk.Rack.Identifier];
                results[idx].AddFailuresFrom(ValidatePlacement(reservation, rack));
            }

            return results;
        }

        #region Pipeline Construction (Single Validation)

        /// <summary>
        /// Validates business rules that do not require database access.
        /// </summary>
        private ValidationResult ValidateWithoutDatabaseAccess(InfraopsReservation reservation)
        {
            var result = new ValidationResult();

            if (reservation.RackFk.IsEmpty)
            {
                result.AddFailReason(InfraopsReservationValidationField.Rack, "Reservation must have a Rack specified.");
            }

            if (reservation.ReservedPositions == null || !reservation.ReservedPositions.Any())
            {
                result.AddFailReason(InfraopsReservationValidationField.ReservedPositions, "Reservation must have at least one position range.");
            }

            return result;
        }

        /// <summary>
        /// Validates that a reservation's reserved positions do not conflict with existing assets
        /// or other reservations in the given rack.
        /// </summary>
        private ValidationResult ValidatePlacement(InfraopsReservation reservation, Rack rack)
        {
            var result = new ValidationResult();

            try
            {
                // Load all occupants (excluding current reservation)
                var occupiedAssets = _validationCore.LoadAllAssetsInRack(rack.Identifier);
                var otherReservations = _validationCore.LoadReservationsForRack(rack, reservation.Identifier);

                // Validate each range in the reservation
                foreach (var position in reservation.ReservedPositions)
                {
                    if (position.LowerBound == default || position.UpperBound == default)
                    {
                        continue;
                    }

                    int rangePosition = (int)position.LowerBound;
                    int rangeHeight = (int)(position.UpperBound - position.LowerBound + 1);

                    result.AddFailuresFrom(_validationCore.ValidateRangeOccupancy(
                        rack,
                        rangePosition,
                        rangeHeight,
                        null, // No current asset
                        reservation,
                        occupiedAssets,
                        otherReservations));
                }
            }
            catch (Exception ex)
            {
                result.AddFailReason(RackValidationField.RackSpacePosition,
                    $"Error validating reservation placement: {ex.Message}");
            }

            return result;
        }

        #endregion
    }
}
