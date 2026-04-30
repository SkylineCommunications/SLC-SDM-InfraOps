namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.AssetManagement.Common.Validation;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.AssetManagement.Repositories;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Public validator service for DeviceType validation including data access for uniqueness checks.
    /// Can be used by both internal CRUD operations and external applications.
    /// </summary>
    public class DeviceTypeValidator
    {
        private readonly IDeviceTypeQueryRepository _deviceTypeRepository;
        private readonly IAssetQueryRepository _assetRepository;
        private readonly IAssetClassQueryRepository _assetClassRepository;

        public DeviceTypeValidator(
            IDeviceTypeQueryRepository deviceTypeRepository,
            IAssetQueryRepository assetRepository)
        {
            _deviceTypeRepository = deviceTypeRepository ?? throw new ArgumentNullException(nameof(deviceTypeRepository));
            _assetRepository = assetRepository ?? throw new ArgumentNullException(nameof(assetRepository));
        }

        /// <summary>
        /// Validates a DeviceType (works for both Create and Update).
        /// Only validates fields that have Changed = true.
        /// </summary>
        /// <param name="deviceType">The device type to validate.</param>
        /// <param name="exceptIdentifiers">Identifiers to exclude from uniqueness checks (e.g., the object being updated).</param>
        public ValidationResult Validate(DeviceType deviceType, List<string> exceptIdentifiers = null)
        {
            if (deviceType == null)
            {
                throw new ArgumentNullException(nameof(deviceType));
            }

            // Modular validation - each concern separated
            List<Func<ValidationResult>> validations = new List<Func<ValidationResult>>()
            {
                () => ValidateInfo(deviceType, exceptIdentifiers),
            };

            ValidationResult result = new ValidationResult();
            foreach (var validation in validations)
            {
                result.CombineResults(validation());
            }

            return result;
        }

        /// <summary>
        /// Validates name uniqueness. Useful for real-time UI validation.
        /// </summary>
        public ValidationResult ValidateNameUniqueness(string name, List<string> exceptIdentifiers = null)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(name))
            {
                result.AddFailReason(DeviceTypeValidationHandler.DeviceTypeValidationField.Name,
                    "Device Type Name cannot be empty or whitespace.");
                return result;
            }

            if (IsNameInUse(name, exceptIdentifiers))
            {
                result.AddFailReason(DeviceTypeValidationHandler.DeviceTypeValidationField.Name,
                    $"Device Type Name '{name}' is already in use.");
            }

            return result;
        }

        /// <summary>
        /// Validates if the DeviceType is in use by existing assets.
        /// </summary>
        public ValidationResult ValidateIsInUse(DeviceType deviceType)
        {
            var result = new ValidationResult();

            if (deviceType == null)
            {
                result.AddFailReason(DeviceTypeValidationHandler.DeviceTypeValidationField.DeviceType,
                    "Device Type must be provided.");
                return result;
            }

            // Check if any assets are using this device type
            var assetClassFilter = AssetClassExposers.DeviceTypeId.Equal(deviceType);
            var assetClasses = _assetClassRepository.Read(assetClassFilter);
            var clauses = assetClasses.Select(ac => AssetExposers.AssetClass.Equal(ac));
            var assetOrFilter = new ORFilterElement<Asset>(clauses.Cast<FilterElement<Asset>>().ToArray());

            var assetsFilter = _assetRepository.Read(assetOrFilter);

            var activeAssets = assetsFilter.Where(asset =>
                asset.Status != SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable &&
                asset.Status != SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Disposed);

            if (activeAssets.Any())
            {
                result.AddFailReason(DeviceTypeValidationHandler.DeviceTypeValidationField.DeviceType,
                    "There are already assets assigned to this device type not in the 'Not Available' or 'Disposed' State.");
            }

            return result;
        }

        #region Private Validation Methods

        private ValidationResult ValidateInfo(DeviceType deviceType, List<string> exceptIdentifiers)
        {
            var result = new ValidationResult();

            // Name validation
            if (deviceType.NameField.Changed)
            {
                result.CombineResults(ValidateNameUniqueness(deviceType.Name, exceptIdentifiers));
            }

            return result;
        }

        private bool IsNameInUse(string name, List<string> exceptIdentifiers = null)
        {
            FilterElement<DeviceType> filter = DeviceTypeExposers.Name.Equal(name);

            if (exceptIdentifiers != null && exceptIdentifiers.Any())
            {
                var clauses = exceptIdentifiers
                    .Select(id => DeviceTypeExposers.Identifier.NotEqual(id))
                    .Cast<FilterElement<DeviceType>>()
                    .ToArray();
                filter = filter.AND(new ANDFilterElement<DeviceType>(clauses));
            }

            return _deviceTypeRepository.Count(filter) > 0;
        }

        #endregion
    }
}