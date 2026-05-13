//namespace Skyline.DataMiner.SDM.AssetManagement.Common.Validation
//{
//    using System;
//    using System.Collections.Generic;
//    using System.Linq;

//    using Skyline.DataMiner.Utils.InfraOps.Common.DOM_Classes.DOM.Applications.All.Validations;
//    using Skyline.DataMiner.Utils.InfraOps.Common.DOM_Classes.DOM.Applications.Asset_Manager.Wrappers;

//    using static Skyline.DataMiner.Utils.InfraOps.Common.DOM_Classes.DOM.Applications.Asset_Manager.Validations.PortWrapperValidationHandler;

//    public static class DataPortValidationHandler2
//    {
//        public enum DataPortValidationField
//        {
//            DataPort,
//        }

//        public static ValidationResult ValidateDataPort(DataPortWrapper dataPort, ValidatorContext<DataPortWrapper> context)
//        {
//            List<Func<ValidationResult>> validations = new List<Func<ValidationResult>>()
//            {
//                () => ValidateDataPortInfo(dataPort, context),
//                () => ValidateDataPortAddressInfo(dataPort, context),
//                () => ValidateDataPortAsset(dataPort, context),
//                () => ValidateDataPortPrimaryPortRelation(dataPort, context),

//            };

//            ValidationResult result = new ValidationResult();
//            foreach (var validation in validations)
//            {
//                result.CombineResults(validation());

//                if (context.ReturnWhenInvalid && !result.IsValid)
//                {
//                    return result;
//                }
//            }

//            return result;
//        }

//        #region Data Port Info

//        private static ValidationResult ValidateDataPortInfo(DataPortWrapper dataPort, ValidatorContext<DataPortWrapper> context)
//        {
//            ValidationResult result = new ValidationResult();
//            if ((dataPort.NameField.Changed || dataPort.PortNumberField.Changed) && !PortWrapperValidationHandler.ValidateMandatoryPortFields(dataPort.Name, dataPort.PortNumber, out var mandatoryPortResults))
//            {
//                result.CombineResults(mandatoryPortResults);
//            }

//            if (context.ReturnWhenInvalid && !result.IsValid)
//            {
//                return result;
//            }

//            if (dataPort.PortTypeIdField.Changed && !isPortTypeValid(dataPort, out var portTypeResult))
//            {
//                result.CombineResults(portTypeResult);
//            }

//            if (context.ReturnWhenInvalid && !result.IsValid)
//            {
//                return result;
//            }

//            return result;
//        }

//        public static bool IsPortNumberValid(AssetWrapper asset, long portNumber, ValidatorContext<DataPortWrapper> context, out ValidationResult result)
//        {
//            result = new ValidationResult();
//            if (asset.DataPorts.Any(p => p.InstanceId != context.BaseEntry?.InstanceId && p.PortNumber == portNumber))
//            {
//                result.AddFailReason(DataPortValidationField.DataPort, "Port number is already in use on the asset.");
//                return result.IsValid;
//            }

//            return result.IsValid;
//        }

//        public static bool IsPortNumberValid(AssetClassWrapper assetClass, long portNumber, ValidatorContext<DataPortWrapper> context, out ValidationResult result)
//        {
//            result = new ValidationResult();
//            if (context.BaseEntry?.PortNumber != portNumber && assetClass.DataPortInfos.Count(p => p.PortNumber == portNumber) > 1)
//            {
//                result.AddFailReason(DataPortValidationField.DataPort, "Port number is already in use on the asset Class.");
//                return result.IsValid;
//            }

//            return result.IsValid;
//        }

//        public static bool isPortTypeValid(DataPortWrapper dataPort, out ValidationResult result)
//        {
//            result = new ValidationResult();

//            if (!dataPort.HasPortType)
//            {
//                result.AddFailReason(PortValidationField.PortType, "Port Type cannot be empty.");
//                return result.IsValid;
//            }

//            var dataPortType = dataPort.PortType;

//            if (!dataPortType.IsDataPortType())
//            {
//                result.AddFailReason(PortValidationField.PortType, "Port Type must be a Data Port Type.");
//                return result.IsValid;
//            }

//            return result.IsValid;
//        }

//        #endregion

//        #region Address Info

//        private static ValidationResult ValidateDataPortAddressInfo(DataPortWrapper dataPort, ValidatorContext<DataPortWrapper> context)
//        {
//            ValidationResult result = new ValidationResult();

//            return result;
//        }

//        #endregion

//        #region Asset

//        private static ValidationResult ValidateDataPortAsset(DataPortWrapper dataPort, ValidatorContext<DataPortWrapper> context)
//        {
//            ValidationResult result = new ValidationResult();
//            if (dataPort.AssetIdField.Changed && !IsDataPortAssetValid(dataPort, context, out var nameResult))
//            {
//                result.CombineResults(nameResult);
//            }

//            if (context.ReturnWhenInvalid && !result.IsValid)
//            {
//                return result;
//            }

//            return result;
//        }

//        private static bool IsDataPortAssetValid(DataPortWrapper dataPort, ValidatorContext<DataPortWrapper> context, out ValidationResult result)
//        {
//            result = new ValidationResult();

//            if (!dataPort.HasAsset)
//            {
//                result.AddFailReason(DataPortValidationField.DataPort, "Data port must be linked to an asset.");
//                return result.IsValid;
//            }

//            if (context.ReturnWhenInvalid && !result.IsValid)
//            {
//                return result.IsValid;
//            }

//            if (AssetValidationHandler.IsValidDataPortAssetRelation(dataPort.Asset.DataPorts, out ValidationResult relationResult))
//            {
//                result.CombineResults(relationResult);
//                return result.IsValid;
//            }

//            return result.IsValid;
//        }

//        #endregion

//        #region Primary Port Relation

//        private static ValidationResult ValidateDataPortPrimaryPortRelation(DataPortWrapper dataPort, ValidatorContext<DataPortWrapper> context)
//        {
//            ValidationResult result = new ValidationResult();

//            return result;
//        }

//        #endregion
//    }
//}