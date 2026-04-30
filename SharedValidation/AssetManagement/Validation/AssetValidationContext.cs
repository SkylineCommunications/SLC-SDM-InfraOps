namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.SDM.AssetManagement.Models;

    /// <summary>
    /// Context for bulk asset validation.
    /// Holds validation batch data to avoid redundant database queries and check for batch conflicts.
    /// </summary>
    public class AssetValidationContext
    {
        /// <summary>
        /// Assets being validated in this batch (excluded from database uniqueness checks).
        /// </summary>
        public List<Asset> AssetsBeingValidated { get; set; } = new List<Asset>();

        /// <summary>
        /// Gets list of identifiers for assets being validated.
        /// </summary>
        public List<string> ValidatedAssetIdentifiers =>
            AssetsBeingValidated.Select(a => a.Identifier).ToList();

        /// <summary>
        /// Loaded parent assets (cached to avoid multiple queries).
        /// Key: Parent Asset Identifier
        /// </summary>
        public Dictionary<string, Asset> LoadedParentAssets { get; set; } =
            new Dictionary<string, Asset>();

        /// <summary>
        /// Existing child assets for each parent (excluding AssetsBeingValidated).
        /// Key: Parent Asset Identifier
        /// Value: List of child assets with their holder numbers
        /// </summary>
        public Dictionary<string, List<(Asset Asset, long HolderNumber)>> ExistingChildAssetsInParents { get; set; } =
            new Dictionary<string, List<(Asset, long)>>();

        /// <summary>
        /// Existing child assets for each destination parent (excluding AssetsBeingValidated).
        /// Key: Destination Parent Asset Identifier
        /// Value: List of child assets with their destination holder numbers
        /// </summary>
        public Dictionary<string, List<(Asset Asset, long HolderNumber)>> ExistingChildAssetsInDestinationParents { get; set; } =
            new Dictionary<string, List<(Asset, long)>>();
    }
}