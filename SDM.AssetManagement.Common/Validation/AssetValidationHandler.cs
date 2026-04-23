namespace Skyline.DataMiner.SDM.AssetManagement.Common.Validation
{
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.SDM.AssetManagement.Models;
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
            State,

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
                asset.Location?.RackId != default,
                asset.Location?.DeskId != default,
                asset.Location?.ContainerId != null && asset.Location.ContainerId.HasValue(),
                asset.Location?.RoomId != default,
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
        public static bool IsParentAssetHolderValid(Asset asset, AssetClass assetClass, out ValidationResult result)
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
        public static bool IsRackPositionValid(Asset asset, AssetClass assetClass, out ValidationResult result)
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

            var hasRack = asset.Location?.RackId != null && asset.Location.RackId != default;
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

            // Validate asset class has height (rack attachable)
            if (assetClass.HeightU <= 0)
            {
                result.AddFailReason(AssetValidationField.AssetClass,
                    "Asset Class must have a Height (U) greater than 0 to be attached to a Rack.");
                return result.IsValid;
            }

            return result.IsValid;
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
                asset.DestinationLocation?.RackId != default,
                asset.DestinationLocation?.DeskId != default,
                asset.DestinationLocation?.ContainerId != null && asset.DestinationLocation.ContainerId.HasValue(),
                asset.DestinationLocation?.RoomId != default,
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
        public static bool IsDestinationParentAssetHolderValid(Asset asset, AssetClass assetClass, out ValidationResult result)
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

            var hasUserId = asset.InstallationUserId != default;
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

            var hasUserId = asset.ModificationUserId != default;
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

            var hasContactPerson = asset.Ownership?.ContactPerson != null && asset.Ownership.ContactPerson != default;
            var hasRole = asset.Ownership?.ContactPersonRole != null && asset.Ownership.ContactPersonRole != default;

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

            var hasContactPerson = asset.Custody?.ContactPerson != null && asset.Custody.ContactPerson != default;
            var hasRole = asset.Custody?.ContactPersonRole != null && asset.Custody.ContactPersonRole != default;

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

            // Check for empty slot numbers
            var emptySlots = holders.Where(h => h.SlotNumber == null).ToList();
            if (emptySlots.Any())
            {
                result.AddFailReason(AssetValidationField.Holder,
                    "All Holders must have a Slot Number.");
            }

            // Check for negative slot numbers
            var negativeSlots = holders.Where(h => h.SlotNumber != null && h.SlotNumber < 0).ToList();
            if (negativeSlots.Any())
            {
                result.AddFailReason(AssetValidationField.Holder,
                    $"Holder Slot numbers cannot be negative. Found: {string.Join(", ", negativeSlots.Select(h => h.SlotNumber))}");
            }

            // Check for duplicate slot+role combinations
            var duplicates = holders
                .Where(h => h.SlotNumber != null && h.HierarchyRole != null)
                .GroupBy(h => new { h.SlotNumber, h.HierarchyRole })
                .Where(g => g.Count() > 1)
                .Select(g => $"Slot {g.Key.SlotNumber}, Role {g.Key.HierarchyRole}")
                .ToList();

            if (duplicates.Any())
            {
                result.AddFailReason(AssetValidationField.Holder,
                    $"Duplicate Holder combinations found: {string.Join("; ", duplicates)}");
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

            var elements = asset.Elements ?? new List<ElementLink>();

            // Check for multiple primary elements
            var primaryCount = elements.Count(e => e.IsPrimary);
            if (primaryCount > 1)
            {
                result.AddFailReason(AssetValidationField.Element,
                    "Only one Element can be marked as Primary.");
            }

            // Check for duplicate element IDs
            var duplicates = elements
                .Where(e => e.ElementID != null)
                .GroupBy(e => e.ElementID)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicates.Any())
            {
                result.AddFailReason(AssetValidationField.Element,
                    $"Duplicate Element IDs found: {string.Join(", ", duplicates)}");
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

            return asset.State == SharedMappers.DomIds.SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available
                || asset.State == SharedMappers.DomIds.SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InPlanning
                || asset.State == SharedMappers.DomIds.SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.BuildPlanReady
                || asset.State == SharedMappers.DomIds.SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InRepair
                || asset.State == SharedMappers.DomIds.SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Disposed
                || asset.State == SharedMappers.DomIds.SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InTransit;
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

            return asset.State == SharedMappers.DomIds.SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available
                || asset.State == SharedMappers.DomIds.SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InPlanning
                || asset.State == SharedMappers.DomIds.SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.BuildPlanReady
                || asset.State == SharedMappers.DomIds.SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InRepair
                || asset.State == SharedMappers.DomIds.SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Disposed
                || asset.State == SharedMappers.DomIds.SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InTransit;
        }

        #endregion
    }
}