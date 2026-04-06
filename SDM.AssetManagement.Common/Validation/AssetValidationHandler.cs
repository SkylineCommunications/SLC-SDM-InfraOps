namespace Skyline.DataMiner.Utils.InfraOps.Common.DOM_Classes.DOM.Applications.Asset_Manager.Validations
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Utils.InfraOps.Common.DOM_Classes.DOM.Applications.All.Validations;
	using Skyline.DataMiner.Utils.InfraOps.Common.DOM_Classes.DOM.Applications.Asset_Manager.Wrappers;
	using Skyline.DataMiner.Utils.InfraOps.Common.DOM_Classes.DOM.Applications.DomIds;
	using Skyline.DataMiner.Utils.InfraOps.Common.DOM_Classes.DOM.Applications.Facility_Manager.Validations;

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
			List<Func<ValidationResult>> validations = new List<Func<ValidationResult>> ()
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
			ValidationResult result = new ValidationResult();
			if (asset.NameField.Changed && !IsAssetNameValid(asset.ModuleHandlers, asset.Name, context, out var nameResult))
			{
				result.CombineResults(nameResult);
			}

			if (context.ReturnWhenInvalid && !result.IsValid)
			{
				return result;
			}

			if (asset.AssetIDField.Changed && !IsAssetIdValid(asset.ModuleHandlers, asset.AssetID, context, out var assetIdResult))
			{
				result.CombineResults(assetIdResult);
			}

			if (context.ReturnWhenInvalid && !result.IsValid)
			{
				return result;
			}

			if ((!asset.HasAssetClass || asset.AssetClassIdField.Changed) && !IsAssetClassValid(asset, out var assetClassResult))
			{
				result.CombineResults(assetClassResult);
			}

			if (context.ReturnWhenInvalid && !result.IsValid)
			{
				return result;
			}

			if (asset.SerialNumberField.Changed && !IsSerialNumberValid(asset.ModuleHandlers, asset.SerialNumber, asset.AssetClass, context, out var serialNumberResult))
			{
				result.CombineResults(serialNumberResult);
			}

			return result;
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
					result.AddFailReason(AssetValidationField.SerialNumber, "Seria lNumber already in use in the same Asset Class.");
					return result.IsValid;
				}
			}

			if (moduleHandlers.AssetHandler.IsSerialNumberValid(serialNumber, assetClass))
			{
				result.AddFailReason(AssetValidationField.SerialNumber, "Serial Number Already in use in the same Asset Class.");
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
				if(
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

			if(locationExists.Count(entry => entry) > 1)
			{
				result.AddFailReason(AssetValidationField.Asset, "Has multiple Locations attached.");
				if (context.ReturnWhenInvalid && !result.IsValid)
				{
					return result;
				}
			}

			if ((asset.DirectParentAssetIdField.Changed || asset.HolderNumberField.Changed) && !IsValidParentAssetAttachment(asset, out ValidationResult parentAssetAttachmentResult))
			{
				result.CombineResults(parentAssetAttachmentResult);
				if (context.ReturnWhenInvalid && !result.IsValid)
				{
					return result;
				}
			}

			if ((asset.DirectRackIdField.Changed || asset.RackPositionField.Changed || asset.RackSideField.Changed) && !IsValidRackAttachment(asset, out ValidationResult parentRackAttachmentResult))
			{
				result.CombineResults(parentRackAttachmentResult);
				if (context.ReturnWhenInvalid && !result.IsValid)
				{
					return result;
				}
			}

			return result;
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

			if (!parentAsset.Holders.Any(h => h.HierarchyRole == hierarchyRole && h.SlotNumber.HasValue && h.SlotNumber.Value == asset.HolderNumber.Value))
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

			if (!parentAsset.GetAvailableHolders().Any(holder => holder.SlotNumber == slot && holder.HierarchyRole == childHierarchyRole))
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

			if ((asset.DestinationDirectParentAssetIdField.Changed || asset.DestinationHolderNumberField.Changed) && !IsValidDestinationParentAssetAttachment(asset, out ValidationResult parentAssetAttachmentResult))
			{
				result.CombineResults(parentAssetAttachmentResult);
				if (context.ReturnWhenInvalid && !result.IsValid)
				{
					return result;
				}
			}

			if ((asset.DestinationDirectRackIdField.Changed || asset.DestinationRackPositionField.Changed || asset.DestinationRackSideField.Changed) && !IsValidDestinationRackAttachment(asset, out ValidationResult parentRackAttachmentResult))
			{
				result.CombineResults(parentRackAttachmentResult);
				if (context.ReturnWhenInvalid && !result.IsValid)
				{
					return result;
				}
			}

			return result;
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

			if (!parentAsset.Holders.Any(h => h.HierarchyRole == hierarchyRole && h.SlotNumber.HasValue && h.SlotNumber.Value == asset.DestinationHolderNumber.Value))
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
			ValidationResult result = new ValidationResult();

			if ((asset.InstallationUserIdField.Changed || asset.InstallationDateField.Changed) && !IsInstallationValid(asset, out ValidationResult installationResult))
			{
				result.CombineResults(installationResult);
				if (context.ReturnWhenInvalid && !result.IsValid)
				{
					return result;
				}
			}

			if ((asset.ModificationUserIdField.Changed || asset.ModificationDateField.Changed || asset.RackSideField.Changed) && !IsModificationValid(asset, out ValidationResult modificationResult))
			{
				result.CombineResults(modificationResult);
				if (context.ReturnWhenInvalid && !result.IsValid)
				{
					return result;
				}
			}

			return result;
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
			ValidationResult result = new ValidationResult();

			if ((asset.OwnerContactPersonIdField.Changed || asset.OwnerContactPersonRoleIdField.Changed) && !IsOwnerValid(asset, out ValidationResult ownerResult))
			{
				result.CombineResults(ownerResult);
				if (context.ReturnWhenInvalid && !result.IsValid)
				{
					return result;
				}
			}

			return result;
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
			ValidationResult result = new ValidationResult();

			if ((asset.CustodyContactPersonIdField.Changed || asset.CustodyContactPersonRoleIdField.Changed) && !IsCustodyValid(asset, out ValidationResult custodyResult))
			{
				result.CombineResults(custodyResult);
				if (context.ReturnWhenInvalid && !result.IsValid)
				{
					return result;
				}
			}

			return result;
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
			ValidationResult result = new ValidationResult();

			if (asset.DataPortInfosField.Changed && !IsValidDataPortAssetRelation(asset.DataPorts, out var dataPortsResult))
			{
				result.CombineResults(dataPortsResult);
				if (context.ReturnWhenInvalid && !result.IsValid)
				{
					return result;
				}
			}

			return result;
		}

		public static bool IsValidDataPortAssetRelation(IEnumerable<DataPortWrapper> dataPorts, out ValidationResult result)
		{
			result = new ValidationResult();

			bool primaryIpV4Found = false;
			bool primaryIpV6Found = false;
			foreach (var port in dataPorts)
			{
				if (port.PortNumber < 0)
				{
					result.AddFailReason(AssetValidationField.DataPort, "Data Port Number cannot be negative.");
					return result.IsValid;
				}

				if (port.IsPrimaryIpv4Primary)
				{
					if (primaryIpV4Found)
					{
						result.AddFailReason(AssetValidationField.DataPort, $"Multiple primary IPV4 Port found.");
						return result.IsValid;
					}
					else
					{
						primaryIpV4Found = true;
					}
				}

				if (port.IsPrimaryIpv6Primary)
				{
					if (primaryIpV6Found)
					{
						result.AddFailReason(AssetValidationField.DataPort, $"Multiple primary IPV4 Port found.");
						return result.IsValid;
					}
					else
					{
						primaryIpV6Found = true;
					}
				}

				foreach (var otherPort in dataPorts)
				{
					if (otherPort == port)
					{
						continue;
					}

					if (otherPort.PortNumber == port.PortNumber)
					{
						result.AddFailReason(AssetValidationField.DataPort, $"Multiple Data Ports have the same Port Number '{port.PortNumber}' and Port Type '{port.PortType.Name}'.");
						return result.IsValid;
					}
				}
			}

			return result.IsValid;
		}

		#endregion

		#region Power Ports

		public static ValidationResult ValidateAssetPowerPorts(AssetWrapper asset, ValidatorContext<AssetWrapper> context)
		{
			ValidationResult result = new ValidationResult();

			if (asset.PowerPortInfosField.Changed && !IsValidPowerPortAssetRelation(asset.PowerPorts, out var dataPortsResult))
			{
				result.CombineResults(dataPortsResult);
				if (context.ReturnWhenInvalid && !result.IsValid)
				{
					return result;
				}
			}

			return result;
		}

		public static bool IsValidPowerPortAssetRelation(IEnumerable<PowerPortWrapper> powerPorts, out ValidationResult result)
		{
			result = new ValidationResult();

			foreach (var port in powerPorts)
			{
				if (port.PortNumber < 0)
				{
					result.AddFailReason(AssetValidationField.PowerPort, "Power Port Number cannot be negative.");
					return result.IsValid;
				}

				foreach (var otherPort in powerPorts)
				{
					if (otherPort == port)
					{
						continue;
					}

					if (otherPort.PortNumber == port.PortNumber)
					{
						result.AddFailReason(AssetValidationField.PowerPort, $"Multiple Power Ports have the same Port Number '{port.PortNumber}' and Port Type '{port.PortType.Name}'.");
						return result.IsValid;
					}
				}
			}

			return result.IsValid;
		}

		#endregion

		#region Holders

		public static ValidationResult ValidateAssetHolders(AssetWrapper asset)
		{
			ValidationResult result = new ValidationResult();

			if (asset.HoldersField.Changed)
			{
				foreach (var holder in asset.Holders)
				{
					if(holder.SlotNumber < 0)
					{
						result.AddFailReason(AssetValidationField.HolderSlotNumber, "Holder Slot Number cannot be negative.");
						return result;
					}

					foreach (var otherHolder in asset.Holders)
					{
						if(otherHolder == holder)
						{
							continue;
						}

						if(otherHolder.SlotNumber == holder.SlotNumber && otherHolder.HierarchyRole == holder.HierarchyRole)
						{
							result.AddFailReason(AssetValidationField.HolderSlotNumber, $"Multiple Holders have the same Slot Number '{holder.SlotNumber}' and Hierarchy Role '{holder.HierarchyRole}'.");
							return result;
						}
					}
				}
			}

			return result;
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

			if(parentAsset.Holders == null)
			{
				result.AddFailReason(AssetValidationField.HolderSlotNumber, $"Asset does not contain Holders.");
				return result.IsValid;
			}

			if (parentAsset.Holders.Any(h => h.SlotNumber == slotNumber && h.HierarchyRole == hierarchyRole))
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
			ValidationResult result = new ValidationResult();

			if (asset.ElementsField.Changed)
			{
				bool primaryFound = false;
				foreach (var element in asset.Elements)
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

					foreach (var otherElement in asset.Elements)
					{
						if (otherElement == element)
						{
							continue;
						}

						if (otherElement.ElementID == element.ElementID)
						{
							result.AddFailReason(AssetValidationField.Element, $"Multiple Elements found.");
							return result;
						}
					}
				}
			}

			return result;
		}

		#endregion
	}
}