//namespace Skyline.DataMiner.Utils.InfraOps.Common.DOM_Classes.DOM.Applications.Facility_Manager.Validations
//{
//    using System;
//    using System.Collections.Generic;

//    using SharedMappers.DomIds;

//    using Skyline.DataMiner.Utils.InfraOps.Common.DOM_Classes.DOM.Applications.All.Validations;
//    using Skyline.DataMiner.Utils.InfraOps.Common.DOM_Classes.DOM.Applications.Asset_Manager.Wrappers;
//    using Skyline.DataMiner.Utils.InfraOps.Common.DOM_Classes.DOM.Applications.DomIds;
//    using Skyline.DataMiner.Utils.InfraOps.Common.DOM_Classes.DOM.Applications.Facility_Manager.Wrappers;
//    using Skyline.DataMiner.Utils.InfraOps.Common.Validation;
//    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

//    using static Skyline.DataMiner.Utils.InfraOps.Common.DOM_Classes.DOM.Applications.Asset_Manager.Validations.AssetValidationHandler;

//    public static class RackValidationHandler2
//    {
//        private const double MAX_RACK_CAPACITIY_U = 70;
//        private const double MAX_RACK_SIZE_CM = 120;
//        private const double MAX_RACK_HEIGHT_CM = 320;

//        public enum RackValidationField
//        {
//            RackId,

//            RackSpacePosition,
//        }

//        public static ValidationResult ValidateRack(RackWrapper rack, ValidatorContext<RackWrapper> context)
//        {
//            List<Func<ValidationResult>> validations = new List<Func<ValidationResult>>()
//            {
//                () => ValidateRackInfo(rack, context),
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

//        #region Info

//        private static ValidationResult ValidateRackInfo(RackWrapper rack, ValidatorContext<RackWrapper> context)
//        {
//            var validationFactory = ValidationFactory<RackWrapper>
//                .PrepareValidation(
//                (dat) => dat.Object.RackIdField.Changed,
//                (dat) =>
//                {
//                    IsRackIdValid(dat.Object.ModuleHandlers, dat.Object.RackId, dat.Context, out var result);
//                    return result;
//                });

//            validationFactory.Validate(rack, context, out var rackValidationResult);
//            return rackValidationResult;
//        }

//        public static bool IsRackIdValid(GlobalInfraOpsModuleHandler moduleHandlers, string rackId, ValidatorContext<RackWrapper> context, out ValidationResult result)
//        {
//            result = new ValidationResult();

//            if (string.IsNullOrWhiteSpace(rackId))
//            {
//                result.AddFailReason(RackValidationField.RackId, "Rack Id cannot be empty or whiteSpace.");
//                return result.IsValid;
//            }

//            foreach (var otherAsset in context.OtherChangedEntries)
//            {
//                if (string.Equals(rackId, otherAsset.RackId))
//                {
//                    result.AddFailReason(RackValidationField.RackId, "Rack Id already in use.");
//                    return result.IsValid;
//                }
//            }

//            if (moduleHandlers.RackHandler.IsRackIdInUse(rackId, context.ChangedEntries))
//            {
//                result.AddFailReason(RackValidationField.RackId, "Rack Id Already in Use.");
//                return result.IsValid;
//            }

//            return result.IsValid;
//        }

//        #endregion

//        #region Assets

//        public static bool IsValidRackAssetRelation(RackWrapper rack, out ValidationResult result)
//        {
//            result = new ValidationResult();

//            var assets = rack.GetRackPlaceableAssets();

//            foreach (var asset in assets)
//            {
//                if (asset.RackPosition < 0)
//                {
//                    result.AddFailReason(AssetValidationField.DataPort, "Asset Rack Position cannot be negative.");
//                    return result.IsValid;
//                }

//                foreach (var otherAsset in assets)
//                {
//                    if (otherAsset == asset)
//                    {
//                        continue;
//                    }

//                    if (otherAsset.RackPosition == asset.RackPosition && otherAsset.RackSide == asset.RackSide)
//                    {
//                        result.AddFailReason(AssetValidationField.DataPort, $"Multiple Assets have the same Rack Position '{asset.RackPosition}' and Rack Side '{asset.RackSide}'.");
//                        return result.IsValid;
//                    }
//                }
//            }

//            return result.IsValid;
//        }

//        #endregion

//        public static bool ValidateRackSpace(RackWrapper rack, AssetWrapper asset, int position, out ValidationResult result)
//        {
//            if (rack == null)
//            {
//                throw new ArgumentNullException(nameof(rack));
//            }

//            if (asset == null)
//            {
//                throw new ArgumentNullException(nameof(asset));
//            }

//            result = new ValidationResult();
//            var assetClass = asset.AssetClass;
//            if (!assetClass.HasTag(SlcAsset_Management.Enums.TagOption.RackUnitConsumer))
//            {
//                return result.IsValid;
//            }

