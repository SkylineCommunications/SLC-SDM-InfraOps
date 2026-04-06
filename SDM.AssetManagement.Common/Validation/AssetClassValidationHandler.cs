namespace Skyline.DataMiner.SDM.AssetManagement.Common.Validation
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;
    using Skyline.DataMiner.Utils.InfraOps.Validations;

    using static Skyline.DataMiner.Utils.InfraOps.Common.DOM_Classes.DOM.Applications.Asset_Manager.Validations.AssetValidationHandler;

	public static class AssetClassValidationHandler
	{
		public enum AssetClassValidationField
		{
			Name,
			AssetClass,
			DeviceTypeId,
			DeviceTypeName,
			PowerSupply,

			Depth,
			Width,
			Height,
			HeightU,
			Weight,

			TypicalPowerConsumption,
			MaxPowerConsumption,

			DataPortNumber,

			PowerPortNumber,

			HolderSlotNumber,
		}

		public static ValidationResult ValidateAssetClass(AssetClass assetClass, ValidatorContext<AssetClass> context)
		{
			List<Func<ValidationResult>> validations = new List<Func<ValidationResult>>()
			{
				() => ValidateAssetClassInfo(assetClass, context),
				() => ValidateAssetClassLifecycleInfo(assetClass, context),
				() => ValidateAssetClassDataPort(assetClass),
				() => ValidateAssetClassPowerPort(assetClass),
				() => ValidateAssetClassHolders(assetClass),
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

		private static ValidationResult ValidateAssetClassInfo(AssetClass assetClass, ValidatorContext<AssetClass> context)
		{
			ValidationResult result = new ValidationResult();
			if (assetClass.DeviceName.Changed && !IsAssetClassNameValid(assetClass.ModuleHandlers, assetClass.DeviceName.Value, context, out var nameResult))
			{
				result.CombineResults(nameResult);
			}

			if (context.ReturnWhenInvalid && !result.IsValid)
			{
				return result;
			}

			if (!assetClass.HasDeviceType || assetClass.DeviceTypeIdField.Changed || assetClass.PowerSupplyField.Changed)
			{
				if (!IsAssetClassDeviceTypeValid(assetClass, out var deviceTypeResult))
				{
					result.CombineResults(deviceTypeResult);
				}

				if (context.ReturnWhenInvalid && !result.IsValid)
				{
					return result;
				}

				if (assetClass.HasDeviceType && !IsAssetClassPowerSupplyValid(assetClass, out var powerSupplyResult))
				{
					result.CombineResults(powerSupplyResult);
				}

				if (context.ReturnWhenInvalid && !result.IsValid)
				{
					return result;
				}
			}

			if (assetClass.DepthField.Changed && !IsDepthValid(assetClass, out var depthResult))
			{
				result.CombineResults(depthResult);
			}

			if (context.ReturnWhenInvalid && !result.IsValid)
			{
				return result;
			}

			if (assetClass.WidthField.Changed && !IsWidthValid(assetClass, out var widthResult))
			{
				result.CombineResults(widthResult);
			}

			if (context.ReturnWhenInvalid && !result.IsValid)
			{
				return result;
			}

			if (assetClass.HeightField.Changed && !IsHeightValid(assetClass, out var heightResult))
			{
				result.CombineResults(heightResult);
			}

			if (context.ReturnWhenInvalid && !result.IsValid)
			{
				return result;
			}

			if (assetClass.HeightUField.Changed && !IsHeightUnitValid(assetClass, out var heightUnitResult))
			{
				result.CombineResults(heightUnitResult);
			}

			if (context.ReturnWhenInvalid && !result.IsValid)
			{
				return result;
			}

			if (assetClass.WeightField.Changed && !IsWeightValid(assetClass, out var weightResult))
			{
				result.CombineResults(weightResult);
			}

			if (context.ReturnWhenInvalid && !result.IsValid)
			{
				return result;
			}

			if (assetClass.TypicalPowerConsumptionField.Changed && !IsTypicalPowerConsumptionValid(assetClass, out var typicalPCResult))
			{
				result.CombineResults(typicalPCResult);
			}

			if (context.ReturnWhenInvalid && !result.IsValid)
			{
				return result;
			}

			if (assetClass.MaximumPowerConsumptionField.Changed && !IsMaxPowerConsumptionValid(assetClass, out var maxPCResult))
			{
				result.CombineResults(maxPCResult);
			}

			if (context.ReturnWhenInvalid && !result.IsValid)
			{
				return result;
			}

			return result;
		}

		public static bool IsAssetClassNameValid(GlobalInfraOpsModuleHandler moduleHandlers, string assetName, ValidatorContext<AssetClass> context, out ValidationResult result)
		{
			result = new ValidationResult();

			if (string.IsNullOrWhiteSpace(assetName))
			{
				result.AddFailReason(AssetClassValidationField.Name, "Asset Class Name cannot be empty or whiteSpace.");
				return result.IsValid;
			}

			foreach (var otherAsset in context.OtherChangedEntries)
			{
				if (string.Equals(assetName, otherAsset.Name))
				{
					result.AddFailReason(AssetClassValidationField.Name, "Asset Class Name already in use.");
					return result.IsValid;
				}
			}

			if (moduleHandlers.AssetClassHandler.IsNameInUse(assetName, context.ChangedEntries))
			{
				result.AddFailReason(AssetClassValidationField.Name, "Asset Class Name Already in Use.");
				return result.IsValid;
			}

			return result.IsValid;
		}

		public static bool IsAssetClassDeviceTypeValid(AssetClass asset, out ValidationResult result)
		{
			result = new ValidationResult();
			if (!asset.HasDeviceType)
			{
				result.AddFailReason(AssetClassValidationField.DeviceTypeId, "Asset Class Device Type cannot be empty.");
				return result.IsValid;
			}

			return result.IsValid;
		}

		public static bool IsAssetClassPowerSupplyValid(AssetClass asset, out ValidationResult result)
		{
			result = new ValidationResult();
			if (asset.HasDeviceType)
			{
				var deviceType = asset.DeviceType;

				if (deviceType.HasTag(SlcAsset_Management.Enums.TagOption.PowerProvider) && asset.PowerSupply == null)
				{
					result.AddFailReason(AssetClassValidationField.PowerSupply, "Asset Class Device Type with 'Power Provider' must have a Power Supply.");
					return result.IsValid;
				}
			}

			return result.IsValid;
		}

		public static bool IsDepthValid(AssetClass asset, out ValidationResult result)
		{
			result = new ValidationResult();
			if(asset.DepthOrDefault < 0)
			{
				result.AddFailReason(AssetClassValidationField.Depth, $"The depth cannot be negative.");
				return result.IsValid;
			}

			return result.IsValid;
		}

		public static bool IsWidthValid(AssetClass asset, out ValidationResult result)
		{
			result = new ValidationResult();
			if (asset.WidthOrDefault < 0)
			{
				result.AddFailReason(AssetClassValidationField.Width, $"The width cannot be negative.");
				return result.IsValid;
			}

			return result.IsValid;
		}

		public static bool IsHeightValid(AssetClass asset, out ValidationResult result)
		{
			result = new ValidationResult();
			if (asset.HeightOrDefault < 0)
			{
				result.AddFailReason(AssetClassValidationField.Height, $"The height cannot be negative.");
				return result.IsValid;
			}

			return result.IsValid;
		}

		public static bool IsHeightUnitValid(AssetClass asset, out ValidationResult result)
		{
			result = new ValidationResult();
			if (asset.HeightUOrDefault < 0)
			{
				result.AddFailReason(AssetClassValidationField.HeightU, $"The height Units cannot be negative.");
				return result.IsValid;
			}

			return result.IsValid;
		}

		public static bool IsWeightValid(AssetClass asset, out ValidationResult result)
		{
			result = new ValidationResult();
			if (asset.WeightOrDefault < 0)
			{
				result.AddFailReason(AssetClassValidationField.Weight, $"The weight cannot be negative.");
				return result.IsValid;
			}

			return result.IsValid;
		}

		public static bool IsTypicalPowerConsumptionValid(AssetClass asset, out ValidationResult result)
		{
			result = new ValidationResult();
			if (asset.TypicalPowerConsumptionOrDefault < 0)
			{
				result.AddFailReason(AssetClassValidationField.TypicalPowerConsumption, $"The typical power consumption cannot be negative.");
				return result.IsValid;
			}

			return result.IsValid;
		}

		public static bool IsMaxPowerConsumptionValid(AssetClass asset, out ValidationResult result)
		{
			result = new ValidationResult();
			if (asset.MaximumPowerConsumptionOrDefault < 0)
			{
				result.AddFailReason(AssetClassValidationField.MaxPowerConsumption, $"The max power consumption cannot be negative.");
				return result.IsValid;
			}

			return result.IsValid;
		}

		public static bool IsRackAttacheable(AssetClass assetClass, out ValidationResult result)
		{
			result = new ValidationResult();

			if (assetClass == null)
			{
				result.AddFailReason(AssetClassValidationField.AssetClass, "Asset Class must be provided.");
				return result.IsValid;
			}

			if (assetClass.HeightOrDefault <= 0)
			{
				result.AddFailReason(AssetClassValidationField.AssetClass, "Asset Class must have a defined height higher than 0.");
				return result.IsValid;
			}

			result = result.CombineResults(DeviceTypeValidationHandler.IsRackAttacheable(assetClass.DeviceType));
			return result.IsValid;
		}

		#endregion

		#region LifecycleInfo

		private static ValidationResult ValidateAssetClassLifecycleInfo(AssetClass assetClass, ValidatorContext<AssetClass> context)
		{
			ValidationResult result = new ValidationResult();

			return result;
		}

		#endregion

		#region Data Ports

		private static ValidationResult ValidateAssetClassDataPort(AssetClass assetClass)
		{
			ValidationResult result = new ValidationResult();

			if (asset.DataPortInfosField.Changed)
			{
				foreach (var port in asset.DataPortInfos)
				{
					if (port.PortNumber < 0)
					{
						result.AddFailReason(AssetClassValidationField.DataPortNumber, "Data Port Number cannot be negative.");
						return result;
					}

					foreach (var otherPorts in asset.DataPortInfos)
					{
						if (otherPorts == port)
						{
							continue;
						}

						if (otherPorts.PortNumber == port.PortNumber && otherPorts.PortTypeId == port.PortTypeId)
						{
							result.AddFailReason(AssetClassValidationField.DataPortNumber, $"Multiple Data Ports have the same Port Number '{port.PortNumber}' and Port Type '{port.PortType.Name}'.");
							return result;
						}
					}
				}
			}

			return result;
		}

		#endregion

		#region Power Ports

		private static ValidationResult ValidateAssetClassPowerPort(AssetClass assetClass)
		{
			ValidationResult result = new ValidationResult();

			if (asset.PowerPortInfosField.Changed)
			{
				foreach (var port in asset.PowerPortInfos)
				{
					if (port.PortNumber < 0)
					{
						result.AddFailReason(AssetClassValidationField.PowerPortNumber, "Power Port Number cannot be negative.");
						return result;
					}

					foreach (var otherPorts in asset.PowerPortInfos)
					{
						if (otherPorts == port)
						{
							continue;
						}

						if (otherPorts.PortNumber == port.PortNumber && otherPorts.PortTypeId == port.PortTypeId)
						{
							result.AddFailReason(AssetClassValidationField.PowerPortNumber, $"Multiple Power Ports have the same Port Number '{port.PortNumber}' and Port Type '{port.PortType.Name}'.");
							return result;
						}
					}
				}
			}

			return result;
		}

		#endregion

		#region Holders

		private static ValidationResult ValidateAssetClassHolders(AssetClass assetClass)
		{
			ValidationResult result = new ValidationResult();

			if (asset.HoldersField.Changed)
			{
				foreach (var holder in asset.Holders)
				{
					if (holder.SlotNumber < 0)
					{
						result.AddFailReason(AssetClassValidationField.HolderSlotNumber, "Holder Slot Number cannot be negative.");
						return result;
					}

					foreach (var otherHolder in asset.Holders)
					{
						if (otherHolder == holder)
						{
							continue;
						}

						if (otherHolder.SlotNumber == holder.SlotNumber && otherHolder.HierarchyRole == holder.HierarchyRole)
						{
							result.AddFailReason(AssetClassValidationField.HolderSlotNumber, $"Multiple Holders have the same Slot Number '{holder.SlotNumber}' and Hierarchy Role '{holder.HierarchyRole}'.");
							return result;
						}
					}
				}
			}

			return result;
		}

		public static bool IsValidHolderSlot(AssetClass assetClass, long slotNumber, SlcAsset_Management.Enums.HierarchyRoleEnum hierarchyRole, out ValidationResult result)
		{
			result = new ValidationResult();
			if (assetClass == null)
			{
				result.AddFailReason(AssetClassValidationField.AssetClass, $"An Asset Class must be provided.");
				return result.IsValid;
			}

			if (assetClass.Holders == null)
			{
				result.AddFailReason(AssetValidationField.HolderSlotNumber, $"Asset Class does not contain Holders.");
				return result.IsValid;
			}

			if (slotNumber < 0)
			{
				result.AddFailReason(AssetClassValidationField.HolderSlotNumber, $"The slot number cannot be negative.");
				return result.IsValid;
			}

			if (assetClass.Holders.Any(h => h.SlotNumber == slotNumber && h.HierarchyRole == hierarchyRole))
			{
				result.AddFailReason(AssetClassValidationField.HolderSlotNumber, $"Asset Class already contains a Holder with slot number '{slotNumber}' and role '{hierarchyRole}'.");
				return result.IsValid;
			}

			return result.IsValid;
		}

		#endregion
	}
}