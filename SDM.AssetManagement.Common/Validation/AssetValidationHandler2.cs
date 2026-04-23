namespace Skyline.DataMiner.Utils.InfraOps.Common.DOM_Classes.DOM.Applications.Asset_Manager.Validations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM.AssetManagement.Common.Validation;
    using Skyline.DataMiner.Utils.InfraOps.Common.DOM_Classes.DOM.Applications.All.Validations;
    using Skyline.DataMiner.Utils.InfraOps.Common.DOM_Classes.DOM.Applications.Asset_Manager.Wrappers;
    using Skyline.DataMiner.Utils.InfraOps.Common.DOM_Classes.DOM.Applications.DomIds;
    using Skyline.DataMiner.Utils.InfraOps.Common.DOM_Classes.DOM.Applications.Facility_Manager.Validations;
    using Skyline.DataMiner.Utils.InfraOps.Common.Validation;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    public static class AssetValidationHandler
    {
        public enum AssetValidationField
        {
            Asset,
            Name,
            AssetId,
            AssetClass,
            SerialNumber,

            ParentAsset,
            ParentAssetHolderNumber,

            Rack,
            RackPosition,
            RackSide,

            DestinationParentAsset,
            DestinationParentAssetHolderNumber,

            DestinationRack,
            DestinationRackPosition,
            DestinationRackSide,

            LifeCycleInstallationUser,
            LifeCycleInstallationDate,

            LifeCycleModificationUser,
            LifeCycleModificationDate,

            OwnerContactPerson,
            OwnerContactPersonRole,

            CustodyContactPerson,
            CustodyContactPersonRole,

            DataPort,

            PowerPort,

            HolderSlotNumber,

            Element,
        }

        public static ValidationResult ValidateAsset(this AssetWrapper asset, ValidatorContext<AssetWrapper> context)
        {
            List<Func<ValidationResult>> validations = new List<Func<ValidationResult>>()
            {
                () => ValidateAssetInfo(asset, context),
                () => ValidateAssetNetworkDetails(asset, context),
                () => ValidateAssetLocation(asset, context),
                () => ValidateAssetDestinationLocation(asset, context),
                () => ValidateAssetLifecycleInfo(asset, context),
                () => ValidateAssetOwnership(asset, context),
                () => ValidateAssetCustody(asset, context),
                () => ValidateAssetDataPorts(asset, context),
                () => ValidateAssetPowerPorts(asset, context),
                () => ValidateAssetHolders(asset),
                () => ValidateAssetElements(asset),
            };

            ValidationResult result = new ValidationResult();
            foreach (var validation in validations)
            {
                result.CombineResults(validation());

                if (context.ReturnWhenInvalid && !result.IsValid)
                {
                    return result;
                }
            }

            return result;
        }

        #region Info

        private static ValidationResult ValidateAssetInfo(AssetWrapper asset, ValidatorContext<AssetWrapper> context)
        {
            var validationFactory = ValidationFactory<AssetWrapper>
                .PrepareValidation(
                (dat) => dat.Object.NameField.Changed,
                (dat) =>
                {
                    IsAssetNameValid(dat.Object.ModuleHandlers, dat.Object.Name, dat.Context, out var result);
                    return result;
                })
                .AddValidation(
                (dat) => dat.Object.AssetIDField.Changed,
                (dat) =>
                {
                    IsAssetIdValid(dat.Object.ModuleHandlers, dat.Object.AssetID, dat.Context, out var result);
                    return result;
                })
                .AddValidation(
                (dat) => !dat.Object.HasAssetClass || dat.Object.AssetClassIdField.Changed,
                (dat) =>
                {
                    IsAssetClassValid(dat.Object, out var result);
                    return result;
                })
                .AddValidation(
                (dat) => dat.Object.SerialNumberField.Changed,
                (dat) =>
                {
                    IsSerialNumberValid(dat.Object.ModuleHandlers, dat.Object.SerialNumber, dat.Object.AssetClass, dat.Context, out var result);
                    return result;
                });

            validationFactory.Validate(asset, context, out var assetValidationResult);
            return assetValidationResult;
        }

        public static bool IsAssetNameValid(GlobalInfraOpsModuleHandler moduleHandlers, string assetName, ValidatorContext<AssetWrapper> context, out ValidationResult result)
        {
            result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(assetName))
            {
                result.AddFailReason(AssetValidationField.Name, "Asset Name cannot be empty or whiteSpace.");
                return result.IsValid;
            }

            foreach (var otherAsset in context.OtherChangedEntries)
            {
                if (string.Equals(assetName, otherAsset.Name))
                {
                    result.AddFailReason(AssetValidationField.Name, "Asset Name already in use.");
                    return result.IsValid;
                }
            }

            if (moduleHandlers.AssetHandler.IsNameInUse(assetName, context.ChangedEntries))
            {
                result.AddFailReason(AssetValidationField.Name, "Asset Name Already in Use.");
                return result.IsValid;
            }

            return result.IsValid;
        }

        public static bool IsAssetIdValid(GlobalInfraOpsModuleHandler moduleHandlers, string assetId, ValidatorContext<AssetWrapper> context, out ValidationResult result)
        {
            result = new ValidationResult();

            // Empty asset name is not valid
            if (string.IsNullOrWhiteSpace(assetId))
            {
                result.AddFailReason(AssetValidationField.AssetId, "Asset Id cannot be empty or whiteSpace.");
                return result.IsValid;
            }

            foreach (var otherAsset in context.OtherChangedEntries)
            {
                if (string.Equals(assetId, otherAsset.AssetID))
                {
                    result.AddFailReason(AssetValidationField.AssetId, "Asset Id already in use.");
                    return result.IsValid;
                }
            }

            if (moduleHandlers.AssetHandler.IsAssetIdInUse(assetId, context.ChangedEntries))
            {
                result.AddFailReason(AssetValidationField.AssetId, "Asset Id Already in Use.");
                return result.IsValid;
            }

            return result.IsValid;
        }

        public static bool IsAssetClassValid(AssetWrapper asset, out ValidationResult result)
        {
            result = new ValidationResult();
            if (!asset.HasAssetClass)
            {
                result.AddFailReason(AssetValidationField.AssetClass, "Asset Class cannot be empty.");
                return result.IsValid;
            }

            return result.IsValid;
        }

        public static bool IsSerialNumberValid(GlobalInfraOpsModuleHandler moduleHandlers, string serialNumber, AssetClassWrapper assetClass, ValidatorContext<AssetWrapper> context, out ValidationResult result)
        {
            result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                // Empty serial Number is valid
                return result.IsValid;
            }

            foreach (var otherAsset in context.OtherChangedEntries)
            {
                if (assetClass == otherAsset.AssetClass && string.Equals(serialNumber, otherAsset.SerialNumber))
                {
                    result.AddFailReason(AssetValidationField.SerialNumber, "Seria Number already in use in the another changed Asset Class.");
                    return result.IsValid;
                }
            }

            if (!moduleHandlers.AssetHandler.IsSerialNumberValid(serialNumber, assetClass, context.BaseEntry))
            {
                result.AddFailReason(AssetValidationField.SerialNumber, "Serial Number Already in use in the another Asset Class.");
                return result.IsValid;
            }

            return result.IsValid;
        }

        #endregion

        #region NetworkDetails

        private static ValidationResult ValidateAssetNetworkDetails(AssetWrapper asset, ValidatorContext<AssetWrapper> context)
        {
            ValidationResult result = new ValidationResult();

            return result;
        }

        #endregion

        #region Location

        public static ValidationResult ValidateAssetLocation(AssetWrapper asset, ValidatorContext<AssetWrapper> context)
        {
            ValidationResult result = new ValidationResult();

            if (!asset.CanEditLocation())
            {
                if (
                    asset.DirectParentAssetIdField.Changed ||
                    asset.HolderNumberField.Changed ||
                    asset.DirectRackIdField.Changed ||
                    asset.RackPositionField.Changed ||
                    asset.RackSideField.Changed ||
                    asset.DirectDeskIdField.Changed ||
                    asset.DirectFacilityIdField.Changed ||
                    asset.DirectRoomIdField.Changed)
                {
                    result.AddFailReason(AssetValidationField.Asset, "Cannot change Location in current State.");
                    return result;
                }

                return result;
            }

            bool[] locationExists = new bool[]
            {
                asset.HasDirectParentAsset,
                asset.HasDirectRack,
                asset.HasDirectDesk,
                asset.HasDirectFacility,
                asset.HasDirectRoom,
            };

            if (locationExists.Count(entry => entry) > 1)
            {
                result.AddFailReason(AssetValidationField.Asset, "Has multiple Locations attached.");
                if (context.ReturnWhenInvalid && !result.IsValid)
                {
                    return result;
                }
            }

            var validationFactory = ValidationFactory<AssetWrapper>
                .PrepareValidation(
                (dat) => dat.Object.DirectParentAssetIdField.Changed || dat.Object.HolderNumberField.Changed,
                (dat) =>
                {
                    IsValidParentAssetAttachment(dat.Object, out var parentAssetAttachmentResult);
                    return parentAssetAttachmentResult;
                })
                .AddValidation(
                (dat) => dat.Object.DirectRackIdField.Changed || dat.Object.RackPositionField.Changed || dat.Object.RackSideField.Changed,
                (dat) =>
                {
                    IsValidParentAssetAttachment(dat.Object, out var parentRackAttachmentResult);
                    return parentRackAttachmentResult;
                });

            validationFactory.Validate(asset, context, out var assetValidationResult);
            return assetValidationResult;
        }

        private static bool IsValidParentAssetAttachment(AssetWrapper asset, out ValidationResult result)
        {
            result = new ValidationResult();

            if (!asset.HasDirectParentAsset)
            {
                if (asset.HolderNumber != null)
                {
                    result.AddFailReason(AssetValidationField.ParentAssetHolderNumber, "Holder Number cannot be set when there is no Parent Asset.");
                    return result.IsValid;
                }

                return result.IsValid;
            }

            if (asset.HolderNumber == null)
            {
                result.AddFailReason(AssetValidationField.ParentAssetHolderNumber, "Holder Number must be set when Parent Asset is provided.");
                return result.IsValid;
            }

            var parentAsset = asset.DirectParentAsset;
            var hierarchyRole = asset.AssetClass.DeviceType.HierarchyRole;

            if (asset.HolderNumber.Value < 0)
            {
                result.AddFailReason(AssetValidationField.ParentAssetHolderNumber, "Invalid Holder Number: Must be within Asset Holders.");
                return result.IsValid;
            }

            if (!parentAsset.Holders.Any(h => h.HierarchyRole == hierarchyRole && h.Number.HasValue && h.Number.Value == asset.HolderNumber.Value))
            {
                result.AddFailReason(AssetValidationField.ParentAssetHolderNumber, "Invalid Holder Number: No Slot exists matching the Holder Number and Asset Hierarchy Role.");
                return result.IsValid;
            }

            if (parentAsset.IsHolderOccupied((int)asset.HolderNumber.Value, hierarchyRole.Value, asset))
            {
                result.AddFailReason(AssetValidationField.ParentAssetHolderNumber, "Holder Number is already occupied on the Parent Asset.");
                return result.IsValid;
            }

            return result.IsValid;
        }

        public static bool IsValidRackAttachment(AssetWrapper asset, out ValidationResult result)
        {
            result = new ValidationResult();

            if (!asset.HasDirectRack)
            {
                if (asset.RackPosition != null)
                {
                    result.AddFailReason(AssetValidationField.RackPosition, "Rack Position cannot be set when there is no Rack.");
                    return result.IsValid;
                }

                if (asset.RackSide != null)
                {
                    result.AddFailReason(AssetValidationField.RackSide, "Rack Side cannot be set when there is no Rack.");
                    return result.IsValid;
                }

                return result.IsValid;
            }

            var assetClass = asset.AssetClass;

            if (!AssetClassValidationHandler.IsRackAttacheable(assetClass, out var rackAttacheableResult))
            {
                result = result.CombineResults(rackAttacheableResult);
                return result.IsValid;
            }

            var position = asset.RackPosition;
            if (position == null)
            {
                result.AddFailReason(AssetValidationField.RackPosition, "Rack Position must be set when Rack is provided.");
                return result.IsValid;
            }

            if (asset.RackSide == null)
            {
                result.AddFailReason(AssetValidationField.RackPosition, "Rack Side must be set when Rack is provided.");
                return result.IsValid;
            }

            var rack = asset.DirectRack;

            if (position.Value < 1 || position > rack.RackUnits)
            {
                result.AddFailReason(AssetValidationField.RackPosition, "Invalid Position: Must be within Rack.");
                return result.IsValid;
            }

            if (!RackValidationHandler.ValidateRackSpace(rack, asset, (int)position.Value, (int)assetClass.HeightUOrDefault, out var rackSpaceResult))
            {
                result.CombineResults(rackSpaceResult);
                return result.IsValid;
            }

            return result.IsValid;
        }

        public static bool IsParentHolderSlotValid(
            AssetWrapper parentAsset,
            long slot,
            AssetWrapper childAsset,
            SlcAsset_Management.Enums.HierarchyRoleEnum childHierarchyRole,
            out ValidationResult result)
        {
            result = new ValidationResult();

            if (parentAsset == null)
            {
                result.AddFailReason(AssetValidationField.ParentAsset, $"A Parent Asset must be provided.");
                return result.IsValid;
            }

            if (childAsset.HasDirectParentAsset && parentAsset.InstanceId.Equals(childAsset.DirectParentAssetId.Value) && childAsset.HolderNumber.Value == slot)
            {
                return result.IsValid;
            }

            if (!parentAsset.GetAvailableHolders().Any(holder => holder.Number == slot && holder.HierarchyRole == childHierarchyRole))
            {
                result.AddFailReason(AssetValidationField.ParentAssetHolderNumber, $"The selected slot number '{slot}' is not available for the parent asset '{parentAsset.InstanceName}'.");
                return result.IsValid;
            }

            return result.IsValid;
        }

        #endregion

        #region Destination Location

        public static ValidationResult ValidateAssetDestinationLocation(AssetWrapper asset, ValidatorContext<AssetWrapper> context)
        {
            ValidationResult result = new ValidationResult();

            if (!asset.CanEditDestinationLocation())
            {
                if (
                    asset.DestinationDirectParentAssetIdField.Changed ||
                    asset.DestinationHolderNumberField.Changed ||
                    asset.DestinationDirectRackIdField.Changed ||
                    asset.DestinationRackPositionField.Changed ||
                    asset.DestinationRackSideField.Changed ||
                    asset.DestinationDirectDeskIdField.Changed ||
                    asset.DestinationDirectFacilityIdField.Changed ||
                    asset.DestinationDirectRoomIdField.Changed
                    )
                {
                    result.AddFailReason(AssetValidationField.Asset, "Cannot change Destination Location in current State.");
                    return result;
                }

                return result;
            }

            bool[] locationExists = new bool[]
            {
                asset.HasDestinationDirectParentAsset,
                asset.HasDestinationDirectRack,
                asset.HasDestinationDirectDesk,
                asset.HasDestinationDirectFacility,
                asset.HasDestinationDirectRoom,
            };

            if (locationExists.Count(entry => entry) > 1)
            {
                result.AddFailReason(AssetValidationField.Asset, "Has multiple Destination Locations attached.");
                if (context.ReturnWhenInvalid && !result.IsValid)
                {
                    return result;
                }
            }

            var validationFactory = ValidationFactory<AssetWrapper>
                .PrepareValidation(
                (dat) => dat.Object.DestinationDirectParentAssetIdField.Changed || dat.Object.DestinationHolderNumberField.Changed,
                (dat) =>
                {
                    IsValidDestinationParentAssetAttachment(dat.Object, out var parentAssetAttachmentResult);
                    return parentAssetAttachmentResult;
                })
                .AddValidation(
                (dat) => dat.Object.DestinationDirectRackIdField.Changed || dat.Object.DestinationRackPositionField.Changed || dat.Object.DestinationRackSideField.Changed,
                (dat) =>
                {
                    IsValidDestinationRackAttachment(dat.Object, out var parentRackAttachmentResult);
                    return parentRackAttachmentResult;
                });

            validationFactory.Validate(asset, context, out var assetValidationResult);
            return assetValidationResult;
        }

        public static bool IsValidDestinationParentAssetAttachment(AssetWrapper asset, out ValidationResult result)
        {
            result = new ValidationResult();
            if (asset == null)
            {
                result.AddFailReason(AssetValidationField.Asset, "Asset must be provided.");
                return result.IsValid;
            }

            if (!asset.HasDestinationDirectParentAsset)
            {
                if (asset.DestinationHolderNumber != null)
                {
                    result.AddFailReason(AssetValidationField.DestinationParentAssetHolderNumber, "Holder Number cannot be set when there is no Parent Asset.");
                    return result.IsValid;
                }

                return result.IsValid;
            }

            if (asset.DestinationHolderNumber == null)
            {
                result.AddFailReason(AssetValidationField.DestinationParentAssetHolderNumber, "Holder Number must be set when Parent Asset is provided.");
                return result.IsValid;
            }

            var parentAsset = asset.DestinationDirectParentAsset;
            var hierarchyRole = asset.AssetClass.DeviceType.HierarchyRole;

            if (asset.DestinationHolderNumber.Value < 0)
            {
                result.AddFailReason(AssetValidationField.DestinationParentAssetHolderNumber, "Invalid Holder Number: Must be within Asset Holders.");
                return result.IsValid;
            }

            if (!parentAsset.Holders.Any(h => h.HierarchyRole == hierarchyRole && h.Number.HasValue && h.Number.Value == asset.DestinationHolderNumber.Value))
            {
                result.AddFailReason(AssetValidationField.DestinationParentAssetHolderNumber, "Invalid Holder Number: No Slot exists matching the Holder Number and Asset Hierarchy Role.");
                return result.IsValid;
            }

            if (parentAsset.IsHolderOccupied((int)asset.DestinationHolderNumber.Value, hierarchyRole.Value, asset))
            {
                result.AddFailReason(AssetValidationField.DestinationParentAssetHolderNumber, "Holder Number is already occupied on the Parent Asset.");
                return result.IsValid;
            }

            return result.IsValid;
        }

        public static bool IsValidDestinationRackAttachment(AssetWrapper asset, out ValidationResult result)
        {
            result = new ValidationResult();
            if (asset == null)
            {
                result.AddFailReason(AssetValidationField.Rack, "Asset must be provided.");
                return result.IsValid;
            }

            if (!asset.HasDestinationDirectRack)
            {
                if (asset.DestinationRackPosition != null)
                {
                    result.AddFailReason(AssetValidationField.DestinationRackPosition, "Rack Position cannot be set when there is no Rack.");
                    return result.IsValid;
                }

                if (asset.DestinationRackSide != null)
                {
                    result.AddFailReason(AssetValidationField.DestinationRackSide, "Rack Side cannot be set when there is no Rack.");
                    return result.IsValid;
                }

                return result.IsValid;
            }

            var assetClass = asset.AssetClass;

            if (!AssetClassValidationHandler.IsRackAttacheable(assetClass, out var rackAttacheableResult))
            {
                result = result.CombineResults(rackAttacheableResult);
                return result.IsValid;
            }

            var position = asset.DestinationRackPosition;
            if (position == null)
            {
                result.AddFailReason(AssetValidationField.DestinationRackPosition, "Rack Position must be set when Rack is provided.");
                return result.IsValid;
            }

            if (asset.DestinationRackSide == null)
            {
                result.AddFailReason(AssetValidationField.DestinationRackPosition, "Rack Side must be set when Rack is provided.");
                return result.IsValid;
            }

            var rack = asset.DestinationDirectRack;

            if (position.Value < 1 || position > rack.RackUnits)
            {
                result.AddFailReason(AssetValidationField.DestinationRackPosition, "Invalid Position: Must be within Rack.");
                return result.IsValid;
            }

            if (!RackValidationHandler.ValidateRackSpace(rack, asset, (int)position.Value, (int)assetClass.HeightUOrDefault, out var rackSpaceResult))
            {
                result.CombineResults(rackSpaceResult);
                return result.IsValid;
            }

            return result.IsValid;
        }

        #endregion

        #region LifecycleInfo

        public static ValidationResult ValidateAssetLifecycleInfo(AssetWrapper asset, ValidatorContext<AssetWrapper> context)
        {
            var validationFactory = ValidationFactory<AssetWrapper>
                .PrepareValidation(
                (dat) => dat.Object.InstallationUserIdField.Changed || dat.Object.InstallationDateField.Changed,
                (dat) =>
                {
                    IsInstallationValid(dat.Object, out var result);
                    return result;
                })
                .AddValidation(
                (dat) => dat.Object.ModificationUserIdField.Changed || dat.Object.ModificationDateField.Changed || dat.Object.RackSideField.Changed,
                (dat) =>
                {
                    IsModificationValid(dat.Object, out var result);
                    return result;
                });

            validationFactory.Validate(asset, context, out var assetValidationResult);
            return assetValidationResult;
        }

        public static bool IsInstallationValid(AssetWrapper asset, out ValidationResult result)
        {
            result = new ValidationResult();

            bool hasInstallerUser = asset.HasInstallationUser;
            bool hasInstallerDate = asset.InstallationDate != null;

            if ((hasInstallerUser && !hasInstallerDate) || (!hasInstallerUser && hasInstallerDate))
            {
                result.AddFailReason(AssetValidationField.LifeCycleInstallationUser, "If it has Lyfecycle Installer data it must have both the User and Date. If not both fields must be empty");
                result.AddFailReason(AssetValidationField.LifeCycleInstallationDate, "If it has Lyfecycle Installer data it must have both the User and Date. If not both fields must be empty");
                return result.IsValid;
            }

            return result.IsValid;
        }

        public static bool IsModificationValid(AssetWrapper asset, out ValidationResult result)
        {
            result = new ValidationResult();

            bool hasInstallerUser = asset.HasModificationUser;
            bool hasInstallerDate = asset.ModificationDate != null;

            if ((hasInstallerUser && !hasInstallerDate) || (!hasInstallerUser && hasInstallerDate))
            {
                result.AddFailReason(AssetValidationField.LifeCycleModificationUser, "If it has Lyfecycle Modification data it must have both the User and Date. If not both fields must be empty");
                result.AddFailReason(AssetValidationField.LifeCycleModificationDate, "If it has Lyfecycle Modification data it must have both the User and Date. If not both fields must be empty");
                return result.IsValid;
            }

            return result.IsValid;
        }

        #endregion

        #region Ownership

        public static ValidationResult ValidateAssetOwnership(AssetWrapper asset, ValidatorContext<AssetWrapper> context)
        {
            var validationFactory = ValidationFactory<AssetWrapper>
                .PrepareValidation(
                (dat) => dat.Object.OwnerContactPersonIdField.Changed || dat.Object.OwnerContactPersonRoleIdField.Changed,
                (dat) =>
                {
                    IsOwnerValid(dat.Object, out var ownerResult);
                    return ownerResult;
                });

            validationFactory.Validate(asset, context, out var assetValidationResult);
            return assetValidationResult;
        }

        public static bool IsOwnerValid(AssetWrapper asset, out ValidationResult result)
        {
            result = new ValidationResult();

            bool hasOwnerPerson = asset.HasOwnerContactPerson;
            bool hasOwnerPersonRole = asset.HasOwnerContactPersonRole;

            if ((hasOwnerPerson && !hasOwnerPersonRole) || (!hasOwnerPerson && hasOwnerPersonRole))
            {
                result.AddFailReason(AssetValidationField.OwnerContactPerson, "If it has Owner Contact Person data it must have both the Person and Role. If not both fields must be empty");
                result.AddFailReason(AssetValidationField.OwnerContactPersonRole, "If it has Owner Contact Person data it must have both the Person and Role. If not both fields must be empty");
                return result.IsValid;
            }

            return result.IsValid;
        }

        #endregion

        #region Custody

        public static ValidationResult ValidateAssetCustody(AssetWrapper asset, ValidatorContext<AssetWrapper> context)
        {
            var validationFactory = ValidationFactory<AssetWrapper>
                .PrepareValidation(
                (dat) => dat.Object.CustodyContactPersonIdField.Changed || dat.Object.CustodyContactPersonRoleIdField.Changed,
                (dat) =>
                {
                    IsCustodyValid(dat.Object, out var result);
                    return result;
                });

            validationFactory.Validate(asset, context, out var assetValidationResult);
            return assetValidationResult;
        }

        public static bool IsCustodyValid(AssetWrapper asset, out ValidationResult result)
        {
            result = new ValidationResult();

            bool hasCustodyPerson = asset.HasCustodyContactPerson;
            bool hasCustodyPersonRole = asset.HasCustodyContactPersonRole;

            if ((hasCustodyPerson && !hasCustodyPersonRole) || (!hasCustodyPerson && hasCustodyPersonRole))
            {
                result.AddFailReason(AssetValidationField.CustodyContactPerson, "If it has Custody Contact Person data it must have both the Person Name and Role Name. If not both fields must be empty");
                result.AddFailReason(AssetValidationField.CustodyContactPersonRole, "If it has Custody Contact Person data it must have both the Person Name and Role Name. If not both fields must be empty");
                return result.IsValid;
            }

            return result.IsValid;
        }

        #endregion

        #region Data Ports

        public static ValidationResult ValidateAssetDataPorts(AssetWrapper asset, ValidatorContext<AssetWrapper> context)
        {
            var validationFactory = ValidationFactory<AssetWrapper>
                .PrepareValidation(
                (dat) => dat.Object.DataPortInfosField.Changed,
                (dat) =>
                {
                    IsValidDataPortAssetRelation(dat.Object.DataPorts, out var result);
                    return result;
                });

            validationFactory.Validate(asset, context, out var assetValidationResult);
            return assetValidationResult;
        }

        public static bool IsValidDataPortAssetRelation(IEnumerable<DataPortWrapper> dataPorts, out ValidationResult result)
        {
            result = new ValidationResult();

            int primaryIpV4FoundCount = 0;
            int primaryIpV6FoundCount = 0;

            var seenHolders = new HashSet<long>();

            foreach (var port in dataPorts)
            {
                if (port.PortNumber < 0)
                {
                    result.AddFailReason(AssetValidationField.DataPort, "Data Port Number cannot be negative.");
                    return result.IsValid;
                }

                if (port.IsPrimaryIpv4Primary)
                {
                    primaryIpV4FoundCount++;
                }

                if (primaryIpV4FoundCount > 1)
                {
                    result.AddFailReason(AssetValidationField.DataPort, $"Multiple primary IPV4 Port found.");
                    return result.IsValid;
                }

                if (port.IsPrimaryIpv6Primary)
                {
                    primaryIpV6FoundCount++;
                }

                if (primaryIpV6FoundCount > 1)
                {
                    result.AddFailReason(AssetValidationField.DataPort, $"Multiple primary IPV6 Port found.");
                    return result.IsValid;
                }

                var holderKey = port.PortNumber;
                if (!seenHolders.Add(holderKey))
                {
                    result.AddFailReason(AssetValidationField.DataPort, $"Multiple Data Ports have the same Port Number '{port.PortNumber}' and Port Type '{port.PortType.Name}'.");
                    return result.IsValid;
                }
            }

            return result.IsValid;
        }

        #endregion

        #region Power Ports

        public static ValidationResult ValidateAssetPowerPorts(AssetWrapper asset, ValidatorContext<AssetWrapper> context)
        {
            var validationFactory = ValidationFactory<AssetWrapper>
                .PrepareValidation(
                (dat) => dat.Object.PowerPortInfosField.Changed,
                (dat) =>
                {
                    IsValidPowerPortAssetRelation(dat.Object.PowerPorts, out var result);
                    return result;
                });

            validationFactory.Validate(asset, context, out var assetValidationResult);
            return assetValidationResult;
        }

        public static bool IsValidPowerPortAssetRelation(IEnumerable<PowerPortWrapper> powerPorts, out ValidationResult result)
        {
            result = new ValidationResult();

            var seenHolders = new HashSet<long>();

            foreach (var port in powerPorts)
            {
                if (port.PortNumber < 0)
                {
                    result.AddFailReason(AssetValidationField.PowerPort, "Power Port Number cannot be negative.");
                    return result.IsValid;
                }

                var holderKey = port.PortNumber;
                if (!seenHolders.Add(holderKey))
                {
                    result.AddFailReason(AssetValidationField.PowerPort, $"Multiple Power Ports have the same Port Number '{port.PortNumber}' and Port Type '{port.PortType.Name}'.");
                    return result.IsValid;
                }
            }

            return result.IsValid;
        }

        #endregion

        #region Holders

        public static ValidationResult ValidateAssetHolders(AssetWrapper asset)
        {
            var validationFactory = ValidationFactory<AssetWrapper>
                .PrepareValidation(
                (dat) => dat.Object.HoldersField.Changed,
                (dat) =>
                {
                    var result = new ValidationResult();

                    var seenHolders = new HashSet<(long Number, SlcAsset_Management.Enums.HierarchyRoleEnum? HierarchyRole)>();

                    foreach (var holder in dat.Object.Holders)
                    {
                        if (holder.Number == null)
                        {
                            result.AddFailReason(AssetValidationField.HolderSlotNumber, "Holder Slot Number must have a value.");
                            return result;
                        }

                        if (holder.Number.Value < 0)
                        {
                            result.AddFailReason(AssetValidationField.HolderSlotNumber, "Holder Slot Number cannot be negative.");
                            return result;
                        }

                        var holderKey = (holder.Number.Value, holder.HierarchyRole);
                        if (!seenHolders.Add(holderKey))
                        {
                            result.AddFailReason(AssetValidationField.HolderSlotNumber, $"Multiple Holders have the same Slot Number '{holder.Number}' and Hierarchy Role '{holder.HierarchyRole}'.");
                            return result;
                        }
                    }

                    return result;
                });

            validationFactory.Validate(asset, new ValidatorContext<AssetWrapper>() { ReturnWhenInvalid = true }, out var assetValidationResult);
            return assetValidationResult;
        }

        public static bool IsValidHolderSlot(AssetWrapper parentAsset, long slotNumber, SlcAsset_Management.Enums.HierarchyRoleEnum hierarchyRole, out ValidationResult result)
        {
            result = new ValidationResult();
            if (parentAsset == null)
            {
                result.AddFailReason(AssetValidationField.Asset, $"An Asset must be provided.");
                return result.IsValid;
            }

            if (slotNumber < 0)
            {
                result.AddFailReason(AssetValidationField.HolderSlotNumber, $"The slot number cannot be negative.");
                return result.IsValid;
            }

            if (parentAsset.Holders == null)
            {
                result.AddFailReason(AssetValidationField.HolderSlotNumber, $"Asset does not contain Holders.");
                return result.IsValid;
            }

            if (parentAsset.Holders.Any(h => h.Number == slotNumber && h.HierarchyRole == hierarchyRole))
            {
                result.AddFailReason(AssetValidationField.HolderSlotNumber, $"Asset already contain a Holder with slot number '{slotNumber}' and role '{hierarchyRole}'.");
                return result.IsValid;
            }

            return result.IsValid;
        }

        #endregion

        #region Elements

        public static ValidationResult ValidateAssetElements(AssetWrapper asset)
        {
            var validationFactory = ValidationFactory<AssetWrapper>
                .PrepareValidation(
                (dat) => dat.Object.ElementsField.Changed,
                (dat) =>
                {
                    var result = new ValidationResult();

                    var seenHolders = new HashSet<string>();

                    bool primaryFound = false;
                    foreach (var element in dat.Object.Elements)
                    {
                        if (element.IsPrimary)
                        {
                            if (primaryFound)
                            {
                                result.AddFailReason(AssetValidationField.Element, $"Multiple primary Elements found.");
                                return result;
                            }
                            else
                            {
                                primaryFound = true;
                            }
                        }

                        var holderKey = element.ElementID;
                        if (!seenHolders.Add(holderKey))
                        {
                            result.AddFailReason(AssetValidationField.Element, $"Multiple Elements found.");
                            return result;
                        }
                    }

                    return result;
                });

            validationFactory.Validate(asset, new ValidatorContext<AssetWrapper>() { ReturnWhenInvalid = true }, out var assetValidationResult);
            return assetValidationResult;
        }

        #endregion
    }
}