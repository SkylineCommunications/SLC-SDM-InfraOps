namespace Skyline.DataMiner.Utils.InfraOps.Common.Fields
{
    using System.Collections.Generic;
    using System.Linq;

    internal static class SectionChangesExtensions
    {
        /// <summary>
        /// Returns the changes of a section object, stamped with the section name.
        /// A null section yields no changes.
        /// </summary>
        internal static IEnumerable<TrackingFieldValueDifference> GetSectionChanges(this IChangeTracking section, string sectionName)
        {
            if (section == null)
            {
                return Enumerable.Empty<TrackingFieldValueDifference>();
            }

            return section.GetChanges()
                .Select(change => new TrackingFieldValueDifference
                {
                    FieldName = change.FieldName,
                    Section = string.IsNullOrEmpty(change.Section) ? sectionName : sectionName + "." + change.Section,
                    OldValue = change.OldValue,
                    NewValue = change.NewValue,
                })
                .ToList();
        }
    }
}
