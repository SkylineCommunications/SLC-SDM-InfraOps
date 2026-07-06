namespace Skyline.DataMiner.SDM.InfraOpsProperties.Validation
{
    using System;

    using Skyline.DataMiner.SDM.InfraOpsProperties.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Public validator service for PropertyValues validation with comprehensive error handling.
    /// </summary>
    public class PropertyValuesValidator
    {
        private readonly Validator<PropertyValues> _validationPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyValuesValidator"/> class.
        /// </summary>
        public PropertyValuesValidator()
        {
            _validationPipeline = BuildValidationPipeline();
        }

        #region PropertyValues Validation

        /// <summary>
        /// Validates a PropertyValues and returns ValidationResult.
        /// Collects all errors without throwing exceptions.
        /// </summary>
        public ValidationResult Validate(PropertyValues propertyValues)
        {
            if (propertyValues == null)
            {
                throw new ArgumentNullException(nameof(propertyValues));
            }

            return _validationPipeline.Validate(propertyValues);
        }

        /// <summary>
        /// Validates a PropertyValues and throws ValidationException if invalid.
        /// Use this when you want fail-fast behavior.
        /// </summary>
        public void ValidateAndThrow(PropertyValues propertyValues)
        {
            _validationPipeline.ValidateAndThrow(propertyValues);
        }

        /// <summary>
        /// Validates with custom error handling callback.
        /// </summary>
        public ValidationResult ValidateWithHandler(PropertyValues propertyValues, Action<ValidationResult> onError)
        {
            return _validationPipeline.ValidateWithHandler(propertyValues, onError);
        }

        #endregion

        #region Pipeline Construction

        private Validator<PropertyValues> BuildValidationPipeline()
        {
            // Critical validations - stop on failure
            var criticalValidations = Validator<PropertyValues>
                .Create(ValidateInfo)
                .StopOnFailure();

            // Standard validations - collect all errors
            var standardValidations = Validator<PropertyValues>
                .Create(ValidateValues);

            // Combine: critical first, then standard
            return criticalValidations.AndThen(standardValidations);
        }

        #endregion

        #region Validation Methods

        private ValidationResult ValidateInfo(PropertyValues propertyValues)
        {
            var result = new ValidationResult();

            if (propertyValues.LinkedObjectIDField.Changed && !PropertyValuesValidationHandler.IsLinkedObjectIDValid(propertyValues, out var linkedObjectIdResult))
            {
                result.AddFailuresFrom(linkedObjectIdResult);
            }

            if (propertyValues.ScopeField.Changed && !PropertyValuesValidationHandler.IsScopeValid(propertyValues, out var scopeResult))
            {
                result.AddFailuresFrom(scopeResult);
            }

            return result;
        }

        private ValidationResult ValidateValues(PropertyValues propertyValues)
        {
            var result = new ValidationResult();

            if (propertyValues.ValuesField.Changed && !PropertyValuesValidationHandler.IsValuesValid(propertyValues, out var valuesResult))
            {
                result.AddFailuresFrom(valuesResult);
            }

            return result;
        }

        #endregion
    }
}
