namespace Skyline.DataMiner.SDM.AssetManagement.Common.Validation
{
    using System.Linq;

    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

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

		#region Info

		
        public static bool IsAssetClassDeviceTypeValid(AssetClass asset, out ValidationResult result)
		{
			result = new ValidationResult();
			if (!asset.DeviceTypeId.HasValue())
			{
				result.AddFailReason(AssetClassValidationField.DeviceTypeId, "Asset Class Device Type cannot be empty.");
				return result.IsValid;
			}

			return result.IsValid;
		}

		public static bool IsDepthValid(AssetClass asset, out ValidationResult result)
		{
			result = new ValidationResult();
			if(asset.Depth < 0)
			{
				result.AddFailReason(AssetClassValidationField.Depth, $"The depth cannot be negative.");
				return result.IsValid;
			}

			return result.IsValid;
		}

		public static bool IsWidthValid(AssetClass asset, out ValidationResult result)
		{
			result = new ValidationResult();
			if (asset.Width < 0)
			{
				result.AddFailReason(AssetClassValidationField.Width, $"The width cannot be negative.");
				return result.IsValid;
			}

			return result.IsValid;
		}

		public static bool IsHeightValid(AssetClass asset, out ValidationResult result)
		{
			result = new ValidationResult();
			if (asset.Height < 0)
			{
				result.AddFailReason(AssetClassValidationField.Height, $"The height cannot be negative.");
				return result.IsValid;
			}

			return result.IsValid;
		}

		public static bool IsHeightUnitValid(AssetClass asset, out ValidationResult result)
		{
			result = new ValidationResult();
			if (asset.HeightU < 0)
			{
				result.AddFailReason(AssetClassValidationField.HeightU, $"The height Units cannot be negative.");
				return result.IsValid;
			}

			return result.IsValid;
		}

		public static bool IsWeightValid(AssetClass asset, out ValidationResult result)
		{
			result = new ValidationResult();
			if (asset.Weight < 0)
			{
				result.AddFailReason(AssetClassValidationField.Weight, $"The weight cannot be negative.");
				return result.IsValid;
			}

			return result.IsValid;
		}

		public static bool IsTypicalPowerConsumptionValid(AssetClass asset, out ValidationResult result)
		{
			result = new ValidationResult();
			if (asset.TypicalPowerConsumption < 0)
			{
				result.AddFailReason(AssetClassValidationField.TypicalPowerConsumption, $"The typical power consumption cannot be negative.");
				return result.IsValid;
			}

			return result.IsValid;
		}

		public static bool IsMaxPowerConsumptionValid(AssetClass asset, out ValidationResult result)
		{
			result = new ValidationResult();
			if (asset.MaximumPowerConsumption < 0)
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

			if (assetClass.Height <= 0)
			{
				result.AddFailReason(AssetClassValidationField.AssetClass, "Asset Class must have a defined height higher than 0.");
				return result.IsValid;
			}

			result = result.CombineResults(DeviceTypeValidationHandler.IsRackAttacheable(assetClass.DeviceType));
			return result.IsValid;
		}

		#endregion


        #region Data Ports

        internal static ValidationResult ValidateAssetClassDataPort(AssetClass assetClass)
		{
			ValidationResult result = new ValidationResult();

			if (assetClass.DataPortsField.Changed)
			{
				foreach (var port in assetClass.DataPorts)
				{
					if (port.PortNumber < 0)
					{
						result.AddFailReason(AssetClassValidationField.DataPortNumber, "Data Port Number cannot be negative.");
						return result;
					}

					foreach (var otherPorts in assetClass.DataPorts)
					{
						if (otherPorts == port)
						{
							continue;
						}

						if (otherPorts.PortNumber == port.PortNumber && otherPorts.Type == port.Type)
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

		internal static ValidationResult ValidateAssetClassPowerPort(AssetClass assetClass)
		{
			ValidationResult result = new ValidationResult();

			if (assetClass.PowerPortsField.Changed)
			{
				foreach (var port in assetClass.PowerPorts)
				{
					if (port.PortNumber < 0)
					{
						result.AddFailReason(AssetClassValidationField.PowerPortNumber, "Power Port Number cannot be negative.");
						return result;
					}

					foreach (var otherPorts in assetClass.PowerPorts)
					{
						if (otherPorts == port)
						{
							continue;
						}

						if (otherPorts.PortNumber == port.PortNumber && otherPorts.PortType == port.PortType)
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

        internal static ValidationResult ValidateAssetClassHolders(AssetClass assetClass)
		{
			ValidationResult result = new ValidationResult();

			if (assetClass.HoldersField.Changed)
			{
				foreach (var holder in assetClass.Holders)
				{
					if (holder.SlotNumber < 0)
					{
						result.AddFailReason(AssetClassValidationField.HolderSlotNumber, "Holder Slot Number cannot be negative.");
						return result;
					}

					foreach (var otherHolder in assetClass.Holders)
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

		public static bool IsValidHolderSlot(AssetClass assetClass, long slotNumber, SharedMappers.DomIds.SlcAsset_Management.Enums.HierarchyRoleEnum hierarchyRole, out ValidationResult result)
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