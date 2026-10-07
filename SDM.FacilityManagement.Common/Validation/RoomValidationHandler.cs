namespace Skyline.DataMiner.SDM.FacilityManagement.Validation
{
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Static validation handler for Room business rules.
    /// Contains pure validation logic without data access.
    /// </summary>
    public static class RoomValidationHandler
    {
        public enum RoomValidationField
        {
            RoomId,
            Name,
            FloorId,
            Width,
            Depth,
        }

        /// <summary>
        /// Validates that the Room id is not empty or whitespace.
        /// </summary>
        public static bool IsRoomIdValid(Room entity, out ValidationResult result)
        {
            result = new ValidationResult();

            if (entity == null || string.IsNullOrWhiteSpace(entity.RoomId))
            {
                result.AddFailReason(RoomValidationField.RoomId, "Room Id cannot be empty or whitespace.");
            }

            return result.IsValid;
        }

        public static bool IsRoomNameValid(Room entity, out ValidationResult result)
        {
            result = new ValidationResult();

            if (entity == null || string.IsNullOrWhiteSpace(entity.Name))
            {
                result.AddFailReason(RoomValidationField.Name, "Room Name cannot be empty or whitespace.");
            }

            return result.IsValid;
        }

        /// <summary>
        /// Validates that, when defined, the Room width is not negative.
        /// </summary>
        public static bool IsRoomWidthValid(Room entity, out ValidationResult result)
        {
            if (entity?.Width == null)
            {
                result = new ValidationResult();
                return true;
            }

            return NumericValidators.ValidateNonNegative(entity.Width.Value, RoomValidationField.Width, out result);
        }

        /// <summary>
        /// Validates that, when defined, the Room depth is not negative.
        /// </summary>
        public static bool IsRoomDepthValid(Room entity, out ValidationResult result)
        {
            if (entity?.Depth == null)
            {
                result = new ValidationResult();
                return true;
            }

            return NumericValidators.ValidateNonNegative(entity.Depth.Value, RoomValidationField.Depth, out result);
        }

        /// <summary>
        /// Validates the Room name together with its dimensions.
        /// </summary>
        internal static bool IsRoomNameAndDimensionsValid(Room entity, out ValidationResult result)
        {
            result = new ValidationResult();

            if (!IsRoomNameValid(entity, out var nameResult))
            {
                result.AddFailuresFrom(nameResult);
            }

            if (!IsRoomWidthValid(entity, out var widthResult))
            {
                result.AddFailuresFrom(widthResult);
            }

            if (!IsRoomDepthValid(entity, out var depthResult))
            {
                result.AddFailuresFrom(depthResult);
            }

            return result.IsValid;
        }
    }
}