//            if (assetClass.HeightUAsInt == null || assetClass.HeightUAsInt <= 0)
//            {
//                // If it is rack Consumer but has no height or less than 1, we consider it valid.
//                return result.IsValid;
//            }

//            return ValidateRackSpace(rack, position, assetClass.HeightUAsInt.Value, asset, null, out result);
//        }

//        public static bool ValidateRackSpace(RackWrapper rack, int position, int heightU, out ValidationResult result)
//        {
//            return ValidateRackSpace(rack, position, heightU, null, null, out result);
//        }

//        private static bool ValidateRackSpace(RackWrapper rack, int position, int heightU, AssetWrapper requestedAsset, ReservationWrapper requestedReservation, out ValidationResult result)
//        {
//            result = new ValidationResult();

//            if (position <= 0)
//            {
//                result.AddFailReason(RackValidationField.RackSpacePosition, "Invalid Position. Position should higher than 0.");
//                return result.IsValid;
//            }

//            var rackAssetOccupation = rack.GetRackAssetOccupationArray();
//            var rackReservationOccupation = rack.GetRackReservationOccupationArray();

//            int zeroBasedAssetRackPosition = position - 1;

//            int assetRackHeight = heightU;

//            long assetStartPosition;
//            long assetEndPosition;

//            if (rack.Position == SlcFacility_Management.Enums.RackpositionenumEnum.Top)
//            {
//                assetStartPosition = zeroBasedAssetRackPosition - (assetRackHeight - 1);
//            }
//            else
//            {
//                assetStartPosition = zeroBasedAssetRackPosition;
//            }

//            assetEndPosition = assetStartPosition + assetRackHeight;

//            if (assetStartPosition < 0 || assetEndPosition >= rackAssetOccupation.Length)
//            {
//                result.AddFailReason(RackValidationField.RackSpacePosition, "Invalid Position. Rack height makes it go out of the Rack");
//                return result.IsValid;
//            }

//            for (long idx = assetStartPosition; idx < assetEndPosition; idx++)
//            {
//                if (rackReservationOccupation[idx] != null && rackReservationOccupation[idx] != requestedReservation)
//                {
//                    result.AddFailReason(RackValidationField.RackSpacePosition, "Invalid Position. Rack position already reserved.");
//                    return result.IsValid;
//                }

//                if (rackAssetOccupation[idx] != null && rackAssetOccupation[idx] != requestedAsset)
//                {
//                    result.AddFailReason(RackValidationField.RackSpacePosition, "Invalid Position. Rack position already in use.");
//                    return result.IsValid;
//                }
//            }

//            return result.IsValid;
//        }

//        public static bool IsRackHeightValid(double? height)
//        {
//            if (height == null)
//            {
//                return true;
//            }
//            else if (height > MAX_RACK_HEIGHT_CM || height < 0)
//            {
//                return false;
//            }

//            return true;
//        }

//        public static bool IsRackDepthValid(double? depth)
//        {
//            if (depth == null)
//            {
//                return true;
//            }
//            else if (depth > MAX_RACK_SIZE_CM || depth < 0)
//            {
//                return false;
//            }

//            return true;
//        }

//        public static bool IsRackWidthValid(double? width)
//        {
//            if (width == null)
//            {
//                return true;
//            }
//            else if (width > MAX_RACK_SIZE_CM || width < 0)
//            {
//                return false;
//            }

//            return true;
//        }

//        public static bool IsRackUnitCapacityValid(double rackUnitCapacity)
//        {
//            if (rackUnitCapacity > MAX_RACK_CAPACITIY_U || rackUnitCapacity < 1)
//            {
//                return false;
//            }

//            return true;
//        }

//        public static bool IsRackPowerCapacityValid(double? powerCapacity)
//        {
//            if (powerCapacity == null)
//            {
//                return true;
//            }
//            else if (powerCapacity < 0)
//            {
//                return false;
//            }

//            return true;
//        }

//        public static bool IsRackSizeValid(double rackUnitCapacity, double? rackWidth = null, double? rackDepth = null, double? rackHeight = null)
//        {
//            if (rackUnitCapacity > MAX_RACK_CAPACITIY_U || rackUnitCapacity < 0)
//            {
//                return false;
//            }

//            if ((rackWidth != null && rackWidth > MAX_RACK_SIZE_CM) || (rackWidth != null && rackWidth < 0))
//            {
//                return false;
//            }

//            if ((rackDepth != null && rackDepth > MAX_RACK_SIZE_CM) || (rackDepth != null && rackDepth < 0))
//            {
//                return false;
//            }

//            if ((rackHeight != null && rackHeight > MAX_RACK_HEIGHT_CM) || (rackHeight != null && rackHeight < 0))
//            {
//                return false;
//            }

//            return true;
//        }
//    }
//}