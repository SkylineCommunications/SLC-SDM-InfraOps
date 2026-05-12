namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SharedCommonLibrary.AssetManagement.State_Management;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel.Status;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.AssetManagement.Repositories;
    using Skyline.DataMiner.SDM.Common.Services;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Public validator service for Asset validation with comprehensive error handling.
    /// </summary>
    public class AssetValidator
    {
        private readonly SdmEntityLoader _entityLoader;
        private readonly Validator<Asset> _validationPipeline;
        private readonly AssetValidationCore _validationCore;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetValidator"/> class.
        /// </summary>
        /// <param name="entityLoader">Shared entity loader service.</param>
        public AssetValidator(SdmEntityLoader entityLoader)
        {
            _entityLoader = entityLoader ?? throw new ArgumentNullException(nameof(entityLoader));
            _validationCore = new AssetValidationCore(_entityLoader);
            _validationPipeline = BuildValidationPipeline();
        }

        #region Public API

        /// <summary>
        /// Validates an Asset and returns ValidationResult.
        /// Collects all errors without throwing exceptions.
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
        /// Use this when you want fail-fast behavior.
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
        /// Validates name uniqueness - used for real-time UI validation.
        /// </summary>
        public ValidationResult IsAssetNameValid(string name, List<string> exceptIdentifiers = null)
        {
            return _validationCore.ValidateNameUniqueness(name, exceptIdentifiers);
        }

        /// <summary>
        /// Validates the uniqueness of the Asset name for the specified <see cref="Asset"/> instance.
        /// Excludes the current asset identifier from the uniqueness check.
        /// </summary>
        public ValidationResult IsAssetNameValid(Asset asset)
        {
            return IsAssetNameValid(asset.Name, new List<string> { asset.Identifier });
        }

        /// <summary>
        /// Validates asset ID uniqueness - used for real-time UI validation.
        /// </summary>
        public ValidationResult IsAssetIdValid(string assetId, List<string> exceptIdentifiers = null)
        {
            return _validationCore.ValidateAssetIdUniqueness(assetId, exceptIdentifiers);
        }

        /// <summary>
        /// Validates all DataPorts associated with the Asset.
        /// </summary>
        public ValidationResult ValidateAssetDataPorts(Asset asset)
        {
            return _validationCore.ValidateDataPorts(asset);
        }

        /// <summary>
        /// Validates all PowerPorts associated with the Asset.
        /// </summary>
        public ValidationResult ValidateAssetPowerPorts(Asset asset)
        {
            return _validationCore.ValidatePowerPorts(asset);
        }

        /// <summary>
        /// Validates multiple assets in bulk with optimized performance.
        /// Returns validation results in the same order as the input assets.
        /// Result at index i corresponds to asset at index i.
        /// </summary>
        public List<ValidationResult> ValidateBulk(List<Asset> assets)
        {
            if (assets == null || !assets.Any())
            {
                return new List<ValidationResult>();
            }

            var context = new AssetValidationContext
            {
                AssetsBeingValidated = assets
            };

            // Initialize results - same order as input
            var results = assets.Select(a => new ValidationResult()).ToList();

            // ============================================================
            // PHASE 1: NO DATABASE ACCESS CHECKS (BUSINESS RULES)
            // ============================================================
            for (int i = 0; i < assets.Count; i++)
            {
                results[i].AddFailuresFrom(
                    _validationCore.ValidateWithoutDatabaseAccess(assets[i]));
            }

            // Fast-fail if business rules fail
            if (results.AnyInvalid())
            {
                return results;
            }

            // ============================================================
            // PHASE 2: IN-MEMORY BATCH CONFLICT DETECTION (NO DATABASE)
            // ============================================================
            var batchConflicts = _validationCore.ValidateBatchConflicts(assets);
            results.MergeFrom(batchConflicts);

            // Fast-fail if batch conflicts exist
            if (results.AnyInvalid())
            {
                return results;
            }

            // ============================================================
            // PHASE 3: DATABASE ACCESS CHECKS (UNIQUENESS)
            // ============================================================
            for (int i = 0; i < assets.Count; i++)
            {
                results[i].AddFailuresFrom(
                    _validationCore.ValidateWithDatabaseAccess(assets[i], context));
            }

            // ============================================================
            // PHASE 4: BULK PLACEMENT VALIDATION (OPTIMIZED)
            // ============================================================
            var placementValidator = new PlacementValidator(_entityLoader);
            var placementResults = placementValidator.ValidateBulkPlacements(assets);
            results.MergeFrom(placementResults);

            return results;
        }

        #endregion

        #region Pipeline Construction (Single Validation)

        private Validator<Asset> BuildValidationPipeline()
        {
            // Phase 1: No database access checks (fail fast on business rules)
            var noDatabaseChecks = Validator<Asset>
                .Create(a => _validationCore.ValidateWithoutDatabaseAccess(a))
                .StopOnFailure();

            // Phase 2: Database access checks (uniqueness, placement)
            var databaseChecks = Validator<Asset>
                .Create(a => _validationCore.ValidateWithDatabaseAccess(a, null));

            return noDatabaseChecks.AndThen(databaseChecks);
        }

        #endregion
    }
}