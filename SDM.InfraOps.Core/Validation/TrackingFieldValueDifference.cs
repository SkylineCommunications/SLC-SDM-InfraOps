namespace Skyline.DataMiner.Utils.InfraOps.Common.Fields
{
    public class TrackingFieldValueDifference
    {
        public string FieldName {  get; internal set; }

        /// <summary>
        /// Gets the section the field belongs to (for example "Location" or "DestinationLocation"),
        /// or <c>null</c> when the field lives on the root object. Nested sections are dot-separated.
        /// </summary>
        public string Section { get; internal set; }

        /// <summary>
        /// Gets the field name prefixed by its section; unique within one <c>GetChanges()</c> result.
        /// </summary>
        public string QualifiedName => string.IsNullOrEmpty(Section) ? FieldName : Section + "." + FieldName;

        public object OldValue { get; internal set; }

        public object NewValue { get; internal set; }
    }
}