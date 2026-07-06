namespace Skyline.DataMiner.SDM.InfraOpsProperties.Validation
{
    using System;

    using Skyline.DataMiner.SDM.InfraOpsProperties.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Public validator service for Property validation with comprehensive error handling.
    /// </summary>
    public class PropertyValidator
    {
        private readonly Validator<Property> _validationPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyValidator"/> class.
        /// </summary>
        public PropertyValidator()
        {
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
                .Create(ValidateStringConstraints)
                .AndThen(ValidateDiscreteConstraints);

            // Combine: critical first, then standard
            return criticalValidations.AndThen(standardValidations);
        }

        #endregion

        #region Validation Methods

        private ValidationResult ValidateInfo(Property property)
        {
            var result = new ValidationResult();

            if (property.NameField.Changed && !PropertyValidationHandler.IsNameValid(property, out var nameResult))
            {
                result.AddFailuresFrom(nameResult);
            }

            if (property.ScopeField.Changed && !PropertyValidationHandler.IsScopeValid(property, out var scopeResult))
            {
                result.AddFailuresFrom(scopeResult);
            }

            return result;
        }

        private ValidationResult ValidateStringConstraints(Property property)
        {
            var result = new ValidationResult();

            if (property.StringSizeLimitField.Changed && !PropertyValidationHandler.IsStringSizeLimitValid(property, out var sizeLimitResult))
            {
                result.AddFailuresFrom(sizeLimitResult);
            }

            return result;
        }

        private ValidationResult ValidateDiscreteConstraints(Property property)
        {
            var result = new ValidationResult();

            if ((property.PropertyTypeField.Changed || property.OptionsField.Changed) && !PropertyValidationHandler.IsOptionsValid(property, out var optionsResult))
            {
                result.AddFailuresFrom(optionsResult);
            }

            return result;
        }

        #endregion
    }
}
