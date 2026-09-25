namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using Newtonsoft.Json;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;
    public sealed class InfraopsReservationBounderies : ChangeTrackingBase, IEquatable<InfraopsReservationBounderies>, ISectionTrackable, ISectionEmptyState
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }
        [JsonIgnore]
        [SdmIgnore]
        public bool IsEmpty => LowerBound == default &&
            UpperBound == default;

        public long? LowerBound
        {
            get => LowerBoundField.Value;
            set => LowerBoundField.Value = value;
        }

        public long? UpperBound
        {
            get => UpperBoundField.Value;
            set => UpperBoundField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<long?> LowerBoundField => FieldHandler.GetOrCreateField(
            nameof(LowerBound),
            () => new ChangeTrackingField<long?>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<long?> UpperBoundField => FieldHandler.GetOrCreateField(
            nameof(UpperBound),
            () => new ChangeTrackingField<long?>(null));

        public static bool operator ==(InfraopsReservationBounderies left, InfraopsReservationBounderies right)
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

        public static bool operator !=(InfraopsReservationBounderies left, InfraopsReservationBounderies right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as InfraopsReservationBounderies);
        }

        public bool Equals(InfraopsReservationBounderies other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return
                LowerBound == other.LowerBound &&
                UpperBound == other.UpperBound;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + LowerBound.GetHashCode();
                hash = (hash * 23) + UpperBound.GetHashCode();
                return hash;
            }
        }
    }
}