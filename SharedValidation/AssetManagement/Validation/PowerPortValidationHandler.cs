//namespace Skyline.DataMiner.SDM.AssetManagement.Common.Validation
//{
//	using System;
//	using System.Collections.Generic;
//	using System.Linq;
//	using Skyline.DataMiner.Utils.InfraOps.Common.DOM_Classes.DOM.Applications.All.Validations;
//	using Skyline.DataMiner.Utils.InfraOps.Common.DOM_Classes.DOM.Applications.Asset_Manager.Wrappers;
//	using static Skyline.DataMiner.Utils.InfraOps.Common.DOM_Classes.DOM.Applications.Asset_Manager.Validations.PortWrapperValidationHandler;

//	public static class PowerPortValidationHandler
//	{
//		public enum PowerPortValidationField
//		{
//			PowerPort,
//		}

//		public static ValidationResult ValidatePowerPort(PowerPortWrapper dataPort, ValidatorContext<PowerPortWrapper> context)
//		{
//			List<Func<ValidationResult>> validations = new List<Func<ValidationResult>>()
//			{
//				() => ValidatePowerPortInfo(dataPort, context),
//				() => ValidatePowerPortAsset(dataPort, context),
//			};

//			ValidationResult result = new ValidationResult();
//			foreach (var validation in validations)
//			{
//				result.CombineResults(validation());

//				if (context.ReturnWhenInvalid && !result.IsValid)
//				{
//					return result;
//				}
//			}

//			return result;
//		}

//		#region Data Port Info

//		private static ValidationResult ValidatePowerPortInfo(PowerPortWrapper dataPort, ValidatorContext<PowerPortWrapper> context)
//		{
//			ValidationResult result = new ValidationResult();
//			if ((dataPort.NameField.Changed || dataPort.PortNumberField.Changed) && !PortWrapperValidationHandler.ValidateMandatoryPortFields(dataPort.Name, dataPort.PortNumber, out var mandatoryPortResults))
//			{
//				result.CombineResults(mandatoryPortResults);
//			}

//			if (context.ReturnWhenInvalid && !result.IsValid)
//			{
//				return result;
//			}

//			if (dataPort.PortTypeIdField.Changed && !isPortTypeValid(dataPort, out var portTypeResult))
//			{
//				result.CombineResults(portTypeResult);
//			}

//			if (context.ReturnWhenInvalid && !result.IsValid)
//			{
//				return result;
//			}

//			return result;
//		}

//		public static bool IsPortNumberValid(AssetWrapper asset, long portNumber, ValidatorContext<PowerPortWrapper> context, out ValidationResult result)
//		{
//			result = new ValidationResult();
//			if (asset.PowerPorts.Any(p => p.InstanceId != context.BaseEntry?.InstanceId && p.PortNumber == portNumber))
//			{
//				result.AddFailReason(PowerPortValidationField.PowerPort, "Port number is already in use on the asset.");
//				return result.IsValid;
//			}

//			return result.IsValid;
//		}

//		public static bool IsPortNumberValid(AssetClassWrapper assetClass, long portNumber, ValidatorContext<PowerPortWrapper> context, out ValidationResult result)
//		{
//			result = new ValidationResult();
//			if (context.BaseEntry?.PortNumber != portNumber && assetClass.PowerPortInfos.Count(p => p.PortNumber == portNumber) > 1)
//			{
//				result.AddFailReason(PowerPortValidationField.PowerPort, "Port number is already in use on the asset Class.");
//				return result.IsValid;
//			}

//			return result.IsValid;
//		}

//		public static bool isPortTypeValid(PowerPortWrapper dataPort, out ValidationResult result)
//		{
//			result = new ValidationResult();

//			if (!dataPort.HasPortType)
//			{
//				result.AddFailReason(PortValidationField.PortType, "Port Type cannot be empty.");
//				return result.IsValid;
//			}

//			var dataPortType = dataPort.PortType;

//			if (!dataPortType.IsPowerPortType())
//			{
//				result.AddFailReason(PortValidationField.PortType, "Port Type must be a Power Port Type.");
//				return result.IsValid;
//			}

//			return result.IsValid;
//		}

//		#endregion

//		#region Asset

//		private static ValidationResult ValidatePowerPortAsset(PowerPortWrapper dataPort, ValidatorContext<PowerPortWrapper> context)
//		{
//			ValidationResult result = new ValidationResult();
//			if (dataPort.AssetIdField.Changed && !IsPowerPortAssetValid(dataPort, context, out var nameResult))
//			{
//				result.CombineResults(nameResult);
//			}

//			if (context.ReturnWhenInvalid && !result.IsValid)
//			{
//				return result;
//			}

//			return result;
//		}

//		private static bool IsPowerPortAssetValid(PowerPortWrapper dataPort, ValidatorContext<PowerPortWrapper> context, out ValidationResult result)
//		{
//			result = new ValidationResult();

//			if (!dataPort.HasAsset)
//			{
//				result.AddFailReason(PowerPortValidationField.PowerPort, "Power port must be linked to an asset.");
//				return result.IsValid;
//			}

//			if (context.ReturnWhenInvalid && !result.IsValid)
//			{
//				return result.IsValid;
//			}

//			if (AssetValidationHandler.IsValidPowerPortAssetRelation(dataPort.Asset.PowerPorts, out ValidationResult relationResult))
//			{
//				result.CombineResults(relationResult);
//				return result.IsValid;
//			}

//			return result.IsValid;
//		}

//		#endregion
//	}
//}