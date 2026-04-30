namespace Skyline.DataMiner.SDM.FacilityManagement.Validation
{
    using System.Collections.Generic;
    using System.Linq;

    using SharedCommonLibrary.AssetManagement.Models;

    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Static validation handler for Rack business rules.
    /// Contains pure validation logic without data access.
    /// </summary>
    public static class RackValidationHandler
    {
        private const double MAX_RACK_CAPACITY_U = 70;
        private const double MAX_RACK_SIZE_CM = 120;
        private const double MAX_RACK_HEIGHT_CM = 320;

        public enum RackValidationField
        {
            Rack,
            RackId,
            Name,
            RackUnits,
            RackPosition,
            Width,
            Depth,
            Height,
            PowerCapacity,
            RackSpacePosition,
            RackSpaceOccupied,
        }

        #region Dimensions Validation

        public static bool IsRackHeightValid(Rack rack, out ValidationResult result)
        {
            result = new ValidationResult();

            if (rack == null)
            {
                result.AddFailReason(RackValidationField.Rack, "Rack cannot be null.");
                return result.IsValid;
            }

            var height = rack.Height;

            if (height != null && (height > MAX_RACK_HEIGHT_CM || height < 0))
            {
                result.AddFailReason(RackValidationField.Height,
                    $"Rack Height must be between 0 and {MAX_RACK_HEIGHT_CM} cm.");
            }

            return result.IsValid;
        }

        public static bool IsRackDepthValid(Rack rack, out ValidationResult result)
        {
            result = new ValidationResult();

            if (rack == null)
            {
                result.AddFailReason(RackValidationField.Rack, "Rack cannot be null.");
                return result.IsValid;
            }

            var depth = rack.Depth;

            if (depth != null && (depth > MAX_RACK_SIZE_CM || depth < 0))
            {
                result.AddFailReason(RackValidationField.Depth,
                    $"Rack Depth must be between 0 and {MAX_RACK_SIZE_CM} cm.");
            }

            return result.IsValid;
        }

        public static bool IsRackWidthValid(Rack rack, out ValidationResult result)
        {
            result = new ValidationResult();

            if (rack == null)
            {
                result.AddFailReason(RackValidationField.Rack, "Rack cannot be null.");
                return result.IsValid;
            }

            var width = rack.Width;

            if (width != null && (width > MAX_RACK_SIZE_CM || width < 0))
            {
                result.AddFailReason(RackValidationField.Width,
                    $"Rack Width must be between 0 and {MAX_RACK_SIZE_CM} cm.");
            }

            return result.IsValid;
        }

        public static bool IsRackUnitCapacityValid(Rack rack, out ValidationResult result)
        {
            result = new ValidationResult();

            if (rack == null)
            {
                result.AddFailReason(RackValidationField.Rack, "Rack cannot be null.");
                return result.IsValid;
            }

            var rackUnits = rack.Capacity?.MaximumRackCapacity;

            if (rackUnits == null)
            {
                result.AddFailReason(RackValidationField.RackUnits, "Rack Units cannot be empty.");
                return result.IsValid;
            }

            if (rackUnits > MAX_RACK_CAPACITY_U || rackUnits < 1)
            {
                result.AddFailReason(RackValidationField.RackUnits,
                    $"Rack Units must be between 1 and {MAX_RACK_CAPACITY_U}.");
            }

            return result.IsValid;
        }

        public static bool IsRackPowerCapacityValid(Rack rack, out ValidationResult result)
        {
            result = new ValidationResult();

            if (rack == null)
            {
                result.AddFailReason(RackValidationField.Rack, "Rack cannot be null.");
                return result.IsValid;
            }

            var powerCapacity = rack.Capacity?.MaximumPowerCapacity;

            if (powerCapacity != null && powerCapacity < 0)
            {
                result.AddFailReason(RackValidationField.PowerCapacity,
                    "Rack Power Capacity cannot be negative.");
            }

            return result.IsValid;
        }

        #endregion

        #region Rack Space Validation - Internal Helpers

        /// <summary>
        /// Validates position and bounds are valid.
        /// Internal helper for basic position checks.
        /// </summary>
        internal static bool CheckAssetConflicts(
            SharedMappers.DomIds.SlcFacility_Management.Enums.RackpositionenumEnum rackPosition,
            long startPosition,
            long endPosition,
            Asset currentAsset,
            List<(Asset Asset, int Position, int HeightU)> occupiedAssets,
            out ValidationResult result)
        {
            result = new ValidationResult();

            if (occupiedAssets == null)
            {
                return result.IsValid;
            }

            foreach (var occupied in occupiedAssets)
            {
                if (currentAsset != null && occupied.Asset.Identifier == currentAsset.Identifier)
                {
                    continue;
                }

                var (occupiedStart, occupiedEnd) = CalculateOccupiedRange(rackPosition, occupied.Position, occupied.HeightU);

                if (startPosition < occupiedEnd && endPosition > occupiedStart)
                {
                    result.AddFailReason(RackValidationField.RackSpaceOccupied,
                        $"Invalid Position. Rack space is already occupied by asset '{occupied.Asset.Name}' at position {occupied.Position}.");
                    return result.IsValid;
                }
            }

            return result.IsValid;
        }

        /// <summary>
        /// Checks if a specific range conflicts with reservations.
        /// Internal helper - not meant for public use.
        /// </summary>
        internal static bool CheckReservationConflicts(
            long startPosition,
            long endPosition,
            InfraopsReservation currentReservation,
            List<(InfraopsReservation Reservation, List<(long LowerBound, long UpperBound)> Ranges)> reservations,
            out ValidationResult result)
        {
            result = new ValidationResult();

            if (reservations == null)
            {
                return result.IsValid;
            }

            foreach (var reservation in reservations)
            {
                if (currentReservation != null && reservation.Reservation.Identifier == currentReservation.Identifier)
                {
                    continue;
                }

                foreach (var range in reservation.Ranges)
                {
                    long reservedStart = range.LowerBound - 1;
                    long reservedEnd = range.UpperBound;

                    if (startPosition < reservedEnd && endPosition > reservedStart)
                    {
                        result.AddFailReason(RackValidationField.RackSpacePosition,
                            $"Invalid Position. Rack space is already reserved (units {range.LowerBound}-{range.UpperBound}).");
                        return result.IsValid;
                    }
                }
            }

            return result.IsValid;
        }

        /// <summary>
        /// Calculates the occupied range (start and end positions) based on rack numbering scheme.
        /// Internal helper for position calculations.
        /// </summary>
        internal static (long StartPosition, long EndPosition) CalculateOccupiedRange(
            SharedMappers.DomIds.SlcFacility_Management.Enums.RackpositionenumEnum rackPosition,
            int position,
            int heightU)
        {
            int zeroBasedPosition = position - 1;
            long startPosition;

            if (rackPosition == SharedMappers.DomIds.SlcFacility_Management.Enums.RackpositionenumEnum.Top)
            {
                startPosition = zeroBasedPosition - (heightU - 1);
            }
            else
            {
                startPosition = zeroBasedPosition;
            }

            long endPosition = startPosition + heightU;

            return (startPosition, endPosition);
        }

        /// <summary>
        /// Validates position and bounds are valid.
        /// Internal helper for basic position checks.
        /// </summary>
        internal static bool ValidatePositionAndBounds(
            Rack rack,
            int position,
            int heightU,
            out ValidationResult result)
        {
            result = new ValidationResult();

            if (position <= 0)
            {
                result.AddFailReason(RackValidationField.RackSpacePosition,
                    "Invalid Position. Position must be greater than 0.");
                return result.IsValid;
            }

            if (heightU <= 0)
            {
                result.AddFailReason(RackValidationField.RackSpacePosition,
                    "Invalid Height. Height (U) must be greater than 0.");
                return result.IsValid;
            }

            var (startPos, endPos) = CalculateOccupiedRange(rack.Position, position, heightU);

            if (startPos < 0 || endPos > rack.Capacity.MaximumRackCapacity)
            {
                result.AddFailReason(RackValidationField.RackSpacePosition,
                    $"Invalid Position. Extends beyond rack boundaries (Rack has {rack.Capacity.MaximumRackCapacity} units).");
                return result.IsValid;
            }

            return result.IsValid;
        }

        #endregion

        #region Rack Space Validation - Public API

        /// <summary>
        /// Validates if an asset can be placed at the specified position in the rack.
        /// Public API for asset placement validation.
        /// </summary>
        public static bool IsAssetPlacementValid(
            Rack rack,
            int assetPosition,
            int assetHeightU,
            Asset currentAsset,
            List<(Asset Asset, int Position, int HeightU)> occupiedAssets,
            List<(InfraopsReservation Reservation, List<(long LowerBound, long UpperBound)> Ranges)> reservations,
            out ValidationResult result)
        {
            result = new ValidationResult();

            // Basic position validation
            if (!ValidatePositionAndBounds(rack, assetPosition, assetHeightU, out var boundsResult))
            {
                result = boundsResult;
                return result.IsValid;
            }

            var (startPos, endPos) = CalculateOccupiedRange(rack.Position, assetPosition, assetHeightU);

            // Check asset conflicts
            if (!CheckAssetConflicts(rack.Position, startPos, endPos, currentAsset, occupiedAssets, out var assetConflict))
            {
                result = assetConflict;
                return result.IsValid;
            }

            // Check reservation conflicts
            if (!CheckReservationConflicts(startPos, endPos, null, reservations, out var reservationConflict))
            {
                result = reservationConflict;
                return result.IsValid;
            }

            return result.IsValid;
        }

        /// <summary>
        /// Validates if a reservation can be placed at the specified ranges in the rack.
        /// Public API for reservation placement validation.
        /// </summary>
        public static bool IsReservationPlacementValid(
            Rack rack,
            InfraopsReservation currentReservation,
            List<(long LowerBound, long UpperBound)> reservationRanges,
            List<(Asset Asset, int Position, int HeightU)> occupiedAssets,
            List<(InfraopsReservation Reservation, List<(long LowerBound, long UpperBound)> Ranges)> otherReservations,
            out ValidationResult result)
        {
            result = new ValidationResult();

            if (reservationRanges == null || !reservationRanges.Any())
            {
                result.AddFailReason(RackValidationField.RackSpacePosition,
                    "Reservation must have at least one position range.");
                return result.IsValid;
            }

            // Validate each range in the reservation
            foreach (var range in reservationRanges)
            {
                int position = (int)range.LowerBound;
                int heightU = (int)(range.UpperBound - range.LowerBound + 1);

                if (!ValidatePositionAndBounds(rack, position, heightU, out var boundsResult))
                {
                    result.AddFailuresFrom(boundsResult);
                    return result.IsValid;
                }

                var (startPos, endPos) = CalculateOccupiedRange(rack.Position, position, heightU);

                // Check asset conflicts
                if (!CheckAssetConflicts(rack.Position, startPos, endPos, null, occupiedAssets, out var assetConflict))
                {
                    result.AddFailuresFrom(assetConflict);
                    return result.IsValid;
                }

                // Check reservation conflicts (excluding current reservation)
                if (!CheckReservationConflicts(startPos, endPos, currentReservation, otherReservations, out var reservationConflict))
                {
                    result.AddFailuresFrom(reservationConflict);
                    return result.IsValid;
                }
            }

            return result.IsValid;
        }

        #endregion
    }
}