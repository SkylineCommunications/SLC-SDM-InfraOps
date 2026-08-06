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

    /// <summary>
    /// Public validator service for AssetClass validation with comprehensive error handling.
    /// </summary>
    public class AssetClassValidator : ValidatorBase<AssetClass>
    {
        private readonly SdmEntityLoader _entityLoader;
        private readonly Validator<AssetClass> _validationPipeline;


        /// <summary>
        /// Initializes a new instance of the <see cref="AssetClassValidator"/> class.
        /// </summary>
        /// <param name="entityLoader">The entity loader for querying asset classes and device types.</param>
        public AssetClassValidator(SdmEntityLoader entityLoader)
        {
            _entityLoader = entityLoader ?? throw new ArgumentNullException(nameof(entityLoader));
            _validationPipeline = BuildValidationPipeline();
        }

        /// <summary>
        /// Validates an AssetClass and returns ValidationResult.
        /// Collects all errors without throwing exceptions.
        /// <para><b>Not suitable for bulk scenarios</b>: issues one DB query per item. Use <see cref="ValidateBulk"/> instead.</para>
        /// </summary>
        protected override ValidationResult Validate(AssetClass assetClass)
        {
            return _validationPipeline.Validate(assetClass);
        }

        /// <summary>
        /// Validates an AssetClass and throws ValidationException if invalid.
        /// Use this when you want fail-fast behavior.
        /// <para><b>Not suitable for bulk scenarios</b>: issues one DB query per item. Use <see cref="ValidateBulk"/> instead.</para>
        /// </summary>
        public void ValidateAndThrow(AssetClass assetClass)
        {
            _validationPipeline.ValidateAndThrow(assetClass);
        }

        /// <summary>
        /// Validates with custom error handling callback.
        /// <para><b>Not suitable for bulk scenarios</b>: issues one DB query per item. Use <see cref="ValidateBulk"/> instead.</para>
        /// </summary>
        public ValidationResult ValidateWithHandler(AssetClass assetClass, Action<ValidationResult> onError)
        {
            return _validationPipeline.ValidateWithHandler(assetClass, onError);
        }

        /// <summary>
        /// Validates a batch of AssetClasses in three phases:
        /// 1. Non-database checks per item (fast-fail on business rule violations).
        /// 2. In-memory batch conflict detection (name uniqueness within batch).
        /// 3. Database checks per item (name uniqueness vs DB, power supply against device type).
        /// Results are returned in the same order as the input list.
        /// </summary>
        protected override List<ValidationResult> ValidateBulk(List<AssetClass> assetClasses)
        {
            if (assetClasses == null || !assetClasses.Any())
            {
                return new List<ValidationResult>();
            }

            var results = assetClasses.Select(_ => new ValidationResult()).ToList();

            // ============================================================
            // PHASE 1: NO DATABASE ACCESS CHECKS (BUSINESS RULES)
            // ============================================================
            for (int i = 0; i < assetClasses.Count; i++)
            {
                results[i].AddFailuresFrom(ValidateWithoutDatabaseAccess(assetClasses[i]));
            }

            if (results.AnyInvalid())
            {
                return results;
            }

            // ============================================================
            // PHASE 2: IN-MEMORY BATCH CONFLICT DETECTION
            // ============================================================
            var batchConflicts = ValidateNameDuplicatesInBatch(assetClasses);
            results.MergeFrom(batchConflicts);

            if (results.AnyInvalid())
            {
                return results;
            }

            // ============================================================
            // PHASE 2.5: BULK NAME UNIQUENESS CHECK AGAINST DATABASE
            // One OR-based query via Tools.RetrieveBigOrFilter — no large AND filter.
            // Phase 3 per-item check skips name when context is present.
            // ============================================================
            var nameDbConflicts = ValidateBulkNamesAgainstDatabase(assetClasses);
            results.MergeFrom(nameDbConflicts);

            if (results.AnyInvalid())
            {
                return results;
            }

            // ============================================================
            // PHASE 3: DATABASE ACCESS CHECKS
            // ============================================================
            results.MergeFrom(ValidateBulkReferencesAgainstDatabase(assetClasses));

            if (results.AnyInvalid())
            {
                return results;
            }

            var deviceTypeMap = _entityLoader.GetDeviceTypesByDomIds(
                    assetClasses
                        .Where(ac => ac.DeviceTypeId != null && ac.DeviceTypeId.HasValue())
                        .Select(ac => ac.DeviceTypeId.Identifier)
                        .Distinct()
                        .ToList())
                .ToDictionary(dt => dt.Identifier);

            for (int i = 0; i < assetClasses.Count; i++)
            {
                results[i].AddFailuresFrom(ValidateWithDatabaseAccess(assetClasses[i], deviceTypeMap));
            }

            return results;
        }

        protected override ValidationResult ValidateForDelete(AssetClass assetClass)
        {
            if (assetClass == null)
            {
                throw new ArgumentNullException(nameof(assetClass));
            }

            return ValidateNotInUseWhenDeleted(assetClass);
        }

        protected override List<ValidationResult> ValidateBulkForDelete(List<AssetClass> assetClasses)
        {
            if (assetClasses == null || !assetClasses.Any())
            {
                return new List<ValidationResult>();
            }

            return ValidateNotInUseWhenDeleted(assetClasses);
        }

        private ValidationResult ValidateNotInUseWhenDeleted(AssetClass assetClass)
        {
            return ValidateNotInUseWhenDeleted(new List<AssetClass> { assetClass })[0];
        }

        private List<ValidationResult> ValidateNotInUseWhenDeleted(List<AssetClass> assetClasses)
        {
            var results = assetClasses.Select(_ => new ValidationResult()).ToList();

            var identifiers = assetClasses
                .Select(ac => ac.Identifier)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            var assetClassIdsInUse = _entityLoader.GetAssetsByAssetClassIds(identifiers)
                .Where(asset => asset.AssetClassId != null && asset.AssetClassId.HasValue())
                .Select(asset => asset.AssetClassId.Identifier)
                .ToHashSet();

            for (int i = 0; i < assetClasses.Count; i++)
            {
                if (assetClasses[i].State == SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Active)
                {
                    results[i].AddFailReason(
                        AssetClassValidationHandler.AssetClassValidationField.State,
                        "Asset Class must not be in 'Active' State to Delete");
                }

                if (assetClassIdsInUse.Contains(assetClasses[i].Identifier))
                {
                    results[i].AddFailReason(
                        AssetClassValidationHandler.AssetClassValidationField.AssetClass,
                        "There are still Assets in this Asset Class. Please remove them first");
                }
            }

            return results;
        }

        /// <summary>
        /// Validates name uniqueness — used for real-time UI validation.
        /// <para><b>Not suitable for bulk scenarios</b>: issues one DB query per call. Use <see cref="ValidateBulk"/> instead.</para>
        /// </summary>
        public ValidationResult IsAssetClassNameValid(string name, string exceptIdentifier = null)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(name))
            {
                result.AddFailReason(AssetClassValidationHandler.AssetClassValidationField.Name,
                    "Asset Class Name cannot be empty or whitespace.");
                return result;
            }

            if (IsNameInUse(name, exceptIdentifier))
            {
                result.AddFailReason(AssetClassValidationHandler.AssetClassValidationField.Name,
                    $"Asset Class Name '{name}' is already in use.");
            }

            return result;
        }

        /// <summary>
        /// Validates the uniqueness of the AssetClass name for the specified <see cref="AssetClass"/> instance.
        /// Excludes the current asset class identifier from the uniqueness check.
        /// <para><b>Not suitable for bulk scenarios</b>: issues one DB query per call. Use <see cref="ValidateBulk"/> instead.</para>
        /// </summary>
        /// <param name="assetClass">The asset class to validate.</param>
        /// <returns>A <see cref="ValidationResult"/> indicating whether the asset class name is valid.</returns>
        public ValidationResult IsAssetClassNameValid(AssetClass assetClass)
        {
            return IsAssetClassNameValid(assetClass.Name, assetClass.Identifier);
        }

        #region Pipeline Construction

        private Validator<AssetClass> BuildValidationPipeline()
        {
            // Phase 1: No database access checks (fail fast on business rules)
            var noDatabaseChecks = Validator<AssetClass>
                .Create(ValidateWithoutDatabaseAccess)
                .StopOnFailure();

            // Phase 2: Database access checks (uniqueness, relationships)
            var databaseChecks = Validator<AssetClass>
                .Create(ac => ValidateWithDatabaseAccess(ac));

            return noDatabaseChecks.AndThen(databaseChecks);
        }

        #endregion

        #region Validation Methods

        private ValidationResult ValidateWithoutDatabaseAccess(AssetClass assetClass)
        {
            // DeviceTypeId format is critical - stop if invalid
            var criticalCheck = Validator<AssetClass>
                .Create(ValidateNonDbCriticalFields)
                .StopOnFailure();

            var standardChecks = Validator<AssetClass>
                .Create(ValidateDimensions)
                .AndThen(ValidatePowerConsumption)
                .AndThen(ValidateCollections);

            return criticalCheck.AndThen(standardChecks).Validate(assetClass);
        }

        private ValidationResult ValidateNonDbCriticalFields(AssetClass assetClass)
        {
            var result = new ValidationResult();

            if (assetClass.ShouldValidate(assetClass.DeviceTypeIdField)
                && !AssetClassValidationHandler.IsAssetClassDeviceTypeValid(assetClass, out var deviceTypeResult))
            {
                result.AddFailuresFrom(deviceTypeResult);
            }

            return result;
        }

        private ValidationResult ValidateWithDatabaseAccess(AssetClass assetClass, Dictionary<string, DeviceType> deviceTypeCache = null)
        {
            var validations = new List<ValidationResult>();

            bool isBulkValidation = deviceTypeCache != null;    

            // Name uniqueness: in bulk mode it was already resolved in Phase 2.5 via bulk DB query.
            // In single mode, use the standard per-item check (1 exclude ID — safe).
            if (!isBulkValidation && assetClass.ShouldValidate(assetClass.NameField))
            {
                validations.Add(IsAssetClassNameValid(assetClass.Name, assetClass.Identifier));
            }

            // Device-type-dependent validation (Power Supply, HeightU for Rack Unit Consumer)
            if ((assetClass.DeviceTypeIdField.Changed || assetClass.PowerSupplyField.Changed || assetClass.HeightUField.Changed)
                && assetClass.DeviceTypeId.HasValue())
            {
                try
                {
                    validations.Add(ValidateAgainstDeviceType(assetClass, deviceTypeCache));
                }
                catch (Exception ex)
                {
                    var r = new ValidationResult();
                    r.AddFailReason(AssetClassValidationHandler.AssetClassValidationField.DeviceTypeId,
                        $"Error validating against device type: {ex.Message}");
                    validations.Add(r);
                }
            }

            validations.Add(ValidateReferencesAgainstDatabase(assetClass));

            return validations.MergeAll();
        }

        private ValidationResult ValidateAgainstDeviceType(AssetClass assetClass, Dictionary<string, DeviceType> deviceTypeCache = null)
        {
            var result = new ValidationResult();

            if (!assetClass.DeviceTypeId.HasValue())
            {
                return result;
            }

                DeviceType deviceType;
            if (deviceTypeCache != null && deviceTypeCache.TryGetValue(assetClass.DeviceTypeId, out var cached))
            {
                deviceType = cached;
            }
            else
            {
                deviceType = _entityLoader.LoadDeviceType(assetClass.DeviceTypeId);
                if (deviceTypeCache != null)
                {
                    deviceTypeCache[assetClass.DeviceTypeId] = deviceType;
                }
            }

            if (deviceType == null)
            {
                result.AddFailReason(AssetClassValidationHandler.AssetClassValidationField.DeviceTypeId,
                    $"Device Type not found. Referenced Device Type '{assetClass.DeviceTypeId.Identifier}' does not exist.");
                return result;
            }

            if (deviceType.TagsInfo.Tags.Contains(SlcAsset_Management.Enums.TagOption.PowerProvider) && assetClass.PowerSupply == null)
            {
                result.AddFailReason(AssetClassValidationHandler.AssetClassValidationField.PowerSupply,
                    "Asset Class with 'Power Provider' Device Type must have a Power Supply.");
            }

            bool isRackUnitConsumer = deviceType.TagsInfo.Tags.Contains(SlcAsset_Management.Enums.TagOption.RackUnitConsumer);
            if (!AssetClassValidationHandler.IsHeightUnitValid(assetClass, isRackUnitConsumer, out var heightUResult))
            {
                result.AddFailuresFrom(heightUResult);
            }

            return result;
        }

        private ValidationResult ValidateReferencesAgainstDatabase(AssetClass assetClass)
            {
                var result = new ValidationResult();

                if (assetClass.ShouldValidate(assetClass.DeviceTypeIdField) && assetClass.DeviceTypeId.HasValue())
                {
                    var deviceType = _entityLoader.LoadDeviceType(assetClass.DeviceTypeId);
                    if (deviceType == null)
                    {
                        result.AddFailReason(AssetClassValidationHandler.AssetClassValidationField.DeviceTypeId,
                            $"Referenced Device Type '{assetClass.DeviceTypeId.Identifier}' does not exist.");
                    }
                }

                ValidateDataPortTypeReferences(assetClass.DataPorts, "AssetClass.DataPorts.Type", "DataPorts", result);
                ValidatePowerPortTypeReferences(assetClass.PowerPorts, "AssetClass.PowerPorts.PortType", "PowerPorts", result);
                ValidateExternalGuid(assetClass.Manufacturer, AssetManagementExternalReferenceType.Organization, "AssetClass.Manufacturer", "Manufacturer", result);

                return result;
            }

            private List<ValidationResult> ValidateBulkReferencesAgainstDatabase(List<AssetClass> assetClasses)
            {
                var results = assetClasses.Select(_ => new ValidationResult()).ToList();

                var deviceTypeIds = assetClasses
                    .Where(ac => ac.ShouldValidate(ac.DeviceTypeIdField) && ac.DeviceTypeId != null && ac.DeviceTypeId.HasValue())
                    .Select(ac => ac.DeviceTypeId.Identifier)
                    .Distinct()
                    .ToList();
                var existingDeviceTypeIds = _entityLoader.GetDeviceTypesByDomIds(deviceTypeIds).Select(dt => dt.Identifier).ToHashSet();

                var portTypeIds = assetClasses
                    .SelectMany(ac => (ac.DataPorts ?? new List<DataPortInfo>())
                        .Where(p => p?.Type != null && p.Type.HasValue())
                        .Select(p => p.Type.Identifier)
                        .Concat((ac.PowerPorts ?? new List<PowerPortInfo>())
                            .Where(p => p?.PortType != null && p.PortType.HasValue())
                            .Select(p => p.PortType.Identifier)))
                    .Distinct()
                    .ToList();
                var existingPortTypeIds = _entityLoader.GetPortTypesByDomIds(portTypeIds).Select(pt => pt.Identifier).ToHashSet();

                var existingManufacturers = GetExistingExternalIds(
                    AssetManagementExternalReferenceType.Organization,
                    assetClasses.Where(ac => ac.ShouldValidate(ac.ManufacturerField)).Select(ac => ac.Manufacturer));

                for (int i = 0; i < assetClasses.Count; i++)
                {
                    var assetClass = assetClasses[i];
                    if (assetClass.ShouldValidate(assetClass.DeviceTypeIdField)
                        && assetClass.DeviceTypeId != null
                        && assetClass.DeviceTypeId.HasValue()
                        && !existingDeviceTypeIds.Contains(assetClass.DeviceTypeId.Identifier))
                    {
                        results[i].AddFailReason(AssetClassValidationHandler.AssetClassValidationField.DeviceTypeId,
                            $"Referenced Device Type '{assetClass.DeviceTypeId.Identifier}' does not exist.");
                    }

                    foreach (var port in assetClass.DataPorts ?? new List<DataPortInfo>())
                    {
                        if (port?.Type != null && port.Type.HasValue() && !existingPortTypeIds.Contains(port.Type.Identifier))
                        {
                            results[i].AddFailReason("AssetClass.DataPorts.Type", "DataPorts", $"Referenced Port Type '{port.Type.Identifier}' does not exist.");
                        }
                    }

                    foreach (var port in assetClass.PowerPorts ?? new List<PowerPortInfo>())
                    {
                        if (port?.PortType != null && port.PortType.HasValue() && !existingPortTypeIds.Contains(port.PortType.Identifier))
                        {
                            results[i].AddFailReason("AssetClass.PowerPorts.PortType", "PowerPorts", $"Referenced Port Type '{port.PortType.Identifier}' does not exist.");
                        }
                    }

                    if (assetClass.ShouldValidate(assetClass.ManufacturerField)
                        && assetClass.Manufacturer != Guid.Empty
                        && _entityLoader.ExternalReferenceChecker != null
                        && !existingManufacturers.Contains(assetClass.Manufacturer))
                    {
                        results[i].AddFailReason("AssetClass.Manufacturer", "Manufacturer", $"Referenced Organization '{assetClass.Manufacturer}' does not exist.");
                    }
                }

                return results;
            }

            private void ValidateDataPortTypeReferences(IEnumerable<DataPortInfo> ports, string fieldId, string fieldName, ValidationResult result)
            {
                foreach (var port in ports ?? Enumerable.Empty<DataPortInfo>())
                {
                    if (port?.Type == null || !port.Type.HasValue())
                    {
                        continue;
                    }

                    var reference = port.Type;
                    if (!_entityLoader.GetPortTypesByDomIds(new List<string> { reference.Identifier }).Any())
                    {
                        result.AddFailReason(fieldId, fieldName, $"Referenced Port Type '{reference.Identifier}' does not exist.");
                    }
                }
            }

            private void ValidatePowerPortTypeReferences(IEnumerable<PowerPortInfo> ports, string fieldId, string fieldName, ValidationResult result)
            {
                foreach (var port in ports ?? Enumerable.Empty<PowerPortInfo>())
                {
                    if (port?.PortType == null || !port.PortType.HasValue())
                    {
                        continue;
                    }

                    var reference = port.PortType;
                    if (!_entityLoader.GetPortTypesByDomIds(new List<string> { reference.Identifier }).Any())
                    {
                        result.AddFailReason(fieldId, fieldName, $"Referenced Port Type '{reference.Identifier}' does not exist.");
                    }
                }
            }

            private void ValidateExternalGuid(Guid identifier, AssetManagementExternalReferenceType type, string fieldId, string fieldName, ValidationResult result)
            {
                if (identifier == Guid.Empty || _entityLoader.ExternalReferenceChecker == null)
                {
                    return;
                }

                if (!GetExistingExternalIds(type, new[] { identifier }).Contains(identifier))
                {
                    result.AddFailReason(fieldId, fieldName, $"Referenced {FormatExternalType(type)} '{identifier}' does not exist.");
                }
            }

            private HashSet<Guid> GetExistingExternalIds(AssetManagementExternalReferenceType type, IEnumerable<Guid> identifiers)
            {
                var ids = identifiers.Where(id => id != Guid.Empty).Distinct().ToList();
                if (!ids.Any() || _entityLoader.ExternalReferenceChecker == null)
                {
                    return new HashSet<Guid>();
                }

                return (_entityLoader.ExternalReferenceChecker.GetExistingIdentifiers(type, ids) ?? new List<Guid>()).ToHashSet();
            }

            private static string FormatExternalType(AssetManagementExternalReferenceType type)
            {
                return type == AssetManagementExternalReferenceType.ContactPersonRole ? "Contact Person Role" : type.ToString();
            }

        private ValidationResult ValidateDimensions(AssetClass assetClass)
        {
            var validations = new List<ValidationResult>();

            if (assetClass.ShouldValidate(assetClass.DepthField)
                && !AssetClassValidationHandler.IsDepthValid(assetClass, out var depthResult))
                validations.Add(depthResult);

            if (assetClass.ShouldValidate(assetClass.WidthField)
                && !AssetClassValidationHandler.IsWidthValid(assetClass, out var widthResult))
                validations.Add(widthResult);

            if (assetClass.ShouldValidate(assetClass.HeightField)
                && !AssetClassValidationHandler.IsHeightValid(assetClass, out var heightResult))
                validations.Add(heightResult);

            if (assetClass.ShouldValidate(assetClass.HeightUField)
                && !AssetClassValidationHandler.IsHeightUnitValid(assetClass, out var heightUResult))
                validations.Add(heightUResult);

            if (assetClass.ShouldValidate(assetClass.WeightField)
                && !AssetClassValidationHandler.IsWeightValid(assetClass, out var weightResult))
                validations.Add(weightResult);

            return validations.MergeAll();
        }

        private ValidationResult ValidatePowerConsumption(AssetClass assetClass)
        {
            var validations = new List<ValidationResult>();

            if (assetClass.ShouldValidate(assetClass.TypicalPowerConsumptionField)
                && !AssetClassValidationHandler.IsTypicalPowerConsumptionValid(assetClass, out var typicalResult))
                validations.Add(typicalResult);

            if (assetClass.ShouldValidate(assetClass.MaximumPowerConsumptionField)
                && !AssetClassValidationHandler.IsMaxPowerConsumptionValid(assetClass, out var maxResult))
                validations.Add(maxResult);

            return validations.MergeAll();
        }

        private ValidationResult ValidateCollections(AssetClass assetClass)
        {
            var validations = new List<ValidationResult>();

            if (assetClass.ShouldValidate(assetClass.DataPortsField))
            {
                validations.Add(AssetClassValidationHandler.ValidateAssetClassDataPort(assetClass));
            }

            if (assetClass.ShouldValidate(assetClass.PowerPortsField))
            {
                validations.Add(AssetClassValidationHandler.ValidateAssetClassPowerPort(assetClass));
            }

            if (assetClass.ShouldValidate(assetClass.HoldersField))
            {
                validations.Add(AssetClassValidationHandler.ValidateAssetClassHolders(assetClass));
            }

            return validations.MergeAll();
        }

        #endregion

        #region Helper Methods

        private bool IsNameInUse(string name, string exceptIdentifier = null)
        {
            return _entityLoader.CountAssetClassesByName(name, exceptIdentifier) > 0;
        }

        private List<ValidationResult> ValidateBulkNamesAgainstDatabase(List<AssetClass> assetClasses)
        {
            var results = assetClasses.Select(_ => new ValidationResult()).ToList();

            var uniqueNames = assetClasses
                .Select(a => a.Name)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!uniqueNames.Any())
            {
                return results;
            }

            var batchIds = new HashSet<string>(
                assetClasses.Select(a => a.Identifier).Where(id => !string.IsNullOrWhiteSpace(id)));

            // Single bulk query via Tools.RetrieveBigOrFilter — no large AND filter
            var dbMatches = _entityLoader.GetAssetClassesByNames(uniqueNames);

            // External conflicts: DB records whose identifier is NOT in the current batch
            var externalConflictNames = dbMatches
                .Where(r => !batchIds.Contains(r.Identifier))
                .Select(r => r.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < assetClasses.Count; i++)
            {
                var name = assetClasses[i].Name;
                if (!string.IsNullOrWhiteSpace(name) && externalConflictNames.Contains(name))
                {
                    results[i].AddFailReason(
                        AssetClassValidationHandler.AssetClassValidationField.Name,
                        $"Asset Class Name '{name}' is already in use.");
                }
            }

            return results;
        }

        private static List<ValidationResult> ValidateNameDuplicatesInBatch(List<AssetClass> assetClasses)
        {
            var results = assetClasses.Select(_ => new ValidationResult()).ToList();

            var duplicateNames = assetClasses
                .Select((ac, idx) => new { ac.Name, Index = idx })
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1);

            foreach (var group in duplicateNames)
            {
                foreach (var item in group)
                {
                    results[item.Index].AddFailReason(
                        AssetClassValidationHandler.AssetClassValidationField.Name,
                        $"Asset Class Name '{item.Name}' is duplicated within the batch.");
                }
            }

            return results;
        }

        #endregion
    }
}