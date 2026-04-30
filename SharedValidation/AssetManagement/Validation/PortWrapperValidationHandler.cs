//namespace Skyline.DataMiner.SDM.AssetManagement.Common.Validation
//{
//	using System;
//	using Skyline.DataMiner.Utils.InfraOps.Common.DOM_Classes.DOM.Applications.All.Validations;

//	/// <summary>
//	/// Reusable validation helpers for DataPort and PowerPort wrappers.
//	/// Eliminates code duplication while keeping wrappers independent.
//	/// </summary>
//	public static class PortWrapperValidationHandler
//	{
//		public enum PortValidationField
//		{
//			PortName,
//			PortNumber,

//			Asset,

//			PortType,
//		}

//		/// <summary>
//		/// Validates mandatory port fields (Name, Number).
//		/// </summary>
//		public static bool ValidateMandatoryPortFields(string portName, long? portNumber, out ValidationResult result)
//		{
//			result = new ValidationResult();

//			if (string.IsNullOrEmpty(portName))
//			{
//				result.AddFailReason(PortValidationField.PortName, "Port Name cannot be empty.");
//			}

//			if (portNumber == null || portNumber < 0)
//			{
//				result.AddFailReason(PortValidationField.PortNumber, "Port Number must be equal or greater than 0.");
//			}

//			return result.IsValid;
//		}

//		/// <summary>
//		/// Validates mandatory asset fields
//		/// </summary>
//		public static bool ValidateMandatoryAssetFields(Guid? assetId, string assetName, out ValidationResult result)
//		{
//			result = new ValidationResult();

//			if (string.IsNullOrWhiteSpace(assetName) && (assetId == null || assetId == Guid.Empty))
//			{
//				result.AddFailReason(PortValidationField.Asset, "The port must have an asset.");
//			}

//			return result.IsValid;
//		}

//		/// <summary>
//		/// Internal helper for port type validation
//		/// </summary>
//		public static bool ValidatePortTypeFields(Guid? portTypeId, string portTypeName, out ValidationResult result)
//		{
//			result = new ValidationResult();

//			if (string.IsNullOrWhiteSpace(portTypeName) && (portTypeId == null || portTypeId == Guid.Empty))
//			{
//				result.AddFailReason(PortValidationField.PortType, "Port Type cannot be empty.");
//			}

//			return result.IsValid;
//		}
//	}
//}