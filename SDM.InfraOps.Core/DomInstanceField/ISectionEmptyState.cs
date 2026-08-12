namespace Skyline.DataMiner.Utils.InfraOps.Common.Fields
{
    /// <summary>
    /// Reports whether every one of a section's own properties is currently at its default value.
    /// </summary>
    /// <remarks>
    /// Implemented by each section-detail model so both the generated <c>ToInstance()</c> write-gates and
    /// consumer code can share a single definition of "does this section actually hold any data" instead of
    /// each re-implementing their own ad hoc single-field check (which drifts out of sync when a section gains
    /// fields - see e.g. the historical <c>ElementLink</c> gate, which only ever checked <c>Element</c> and
    /// ignored <c>ConnectionType</c>/<c>ConnectionId</c>). <see cref="IsEmpty"/> is <see langword="true"/> only
    /// when every property on the section is unchanged from its type's default value. The generated
    /// <c>ToInstance()</c> omits an empty section entirely (and drops any stale DOM entry), which keeps a
    /// section's <c>SectionID</c> stable across updates without persisting all-default sections.
    /// </remarks>
    internal interface ISectionEmptyState
    {
        /// <summary>
        /// Gets a value indicating whether every property of this section is at its default value.
        /// </summary>
        bool IsEmpty { get; }
    }
}
