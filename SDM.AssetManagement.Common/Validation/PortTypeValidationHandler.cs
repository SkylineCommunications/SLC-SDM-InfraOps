namespace Skyline.DataMiner.SDM.AssetManagement.Common.Validation
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.Net.Helper;
	using Skyline.DataMiner.Utils.InfraOps.Common.DOM_Classes.DOM.Applications.All.Validations;
	using Skyline.DataMiner.Utils.InfraOps.Common.DOM_Classes.DOM.Applications.Asset_Manager.Wrappers;

	public static class PortTypeValidationHandler
	{
		public enum PortTypeValidationField
		{
			Name,
			Category,
		}

		public static ValidationResult ValidatePortType(this PortTypeWrapper portType, ValidatorContext<PortTypeWrapper> context)
		{
			List<Func<ValidationResult>> validations = new List<Func<ValidationResult>>()
			{
				() => ValidatePortTypeInfo(portType, context),
				() => ValidatePortTypeCategory(portType, context),
				() => ValidatePortTypeCableCompatibility(portType, context),
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

		#region Port Type Information

		private static ValidationResult ValidatePortTypeInfo(PortTypeWrapper portType, ValidatorContext<PortTypeWrapper> context)
		{
			ValidationResult result = new ValidationResult();
			if (portType.NameField.Changed && !IsPortTypeNameValid(portType.ModuleHandlers, portType.Name, context, out var nameResult))
			{
				result.CombineResults(nameResult);
			}

			return result;
		}

		public static bool IsPortTypeNameValid(GlobalInfraOpsModuleHandler moduleHandlers, string name, ValidatorContext<PortTypeWrapper> context, out ValidationResult result)
		{
			result = new ValidationResult();
			if (string.IsNullOrWhiteSpace(name))
			{
				result.AddFailReason(PortTypeValidationField.Name, "Port Type Name cannot be empty or whiteSpace.");
				return result.IsValid;
			}

			foreach (var other in context.OtherChangedEntries)
			{
				if (string.Equals(name, other.Name))
				{
					result.AddFailReason(PortTypeValidationField.Name, "Port Type Name already in use.");
					return result.IsValid;
				}
			}

			if (moduleHandlers.PortTypeHandler.IsNameInUse(name, context.ChangedEntries))
			{
				result.AddFailReason(PortTypeValidationField.Name, "Port Type Name already in use.");
				return result.IsValid;
			}

			return result.IsValid;
		}

		#endregion

		#region Category

		private static ValidationResult ValidatePortTypeCategory(PortTypeWrapper portType, ValidatorContext<PortTypeWrapper> context)
		{
			ValidationResult result = new ValidationResult();

			if (portType.Categories.IsNullOrEmpty())
			{
				result.AddFailReason(PortTypeValidationField.Category, "Port Type must contain at least one category.");
			}

			return result;
		}

		#endregion

		#region Cable Compatibility

		private static ValidationResult ValidatePortTypeCableCompatibility(PortTypeWrapper portType, ValidatorContext<PortTypeWrapper> context)
		{
			ValidationResult result = new ValidationResult();

			return result;
		}

		#endregion

		// TODO: implement "IsInUse" validation for Port Types to prevent accidental Deletetions of instances that are still in use.
	}
}