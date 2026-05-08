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
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;
    using Skyline.DataMiner.SDM.FacilityManagement.Validation;

    using static Skyline.DataMiner.SDM.AssetManagement.Common.Validation.AssetValidationHandler;

    /// <summary>
    /// Central validation logic shared between single and bulk validation.
    /// Separated into no-database and database-access methods for optimal performance.
    /// </summary>
    internal class AssetValidationCore
    {
        private readonly SdmEntityLoader _entityLoader;
        private readonly Validator<Asset> _noDatabasePipeline;

        public AssetValidationCore(SdmEntityLoader entityLoader)
        {
            _entityLoader = entityLoader;
            _noDatabasePipeline = BuildNoDatabasePipeline();
        }

        #region No Database Access Validation

        /// <summary>
        /// Validates asset without database access (business rules, lifecycle, ownership).
        /// Uses a validation pipeline for clean, readable validation flow.
        /// </summary>
        public ValidationResult ValidateWithoutDatabaseAccess(Asset asset)
        {
            return _noDatabasePipeline.Validate(asset);
        }

        /// <summary>
        /// Builds the no-database validation pipeline.
        /// Asset Class → Location → Destination Location → Lifecycle → Ownership → Collections
        /// </summary>
        private Validator<Asset> BuildNoDatabasePipeline()
        {
            // Asset Class is critical - stop if invalid
            var assetClassValidation = Validator<Asset>
                .Create(ValidateAssetClass)
                .StopOnFailure();

            // Business rules - collect all errors
            var businessRules = Validator<Asset>
                .Create(ValidateLocationBusinessRules)
                .AndThen(ValidateDestinationLocationBusinessRules)
                .AndThen(ValidateLifecycle)
                .AndThen(ValidateOwnership)
                .AndThen(ValidateCollections);

            return assetClassValidation.AndThen(businessRules);
        }

        /// <summary>
        /// Validates asset class (critical - stops pipeline on failure).
        /// </summary>
        private ValidationResult ValidateAssetClass(Asset asset)
        {
            var result = new ValidationResult();

            if (asset.AssetClassIdField.Changed)
            {
                if (!AssetValidationHandler.IsAssetClassValid(asset, out var assetClassResult))
                {
                    result.AddFailuresFrom(assetClassResult);
                }
            }

            return result;
        }

        /// <summary>
        /// Validates location business rules (no database access).
        /// Checks permissions, single location type, and basic logic validation.
        /// </summary>
        private ValidationResult ValidateLocationBusinessRules(Asset asset)
        {
            var result = new ValidationResult();

            // State permission check
            if (!AssetValidationHandler.IsLocationChangeAllowed(asset, out var permissionResult))
            {
                result.AddFailuresFrom(permissionResult);
                return result; // Cannot edit location in current state
            }

            // Single location type
            if (!AssetValidationHandler.HasSingleLocation(asset, out var singleLocationResult))
            {
                result.AddFailuresFrom(singleLocationResult);
            }

            // Parent asset holder - basic logic validation only
            if ((asset.Location.ParentAssetField.Changed || asset.Location.HolderNumberField.Changed)
                && asset.AssetClassId.HasValue())
            {
                if (!AssetValidationHandler.IsParentAssetHolderValid(asset, out var parentResult))
                {
                    result.AddFailuresFrom(parentResult);
                }
            }

            // Rack position - basic logic validation only
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
        /// Validates destination location business rules (no database access).
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

            // Single destination location type
            if (!AssetValidationHandler.HasSingleDestinationLocation(asset, out var singleLocationResult))
            {
                result.AddFailuresFrom(singleLocationResult);
            }

            // Destination parent asset holder - basic logic validation only
            if ((asset.DestinationLocation.ParentAssetField.Changed ||
                 asset.DestinationLocation.HolderNumberField.Changed)
                && asset.AssetClassId.HasValue())
            {
                var assetClass = _entityLoader.LoadAssetClass(asset.AssetClassId);
                if (assetClass != null)
                {
                    if (!AssetValidationHandler.IsDestinationParentAssetHolderValid(
                        asset, assetClass, out var parentResult))
                    {
                        result.AddFailuresFrom(parentResult);
                    }
                }
            }

            // Destination rack position - basic logic validation only
            if ((asset.DestinationLocation.RackIdField.Changed ||
                 asset.DestinationLocation.RackPositionField.Changed ||
                 asset.DestinationLocation.SideField.Changed)
                && asset.AssetClassId.HasValue())
            {
                var assetClass = _entityLoader.LoadAssetClass(asset.AssetClassId);
                if (assetClass != null)
                {
                    if (!AssetValidationHandler.IsDestinationRackPositionValid(
                        asset, assetClass, out var rackResult))
                    {
                        result.AddFailuresFrom(rackResult);
                    }
                }
            }

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

            return result;
        }

        #endregion

        #region Database Access Validation

        /// <summary>
        /// Validates asset with database access (uniqueness checks, placement, ports).
        /// Only called after no-database checks pass.
        /// Uses a pipeline pattern organized by validation concern.
        /// </summary>
        public ValidationResult ValidateWithDatabaseAccess(Asset asset, AssetValidationContext context)
        {
            var result = ValidateUniquenessChecks(asset, context);
            // Port validation runs for both single and bulk validation
            result = result.CombineWith(ValidatePortChecks(asset));

            // Only run placement checks in single validation mode (context == null)
            // Bulk validation uses optimized PlacementValidator instead
            if (context == null)
            {
                result = result.CombineWith(ValidatePlacementChecks(asset));
            }


            return result;
        }

        /// <summary>
        /// Validates port data (data ports, power ports).
        /// Runs for both single and bulk validation.
        /// </summary>
        private ValidationResult ValidatePortChecks(Asset asset)
        {
            var result = new ValidationResult();

            // Data ports validation
            //if (asset.DataPortsField.Changed)
            //{
            //    result.AddFailuresFrom(ValidateDataPorts(asset));
            //}

            //// Power ports validation
            //if (asset.PowerPortsField.Changed)
            //{
            //    result.AddFailuresFrom(ValidatePowerPorts(asset));
            //}

            return result;
        }

        /// <summary>
        /// Validates uniqueness constraints (Name, Asset ID, Serial Number).
        /// Queries database to ensure values are not already in use.
        /// </summary>
        private ValidationResult ValidateUniquenessChecks(Asset asset, AssetValidationContext context)
        {
            var result = new ValidationResult();
            var exceptIds = GetExceptIdentifiers(asset, context);

            // Name uniqueness
            if (asset.NameField.Changed)
            {
                result.AddFailuresFrom(ValidateNameUniqueness(asset.Name, exceptIds));
            }

            // Asset ID uniqueness
            if (asset.AssetIDField.Changed)
            {
                result.AddFailuresFrom(ValidateAssetIdUniqueness(asset.AssetID, exceptIds));
            }

            // Serial number uniqueness
            if (asset.SerialNumberField.Changed)
            {
                result.AddFailuresFrom(ValidateSerialNumberUniqueness(
                    asset.SerialNumber, asset.AssetClassId, exceptIds));
            }

            return result;
        }

        /// <summary>
        /// Validates physical placement (holder availability, rack space).
        /// Only runs in single validation mode.
        /// </summary>
        private ValidationResult ValidatePlacementChecks(Asset asset)
        {
            return ValidateLocationPlacement(asset)
                .CombineWith(ValidateDestinationLocationPlacement(asset));
        }

        /// <summary>
        /// Validates location placement (holder availability, rack space).
        /// Requires database access to load parent assets and racks.
        /// </summary>
        private ValidationResult ValidateLocationPlacement(Asset asset)
        {
            var result = new ValidationResult();

            // Parent asset holder availability
            if ((asset.Location.ParentAssetField.Changed || asset.Location.HolderNumberField.Changed)
                && asset.AssetClassId.HasValue())
            {
                var assetClass = _entityLoader.LoadAssetClass(asset.AssetClassId);
                if (assetClass != null)
                {
                    result.AddFailuresFrom(ValidateParentAssetHolderAvailability(
                        asset, assetClass, context: null));
                }
            }

            // Rack space availability
            if ((asset.Location.RackIdField.Changed ||
                 asset.Location.RackPositionField.Changed ||
                 asset.Location.SideField.Changed)
                && asset.AssetClassId.HasValue())
            {
                result.AddFailuresFrom(ValidateRackSpaceAvailability(asset, context: null));
            }

            return result;
        }

        /// <summary>
        /// Validates destination location placement.
        /// </summary>
        private ValidationResult ValidateDestinationLocationPlacement(Asset asset)
        {
            var result = new ValidationResult();

            // Destination parent asset holder availability
            if ((asset.DestinationLocation.ParentAssetField.Changed ||
                 asset.DestinationLocation.HolderNumberField.Changed)
                && asset.AssetClassId.HasValue())
            {
                var assetClass = _entityLoader.LoadAssetClass(asset.AssetClassId);
                if (assetClass != null)
                {
                    result.AddFailuresFrom(ValidateDestinationParentAssetHolderAvailability(
                        asset, assetClass, context: null));
                }
            }

            // Destination rack space availability
            if ((asset.DestinationLocation.RackIdField.Changed ||
                 asset.DestinationLocation.RackPositionField.Changed ||
                 asset.DestinationLocation.SideField.Changed)
                && asset.AssetClassId.HasValue())
            {
                var assetClass = _entityLoader.LoadAssetClass(asset.AssetClassId);
                if (assetClass != null)
                {
                    result.AddFailuresFrom(ValidateDestinationRackSpaceAvailability(
                        asset, assetClass, context: null));
                }
            }

            return result;
        }

        public ValidationResult ValidateNameUniqueness(string name, List<string> exceptIdentifiers = null)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(name))
            {
                result.AddFailReason(AssetValidationField.Name,
                    "Asset Name cannot be empty or whitespace.");
                return result;
            }

            if (_entityLoader.CountAssetsByName(name, exceptIdentifiers) > 0)
            {
                result.AddFailReason(AssetValidationField.Name,
                    $"Asset Name '{name}' is already in use.");
            }

            return result;
        }

        public ValidationResult ValidateAssetIdUniqueness(string assetId, List<string> exceptIdentifiers = null)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(assetId))
            {
                result.AddFailReason(AssetValidationField.AssetId,
                    "Asset ID cannot be empty or whitespace.");
                return result;
            }

            if (_entityLoader.CountAssetsByAssetId(assetId, exceptIdentifiers) > 0)
            {
                result.AddFailReason(AssetValidationField.AssetId,
                    $"Asset ID '{assetId}' is already in use.");
            }

            return result;
        }

        private ValidationResult ValidateSerialNumberUniqueness(string serialNumber,
            SdmObjectReference<AssetClass> assetClassId, List<string> exceptIdentifiers = null)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(serialNumber) || !assetClassId.HasValue())
            {
                return result;
            }

            if (_entityLoader.CountAssetsBySerialNumber(serialNumber, assetClassId, exceptIdentifiers) > 0)
            {
                result.AddFailReason(AssetValidationField.SerialNumber,
                    "Serial Number is already in use for this Asset Class.");
            }

            return result;
        }

        #endregion

        #region Bulk-Specific Validation

        /// <summary>
        /// Phase 1: In-memory batch conflict detection (optimized with GroupBy).
        /// No database access - fast validation.
        /// </summary>
        public Dictionary<string, ValidationResult> ValidateBatchConflicts(List<Asset> assets)
        {
            var results = assets.ToDictionary(a => a.Identifier, a => new ValidationResult());

            // Duplicate names
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

            // Duplicate asset IDs
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

            // Duplicate serial numbers (per asset class)
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

            // Parent holder conflicts
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

            // Rack position overlaps (optimized)
            ValidateBatchRackConflicts(assets, results);

            return results;
        }

        private void ValidateBatchRackConflicts(List<Asset> assets, Dictionary<string, ValidationResult> results)
        {
            var rackGroups = assets
                .Where(a => a.Location?.RackId != default && a.Location?.RackPosition != null)
                .GroupBy(a => a.Location.RackId);

            foreach (var rackGroup in rackGroups)
            {
                var assetsInRack = rackGroup.ToList();
                if (assetsInRack.Count < 2) continue;

                for (int i = 0; i < assetsInRack.Count; i++)
                {
                    var asset1 = assetsInRack[i];
                    var assetClass1 = _entityLoader.LoadAssetClass(asset1.AssetClassId);
                    if (assetClass1 == null || assetClass1.HeightU <= 0) continue;

                    for (int j = i + 1; j < assetsInRack.Count; j++)
                    {
                        var asset2 = assetsInRack[j];
                        var assetClass2 = _entityLoader.LoadAssetClass(asset2.AssetClassId);
                        if (assetClass2 == null || assetClass2.HeightU <= 0) continue;

                        if (DoRangesOverlap(
                            (int)asset1.Location.RackPosition, (int)assetClass1.HeightU,
                            (int)asset2.Location.RackPosition, (int)assetClass2.HeightU))
                        {
                            results[asset1.Identifier].AddFailReason(AssetValidationField.RackPosition,
                                $"Rack Position {asset1.Location.RackPosition} conflicts with another asset in the validation batch.");
                            results[asset2.Identifier].AddFailReason(AssetValidationField.RackPosition,
                                $"Rack Position {asset2.Location.RackPosition} conflicts with another asset in the validation batch.");
                        }
                    }
                }
            }
        }

        #endregion

        #region Port Validation (Database Access)

        public ValidationResult ValidateDataPorts(Asset asset)
        {
            var result = new ValidationResult();

            if (asset == null)
            {
                result.AddFailReason(AssetValidationField.Asset, "Asset cannot be null.");
                return result;
            }

            try
            {
                var dataPorts = _entityLoader.LoadDataPorts(asset);

                var seenPortNumbers = new HashSet<long>();
                int primaryIPv4Count = 0;
                int primaryIPv6Count = 0;

                foreach (var port in dataPorts)
                {
                    // Check for negative port numbers
                    if (port.DataPortInfo.PortNumber < 0)
                    {
                        result.AddFailReason(AssetValidationField.DataPort,
                            $"Data Port number cannot be negative. Found: {port.DataPortInfo.PortNumber}");
                        return result;
                    }

                    // Check for duplicate port numbers
                    if (!seenPortNumbers.Add(port.DataPortInfo.PortNumber))
                    {
                        result.AddFailReason(AssetValidationField.DataPort,
                            $"Duplicate Data Port number found: {port.DataPortInfo.PortNumber}");
                        return result;
                    }

                    // Count primary IPv4 ports
                    if (port.PrimaryPortRelation.IsPrimaryIpv4)
                    {
                        primaryIPv4Count++;
                        if (primaryIPv4Count > 1)
                        {
                            result.AddFailReason(AssetValidationField.DataPort,
                                "Only one Data Port can be marked as Primary IPv4.");
                            return result;
                        }
                    }

                    // Count primary IPv6 ports
                    if (port.PrimaryPortRelation.IsPrimaryIpv6)
                    {
                        primaryIPv6Count++;
                        if (primaryIPv6Count > 1)
                        {
                            result.AddFailReason(AssetValidationField.DataPort,
                                "Only one Data Port can be marked as Primary IPv6.");
                            return result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.AddFailReason(AssetValidationField.DataPort,
                    $"Error validating data ports: {ex.Message}");
            }

            return result;
        }

        public ValidationResult ValidatePowerPorts(Asset asset)
        {
            var result = new ValidationResult();

            if (asset == null)
            {
                result.AddFailReason(AssetValidationField.Asset, "Asset cannot be null.");
                return result;
            }

            try
            {
                var powerPorts = _entityLoader.LoadPowerPorts(asset);

                var seenPortNumbers = new HashSet<long>();

                foreach (var port in powerPorts)
                {
                    // Check for negative port numbers
                    if (port.PowerPortInfo.PortNumber < 0)
                    {
                        result.AddFailReason(AssetValidationField.PowerPort,
                            $"Power Port number cannot be negative. Found: {port.PowerPortInfo.PortNumber}");
                        return result;
                    }

                    // Check for duplicate port numbers
                    if (!seenPortNumbers.Add(port.PowerPortInfo.PortNumber))
                    {
                        result.AddFailReason(AssetValidationField.PowerPort,
                            $"Duplicate Power Port number found: {port.PowerPortInfo.PortNumber}");
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                result.AddFailReason(AssetValidationField.PowerPort,
                    $"Error validating power ports: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Validates DataPorts that are already loaded in memory.
        /// Avoids redundant database queries when ports are pre-loaded.
        /// </summary>
        /// <param name="dataPorts">The loaded DataPorts collection.</param>
        public ValidationResult ValidateLoadedDataPorts(List<DataPort> dataPorts)
        {
            if (dataPorts == null || !dataPorts.Any())
            {
                return new ValidationResult();
            }

            var result = new ValidationResult();

            try
            {
                // Use DataPortValidationCore for collection validation
                var dataPortValidator = new DataPortValidationCore(_entityLoader);
                result.AddFailuresFrom(dataPortValidator.ValidateDataPortCollection(dataPorts));
            }
            catch (Exception ex)
            {
                result.AddFailReason(AssetValidationField.DataPort,
                    $"Error validating loaded data ports: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Validates PowerPorts that are already loaded in memory.
        /// Avoids redundant database queries when ports are pre-loaded.
        /// </summary>
        /// <param name="powerPorts">The loaded PowerPorts collection.</param>
        public ValidationResult ValidateLoadedPowerPorts(List<PowerPort> powerPorts)
        {
            if (powerPorts == null || !powerPorts.Any())
            {
                return new ValidationResult();
            }

            var result = new ValidationResult();

            try
            {
                var seenPortNumbers = new HashSet<long>();

                foreach (var port in powerPorts)
                {
                    // Check for negative port numbers
                    if (port.PowerPortInfo.PortNumber < 0)
                    {
                        result.AddFailReason(AssetValidationField.PowerPort,
                            $"Power Port number cannot be negative. Found: {port.PowerPortInfo.PortNumber}");
                        return result;
                    }

                    // Check for duplicate port numbers
                    if (!seenPortNumbers.Add(port.PowerPortInfo.PortNumber))
                    {
                        result.AddFailReason(AssetValidationField.PowerPort,
                            $"Duplicate Power Port number found: {port.PowerPortInfo.PortNumber}");
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                result.AddFailReason(AssetValidationField.PowerPort,
                    $"Error validating loaded power ports: {ex.Message}");
            }

            return result;
        }

        #endregion

        #region Helper Methods (Database Access)

        private List<string> GetExceptIdentifiers(Asset asset, AssetValidationContext context)
        {
            return context?.ValidatedAssetIdentifiers ?? new List<string> { asset.Identifier };
        }

        private ValidationResult ValidateParentAssetHolderAvailability(
            Asset asset, AssetClass assetClass, AssetValidationContext context)
        {
            var result = new ValidationResult();

            if (!asset.Location.ParentAsset.HasValue() || asset.Location?.HolderNumber == null)
            {
                return result;
            }

            try
            {
                var parentAsset = _entityLoader.LoadAsset(asset.Location.ParentAsset);
                if (parentAsset == null)
                {
                    result.AddFailReason(AssetValidationField.ParentAsset, "Parent Asset not found.");
                    return result;
                }

                var deviceType = _entityLoader.LoadDeviceType(assetClass.DeviceTypeId);
                if (deviceType?.HierarchyInfo?.HierarchyRole == null)
                {
                    result.AddFailReason(AssetValidationField.AssetClass,
                        "Asset Class Device Type must have a Hierarchy Role to be attached to a parent asset.");
                    return result;
                }

                var hierarchyRole = deviceType.HierarchyInfo.HierarchyRole;
                var holderNumber = asset.Location.HolderNumber;

                var matchingHolder = parentAsset.Holders?
                    .FirstOrDefault(h => h.SlotNumber == holderNumber && h.HierarchyRole == hierarchyRole);

                if (matchingHolder == null)
                {
                    result.AddFailReason(AssetValidationField.HolderNumber,
                        $"Invalid Holder Number: Parent Asset does not have a holder slot '{holderNumber}' for Hierarchy Role '{hierarchyRole}'.");
                    return result;
                }

                var exceptIds = GetExceptIdentifiers(asset, context);
                var childAssets = _entityLoader.FindChildAssets(parentAsset.Identifier, exceptIds);

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

        private ValidationResult ValidateDestinationParentAssetHolderAvailability(
            Asset asset, AssetClass assetClass, AssetValidationContext context)
        {
            var result = new ValidationResult();

            if (!asset.DestinationLocation.ParentAsset.HasValue() || asset.DestinationLocation?.HolderNumber == null)
            {
                return result;
            }

            try
            {
                var parentAsset = _entityLoader.LoadAsset(asset.DestinationLocation.ParentAsset);
                if (parentAsset == null)
                {
                    result.AddFailReason(AssetValidationField.DestinationParentAsset,
                        "Destination Parent Asset not found.");
                    return result;
                }

                var deviceType = _entityLoader.LoadDeviceType(assetClass.DeviceTypeId);
                if (deviceType?.HierarchyInfo?.HierarchyRole == null)
                {
                    result.AddFailReason(AssetValidationField.AssetClass,
                        "Asset Class Device Type must have a Hierarchy Role to be attached to a parent asset.");
                    return result;
                }

                var hierarchyRole = deviceType.HierarchyInfo.HierarchyRole;
                var holderNumber = asset.DestinationLocation.HolderNumber;

                var matchingHolder = parentAsset.Holders?
                    .FirstOrDefault(h => h.SlotNumber == holderNumber && h.HierarchyRole == hierarchyRole);

                if (matchingHolder == null)
                {
                    result.AddFailReason(AssetValidationField.DestinationHolderNumber,
                        $"Invalid Holder Number: Destination Parent Asset does not have a holder slot '{holderNumber}' for Hierarchy Role '{hierarchyRole}'.");
                    return result;
                }

                var exceptIds = GetExceptIdentifiers(asset, context);
                var childAssets = _entityLoader.FindChildAssets(parentAsset.Identifier, exceptIds);

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

        private ValidationResult ValidateRackSpaceAvailability(Asset asset, AssetValidationContext context)
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

        private ValidationResult ValidateDestinationRackSpaceAvailability(
            Asset asset, AssetClass assetClass, AssetValidationContext context)
        {
            // TODO: Implement if needed
            return new ValidationResult();
        }

        private bool DoRangesOverlap(long start1, long end1, long start2, long end2)
        {
            return start1 <= end2 && end1 >= start2;
        }

        #endregion
    }
}