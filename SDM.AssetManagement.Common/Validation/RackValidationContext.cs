namespace Skyline.DataMiner.SDM.FacilityManagement.Validation
{
    using System.Collections.Generic;

    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;

    /// <summary>
    /// Context for bulk rack validation.
    /// Holds validation batch data to avoid redundant database queries.
    /// </summary>
    public class RackValidationContext
    {
        /// <summary>
        /// Assets being validated in this batch (excluded from occupancy checks).
        /// </summary>
        public List<Asset> AssetsBeingValidated { get; set; } = new List<Asset>();

        /// <summary>
        /// Racks loaded for this validation batch (cached to avoid multiple queries).
        /// Key: Rack Identifier
        /// </summary>
        public Dictionary<string, Rack> LoadedRacks { get; set; } = new Dictionary<string, Rack>();

        /// <summary>
        /// Existing assets in each rack (excluding AssetsBeingValidated).
        /// Key: Rack Identifier
        /// </summary>
        public Dictionary<string, List<Asset>> ExistingAssetsInRacks { get; set; } = new Dictionary<string, List<Asset>>();
    }
}