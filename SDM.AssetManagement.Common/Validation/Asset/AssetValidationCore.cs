namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.SDM.AssetManagement.Models;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM.AssetManagement.Common.Validation;
    using Skyline.DataMiner.SDM.Common.Services;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM.InfraOps.Common.Validation;
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
                .Create(ValidateLocationBusinessRules)
                .AndThen(ValidateDestinationLocationBusinessRules)
                .AndThen(ValidateLifecycle)
                .AndThen(ValidateOwnershipAndCustody)
                .AndThen(ValidateCollections);

            return assetClassValidation.AndThen(businessRules);
        }

        /// <summary>
        /// Validates asset class (critical - stops pipeline on failure).
        /// </summary>
        private ValidationResult ValidateAssetClass(Asset asset)
        {
            var result = new ValidationResult();

            if (asset.ShouldValidate(asset.AssetClassIdField)
                && !AssetValidationHandler.IsAssetClassValid(asset, out var assetClassResult))
            {
                result.AddFailuresFrom(assetClassResult);
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
            if (asset.ShouldValidateAny(asset.Location.ParentAssetField, asset.Location.HolderNumberField)
                && asset.AssetClassId.HasValue()
                && !AssetValidationHandler.IsParentAssetHolderValid(asset, out var parentResult))
                validations.Add(parentResult);

            // Rack position - basic logic validation only
            if (asset.ShouldValidateAny(asset.Location.RackIdField,
                 asset.Location.RackPositionField,
                 asset.Location.SideField)
                && asset.AssetClassId.HasValue()
                && !AssetValidationHandler.IsRackPositionValid(asset, out var rackResult))
                validations.Add(rackResult);

            return validations.MergeAll();
        }

        /// <summary>
        /// Checks if any Location field has changed.
        /// </summary>
        private bool HasLocationChanged(Asset asset)
        {
            if (asset.Location.IsEmpty)
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
                   asset.Location.RoomIdField.Changed;
        }

        /// <summary>
        /// Checks if any DestinationLocation field has changed.
        /// </summary>
        private bool HasDestinationLocationChanged(Asset asset)
        {
            if (asset.DestinationLocation.IsEmpty)
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
                   asset.DestinationLocation.RoomIdField.Changed;
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

            if (!result.IsValid)
            {
                return result; // If mandatory validation fails, return immediately without further business rules checks
            }

            // DestinationLocation business rules validation ONLY applies when state is InTransit
            if (asset.State != SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InTransit)
            {
                return result; // Ignore DestinationLocation in all other states
            }

            // After checks pass, collect all destination location errors
            var validations = new List<ValidationResult> { result };

            // Single destination location type
            if (!AssetValidationHandler.HasSingleDestinationLocation(asset, out var singleLocationResult))
            {
                validations.Add(singleLocationResult);
            }

            // Destination parent asset holder - basic logic validation only
            if (asset.ShouldValidateAny(asset.DestinationLocation.ParentAssetField, asset.DestinationLocation.HolderNumberField)
                && asset.AssetClassId.HasValue()
                && !AssetValidationHandler.IsDestinationParentAssetHolderValid(asset, out var parentResult))
                validations.Add(parentResult);

            var assetClass = _entityLoader.LoadAssetClass(asset.AssetClassId);

            // Destination rack position - basic logic validation only
            if (asset.ShouldValidateAny(asset.DestinationLocation.RackIdField,
                 asset.DestinationLocation.RackPositionField,
                 asset.DestinationLocation.SideField)
                && asset.AssetClassId.HasValue()
                && !AssetValidationHandler.IsDestinationRackPositionValid(asset, assetClass, out var rackResult))
                validations.Add(rackResult);

            return validations.MergeAll();
        }

        private ValidationResult ValidateLifecycle(Asset asset)
        {
            var validations = new List<ValidationResult>();

            if (asset.ShouldValidateAny(asset.InstallationUserIdField, asset.InstallationDateField)
                && !AssetValidationHandler.IsInstallationInformationChangeAllowed(asset, out var installationChangeResult))
                validations.Add(installationChangeResult);

            if (asset.ShouldValidateAny(asset.InstallationUserIdField, asset.InstallationDateField)
                && !AssetValidationHandler.IsInstallationInfoValid(asset, out var installationResult))
                validations.Add(installationResult);

            if (asset.ShouldValidateAny(asset.ModificationUserIdField, asset.ModificationDateField)
                && !AssetValidationHandler.IsModificationInfoValid(asset, out var modificationResult))
                validations.Add(modificationResult);

            return validations.MergeAll();
        }

        private ValidationResult ValidateOwnershipAndCustody(Asset asset)
        {
            var validations = new List<ValidationResult>();

            if (asset.ShouldValidate(asset.Ownership)
                && !AssetValidationHandler.IsOwnershipValid(asset, out var ownerResult))
                validations.Add(ownerResult);

            if (asset.ShouldValidate(asset.Custody)
                && !AssetValidationHandler.IsCustodyValid(asset, out var custodyResult))
                validations.Add(custodyResult);

            return validations.MergeAll();
        }

        private ValidationResult ValidateCollections(Asset asset)
        {
            var validations = new List<ValidationResult>();

            if (asset.ShouldValidate(asset.HoldersField))
            {
                validations.Add(AssetValidationHandler.ValidateAssetHolders(asset));
            }

            if (asset.ShouldValidate(asset.ElementsField))
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
        public ValidationResult ValidateWithDatabaseAccess(Asset asset)
        {
            var validations = new List<ValidationResult>
            {
                ValidateUniquenessChecks(asset),
                ValidateAssetClassState(asset),
                ValidateReferencesAgainstDatabase(asset),
                ValidateLocationPlacement(asset),
            };

            return validations.MergeAll();
        }

        private ValidationResult ValidateAssetClassState(Asset asset)
        {
            var result = new ValidationResult();

            if (!asset.ShouldValidate(asset.AssetClassIdField) || !asset.AssetClassId.HasValue())
            {
                return result;
            }

            var assetClass = _entityLoader.LoadAssetClass(asset.AssetClassId);
            if (assetClass == null)
            {
                result.AddFailReason(AssetValidationField.AssetClass,
                    $"Referenced Asset Class '{asset.AssetClassId.Identifier}' does not exist.");
                return result;
            }

            if (assetClass.State != SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Active)
            {
                result.AddFailReason(AssetValidationField.AssetClass, "Asset Class must be Active.");
            }

            return result;
        }

        /// <summary>
        /// Validates uniqueness constraints (Name, Asset ID, Serial Number).
        /// Queries database to ensure values are not already in use.
        /// </summary>
        private ValidationResult ValidateUniquenessChecks(Asset asset)
        {
            var validations = new List<ValidationResult>();

            // Name uniqueness
            if (asset.ShouldValidate(asset.NameField))
            {
                validations.Add(ValidateNameUniqueness(asset.Name, asset.Identifier));
            }

            // Asset ID uniqueness
            if (asset.ShouldValidate(asset.AssetIDField))
            {
                validations.Add(ValidateAssetIdUniqueness(asset.AssetID, asset.Identifier));
            }

            // Serial number uniqueness
            if (asset.ShouldValidate(asset.SerialNumberField))
            {
                validations.Add(ValidateSerialNumberUniqueness(asset.SerialNumber, asset.AssetClassId, asset.Identifier));
            }

            return validations.MergeAll();
        }

        public List<ValidationResult> ValidateBulkReferencesAgainstDatabase(List<Asset> assets)
        {
            var results = assets.Select(_ => new ValidationResult()).ToList();
            var batchAssetIds = assets.Select(a => a.Identifier).Where(id => !string.IsNullOrWhiteSpace(id)).ToHashSet();

            var assetClassIds = assets
                .Where(a => a.ShouldValidate(a.AssetClassIdField) && a.AssetClassId != null && a.AssetClassId.HasValue())
                .Select(a => a.AssetClassId.Identifier)
                .Distinct()
                .ToList();
            var existingAssetClassIds = _entityLoader.GetAssetClassesByDomIds(assetClassIds).Select(ac => ac.Identifier).ToHashSet();

            var parentAssetIds = assets
                .SelectMany(a => GetLocationReferences(a, includeDestination: true, l => l.ParentAsset))
                .Distinct()
                .ToList();
            var existingAssetIds = _entityLoader.GetAssetsByDomIds(parentAssetIds).Select(a => a.Identifier).Concat(batchAssetIds).ToHashSet();

            var rackIds = assets
                .SelectMany(a => GetLocationReferences(a, includeDestination: true, l => l.RackId))
                .Distinct()
                .ToList();
            var existingRackIds = _entityLoader.GetRacksByDomIds(rackIds).Select(r => r.Identifier).ToHashSet();

            var facilityIds = assets
                .SelectMany(a => GetLocationReferences(a, includeDestination: true, l => l.ContainerId))
                .Distinct()
                .ToList();
            var existingFacilityIds = _entityLoader.GetFacilitiesByDomIds(facilityIds).Select(f => f.Identifier).ToHashSet();

            var roomIds = assets
                .SelectMany(a => GetLocationReferences(a, includeDestination: true, l => l.RoomId))
                .Distinct()
                .ToList();
            var existingRoomIds = _entityLoader.GetRoomsByDomIds(roomIds).Select(r => r.Identifier).ToHashSet();

            var deskIds = assets
                .SelectMany(a => GetDeskIds(a, includeDestination: true))
                .Select(id => id.ToString())
                .Distinct()
                .ToList();
            var existingDeskIds = _entityLoader.GetDesksByDomIds(deskIds).Select(d => d.Identifier).ToHashSet();

            var locationLookups = new LocationReferenceLookups(
                existingAssetIds, existingRackIds, existingFacilityIds, existingRoomIds, existingDeskIds);

            for (int i = 0; i < assets.Count; i++)
            {
                var asset = assets[i];
                AddReferenceFailure(asset.ShouldValidate(asset.AssetClassIdField), asset.AssetClassId, existingAssetClassIds, AssetValidationField.AssetClass, "Asset Class", results[i]);

                 if (asset.State == SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InTransit)
                {
                    ValidateLocationReferences(asset.DestinationLocation, true, locationLookups, results[i]);
                }
                else
                {
                    ValidateLocationReferences(asset.Location, false, locationLookups, results[i]);
                }
            }

            return results;
        }

        private ValidationResult ValidateReferencesAgainstDatabase(Asset asset)
        {
            return ValidateBulkReferencesAgainstDatabase(new List<Asset> { asset })[0];
        }

        /// <summary>
        /// Validates location placement (holder availability, rack space).
        /// Requires database access to load parent assets and racks.
        /// </summary>
        private ValidationResult ValidateLocationPlacement(Asset asset)
        {
            var validations = new List<ValidationResult>();

            if(asset.Location.IsEmpty)
            {
                return new ValidationResult();
            }

            // Parent asset holder availability
            if (asset.ShouldValidateAny(asset.Location.ParentAssetField, asset.Location.HolderNumberField)
                && asset.AssetClassId.HasValue())
            {
                var assetClass = _entityLoader.LoadAssetClass(asset.AssetClassId);
                if (assetClass != null)
                {
                    validations.Add(ValidateParentAssetHolderAvailability(
                        asset, assetClass));
                }
            }

            // Rack space availability
            if (asset.ShouldValidateAny(asset.Location.RackIdField,
                 asset.Location.RackPositionField,
                 asset.Location.SideField)
                && asset.AssetClassId.HasValue())
            {
                validations.Add(ValidateRackSpaceAvailability(asset));
            }

            return validations.MergeAll();
        }

        public ValidationResult ValidateNameUniqueness(string name, string exceptIdentifier = null)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(name))
            {
                result.AddFailReason(AssetValidationField.Name,
                    "Asset Name cannot be empty or whitespace.");
                return result;
            }

            if (_entityLoader.CountAssetsByName(name, exceptIdentifier) > 0)
            {
                result.AddFailReason(AssetValidationField.Name,
                    $"Asset Name '{name}' is already in use.");
            }

            return result;
        }

        public ValidationResult ValidateAssetIdUniqueness(string assetId, string exceptIdentifier = null)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(assetId))
            {
                result.AddFailReason(AssetValidationField.AssetId,
                    "Asset ID cannot be empty or whitespace.");
                return result;
            }

            if (_entityLoader.CountAssetsByAssetId(assetId, exceptIdentifier) > 0)
            {
                result.AddFailReason(AssetValidationField.AssetId,
                    $"Asset ID '{assetId}' is already in use.");
            }

            return result;
        }

        private ValidationResult ValidateSerialNumberUniqueness(string serialNumber,
            SdmObjectReference<AssetClass> assetClassId, string exceptIdentifier = null)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(serialNumber) || !assetClassId.HasValue())
            {
                return result;
            }

            if (_entityLoader.CountAssetsBySerialNumber(serialNumber, assetClassId, exceptIdentifier) > 0)
            {
                result.AddFailReason(AssetValidationField.SerialNumber,
                    "Serial Number is already in use for this Asset Class.");
            }

            return result;
        }

        #endregion

        #region Bulk-Specific Validation

        /// <summary>
        /// Phase 2.5a: Bulk Asset Name uniqueness check against the database.
        /// Uses a single OR query via <see cref="Tools.RetrieveBigOrFilter"/>; batch IDs excluded in memory.
        /// </summary>
        public List<ValidationResult> ValidateBulkNameUniquenessAgainstDatabase(List<Asset> assets)
        {
            var batchIds = new HashSet<string>(
                assets.Select(a => a.Identifier).Where(id => !string.IsNullOrWhiteSpace(id)));
            return ValidateBulkSimpleFieldUniquenessAgainstDatabase(
                assets,
                batchIds,
                a => a.Name,
                _entityLoader.GetAssetsByNames,
                AssetValidationField.Name,
                "Asset Name '{0}' is already in use.");
        }

        /// <summary>
        /// Phase 2.5b: Bulk Asset ID uniqueness check against the database.
        /// Uses a single OR query via <see cref="Tools.RetrieveBigOrFilter"/>; batch IDs excluded in memory.
        /// </summary>
        public List<ValidationResult> ValidateBulkAssetIdUniquenessAgainstDatabase(List<Asset> assets)
        {
            var batchIds = new HashSet<string>(
                assets.Select(a => a.Identifier).Where(id => !string.IsNullOrWhiteSpace(id)));
            return ValidateBulkSimpleFieldUniquenessAgainstDatabase(
                assets,
                batchIds,
                a => a.AssetID,
                _entityLoader.GetAssetsByAssetIds,
                AssetValidationField.AssetId,
                "Asset ID '{0}' is already in use.");
        }

        /// <summary>
        /// Phase 2.5c: Bulk Serial Number uniqueness check against the database, scoped per AssetClass.
        /// Groups assets by AssetClass and runs one OR query per class via <see cref="Tools.RetrieveBigOrFilter"/>.
        /// Batch IDs excluded in memory.
        /// </summary>
        public List<ValidationResult> ValidateBulkSerialNumberUniquenessAgainstDatabase(List<Asset> assets)
        {
            var results = assets.Select(_ => new ValidationResult()).ToList();

            var batchIds = new HashSet<string>(
                assets.Select(a => a.Identifier).Where(id => !string.IsNullOrWhiteSpace(id)));

            var assetsByClass = assets
                .Select((a, idx) => new { Asset = a, Index = idx })
                .Where(x => !string.IsNullOrWhiteSpace(x.Asset.SerialNumber) && x.Asset.AssetClassId.HasValue())
                .GroupBy(x => x.Asset.AssetClassId.Identifier);

            foreach (var classGroup in assetsByClass)
            {
                var assetClassRef = classGroup.First().Asset.AssetClassId;

                var uniqueSerials = classGroup
                    .Select(x => x.Asset.SerialNumber)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var dbSerialMatches = _entityLoader.GetAssetsBySerialNumbers(assetClassRef, uniqueSerials);

                var externalSerialConflicts = dbSerialMatches
                    .Where(a => !batchIds.Contains(a.Identifier))
                    .Select(a => a.SerialNumber)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                foreach (var item in classGroup)
                {
                    if (externalSerialConflicts.Contains(item.Asset.SerialNumber))
                    {
                        results[item.Index].AddFailReason(AssetValidationField.SerialNumber,
                            "Serial Number is already in use for this Asset Class.");
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Generic helper for bulk single-field string uniqueness checks.
        /// Extracts unique non-empty values, queries DB via OR filter, excludes batch IDs in memory.
        /// Covers Name and AssetId; SerialNumber uses its own implementation due to AssetClass scoping.
        /// </summary>
        private List<ValidationResult> ValidateBulkSimpleFieldUniquenessAgainstDatabase(
            List<Asset> assets,
            HashSet<string> batchIds,
            Func<Asset, string> getValue,
            Func<List<string>, List<Asset>> dbQuery,
            AssetValidationField field,
            string errorMessageTemplate)
        {
            var results = assets.Select(_ => new ValidationResult()).ToList();

            var uniqueValues = assets
                .Select(getValue)
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!uniqueValues.Any())
            {
                return results;
            }

            var dbMatches = dbQuery(uniqueValues);
            var externalConflicts = dbMatches
                .Where(a => !batchIds.Contains(a.Identifier))
                .Select(getValue)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < assets.Count; i++)
            {
                var value = getValue(assets[i]);
                if (!string.IsNullOrWhiteSpace(value) && externalConflicts.Contains(value))
                {
                    results[i].AddFailReason(field, string.Format(errorMessageTemplate, value));
                }
            }

            return results;
        }

        private static IEnumerable<string> GetLocationReferences(Asset asset, bool includeDestination, Func<AssetLocation, SdmObjectReference<Asset>> selector)
        {
            return GetLocations(asset, includeDestination)
                .Select(selector)
                .Where(reference => reference != null && reference.HasValue())
                .Select(reference => reference.Identifier);
        }

        private static IEnumerable<string> GetLocationReferences(Asset asset, bool includeDestination, Func<AssetLocation, SdmObjectReference<Rack>> selector)
        {
            return GetLocations(asset, includeDestination)
                .Select(selector)
                .Where(reference => reference != null && reference.HasValue())
                .Select(reference => reference.Identifier);
        }

        private static IEnumerable<string> GetLocationReferences(Asset asset, bool includeDestination, Func<AssetLocation, SdmObjectReference<Facility>> selector)
        {
            return GetLocations(asset, includeDestination)
                .Select(selector)
                .Where(reference => reference != null && reference.HasValue())
                .Select(reference => reference.Identifier);
        }

        private static IEnumerable<string> GetLocationReferences(Asset asset, bool includeDestination, Func<AssetLocation, SdmObjectReference<Room>> selector)
        {
            return GetLocations(asset, includeDestination)
                .Select(selector)
                .Where(reference => reference != null && reference.HasValue())
                .Select(reference => reference.Identifier);
        }

        private static IEnumerable<Guid> GetDeskIds(Asset asset, bool includeDestination)
        {
            return GetLocations(asset, includeDestination)
                .Select(location => location.DeskId)
                .Where(id => id != Guid.Empty);
        }

        private static IEnumerable<AssetLocation> GetLocations(Asset asset, bool includeDestination)
        {
            if (!asset.Location.IsEmpty)
            {
                yield return asset.Location;
            }

            if (includeDestination
                && asset.State == SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InTransit
                && !asset.DestinationLocation.IsEmpty)
            {
                yield return asset.DestinationLocation;
            }
        }

        private static void AddReferenceFailure<T>(
            bool shouldValidate,
            SdmObjectReference<T> reference,
            HashSet<string> existingIds,
            AssetValidationField field,
            string targetName,
            ValidationResult result)
            where T : SdmObject<T>
        {
            if (shouldValidate && reference != null && reference.HasValue() && !existingIds.Contains(reference.Identifier))
            {
                result.AddFailReason(field, $"Referenced {targetName} '{reference.Identifier}' does not exist.");
            }
        }

        private static void ValidateLocationReferences(
            AssetLocation location,
            bool isDestination,
            LocationReferenceLookups lookups,
            ValidationResult result)
        {
            if (location == null)
            {
                return;
            }

            AddLocationReferenceFailure(location.ParentAsset, lookups.AssetIds, isDestination ? AssetValidationField.DestinationParentAsset : AssetValidationField.ParentAsset, "Asset", result);
            AddLocationReferenceFailure(location.RackId, lookups.RackIds, isDestination ? AssetValidationField.DestinationRackId : AssetValidationField.RackId, "Rack", result);
            AddLocationReferenceFailure(location.ContainerId, lookups.FacilityIds, isDestination ? AssetValidationField.DestinationContainerId : AssetValidationField.ContainerId, "Facility", result);
            AddLocationReferenceFailure(location.RoomId, lookups.RoomIds, isDestination ? AssetValidationField.DestinationRoomId : AssetValidationField.RoomId, "Room", result);

            if (location.DeskId != Guid.Empty && !lookups.DeskIds.Contains(location.DeskId.ToString()))
            {
                result.AddFailReason(isDestination ? AssetValidationField.DestinationDeskId : AssetValidationField.DeskId,
                    $"Referenced Desk '{location.DeskId}' does not exist.");
            }
        }

        private sealed class LocationReferenceLookups
        {
            public LocationReferenceLookups(
                HashSet<string> assetIds,
                HashSet<string> rackIds,
                HashSet<string> facilityIds,
                HashSet<string> roomIds,
                HashSet<string> deskIds)
            {
                AssetIds = assetIds;
                RackIds = rackIds;
                FacilityIds = facilityIds;
                RoomIds = roomIds;
                DeskIds = deskIds;
            }

            public HashSet<string> AssetIds { get; }

            public HashSet<string> RackIds { get; }

            public HashSet<string> FacilityIds { get; }

            public HashSet<string> RoomIds { get; }

            public HashSet<string> DeskIds { get; }
        }

        private static void AddLocationReferenceFailure<T>(
            SdmObjectReference<T> reference,
            HashSet<string> existingIds,
            AssetValidationField field,
            string targetName,
            ValidationResult result)
            where T : SdmObject<T>
        {
            if (reference != null && reference.HasValue() && !existingIds.Contains(reference.Identifier))
            {
                result.AddFailReason(field, $"Referenced {targetName} '{reference.Identifier}' does not exist.");
            }
        }

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

            ValidateNameDuplicatesInBatch(assets, results);

            ValidateAssetIdDuplicatesInBatch(assets, results);

            ValidateSerialNumberDuplicatesInBatch(assets, results);

            // Parent holder conflicts
            ValidateHolderConflictsInBatch(assets, results);

            // Rack position overlaps (optimized)
            ValidateRackConflictsInBatch(assets, results);

            return results;
        }

        private void ValidateRackConflictsInBatch(List<Asset> assets, List<ValidationResult> results)
        {
            var rackConflicts = ValidateRackPositionOverlaps(assets);

            for (int i = 0; i < assets.Count; i++)
            {
                results[i].AddFailuresFrom(rackConflicts[i]);
            }
        }

        private static void ValidateHolderConflictsInBatch(List<Asset> assets, List<ValidationResult> results)
        {
            var holderGroups = assets
                            .Select((asset, index) => new { asset, index })
                            .Where(x => x.asset.Location.ParentAsset.HasValue() == true && x.asset.Location.HolderNumber != null)
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
        }

        private static void ValidateSerialNumberDuplicatesInBatch(List<Asset> assets, List<ValidationResult> results)
        {
            var serialGroups = assets
                            .Select((asset, index) => new { asset, index })
                            .Where(x => x.asset.ShouldValidate(x.asset.SerialNumberField) &&
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
        }

        private static void ValidateAssetIdDuplicatesInBatch(List<Asset> assets, List<ValidationResult> results)
        {
            var assetIdGroups = assets
                            .Select((asset, index) => new { asset, index })
                            .Where(x => x.asset.ShouldValidate(x.asset.AssetIDField) && !string.IsNullOrWhiteSpace(x.asset.AssetID))
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
        }

        private static void ValidateNameDuplicatesInBatch(List<Asset> assets, List<ValidationResult> results)
        {
            var nameGroups = assets
                .Select((asset, index) => new { asset, index })
                .Where(x => x.asset.ShouldValidate(x.asset.NameField) && !string.IsNullOrWhiteSpace(x.asset.Name))
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
        }

        /// <summary>
        /// Validates rack position conflicts within a batch.
        /// Returns a list of validation results in the same order as input assets.
        /// </summary>
        private List<ValidationResult> ValidateRackPositionOverlaps(List<Asset> assets)
        {
            // Initialize results - same order as input
            var results = assets.Select(a => new ValidationResult()).ToList();

            var rackGroups = assets
                .Select((asset, index) => (asset, index))
                .Where(x => x.asset.Location.RackId.HasValue() && x.asset.Location.RackPosition.HasValue && x.asset.Location.RackPosition.Value > 0)
                .GroupBy(x => x.asset.Location.RackId);

            foreach (var rackGroup in rackGroups)
                ProcessRackGroup(rackGroup, results);

            return results;
        }

        private void ProcessRackGroup(IGrouping<SdmObjectReference<Rack>, (Asset asset, int index)> rackGroup, List<ValidationResult> results)
        {
            var assetsInRack = rackGroup.ToList();
            if (assetsInRack.Count < 2) return;

            // Load rack to get Position enum (needed for overlap calculation)
            var rack = _entityLoader.LoadRack(rackGroup.Key);
            if (rack == null) return;

            for (int i = 0; i < assetsInRack.Count; i++)
            {
                var item1 = assetsInRack[i];
                var assetClass1 = _entityLoader.LoadAssetClass(item1.asset.AssetClassId);
                if (assetClass1 == null || assetClass1.HeightU <= 0) continue;

                for (int j = i + 1; j < assetsInRack.Count; j++)
                    CheckAndRecordOverlap(item1, assetClass1, assetsInRack[j], rack, results);
            }
        }

        private void CheckAndRecordOverlap((Asset asset, int index) item1, AssetClass assetClass1, (Asset asset, int index) item2, Rack rack, List<ValidationResult> results)
        {
            var assetClass2 = _entityLoader.LoadAssetClass(item2.asset.AssetClassId);
            if (assetClass2 == null || assetClass2.HeightU <= 0) return;

            if (!RackPlacementValidation.DoAssetsOverlap(
                rack.Position.Value,
                (int)item1.asset.Location.RackPosition,
                (int)assetClass1.HeightU,
                (int)item2.asset.Location.RackPosition,
                (int)assetClass2.HeightU))
                return;

            results[item1.index].AddFailReason(AssetValidationField.RackPosition,
                $"Rack Position {item1.asset.Location.RackPosition} conflicts with another asset in the validation batch.");
            results[item2.index].AddFailReason(AssetValidationField.RackPosition,
                $"Rack Position {item2.asset.Location.RackPosition} conflicts with another asset in the validation batch.");
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
                var core = new DataPortValidationCore(_entityLoader);
                result.AddFailuresFrom(core.ValidateDataPortCollection(dataPorts));
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
                result.AddFailuresFrom(PortNumberValidator.ValidateCollection(
                    powerPorts, p => p.PowerPortInfo.PortNumber, AssetValidationField.PowerPort, "Power Port"));
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
                result.AddFailuresFrom(PortNumberValidator.ValidateCollection(
                    powerPorts, p => p.PowerPortInfo.PortNumber, AssetValidationField.PowerPort, "Power Port"));
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

        private ValidationResult ValidateParentAssetHolderAvailability(
            Asset asset, AssetClass assetClass)
        {
            var result = new ValidationResult();

            if (!asset.Location.ParentAsset.HasValue() || asset.Location.HolderNumber == null)
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
                if (deviceType?.HierarchyInfo.HierarchyRole == null)
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

                var exceptIds = new List<string> { asset.Identifier };
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

        private ValidationResult ValidateRackSpaceAvailability(Asset asset)
        {
            var result = new ValidationResult();

            if (asset.Location.RackId == default || asset.Location.RackPosition == null)
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
        private ValidationResult ValidateAssetInRack(Asset asset, string rackIdentifier, int position)
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

            var (startPos, endPos) = RackPlacementValidation.CalculateOccupiedRange(rack.Position.Value, position, heightU);

            // Check asset conflicts
            if (!RackPlacementValidation.CheckAssetConflicts(rack.Position.Value, startPos, endPos, currentAsset, occupiedAssets, out var assetConflict))
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

            foreach (var asset in assets.Where(a => a.Location.RackPosition != null))
            {
                try
                {
                    var heightU = GetAssetHeightU(asset);
                    occupationList.Add((asset, (int)asset.Location.RackPosition, heightU));
                }
                catch (InvalidOperationException)
                {
                    // Skip assets with invalid height data
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
                        .Select(p => (p.LowerBound.Value, p.UpperBound.Value))
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