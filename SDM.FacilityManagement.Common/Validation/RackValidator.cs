namespace Skyline.DataMiner.SDM.FacilityManagement.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.AssetManagement.Repositories;
    using Skyline.DataMiner.SDM.Common.Services;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM.FacilityManagement.Repositories;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;


    /// <summary>
    /// Public validator service for Rack validation with comprehensive error handling.
    /// </summary>
    public class RackValidator
    {
        private readonly IRackQueryRepository _rackRepository;
        private readonly IAssetQueryRepository _assetRepository;
        private readonly SdmEntityLoader _entityLoader;
        private readonly Validator<Rack> _validationPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="RackValidator"/> class.
        /// </summary>
        /// <param name="rackRepository">Repository for querying racks.</param>
        /// <param name="assetRepository">Repository for querying assets (for rack space validation).</param>
        /// <param name="entityLoader">Shared entity loader service.</param>
        public RackValidator(
            IRackQueryRepository rackRepository,
            IAssetQueryRepository assetRepository,
            SdmEntityLoader entityLoader)
        {
            _rackRepository = rackRepository ?? throw new ArgumentNullException(nameof(rackRepository));
            _assetRepository = assetRepository ?? throw new ArgumentNullException(nameof(assetRepository));
            _entityLoader = entityLoader ?? throw new ArgumentNullException(nameof(entityLoader));

            _validationPipeline = BuildValidationPipeline();
        }

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

        /// <summary>
        /// Validates if an asset can be placed at the specified position in the rack.
        /// Queries all assets in the rack to check for space conflicts.
        /// </summary>
        public ValidationResult ValidateAssetPlacement(
            Rack rack,
            Asset asset,
            int position,
            SharedMappers.DomIds.SlcAsset_Management.Enums.SideEnum side,
            int heightU)
        {
            var result = new ValidationResult();

            if (rack == null)
            {
                result.AddFailReason(RackValidationHandler.RackValidationField.Rack, "Rack cannot be null.");
                return result;
            }

            if (asset == null)
            {
                result.AddFailReason(RackValidationHandler.RackValidationField.RackSpacePosition, "Asset cannot be null.");
                return result;
            }

            try
            {
                // Load all assets in this rack
                var assetsInRack = LoadAssetsInRack(rack.Identifier);

                // Build occupation list
                var occupiedSpaces = assetsInRack
                    .Where(a => a.Location?.RackPosition != null && a.Location?.Side != null)
                    .Select(a => (
                        Asset: a,
                        Position: (int)a.Location.RackPosition,
                        HeightU: GetAssetHeightU(a),
                        Side: a.Location.Side
                    ))
                    .ToList();

                // Validate using static handler
                return RackValidationHandler.IsRackSpaceAvailable(
                    Convert.ToInt64(rack.Capacity.MaximumRackCapacity),
                    rack.Position,
                    position,
                    heightU,
                    asset,
                    occupiedSpaces,
                    side,
                    out result) ? result : result;
            }
            catch (Exception ex)
            {
                result.AddFailReason(RackValidationField.RackSpacePosition,
                    $"Error validating rack space: {ex.Message}");
            }

            return result;
        }

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
            if (rack.RackInfo.RackUnitsField.Changed)
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

            if (rack.RackInfo.HeightField.Changed)
            {
                if (!RackValidationHandler.IsRackHeightValid(rack, out var heightResult))
                {
                    result.AddFailuresFrom(heightResult);
                }
            }

            if (rack.RackInfo.WidthField.Changed)
            {
                if (!RackValidationHandler.IsRackWidthValid(rack, out var widthResult))
                {
                    result.AddFailuresFrom(widthResult);
                }
            }

            if (rack.RackInfo.DepthField.Changed)
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

            if (rack.RackInfo.PowerCapacityField.Changed)
            {
                if (!RackValidationHandler.IsRackPowerCapacityValid(rack, out var powerResult))
                {
                    result.AddFailuresFrom(powerResult);
                }
            }

            return result;
        }

        #endregion

        #region Helper Methods

        private List<Asset> LoadAssetsInRack(string rackIdentifier)
        {
            if (_assetRepository == null || string.IsNullOrEmpty(rackIdentifier))
            {
                return new List<Asset>();
            }

            try
            {
                // Query all assets with this rack
                var filter = AssetExposers.Identifier.NotEqual(string.Empty); // Get all assets
                var allAssets = _assetRepository.Read(filter);

                // Filter in memory for assets in this rack
                return allAssets
                    .Where(a => a.Location?.RackId != null &&
                               a.Location.RackId.ToString() == rackIdentifier)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load assets in rack: {ex.Message}", ex);
            }
        }

        private int GetAssetHeightU(Asset asset)
        {
            if (asset?.AssetClassId == null || !asset.AssetClassId.HasValue())
            {
                return 1; // Default to 1U if cannot determine
            }

            try
            {
                var assetClass = _entityLoader.LoadAssetClass(asset.AssetClassId);
                return assetClass?.HeightU > 0 ? (int)assetClass.HeightU : 1;
            }
            catch
            {
                return 1; // Default to 1U on error
            }
        }

        #endregion
    }
}