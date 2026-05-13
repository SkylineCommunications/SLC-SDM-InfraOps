namespace Skyline.DataMiner.SDM.FacilityManagement.Validation
{
    using System;

    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Public validator service for Rack validation with comprehensive error handling.
    /// </summary>
    public class RackValidator
    {
        private readonly Validator<Rack> _validationPipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="RackValidator"/> class.
        /// </summary>
        /// <param name="entityLoader">Shared entity loader service (handles all data access).</param>
        public RackValidator()
        {
            _validationPipeline = BuildValidationPipeline();
        }

        #region Rack Validation

        /// <summary>
        /// Validates a Rack and returns ValidationResult.
        /// Collects all errors without throwing exceptions.
        /// </summary>
        public ValidationResult Validate(Rack rack)
        {
            if (rack == null)
            {
                throw new ArgumentNullException(nameof(rack));
            }

            return _validationPipeline.Validate(rack);
        }

        /// <summary>
        /// Validates a Rack and throws ValidationException if invalid.
        /// Use this when you want fail-fast behavior.
        /// </summary>
        public void ValidateAndThrow(Rack rack)
        {
            _validationPipeline.ValidateAndThrow(rack);
        }

        /// <summary>
        /// Validates with custom error handling callback.
        /// </summary>
        public ValidationResult ValidateWithHandler(Rack rack, Action<ValidationResult> onError)
        {
            return _validationPipeline.ValidateWithHandler(rack, onError);
        }

        #endregion

      
        #region Pipeline Construction

        private Validator<Rack> BuildValidationPipeline()
        {
            // Critical validations - stop on failure
            var criticalValidations = Validator<Rack>
                .Create(ValidateCriticalFields)
                .StopOnFailure();

            // Standard validations - collect all errors
            var standardValidations = Validator<Rack>
                .Create(ValidateDimensions)
                .AndThen(ValidatePowerCapacity);

            // Combine: critical first, then standard
            return criticalValidations.AndThen(standardValidations);
        }

        #endregion

        #region Validation Methods

        private ValidationResult ValidateCriticalFields(Rack rack)
        {
            var result = new ValidationResult();

            // Rack Units is critical
            if (rack.Capacity.RackUnitsField.Changed)
            {
                if (!RackValidationHandler.IsRackUnitCapacityValid(rack, out var unitsResult))
                {
                    result.AddFailuresFrom(unitsResult);
                }
            }

            return result;
        }

        private ValidationResult ValidateDimensions(Rack rack)
        {
            var result = new ValidationResult();

            if (rack.HeightField.Changed)
            {
                if (!RackValidationHandler.IsRackHeightValid(rack, out var heightResult))
                {
                    result.AddFailuresFrom(heightResult);
                }
            }

            if (rack.WidthField.Changed)
            {
                if (!RackValidationHandler.IsRackWidthValid(rack, out var widthResult))
                {
                    result.AddFailuresFrom(widthResult);
                }
            }

            if (rack.DepthField.Changed)
            {
                if (!RackValidationHandler.IsRackDepthValid(rack, out var depthResult))
                {
                    result.AddFailuresFrom(depthResult);
                }
            }

            return result;
        }

        private ValidationResult ValidatePowerCapacity(Rack rack)
        {
            var result = new ValidationResult();

            if (rack.Capacity.PowerCapacityField.Changed)
            {
                if (!RackValidationHandler.IsRackPowerCapacityValid(rack, out var powerResult))
                {
                    result.AddFailuresFrom(powerResult);
                }
            }

            return result;
        }

        #endregion

    }
}