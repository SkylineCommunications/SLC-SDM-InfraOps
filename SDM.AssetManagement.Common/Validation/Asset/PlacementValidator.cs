namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SharedMappers.DomIds;
    using Skyline.DataMiner.SDM.AssetManagement.Common.Validation;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Common.Services;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM.InfraOps.Common.Validation;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;
    using static Skyline.DataMiner.SDM.AssetManagement.Common.Validation.AssetValidationHandler;
    using static Skyline.DataMiner.SDM.FacilityManagement.Validation.RackValidationHandler;

    /// <summary>
    /// Validates physical asset placement (both rack positions and parent asset holders).
    /// Optimized for bulk operations - loads parent assets and racks once.
    /// </summary>
    public class PlacementValidator
    {
        // Safety cap on parent chain walks; the hierarchy has no business depth limit.
        private const int MaxHierarchyDepth = 20;

        private readonly SdmEntityLoader _entityLoader;

        public PlacementValidator(SdmEntityLoader entityLoader)
        {
            _entityLoader = entityLoader ?? throw new ArgumentNullException(nameof(entityLoader));
        }

        /// <summary>
        /// Validates physical placement for multiple assets in bulk.
        /// Handles BOTH rack positions AND parent asset holder placements.
        /// Returns validation results in the same order as the input assets.
        /// Result at index i corresponds to asset at index i.
        /// </summary>
        public List<ValidationResult> ValidateBulkPlacements(List<Asset> assets)
        {
            if (assets == null || !assets.Any())
            {
                return new List<ValidationResult>();
            }

            // Initialize results - same order as input
            var results = assets.Select(a => new ValidationResult()).ToList();

            try
            {
                // Build context with all parent assets and racks
                var context = BuildPlacementContext(assets);

                // Validate each asset by index
                for (int i = 0; i < assets.Count; i++)
                {
                    var asset = assets[i];

                    // Validate parent asset holder placement
                    if (ShouldValidateHolder(asset))
                    {
                        results[i].AddFailuresFrom(ValidateParentHolderPlacement(asset, i, context));
                    }

                    // Validate rack placement
                    if (ShouldValidateRack(asset))
                    {
                        results[i].AddFailuresFrom(ValidateRackPlacement(asset, context));
                    }

                    // Validate destination rack placement
                    if (ShouldValidateDestinationRack(asset))
                    {
                        results[i].AddFailuresFrom(ValidateDestinationRackPlacement(asset, context));
                    }

                    // Validate destination parent holder placement
                    if (ShouldValidateDestinationHolder(asset))
                    {
                        results[i].AddFailuresFrom(ValidateDestinationParentHolderPlacement(asset, context));
                    }
                }
            }
            catch (Exception ex)
            {
                var globalError = new ValidationResult();
                globalError.AddFailReason(AssetValidationField.Asset,
                    $"Error validating placements: {ex.Message}");

                // Add error to all results
                for (int i = 0; i < results.Count; i++)
                {
                    results[i].AddFailuresFrom(globalError);
                }
            }

            return results;
        }

        // The Asset Class drives the Hierarchy Role and the height, so changing it re-triggers placement validation.
        // Side is included because it is only allowed under a Shelf parent.
        private static bool ShouldValidateHolder(Asset asset) =>
            asset.ShouldValidateAny(asset.Location.ParentAssetField, asset.Location.HolderNumberField, asset.Location.SideField, asset.AssetClassIdField);

        private static bool ShouldValidateRack(Asset asset) =>
            asset.ShouldValidateAny(asset.Location.RackIdField, asset.Location.RackPositionField, asset.Location.SideField, asset.AssetClassIdField);

        private static bool ShouldValidateDestinationHolder(Asset asset) =>
            asset.ShouldValidateAny(asset.DestinationLocation.ParentAssetField, asset.DestinationLocation.HolderNumberField, asset.DestinationLocation.SideField, asset.AssetClassIdField);

        private static bool ShouldValidateDestinationRack(Asset asset) =>
            asset.ShouldValidateAny(asset.DestinationLocation.RackIdField, asset.DestinationLocation.RackPositionField, asset.DestinationLocation.SideField, asset.AssetClassIdField);

        private static bool IsRackUnitConsumer(DeviceType deviceType) =>
            deviceType?.TagsInfo?.Tags?.Contains(SlcAsset_Management.Enums.TagOption.RackUnitConsumer) == true;

        /// <summary>
        /// Builds placement context - loads all affected parent assets and racks once.
        /// Only data needed by assets that actually require placement validation is loaded.
        /// </summary>
        //TODO: consider reviewing the bulk operations done on load. They are not fully bulk reads. And we are missing the fetch of reservations.
        private PlacementValidationContext BuildPlacementContext(List<Asset> assets)
        {
            var context = new PlacementValidationContext
            {
                AssetsBeingValidated = assets
            };

            foreach (var batchAsset in assets.Where(a => !String.IsNullOrWhiteSpace(a.Identifier)))
            {
                context.BatchAssetsById[batchAsset.Identifier] = batchAsset;
            }

            var assetIds = assets.Where(a => !String.IsNullOrWhiteSpace(a.Identifier)).Select(a => a.Identifier).ToList();

            CollectParentAssetIds(assets, out var parentAssetIds, out var destinationParentAssetIds);
            LoadParentAssets(context, parentAssetIds, destinationParentAssetIds, assetIds);
            LoadRacks(context, CollectRackIds(assets), assetIds);

            return context;
        }

        /// <summary>
        /// Collects all unique parent asset IDs of the current location and of the destination.
        /// </summary>
        private static void CollectParentAssetIds(
            List<Asset> assets,
            out HashSet<SdmObjectReference<Asset>> parentAssetIds,
            out HashSet<SdmObjectReference<Asset>> destinationParentAssetIds)
        {
            parentAssetIds = new HashSet<SdmObjectReference<Asset>>();
            destinationParentAssetIds = new HashSet<SdmObjectReference<Asset>>();

            foreach (var asset in assets)
            {
                if (asset.Location.ParentAsset.HasValue() && ShouldValidateHolder(asset))
                {
                    parentAssetIds.Add(asset.Location.ParentAsset);
                }

                if (asset.DestinationLocation.ParentAsset.HasValue() && ShouldValidateDestinationHolder(asset))
                {
                    destinationParentAssetIds.Add(asset.DestinationLocation.ParentAsset);
                }
            }
        }

        private void LoadParentAssets(
            PlacementValidationContext context,
            HashSet<SdmObjectReference<Asset>> parentAssetIds,
            HashSet<SdmObjectReference<Asset>> destinationParentAssetIds,
            List<string> assetIds)
        {
            foreach (var parentId in parentAssetIds.Union(destinationParentAssetIds))
            {
                var parent = _entityLoader.LoadAsset(parentId);
                if (parent == null)
                {
                    continue;
                }

                context.LoadedParentAssets[parentId] = parent;

                // Load existing children (excluding batch); the destination does not check occupancy
                if (parentAssetIds.Contains(parentId))
                {
                    context.ExistingChildrenInParents[parentId] = _entityLoader.FindChildAssets(parentId, assetIds);
                }
            }
        }

        /// <summary>
        /// Collects all unique rack IDs of the current location and of the destination.
        /// </summary>
        private static HashSet<string> CollectRackIds(List<Asset> assets)
        {
            var rackIds = new HashSet<string>();

            foreach (var asset in assets)
            {
                if (asset.Location.RackId.HasValue() && ShouldValidateRack(asset))
                {
                    rackIds.Add(asset.Location.RackId.Identifier);
                }

                if (asset.DestinationLocation.RackId.HasValue() && ShouldValidateDestinationRack(asset))
                {
                    rackIds.Add(asset.DestinationLocation.RackId.Identifier);
                }
            }

            return rackIds;
        }

        private void LoadRacks(PlacementValidationContext context, HashSet<string> rackIds, List<string> assetIds)
        {
            foreach (var rackId in rackIds)
            {
                var rack = _entityLoader.LoadRack(rackId);
                if (rack == null)
                {
                    throw new InvalidOperationException($"Rack with ID '{rackId}' not found during context building.");
                }

                context.LoadedRacks[rackId] = rack;

                // Load existing assets in rack (excluding batch)
                context.ExistingAssetsInRacks[rackId] = _entityLoader.FindAssetsInRack(rackId, assetIds);
            }
        }

        /// <summary>
        /// Validates parent asset holder placement using pre-loaded context.
        /// </summary>
        private ValidationResult ValidateParentHolderPlacement(Asset asset, int assetIndex, PlacementValidationContext context)
        {
            var result = new ValidationResult();

            if (!asset.Location.ParentAsset.HasValue() || asset.Location.HolderNumber == null)
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

            if (!IsParentChainValid(asset, parentId, AssetValidationField.ParentAsset, context, result))
            {
                return result;
            }

            var isShelfParent = IsShelf(parentAsset, context);
            if (!AreShelfRulesValid(asset, parentAsset, isShelfParent, asset.Location.Side, AssetValidationField.Side, context, result))
            {
                return result;
            }

            // Get AssetClass and DeviceType
            var hierarchyRole = ResolveHierarchyRole(asset, context);

            // A Shelf holds any device, with or without a Hierarchy Role.
            if (hierarchyRole == null && !isShelfParent)
            {
                result.AddFailReason(AssetValidationField.AssetClass,
                    "Asset Class Device Type must have a Hierarchy Role to be attached to a parent asset.");
                return result;
            }

            if (!HasMatchingHolder(parentAsset, holderNumber, hierarchyRole, isShelfParent))
            {
                result.AddFailReason(AssetValidationField.HolderNumber, isShelfParent
                    ? $"Invalid Holder Number: Parent Shelf does not have a holder slot '{holderNumber}'."
                    : $"Invalid Holder Number: Parent Asset does not have a holder slot '{holderNumber}' for Hierarchy Role '{hierarchyRole}'.");
                return result;
            }

            if (IsHolderOccupiedByExistingChild(parentId, holderNumber, hierarchyRole, isShelfParent, context))
            {
                result.AddFailReason(AssetValidationField.HolderNumber,
                    $"Holder Number '{holderNumber}' is already occupied on the Parent Asset by another asset.");
                return result;
            }

            if (HasBatchHolderConflict(assetIndex, parentId, holderNumber, hierarchyRole, isShelfParent, context))
            {
                result.AddFailReason(AssetValidationField.HolderNumber,
                    $"Holder Number '{holderNumber}' conflicts with another asset in the same batch.");
            }

            return result;
        }

        // Shelf holders match on slot number only; other parents also match on Hierarchy Role.
        private static bool HasMatchingHolder(
            Asset parentAsset,
            long? holderNumber,
            SlcAsset_Management.Enums.HierarchyRoleEnum? hierarchyRole,
            bool isShelfParent)
        {
            return parentAsset.Holders?
                .Any(h => h.SlotNumber == holderNumber && (isShelfParent || h.HierarchyRole == hierarchyRole)) == true;
        }

        // Existing children (from DB, excluding batch): same number AND same Hierarchy Role (number only for a Shelf)
        private bool IsHolderOccupiedByExistingChild(
            string parentId,
            long? holderNumber,
            SlcAsset_Management.Enums.HierarchyRoleEnum? hierarchyRole,
            bool isShelfParent,
            PlacementValidationContext context)
        {
            return context.ExistingChildrenInParents.TryGetValue(parentId, out var existingChildren)
                && existingChildren.Any(c => OccupiesHolder(c, holderNumber, hierarchyRole, isShelfParent, context));
        }

        // Conflicts within the batch (excluding the asset itself): same parent, same number AND same Hierarchy Role (number only for a Shelf)
        private bool HasBatchHolderConflict(
            int assetIndex,
            string parentId,
            long? holderNumber,
            SlcAsset_Management.Enums.HierarchyRoleEnum? hierarchyRole,
            bool isShelfParent,
            PlacementValidationContext context)
        {
            return context.AssetsBeingValidated
                .Where((other, i) => i != assetIndex
                    && other.Location.ParentAsset.HasValue()
                    && other.Location.ParentAsset.Identifier == parentId)
                .Any(other => OccupiesHolder(other, holderNumber, hierarchyRole, isShelfParent, context));
        }

        private bool OccupiesHolder(
            Asset asset,
            long? holderNumber,
            SlcAsset_Management.Enums.HierarchyRoleEnum? hierarchyRole,
            bool isShelfParent,
            PlacementValidationContext context)
        {
            return asset.Location.HolderNumber == holderNumber
                && (isShelfParent || ResolveHierarchyRole(asset, context) == hierarchyRole);
        }

        /// <summary>
        /// Validates the rules that depend on the parent being a Shelf: a Side is only allowed under a Shelf,
        /// and a child cannot be taller than its Shelf when both heights are known.
        /// </summary>
        private bool AreShelfRulesValid(
            Asset asset,
            Asset parentAsset,
            bool isShelfParent,
            SlcAsset_Management.Enums.SideEnum? side,
            AssetValidationField sideField,
            PlacementValidationContext context,
            ValidationResult result)
        {
            if (!isShelfParent)
            {
                if (side != null)
                {
                    result.AddFailReason(sideField, "Rack Side can only be set under a Parent Asset when the parent is a Shelf.");
                    return false;
                }

                return true;
            }

            var childHeight = ResolveClassInfo(asset, context).AssetClass?.HeightU;
            var shelfHeight = ResolveClassInfo(parentAsset, context).AssetClass?.HeightU;
            if (childHeight > 0 && shelfHeight > 0 && childHeight > shelfHeight)
            {
                result.AddFailReason(AssetValidationField.AssetClass,
                    $"Asset Class Height ({childHeight}U) cannot exceed the Height of the parent Shelf ({shelfHeight}U).");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Walks up the parent chain from <paramref name="parentId"/>. Fails when the chain reaches the asset itself
        /// (a cycle), loops in existing data, or exceeds <see cref="MaxHierarchyDepth"/> levels.
        /// Batch versions are preferred so moves within the same batch are taken into account.
        /// </summary>
        private bool IsParentChainValid(
            Asset asset,
            string parentId,
            AssetValidationField field,
            PlacementValidationContext context,
            ValidationResult result)
        {
            var hasIdentifier = !String.IsNullOrWhiteSpace(asset.Identifier);
            var visited = new HashSet<string>();
            var currentId = parentId;

            for (int depth = 0; depth < MaxHierarchyDepth; depth++)
            {
                if (hasIdentifier && currentId == asset.Identifier)
                {
                    result.AddFailReason(field, "Parent Asset cannot be the asset itself or one of its descendants.");
                    return false;
                }

                if (!visited.Add(currentId))
                {
                    result.AddFailReason(field, "The parent chain of the Parent Asset contains a cycle. Please check the asset hierarchy data.");
                    return false;
                }

                var current = GetChainAsset(currentId, context);
                if (current == null || !current.Location.ParentAsset.HasValue())
                {
                    return true;
                }

                currentId = current.Location.ParentAsset.Identifier;
            }

            result.AddFailReason(field, $"The parent chain of the Parent Asset is deeper than {MaxHierarchyDepth} levels. Please check the asset hierarchy data.");
            return false;
        }

        private Asset GetChainAsset(string assetId, PlacementValidationContext context)
        {
            if (context.BatchAssetsById.TryGetValue(assetId, out var batchAsset))
            {
                return batchAsset;
            }

            if (context.LoadedParentAssets.TryGetValue(assetId, out var parentAsset))
            {
                return parentAsset;
            }

            if (!context.ChainAssets.TryGetValue(assetId, out var chainAsset))
            {
                chainAsset = _entityLoader.LoadAsset(new SdmObjectReference<Asset>(assetId));
                context.ChainAssets[assetId] = chainAsset;
            }

            return chainAsset;
        }

        /// <summary>
        /// Validates that the destination parent asset exposes a holder for the destination HolderNumber and the
        /// asset's Hierarchy Role. The destination is a future location, so occupancy is not evaluated.
        /// </summary>
        private ValidationResult ValidateDestinationParentHolderPlacement(Asset asset, PlacementValidationContext context)
        {
            var result = new ValidationResult();
            var destination = asset.DestinationLocation;

            if (destination.IsEmpty || !destination.ParentAsset.HasValue() || destination.HolderNumber == null)
            {
                return result;
            }

            if (!context.LoadedParentAssets.TryGetValue(destination.ParentAsset.Identifier, out var parentAsset))
            {
                result.AddFailReason(AssetValidationField.DestinationParentAsset, "Destination Parent Asset not found.");
                return result;
            }

            if (!IsParentChainValid(asset, destination.ParentAsset.Identifier, AssetValidationField.DestinationParentAsset, context, result))
            {
                return result;
            }

            var isShelfParent = IsShelf(parentAsset, context);
            if (!AreShelfRulesValid(asset, parentAsset, isShelfParent, destination.Side, AssetValidationField.DestinationSide, context, result))
            {
                return result;
            }

            var hierarchyRole = ResolveHierarchyRole(asset, context);
            if (hierarchyRole == null && !isShelfParent)
            {
                result.AddFailReason(AssetValidationField.AssetClass,
                    "Asset Class Device Type must have a Hierarchy Role to be attached to a parent asset.");
                return result;
            }

            var holderNumber = destination.HolderNumber;
            if (!HasMatchingHolder(parentAsset, holderNumber, hierarchyRole, isShelfParent))
            {
                result.AddFailReason(AssetValidationField.DestinationHolderNumber, isShelfParent
                    ? $"Invalid Destination Holder Number: Destination Parent Shelf does not have a holder slot '{holderNumber}'."
                    : $"Invalid Destination Holder Number: Destination Parent Asset does not have a holder slot '{holderNumber}' for Hierarchy Role '{hierarchyRole}'.");
            }

            return result;
        }

        private bool IsShelf(Asset asset, PlacementValidationContext context) =>
            ResolveClassInfo(asset, context).DeviceType?.HierarchyInfo.HierarchyRole == SlcAsset_Management.Enums.HierarchyRoleEnum.Shelf;

        /// <summary>
        /// Resolves the Asset Class and its Device Type of an asset, cached per Asset Class.
        /// </summary>
        private (AssetClass AssetClass, DeviceType DeviceType) ResolveClassInfo(Asset asset, PlacementValidationContext context)
        {
            if (!asset.AssetClassId.HasValue())
            {
                return (null, null);
            }

            var assetClassId = asset.AssetClassId.Identifier;
            if (!context.ClassInfoByAssetClass.TryGetValue(assetClassId, out var info))
            {
                var assetClass = _entityLoader.LoadAssetClass(asset.AssetClassId);
                var deviceType = assetClass == null ? null : _entityLoader.LoadDeviceType(assetClass.DeviceTypeId);
                info = (assetClass, deviceType);
                context.ClassInfoByAssetClass[assetClassId] = info;
            }

            return info;
        }

        /// <summary>
        /// Resolves the Hierarchy Role of an asset through its Asset Class Device Type, cached per Asset Class.
        /// </summary>
        private SlcAsset_Management.Enums.HierarchyRoleEnum? ResolveHierarchyRole(Asset asset, PlacementValidationContext context)
        {
            var role = ResolveClassInfo(asset, context).DeviceType?.HierarchyInfo.HierarchyRole;

            // "None" means the Device Type cannot occupy a holder, same as no role.
            return role == SlcAsset_Management.Enums.HierarchyRoleEnum.None ? null : role;
        }

        /// <summary>
        /// Validates rack placement using pre-loaded context.
        /// </summary>
        private ValidationResult ValidateRackPlacement(Asset asset, PlacementValidationContext context)
        {
            var result = new ValidationResult();

            if (asset.Location.IsEmpty || !asset.Location.RackId.HasValue() || asset.Location.RackPosition == null)
            {
                return result;
            }

            var rackId = asset.Location.RackId.Identifier.ToString();

            // Get rack from context
            if (!context.LoadedRacks.TryGetValue(rackId, out var rack))
            {
                result.AddFailReason(AssetValidationField.RackId, "Rack not found.");
                return result;
            }

            // Validate using RackValidationHandler
            var assetClass = _entityLoader.LoadAssetClass(asset.AssetClassId);
            var deviceType = assetClass == null ? null : _entityLoader.LoadDeviceType(assetClass.DeviceTypeId);
            if (assetClass != null && IsRackUnitConsumer(deviceType) && assetClass.HeightU > 0)
            {
                // Build occupation list (existing assets + batch assets)
                var occupiedAssets = BuildRackOccupationList(rackId, asset, context);
                var reservations = LoadReservationsForRack(rack);

                if (!RackPlacementValidation.IsAssetPlacementValid(
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

        /// <summary>
        /// Validates the destination rack position against the rack boundaries using pre-loaded context.
        /// Only bounds are checked: the destination is a future location, so occupancy is not evaluated.
        /// </summary>
        private ValidationResult ValidateDestinationRackPlacement(Asset asset, PlacementValidationContext context)
        {
            var result = new ValidationResult();
            var destination = asset.DestinationLocation;

            if (destination.IsEmpty || !destination.RackId.HasValue() || destination.RackPosition == null)
            {
                return result;
            }

            if (!context.LoadedRacks.TryGetValue(destination.RackId.Identifier, out var rack))
            {
                result.AddFailReason(AssetValidationField.DestinationRackId, "Destination Rack not found.");
                return result;
            }

            var assetClass = _entityLoader.LoadAssetClass(asset.AssetClassId);
            var deviceType = assetClass == null ? null : _entityLoader.LoadDeviceType(assetClass.DeviceTypeId);
            if (assetClass != null && IsRackUnitConsumer(deviceType) && assetClass.HeightU > 0
                && !RackPlacementValidation.ValidatePositionAndBounds(rack, (int)destination.RackPosition, (int)assetClass.HeightU, out var boundsResult))
            {
                result.AddFailReason(AssetValidationField.DestinationRackPosition, boundsResult.GetFailReason(RackValidationField.RackSpacePosition));
            }

            return result;
        }

        /// <summary>
        /// Builds rack occupation list for validation.
        /// Uses index instead of identifier to avoid null/empty identifier issues.
        /// </summary>
        private List<(Asset, int, int)> BuildRackOccupationList(
            string rackId,
            Asset asset,
            PlacementValidationContext context)
        {
            var list = new List<(Asset, int, int)>();

            if (context.ExistingAssetsInRacks.TryGetValue(rackId, out var existing))
            {
                foreach (var a in existing)
                {
                    TryAddToList(list, a, a.Location.RackPosition);
                }
            }

            foreach (var other in context.AssetsBeingValidated)
            {
                if (other.Identifier == asset.Identifier) continue;
                    
                if (other.Location.RackId.Identifier == rackId)
                {
                    TryAddToList(list, other, other.Location.RackPosition);
                }
            }

            return list;
        }

        private void TryAddToList(List<(Asset, int, int)> list, Asset a, long? position)
        {
            if (position == null) return;
            var ac = _entityLoader.LoadAssetClass(a.AssetClassId);
            var deviceType = ac == null ? null : _entityLoader.LoadDeviceType(ac.DeviceTypeId);
            if (ac != null && IsRackUnitConsumer(deviceType) && ac.HeightU > 0)
                list.Add((a, (int)position.Value, (int)ac.HeightU));
        }

        private List<(InfraopsReservation, List<(long, long)>)> LoadReservationsForRack(Rack rack)
        {
            var reservations = _entityLoader.FindReservationsInRack(rack);

            return reservations
                .Select(r => (
                    r,
                    r.ReservedPositions?
                        .Where(p => p.LowerBound != default && p.UpperBound != default)
                        .Select(p => (p.LowerBound.Value, p.UpperBound.Value))
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
        public Dictionary<string, (AssetClass AssetClass, DeviceType DeviceType)> ClassInfoByAssetClass { get; set; } = new Dictionary<string, (AssetClass AssetClass, DeviceType DeviceType)>();
        public Dictionary<string, Asset> BatchAssetsById { get; set; } = new Dictionary<string, Asset>();
        public Dictionary<string, Asset> ChainAssets { get; set; } = new Dictionary<string, Asset>();
    }
}