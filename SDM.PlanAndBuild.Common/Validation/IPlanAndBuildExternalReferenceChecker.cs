namespace Skyline.DataMiner.SDM.PlanAndBuild.Validation
{
    using System.Collections.Generic;

    /// <summary>
    /// Optional cross-module reference checker for Plan &amp; Build Job validation.
    /// </summary>
    public interface IPlanAndBuildExternalReferenceChecker
    {
        /// <summary>
        /// Gets the Facility Management location identifiers that exist. Return <c>null</c> to skip this check.
        /// </summary>
        IReadOnlyCollection<System.Guid> GetExistingLocationIds(IReadOnlyCollection<System.Guid> locationIds);

        /// <summary>
        /// Gets the Asset Management Asset identifiers that exist. Return <c>null</c> to skip this check.
        /// </summary>
        IReadOnlyCollection<string> GetExistingAssetIds(IReadOnlyCollection<string> assetIds);

        /// <summary>
        /// Gets the Asset Management Connection identifiers that exist. Return <c>null</c> to skip this check.
        /// </summary>
        IReadOnlyCollection<string> GetExistingConnectionIds(IReadOnlyCollection<string> connectionIds);

        /// <summary>
        /// Gets the Asset Management CableType identifiers that exist. Return <c>null</c> to skip this check.
        /// </summary>
        IReadOnlyCollection<string> GetExistingCableTypeIds(IReadOnlyCollection<string> cableTypeIds);
    }
}
