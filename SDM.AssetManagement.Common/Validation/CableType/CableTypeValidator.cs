namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using System;
    using System.Collections.Generic;

    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Common.Services;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Public validator service for CableType validation with comprehensive error handling.
    /// </summary>
    public class CableTypeValidator
    {
        private readonly SdmEntityLoader _entityLoader;
        private readonly Validator<CableType> _validationPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="CableTypeValidator"/> class.
        /// </summary>
        /// <param name="entityLoader">The entity loader for querying cable types.</param>
        public CableTypeValidator(SdmEntityLoader entityLoader)
        {
            _entityLoader = entityLoader ?? throw new ArgumentNullException(nameof(entityLoader));
            _validationPipeline = BuildValidationPipeline();
        }

        /// <summary>
        /// Validates a CableType and returns a ValidationResult.
        /// Collects all errors without throwing exceptions.
        /// </summary>
        public ValidationResult Validate(CableType cableType)
        {
            if (cableType == null)
            {
                throw new ArgumentNullException(nameof(cableType));
            }

            return _validationPipeline.Validate(cableType);
        }

        /// <summary>
        /// Validates a CableType and throws a ValidationException if invalid.
        /// Use this when you want fail-fast behavior.
        /// </summary>
        public void ValidateAndThrow(CableType cableType)
        {
            _validationPipeline.ValidateAndThrow(cableType);
        }

        /// <summary>
        /// Validates with a custom error handling callback.
        /// </summary>
        public ValidationResult ValidateWithHandler(CableType cableType, Action<ValidationResult> onError)
        {
            return _validationPipeline.ValidateWithHandler(cableType, onError);
        }

        /// <summary>
        /// Validates name uniqueness — used for real-time UI validation.
        /// </summary>
        public ValidationResult IsCableTypeNameValid(string name, List<string> exceptIdentifiers = null)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(name))
            {
                result.AddFailReason(CableTypeValidationHandler.CableTypeValidationField.Name,
                    "Cable Type Name cannot be empty or whitespace.");
                return result;
            }

            if (IsNameInUse(name, exceptIdentifiers))
            {
                result.AddFailReason(CableTypeValidationHandler.CableTypeValidationField.Name,
                    $"Cable Type Name '{name}' is already in use.");
            }

            return result;
        }

        /// <summary>
        /// Validates name uniqueness for the specified <see cref="CableType"/> instance.
        /// Excludes the current cable type identifier from the uniqueness check.
        /// </summary>
        public ValidationResult IsCableTypeNameValid(CableType cableType)
        {
            return IsCableTypeNameValid(cableType.Name, new List<string> { cableType.Identifier });
        }

        #region Pipeline Construction

        private Validator<CableType> BuildValidationPipeline()
        {
            // Critical validations - stop on failure
            var criticalValidations = Validator<CableType>
                .Create(ValidateCriticalFields)
                .StopOnFailure();

            // Standard validations - collect all errors
            var standardValidations = Validator<CableType>
                .Create(ValidateCategories);

            return criticalValidations.AndThen(standardValidations);
        }

        #endregion

        #region Validation Methods

        private ValidationResult ValidateCriticalFields(CableType cableType)
        {
            return IsCableTypeNameValid(cableType);
        }

        private ValidationResult ValidateCategories(CableType cableType)
        {
            var result = new ValidationResult();

            if (!CableTypeValidationHandler.IsCableTypeCategoriesValid(cableType, out var categoriesResult))
            {
                result.AddFailuresFrom(categoriesResult);
            }

            return result;
        }

        #endregion

        #region Helper Methods

        private bool IsNameInUse(string name, List<string> exceptIdentifiers = null)
        {
            return _entityLoader.CountCableTypesByName(name, exceptIdentifiers) > 0;
        }

        #endregion
    }
}
