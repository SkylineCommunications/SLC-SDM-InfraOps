namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Common.Services;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    public class DeviceTypeValidator : ValidatorBase<DeviceType>
    {
        private readonly SdmEntityLoader _entityLoader;

        public DeviceTypeValidator(SdmEntityLoader entityLoader)
        {
            _entityLoader = entityLoader ?? throw new ArgumentNullException(nameof(entityLoader));
        }

        protected override ValidationResult Validate(DeviceType entity)
        {
            return new ValidationResult();
        }

        protected override List<ValidationResult> ValidateBulk(List<DeviceType> entities)
        {
            return entities == null
                ? new List<ValidationResult>()
                : entities.Select(_ => new ValidationResult()).ToList();
        }

        protected override ValidationResult ValidateForDelete(DeviceType deviceType)
        {
            if (deviceType == null)
            {
                throw new ArgumentNullException(nameof(deviceType));
            }

            return ValidateNotInUseWhenDeleted(new List<DeviceType> { deviceType })[0];
        }

        protected override List<ValidationResult> ValidateBulkForDelete(List<DeviceType> deviceTypes)
        {
            if (deviceTypes == null || !deviceTypes.Any())
            {
                return new List<ValidationResult>();
            }

            return ValidateNotInUseWhenDeleted(deviceTypes);
        }

        private List<ValidationResult> ValidateNotInUseWhenDeleted(List<DeviceType> deviceTypes)
        {
            var results = deviceTypes.Select(_ => new ValidationResult()).ToList();

            var deviceTypeIds = deviceTypes
                .Select(dt => dt.Identifier)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            var assetClasses = _entityLoader.GetAssetClassesByDeviceTypeIds(deviceTypeIds);
            var deviceTypeIdsUsedByAssetClasses = assetClasses
                .Where(assetClass => assetClass.DeviceTypeId != null && assetClass.DeviceTypeId.HasValue())
                .Select(assetClass => assetClass.DeviceTypeId.Identifier)
                .ToHashSet();

            var assetClassesByDeviceType = assetClasses
                .Where(assetClass => assetClass.DeviceTypeId != null && assetClass.DeviceTypeId.HasValue())
                .GroupBy(assetClass => assetClass.DeviceTypeId.Identifier)
                .ToDictionary(group => group.Key, group => group.ToList());

            var assetsByAssetClassId = _entityLoader.GetAssetsByAssetClassIds(assetClasses.Select(ac => ac.Identifier).ToList())
                .Where(asset => asset.AssetClassId != null && asset.AssetClassId.HasValue())
                .GroupBy(asset => asset.AssetClassId.Identifier)
                .ToDictionary(group => group.Key, group => group.ToList());

            for (int i = 0; i < deviceTypes.Count; i++)
            {
                ValidateDeviceTypeNotInUse(deviceTypes[i], results[i], deviceTypeIdsUsedByAssetClasses, assetClassesByDeviceType, assetsByAssetClassId);
            }

            return results;
        }

        private static void ValidateDeviceTypeNotInUse(
            DeviceType deviceType,
            ValidationResult result,
            HashSet<string> deviceTypeIdsUsedByAssetClasses,
            Dictionary<string, List<AssetClass>> assetClassesByDeviceType,
            Dictionary<string, List<Asset>> assetsByAssetClassId)
        {
            if (deviceTypeIdsUsedByAssetClasses.Contains(deviceType.Identifier))
            {
                result.AddFailReason(
                    DeviceTypeValidationHandler.DeviceTypeValidationField.AssetClass,
                    "There are still asset classes associated with this device type. Please remove them first.");
            }

            var referencingAssets = CollectReferencingAssets(deviceType.Identifier, assetClassesByDeviceType, assetsByAssetClassId);
            if (!DeviceTypeValidationHandler.CanDelete(referencingAssets, out var deviceTypeResult))
            {
                result.AddFailuresFrom(deviceTypeResult);
            }
        }

        private static List<Asset> CollectReferencingAssets(
            string deviceTypeId,
            Dictionary<string, List<AssetClass>> assetClassesByDeviceType,
            Dictionary<string, List<Asset>> assetsByAssetClassId)
        {
            var referencingAssets = new List<Asset>();
            if (assetClassesByDeviceType.TryGetValue(deviceTypeId, out var referencingAssetClasses))
            {
                foreach (var assetClass in referencingAssetClasses)
                {
                    if (assetsByAssetClassId.TryGetValue(assetClass.Identifier, out var assets))
                    {
                        referencingAssets.AddRange(assets);
                    }
                }
            }

            return referencingAssets;
        }
    }
}
