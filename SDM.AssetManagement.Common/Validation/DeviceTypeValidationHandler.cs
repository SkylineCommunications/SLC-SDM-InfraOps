namespace Skyline.DataMiner.SDM.AssetManagement.Common.Validation
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Utils.InfraOps.Common.DOM_Classes.DOM.Applications.All.Validations;
	using Skyline.DataMiner.Utils.InfraOps.Common.DOM_Classes.DOM.Applications.Asset_Manager.Wrappers;
	using Skyline.DataMiner.Utils.InfraOps.Common.DOM_Classes.DOM.Applications.DomIds;

	public static class DeviceTypeValidationHandler
	{
		public enum DeviceTypeValidationField
		{
			Name,
			DeviceType,
		}

		public static ValidationResult ValidateDeviceType(this DeviceTypeWrapper deviceType, ValidatorContext<DeviceTypeWrapper> context)
		{
			List<Func<ValidationResult>> validations = new List<Func<ValidationResult>>()
			{
				() => ValidateDeviceTypeInfo(deviceType, context),
				() => ValidateDeviceTypeTags(deviceType, context),
				() => ValidateDeviceTypeHierarchyRoleInformation(deviceType, context),
				() => ValidateDeviceTypeConnectionPanel(deviceType, context),
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

		#region Device Type Information

		private static ValidationResult ValidateDeviceTypeInfo(DeviceTypeWrapper deviceType, ValidatorContext<DeviceTypeWrapper> context)
		{
			ValidationResult result = new ValidationResult();
			if (deviceType.NameField.Changed && !IsDeviceTypeNameValid(deviceType.ModuleHandlers, deviceType.Name, context, out var nameResult))
			{
				result.CombineResults(nameResult);
			}

			if (context.ReturnWhenInvalid && !result.IsValid)
			{
				return result;
			}

			return result;
		}

		public static bool IsDeviceTypeNameValid(GlobalInfraOpsModuleHandler moduleHandlers, string name, ValidatorContext<DeviceTypeWrapper> context, out ValidationResult result)
		{
			result = new ValidationResult();
			if (string.IsNullOrWhiteSpace(name))
			{
				result.AddFailReason(DeviceTypeValidationField.Name, "Device Type Name cannot be empty or whiteSpace.");
				return result.IsValid;
			}

			foreach (var other in context.OtherChangedEntries)
			{
				if (string.Equals(name, other.Name))
				{
					result.AddFailReason(DeviceTypeValidationField.Name, "Device Type Name already in use.");
					return result.IsValid;
				}
			}

			if (moduleHandlers.DeviceTypeHandler.IsNameInUse(name, context.ChangedEntries))
			{
				result.AddFailReason(DeviceTypeValidationField.Name, "Device Type Name already in use.");
				return result.IsValid;
			}

			return result.IsValid;
		}

		#endregion

		#region Tags

		private static ValidationResult ValidateDeviceTypeTags(DeviceTypeWrapper deviceType, ValidatorContext<DeviceTypeWrapper> context)
		{
			ValidationResult result = new ValidationResult();

			return result;
		}

		#endregion

		#region Hierarchy Role Information

		private static ValidationResult ValidateDeviceTypeHierarchyRoleInformation(DeviceTypeWrapper deviceType, ValidatorContext<DeviceTypeWrapper> context)
		{
			ValidationResult result = new ValidationResult();

			return result;
		}

		#endregion

		#region Connection Panel

		private static ValidationResult ValidateDeviceTypeConnectionPanel(DeviceTypeWrapper deviceType, ValidatorContext<DeviceTypeWrapper> context)
		{
			ValidationResult result = new ValidationResult();

			return result;
		}

		#endregion

		public static bool IsInUse(DeviceTypeWrapper deviceType, out ValidationResult result)
		{
			result = new ValidationResult();

			var assets = deviceType.ModuleHandlers.AssetHandler.GetWrappersByAssetClassDeviceType(deviceType);
			if (assets.Any(asset => asset.Status != AssetStatus.NotAvailable && asset.Status != AssetStatus.Disposed))
			{
				result.AddFailReason(DeviceTypeValidationField.DeviceType, "There are already assets assigned to this device type not in the 'Not Available' or 'Disposed' State");
				return false;
			}

			return true;
		}

		public static ValidationResult IsRackAttacheable(DeviceTypeWrapper deviceType)
		{
			ValidationResult result = new ValidationResult();

			if (deviceType == null)
			{
				result.AddFailReason(DeviceTypeValidationField.DeviceType, "Device Type must be provided.");
				return result;
			}

			if (!deviceType.HasTag(SlcAsset_Management.Enums.TagOption.RackUnitConsumer))
			{
				result.AddFailReason(DeviceTypeValidationField.DeviceType, "Device Type lacks the 'Rack Unit Consumer' Tag.");
				return result;
			}

			return result;
		}
	}
}