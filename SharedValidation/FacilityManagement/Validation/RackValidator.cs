namespace Skyline.DataMiner.SDM.FacilityManagement.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SharedCommonLibrary.AssetManagement.Models;

    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Common.Services;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    using static Skyline.DataMiner.SDM.FacilityManagement.Validation.RackValidationHandler;

    /// <summary>
    /// Public validator service for Rack validation with comprehensive error handling.
    /// </summary>
    public class RackValidator
    {
        private readonly SdmEntityLoader _entityLoader;
        private readonly Validator<Rack> _validationPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="RackValidator"/> class.
        /// </summary>
        /// <param name="entityLoader">Shared entity loader service (handles all data access).</param>
        public RackValidator(SdmEntityLoader entityLoader)
        {
            _entityLoader = entityLoader ?? throw new ArgumentNullException(nameof(entityLoader));
            _validationPipeline = BuildValidationPipeline();
        }

        #region Rack Validation

        /// <summary>
        /// Validates a Rack and returns ValidationResult.
        /// Collects all errors without throwing exceptions.
        /// </summary>
        public ValidationResult Validate(Rack rack)
        {
            if (rack == null)
            {
                throw new ArgumentNullException(nameof(rack));
            }

            return _validationPipeline.Validate(rack);
        }

        /// <summary>
        /// Validates a Rack and throws ValidationException if invalid.
        /// Use this when you want fail-fast behavior.
        /// </summary>
        public void ValidateAndThrow(Rack rack)
        {
            _validationPipeline.ValidateAndThrow(rack);
        }

        /// <summary>
        /// Validates with custom error handling callback.
        /// </summary>
        public ValidationResult ValidateWithHandler(Rack rack, Action<ValidationResult> onError)
        {
            return _validationPipeline.ValidateWithHandler(rack, onError);
        }

        #endregion

        #region Public API - Asset Placement

        /// <summary>
        /// Validates if an asset can be placed at a specific position in a rack.
        /// Automatically loads all necessary data (rack, other assets, reservations).
        /// </summary>
        /// <param name="asset">The asset to place.</param>
        /// <param name="rackId">The rack identifier (as Guid).</param>
        /// <param name="position">The desired position in the rack.</param>
        /// <returns>ValidationResult indicating if placement is valid.</returns>
        public ValidationResult ValidateAssetPlacement(Asset asset, string rackId, int position)
        {
            var result = new ValidationResult();

            if (asset == null)
            {
                result.AddFailReason(RackValidationField.RackSpacePosition, "Asset cannot be null.");
                return result;
            }

            if (rackId == default)
            {
                result.AddFailReason(RackValidationField.Rack, "Rack ID cannot be empty.");
                return result;
            }

            return ValidateAssetInRack(asset, rackId, position);
        }

        /// <summary>
        /// Validates if multiple assets can be placed in their specified rack locations.
        /// Optimized for bulk operations - loads rack data once per rack and checks batch conflicts.
        /// </summary>
        /// <param name="assets">List of assets to validate.</param>
        /// <returns>Dictionary mapping asset identifier to validation result.</returns>
        public Dictionary<string, ValidationResult> ValidateBulkAssetPlacements(List<Asset> assets)
        {
            var results = new Dictionary<string, ValidationResult>();

            if (assets == null || !assets.Any())
            {
                return results;
            }

            try
            {
                var context = BuildRackValidationContext(assets);

                foreach (var asset in assets)
                {
                    results[asset.Identifier] = ValidateAssetPlacementWithContext(asset, context);
                }
            }
            catch (Exception ex)
            {
                var globalError = new ValidationResult();
                globalError.AddFailReason(RackValidationField.Rack,
                    $"Error preparing bulk validation: {ex.Message}");

                foreach (var asset in assets)
                {
                    results[asset.Identifier] = globalError;
                }
            }

            return results;
        }

        #endregion

        #region Public API - Reservation Placement

        /// <summary>
        /// Validates if a reservation can be placed in the specified rack.
        /// Automatically loads all necessary data (rack, assets, other reservations).
        /// </summary>
        public ValidationResult ValidateReservationPlacement(InfraopsReservation reservation)
        {
            var result = new ValidationResult();

            if (reservation == null)
            {
                result.AddFailReason(RackValidationField.RackSpacePosition, "Reservation cannot be null.");
                return result;
            }

            if (reservation.RackFk == null || !reservation.RackFk.Rack.HasValue())
            {
                result.AddFailReason(RackValidationField.Rack, "Reservation must have a Rack specified.");
                return result;
            }

            if (reservation.ReservedPositions == null || !reservation.ReservedPositions.Any())
            {
                result.AddFailReason(RackValidationField.RackSpacePosition,
                    "Reservation must have at least one position range.");
                return result;
            }

            try
            {
                var rack = _entityLoader.LoadRack(reservation.RackFk.Rack);
                if (rack == null)
                {
                    result.AddFailReason(RackValidationField.Rack, "Rack not found.");
                    return result;
                }

                // Load all occupants (excluding current reservation)
                var occupiedAssets = LoadAllAssetsInRack(rack.Identifier);
                var otherReservations = LoadReservationsForRack(rack, reservation.Identifier);

                // Validate each range in the reservation
                foreach (var position in reservation.ReservedPositions)
                {
                    if (position.LowerBound == default || position.UpperBound == default)
                    {
                        continue;
                    }

                    int rangePosition = (int)position.LowerBound;
                    int rangeHeight = (int)(position.UpperBound - position.LowerBound + 1);

                    result.AddFailuresFrom(ValidateRangeOccupancy(
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

        #region Pipeline Construction

        private Validator<Rack> BuildValidationPipeline()
        {
            // Critical validations - stop on failure
            var criticalValidations = Validator<Rack>
                .Create(ValidateCriticalFields)
                .StopOnFailure();

            // Standard validations - collect all errors
            var standardValidations = Validator<Rack>
                .Create(ValidateDimensions)
                .AndThen(ValidatePowerCapacity);

            // Combine: critical first, then standard
            return criticalValidations.AndThen(standardValidations);
        }

        #endregion

        #region Validation Methods

        private ValidationResult ValidateCriticalFields(Rack rack)
        {
            var result = new ValidationResult();

            // Rack Units is critical
            if (rack.Capacity.RackUnitsField.Changed)
            {
                if (!RackValidationHandler.IsRackUnitCapacityValid(rack, out var unitsResult))
                {
                    result.AddFailuresFrom(unitsResult);
                }
            }

            return result;
        }

        private ValidationResult ValidateDimensions(Rack rack)
        {
            var result = new ValidationResult();

            if (rack.HeightField.Changed)
            {
                if (!RackValidationHandler.IsRackHeightValid(rack, out var heightResult))
                {
                    result.AddFailuresFrom(heightResult);
                }
            }

            if (rack.WidthField.Changed)
            {
                if (!RackValidationHandler.IsRackWidthValid(rack, out var widthResult))
                {
                    result.AddFailuresFrom(widthResult);
                }
            }

            if (rack.DepthField.Changed)
            {
                if (!RackValidationHandler.IsRackDepthValid(rack, out var depthResult))
                {
                    result.AddFailuresFrom(depthResult);
                }
            }

            return result;
        }

        private ValidationResult ValidatePowerCapacity(Rack rack)
        {
            var result = new ValidationResult();

            if (rack.Capacity.PowerCapacityField.Changed)
            {
                if (!RackValidationHandler.IsRackPowerCapacityValid(rack, out var powerResult))
                {
                    result.AddFailuresFrom(powerResult);
                }
            }

            return result;
        }

        #endregion

        #region Private Asset Validation Logic

        /// <summary>
        /// Validates a single asset in a specific rack.
        /// </summary>
        private ValidationResult ValidateAssetInRack(Asset asset, string rackIdentifier, int position, bool isDestination = false)
        {
            var result = new ValidationResult();

            try
            {
                var rack = _entityLoader.LoadRack(rackIdentifier);
                if (rack == null)
                {
                    result.AddFailReason(RackValidationField.Rack, "Rack not found.");
                    return result;
                }

                var heightU = GetAssetHeightU(asset);

                // Load all occupants (excluding current asset)
                var occupiedAssets = LoadAllAssetsInRack(rack.Identifier, asset.Identifier);
                var reservations = LoadReservationsForRack(rack);

                result.AddFailuresFrom(ValidateRangeOccupancy(
                    rack,
                    position,
                    heightU,
                    asset,
                    null, // No current reservation
                    occupiedAssets,
                    reservations));
            }
            catch (Exception ex)
            {
                result.AddFailReason(RackValidationField.RackSpacePosition,
                    $"Error validating asset placement: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Validates asset placement using pre-loaded context (for bulk validation).
        /// </summary>
        private ValidationResult ValidateAssetPlacementWithContext(Asset asset, RackValidationContext context)
        {
            var result = new ValidationResult();

            // Check Location rack placement
            if (asset.Location?.RackId != null && asset.Location.RackId != default && asset.Location.RackPosition != null)
            {
                result.AddFailuresFrom(ValidateRackPlacementWithContext(
                    asset,
                    asset.Location.RackId.ToString(),
                    asset.Location.RackPosition,
                    context));
            }

            // Check DestinationLocation rack placement
            if (asset.DestinationLocation?.RackId != null && asset.DestinationLocation.RackId != default && asset.DestinationLocation.RackPosition != null)
            {
                result.AddFailuresFrom(ValidateRackPlacementWithContext(
                    asset,
                    asset.DestinationLocation.RackId.ToString(),
                    asset.DestinationLocation.RackPosition,
                    context,
                    isDestination: true));
            }

            return result;
        }

        /// <summary>
        /// Validates specific rack placement using pre-loaded context.
        /// </summary>
        private ValidationResult ValidateRackPlacementWithContext(
            Asset asset,
            string rackIdentifier,
            long? rackPosition,
            RackValidationContext context,
            bool isDestination = false)
        {
            var result = new ValidationResult();

            if (rackPosition == null)
            {
                return result;
            }

            if (!context.LoadedRacks.TryGetValue(rackIdentifier, out var rack))
            {
                result.AddFailReason(RackValidationField.RackSpacePosition, "Rack not found.");
                return result;
            }

            var heightU = GetAssetHeightU(asset);

            // Build occupation list (existing assets + other assets being validated)
            var occupiedSpaces = BuildBulkOccupationList(rackIdentifier, asset, context, isDestination);

            // Load reservations for this rack
            var reservations = LoadReservationsForRack(rack);

            result.AddFailuresFrom(ValidateRangeOccupancy(
                rack,
                (int)rackPosition,
                heightU,
                asset,
                null, // No current reservation
                occupiedSpaces,
                reservations));

            return result;
        }

        /// <summary>
        /// Core validation logic - checks if a range is available in the rack.
        /// </summary>
        private ValidationResult ValidateRangeOccupancy(
            Rack rack,
            int position,
            int heightU,
            Asset currentAsset,
            InfraopsReservation currentReservation,
            List<(Asset Asset, int Position, int HeightU)> occupiedAssets,
            List<(InfraopsReservation Reservation, List<(long LowerBound, long UpperBound)> Ranges)> reservations)
        {
            var result = new ValidationResult();

            // Basic position validation
            if (!RackValidationHandler.ValidatePositionAndBounds(rack, position, heightU, out var boundsResult))
            {
                return boundsResult;
            }

            var (startPos, endPos) = RackValidationHandler.CalculateOccupiedRange(rack.Position, position, heightU);

            // Check asset conflicts
            if (!RackValidationHandler.CheckAssetConflicts(rack.Position, startPos, endPos, currentAsset, occupiedAssets, out var assetConflict))
            {
                return assetConflict;
            }

            // Check reservation conflicts
            if (!RackValidationHandler.CheckReservationConflicts(startPos, endPos, currentReservation, reservations, out var reservationConflict))
            {
                return reservationConflict;
            }

            return result;
        }

        #endregion

        #region Bulk Validation

        /// <summary>
        /// Builds validation context for bulk operations.
        /// Loads all affected racks and existing assets once.
        /// </summary>
        private RackValidationContext BuildRackValidationContext(List<Asset> assetsToValidate)
        {
            var context = new RackValidationContext
            {
                AssetsBeingValidated = assetsToValidate
            };

            // Get all unique rack identifiers from validation batch
            var rackIds = new HashSet<string>();
            foreach (var asset in assetsToValidate)
            {
                if (asset.Location?.RackId != null && asset.Location.RackId != default)
                {
                    rackIds.Add(asset.Location.RackId.ToString());
                }

                if (asset.DestinationLocation?.RackId != null && asset.DestinationLocation.RackId != default)
                {
                    rackIds.Add(asset.DestinationLocation.RackId.ToString());
                }
            }

            // Load all affected racks
            foreach (var rackId in rackIds)
            {
                var rack = _entityLoader.LoadRack(rackId);
                if (rack != null)
                {
                    context.LoadedRacks[rackId] = rack;
                }
            }

            // Get identifiers of assets being validated
            var validatedAssetIds = assetsToValidate.Select(a => a.Identifier).ToList();

            // Load existing assets in affected racks (excluding assets being validated)
            foreach (var rackId in rackIds)
            {
                var assetsInRack = _entityLoader.FindAssetsInRack(rackId, validatedAssetIds);
                context.ExistingAssetsInRacks[rackId] = assetsInRack;
            }

            return context;
        }

        /// <summary>
        /// Builds occupation list for bulk validation context.
        /// Combines existing assets (from DB) + other assets being validated.
        /// </summary>
        private List<(Asset Asset, int Position, int HeightU)> BuildBulkOccupationList(
            string rackIdentifier,
            Asset currentAsset,
            RackValidationContext context,
            bool isDestination)
        {
            var occupationList = new List<(Asset, int, int)>();

            // Add existing assets in rack (already filtered to exclude validation batch)
            if (context.ExistingAssetsInRacks.TryGetValue(rackIdentifier, out var existingAssets))
            {
                foreach (var existing in existingAssets)
                {
                    var position = isDestination ? existing.DestinationLocation?.RackPosition : existing.Location?.RackPosition;

                    if (position != null)
                    {
                        try
                        {
                            var heightU = GetAssetHeightU(existing);
                            occupationList.Add((existing, (int)position, heightU));
                        }
                        catch (InvalidOperationException)
                        {
                            // Skip assets with invalid height data
                            continue;
                        }
                    }
                }
            }

            // Add other assets being validated in the same rack (exclude current asset)
            foreach (var other in context.AssetsBeingValidated)
            {
                if (other.Identifier == currentAsset.Identifier)
                {
                    continue;
                }

                var rackId = isDestination ? other.DestinationLocation?.RackId : other.Location?.RackId;
                var position = isDestination ? other.DestinationLocation?.RackPosition : other.Location?.RackPosition;

                if (rackId?.ToString() == rackIdentifier && position != null)
                {
                    try
                    {
                        var heightU = GetAssetHeightU(other);
                        occupationList.Add((other, (int)position, heightU));
                    }
                    catch (InvalidOperationException)
                    {
                        // Skip - let individual validation catch this
                        continue;
                    }
                }
            }

            return occupationList;
        }

        #endregion

        #region Data Loading Helpers

        /// <summary>
        /// Loads all assets in a rack (excluding specified asset).
        /// </summary>
        private List<(Asset Asset, int Position, int HeightU)> LoadAllAssetsInRack(string rackIdentifier, string excludeAssetId = null)
        {
            var excludeIds = excludeAssetId != null ? new List<string> { excludeAssetId } : null;
            var assets = _entityLoader.FindAssetsInRack(rackIdentifier, excludeIds);

            var occupationList = new List<(Asset, int, int)>();

            foreach (var asset in assets.Where(a => a.Location?.RackPosition != null))
            {
                try
                {
                    var heightU = GetAssetHeightU(asset);
                    occupationList.Add((asset, (int)asset.Location.RackPosition, heightU));
                }
                catch (InvalidOperationException)
                {
                    // Skip assets with invalid height data
                    continue;
                }
            }

            return occupationList;
        }

        /// <summary>
        /// Loads all reservations for a specific rack (excluding specified reservation).
        /// </summary>
        private List<(InfraopsReservation Reservation, List<(long LowerBound, long UpperBound)> Ranges)> LoadReservationsForRack(
            Rack rack,
            string excludeReservationId = null)
        {
            var reservations = _entityLoader.FindReservationsInRack(rack);

            return reservations
                .Where(r => excludeReservationId == null || r.Identifier != excludeReservationId)
                .Select(r => (
                    Reservation: r,
                    Ranges: r.ReservedPositions?
                        .Where(p => p.LowerBound != default && p.UpperBound != default)
                        .Select(p => (p.LowerBound, p.UpperBound))
                        .ToList() ?? new List<(long, long)>()
                ))
                .ToList();
        }

        /// <summary>
        /// Gets the height in rack units for an asset.
        /// Throws exception if height cannot be determined.
        /// </summary>
        private int GetAssetHeightU(Asset asset)
        {
            if (asset?.AssetClassId == null || !asset.AssetClassId.HasValue())
            {
                throw new InvalidOperationException(
                    $"Cannot determine asset height: Asset '{asset?.Identifier}' does not have a valid AssetClass reference.");
            }

            var assetClass = _entityLoader.LoadAssetClass(asset.AssetClassId);

            if (assetClass == null)
            {
                throw new InvalidOperationException(
                    $"Cannot determine asset height: AssetClass '{asset.AssetClassId.Identifier}' not found.");
            }

            if (assetClass.HeightU <= 0)
            {
                throw new InvalidOperationException(
                    $"Cannot determine asset height: AssetClass '{assetClass.Name}' has invalid HeightU ({assetClass.HeightU}).");
            }

            return (int)assetClass.HeightU;
        }

        #endregion
    }
}