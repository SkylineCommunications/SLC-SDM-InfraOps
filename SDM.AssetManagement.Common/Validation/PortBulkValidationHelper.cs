namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Shared bulk-validation logic for asset-scoped ports (DataPort/PowerPort), which only differ
    /// by port type, entity-loader call, and collection validator.
    /// </summary>
    internal static class PortBulkValidationHelper
    {
        /// <summary>
        /// Validates multiple ports for a single asset (bulk optimization). All ports must belong to the specified asset.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when the asset is null.</exception>
        /// <exception cref="ArgumentException">Thrown when ports don't belong to the asset.</exception>
        public static Dictionary<string, ValidationResult> ValidatePortsForAsset<TPort>(
            List<TPort> portsToValidate,
            Asset asset,
            string portTypeName,
            Func<TPort, string> getIdentifier,
            Func<TPort, string> getAssetIdentifier,
            Func<Asset, IEnumerable<TPort>> loadExistingPorts,
            Func<List<TPort>, ValidationResult> validateCollection)
        {
            if (asset == null)
            {
                throw new ArgumentNullException(nameof(asset));
            }

            if (portsToValidate == null || !portsToValidate.Any())
            {
                return new Dictionary<string, ValidationResult>();
            }

            // Defensive check: ensure all ports belong to this asset.
            var mismatchedPorts = portsToValidate
                .Where(p => getAssetIdentifier(p) != asset.Identifier)
                .ToList();

            if (mismatchedPorts.Any())
            {
                throw new ArgumentException(
                    $"All {portTypeName} must belong to Asset '{asset.Identifier}'. Found {mismatchedPorts.Count} port(s) belonging to different assets. ",
                    nameof(portsToValidate));
            }

            var results = portsToValidate.ToDictionary(getIdentifier, p => new ValidationResult());

            var validatedIds = portsToValidate.Select(getIdentifier).ToList();
            var existingPorts = loadExistingPorts(asset)
                .Where(p => !validatedIds.Contains(getIdentifier(p)))
                .ToList();

            var allPorts = existingPorts.Concat(portsToValidate).ToList();

            var collectionResult = validateCollection(allPorts);

            if (!collectionResult.IsValid)
            {
                foreach (var port in portsToValidate)
                {
                    results[getIdentifier(port)].AddFailuresFrom(collectionResult);
                }
            }

            return results;
        }
    }
}
