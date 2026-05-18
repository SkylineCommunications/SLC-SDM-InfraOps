namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SharedCommonLibrary.AssetManagement.Models;
    using SharedCommonLibrary.AssetManagement.State_Management;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM.AssetManagement.Common.Validation;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Common.Services;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM.FacilityManagement.Validation;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    using static Skyline.DataMiner.SDM.AssetManagement.Common.Validation.AssetValidationHandler;
    using static Skyline.DataMiner.SDM.FacilityManagement.Validation.RackValidationHandler;


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
                .Create(ValidateStateTransition)
                .AndThen(ValidateLocationBusinessRules)
                .AndThen(ValidateDestinationLocationBusinessRules)
                .AndThen(ValidateLifecycle)
                .AndThen(ValidateOwnershipAndCustody)
                .AndThen(ValidateCollections);

            return assetClassValidation.AndThen(businessRules);
        }

        /// <summary>
        /// Validates state transition using AssetValidationHandler.
        /// </summary>
        private ValidationResult ValidateStateTransition(Asset asset)
        {
            return AssetValidationHandler.ValidateStateTransition(asset);
        }

        /// <summary>
        /// Validates asset class (critical - stops pipeline on failure).
        /// </summary>
        private ValidationResult ValidateAssetClass(Asset asset)
        {
            var result = new ValidationResult();

            if (asset.IsNew || asset.AssetClassIdField.Changed)
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
            // Early return if Location hasn't changed at all
            if (!HasLocationChanged(asset))
            {
                return new ValidationResult();
            }

            // State permission check - early return if fails
            if (!AssetValidationHandler.IsLocationChangeAllowed(asset, out var permissionResult))
            {
                return permissionResult; // Cannot edit location in current state
            }

            // After permission check passes, collect all location errors
            var validations = new List<ValidationResult>();

            // Single location type
            if (!AssetValidationHandler.HasSingleLocation(asset, out var singleLocationResult))
            {
                validations.Add(singleLocationResult);
            }

            // Parent asset holder - basic logic validation only
            if ((asset.Location.ParentAssetField.Changed || asset.Location.HolderNumberField.Changed)
                && asset.AssetClassId.HasValue())
            {
                if (!AssetValidationHandler.IsParentAssetHolderValid(asset, out var parentResult))
                {
                    validations.Add(parentResult);
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
                    validations.Add(rackResult);
                }
            }

            return validations.MergeAll();
        }

        /// <summary>
        /// Checks if any Location field has changed.
        /// </summary>
        private bool HasLocationChanged(Asset asset)
        {
            if (asset.Location == null)
            {
                return false;
            }

            return asset.Location.ParentAssetField.Changed ||
                   asset.Location.HolderNumberField.Changed ||
                   asset.Location.RackIdField.Changed ||
                   asset.Location.RackPositionField.Changed ||
                   asset.Location.SideField.Changed ||
                   asset.Location.DeskIdField.Changed ||
                   asset.Location.ContainerIdField.Changed ||
                   asset.Location.RoomIdField.Changed ||
                   asset.Location.PowerSupplyRackPositionField.Changed;
        }

        /// <summary>
        /// Checks if any DestinationLocation field has changed.
        /// </summary>
        private bool HasDestinationLocationChanged(Asset asset)
        {
            if (asset.DestinationLocation == null)
            {
                return false;
            }

            return asset.DestinationLocation.ParentAssetField.Changed ||
                   asset.DestinationLocation.HolderNumberField.Changed ||
                   asset.DestinationLocation.RackIdField.Changed ||
                   asset.DestinationLocation.RackPositionField.Changed ||
                   asset.DestinationLocation.SideField.Changed ||
                   asset.DestinationLocation.DeskIdField.Changed ||
                   asset.DestinationLocation.ContainerIdField.Changed ||
                   asset.DestinationLocation.RoomIdField.Changed ||
                   asset.DestinationLocation.PowerSupplyRackPositionField.Changed;
        }

        /// <summary>
        /// Validates destination location business rules (no database access).
        /// Rules:
        /// - DestinationLocation is MANDATORY when state is InTransit
        /// - Only validate business rules when InTransit AND it has values
        /// - In all other states, DestinationLocation is ignored (with warning if present)
        /// </summary>
        private ValidationResult ValidateDestinationLocationBusinessRules(Asset asset)
        {
            var result = new ValidationResult();

            // Early return if DestinationLocation hasn't changed at all
            if (!HasDestinationLocationChanged(asset) && !asset.StateField.Changed)
            {
                return result;
            }

            // If we reached here, at least one field changed - check for mandatory/warning validation
            var destinationLocationResult = AssetValidationHandler.ValidateDestinationLocation(asset);
            result.AddFrom(destinationLocationResult);

            // DestinationLocation business rules validation ONLY applies when state is InTransit
            if (asset.State != SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InTransit)
            {
                return result; // Ignore DestinationLocation in all other states
            }

            // Check if DestinationLocation has any values set
            if (!HasDestinationLocationValues(asset))
            {
                return result; // Already validated as mandatory above, no business rules to check
            }

            // After checks pass, collect all destination location errors
            var validations = new List<ValidationResult> { result };

            // Single destination location type
            if (!AssetValidationHandler.HasSingleDestinationLocation(asset, out var singleLocationResult))
            {
                validations.Add(singleLocationResult);
            }

            // Destination parent asset holder - basic logic validation only
            if ((asset.DestinationLocation.ParentAssetField.Changed || asset.DestinationLocation.HolderNumberField.Changed)
                && asset.AssetClassId.HasValue())
            {
                if (!AssetValidationHandler.IsDestinationParentAssetHolderValid(asset, out var parentResult))
                {
                    validations.Add(parentResult);
                }
            }

            var assetClass = _entityLoader.LoadAssetClass(asset.AssetClassId);

            // Destination rack position - basic logic validation only
            if ((asset.DestinationLocation.RackIdField.Changed ||
                 asset.DestinationLocation.RackPositionField.Changed ||
                 asset.DestinationLocation.SideField.Changed)
                && asset.AssetClassId.HasValue())
            {
                if (!AssetValidationHandler.IsDestinationRackPositionValid(asset, assetClass, out var rackResult))
                {
                    validations.Add(rackResult);
                }
            }

            return validations.MergeAll();
        }

        /// <summary>
        /// Checks if DestinationLocation has any values set.
        /// </summary>
        private bool HasDestinationLocationValues(Asset asset)
        {
            if (asset.DestinationLocation == null)
            {
                return false;
            }

            return asset.DestinationLocation.ParentAsset.HasValue() ||
                   asset.DestinationLocation.RackId.HasValue() ||
                   asset.DestinationLocation.DeskId != default ||
                   asset.DestinationLocation.ContainerId.HasValue() ||
                   asset.DestinationLocation.RoomId.HasValue();
        }

        private ValidationResult ValidateLifecycle(Asset asset)
        {
            var validations = new List<ValidationResult>();

            if (asset.InstallationUserIdField.Changed || asset.InstallationDateField.Changed)
            {
                if (!AssetValidationHandler.IsInstallationInfoValid(asset, out var installationResult))
                {
                    validations.Add(installationResult);
                }
            }

            if (asset.ModificationUserIdField.Changed || asset.ModificationDateField.Changed)
            {
                if (!AssetValidationHandler.IsModificationInfoValid(asset, out var modificationResult))
                {
                    validations.Add(modificationResult);
                }
            }

            return validations.MergeAll();
        }

        private ValidationResult ValidateOwnershipAndCustody(Asset asset)
        {
            var validations = new List<ValidationResult>();

            if (asset.Ownership.ContactPersonField.Changed || asset.Ownership.ContactPersonRoleField.Changed)
            {
                if (!AssetValidationHandler.IsOwnershipValid(asset, out var ownerResult))
                {
                    validations.Add(ownerResult);
                }
            }

            if (asset.Custody.ContactPersonField.Changed || asset.Custody.ContactPersonRoleField.Changed)
            {
                if (!AssetValidationHandler.IsCustodyValid(asset, out var custodyResult))
                {
                    validations.Add(custodyResult);
                }
            }

            return validations.MergeAll();
        }

        private ValidationResult ValidateCollections(Asset asset)
        {
            var validations = new List<ValidationResult>();

            if (asset.HoldersField.Changed)
            {
                validations.Add(AssetValidationHandler.ValidateAssetHolders(asset));
            }

            if (asset.ElementsField.Changed)
            {
                validations.Add(AssetValidationHandler.ValidateAssetElements(asset));
            }

            return validations.MergeAll();
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
            var validations = new List<ValidationResult>
            {
                ValidateUniquenessChecks(asset, context),
                ValidatePortChecks(asset)
            };

            // Only run placement checks in single validation mode (context == null)
            // Bulk validation uses optimized PlacementValidator instead
            if (context == null)
            {
                validations.Add(ValidateLocationPlacement(asset));
            }

            return validations.MergeAll();
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
            var validations = new List<ValidationResult>();
            var exceptIds = GetExceptIdentifiers(asset, context);

            // Name uniqueness
            if (asset.IsNew || asset.NameField.Changed)
            {
                validations.Add(ValidateNameUniqueness(asset.Name, exceptIds));
            }

            // Asset ID uniqueness
            if (asset.IsNew || asset.AssetIDField.Changed)
            {
                validations.Add(ValidateAssetIdUniqueness(asset.AssetID, exceptIds));
            }

            // Serial number uniqueness
            if (asset.SerialNumberField.Changed)
            {
                validations.Add(ValidateSerialNumberUniqueness(
                    asset.SerialNumber, asset.AssetClassId, exceptIds));
            }

            return validations.MergeAll();
        }

        /// <summary>
        /// Validates location placement (holder availability, rack space).
        /// Requires database access to load parent assets and racks.
        /// </summary>
        private ValidationResult ValidateLocationPlacement(Asset asset)
        {
            var validations = new List<ValidationResult>();

            // Parent asset holder availability
            if ((asset.Location.ParentAssetField.Changed || asset.Location.HolderNumberField.Changed)
                && asset.AssetClassId.HasValue())
            {
                var assetClass = _entityLoader.LoadAssetClass(asset.AssetClassId);
                if (assetClass != null)
                {
                    validations.Add(ValidateParentAssetHolderAvailability(
                        asset, assetClass, context: null));
                }
            }

            // Rack space availability
            if ((asset.Location.RackIdField.Changed ||
                 asset.Location.RackPositionField.Changed ||
                 asset.Location.SideField.Changed)
                && asset.AssetClassId.HasValue())
            {
                validations.Add(ValidateRackSpaceAvailability(asset, context: null));
            }

            return validations.MergeAll();
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
        /// Phase 2: In-memory batch conflict detection (optimized with GroupBy).
        /// No database access - fast validation.
        /// Returns validation results in the same order as the input assets.
        /// Result at index i corresponds to asset at index i.
        /// </summary>
        public List<ValidationResult> ValidateBatchConflicts(List<Asset> assets)
        {
            // Initialize results - same order as input
            var results = assets.Select(a => new ValidationResult()).ToList();

            // Duplicate names
            var nameGroups = assets
                .Select((asset, index) => new { asset, index })
                .Where(x => x.asset.NameField.Changed && !string.IsNullOrWhiteSpace(x.asset.Name))
                .GroupBy(x => x.asset.Name, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1);

            foreach (var group in nameGroups)
            {
                foreach (var item in group)
                {
                    results[item.index].AddFailReason(AssetValidationField.Name,
                        $"Asset Name '{item.asset.Name}' is duplicated within the validation batch.");
                }
            }

            // Duplicate asset IDs
            var assetIdGroups = assets
                .Select((asset, index) => new { asset, index })
                .Where(x => x.asset.AssetIDField.Changed && !string.IsNullOrWhiteSpace(x.asset.AssetID))
                .GroupBy(x => x.asset.AssetID, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1);

            foreach (var group in assetIdGroups)
            {
                foreach (var item in group)
                {
                    results[item.index].AddFailReason(AssetValidationField.AssetId,
                        $"Asset ID '{item.asset.AssetID}' is duplicated within the validation batch.");
                }
            }

            // Duplicate serial numbers (per asset class)
            var serialGroups = assets
                .Select((asset, index) => new { asset, index })
                .Where(x => x.asset.SerialNumberField.Changed &&
                           !string.IsNullOrWhiteSpace(x.asset.SerialNumber) &&
                           x.asset.AssetClassId.HasValue())
                .GroupBy(x => new { AssetClassId = x.asset.AssetClassId.Identifier, SerialNumber = x.asset.SerialNumber.ToLower() })
                .Where(g => g.Count() > 1);

            foreach (var group in serialGroups)
            {
                foreach (var item in group)
                {
                    results[item.index].AddFailReason(AssetValidationField.SerialNumber,
                        $"Serial Number '{item.asset.SerialNumber}' is duplicated within the validation batch for this Asset Class.");
                }
            }

            // Parent holder conflicts
            var holderGroups = assets
                .Select((asset, index) => new { asset, index })
                .Where(x => x.asset.Location?.ParentAsset.HasValue() == true && x.asset.Location?.HolderNumber != null)
                .GroupBy(x => new { ParentId = x.asset.Location.ParentAsset.Identifier, x.asset.Location.HolderNumber })
                .Where(g => g.Count() > 1);

            foreach (var group in holderGroups)
            {
                foreach (var item in group)
                {
                    results[item.index].AddFailReason(AssetValidationField.HolderNumber,
                        $"Holder Number '{item.asset.Location.HolderNumber}' on Parent Asset is already claimed by another asset in the validation batch.");
                }
            }

            // Rack position overlaps (optimized)
            var rackConflicts = ValidateBatchRackConflicts(assets);
            for (int i = 0; i < assets.Count; i++)
            {
                results[i].AddFailuresFrom(rackConflicts[i]);
            }

            return results;
        }

        /// <summary>
        /// Validates rack position conflicts within a batch.
        /// Returns a list of validation results in the same order as input assets.
        /// </summary>
        private List<ValidationResult> ValidateBatchRackConflicts(List<Asset> assets)
        {
            // Initialize results - same order as input
            var results = assets.Select(a => new ValidationResult()).ToList();

            var rackGroups = assets
                .Select((asset, index) => new { asset, index })
                .Where(x => x.asset.Location != null && x.asset.Location.RackId.HasValue() && x.asset.Location.RackPosition > 0)
                .GroupBy(x => x.asset.Location.RackId);

            foreach (var rackGroup in rackGroups)
            {
                var assetsInRack = rackGroup.ToList();
                if (assetsInRack.Count < 2) continue;

                for (int i = 0; i < assetsInRack.Count; i++)
                {
                    var item1 = assetsInRack[i];
                    var assetClass1 = _entityLoader.LoadAssetClass(item1.asset.AssetClassId);
                    if (assetClass1 == null || assetClass1.HeightU <= 0) continue;

                    for (int j = i + 1; j < assetsInRack.Count; j++)
                    {
                        var item2 = assetsInRack[j];
                        var assetClass2 = _entityLoader.LoadAssetClass(item2.asset.AssetClassId);
                        if (assetClass2 == null || assetClass2.HeightU <= 0) continue;

                        if (DoRangesOverlap(
                            (int)item1.asset.Location.RackPosition, (int)assetClass1.HeightU,
                            (int)item2.asset.Location.RackPosition, (int)assetClass2.HeightU))
                        {
                            results[item1.index].AddFailReason(AssetValidationField.RackPosition,
                                $"Rack Position {item1.asset.Location.RackPosition} conflicts with another asset in the validation batch.");
                            results[item2.index].AddFailReason(AssetValidationField.RackPosition,
                                $"Rack Position {item2.asset.Location.RackPosition} conflicts with another asset in the validation batch.");
                        }
                    }
                }
            }

            return results;
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

                 result.AddFailuresFrom(ValidateAssetPlacement(
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

        private bool DoRangesOverlap(long start1, long end1, long start2, long end2)
        {
            return start1 <= end2 && end1 >= start2;
        }

        #endregion

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
        internal ValidationResult ValidateAssetPlacementWithContext(Asset asset, RackValidationContext context)
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
        internal ValidationResult ValidateRangeOccupancy(
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
            if (!RackPlacementValidation.ValidatePositionAndBounds(rack, position, heightU, out var boundsResult))
            {
                return boundsResult;
            }

            var (startPos, endPos) = RackPlacementValidation.CalculateOccupiedRange(rack.Position, position, heightU);

            // Check asset conflicts
            if (!RackPlacementValidation.CheckAssetConflicts(rack.Position, startPos, endPos, currentAsset, occupiedAssets, out var assetConflict))
            {
                return assetConflict;
            }

            // Check reservation conflicts
            if (!RackPlacementValidation.CheckReservationConflicts(startPos, endPos, currentReservation, reservations, out var reservationConflict))
            {
                return reservationConflict;
            }

            return result;
        }

        #endregion

        #region Data Loading Helpers

        /// <summary>
        /// Loads all assets in a rack (excluding specified asset).
        /// </summary>
        internal List<(Asset Asset, int Position, int HeightU)> LoadAllAssetsInRack(string rackIdentifier, string excludeAssetId = null)
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
        internal List<(InfraopsReservation Reservation, List<(long LowerBound, long UpperBound)> Ranges)> LoadReservationsForRack(
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

    }
}