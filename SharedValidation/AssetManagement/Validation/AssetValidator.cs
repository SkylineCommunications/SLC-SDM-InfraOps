namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Remoting.Contexts;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.AssetManagement.Common.Validation;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.AssetManagement.Repositories;
    using Skyline.DataMiner.SDM.Common.Services;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;
    using Skyline.DataMiner.SDM.FacilityManagement.Validation;

    using static Skyline.DataMiner.SDM.AssetManagement.Common.Validation.AssetValidationHandler;

    /// <summary>
    /// Public validator service for Asset validation with comprehensive error handling.
    /// </summary>
    public class AssetValidator
    {
        private readonly SdmEntityLoader _entityLoader;
        private readonly Validator<Asset> _validationPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetValidator"/> class.
        /// </summary>
        /// <param name="assetRepository">Repository for querying assets.</param>
        /// <param name="entityLoader">Shared entity loader service.</param>
        public AssetValidator(IAssetQueryRepository assetRepository, SdmEntityLoader entityLoader)
        {
            _entityLoader = entityLoader ?? throw new ArgumentNullException(nameof(entityLoader));
            _validationPipeline = BuildValidationPipeline();
        }

        /// <summary>
        /// Validates an Asset and returns ValidationResult.
        /// Collects all errors without throwing exceptions.
        /// </summary>
        public ValidationResult Validate(Asset asset)
        {
            if (asset == null)
            {
                throw new ArgumentNullException(nameof(asset));
            }

            return _validationPipeline.Validate(asset);
        }

        /// <summary>
        /// Validates an Asset and throws ValidationException if invalid.
        /// Use this when you want fail-fast behavior.
        /// </summary>
        public void ValidateAndThrow(Asset asset)
        {
            _validationPipeline.ValidateAndThrow(asset);
        }

        /// <summary>
        /// Validates with custom error handling callback.
        /// </summary>
        public ValidationResult ValidateWithHandler(Asset asset, Action<ValidationResult> onError)
        {
            return _validationPipeline.ValidateWithHandler(asset, onError);
        }

        /// <summary>
        /// Validates name uniqueness - used for real-time UI validation.
        /// </summary>
        public ValidationResult IsAssetNameValid(string name, List<string> exceptIdentifiers = null)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(name))
            {
                result.AddFailReason(AssetValidationField.Name,
                    "Asset Name cannot be empty or whitespace.");
                return result;
            }

            if (IsNameInUse(name, exceptIdentifiers))
            {
                result.AddFailReason(AssetValidationField.Name,
                    $"Asset Name '{name}' is already in use.");
            }

            return result;
        }

        /// <summary>
        /// Validates the uniqueness of the Asset name for the specified <see cref="Asset"/> instance.
        /// Excludes the current asset identifier from the uniqueness check.
        /// </summary>
        /// <param name="asset">The asset to validate.</param>
        /// <returns>A <see cref="ValidationResult"/> indicating whether the asset name is valid.</returns>
        public ValidationResult IsAssetNameValid(Asset asset)
        {
            return IsAssetNameValid(asset.Name, new List<string> { asset.Identifier });
        }

        /// <summary>
        /// Validates asset ID uniqueness - used for real-time UI validation.
        /// </summary>
        public ValidationResult IsAssetIdValid(string assetId, List<string> exceptIdentifiers = null)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(assetId))
            {
                result.AddFailReason(AssetValidationField.AssetId,
                    "Asset ID cannot be empty or whitespace.");
                return result;
            }

            if (IsAssetIdInUse(assetId, exceptIdentifiers))
            {
                result.AddFailReason(AssetValidationField.AssetId,
                    $"Asset ID '{assetId}' is already in use.");
            }

            return result;
        }

        /// <summary>
        /// Validates all DataPorts associated with the Asset.
        /// Queries DataPort repository to load ports for this asset.
        /// </summary>
        /// <param name="asset">The asset whose data ports should be validated.</param>
        /// <returns>ValidationResult containing any port validation errors.</returns>
        public ValidationResult ValidateAssetDataPorts(Asset asset)
        {
            var result = new ValidationResult();

            if (asset == null)
            {
                result.AddFailReason(AssetValidationField.Asset, "Asset cannot be null.");
                return result;
            }

            try
            {
                // Query DataPorts for this Asset
                var dataPorts = _entityLoader.LoadDataPorts(asset);

                // Check for negative port numbers
                var negativeNumbers = dataPorts.Where(p => p.DataPortInfo.PortNumber < 0).ToList();
                if (negativeNumbers.Any())
                {
                    result.AddFailReason(AssetValidationField.DataPort,
                        $"Data Port numbers cannot be negative. Found: {string.Join(", ", negativeNumbers.Select(p => p.DataPortInfo.PortNumber))}");
                }

                // Check for duplicates
                var duplicates = dataPorts
                    .GroupBy(p => p.DataPortInfo.PortNumber)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicates.Any())
                {
                    result.AddFailReason(AssetValidationField.DataPort,
                        $"Duplicate Data Port numbers found: {string.Join(", ", duplicates)}");
                }

                // Check for multiple primary IPv4
                var primaryIPv4Count = dataPorts.Count(p => p.PrimaryPortRelation.IsPrimaryIpv4);
                if (primaryIPv4Count > 1)
                {
                    result.AddFailReason(AssetValidationField.DataPort,
                        "Only one Data Port can be marked as Primary IPv4.");
                }

                // Check for multiple primary IPv6
                var primaryIPv6Count = dataPorts.Count(p => p.PrimaryPortRelation.IsPrimaryIpv6);
                if (primaryIPv6Count > 1)
                {
                    result.AddFailReason(AssetValidationField.DataPort,
                        "Only one Data Port can be marked as Primary IPv6.");
                }
            }
            catch (Exception ex)
            {
                result.AddFailReason(AssetValidationField.DataPort,
                    $"Error validating data ports: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Validates all PowerPorts associated with the Asset.
        /// Queries PowerPort repository to load ports for this asset.
        /// </summary>
        /// <param name="asset">The asset whose power ports should be validated.</param>
        /// <returns>ValidationResult containing any port validation errors.</returns>
        public ValidationResult ValidateAssetPowerPorts(Asset asset)
        {
            var result = new ValidationResult();

            if (asset == null)
            {
                result.AddFailReason(AssetValidationField.Asset, "Asset cannot be null.");
                return result;
            }

            try
            {
                // Query PowerPorts for this Asset
                var powerPorts = _entityLoader.LoadPowerPorts(asset);

                // Check for negative port numbers
                var negativeNumbers = powerPorts.Where(p => p.PowerPortInfo.PortNumber < 0).ToList();
                if (negativeNumbers.Any())
                {
                    result.AddFailReason(AssetValidationField.PowerPort,
                        $"Power Port numbers cannot be negative. Found: {string.Join(", ", negativeNumbers.Select(p => p.PowerPortInfo.PortNumber))}");
                }

                // Check for duplicates
                var duplicates = powerPorts
                    .GroupBy(p => p.PowerPortInfo.PortNumber)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicates.Any())
                {
                    result.AddFailReason(AssetValidationField.PowerPort,
                        $"Duplicate Power Port numbers found: {string.Join(", ", duplicates)}");
                }
            }
            catch (Exception ex)
            {
                result.AddFailReason(AssetValidationField.PowerPort,
                    $"Error validating power ports: {ex.Message}");
            }

            return result;
        }

        #region Pipeline Construction

        private Validator<Asset> BuildValidationPipeline()
        {
            // Critical validations - stop on failure
            var criticalValidations = Validator<Asset>
                .Create(a => ValidateCriticalFields(a))
                .StopOnFailure();

            // Standard validations - collect all errors
            var standardValidations = Validator<Asset>
                .Create(ValidateInfo)
                .AndThen(ValidateLocation)
                .AndThen(ValidateDestinationLocation)
                .AndThen(ValidateLifecycle)
                .AndThen(ValidateOwnership)
                .AndThen(ValidateCollections);

            // Combine: critical first, then standard
            return criticalValidations.AndThen(standardValidations);
        }

        #endregion

        #region Validation Methods

        private ValidationResult ValidateCriticalFields(Asset asset, AssetValidationContext context = null)
        {
            var result = new ValidationResult();

            if (asset.NameField.Changed)
            {
                var exceptIds = context != null ?
                    context.ValidatedAssetIdentifiers :
                    new List<string> { asset.Identifier };
                result.AddFailuresFrom(IsAssetNameValid(asset.Name, exceptIds));
            }

            if (asset.AssetIDField.Changed)
            {
                var exceptIds = context != null ?
                    context.ValidatedAssetIdentifiers :
                    new List<string> { asset.Identifier };
                result.AddFailuresFrom(IsAssetIdValid(asset.AssetID, exceptIds));
            }

            // Asset Class is critical
            if (asset.AssetClassIdField.Changed)
            {
                if (!AssetValidationHandler.IsAssetClassValid(asset, out var assetClassResult))
                {
                    result.AddFailuresFrom(assetClassResult);
                }
            }

            return result;
        }

        private ValidationResult ValidateInfo(Asset asset)
        {
            var result = new ValidationResult();

            // Serial number validation (if changed and asset class is set)
            if (asset.SerialNumberField.Changed)
            {
                result.AddFailuresFrom(ValidateSerialNumber(asset));
            }

            return result;
        }

        private ValidationResult ValidateSerialNumber(Asset asset, AssetValidationContext context = null)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(asset.SerialNumber))
            {
                return result; // Empty serial number is valid
            }

            if (!asset.AssetClassId.HasValue())
            {
                return result; // Cannot validate without asset class
            }

            var exceptIds = context != null ?
                context.ValidatedAssetIdentifiers :
                new List<string> { asset.Identifier };

            if (IsSerialNumberInUse(asset.SerialNumber, asset.AssetClassId, exceptIds))
            {
                result.AddFailReason(AssetValidationField.SerialNumber,
                    "Serial Number is already in use for this Asset Class.");
            }

            return result;
        }

        private ValidationResult ValidateLocation(Asset asset)
        {
            var result = new ValidationResult();

            // Check if location can be edited based on state
            if (!AssetValidationHandler.IsLocationChangeAllowed(asset, out var permissionResult))
            {
                result.AddFailuresFrom(permissionResult);
                return result; // Cannot edit location in current state
            }

            // Validate single location type
            if (!AssetValidationHandler.HasSingleLocation(asset, out var singleLocationResult))
            {
                result.AddFailuresFrom(singleLocationResult);
            }

            // Parent Asset + Holder validation
            result.AddFailuresFrom(ValidateLocationParentAssetHolder(asset));

            // Rack validation
            result.AddFailuresFrom(ValidateLocationRackPosition(asset));

            return result;
        }

        private ValidationResult ValidateLocationParentAssetHolder(Asset asset)
        {
            var result = new ValidationResult();

            if ((asset.Location.ParentAssetField.Changed || asset.Location.HolderNumberField.Changed)
                && asset.AssetClassId.HasValue())
            {
                try
                {
                    // Pure logic validation (no data access)
                    if (!AssetValidationHandler.IsParentAssetHolderValid(asset, out var parentResult))
                    {
                        result.AddFailuresFrom(parentResult);
                    }

                    var assetClass = _entityLoader.LoadAssetClass(asset.AssetClassId);
                    if (assetClass != null)
                    {
                        // Advanced validation (requires data access)
                        result.AddFailuresFrom(ValidateParentAssetHolderAvailability(asset, assetClass));
                    }
                }
                catch (Exception ex)
                {
                    result.AddFailReason(AssetValidationField.ParentAsset,
                        $"Error validating parent asset: {ex.Message}");
                }
            }

            return result;
        }

        private ValidationResult ValidateDestinationLocation(Asset asset)
        {
            var result = new ValidationResult();

            // Check if destination location can be edited based on state
            if (!AssetValidationHandler.IsDestinationLocationChangeAllowed(asset, out var permissionResult))
            {
                result.AddFailuresFrom(permissionResult);
                return result; // Cannot edit destination location in current state
            }

            // Validate single destination location type
            if (!AssetValidationHandler.HasSingleDestinationLocation(asset, out var singleLocationResult))
            {
                result.AddFailuresFrom(singleLocationResult);
            }

            // Destination Parent Asset + Holder validation
            result.AddFailuresFrom(ValidateDestinationLocationParentAssetHolder(asset));

            // Destination Rack validation
            result.AddFailuresFrom(ValidateDestinationLocationRackPosition(asset));

            return result;
        }

        private ValidationResult ValidateLifecycle(Asset asset)
        {
            var result = new ValidationResult();

            if (asset.InstallationUserIdField.Changed || asset.InstallationDateField.Changed)
            {
                if (!AssetValidationHandler.IsInstallationInfoValid(asset, out var installationResult))
                {
                    result.AddFailuresFrom(installationResult);
                }
            }

            if (asset.ModificationUserIdField.Changed || asset.ModificationDateField.Changed)
            {
                if (!AssetValidationHandler.IsModificationInfoValid(asset, out var modificationResult))
                {
                    result.AddFailuresFrom(modificationResult);
                }
            }

            return result;
        }

        private ValidationResult ValidateOwnership(Asset asset)
        {
            var result = new ValidationResult();

            if (asset.Ownership.ContactPersonField.Changed || asset.Ownership.ContactPersonRoleField.Changed)
            {
                if (!AssetValidationHandler.IsOwnershipValid(asset, out var ownerResult))
                {
                    result.AddFailuresFrom(ownerResult);
                }
            }

            if (asset.Custody.ContactPersonField.Changed || asset.Custody.ContactPersonRoleField.Changed)
            {
                if (!AssetValidationHandler.IsCustodyValid(asset, out var custodyResult))
                {
                    result.AddFailuresFrom(custodyResult);
                }
            }

            return result;
        }

        private ValidationResult ValidateCollections(Asset asset)
        {
            var result = new ValidationResult();

            if (asset.HoldersField.Changed)
            {
                result.AddFailuresFrom(AssetValidationHandler.ValidateAssetHolders(asset));
            }

            if (asset.ElementsField.Changed)
            {
                result.AddFailuresFrom(AssetValidationHandler.ValidateAssetElements(asset));
            }

            // NOTE: DataPorts and PowerPorts validation requires repository queries
            // These are called separately via ValidateAssetDataPorts() and ValidateAssetPowerPorts()
            // They are not included in the standard pipeline to keep it lightweight

            return result;
        }

        #endregion

        #region Helper Methods

        private bool IsNameInUse(string name, List<string> exceptIdentifiers = null)
        {
            return _entityLoader.CountAssetsByName(name, exceptIdentifiers) > 0;
        }

        private bool IsAssetIdInUse(string assetId, List<string> exceptIdentifiers = null)
        {
            return _entityLoader.CountAssetsByAssetId(assetId, exceptIdentifiers) > 0;
        }

        private bool IsSerialNumberInUse(string serialNumber, SdmObjectReference<AssetClass> assetClassId, List<string> exceptIdentifiers = null)
        {
            return _entityLoader.CountAssetsBySerialNumber(serialNumber, assetClassId, exceptIdentifiers) > 0;
        }

        /// <summary>
        /// Validates parent asset holder availability - requires loading parent asset from repository.
        /// Checks if the parent asset has an available holder matching the hierarchy role and slot number.
        /// </summary>
        private ValidationResult ValidateParentAssetHolderAvailability(Asset asset, AssetClass assetClass)
        {
            var result = new ValidationResult();

            if (!asset.Location.ParentAsset.HasValue() || asset.Location?.HolderNumber == null)
            {
                return result; // Basic validation already done in handler
            }

            try
            {
                // Load the parent asset from repository
                var parentAsset = _entityLoader.LoadAsset(asset.Location.ParentAsset);

                if (parentAsset == null)
                {
                    result.AddFailReason(AssetValidationField.ParentAsset,
                        "Parent Asset not found.");
                    return result;
                }

                // Get hierarchy role from device type
                var deviceTypeFilter = DeviceTypeExposers.Identifier.Equal(assetClass.DeviceTypeId.Identifier);
                var deviceType = _entityLoader.LoadDeviceType(assetClass.DeviceTypeId);

                if (deviceType?.HierarchyInfo?.HierarchyRole == null)
                {
                    result.AddFailReason(AssetValidationField.AssetClass,
                        "Asset Class Device Type must have a Hierarchy Role to be attached to a parent asset.");
                    return result;
                }

                var hierarchyRole = deviceType.HierarchyInfo.HierarchyRole;
                var holderNumber = asset.Location.HolderNumber;

                // Check if parent asset has a holder slot matching the holder number and hierarchy role
                var matchingHolder = parentAsset.Holders?
                    .FirstOrDefault(h => h.SlotNumber == holderNumber && h.HierarchyRole == hierarchyRole);

                if (matchingHolder == null)
                {
                    result.AddFailReason(AssetValidationField.HolderNumber,
                        $"Invalid Holder Number: Parent Asset does not have a holder slot '{holderNumber}' for Hierarchy Role '{hierarchyRole}'.");
                    return result;
                }

                var childAssets = _entityLoader.FindChildAssets(parentAsset.Identifier, new List<string> { asset.Identifier });

                var occupyingAssets = childAssets
                    .Where(a => a.Location.ParentAsset != null &&
                               a.Location.ParentAsset.Identifier == parentAsset.Identifier &&
                               a.Location.HolderNumber == holderNumber)
                    .ToList();

                if (occupyingAssets.Any())
                {
                    result.AddFailReason(AssetValidationField.HolderNumber,
                        $"Holder Number '{holderNumber}' is already occupied on the Parent Asset by another asset.");
                }
            }
            catch (Exception ex)
            {
                result.AddFailReason(AssetValidationField.ParentAsset,
                    $"Error validating parent asset holder availability: {ex.Message}");
            }

            return result;
        }
        /// <summary>
        /// Validates rack space availability - requires loading rack from repository.
        /// Checks if the rack has available space for this asset.
        /// </summary>
        private ValidationResult ValidateRackSpaceAvailability(Asset asset)
        {
            var result = new ValidationResult();

            if (asset.Location.RackId == default || asset.Location?.RackPosition == null)
            {
                return result;
            }

            try
            {
                var (assetClass, deviceType) = _entityLoader.LoadAssetClassAndDeviceType(asset);

                if (!deviceType.TagsInfo.Tags.Contains(SlcAsset_Management.Enums.TagOption.RackUnitConsumer))
                {
                    return result;
                }

                if (assetClass.HeightU == default || assetClass.HeightU <= 0)
                {
                    return result;
                }

                var rack = _entityLoader.LoadRack(asset.Location.RackId);

                if (rack == null)
                {
                    result.AddFailReason(AssetValidationField.RackId, "Rack not found.");
                    return result;
                }

                if (asset.Location.RackPosition > rack.Capacity.MaximumRackCapacity)
                {
                    result.AddFailReason(AssetValidationField.RackPosition,
                        $"Invalid Position: Must be within Rack (max {rack.Capacity.MaximumRackCapacity} units).");
                    return result;
                }

                // ✅ Use RackValidator with explicit parameters
                var rackValidator = new RackValidator(_entityLoader);
                result.AddFailuresFrom(rackValidator.ValidateAssetPlacement(
                    asset,
                    asset.Location.RackId.Identifier,
                    (int)asset.Location.RackPosition));
            }
            catch (Exception ex)
            {
                result.AddFailReason(AssetValidationField.RackId,
                    $"Error validating rack space: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Validates destination rack space availability - requires loading rack from repository.
        /// Checks if the destination rack has available space for this asset.
        /// </summary>
        // TODO: Implement ValidateDestinationRackSpaceAvailability similarly
        private ValidationResult ValidateDestinationRackSpaceAvailability(Asset asset, AssetClass assetClass)
        {
            var result = new ValidationResult();

            if (asset.DestinationLocation.RackId == default || asset.DestinationLocation.RackPosition == null)
            {
                return result;
            }

            // TODO: Follow same pattern as ValidateRackSpaceAvailability
            // var rackValidator = new RackValidator(_entityLoader);
            // result.AddFailuresFrom(rackValidator.ValidateAssetPlacement(
            //     asset,
            //     asset.DestinationLocation.RackId,
            //     (int)asset.DestinationLocation.RackPosition));

            return result;
        }

        private ValidationResult ValidateLocationRackPosition(Asset asset)
        {
            var result = new ValidationResult();

            if ((asset.Location.RackIdField.Changed ||
                 asset.Location.RackPositionField.Changed ||
                 asset.Location.SideField.Changed)
                && asset.AssetClassId.HasValue())
            {
                try
                {
                    // Pure logic validation (basic checks)
                    if (!AssetValidationHandler.IsRackPositionValid(asset, out var rackResult))
                    {
                        result.AddFailuresFrom(rackResult);
                    }

                    // Advanced validation (rack space availability - requires Rack repository)
                    result.AddFailuresFrom(ValidateRackSpaceAvailability(asset));

                }
                catch (Exception ex)
                {
                    result.AddFailReason(AssetValidationField.RackId,
                        $"Error validating rack position: {ex.Message}");
                }
            }

            return result;
        }

        private ValidationResult ValidateDestinationLocationParentAssetHolder(Asset asset)
        {
            var result = new ValidationResult();

            if ((asset.DestinationLocation.ParentAssetField.Changed || asset.DestinationLocation.HolderNumberField.Changed)
                && asset.AssetClassId.HasValue())
            {
                try
                {
                    var assetClass = _entityLoader.LoadAssetClass(asset.AssetClassId);
                    if (assetClass != null)
                    {
                        // Pure logic validation (no data access)
                        if (!AssetValidationHandler.IsDestinationParentAssetHolderValid(asset, assetClass, out var parentResult))
                        {
                            result.AddFailuresFrom(parentResult);
                        }

                        // Advanced validation (requires data access)
                        result.AddFailuresFrom(ValidateDestinationParentAssetHolderAvailability(asset, assetClass));
                    }
                }
                catch (Exception ex)
                {
                    result.AddFailReason(AssetValidationField.DestinationParentAsset,
                        $"Error validating destination parent asset: {ex.Message}");
                }
            }

            return result;
        }

        private ValidationResult ValidateDestinationLocationRackPosition(Asset asset)
        {
            var result = new ValidationResult();

            if ((asset.DestinationLocation.RackIdField.Changed ||
                 asset.DestinationLocation.RackPositionField.Changed ||
                 asset.DestinationLocation.SideField.Changed)
                && asset.AssetClassId.HasValue())
            {
                try
                {
                    var assetClass = _entityLoader.LoadAssetClass(asset.AssetClassId);
                    if (assetClass != null)
                    {
                        // Pure logic validation (basic checks)
                        if (!AssetValidationHandler.IsDestinationRackPositionValid(asset, assetClass, out var rackResult))
                        {
                            result.AddFailuresFrom(rackResult);
                        }

                        // Advanced validation (rack space availability - requires Rack repository)
                        result.AddFailuresFrom(ValidateDestinationRackSpaceAvailability(asset, assetClass));
                    }
                }
                catch (Exception ex)
                {
                    result.AddFailReason(AssetValidationField.DestinationRackId,
                        $"Error validating destination rack position: {ex.Message}");
                }
            }

            return result;
        }

        /// <summary>
        /// Validates destination parent asset holder availability - requires loading parent asset from repository.
        /// Checks if the destination parent asset has an available holder matching the hierarchy role and slot number.
        /// </summary>
        private ValidationResult ValidateDestinationParentAssetHolderAvailability(Asset asset, AssetClass assetClass)
        {
            var result = new ValidationResult();

            if (!asset.DestinationLocation.ParentAsset.HasValue() || asset.DestinationLocation?.HolderNumber == null)
            {
                return result; // Basic validation already done in handler
            }

            try
            {
                // Load the destination parent asset from repository
                var parentAssetFilter = AssetExposers.Identifier.Equal(asset.DestinationLocation.ParentAsset.Identifier);
                var parentAsset = _entityLoader.LoadAsset(asset.DestinationLocation.ParentAsset);

                if (parentAsset == null)
                {
                    result.AddFailReason(AssetValidationField.DestinationParentAsset,
                        "Destination Parent Asset not found.");
                    return result;
                }

                // Get hierarchy role from device type
                var deviceType = _entityLoader.LoadDeviceType(assetClass.DeviceTypeId);

                if (deviceType?.HierarchyInfo?.HierarchyRole == null)
                {
                    result.AddFailReason(AssetValidationField.AssetClass,
                        "Asset Class Device Type must have a Hierarchy Role to be attached to a parent asset.");
                    return result;
                }

                var hierarchyRole = deviceType.HierarchyInfo.HierarchyRole;
                var holderNumber = asset.DestinationLocation.HolderNumber;

                // Check if parent asset has a holder slot matching the holder number and hierarchy role
                var matchingHolder = parentAsset.Holders?
                    .FirstOrDefault(h => h.SlotNumber == holderNumber && h.HierarchyRole == hierarchyRole);

                if (matchingHolder == null)
                {
                    result.AddFailReason(AssetValidationField.DestinationHolderNumber,
                        $"Invalid Holder Number: Destination Parent Asset does not have a holder slot '{holderNumber}' for Hierarchy Role '{hierarchyRole}'.");
                    return result;
                }

                // Check if the holder slot is already occupied by a different asset
                var occupiedFilter = AssetExposers.Identifier.NotEqual(asset.Identifier);

                var childAssets = _entityLoader.FindChildAssets(parentAsset.Identifier, new List<string> { asset.Identifier });

                var occupyingAssets = childAssets
                    .Where(a => a.DestinationLocation?.ParentAsset != null &&
                               a.DestinationLocation.ParentAsset.Identifier == parentAsset.Identifier &&
                               a.DestinationLocation.HolderNumber == holderNumber)
                    .ToList();

                if (occupyingAssets.Any())
                {
                    result.AddFailReason(AssetValidationField.DestinationHolderNumber,
                        $"Holder Number '{holderNumber}' is already occupied on the Destination Parent Asset by another asset.");
                }
            }
            catch (Exception ex)
            {
                result.AddFailReason(AssetValidationField.DestinationParentAsset,
                    $"Error validating destination parent asset holder availability: {ex.Message}");
            }

            return result;
        }

        #endregion

        #region Bulk Validation

        /// <summary>
        /// PHASE 1: Validates conflicts within the batch (in-memory, NO database access).
        /// Optimized - uses GroupBy instead of nested loops.
        /// Returns a dictionary of all validation results at once.
        /// </summary>
        private Dictionary<string, ValidationResult> ValidateAllBatchConflicts(List<Asset> assets)
        {
            var results = new Dictionary<string, ValidationResult>();

            // Initialize empty results for all assets
            foreach (var asset in assets)
            {
                results[asset.Identifier] = new ValidationResult();
            }

            // ===== 1. DUPLICATE NAMES =====
            var nameGroups = assets
                .Where(a => a.NameField.Changed && !string.IsNullOrWhiteSpace(a.Name))
                .GroupBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1);

            foreach (var group in nameGroups)
            {
                foreach (var asset in group)
                {
                    results[asset.Identifier].AddFailReason(AssetValidationField.Name,
                        $"Asset Name '{asset.Name}' is duplicated within the validation batch.");
                }
            }

            // ===== 2. DUPLICATE ASSET IDs =====
            var assetIdGroups = assets
                .Where(a => a.AssetIDField.Changed && !string.IsNullOrWhiteSpace(a.AssetID))
                .GroupBy(a => a.AssetID, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1);

            foreach (var group in assetIdGroups)
            {
                foreach (var asset in group)
                {
                    results[asset.Identifier].AddFailReason(AssetValidationField.AssetId,
                        $"Asset ID '{asset.AssetID}' is duplicated within the validation batch.");
                }
            }

            // ===== 3. DUPLICATE SERIAL NUMBERS (per AssetClass) =====
            var serialGroups = assets
                .Where(a => a.SerialNumberField.Changed &&
                           !string.IsNullOrWhiteSpace(a.SerialNumber) &&
                           a.AssetClassId.HasValue())
                .GroupBy(a => new { AssetClassId = a.AssetClassId.Identifier, SerialNumber = a.SerialNumber.ToLower() })
                .Where(g => g.Count() > 1);

            foreach (var group in serialGroups)
            {
                foreach (var asset in group)
                {
                    results[asset.Identifier].AddFailReason(AssetValidationField.SerialNumber,
                        $"Serial Number '{asset.SerialNumber}' is duplicated within the validation batch for this Asset Class.");
                }
            }

            // ===== 4. PARENT ASSET HOLDER CONFLICTS =====
            var holderGroups = assets
                .Where(a => a.Location?.ParentAsset.HasValue() == true && a.Location?.HolderNumber != null)
                .GroupBy(a => new { ParentId = a.Location.ParentAsset.Identifier, a.Location.HolderNumber })
                .Where(g => g.Count() > 1);

            foreach (var group in holderGroups)
            {
                foreach (var asset in group)
                {
                    results[asset.Identifier].AddFailReason(AssetValidationField.HolderNumber,
                        $"Holder Number '{asset.Location.HolderNumber}' on Parent Asset is already claimed by another asset in the validation batch.");
                }
            }

            // ===== 5. DESTINATION PARENT HOLDER CONFLICTS =====
            var destHolderGroups = assets
                .Where(a => a.DestinationLocation?.ParentAsset.HasValue() == true && a.DestinationLocation?.HolderNumber != null)
                .GroupBy(a => new { ParentId = a.DestinationLocation.ParentAsset.Identifier, a.DestinationLocation.HolderNumber })
                .Where(g => g.Count() > 1);

            foreach (var group in destHolderGroups)
            {
                foreach (var asset in group)
                {
                    results[asset.Identifier].AddFailReason(AssetValidationField.DestinationHolderNumber,
                        $"Destination Holder Number '{asset.DestinationLocation.HolderNumber}' on Parent Asset is already claimed by another asset in the validation batch.");
                }
            }

            // ===== 6. RACK POSITION OVERLAPS (Location) =====
            var rackGroups = assets
                .Where(a => a.Location?.RackId != default && a.Location?.RackPosition != null)
                .GroupBy(a => a.Location.RackId);

            foreach (var rackGroup in rackGroups)
            {
                var rackId = rackGroup.Key;
                var assetsInRack = rackGroup.ToList();

                if (assetsInRack.Count < 2)
                {
                    continue; // No conflicts possible
                }

                // Load rack once for this group
                var rack = _entityLoader.LoadRack(rackId);
                if (rack == null)
                {
                    continue;
                }

                // Check all pairs for overlaps
                for (int i = 0; i < assetsInRack.Count; i++)
                {
                    var asset1 = assetsInRack[i];
                    var assetClass1 = _entityLoader.LoadAssetClass(asset1.AssetClassId);
                    if (assetClass1 == null || assetClass1.HeightU <= 0)
                    {
                        continue;
                    }

                    for (int j = i + 1; j < assetsInRack.Count; j++)
                    {
                        var asset2 = assetsInRack[j];
                        var assetClass2 = _entityLoader.LoadAssetClass(asset2.AssetClassId);
                        if (assetClass2 == null || assetClass2.HeightU <= 0)
                        {
                            continue;
                        }

                        if (DoRangesOverlap(
                            (int)asset1.Location.RackPosition, (int)assetClass1.HeightU,
                            (int)asset2.Location.RackPosition, (int)assetClass2.HeightU
                            ))
                        {
                            // Mark BOTH assets as invalid
                            results[asset1.Identifier].AddFailReason(AssetValidationField.RackPosition,
                                $"Rack Position {asset1.Location.RackPosition} conflicts with another asset in the validation batch.");
                            results[asset2.Identifier].AddFailReason(AssetValidationField.RackPosition,
                                $"Rack Position {asset2.Location.RackPosition} conflicts with another asset in the validation batch.");
                        }
                    }
                }
            }

            // ===== 7. DESTINATION RACK POSITION OVERLAPS =====
            var destRackGroups = assets
                .Where(a => a.DestinationLocation?.RackId != default && a.DestinationLocation?.RackPosition != null)
                .GroupBy(a => a.DestinationLocation.RackId);

            foreach (var rackGroup in destRackGroups)
            {
                var rackId = rackGroup.Key;
                var assetsInRack = rackGroup.ToList();

                if (assetsInRack.Count < 2)
                {
                    continue;
                }

                var rack = _entityLoader.LoadRack(rackId);
                if (rack == null)
                {
                    continue;
                }

                // Check all pairs for overlaps
                for (int i = 0; i < assetsInRack.Count; i++)
                {
                    var asset1 = assetsInRack[i];
                    var assetClass1 = _entityLoader.LoadAssetClass(asset1.AssetClassId);
                    if (assetClass1 == null || assetClass1.HeightU <= 0)
                    {
                        continue;
                    }

                    for (int j = i + 1; j < assetsInRack.Count; j++)
                    {
                        var asset2 = assetsInRack[j];
                        var assetClass2 = _entityLoader.LoadAssetClass(asset2.AssetClassId);
                        if (assetClass2 == null || assetClass2.HeightU <= 0)
                        {
                            continue;
                        }

                        if (DoRackPositionsOverlap(
                            (int)asset1.DestinationLocation.RackPosition, (int)assetClass1.HeightU,
                            (int)asset2.DestinationLocation.RackPosition, (int)assetClass2.HeightU,
                            rack.Position))
                        {
                            results[asset1.Identifier].AddFailReason(AssetValidationField.DestinationRackPosition,
                                $"Destination Rack Position {asset1.DestinationLocation.RackPosition} conflicts with another asset in the validation batch.");
                            results[asset2.Identifier].AddFailReason(AssetValidationField.DestinationRackPosition,
                                $"Destination Rack Position {asset2.DestinationLocation.RackPosition} conflicts with another asset in the validation batch.");
                        }
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Validates multiple assets in bulk.
        /// PHASE 1: In-memory batch validation (fast, no DB)
        /// PHASE 2: Database validation with batch exclusion (only if Phase 1 passes)
        /// PHASE 3: Physical placement validation (optimized bulk)
        /// </summary>
        public Dictionary<string, ValidationResult> ValidateBulk(List<Asset> assets)
        {
            if (assets == null || !assets.Any())
            {
                return new Dictionary<string, ValidationResult>();
            }

            try
            {
                var context = new AssetValidationContext
                {
                    AssetsBeingValidated = assets
                };

                // ============================================================
                // PHASE 1: IN-MEMORY BATCH VALIDATION (NO DATABASE ACCESS)
                // ============================================================
                var results = ValidateAllBatchConflicts(assets);

                // Check if any asset failed Phase 1
                if (results.Any(r => !r.Value.IsValid))
                {
                    return results; // Fast fail - don't hit database
                }

                // ============================================================
                // PHASE 2: DATABASE VALIDATION (WITH BATCH EXCLUSION)
                // ============================================================
                foreach (var asset in assets)
                {
                    results[asset.Identifier].AddFailuresFrom(ValidateAgainstDatabase(asset, context));
                }

                // ============================================================
                // PHASE 3: BULK PLACEMENT VALIDATION
                // (Optimized - loads parent assets and racks once, validates all placements)
                // ============================================================
                var placementValidator = new PlacementValidator(_entityLoader);
                var placementResults = placementValidator.ValidateBulkPlacements(assets);

                foreach (var kvp in placementResults)
                {
                    if (results.ContainsKey(kvp.Key))
                    {
                        results[kvp.Key].AddFailuresFrom(kvp.Value);
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                var globalError = new ValidationResult();
                globalError.AddFailReason(AssetValidationField.Asset,
                    $"Error preparing bulk validation: {ex.Message}");

                var errorResults = new Dictionary<string, ValidationResult>();
                foreach (var asset in assets)
                {
                    errorResults[asset.Identifier] = globalError;
                }

                return errorResults;
            }
        }

        /// <summary>
        /// PHASE 2: Validates against database with batch exclusion.
        /// Only called if Phase 1 (batch conflicts) passes.
        /// </summary>
        private ValidationResult ValidateAgainstDatabase(Asset asset, AssetValidationContext context)
        {
            var result = new ValidationResult();

            // 1. Critical fields (Name/AssetID uniqueness in DB, excluding batch)
            result.AddFailuresFrom(ValidateCriticalFields(asset, context));

            if (!result.IsValid)
            {
                return result; // Stop on critical failures
            }

            // 2. ✅ FIX: ValidateInfo now accepts context
            result.AddFailuresFrom(ValidateInfo(asset, context));

            // 3. Location business rules
            result.AddFailuresFrom(ValidateLocationBusinessRules(asset));

            // 4. Destination location business rules
            result.AddFailuresFrom(ValidateDestinationLocationBusinessRules(asset));

            // 5. Lifecycle validation
            result.AddFailuresFrom(ValidateLifecycle(asset));

            // 6. Ownership validation
            result.AddFailuresFrom(ValidateOwnership(asset));

            // 7. Collections validation
            result.AddFailuresFrom(ValidateCollections(asset));

            return result;
        }

        /// <summary>
        /// Validates location business rules (state permissions, single location type).
        /// Does NOT validate physical placement - that's handled separately.
        /// </summary>
        private ValidationResult ValidateLocationBusinessRules(Asset asset)
        {
            var result = new ValidationResult();

            // State permission check
            if (!AssetValidationHandler.IsLocationChangeAllowed(asset, out var permissionResult))
            {
                result.AddFailuresFrom(permissionResult);
                return result;
            }

            // Single location type check
            if (!AssetValidationHandler.HasSingleLocation(asset, out var singleLocationResult))
            {
                result.AddFailuresFrom(singleLocationResult);
            }

            // Basic parent asset holder logic validation (not availability)
            if ((asset.Location.ParentAssetField.Changed || asset.Location.HolderNumberField.Changed)
                && asset.AssetClassId.HasValue())
            {
                if (!AssetValidationHandler.IsParentAssetHolderValid(asset, out var parentResult))
                {
                    result.AddFailuresFrom(parentResult);
                }
            }

            // Basic rack position logic validation (not space availability)
            if ((asset.Location.RackIdField.Changed ||
                 asset.Location.RackPositionField.Changed ||
                 asset.Location.SideField.Changed)
                && asset.AssetClassId.HasValue())
            {
                if (!AssetValidationHandler.IsRackPositionValid(asset, out var rackResult))
                {
                    result.AddFailuresFrom(rackResult);
                }
            }

            return result;
        }

        /// <summary>
        /// Validates destination location business rules.
        /// </summary>
        private ValidationResult ValidateDestinationLocationBusinessRules(Asset asset)
        {
            var result = new ValidationResult();

            // State permission check
            if (!AssetValidationHandler.IsDestinationLocationChangeAllowed(asset, out var permissionResult))
            {
                result.AddFailuresFrom(permissionResult);
                return result;
            }

            // Single destination location type check
            if (!AssetValidationHandler.HasSingleDestinationLocation(asset, out var singleLocationResult))
            {
                result.AddFailuresFrom(singleLocationResult);
            }

            // Basic destination parent holder logic validation (not availability)
            if ((asset.DestinationLocation.ParentAssetField.Changed || asset.DestinationLocation.HolderNumberField.Changed)
                && asset.AssetClassId.HasValue())
            {
                var assetClass = _entityLoader.LoadAssetClass(asset.AssetClassId);
                if (assetClass != null)
                {
                    if (!AssetValidationHandler.IsDestinationParentAssetHolderValid(asset, assetClass, out var parentResult))
                    {
                        result.AddFailuresFrom(parentResult);
                    }
                }
            }

            // Basic destination rack position logic validation (not space availability)
            if ((asset.DestinationLocation.RackIdField.Changed ||
                 asset.DestinationLocation.RackPositionField.Changed ||
                 asset.DestinationLocation.SideField.Changed)
                && asset.AssetClassId.HasValue())
            {
                var assetClass = _entityLoader.LoadAssetClass(asset.AssetClassId);
                if (assetClass != null)
                {
                    if (!AssetValidationHandler.IsDestinationRackPositionValid(asset, assetClass, out var rackResult))
                    {
                        result.AddFailuresFrom(rackResult);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// ✅ FIX: Update ValidateInfo to accept context parameter.
        /// </summary>
        private ValidationResult ValidateInfo(Asset asset, AssetValidationContext context = null)
        {
            var result = new ValidationResult();

            if (asset.SerialNumberField.Changed)
            {
                result.AddFailuresFrom(ValidateSerialNumber(asset, context));
            }

            return result;
        }

        /// <summary>
        /// Checks if two ranges overlap (scheme-agnostic).
        /// </summary>
        private bool DoRangesOverlap(long start1, long end1, long start2, long end2)
        {
            return start1 < end2 && end1 > start2;
        }

        #endregion
    }
}