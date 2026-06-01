namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using System;

    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Common.Services;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Public validator service for DataPort validation.
    /// DataPorts are validated in context of their parent Asset.
    /// </summary>
    public class DataPortValidator
    {
         private readonly DataPortValidationCore _validationCore;
        private readonly Validator<DataPort> _validationPipeline;

        public DataPortValidator(SdmEntityLoader entityLoader)
        {
           _validationCore = new DataPortValidationCore(entityLoader);
            _validationPipeline = BuildValidationPipeline();
        }

        #region Single DataPort Validation

        /// <summary>
        /// Validates a single DataPort and returns ValidationResult.
        /// Collects all errors without throwing exceptions.
        /// </summary>
        public ValidationResult Validate(DataPort dataPort)
        {
            if (dataPort == null)
            {
                throw new ArgumentNullException(nameof(dataPort));
            }

            return _validationPipeline.Validate(dataPort);
        }

        /// <summary>
        /// Validates a DataPort and throws ValidationException if invalid.
        /// Use this when you want fail-fast behavior.
        /// </summary>
        public void ValidateAndThrow(DataPort dataPort)
        {
            _validationPipeline.ValidateAndThrow(dataPort);
        }

        /// <summary>
        /// Validates with custom error handling callback.
        /// </summary>
        public ValidationResult ValidateWithHandler(DataPort dataPort, Action<ValidationResult> onError)
        {
            return _validationPipeline.ValidateWithHandler(dataPort, onError);
        }

        #endregion

        #region Pipeline Construction

        private Validator<DataPort> BuildValidationPipeline()
        {
            // Critical validations (business rules) - stop on failure
            var criticalValidations = Validator<DataPort>
                .Create(ValidateCriticalFields)
                .StopOnFailure();

            // Database validations - collect all errors
            var databaseValidations = Validator<DataPort>
                .Create(ValidateDatabaseFields);

            // Combine: critical first, then database
            return criticalValidations.AndThen(databaseValidations);
        }

        #endregion

        #region Validation Methods

        private ValidationResult ValidateCriticalFields(DataPort dataPort)
        {
            // Phase 1: No-database validation (mandatory fields, business rules)
            return _validationCore.ValidateWithoutDatabaseAccess(dataPort);
        }

        private ValidationResult ValidateDatabaseFields(DataPort dataPort)
        {
            // Phase 2: Database validation (Port Type, Asset context)
            return _validationCore.ValidateWithDatabaseAccess(dataPort);
        }

        #endregion
    }
}