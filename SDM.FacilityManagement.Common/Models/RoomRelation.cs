namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    using System;

    using Newtonsoft.Json;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public sealed class RoomRelation : ChangeTrackingBase, IEquatable<RoomRelation>, ISectionTrackable, ISectionEmptyState
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }

        [JsonIgnore]
        [SdmIgnore]
        public bool IsEmpty =>
            !Room.HasValue();

        public SdmObjectReference<Room> Room
        {
            get => RoomField.Value;
            set => RoomField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<SdmObjectReference<Room>> RoomField => FieldHandler.GetOrCreateField(
            nameof(Room),
            () => new ChangeTrackingField<SdmObjectReference<Room>>(default));

        public static bool operator ==(RoomRelation left, RoomRelation right)
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

        public static bool operator !=(RoomRelation left, RoomRelation right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as RoomRelation);
        }

        public bool Equals(RoomRelation other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Room == other.Room;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + (Room != null ? Room.GetHashCode() : 0);
                return hash;
            }
        }
    } 
}