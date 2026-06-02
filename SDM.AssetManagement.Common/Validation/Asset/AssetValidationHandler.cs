namespace Skyline.DataMiner.SDM.AssetManagement.Common.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SharedCommonLibrary.AssetManagement.State_Management;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.AssetManagement.Validation;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Static validation handler for Asset business rules.
    /// Contains pure validation logic without data access.
    /// </summary>
    public static class AssetValidationHandler
    {
        public enum AssetValidationField
        {
            Asset,
            Name,
            AssetId,
            AssetClass,
            SerialNumber,
            Description,
            FwOs,
            HardwareVersion,
            OperationalFlags,

            // Network
            MacAddress,

            // Location
            ParentAsset,
            HolderNumber,
            RackId,
            RackPosition,
            Side,
            DeskId,
            ContainerId,
            RoomId,
            PowerSupplyRackPosition,
            Location,
            DestinationLocation,

            // Destination Location
            DestinationParentAsset,
            DestinationHolderNumber,
            DestinationRackId,
            DestinationRackPosition,
            DestinationSide,
            DestinationDeskId,
            DestinationContainerId,
            DestinationRoomId,
            DestinationPowerSupplyRackPosition,

            // Lifecycle
            InstallationUserId,
            InstallationDate,
            FirstUseDate,
            PurchaseDate,
            ModificationUserId,
            ModificationDate,
            EndOfLifeDate,
            EndOfWarrantyDate,

            // Ownership
            OwnerOrganization,
            OwnerContactPerson,
            OwnerContactPersonRole,
            OwnerTeam,

            // Custody
            CustodyFrom,
            CustodyTill,
            CustodyContactPerson,
            CustodyTeam,
            CustodyOrganization,
            CustodyContactPersonRole,

            // Collections
            DataPort,
            PowerPort,
            Holder,
            Element,
        }

        #region Info Validation

        /// <summary>
        /// Validates that Asset has a valid AssetClass reference.
        /// </summary>
        public static bool IsAssetClassValid(Asset asset, out ValidationResult result)
        {
            result = new ValidationResult();

            if (asset == null)
            {
                result.AddFailReason(AssetValidationField.Asset, "Asset cannot be null.");
                return result.IsValid;
            }

            if (asset.AssetClassId == null || !asset.AssetClassId.HasValue())
            {
                result.AddFailReason(AssetValidationField.AssetClass, "Asset Class cannot be empty.");
                return result.IsValid;
            }

            return result.IsValid;
        }

        #endregion

        #region Location Validation

        /// <summary>
        /// Validates that only one type of location is attached to the asset.
        /// </summary>
        public static bool HasSingleLocation(Asset asset, out ValidationResult result)
        {
            result = new ValidationResult();

            if (asset == null)
            {
                result.AddFailReason(AssetValidationField.Asset, "Asset cannot be null.");
                return result.IsValid;
            }

            bool[] locationExists = new bool[]
            {
                asset.Location?.ParentAsset != null && asset.Location.ParentAsset.HasValue(),
                asset.Location?.RackId != default && asset.Location.RackId.HasValue(),
                 asset.Location?.DeskId != null && asset.Location.DeskId != System.Guid.Empty,
                asset.Location?.ContainerId != null && asset.Location.ContainerId.HasValue(),
                asset.Location?.RoomId != default && asset.Location.RoomId.HasValue(),
            };

            if (locationExists.Count(entry => entry) > 1)
            {
                result.AddFailReason(AssetValidationField.Asset, "Has multiple Locations attached.");
                return result.IsValid;
            }

            return result.IsValid;
        }

        /// <summary>
        /// Validates parent asset holder relationship (pure logic, no data access).
        /// </summary>
        public static bool IsParentAssetHolderValid(Asset asset, out ValidationResult result)
        {
            result = new ValidationResult();

            if (asset == null)
            {
                result.AddFailReason(AssetValidationField.Asset, "Asset cannot be null.");
                return result.IsValid;
            }

            var hasParentAsset = asset.Location?.ParentAsset != null && asset.Location.ParentAsset.HasValue();
            var hasHolderNumber = asset.Location?.HolderNumber != null;

            // If no parent asset, holder number must not be set
            if (!hasParentAsset && hasHolderNumber)
            {
                result.AddFailReason(AssetValidationField.HolderNumber,
                    "Holder Number cannot be set when there is no Parent Asset.");
                return result.IsValid;
            }

            // If parent asset is set, holder number must also be set
            if (hasParentAsset && !hasHolderNumber)
            {
                result.AddFailReason(AssetValidationField.HolderNumber,
                    "Holder Number must be set when Parent Asset is provided.");
                return result.IsValid;
            }

            // Validate holder number is not negative
            if (hasHolderNumber && asset.Location.HolderNumber < 0)
            {
                result.AddFailReason(AssetValidationField.HolderNumber,
                    "Holder Number cannot be negative.");
                return result.IsValid;
            }

            return result.IsValid;
        }

        /// <summary>
        /// Validates rack position (pure logic, no data access).
        /// </summary>
        public static bool IsRackPositionValid(Asset asset, out ValidationResult result)
        {
            result = new ValidationResult();

            if (asset == null)
            {
                result.AddFailReason(AssetValidationField.Asset, "Asset cannot be null.");
                return result.IsValid;
            }

            var hasRack = asset.Location?.RackId != null && asset.Location.RackId != default;
            //TODO SDM-1234: Change rack check to only check for HasValue() once all code is updated to use nullable DomIds
            var hasPosition = asset.Location?.RackPosition != null;
            var hasSide = asset.Location?.Side != null;

            // If no rack, position and side must not be set
            if (!hasRack)
            {
                if (hasPosition)
                {
                    result.AddFailReason(AssetValidationField.RackPosition,
                        "Rack Position cannot be set when there is no Rack.");
                    return result.IsValid;
                }

                if (hasSide)
                {
                    result.AddFailReason(AssetValidationField.Side,
                        "Rack Side cannot be set when there is no Rack.");
                    return result.IsValid;
                }

                return result.IsValid;
            }

            // If rack is set, position and side must also be set
            if (!hasPosition)
            {
                result.AddFailReason(AssetValidationField.RackPosition,
                    "Rack Position must be set when Rack is provided.");
                return result.IsValid;
            }

            if (!hasSide)
            {
                result.AddFailReason(AssetValidationField.Side,
                    "Rack Side must be set when Rack is provided.");
                return result.IsValid;
            }

            // Validate position is positive
            if (asset.Location.RackPosition <= 0)
            {
                result.AddFailReason(AssetValidationField.RackPosition,
                    "Rack Position must be greater than 0.");
                return result.IsValid;
            }

            return result.IsValid;
        }

        /// <summary>
        /// Validates if location can be edited based on asset state (pure logic, no data access).
        /// </summary>
        public static bool IsLocationChangeAllowed(Asset asset, out ValidationResult result)
        {
            result = new ValidationResult();

            if (asset == null)
            {
                result.AddFailReason(AssetValidationField.Asset, "Asset cannot be null.");
                return result.IsValid;
            }

            if (LocationChanged(asset) && !CanEditLocation(asset))
            {
                result.AddFailReason(AssetValidationField.Asset, $"Cannot change Location in current State '{asset.StateField.OriginalValue}'.");
            }

            return result.IsValid;
        }

        private static bool LocationChanged(Asset asset)
        {
            return asset.Location.ParentAssetField.Changed ||
                                asset.Location.HolderNumberField.Changed ||
                                asset.Location.RackIdField.Changed ||
                                asset.Location.RackPositionField.Changed ||
                                asset.Location.SideField.Changed ||
                                asset.Location.DeskIdField.Changed ||
                                asset.Location.ContainerIdField.Changed ||
                                asset.Location.RoomIdField.Changed;
        }
        #endregion

        #region Destination Location Validation

        /// <summary>
        /// Validates that only one type of destination location is attached to the asset.
        /// </summary>
        public static bool HasSingleDestinationLocation(Asset asset, out ValidationResult result)
        {
            result = new ValidationResult();

            if (asset == null)
            {
                result.AddFailReason(AssetValidationField.Asset, "Asset cannot be null.");
                return result.IsValid;
            }

            bool[] locationExists = new bool[]
            {
                asset.DestinationLocation?.ParentAsset != null && asset.DestinationLocation.ParentAsset.HasValue(),
                asset.DestinationLocation?.RackId != default && asset.DestinationLocation.RackId.HasValue(),
                asset.DestinationLocation?.DeskId != null && asset.DestinationLocation.DeskId != System.Guid.Empty,
                asset.DestinationLocation?.ContainerId != default && asset.DestinationLocation.ContainerId.HasValue(),
                asset.DestinationLocation?.RoomId != default && asset.DestinationLocation.RoomId.HasValue(),
            };

            if (locationExists.Count(entry => entry) > 1)
            {
                result.AddFailReason(AssetValidationField.Asset, "Has multiple Destination Locations attached.");
                return result.IsValid;
            }

            return result.IsValid;
        }

        /// <summary>
        /// Validates destination parent asset holder relationship (pure logic, no data access).
        /// </summary>
        public static bool IsDestinationParentAssetHolderValid(Asset asset, out ValidationResult result)
        {
            result = new ValidationResult();

            if (asset == null)
            {
                result.AddFailReason(AssetValidationField.Asset, "Asset cannot be null.");
                return result.IsValid;
            }

            var hasParentAsset = asset.DestinationLocation?.ParentAsset != null && asset.DestinationLocation.ParentAsset.HasValue();
            var hasHolderNumber = asset.DestinationLocation?.HolderNumber != null;

            // If no parent asset, holder number must not be set
            if (!hasParentAsset && hasHolderNumber)
            {
                result.AddFailReason(AssetValidationField.DestinationHolderNumber,
                    "Holder Number cannot be set when there is no Parent Asset.");
                return result.IsValid;
            }

            // If parent asset is set, holder number must also be set
            if (hasParentAsset && !hasHolderNumber)
            {
                result.AddFailReason(AssetValidationField.DestinationHolderNumber,
                    "Holder Number must be set when Parent Asset is provided.");
                return result.IsValid;
            }

            // Validate holder number is not negative
            if (hasHolderNumber && asset.DestinationLocation.HolderNumber < 0)
            {
                result.AddFailReason(AssetValidationField.DestinationHolderNumber,
                    "Holder Number cannot be negative.");
                return result.IsValid;
            }

            return result.IsValid;
        }

        /// <summary>
        /// Validates destination rack position (pure logic, no data access).
        /// </summary>
        public static bool IsDestinationRackPositionValid(Asset asset, AssetClass assetClass, out ValidationResult result)
        {
            result = new ValidationResult();

            if (asset == null)
            {
                result.AddFailReason(AssetValidationField.Asset, "Asset cannot be null.");
                return result.IsValid;
            }

            if (assetClass == null)
            {
                result.AddFailReason(AssetValidationField.AssetClass, "Asset Class cannot be null.");
                return result.IsValid;
            }

            var hasRack = asset.DestinationLocation?.RackId != null && asset.DestinationLocation.RackId != default;
            var hasPosition = asset.DestinationLocation?.RackPosition != null;
            var hasSide = asset.DestinationLocation?.Side != null;

            // If no rack, position and side must not be set
            if (!hasRack)
            {
                if (hasPosition)
                {
                    result.AddFailReason(AssetValidationField.DestinationRackPosition,
                        "Rack Position cannot be set when there is no Rack.");
                    return result.IsValid;
                }

                if (hasSide)
                {
                    result.AddFailReason(AssetValidationField.DestinationSide,
                        "Rack Side cannot be set when there is no Rack.");
                    return result.IsValid;
                }

                return result.IsValid;
            }

            // If rack is set, position and side must also be set
            if (!hasPosition)
            {
                result.AddFailReason(AssetValidationField.DestinationRackPosition,
                    "Rack Position must be set when Rack is provided.");
                return result.IsValid;
            }

            if (!hasSide)
            {
                result.AddFailReason(AssetValidationField.DestinationSide,
                    "Rack Side must be set when Rack is provided.");
                return result.IsValid;
            }

            // Validate position is positive
            if (asset.DestinationLocation.RackPosition <= 0)
            {
                result.AddFailReason(AssetValidationField.DestinationRackPosition,
                    "Rack Position must be greater than 0.");
                return result.IsValid;
            }

            // Validate asset class has height (rack attachable)
            if (assetClass.HeightU <= 0)
            {
                result.AddFailReason(AssetValidationField.AssetClass,
                    "Asset Class must have a Height (U) greater than 0 to be attached to a Rack.");
                return result.IsValid;
            }

            return result.IsValid;
        }

        /// <summary>
        /// Validates if destination location can be edited based on asset state (pure logic, no data access).
        /// </summary>
        public static bool IsDestinationLocationChangeAllowed(Asset asset, out ValidationResult result)
        {
            result = new ValidationResult();

            if (asset == null)
            {
                result.AddFailReason(AssetValidationField.Asset, "Asset cannot be null.");
                return result.IsValid;
            }

            if (!CanEditDestinationLocation(asset) && DestinationLocationChanged(asset))
            {
                result.AddFailReason(AssetValidationField.Asset, $"Cannot change Destination Location in current State ({asset.State}).");
            }

            return result.IsValid;
        }

        private static bool DestinationLocationChanged(Asset asset)
        {
            return asset.DestinationLocation.ParentAssetField.Changed ||
                                asset.DestinationLocation.HolderNumberField.Changed ||
                                asset.DestinationLocation.RackIdField.Changed ||
                                asset.DestinationLocation.RackPositionField.Changed ||
                                asset.DestinationLocation.SideField.Changed ||
                                asset.DestinationLocation.DeskIdField.Changed ||
                                asset.DestinationLocation.ContainerIdField.Changed ||
                                asset.DestinationLocation.RoomIdField.Changed;
        }

        #endregion

        #region Lifecycle Validation

        /// <summary>
        /// Validates installation info - both user and date must be set together or both empty.
        /// </summary>
        public static bool IsInstallationInfoValid(Asset asset, out ValidationResult result)
        {
            result = new ValidationResult();

            if (asset == null)
            {
                result.AddFailReason(AssetValidationField.Asset, "Asset cannot be null.");
                return result.IsValid;
            }

            var hasUserId = asset.InstallationUserId != Guid.Empty;
            var hasDate = asset.InstallationDate.HasValue;

            // Both must be set or both must be empty
            if (hasUserId && !hasDate)
            {
                result.AddFailReason(AssetValidationField.InstallationDate,
                    "Installation Date must be set when Installation User is provided.");
                return result.IsValid;
            }

            if (!hasUserId && hasDate)
            {
                result.AddFailReason(AssetValidationField.InstallationUserId,
                    "Installation User must be set when Installation Date is provided.");
                return result.IsValid;
            }

            return result.IsValid;
        }

        /// <summary>
        /// Validates modification info - both user and date must be set together or both empty.
        /// </summary>
        public static bool IsModificationInfoValid(Asset asset, out ValidationResult result)
        {
            result = new ValidationResult();

            if (asset == null)
            {
                result.AddFailReason(AssetValidationField.Asset, "Asset cannot be null.");
                return result.IsValid;
            }

            var hasUserId = asset.ModificationUserId != Guid.Empty;
            var hasDate = asset.ModificationDate.HasValue;

            // Both must be set or both must be empty
            if (hasUserId && !hasDate)
            {
                result.AddFailReason(AssetValidationField.ModificationDate,
                    "Modification Date must be set when Modification User is provided.");
                return result.IsValid;
            }

            if (!hasUserId && hasDate)
            {
                result.AddFailReason(AssetValidationField.ModificationUserId,
                    "Modification User must be set when Modification Date is provided.");
                return result.IsValid;
            }

            return result.IsValid;
        }

        #endregion

        #region Ownership Validation

        /// <summary>
        /// Validates ownership - contact person and role must be set together or both empty.
        /// </summary>
        public static bool IsOwnershipValid(Asset asset, out ValidationResult result)
        {
            result = new ValidationResult();

            if (asset == null)
            {
                result.AddFailReason(AssetValidationField.Asset, "Asset cannot be null.");
                return result.IsValid;
            }

            var hasContactPerson = asset.Ownership?.ContactPerson != null && asset.Ownership.ContactPerson != Guid.Empty;
            var hasRole = asset.Ownership?.ContactPersonRole != null && asset.Ownership.ContactPersonRole != Guid.Empty;

            // Both must be set or both must be empty
            if (hasContactPerson && !hasRole)
            {
                result.AddFailReason(AssetValidationField.OwnerContactPersonRole,
                    "Owner Contact Person Role must be set when Contact Person is provided.");
                return result.IsValid;
            }

            if (!hasContactPerson && hasRole)
            {
                result.AddFailReason(AssetValidationField.OwnerContactPerson,
                    "Owner Contact Person must be set when Contact Person Role is provided.");
                return result.IsValid;
            }

            return result.IsValid;
        }

        /// <summary>
        /// Validates custody - contact person and role must be set together or both empty.
        /// </summary>
        public static bool IsCustodyValid(Asset asset, out ValidationResult result)
        {
            result = new ValidationResult();

            if (asset == null)
            {
                result.AddFailReason(AssetValidationField.Asset, "Asset cannot be null.");
                return result.IsValid;
            }

            var hasContactPerson = asset.Custody?.ContactPerson != null && asset.Custody.ContactPerson != Guid.Empty;
            var hasRole = asset.Custody?.ContactPersonRole != null && asset.Custody.ContactPersonRole != Guid.Empty;

            // Both must be set or both must be empty
            if (hasContactPerson && !hasRole)
            {
                result.AddFailReason(AssetValidationField.CustodyContactPersonRole,
                    "Custody Contact Person Role must be set when Contact Person is provided.");
                return result.IsValid;
            }

            if (!hasContactPerson && hasRole)
            {
                result.AddFailReason(AssetValidationField.CustodyContactPerson,
                    "Custody Contact Person must be set when Contact Person Role is provided.");
                return result.IsValid;
            }

            return result.IsValid;
        }

        #endregion

        #region Collection Validation

        /// <summary>
        /// Validates Asset Holders collection.
        /// </summary>
        public static ValidationResult ValidateAssetHolders(Asset asset)
        {
            var result = new ValidationResult();

            if (asset == null)
            {
                result.AddFailReason(AssetValidationField.Asset, "Asset cannot be null.");
                return result;
            }

            var holders = asset.Holders ?? new List<AssetHolder>();
            var seenHolders = new HashSet<(long? SlotNumber, SharedMappers.DomIds.SlcAsset_Management.Enums.HierarchyRoleEnum)>();

            foreach (var holder in holders)
            {
                // Check for empty slot number
#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
                if (holder.SlotNumber == null)
                {
                    result.AddFailReason(AssetValidationField.Holder,
                        "All Holders must have a Slot Number.");
                    return result;
                }
#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'

                // Check for negative slot number
                if (holder.SlotNumber < 0)
                {
                    result.AddFailReason(AssetValidationField.Holder,
                        $"Holder Slot number cannot be negative. Found: {holder.SlotNumber}");
                    return result;
                }

                // Check for duplicate slot+role combination
                var holderKey = (holder.SlotNumber, holder.HierarchyRole);
                if (!seenHolders.Add(holderKey))
                {
                    result.AddFailReason(AssetValidationField.Holder,
                        $"Duplicate Holder found: Slot {holder.SlotNumber}, Role {holder.HierarchyRole}");
                    return result;
                }
            }

            return result;
        }

        /// <summary>
        /// Validates Asset Elements collection.
        /// </summary>
        public static ValidationResult ValidateAssetElements(Asset asset)
        {
            var result = new ValidationResult();

            if (asset == null)
            {
                result.AddFailReason(AssetValidationField.Asset, "Asset cannot be null.");
                return result;
            }

            var elements = asset.ElementLinks ?? new List<ElementLink>();
            var seenElementIds = new HashSet<string>();
            bool primaryFound = false;

            foreach (var element in elements)
            {
                // Check for multiple primary elements
                if (element.IsPrimary)
                {
                    if (primaryFound)
                    {
                        result.AddFailReason(AssetValidationField.Element,
                            "Only one Element can be marked as Primary.");
                        return result;
                    }

                    primaryFound = true;
                }

                // Check for duplicate element IDs
                if (element.ElementID != null && !seenElementIds.Add(element.ElementID))
                {
                    result.AddFailReason(AssetValidationField.Element,
                        $"Duplicate Element ID found: {element.ElementID}");
                    return result;
                }
            }

            return result;
        }

        #endregion

        #region State Helper

        /// <summary>
        /// Checks if the asset is in a state where location can be edited.
        /// </summary>
        public static bool CanEditLocation(Asset asset)
        {
            if (asset == null)
            {
                return false;
            }

            if (asset.IsNew)
            {
                return true;
            }

            return asset.State == SharedMappers.DomIds.SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available
                || asset.State == SharedMappers.DomIds.SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InPlanning
                || asset.State == SharedMappers.DomIds.SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.BuildPlanReady
                || asset.State == SharedMappers.DomIds.SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InRepair
                || asset.State == SharedMappers.DomIds.SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Disposed;
        }

        /// <summary>
        /// Checks if the asset is in a state where destination location can be edited.
        /// </summary>
        public static bool CanEditDestinationLocation(Asset asset)
        {
            if (asset == null)
            {
                return false;
            }

            return asset.State == SharedMappers.DomIds.SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InTransit;
        }

        #endregion

        #region Destination Location Validation

        /// <summary>
        /// Validates Destination Location based on Asset state.
        /// Rules:
        /// - Destination Location is MANDATORY when state is "In Transit"
        /// - If NOT defined in "In Transit" state: ERROR (mandatory)
        /// - If defined in other states: WARNING (value will be ignored)
        /// </summary>
        public static ValidationResult ValidateDestinationLocation(Asset asset)
        {
            var result = new ValidationResult();

            if (asset == null)
            {
                result.AddFailReason(AssetValidationField.Asset, "Asset cannot be null.");
                return result;
            }

            var state = asset.State;
            var hasDestinationLocation = HasDestinationLocation(asset);

            // Check if in "In Transit" state
            bool isInTransit = state == SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InTransit;

            if (isInTransit)
            {
                // Rule: Destination Location is MANDATORY when In Transit
                if (!hasDestinationLocation)
                {
                    result.AddFailReason(AssetValidationField.DestinationLocation,
                        "Destination Location is mandatory when Asset is in 'In Transit' state.");
                }
            }
            else
            {
                // Rule: Destination Location NOT allowed in other states - warn if present
                if (hasDestinationLocation)
                {
                    result.AddWarning(AssetValidationField.DestinationLocation,
                        $"Destination Location is only applicable when Asset is in 'In Transit' state. Current state: '{state}'. The Destination Location will be ignored.");
                }
            }

            return result;
        }

        /// <summary>
        /// Checks if the Asset has any Destination Location defined.
        /// </summary>
        private static bool HasDestinationLocation(Asset asset)
        {
            if (asset.DestinationLocation == null)
            {
                return false;
            }

            // Check if any destination location field is populated
            return asset.DestinationLocation.ParentAsset.HasValue() ||
                   asset.DestinationLocation.RackId.HasValue() ||
                   asset.DestinationLocation.DeskId != Guid.Empty ||
                   asset.DestinationLocation.ContainerId.HasValue() ||
                   asset.DestinationLocation.RoomId.HasValue();
        }

        #endregion
    }
}