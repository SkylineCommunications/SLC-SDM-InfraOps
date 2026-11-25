namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SharedMappers.DomIds;
    using Skyline.DataMiner.SDM.AssetManagement.Models;

    using Skyline.DataMiner.SDM.AssetManagement.Common.Exceptions;
    using Skyline.DataMiner.SDM.AssetManagement.Common.Validation;
    using Skyline.DataMiner.SDM.Common.Services;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Public validator service for Asset validation with comprehensive error handling.
    /// </summary>
    public class AssetValidator : ValidatorBase<Asset>
    {
        private readonly SdmEntityLoader _entityLoader;
        private readonly AssetValidationCore _validationCore;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetValidator"/> class.
        /// </summary>
        /// <param name="entityLoader">Shared entity loader service.</param>
        public AssetValidator(SdmEntityLoader entityLoader)
        {
            _entityLoader = entityLoader ?? throw new ArgumentNullException(nameof(entityLoader));
            _validationCore = new AssetValidationCore(_entityLoader);
        }


        /// <summary>
        /// Validates name uniqueness - used for real-time UI validation.
        /// </summary>
        public ValidationResult IsAssetNameValid(string name, string exceptIdentifier = null)
        {
            return _validationCore.ValidateNameUniqueness(name, exceptIdentifier);
        }

        /// <summary>
        /// Validates the uniqueness of the Asset name for the specified <see cref="Asset"/> instance.
        /// Excludes the current asset identifier from the uniqueness check.
        /// </summary>
        public ValidationResult IsAssetNameValid(Asset asset)
        {
            return _validationCore.ValidateNameUniqueness(asset.Name, asset.Identifier);
        }

        /// <summary>
        /// Validates asset ID uniqueness - used for real-time UI validation.
        /// </summary>
        public ValidationResult IsAssetIdValid(string assetId, string exceptIdentifier = null)
        {
            return _validationCore.ValidateAssetIdUniqueness(assetId, exceptIdentifier);
        }

        /// <summary>
        /// Validates all DataPorts associated with the Asset.
        /// </summary>
        public ValidationResult ValidateAssetDataPorts(Asset asset)
        {
            return _validationCore.ValidateDataPorts(asset);
        }

        /// <summary>
        /// Validates all PowerPorts associated with the Asset.
        /// </summary>
        public ValidationResult ValidateAssetPowerPorts(Asset asset)
        {
            return _validationCore.ValidatePowerPorts(asset);
        }


        protected override ValidationResult CreateNullEntityResult()
        {
            var result = new ValidationResult();
            result.AddFailReason(AssetValidationHandler.AssetValidationField.Asset, "Asset cannot be null.");
            return result;
        }

        protected override List<ValidationResult> ValidateBulkNoDatabaseBusinessRules(List<Asset> entities)
        {
            // ============================================================
            // PHASE 1: NO DATABASE ACCESS CHECKS (BUSINESS RULES)
            // ============================================================
            return entities.Select(_validationCore.ValidateWithoutDatabaseAccess).ToList();
        }

        protected override List<ValidationResult> ValidateBulkNoDatabaseBatchConflicts(List<Asset> entities)
        {
            // ============================================================
            // PHASE 2: IN-MEMORY BATCH CONFLICT DETECTION (NO DATABASE)
            // ============================================================
            return _validationCore.ValidateBatchConflicts(entities);
        }

        protected override List<ValidationResult> ValidateBulkWithDatabase(List<Asset> entities)
        {
            // Initialize results - same order as input
            var results = entities.Select(a => new ValidationResult()).ToList();

            // ============================================================
            // PHASE 2.5: BULK UNIQUENESS CHECKS AGAINST DATABASE
            // One OR query per field via Tools.RetrieveBigOrFilter.
            // All four fields checked; results merged before fast-fail.
            // ============================================================
            results.MergeFrom(_validationCore.ValidateBulkNameUniquenessAgainstDatabase(entities));
            results.MergeFrom(_validationCore.ValidateBulkAssetIdUniquenessAgainstDatabase(entities));
            results.MergeFrom(_validationCore.ValidateBulkSerialNumberUniquenessAgainstDatabase(entities));
            results.MergeFrom(_validationCore.ValidateBulkMacAddressUniquenessAgainstDatabase(entities));
            results.MergeFrom(_validationCore.ValidateBulkReferencesAgainstDatabase(entities));

            // Fast-fail if batch conflicts exist
            if (results.AnyInvalid())
            {
                return results;
            }

            // ============================================================
            // PHASE 4: BULK PLACEMENT VALIDATION (OPTIMIZED)
            // ============================================================
            var placementValidator = new PlacementValidator(_entityLoader);
            var placementResults = placementValidator.ValidateBulkPlacements(entities);
            results.MergeFrom(placementResults);

            return results;
        }

        protected override List<ValidationResult> ValidateBulkForDelete(List<Asset> assets)
        {
            if (assets == null || !assets.Any())
            {
                return new List<ValidationResult>();
            }

            return ValidateCanDelete(assets);
        }

        private List<ValidationResult> ValidateCanDelete(List<Asset> assets)
        {
            var results = assets.Select(_ => new ValidationResult()).ToList();

            for (int idx = 0; idx < assets.Count; idx++)
            {
                var state = assets[idx].State;
                if (state != SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable
                    && state != SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Disposed)
                {
                    results[idx].AddFailReason(
                        AssetValidationHandler.AssetValidationField.Asset,
                        "Asset must be in 'Not Available' or 'Disposed' State to Delete");
                }
            }

            // Children deleted in the same batch do not block their Shelf.
            var batchIds = new HashSet<string>(assets.Select(a => a.Identifier).Where(id => !string.IsNullOrWhiteSpace(id)));
            var shelvesWithChildren = GetShelvesWithChildren(assets, batchIds);

            for (int idx = 0; idx < assets.Count; idx++)
            {
                if (shelvesWithChildren.Contains(assets[idx].Identifier ?? string.Empty))
                {
                    results[idx].AddFailReason(
                        AssetValidationHandler.AssetValidationField.ParentAsset,
                        ShelfHasChildrenMessage);
                }
            }

            return results;
        }

        /// <summary>
        /// Validates a state transition without executing it: the state machine path, the state-entry
        /// requirements and the business rules (e.g. a Shelf that still holds child assets).
        /// Use it to check a transition up front, the same way Validate is used before an Update.
        /// </summary>
        public ValidationResult ValidateTransition(Asset asset, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum newState)
        {
            var result = AssetTransitionValidator.ValidatePath(asset, newState);
            if (!result.IsValid || asset.State == newState)
            {
                return result;
            }

            result.AddFrom(ValidateTransitionRules(asset, newState));
            return result;
        }

        internal void EnsureTransitionAllowed(Asset asset, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum newState)
        {
            var result = ValidateTransitionRules(asset, newState);
            if (!result.IsValid)
            {
                throw new AssetTransitionValidationException(result);
            }
        }

        private ValidationResult ValidateTransitionRules(Asset asset, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum newState)
        {
            var result = new ValidationResult();

            if (asset == null
                || (newState != SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable
                    && newState != SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Disposed))
            {
                return result;
            }

            if (GetShelvesWithChildren(new List<Asset> { asset }, new HashSet<string>()).Contains(asset.Identifier ?? string.Empty))
            {
                result.AddFailReason(AssetValidationHandler.AssetValidationField.Asset, ShelfHasChildrenMessage);
            }

            return result;
        }

        private const string ShelfHasChildrenMessage = "Shelf still holds child assets. Remove them first.";

        private HashSet<string> GetShelvesWithChildren(List<Asset> assets, HashSet<string> excludedChildIds)
        {
            var shelfIds = new HashSet<string>();

            var assetClassIds = assets
                .Where(a => a?.AssetClassId != null && a.AssetClassId.HasValue() && !string.IsNullOrWhiteSpace(a.Identifier))
                .Select(a => a.AssetClassId.Identifier)
                .Distinct()
                .ToList();

            if (!assetClassIds.Any())
            {
                return shelfIds;
            }

            var assetClasses = _entityLoader.GetAssetClassesByDomIds(assetClassIds);
            var deviceTypeIds = assetClasses
                .Where(ac => ac.DeviceTypeId != null && ac.DeviceTypeId.HasValue())
                .Select(ac => ac.DeviceTypeId.Identifier)
                .Distinct()
                .ToList();

            var shelfDeviceTypeIds = _entityLoader.GetDeviceTypesByDomIds(deviceTypeIds)
                .Where(dt => dt.HierarchyInfo?.HierarchyRole == SlcAsset_Management.Enums.HierarchyRoleEnum.Shelf)
                .Select(dt => dt.Identifier)
                .ToHashSet();

            var shelfAssetClassIds = assetClasses
                .Where(ac => ac.DeviceTypeId != null && ac.DeviceTypeId.HasValue() && shelfDeviceTypeIds.Contains(ac.DeviceTypeId.Identifier))
                .Select(ac => ac.Identifier)
                .ToHashSet();

            var shelfAssetIds = assets
                .Where(a => a?.AssetClassId != null && a.AssetClassId.HasValue() && shelfAssetClassIds.Contains(a.AssetClassId.Identifier))
                .Select(a => a.Identifier)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            if (!shelfAssetIds.Any())
            {
                return shelfIds;
            }

            foreach (var child in _entityLoader.GetChildAssetsByParentIds(shelfAssetIds))
            {
                if (!excludedChildIds.Contains(child.Identifier ?? string.Empty)
                    && child.Location?.ParentAsset != null
                    && child.Location.ParentAsset.HasValue())
                {
                    shelfIds.Add(child.Location.ParentAsset.Identifier);
                }
            }

            return shelfIds;
        }

    }
}