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
        private readonly Validator<InfraopsReservation> _validationPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="InfraopsReservationValidator"/> class.
        /// </summary>
        /// <param name="entityLoader">Shared entity loader service.</param>
        public InfraopsReservationValidator(SdmEntityLoader entityLoader)
        {
            _entityLoader = entityLoader ?? throw new ArgumentNullException(nameof(entityLoader));
            _validationCore = new AssetValidationCore(_entityLoader);
            _validationPipeline = BuildValidationPipeline();
        }

        #region Public API

        /// <summary>
        /// Validates an InfraopsReservation and returns ValidationResult.
        /// Collects all errors without throwing exceptions.
        /// <para><b>Not suitable for bulk scenarios</b>: issues one DB query per item. Use <see cref="ValidateBulk"/> instead.</para>
        /// </summary>
        protected override ValidationResult Validate(InfraopsReservation reservation)
        {
            return _validationPipeline.Validate(reservation);
        }

        /// <summary>
        /// Validates an InfraopsReservation and throws ValidationException if invalid.
        /// Use this when you want fail-fast behavior.
        /// </summary>
        public void ValidateAndThrow(InfraopsReservation reservation)
        {
            _validationPipeline.ValidateAndThrow(reservation);
        }

        /// <summary>
        /// Validates with custom error handling callback.
        /// </summary>
        public ValidationResult ValidateWithHandler(InfraopsReservation reservation, Action<ValidationResult> onError)
        {
            return _validationPipeline.ValidateWithHandler(reservation, onError);
        }

        /// <summary>
        /// Validates a batch of InfraopsReservations in three phases:
        /// 1. Non-database checks per item (fast-fail on business rule violations).
        /// 2. Rack existence check in bulk against the database.
        /// 3. Placement/occupancy validation against the database.
        /// Results are returned in the same order as the input list.
        /// </summary>
        protected override List<ValidationResult> ValidateBulk(List<InfraopsReservation> reservations)
        {
            if (reservations == null || !reservations.Any())
            {
                return new List<ValidationResult>();
            }

            var results = reservations.Select(_ => new ValidationResult()).ToList();

            // ============================================================
            // PHASE 1: NO DATABASE ACCESS CHECKS (BUSINESS RULES)
            // ============================================================
            for (int i = 0; i < reservations.Count; i++)
            {
                results[i].AddFailuresFrom(ValidateWithoutDatabaseAccess(reservations[i]));
            }

            if (results.AnyInvalid())
            {
                return results;
            }

            // ============================================================
            // PHASE 2: RACK EXISTENCE CHECK (BULK, AGAINST DATABASE)
            // ============================================================
            var rackIds = reservations
                .Where(r => r.RackFk.Rack != null && r.RackFk.Rack.HasValue())
                .Select(r => r.RackFk.Rack.Identifier)
                .Distinct()
                .ToList();
            var rackMap = _entityLoader.GetRacksByDomIds(rackIds).ToDictionary(r => r.Identifier);

            for (int i = 0; i < reservations.Count; i++)
            {
                var rackFk = reservations[i].RackFk.Rack;
                if (rackFk == null || !rackFk.HasValue())
                {
                    continue;
                }

                if (!rackMap.ContainsKey(rackFk.Identifier))
                {
                    results[i].AddFailReason(InfraopsReservationValidationField.Rack, $"Referenced Rack '{rackFk.Identifier}' does not exist.");
                }
            }

            if (results.AnyInvalid())
            {
                return results;
            }

            // ============================================================
            // PHASE 3: PLACEMENT / OCCUPANCY VALIDATION (DATABASE)
            // ============================================================
            for (int i = 0; i < reservations.Count; i++)
            {
                var reservation = reservations[i];
                var rack = rackMap[reservation.RackFk.Rack.Identifier];
                results[i].AddFailuresFrom(ValidatePlacement(reservation, rack));
            }

            return results;
        }

        #endregion

        #region Pipeline Construction (Single Validation)

        private Validator<InfraopsReservation> BuildValidationPipeline()
        {
            // Phase 1: No database access checks (fail fast on business rules)
            var noDatabaseChecks = Validator<InfraopsReservation>
                .Create(r => ValidateWithoutDatabaseAccess(r))
                .StopOnFailure();

            // Phase 2: Database access checks (rack existence, placement)
            var databaseChecks = Validator<InfraopsReservation>
                .Create(r => ValidateWithDatabaseAccess(r));

            return noDatabaseChecks.AndThen(databaseChecks);
        }

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
        /// Validates rack existence and placement/occupancy for a single reservation.
        /// Only called after no-database checks pass.
        /// </summary>
        private ValidationResult ValidateWithDatabaseAccess(InfraopsReservation reservation)
        {
            var rack = _entityLoader.LoadRack(reservation.RackFk.Rack);
            if (rack == null)
            {
                var result = new ValidationResult();
                result.AddFailReason(InfraopsReservationValidationField.Rack, "Rack not found.");
                return result;
            }

            return ValidatePlacement(reservation, rack);
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
