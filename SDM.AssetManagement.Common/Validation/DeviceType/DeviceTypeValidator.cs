namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Common.Services;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.SDM.InfraOps.Common.Validation;
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
            var result = ValidateInfo(entity);
            result.AddFailuresFrom(ValidateNameUniqueness(entity));

            if (result.IsValid)
            {
                result.AddFailuresFrom(ValidateNoActiveAssetsForUpdate(entity));
            }

            return result;
        }

        protected override List<ValidationResult> ValidateBulk(List<DeviceType> entities)
        {
            if (entities == null || !entities.Any())
            {
                return new List<ValidationResult>();
            }

            var results = entities.Select(ValidateInfo).ToList();

            if (results.AnyInvalid())
            {
                return results;
            }

            var batchConflicts = ValidateNameDuplicatesInBatch(entities);
            results.MergeFrom(batchConflicts);

            if (results.AnyInvalid())
            {
                return results;
            }

            var nameDbConflicts = ValidateBulkNamesAgainstDatabase(entities);
            results.MergeFrom(nameDbConflicts);

            if (results.AnyInvalid())
            {
                return results;
            }

            results.MergeFrom(ValidateBulkNoActiveAssetsForUpdate(entities));

            return results;
        }

        /// <summary>
        /// Blocks updating an existing DeviceType while assets not in the 'Disposed' state are assigned to it.
        /// <para><b>Not suitable for bulk scenarios</b>: issues DB queries per call. Use <see cref="ValidateBulk"/> instead.</para>
        /// </summary>
        private ValidationResult ValidateNoActiveAssetsForUpdate(DeviceType deviceType)
        {
            var result = new ValidationResult();

            if (deviceType == null || string.IsNullOrWhiteSpace(deviceType.Identifier))
            {
                return result;
            }

            var assetClassIds = _entityLoader.GetAssetClassesByDeviceTypeIds(new List<string> { deviceType.Identifier })
                .Select(assetClass => assetClass.Identifier)
                .ToList();

            if (_entityLoader.HasNonDisposedAssetsForAssetClasses(assetClassIds))
            {
                result.AddFailReason(
                    DeviceTypeValidationHandler.DeviceTypeValidationField.Asset,
                    "There are already assets assigned to this device type not in the 'Disposed' State");
            }

            return result;
        }

        private List<ValidationResult> ValidateBulkNoActiveAssetsForUpdate(List<DeviceType> deviceTypes)
        {
            var results = deviceTypes.Select(_ => new ValidationResult()).ToList();

            var identifiers = deviceTypes
                .Select(dt => dt.Identifier)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            if (!identifiers.Any())
            {
                return results;
            }

            var assetClassIdsByDeviceType = _entityLoader.GetAssetClassesByDeviceTypeIds(identifiers)
                .Where(assetClass => assetClass.DeviceTypeId.HasValue())
                .GroupBy(assetClass => assetClass.DeviceTypeId.Identifier)
                .ToDictionary(group => group.Key, group => group.Select(assetClass => assetClass.Identifier).ToList());

            for (int i = 0; i < deviceTypes.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(deviceTypes[i].Identifier)
                    && assetClassIdsByDeviceType.TryGetValue(deviceTypes[i].Identifier, out var assetClassIds)
                    && _entityLoader.HasNonDisposedAssetsForAssetClasses(assetClassIds))
                {
                    results[i].AddFailReason(
                        DeviceTypeValidationHandler.DeviceTypeValidationField.Asset,
                        "There are already assets assigned to this device type not in the 'Disposed' State");
                }
            }

            return results;
        }

        /// <summary>
        /// Validates name uniqueness for a single DeviceType against the database.
        /// <para><b>Not suitable for bulk scenarios</b>: issues one DB query per call. Use <see cref="ValidateBulk"/> instead.</para>
        /// </summary>
        private ValidationResult ValidateNameUniqueness(DeviceType deviceType)
        {
            var result = new ValidationResult();

            if (deviceType == null || string.IsNullOrWhiteSpace(deviceType.Name))
            {
                return result;
            }

            if (_entityLoader.CountDeviceTypesByName(deviceType.Name, deviceType.Identifier) > 0)
            {
                result.AddFailReason(
                    DeviceTypeValidationHandler.DeviceTypeValidationField.Name,
                    $"Device Type Name '{deviceType.Name}' is already in use.");
            }

            return result;
        }

        private static List<ValidationResult> ValidateNameDuplicatesInBatch(List<DeviceType> deviceTypes)
        {
            var results = deviceTypes.Select(_ => new ValidationResult()).ToList();

            var duplicateNames = deviceTypes
                .Select((dt, idx) => new { dt.Name, Index = idx })
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1);

            foreach (var group in duplicateNames)
            {
                foreach (var item in group)
                {
                    results[item.Index].AddFailReason(
                        DeviceTypeValidationHandler.DeviceTypeValidationField.Name,
                        $"Device Type Name '{item.Name}' is duplicated within the batch.");
                }
            }

            return results;
        }

        private List<ValidationResult> ValidateBulkNamesAgainstDatabase(List<DeviceType> deviceTypes)
        {
            var results = deviceTypes.Select(_ => new ValidationResult()).ToList();

            var uniqueNames = deviceTypes
                .Select(dt => dt.Name)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!uniqueNames.Any())
            {
                return results;
            }

            var batchIds = new HashSet<string>(
                deviceTypes.Select(dt => dt.Identifier).Where(id => !string.IsNullOrWhiteSpace(id)));

            var dbMatches = _entityLoader.GetDeviceTypesByNames(uniqueNames);

            var externalConflictNames = dbMatches
                .Where(r => !batchIds.Contains(r.Identifier))
                .Select(r => r.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < deviceTypes.Count; i++)
            {
                var name = deviceTypes[i].Name;
                if (!string.IsNullOrWhiteSpace(name) && externalConflictNames.Contains(name))
                {
                    results[i].AddFailReason(
                        DeviceTypeValidationHandler.DeviceTypeValidationField.Name,
                        $"Device Type Name '{name}' is already in use.");
                }
            }

            return results;
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
                .Where(assetClass => assetClass.DeviceTypeId.HasValue())
                .Select(assetClass => assetClass.DeviceTypeId.Identifier)
                .ToHashSet();

            var assetClassesByDeviceType = assetClasses
                .Where(assetClass => assetClass.DeviceTypeId.HasValue())
                .GroupBy(assetClass => assetClass.DeviceTypeId.Identifier)
                .ToDictionary(group => group.Key, group => group.ToList());

            for (int i = 0; i < deviceTypes.Count; i++)
            {
                if (deviceTypeIdsUsedByAssetClasses.Contains(deviceTypes[i].Identifier))
                {
                    results[i].AddFailReason(
                        DeviceTypeValidationHandler.DeviceTypeValidationField.AssetClass,
                        "There are still asset classes associated with this device type. Please remove them first.");
                }
            }

            var remainingAssetClassIds = deviceTypes
                .SelectMany(deviceType => assetClassesByDeviceType.TryGetValue(deviceType.Identifier, out var referencingAssetClasses)
                    ? referencingAssetClasses
                    : new List<AssetClass>())
                .Select(assetClass => assetClass.Identifier)
                .Distinct()
                .ToList();

            if (!remainingAssetClassIds.Any())
            {
                return results;
            }

            var assetsByAssetClassId = _entityLoader.GetAssetsByAssetClassIds(remainingAssetClassIds)
                .Where(asset => asset.AssetClassId.HasValue())
                .GroupBy(asset => asset.AssetClassId.Identifier)
                .ToDictionary(group => group.Key, group => group.ToList());

            for (int i = 0; i < deviceTypes.Count; i++)
            {
                ValidateDeviceTypeNotInUse(deviceTypes[i], results[i], assetClassesByDeviceType, assetsByAssetClassId);
            }

            return results;
        }

        private static ValidationResult ValidateInfo(DeviceType deviceType)
        {
            var result = new ValidationResult();

            if (deviceType == null)
            {
                return result;
            }

            if (deviceType.ShouldValidate(deviceType.NameField) && string.IsNullOrWhiteSpace(deviceType.Name))
            {
                result.AddFailReason(
                    DeviceTypeValidationHandler.DeviceTypeValidationField.Name,
                    "Device Type Name cannot be empty or whitespace.");
            }

            return result;
        }

        private static void ValidateDeviceTypeNotInUse(
            DeviceType deviceType,
            ValidationResult result,
            Dictionary<string, List<AssetClass>> assetClassesByDeviceType,
            Dictionary<string, List<Asset>> assetsByAssetClassId)
        {
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
