namespace Skyline.DataMiner.SDM.FacilityManagement.Validation
{
    using System.Collections.Generic;

    using Skyline.DataMiner.SDM.FacilityManagement.Models;

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
            Side,
            RackSpacePosition,
            RackSpaceOccupied,
        }

        #region Dimensions Validation

        /// <summary>
        /// Validates rack height is within valid range.
        /// </summary>
        public static bool IsRackHeightValid(Rack rack, out ValidationResult result)
        {
            result = new ValidationResult();

            if (rack == null)
            {
                result.AddFailReason(RackValidationField.Rack, "Rack cannot be null.");
                return result.IsValid;
            }

            var height = rack.RackInfo?.Height;

            if (height == null)
            {
                return result.IsValid; // Null is valid (optional field)
            }

            if (height > MAX_RACK_HEIGHT_CM || height < 0)
            {
                result.AddFailReason(RackValidationField.Height,
                    $"Rack Height must be between 0 and {MAX_RACK_HEIGHT_CM} cm.");
                return result.IsValid;
            }

            return result.IsValid;
        }

        /// <summary>
        /// Validates rack depth is within valid range.
        /// </summary>
        public static bool IsRackDepthValid(Rack rack, out ValidationResult result)
        {
            result = new ValidationResult();

            if (rack == null)
            {
                result.AddFailReason(RackValidationField.Rack, "Rack cannot be null.");
                return result.IsValid;
            }

            var depth = rack.RackInfo?.Depth;

            if (depth == null)
            {
                return result.IsValid; // Null is valid (optional field)
            }

            if (depth > MAX_RACK_SIZE_CM || depth < 0)
            {
                result.AddFailReason(RackValidationField.Depth,
                    $"Rack Depth must be between 0 and {MAX_RACK_SIZE_CM} cm.");
                return result.IsValid;
            }

            return result.IsValid;
        }

        /// <summary>
        /// Validates rack width is within valid range.
        /// </summary>
        public static bool IsRackWidthValid(Rack rack, out ValidationResult result)
        {
            result = new ValidationResult();

            if (rack == null)
            {
                result.AddFailReason(RackValidationField.Rack, "Rack cannot be null.");
                return result.IsValid;
            }

            var width = rack.RackInfo?.Width;

            if (width == null)
            {
                return result.IsValid; // Null is valid (optional field)
            }

            if (width > MAX_RACK_SIZE_CM || width < 0)
            {
                result.AddFailReason(RackValidationField.Width,
                    $"Rack Width must be between 0 and {MAX_RACK_SIZE_CM} cm.");
                return result.IsValid;
            }

            return result.IsValid;
        }

        /// <summary>
        /// Validates rack unit capacity is within valid range.
        /// </summary>
        public static bool IsRackUnitCapacityValid(Rack rack, out ValidationResult result)
        {
            result = new ValidationResult();

            if (rack == null)
            {
                result.AddFailReason(RackValidationField.Rack, "Rack cannot be null.");
                return result.IsValid;
            }

            var rackUnits = rack.RackInfo?.RackUnits;

            if (rackUnits == null)
            {
                result.AddFailReason(RackValidationField.RackUnits, "Rack Units cannot be empty.");
                return result.IsValid;
            }

            if (rackUnits > MAX_RACK_CAPACITY_U || rackUnits < 1)
            {
                result.AddFailReason(RackValidationField.RackUnits,
                    $"Rack Units must be between 1 and {MAX_RACK_CAPACITY_U}.");
                return result.IsValid;
            }

            return result.IsValid;
        }

        /// <summary>
        /// Validates rack power capacity is valid.
        /// </summary>
        public static bool IsRackPowerCapacityValid(Rack rack, out ValidationResult result)
        {
            result = new ValidationResult();

            if (rack == null)
            {
                result.AddFailReason(RackValidationField.Rack, "Rack cannot be null.");
                return result.IsValid;
            }

            var powerCapacity = rack.RackInfo?.PowerCapacity;

            if (powerCapacity == null)
            {
                return result.IsValid; // Null is valid (optional field)
            }

            if (powerCapacity < 0)
            {
                result.AddFailReason(RackValidationField.PowerCapacity,
                    "Rack Power Capacity cannot be negative.");
                return result.IsValid;
            }

            return result.IsValid;
        }

        #endregion

        #region Rack Space Validation

        /// <summary>
        /// Validates if an asset can fit at the specified position in the rack.
        /// Checks position bounds and space occupation (pure logic, requires asset occupation data).
        /// </summary>
        public static bool IsRackSpaceAvailable(
            Rack rack,
            int assetPosition,
            int assetHeightU,
            Asset currentAsset,
            List<(Asset Asset, int Position, int HeightU, SharedMappers.DomIds.SlcAsset_Management.Enums.SideEnum? Side)> occupiedAssets,
            SharedMappers.DomIds.SlcAsset_Management.Enums.SideEnum assetSide,
            out ValidationResult result)
        {
            result = new ValidationResult();

            if (assetPosition <= 0)
            {
                result.AddFailReason(RackValidationField.RackSpacePosition,
                    "Invalid Position. Position must be greater than 0.");
                return result.IsValid;
            }

            if (assetHeightU <= 0)
            {
                result.AddFailReason(RackValidationField.RackSpacePosition,
                    "Invalid Height. Height (U) must be greater than 0.");
                return result.IsValid;
            }

            // Calculate start and end positions based on rack numbering scheme
            int zeroBasedPosition = assetPosition - 1;
            long assetStartPosition;
            long assetEndPosition;

            if (rack.Position == SharedMappers.DomIds.SlcFacility_Management.Enums.RackpositionenumEnum.Top)
            {
                assetStartPosition = zeroBasedPosition - (assetHeightU - 1);
            }
            else
            {
                assetStartPosition = zeroBasedPosition;
            }

            assetEndPosition = assetStartPosition + assetHeightU;

            // Check if asset fits within rack bounds
            if (assetStartPosition < 0 || assetEndPosition > rack.Capacity.MaximumRackCapacity)
            {
                result.AddFailReason(RackValidationField.RackSpacePosition,
                    $"Invalid Position. Asset extends beyond rack boundaries (Rack has {rack.Capacity.MaximumRackCapacity} units).");
                return result.IsValid;
            }

            // Check for collisions with other assets on the same side
            if (occupiedAssets != null)
            {
                foreach (var occupied in occupiedAssets)
                {
                    // Skip if it's the same asset
                    if (currentAsset != null && occupied.Asset.Identifier == currentAsset.Identifier)
                    {
                        continue;
                    }

                    // Only check collisions on the same side
                    if (occupied.Side != assetSide)
                    {
                        continue;
                    }

                    int occupiedZeroBasedPosition = occupied.Position - 1;
                    long occupiedStartPosition;
                    long occupiedEndPosition;

                    if (rackPosition == SharedMappers.DomIds.SlcFacility_Management.Enums.RackpositionenumEnum.Top)
                    {
                        occupiedStartPosition = occupiedZeroBasedPosition - (occupied.HeightU - 1);
                    }
                    else
                    {
                        occupiedStartPosition = occupiedZeroBasedPosition;
                    }

                    occupiedEndPosition = occupiedStartPosition + occupied.HeightU;

                    // Check for overlap
                    if (assetStartPosition < occupiedEndPosition && assetEndPosition > occupiedStartPosition)
                    {
                        result.AddFailReason(RackValidationField.RackSpaceOccupied,
                            $"Invalid Position. Rack space is already occupied by another asset at position {occupied.Position}.");
                        return result.IsValid;
                    }
                }
            }

            return result.IsValid;
        }

        #endregion
    }
}