namespace Skyline.DataMiner.SDM.InfraOpsProperties.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.InfraOps.Common.Validation;
    using Skyline.DataMiner.SDM.InfraOpsProperties.Helpers;
    using Skyline.DataMiner.SDM.InfraOpsProperties.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Public validator service for Property validation, including data access for (Scope, Name) uniqueness checks.
    /// </summary>
    public class PropertyValidator
    {
        private readonly IInfraOpsPropertiesApiHelper _helper;
        private readonly Validator<Property> _validationPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyValidator"/> class.
        /// </summary>
        /// <param name="helper">
        /// The InfraOps Properties API helper used to query existing Properties for (Scope, Name) uniqueness checks.
        /// Note: this is captured by reference during <see cref="InfraOpsPropertiesApiHelper"/> construction, before
        /// its repositories are wired up. Only <see cref="Validate"/>/<see cref="ValidateAndThrow"/> (called
        /// after construction completes) access <paramref name="helper"/>'s repositories.
        /// </param>
        public PropertyValidator(IInfraOpsPropertiesApiHelper helper)
        {
            _helper = helper ?? throw new ArgumentNullException(nameof(helper));
            _validationPipeline = BuildValidationPipeline();
        }

        #region Property Validation

        /// <summary>
        /// Validates a Property and returns ValidationResult.
        /// Collects all errors without throwing exceptions.
        /// </summary>
        public ValidationResult Validate(Property property)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            return _validationPipeline.Validate(property);
        }

        /// <summary>
        /// Validates a Property and throws ValidationException if invalid.
        /// Use this when you want fail-fast behavior.
        /// </summary>
        public void ValidateAndThrow(Property property)
        {
            _validationPipeline.ValidateAndThrow(property);
        }

        /// <summary>
        /// Validates with custom error handling callback.
        /// </summary>
        public ValidationResult ValidateWithHandler(Property property, Action<ValidationResult> onError)
        {
            return _validationPipeline.ValidateWithHandler(property, onError);
        }

        #endregion

        #region Pipeline Construction

        private Validator<Property> BuildValidationPipeline()
        {
            // Critical validations - stop on failure
            var criticalValidations = Validator<Property>
                .Create(ValidateInfo)
                .StopOnFailure();

            // Standard validations - collect all errors
            var standardValidations = Validator<Property>
                .Create(ValidateNameUniqueness)
                .AndThen(ValidateStringConstraints)
                .AndThen(ValidateDiscreteConstraints);

            // Combine: critical first, then standard
            return criticalValidations.AndThen(standardValidations);
        }

        #endregion

        #region Validation Methods

        private ValidationResult ValidateInfo(Property property)
        {
            var result = new ValidationResult();

            if (property.ShouldValidate(property.NameField) && !PropertyValidationHandler.IsNameValid(property, out var nameResult))
            {
                result.AddFailuresFrom(nameResult);
            }

            if (property.ShouldValidate(property.ScopeField) && !PropertyValidationHandler.IsScopeValid(property, out var scopeResult))
            {
                result.AddFailuresFrom(scopeResult);
            }

            return result;
        }

        private ValidationResult ValidateStringConstraints(Property property)
        {
            var result = new ValidationResult();

            if (property.ShouldValidate(property.StringSizeLimitField) && !PropertyValidationHandler.IsStringSizeLimitValid(property, out var sizeLimitResult))
            {
                result.AddFailuresFrom(sizeLimitResult);
            }

            return result;
        }

        private ValidationResult ValidateDiscreteConstraints(Property property)
        {
            var result = new ValidationResult();

            if (property.ShouldValidateAny(property.PropertyTypeField, property.DiscreetsField) && !PropertyValidationHandler.IsOptionsValid(property, out var optionsResult))
            {
                result.AddFailuresFrom(optionsResult);
            }

            return result;
        }

        private ValidationResult ValidateNameUniqueness(Property property)
        {
            var result = new ValidationResult();

            if (!property.ShouldValidateAny(property.NameField, property.ScopeField))
            {
                return result;
            }

            if (IsNameInUse(property.Scope, property.Name, property.Identifier))
            {
                result.AddFailReason(PropertyValidationHandler.PropertyValidationField.Name, $"Property Name '{property.Name}' is already in use within Scope '{property.Scope}'.");
            }

            return result;
        }

        private bool IsNameInUse(string scope, string name, string exceptIdentifier)
        {
            FilterElement<Property> filter = PropertyExposers.Scope.Equal(scope).AND(PropertyExposers.Name.Equal(name));

            if (!string.IsNullOrEmpty(exceptIdentifier))
            {
                filter = filter.AND(PropertyExposers.Identifier.NotEqual(exceptIdentifier));
            }

            return _helper.Properties.Count(filter) > 0;
        }

        /// <summary>
        /// Validates multiple Properties in bulk. Results are returned in the same order as the input Properties.
        /// In addition to the per-Property checks, this also detects (Scope, Name) conflicts <em>within the batch
        /// itself</em> (i.e. two Properties being saved together that share the same Scope and Name), which a
        /// single-Property DB uniqueness query cannot catch since none of the batch's entries are persisted yet.
        /// Mirrors the same batch-conflict detection used for PlanAndBuildJob/JobType.
        /// </summary>
        public List<ValidationResult> ValidateBulk(List<Property> properties)
        {
            if (properties == null || !properties.Any())
            {
                return new List<ValidationResult>();
            }

            // Initialize results - same order as input
            var results = properties.Select(p => new ValidationResult()).ToList();

            // ============================================================
            // PHASE 1: NO DATABASE ACCESS CHECKS (BUSINESS RULES)
            // ============================================================
            for (int i = 0; i < properties.Count; i++)
            {
                results[i].AddFailuresFrom(ValidateInfo(properties[i]));
            }

            // Fast-fail if business rules fail
            if (results.AnyInvalid())
            {
                return results;
            }

            // ============================================================
            // PHASE 2: IN-MEMORY BATCH CONFLICT DETECTION (NO DATABASE)
            // ============================================================
            var batchConflicts = ValidateBatchConflicts(properties);
            results.MergeFrom(batchConflicts);

            // Fast-fail if batch conflicts exist
            if (results.AnyInvalid())
            {
                return results;
            }

            // ============================================================
            // PHASE 3: DATABASE ACCESS CHECKS (UNIQUENESS) + REMAINING RULES
            // ============================================================
            for (int i = 0; i < properties.Count; i++)
            {
                results[i].AddFailuresFrom(ValidateNameUniqueness(properties[i]));
                results[i].AddFailuresFrom(ValidateStringConstraints(properties[i]));
                results[i].AddFailuresFrom(ValidateDiscreteConstraints(properties[i]));
            }

            return results;
        }

        /// <summary>
        /// Detects (Scope, Name) conflicts among the Properties of a single batch (in-memory only, no database
        /// access). Result at index i corresponds to Property at index i.
        /// </summary>
        public List<ValidationResult> ValidateBatchConflicts(List<Property> properties)
        {
            var results = properties.Select(p => new ValidationResult()).ToList();

            var nameGroups = properties
                .Select((property, index) => new { property, index })
                .Where(x => x.property.ShouldValidateAny(x.property.NameField, x.property.ScopeField) &&
                            !string.IsNullOrWhiteSpace(x.property.Name) &&
                            !string.IsNullOrWhiteSpace(x.property.Scope))
                .GroupBy(x => $"{x.property.Scope}\u001F{x.property.Name}".ToUpperInvariant())
                .Where(g => g.Count() > 1);

            foreach (var group in nameGroups)
            {
                foreach (var item in group)
                {
                    results[item.index].AddFailReason(PropertyValidationHandler.PropertyValidationField.Name,
                        $"Property Name '{item.property.Name}' is duplicated within the validation batch for Scope '{item.property.Scope}'.");
                }
            }

            return results;
        }

        #endregion
    }
}
