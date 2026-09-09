namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    using System;

    using Newtonsoft.Json;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public sealed class RowRelation : ChangeTrackingBase, IEquatable<RowRelation>, ISectionTrackable, ISectionEmptyState
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }

        [JsonIgnore]
        [SdmIgnore]
        public bool IsEmpty =>
            !Row.HasValue();

        public SdmObjectReference<Row> Row
        {
            get => RowField.Value;
            set => RowField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<SdmObjectReference<Row>> RowField => FieldHandler.GetOrCreateField(
            nameof(Row),
            () => new ChangeTrackingField<SdmObjectReference<Row>>(default));

        public static bool operator ==(RowRelation left, RowRelation right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left is null || right is null)
            {
                return false;
            }

            return left.Equals(right);
        }

        public static bool operator !=(RowRelation left, RowRelation right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as RowRelation);
        }

        public bool Equals(RowRelation other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Row == other.Row;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + (Row != null ? Row.GetHashCode() : 0);
                return hash;
            }
        }
    }
}