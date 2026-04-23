namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.AssetManagement.Common.Validation;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.AssetManagement.Repositories;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    using static Skyline.DataMiner.SDM.AssetManagement.Common.Validation.AssetValidationHandler;

    /// <summary>
    /// Public validator service for Asset validation with comprehensive error handling.
    /// </summary>
    public class AssetValidator
    {
        private readonly IAssetQueryRepository _assetRepository;
        private readonly IAssetClassQueryRepository _assetClassRepository;
        private readonly IDeviceTypeQueryRepository _deviceTypeRepository;
        private readonly IDataPortQueryRepository _dataPortRepository;
        private readonly IPowerPortQueryRepository _powerPortRepository;
        private readonly IRackQueryRepository _rackRepository;
        private readonly Validator<Asset> _validationPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetValidator"/> class.
        /// </summary>
        /// <param name="assetRepository">Repository for querying assets.</param>
        /// <param name="assetClassRepository">Repository for querying asset classes.</param>
        /// <param name="deviceTypeRepository">Repository for querying device types.</param>
        /// <param name="dataPortRepository">Repository for querying data ports (optional).</param>
        /// <param name="powerPortRepository">Repository for querying power ports (optional).</param>
        public AssetValidator(
            IAssetQueryRepository assetRepository,
            IAssetClassQueryRepository assetClassRepository,
            IDeviceTypeQueryRepository deviceTypeRepository,
            IDataPortQueryRepository dataPortRepository = null,
            IPowerPortQueryRepository powerPortRepository = null,
            IRackQueryRepository rackRepository = null)
        {
            _assetRepository = assetRepository ?? throw new ArgumentNullException(nameof(assetRepository));
            _assetClassRepository = assetClassRepository ?? throw new ArgumentNullException(nameof(assetClassRepository));
            _deviceTypeRepository = deviceTypeRepository ?? throw new ArgumentNullException(nameof(deviceTypeRepository));
            _dataPortRepository = dataPortRepository;
            _powerPortRepository = powerPortRepository;

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

            if (_dataPortRepository == null)
            {
                return result; // Repository not provided, skip validation
            }

            try
            {
                // Query DataPorts for this Asset
                var dataPorts = LoadDataPorts(asset);

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

            if (_powerPortRepository == null)
            {
                return result; // Repository not provided, skip validation
            }

            try
            {
                // Query PowerPorts for this Asset
                var powerPorts = LoadPowerPorts(asset);

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
                .Create(ValidateCriticalFields)
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

        private ValidationResult ValidateCriticalFields(Asset asset)
        {
            var result = new ValidationResult();

            // Name is critical - must be valid before other checks
            if (asset.NameField.Changed)
            {
                result.AddFailuresFrom(IsAssetNameValid(asset));
            }

            // Asset ID is critical
            if (asset.AssetIDField.Changed)
            {
                result.AddFailuresFrom(IsAssetIdValid(asset.AssetID, new List<string> { asset.Identifier }));
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

        private ValidationResult ValidateSerialNumber(Asset asset)
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

            // Check uniqueness within same AssetClass
            if (IsSerialNumberInUse(asset.SerialNumber, asset.AssetClassId, new List<string> { asset.Identifier }))
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
            if (!AssetValidationHandler.CanEditLocation(asset))
            {
                if (asset.Location.ParentAssetField.Changed ||
                    asset.Location.HolderNumberField.Changed ||
                    asset.Location.RackIdField.Changed ||
                    asset.Location.RackPositionField.Changed ||
                    asset.Location.SideField.Changed ||
                    asset.Location.DeskIdField.Changed ||
                    asset.Location.ContainerIdField.Changed ||
                    asset.Location.RoomIdField.Changed ||
                    asset.Location.PowerSupplyRackPositionField.Changed)
                {
                    result.AddFailReason(AssetValidationField.Asset, "Cannot change Location in current State.");
                    return result;
                }

                return result;
            }

            // Validate single location type
            if (!AssetValidationHandler.HasSingleLocation(asset, out var singleLocationResult))
            {
                result.AddFailuresFrom(singleLocationResult);
            }

            // Parent Asset + Holder validation
            if ((asset.Location.ParentAssetField.Changed || asset.Location.HolderNumberField.Changed)
                && asset.AssetClassId.HasValue())
            {
                try
                {
                    var assetClass = LoadAssetClass(asset.AssetClassId);
                    if (assetClass != null)
                    {
                        // Pure logic validation (no data access)
                        if (!AssetValidationHandler.IsParentAssetHolderValid(asset, assetClass, out var parentResult))
                        {
                            result.AddFailuresFrom(parentResult);
                        }

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

            // Rack validation
            if ((asset.Location.RackIdField.Changed ||
                 asset.Location.RackPositionField.Changed ||
                 asset.Location.SideField.Changed)
                && asset.AssetClassId.HasValue())
            {
                try
                {
                    var assetClass = LoadAssetClass(asset.AssetClassId);
                    if (assetClass != null)
                    {
                        if (!AssetValidationHandler.IsRackPositionValid(asset, assetClass, out var rackResult))
                        {
                            result.AddFailuresFrom(rackResult);
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.AddFailReason(AssetValidationField.RackId,
                        $"Error validating rack position: {ex.Message}");
                }
            }

            return result;
        }

        private ValidationResult ValidateDestinationLocation(Asset asset)
        {
            var result = new ValidationResult();

            // Check if destination location can be edited based on state
            if (!AssetValidationHandler.CanEditDestinationLocation(asset))
            {
                if (asset.DestinationLocation.ParentAssetField.Changed ||
                    asset.DestinationLocation.HolderNumberField.Changed ||
                    asset.DestinationLocation.RackIdField.Changed ||
                    asset.DestinationLocation.RackPositionField.Changed ||
                    asset.DestinationLocation.SideField.Changed ||
                    asset.DestinationLocation.DeskIdField.Changed ||
                    asset.DestinationLocation.ContainerIdField.Changed ||
                    asset.DestinationLocation.RoomIdField.Changed ||
                    asset.DestinationLocation.PowerSupplyRackPositionField.Changed)
                {
                    result.AddFailReason(AssetValidationField.Asset, "Cannot change Destination Location in current State.");
                    return result;
                }

                return result;
            }

            // Validate single destination location type
            if (!AssetValidationHandler.HasSingleDestinationLocation(asset, out var singleLocationResult))
            {
                result.AddFailuresFrom(singleLocationResult);
            }

            // Destination Parent Asset + Holder validation
            if ((asset.DestinationLocation.ParentAssetField.Changed || asset.DestinationLocation.HolderNumberField.Changed)
                && asset.AssetClassId.HasValue())
            {
                try
                {
                    var assetClass = LoadAssetClass(asset.AssetClassId);
                    if (assetClass != null)
                    {
                        if (!AssetValidationHandler.IsDestinationParentAssetHolderValid(asset, assetClass, out var parentResult))
                        {
                            result.AddFailuresFrom(parentResult);
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.AddFailReason(AssetValidationField.DestinationParentAsset,
                        $"Error validating destination parent asset: {ex.Message}");
                }
            }

            // Destination Rack validation
            if ((asset.DestinationLocation.RackIdField.Changed ||
                 asset.DestinationLocation.RackPositionField.Changed ||
                 asset.DestinationLocation.SideField.Changed)
                && asset.AssetClassId.HasValue())
            {
                try
                {
                    var assetClass = LoadAssetClass(asset.AssetClassId);
                    if (assetClass != null)
                    {
                        if (!AssetValidationHandler.IsDestinationRackPositionValid(asset, assetClass, out var rackResult))
                        {
                            result.AddFailuresFrom(rackResult);
                        }
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

        private AssetClass LoadAssetClass(SdmObjectReference<AssetClass> reference)
        {
            if (reference == null || !reference.HasValue())
            {
                return null;
            }

            try
            {
                var filter = AssetClassExposers.Identifier.Equal(reference.Identifier);
                return _assetClassRepository.Read(filter).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load AssetClass: {ex.Message}", ex);
            }
        }

        private List<DataPort> LoadDataPorts(Asset asset)
        {
            if (_dataPortRepository == null || asset == null || string.IsNullOrEmpty(asset.Identifier))
            {
                return new List<DataPort>();
            }

            try
            {
                var filter = DataPortExposers.Asset.Equal(asset);
                return _dataPortRepository.Read(filter).ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load DataPorts: {ex.Message}", ex);
            }
        }

        private List<PowerPort> LoadPowerPorts(Asset asset)
        {
            if (_powerPortRepository == null || asset == null || string.IsNullOrEmpty(asset.Identifier))
            {
                return new List<PowerPort>();
            }

            try
            {
                var filter = PowerPortExposers.Asset.Equal(asset);
                return _powerPortRepository.Read(filter).ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load PowerPorts: {ex.Message}", ex);
            }
        }

        private bool IsNameInUse(string name, List<string> exceptIdentifiers = null)
        {
            FilterElement<Asset> filter = AssetExposers.AssetName.Equal(name);

            if (exceptIdentifiers != null && exceptIdentifiers.Any())
            {
                var clauses = exceptIdentifiers
                    .Select(id => AssetExposers.Identifier.NotEqual(id))
                    .Cast<FilterElement<Asset>>()
                    .ToArray();
                filter = filter.AND(new ANDFilterElement<Asset>(clauses));
            }

            return _assetRepository.Count(filter) > 0;
        }

        private bool IsAssetIdInUse(string assetId, List<string> exceptIdentifiers = null)
        {
            FilterElement<Asset> filter = AssetExposers.AssetId.Equal(assetId);

            if (exceptIdentifiers != null && exceptIdentifiers.Any())
            {
                var clauses = exceptIdentifiers
                    .Select(id => AssetExposers.Identifier.NotEqual(id))
                    .Cast<FilterElement<Asset>>()
                    .ToArray();
                filter = filter.AND(new ANDFilterElement<Asset>(clauses));
            }

            return _assetRepository.Count(filter) > 0;
        }

        private bool IsSerialNumberInUse(string serialNumber, SdmObjectReference<AssetClass> assetClassId, List<string> exceptIdentifiers = null)
        {
            if (string.IsNullOrWhiteSpace(serialNumber) || assetClassId == null || !assetClassId.HasValue())
            {
                return false;
            }

            FilterElement<Asset> filter = AssetExposers.SerialNumber.Equal(serialNumber)
                .AND(AssetExposers.AssetClass.Equal(assetClassId));

            if (exceptIdentifiers != null && exceptIdentifiers.Any())
            {
                var clauses = exceptIdentifiers
                    .Select(id => AssetExposers.Identifier.NotEqual(id))
                    .Cast<FilterElement<Asset>>()
                    .ToArray();
                filter = filter.AND(new ANDFilterElement<Asset>(clauses));
            }

            return _assetRepository.Count(filter) > 0;
        }

        // Add this new method to AssetValidator class

        /// <summary>
        /// Validates parent asset holder availability - requires loading parent asset from repository.
        /// Checks if the parent asset has an available holder matching the hierarchy role and slot number.
        /// </summary>
        private ValidationResult ValidateParentAssetHolderAvailability(Asset asset, AssetClass assetClass)
        {
            var result = new ValidationResult();

            if (!asset.Location.ParentAsset.HasValue() || asset.Location.HolderNumber == null)
            {
                return result; // Basic validation already done in handler
            }

            try
            {
                // Load the parent asset from repository
                var parentAssetFilter = AssetExposers.Identifier.Equal(asset.Location.ParentAsset.Identifier);
                var parentAsset = _assetRepository.Read(parentAssetFilter).FirstOrDefault();

                if (parentAsset == null)
                {
                    result.AddFailReason(AssetValidationField.ParentAsset,
                        "Parent Asset not found.");
                    return result;
                }

                // Get hierarchy role from device type
                var deviceTypeFilter = DeviceTypeExposers.Identifier.Equal(assetClass.DeviceTypeId.Identifier);
                var deviceType = _deviceTypeRepository.Read(deviceTypeFilter).FirstOrDefault();

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

                // Check if the holder slot is already occupied by a different asset
                var occupiedFilter = AssetExposers.Location.ParentAsset.Equal(parentAsset)
                    .AND(AssetExposers.Identifier.NotEqual(asset.Identifier));

                var occupyingAssets = _assetRepository.Read(occupiedFilter)
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
        private ValidationResult ValidateRackSpaceAvailability(Asset asset, AssetClass assetClass)
        {
            var result = new ValidationResult();

            if (asset.Location.RackId == default || asset.Location.RackPosition == null)
            {
                return result; // Basic validation already done in handler
            }

            try
            {
                // Load the rack from repository (assuming IRackQueryRepository exists)
                // NOTE: You'll need to add IRackQueryRepository to the constructor
                // For now, this is a placeholder showing the pattern:

                // var rackFilter = RackExposers.Identifier.Equal(asset.Location.RackId);
                // var rack = _rackRepository.Read(rackFilter).FirstOrDefault();

                // if (rack == null)
                // {
                //     result.AddFailReason(AssetValidationField.RackId, "Rack not found.");
                //     return result;
                // }

                // // Check if position is within rack bounds
                // if (asset.Location.RackPosition > rack.RackUnits)
                // {
                //     result.AddFailReason(AssetValidationField.RackPosition,
                //         $"Invalid Position: Must be within Rack (max {rack.RackUnits} units).");
                //     return result;
                // }

                // // Validate rack space availability
                // if (!RackValidationHandler.ValidateRackSpace(rack, asset, (int)asset.Location.RackPosition, (int)assetClass.HeightU, out var rackSpaceResult))
                // {
                //     result.AddFailuresFrom(rackSpaceResult);
                // }

                // TODO: Implement when IRackQueryRepository is available
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
        private ValidationResult ValidateDestinationRackSpaceAvailability(Asset asset, AssetClass assetClass)
        {
            var result = new ValidationResult();

            if (asset.DestinationLocation.RackId == default || asset.DestinationLocation.RackPosition == null)
            {
                return result; // Basic validation already done in handler
            }

            try
            {
                // Load the destination rack from repository
                // Same pattern as ValidateRackSpaceAvailability

                // TODO: Implement when IRackQueryRepository is available
            }
            catch (Exception ex)
            {
                result.AddFailReason(AssetValidationField.DestinationRackId,
                    $"Error validating destination rack space: {ex.Message}");
            }

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
                    var assetClass = LoadAssetClass(asset.AssetClassId);
                    if (assetClass != null)
                    {
                        // Pure logic validation (basic checks)
                        if (!AssetValidationHandler.IsRackPositionValid(asset, assetClass, out var rackResult))
                        {
                            result.AddFailuresFrom(rackResult);
                        }

                        // Advanced validation (rack space availability - requires Rack repository)
                        result.AddFailuresFrom(ValidateRackSpaceAvailability(asset, assetClass));
                    }
                }
                catch (Exception ex)
                {
                    result.AddFailReason(AssetValidationField.RackId,
                        $"Error validating rack position: {ex.Message}");
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
                    var assetClass = LoadAssetClass(asset.AssetClassId);
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

        #endregion
    }
}