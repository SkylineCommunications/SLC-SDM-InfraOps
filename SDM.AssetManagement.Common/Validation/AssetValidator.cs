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
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.Common.DOM_Classes.DOM.Applications.Asset_Manager.Validations;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Public validator service for Asset validation with comprehensive error handling.
    /// Can be used by both internal CRUD operations and external applications.
    /// </summary>
    public class AssetValidator
    {
        private readonly IAssetQueryRepository _assetRepository;
        private readonly IAssetClassQueryRepository _assetClassRepository;
        private readonly Validator<Asset> _validationPipeline;

        public AssetValidator(
            IAssetQueryRepository assetRepository,
            IAssetClassQueryRepository assetClassRepository)
        {
            _assetRepository = assetRepository ?? throw new ArgumentNullException(nameof(assetRepository));
            _assetClassRepository = assetClassRepository ?? throw new ArgumentNullException(nameof(assetClassRepository));

            _validationPipeline = BuildValidationPipeline();
        }

        /// <summary>
        /// Validates an Asset (works for both Create and Update).
        /// Only validates fields that have Changed = true.
        /// </summary>
        public ValidationResult Validate(Asset asset)
        {
            if (asset == null)
            {
                throw new ArgumentNullException(nameof(asset));
            }

            return _validationPipeline.Validate(asset);
        }

        /// <summary>
        /// Validates an Asset and throws ValidationException if invalid.
        /// </summary>
        public void ValidateAndThrow(Asset asset)
        {
            _validationPipeline.ValidateAndThrow(asset);
        }

        /// <summary>
        /// Validates with custom error handling callback.
        /// </summary>
        public ValidationResult ValidateWithHandler(Asset asset, Action<ValidationResult> onError)
        {
            return _validationPipeline.ValidateWithHandler(asset, onError);
        }

        /// <summary>
        /// Validates asset name uniqueness. Useful for real-time UI validation.
        /// </summary>
        public ValidationResult IsAssetNameValid(string name, List<string> exceptIdentifiers = null)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(name))
            {
                result.AddFailReason(AssetValidationHandler.AssetValidationField.Name,
                    "Asset Name cannot be empty or whitespace.");
                return result;
            }

            if (IsNameInUse(name, exceptIdentifiers))
            {
                result.AddFailReason(AssetValidationHandler.AssetValidationField.Name,
                    $"Asset Name '{name}' is already in use.");
            }

            return result;
        }

        /// <summary>
        /// Validates asset name uniqueness for the specified Asset instance.
        /// </summary>
        public ValidationResult IsAssetNameValid(Asset asset)
        {
            return IsAssetNameValid(asset.Name, new List<string> { asset.Identifier });
        }

        /// <summary>
        /// Validates asset ID uniqueness. Useful for real-time UI validation.
        /// </summary>
        public ValidationResult IsAssetIdValid(string assetId, List<string> exceptIdentifiers = null)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(assetId))
            {
                result.AddFailReason(AssetValidationHandler.AssetValidationField.AssetId,
                    "Asset ID cannot be empty or whitespace.");
                return result;
            }

            if (IsAssetIdInUse(assetId, exceptIdentifiers))
            {
                result.AddFailReason(AssetValidationHandler.AssetValidationField.AssetId,
                    $"Asset ID '{assetId}' is already in use.");
            }

            return result;
        }

        #region Pipeline Construction

        private Validator<Asset> BuildValidationPipeline()
        {
            // Critical validations - stop on failure
            var criticalValidations = Validator<Asset>
                .Create(ValidateCriticalFields)
                .StopOnFailure();

            // Standard validations - collect all errors
            var standardValidations = Validator<Asset>
                .Create(ValidateInfo)
                .AndThen(ValidateLocation)
                .AndThen(ValidateLifecycle)
                .AndThen(ValidateOwnership)
                .AndThen(ValidateCollections);

            return criticalValidations.AndThen(standardValidations);
        }

        #endregion

        #region Validation Methods

        private ValidationResult ValidateCriticalFields(Asset asset)
        {
            var result = new ValidationResult();

            // Name is critical
            if (asset.NameField.Changed)
            {
                result.AddFailuresFrom(IsAssetNameValid(asset));
            }

            // Asset ID is critical
            if (asset.AssetIDField.Changed)
            {
                result.AddFailuresFrom(IsAssetIdValid(asset.AssetID, new List<string> { asset.Identifier }));
            }

            // Asset Class is critical
            if (asset.AssetClassIdField.Changed)
            {
                if (!AssetValidationHandler.IsAssetClassValid(asset, out var assetClassResult))
                {
                    result.AddFailuresFrom(assetClassResult);
                }
            }

            return result;
        }

        private ValidationResult ValidateInfo(Asset asset)
        {
            var result = new ValidationResult();

            // Serial number validation
            if (asset.SerialNumberField.Changed)
            {
                result.AddFailuresFrom(ValidateSerialNumber(asset));
            }

            return result;
        }

        private ValidationResult ValidateSerialNumber(Asset asset)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(asset.SerialNumber))
            {
                return result; // Empty serial number is valid
            }

            // Check uniqueness within same AssetClass
            if (IsSerialNumberInUse(asset.SerialNumber, asset.AssetClassId, new List<string> { asset.Identifier }))
            {
                result.AddFailReason(AssetValidationHandler.AssetValidationField.SerialNumber,
                    "Serial Number is already in use for this Asset Class.");
            }

            return result;
        }

        private ValidationResult ValidateLocation(Asset asset)
        {
            var result = new ValidationResult();

            // Parent Asset validation
            if (asset.ParentAssetIdField.Changed || asset.HolderNumberField.Changed)
            {
                var assetClass = LoadAssetClass(asset.AssetClassId);
                if (assetClass != null)
                {
                    if (!AssetValidationHandler.IsParentAssetHolderValid(asset, assetClass, out var parentResult))
                    {
                        result.AddFailuresFrom(parentResult);
                    }

                    // Additional parent asset holder validation (requires parent asset data)
                    if (asset.ParentAssetId != null && asset.ParentAssetId.HasValue())
                    {
                        result.AddFailuresFrom(ValidateParentAssetHolderSlot(asset, assetClass));
                    }
                }
            }

            // Rack validation
            if (asset.RackIdField.Changed || asset.RackPositionField.Changed || asset.RackSideField.Changed)
            {
                var assetClass = LoadAssetClass(asset.AssetClassId);
                if (assetClass != null)
                {
                    if (!AssetValidationHandler.IsRackPositionValid(asset, assetClass, out var rackResult))
                    {
                        result.AddFailuresFrom(rackResult);
                    }
                }
            }

            return result;
        }

        private ValidationResult ValidateParentAssetHolderSlot(Asset asset, AssetClass assetClass)
        {
            var result = new ValidationResult();

            // TODO: Load parent asset from repository and validate holder availability
            // This requires parent Asset data which needs repository access

            return result;
        }

        private ValidationResult ValidateLifecycle(Asset asset)
        {
            var result = new ValidationResult();

            if (asset.InstallationUserIdField.Changed || asset.InstallationDateField.Changed)
            {
                if (!AssetValidationHandler.IsInstallationInfoValid(asset, out var installationResult))
                {
                    result.AddFailuresFrom(installationResult);
                }
            }

            if (asset.ModificationUserIdField.Changed || asset.ModificationDateField.Changed)
            {
                if (!AssetValidationHandler.IsModificationInfoValid(asset, out var modificationResult))
                {
                    result.AddFailuresFrom(modificationResult);
                }
            }

            return result;
        }

        private ValidationResult ValidateOwnership(Asset asset)
        {
            var result = new ValidationResult();

            if (asset.OwnerContactPersonIdField.Changed || asset.OwnerContactPersonRoleIdField.Changed)
            {
                if (!AssetValidationHandler.IsOwnershipValid(asset, out var ownerResult))
                {
                    result.AddFailuresFrom(ownerResult);
                }
            }

            if (asset.CustodyContactPersonIdField.Changed || asset.CustodyContactPersonRoleIdField.Changed)
            {
                if (!AssetValidationHandler.IsCustodyValid(asset, out var custodyResult))
                {
                    result.AddFailuresFrom(custodyResult);
                }
            }

            return result;
        }

        private ValidationResult ValidateCollections(Asset asset)
        {
            var result = new ValidationResult();

            if (asset.DataPortsField.Changed)
            {
                result.AddFailuresFrom(AssetValidationHandler.ValidateAssetDataPorts(asset));
            }

            if (asset.PowerPortsField.Changed)
            {
                result.AddFailuresFrom(AssetValidationHandler.ValidateAssetPowerPorts(asset));
            }

            if (asset.HoldersField.Changed)
            {
                result.AddFailuresFrom(AssetValidationHandler.ValidateAssetHolders(asset));
            }

            if (asset.ElementsField.Changed)
            {
                result.AddFailuresFrom(AssetValidationHandler.ValidateAssetElements(asset));
            }

            return result;
        }

        #endregion

        #region Helper Methods

        private AssetClass LoadAssetClass(SdmObjectReference<AssetClass> reference)
        {
            if (reference == null || !reference.HasValue())
            {
                return null;
            }

            try
            {
                var filter = AssetClassExposers.Identifier.Equal(reference.Identifier);
                return _assetClassRepository.Read(filter).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load AssetClass: {ex.Message}", ex);
            }
        }

        private bool IsNameInUse(string name, List<string> exceptIdentifiers = null)
        {
            FilterElement<Asset> filter = AssetExposers.Name.Equal(name);

            if (exceptIdentifiers != null && exceptIdentifiers.Any())
            {
                var clauses = exceptIdentifiers
                    .Select(id => AssetExposers.Identifier.NotEqual(id))
                    .Cast<FilterElement<Asset>>()
                    .ToArray();
                filter = filter.AND(new ANDFilterElement<Asset>(clauses));
            }

            return _assetRepository.Count(filter) > 0;
        }

        private bool IsAssetIdInUse(string assetId, List<string> exceptIdentifiers = null)
        {
            FilterElement<Asset> filter = AssetExposers.AssetID.Equal(assetId);

            if (exceptIdentifiers != null && exceptIdentifiers.Any())
            {
                var clauses = exceptIdentifiers
                    .Select(id => AssetExposers.Identifier.NotEqual(id))
                    .Cast<FilterElement<Asset>>()
                    .ToArray();
                filter = filter.AND(new ANDFilterElement<Asset>(clauses));
            }

            return _assetRepository.Count(filter) > 0;
        }

        private bool IsSerialNumberInUse(string serialNumber, SdmObjectReference<AssetClass> assetClassId, List<string> exceptIdentifiers = null)
        {
            if (string.IsNullOrWhiteSpace(serialNumber) || assetClassId == null || !assetClassId.HasValue())
            {
                return false;
            }

            FilterElement<Asset> filter = AssetExposers.SerialNumber.Equal(serialNumber)
                .AND(AssetExposers.AssetClassId.Equal(assetClassId.Identifier));

            if (exceptIdentifiers != null && exceptIdentifiers.Any())
            {
                var clauses = exceptIdentifiers
                    .Select(id => AssetExposers.Identifier.NotEqual(id))
                    .Cast<FilterElement<Asset>>()
                    .ToArray();
                filter = filter.AND(new ANDFilterElement<Asset>(clauses));
            }

            return _assetRepository.Count(filter) > 0;
        }

        #endregion
    }
}