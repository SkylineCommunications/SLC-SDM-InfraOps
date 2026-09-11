namespace Skyline.DataMiner.Utils.InfraOps.Common.Fields
{
    public class TrackingFieldValueDifference
    {
        public string FieldName {  get; internal set; }

        public object OldValue { get; internal set; }

        public object NewValue { get; internal set; }
    }
}