namespace Skyline.DataMiner.SDM.AssetManagement.Common.Validation
{
	using System;
	using System.Collections.Generic;
	using Skyline.DataMiner.Net.Helper;
	using Skyline.DataMiner.Utils.InfraOps.Common.DOM_Classes.DOM.Applications.All.Validations;
	using Skyline.DataMiner.Utils.InfraOps.Common.DOM_Classes.DOM.Applications.Asset_Manager.Wrappers;

	public static class CableTypeValidationHandler
	{
		public enum CableTypeValidationField
		{
			Name,

			Category,
		}

		public static ValidationResult ValidateCableType(CableTypeWrapper cableType, ValidatorContext<CableTypeWrapper> context)
		{
			List<Func<ValidationResult>> validations = new List<Func<ValidationResult>>()
			{
				() => ValidateCableTypeInfo(cableType, context),
				() => ValidateCableTypeCategories(cableType, context),
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

		#region Cable Type Information

		private static ValidationResult ValidateCableTypeInfo(CableTypeWrapper cableType, ValidatorContext<CableTypeWrapper> context)
		{
			ValidationResult result = new ValidationResult();
			if (cableType.NameField.Changed && !IsCableTypeNameValid(cableType.ModuleHandlers, cableType.Name, context, out var nameResult))
			{
				result.CombineResults(nameResult);
			}

			return result;
		}

		public static bool IsCableTypeNameValid(GlobalInfraOpsModuleHandler moduleHandlers, string name, ValidatorContext<CableTypeWrapper> context, out ValidationResult result)
		{
			result = new ValidationResult();
			if (string.IsNullOrWhiteSpace(name))
			{
				result.AddFailReason(CableTypeValidationField.Name, "Cable Type Name cannot be empty or whiteSpace.");
				return result.IsValid;
			}

			foreach (var other in context.OtherChangedEntries)
			{
				if (string.Equals(name, other.Name))
				{
					result.AddFailReason(CableTypeValidationField.Name, "Cable Type Name already in use.");
					return result.IsValid;
				}
			}

			if (moduleHandlers.CableTypeHandler.IsNameInUse(name, context.ChangedEntries))
			{
				result.AddFailReason(CableTypeValidationField.Name, "Cable Type Name already in use.");
				return result.IsValid;
			}

			return result.IsValid;
		}

		#endregion

		#region Category

		private static ValidationResult ValidateCableTypeCategories(CableTypeWrapper cableType, ValidatorContext<CableTypeWrapper> context)
		{
			ValidationResult result = new ValidationResult();
			if (cableType.CategoriesField.Changed && !IsCableTypeCategoriesValid(cableType, out var categoryResult))
			{
				result.CombineResults(categoryResult);
			}

			return result;
		}

		private static bool IsCableTypeCategoriesValid(CableTypeWrapper cableType, out ValidationResult result)
		{
			result = new ValidationResult();

			if(cableType.Categories.IsNullOrEmpty())
			{
				result.AddFailReason(CableTypeValidationField.Category, "Cable Type must have at least one category.");
			}

			return result.IsValid;
		}

		#endregion
	}
}