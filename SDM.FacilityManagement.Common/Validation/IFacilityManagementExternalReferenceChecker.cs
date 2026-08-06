namespace Skyline.DataMiner.SDM.FacilityManagement.Validation
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Optional cross-module reference checker for Facility Management delete guards.
    /// </summary>
    public interface IFacilityManagementExternalReferenceChecker
    {
        /// <summary>
        /// Gets the Facility Management entity identifiers that have assigned assets.
        /// </summary>
        /// <param name="entityType">The Facility Management entity type.</param>
        /// <param name="identifiers">The Facility Management entity identifiers to check.</param>
        /// <returns>The identifiers that have one or more assigned assets.</returns>
        IReadOnlyCollection<string> GetIdentifiersWithAssets(FacilityManagementEntityType entityType, IReadOnlyCollection<string> identifiers);

        /// <summary>
        /// Gets the Person ids that exist in People &amp; Organizations.
        /// </summary>
        /// <param name="personIds">The Person ids to check.</param>
        /// <returns>The Person ids that exist.</returns>
        IReadOnlyCollection<Guid> GetExistingPersonIds(IReadOnlyCollection<Guid> personIds);

        /// <summary>
        /// Gets the Team ids that exist in People &amp; Organizations.
        /// </summary>
        /// <param name="teamIds">The Team ids to check.</param>
        /// <returns>The Team ids that exist.</returns>
        IReadOnlyCollection<Guid> GetExistingTeamIds(IReadOnlyCollection<Guid> teamIds);

        /// <summary>
        /// Gets the Resource ids that exist in the consuming resource system.
        /// </summary>
        /// <param name="resourceIds">The Resource ids to check.</param>
        /// <returns>The Resource ids that exist.</returns>
        IReadOnlyCollection<Guid> GetExistingResourceIds(IReadOnlyCollection<Guid> resourceIds);
    }
}
