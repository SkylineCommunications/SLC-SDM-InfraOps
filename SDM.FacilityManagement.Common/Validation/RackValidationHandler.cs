namespace Skyline.DataMiner.SDM.FacilityManagement.Validation
{
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Static validation handler for Rack business rules.
    /// Contains pure validation logic without data access.
    /// </summary>
    public static class RackValidationHandler
    {
        private const double MAX_RACK_CAPACITY_U = 70;
        private const double MAX_RACK_SIZE_CM = 120;
        private const double MAX_RACK_HEIGHT_CM = 320;

        public enum RackValidationField
        {
            Rack,
            RackId,
            Name,
            RackUnits,
            RackPosition,
            Width,
            Depth,
            Height,
            PowerCapacity,
            RackSpacePosition,
            RackSpaceOccupied,
        }

        #region Identity Validation

        /// <summary>
        /// Validates that the Rack id is not empty or whitespace.
        /// </summary>
        public static bool IsRackIdValid(Rack rack, out ValidationResult result)
        {
            result = new ValidationResult();

            if (rack == null || string.IsNullOrWhiteSpace(rack.RackId))
            {
                result.AddFailReason(RackValidationField.RackId, "Rack Id cannot be empty or whitespace.");
            }

            return result.IsValid;
        }

        #endregion

        #region Dimensions Validation

        public static bool IsRackHeightValid(Rack rack, out ValidationResult result)
        {
            result = new ValidationResult();

            if (rack == null)
            {
                result.AddFailReason(RackValidationField.Rack, "Rack cannot be null.");
                return result.IsValid;
            }

            var height = rack.Height;

            if (height != null && (height > MAX_RACK_HEIGHT_CM || height < 0))
            {
                result.AddFailReason(RackValidationField.Height,
                    $"Rack Height must be between 0 and {MAX_RACK_HEIGHT_CM} cm.");
            }

            return result.IsValid;
        }

        public static bool IsRackDepthValid(Rack rack, out ValidationResult result)
        {
            result = new ValidationResult();

            if (rack == null)
            {
                result.AddFailReason(RackValidationField.Rack, "Rack cannot be null.");
                return result.IsValid;
            }

            var depth = rack.Depth;

            if (depth != null && (depth > MAX_RACK_SIZE_CM || depth < 0))
            {
                result.AddFailReason(RackValidationField.Depth,
                    $"Rack Depth must be between 0 and {MAX_RACK_SIZE_CM} cm.");
            }

            return result.IsValid;
        }

        public static bool IsRackWidthValid(Rack rack, out ValidationResult result)
        {
            result = new ValidationResult();

            if (rack == null)
            {
                result.AddFailReason(RackValidationField.Rack, "Rack cannot be null.");
                return result.IsValid;
            }

            var width = rack.Width;

            if (width != null && (width > MAX_RACK_SIZE_CM || width < 0))
            {
                result.AddFailReason(RackValidationField.Width,
                    $"Rack Width must be between 0 and {MAX_RACK_SIZE_CM} cm.");
            }

            return result.IsValid;
        }

        public static bool IsRackUnitCapacityValid(Rack rack, out ValidationResult result)
        {
            result = new ValidationResult();

            if (rack == null)
            {
                result.AddFailReason(RackValidationField.Rack, "Rack cannot be null.");
                return result.IsValid;
            }

            if (rack.Capacity.IsEmpty)
            {
                result.AddFailReason(RackValidationField.RackUnits, "Rack Units cannot be empty.");
                return result.IsValid;
            }

            var rackUnits = rack.Capacity.MaximumRackCapacity;

            if (rackUnits > MAX_RACK_CAPACITY_U || rackUnits < 1)
            {
                result.AddFailReason(RackValidationField.RackUnits,
                    $"Rack Units must be between 1 and {MAX_RACK_CAPACITY_U}.");
            }

            return result.IsValid;
        }

        public static bool IsRackPowerCapacityValid(Rack rack, out ValidationResult result)
        {
            result = new ValidationResult();

            if (rack == null)
            {
                result.AddFailReason(RackValidationField.Rack, "Rack cannot be null.");
                return result.IsValid;
            }

            var powerCapacity = rack.Capacity.MaximumPowerCapacity;

            if (powerCapacity < 0)
            {
                result.AddFailReason(RackValidationField.PowerCapacity,
                    "Rack Power Capacity cannot be negative.");
            }

            return result.IsValid;
        }

        #endregion
    }
}