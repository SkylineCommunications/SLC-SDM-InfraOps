namespace Skyline.DataMiner.Utils.InfraOps.Common.Fields
{
    using System;

    /// <summary>
    /// Marks an object that represents a single DOM <c>Section</c> instance and can remember the
    /// <c>SectionID</c> it was loaded with, so the same identity can be reused when the section is
    /// rebuilt during an update.
    /// </summary>
    /// <remarks>
    /// Implemented (explicitly, to keep it off the public API) by each section-detail model and by
    /// each repeating-section element type. The generated <c>FromInstance()</c> captures the original
    /// <c>SectionID</c>; the generated <c>ToInstance()</c> reuses it when present. Without a stable
    /// <c>SectionID</c>, <c>ToInstance()</c> mints a brand-new random <c>SectionID</c> (via
    /// <c>new Section(sectionDefinitionId)</c>) on every call, which DOM's per-status read-only field
    /// check interprets as the section's identity having changed - triggering false-positive
    /// <c>ReadOnlyFieldsChangedForCurrentStatus</c> failures even when field values are unchanged.
    /// </remarks>
    internal interface ISectionTrackable
    {
        /// <summary>
        /// Gets or sets the original DOM <c>SectionID</c> this object was loaded with, or
        /// <see langword="null"/> if it has not yet been persisted (new object, section not yet created).
        /// </summary>
        Guid? SectionId { get; set; }
    }
}
