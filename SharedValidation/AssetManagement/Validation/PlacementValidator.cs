namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SharedCommonLibrary.AssetManagement.Models;

    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Common.Services;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM.FacilityManagement.Validation;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    using static Skyline.DataMiner.SDM.AssetManagement.Common.Validation.AssetValidationHandler;

    /// <summary>
    /// Validates physical asset placement (both rack positions and parent asset holders).
    /// Optimized for bulk operations - loads parent assets and racks once.
    /// </summary>
    public class PlacementValidator
    {
        private readonly SdmEntityLoader _entityLoader;

        public PlacementValidator(SdmEntityLoader entityLoader)
        {
            _entityLoader = entityLoader ?? throw new ArgumentNullException(nameof(entityLoader));
        }

        /// <summary>
        /// Validates physical placement for multiple assets in bulk.
        /// Handles BOTH rack positions AND parent asset holder placements.
        /// </summary>
        public Dictionary<string, ValidationResult> ValidateBulkPlacements(List<Asset> assets)
        {
            var results = new Dictionary<string, ValidationResult>();

            if (assets == null || !assets.Any())
            {
                return results;
            }

            try
            {
                // Build context with all parent assets and racks
                var context = BuildPlacementContext(assets);

                // Validate each asset
                foreach (var asset in assets)
                {
                    var result = new ValidationResult();

                    // Validate parent asset holder placement
                    result.AddFailuresFrom(ValidateParentHolderPlacement(asset, context));

                    // Validate rack placement
                    result.AddFailuresFrom(ValidateRackPlacement(asset, context));

                    // todo Validate destination parent holder placement

                    results[asset.Identifier] = result;
                }
            }
            catch (Exception ex)
            {
                var globalError = new ValidationResult();
                globalError.AddFailReason(AssetValidationField.Asset,
                    $"Error validating placements: {ex.Message}");

                foreach (var asset in assets)
                {
                    results[asset.Identifier] = globalError;
                }
            }

            return results;
        }

        /// <summary>
        /// Builds placement context - loads all affected parent assets and racks once.
        /// </summary>
        private PlacementValidationContext BuildPlacementContext(List<Asset> assets)
        {
            var context = new PlacementValidationContext
            {
                AssetsBeingValidated = assets
            };

            var assetIds = assets.Select(a => a.Identifier).ToList();

            // Collect all unique parent asset IDs
            var parentAssetIds = new HashSet<SdmObjectReference<Asset>>();
            foreach (var asset in assets)
            {
                if (asset.Location?.ParentAsset.HasValue() == true)
                {
                    parentAssetIds.Add(asset.Location.ParentAsset);
                }

                //todo handle destination parent assets when validating 
            }

            // Load all parent assets
            foreach (var parentId in parentAssetIds)
            {
                var parent = _entityLoader.LoadAsset(parentId);
                if (parent != null)
                {
                    context.LoadedParentAssets[parentId] = parent;

                    // Load existing children (excluding batch)
                    var children = _entityLoader.FindChildAssets(parentId, assetIds);
                    context.ExistingChildrenInParents[parentId] = children;
                }
            }

            // Collect all unique rack IDs
            var rackIds = new HashSet<string>();
            foreach (var asset in assets)
            {
                if (asset.Location?.RackId != default)
                {
                    rackIds.Add(asset.Location.RackId.ToString());
                }

                if (asset.DestinationLocation?.RackId != default)
                {
                    rackIds.Add(asset.DestinationLocation.RackId.ToString());
                }
            }

            // Load all racks
            foreach (var rackId in rackIds)
            {
                var rack = _entityLoader.LoadRack(rackId);
                if (rack != null)
                {
                    context.LoadedRacks[rackId] = rack;

                    // Load existing assets in rack (excluding batch)
                    var assetsInRack = _entityLoader.FindAssetsInRack(rackId, assetIds);
                    context.ExistingAssetsInRacks[rackId] = assetsInRack;
                }
            }

            return context;
        }

        /// <summary>
        /// Validates parent asset holder placement using pre-loaded context.
        /// </summary>
        private ValidationResult ValidateParentHolderPlacement(Asset asset, PlacementValidationContext context)
        {
            var result = new ValidationResult();

            if (!asset.Location?.ParentAsset.HasValue() == true || asset.Location?.HolderNumber == null)
            {
                return result;
            }

            var parentId = asset.Location.ParentAsset.Identifier;
            var holderNumber = asset.Location.HolderNumber;

            // Get parent from context
            if (!context.LoadedParentAssets.TryGetValue(parentId, out var parentAsset))
            {
                result.AddFailReason(AssetValidationField.ParentAsset, "Parent Asset not found.");
                return result;
            }

            // Get AssetClass and DeviceType
            var assetClass = _entityLoader.LoadAssetClass(asset.AssetClassId);
            var deviceType = _entityLoader.LoadDeviceType(assetClass.DeviceTypeId);

            if (deviceType?.HierarchyInfo?.HierarchyRole == null)
            {
                result.AddFailReason(AssetValidationField.AssetClass,
                    "Asset Class Device Type must have a Hierarchy Role to be attached to a parent asset.");
                return result;
            }

            var hierarchyRole = deviceType.HierarchyInfo.HierarchyRole;

            // Check if holder slot exists
            var matchingHolder = parentAsset.Holders?
                .FirstOrDefault(h => h.SlotNumber == holderNumber && h.HierarchyRole == hierarchyRole);

            if (matchingHolder == null)
            {
                result.AddFailReason(AssetValidationField.HolderNumber,
                    $"Invalid Holder Number: Parent Asset does not have a holder slot '{holderNumber}' for Hierarchy Role '{hierarchyRole}'.");
                return result;
            }

            // Check existing children (from DB, excluding batch)
            if (context.ExistingChildrenInParents.TryGetValue(parentId, out var existingChildren))
            {
                var occupied = existingChildren
                    .FirstOrDefault(c => c.Location?.HolderNumber == holderNumber);

                if (occupied != null)
                {
                    result.AddFailReason(AssetValidationField.HolderNumber,
                        $"Holder Number '{holderNumber}' is already occupied on the Parent Asset by another asset.");
                }
            }

            // Check other assets being validated (batch conflicts already checked in Phase 1)

            return result;
        }

        /// <summary>
        /// Validates rack placement using pre-loaded context.
        /// </summary>
        private ValidationResult ValidateRackPlacement(Asset asset, PlacementValidationContext context)
        {
            var result = new ValidationResult();

            if (asset.Location?.RackId == default || asset.Location?.RackPosition == null)
            {
                return result;
            }

            var rackId = asset.Location.RackId.ToString();

            // Get rack from context
            if (!context.LoadedRacks.TryGetValue(rackId, out var rack))
            {
                result.AddFailReason(AssetValidationField.RackId, "Rack not found.");
                return result;
            }

            // Use RackValidator logic for space validation
            var rackValidator = new RackValidator(_entityLoader);

            // Build occupation list (existing assets + batch assets)
            var occupiedAssets = BuildRackOccupationList(rackId, asset, context, isDestination: false);

            // Validate using RackValidationHandler
            var assetClass = _entityLoader.LoadAssetClass(asset.AssetClassId);
            if (assetClass != null && assetClass.HeightU > 0)
            {
                var reservations = LoadReservationsForRack(rack);

                if (!RackValidationHandler.IsAssetPlacementValid(
                    rack,
                    (int)asset.Location.RackPosition,
                    (int)assetClass.HeightU,
                    asset,
                    occupiedAssets,
                    reservations,
                    out var spaceResult))
                {
                    result.AddFailuresFrom(spaceResult);
                }
            }

            return result;
        }

        // Similar methods for ValidateDestinationParentHolderPlacement and ValidateDestinationRackPlacement...

        private List<(Asset, int, int)> BuildRackOccupationList(
            string rackId,
            Asset currentAsset,
            PlacementValidationContext context,
            bool isDestination)
        {
            var list = new List<(Asset, int, int)>();

            // Add existing assets from DB (already excludes batch)
            if (context.ExistingAssetsInRacks.TryGetValue(rackId, out var existing))
            {
                foreach (var a in existing)
                {
                    var pos = isDestination ? a.DestinationLocation?.RackPosition : a.Location?.RackPosition;
                    if (pos != null)
                    {
                        var ac = _entityLoader.LoadAssetClass(a.AssetClassId);
                        if (ac != null && ac.HeightU > 0)
                        {
                            list.Add((a, (int)pos.Value, (int)ac.HeightU));
                        }
                    }
                }
            }

            // Add other assets from batch (exclude current)
            foreach (var other in context.AssetsBeingValidated)
            {
                if (other.Identifier == currentAsset.Identifier) continue;

                var otherRackId = isDestination ? other.DestinationLocation?.RackId : other.Location?.RackId;
                var pos = isDestination ? other.DestinationLocation?.RackPosition : other.Location?.RackPosition;

                if (otherRackId?.ToString() == rackId && pos != null)
                {
                    var ac = _entityLoader.LoadAssetClass(other.AssetClassId);
                    if (ac != null && ac.HeightU > 0)
                    {
                        list.Add((other, (int)pos.Value, (int)ac.HeightU));
                    }
                }
            }

            return list;
        }

        private List<(InfraopsReservation, List<(long, long)>)> LoadReservationsForRack(Rack rack)
        {
            var reservations = _entityLoader.FindReservationsInRack(rack);

            return reservations
                .Select(r => (
                    r,
                    r.ReservedPositions?
                        .Where(p => p.LowerBound != default && p.UpperBound != default)
                        .Select(p => (p.LowerBound, p.UpperBound))
                        .ToList() ?? new List<(long, long)>()
                ))
                .ToList();
        }
    }

    /// <summary>
    /// Context for bulk placement validation.
    /// </summary>
    public class PlacementValidationContext
    {
        public List<Asset> AssetsBeingValidated { get; set; } = new List<Asset>();
        public Dictionary<string, Asset> LoadedParentAssets { get; set; } = new Dictionary<string, Asset>();
        public Dictionary<string, List<Asset>> ExistingChildrenInParents { get; set; } = new Dictionary<string, List<Asset>>();
        public Dictionary<string, Rack> LoadedRacks { get; set; } = new Dictionary<string, Rack>();
        public Dictionary<string, List<Asset>> ExistingAssetsInRacks { get; set; } = new Dictionary<string, List<Asset>>();
    }
}