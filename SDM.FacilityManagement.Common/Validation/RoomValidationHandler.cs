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
    }
}
