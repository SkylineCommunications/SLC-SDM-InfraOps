namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using Newtonsoft.Json;
    using SharedMappers.DomIds;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public sealed class AssetLocation : ChangeTrackingBase, IEquatable<AssetLocation>, ISectionTrackable, ISectionEmptyState
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }
        [JsonIgnore]
        [SdmIgnore]
        public bool IsEmpty => !ParentAsset.HasValue() &&
            HolderNumber == default &&
            !RackId.HasValue() &&
            RackPosition == default &&
            Side == default &&
            DeskId == Guid.Empty &&
            !ContainerId.HasValue() &&
            !RoomId.HasValue();

        public SdmObjectReference<Asset> ParentAsset
        {
            get => ParentAssetField.Value;
            set => ParentAssetField.Value = value;
        }

        public long? HolderNumber
        {
            get => HolderNumberField.Value;
            set => HolderNumberField.Value = value;
        }

        public SdmObjectReference<Rack> RackId
        {
            get => RackIdField.Value;
            set => RackIdField.Value = value;
        }

        public long? RackPosition
        {
            get => RackPositionField.Value;
            set => RackPositionField.Value = value;
        }

        public SlcAsset_Management.Enums.SideEnum? Side
        {
            get => SideField.Value;
            set => SideField.Value = value;
        }

        public Guid DeskId
        {
            get => DeskIdField.Value;
            set => DeskIdField.Value = value;
        }

        public SdmObjectReference<FacilityManagement.Models.Facility> ContainerId
        {
            get => ContainerIdField.Value;
            set => ContainerIdField.Value = value;
        }

        public SdmObjectReference<FacilityManagement.Models.Room> RoomId
        {
            get => RoomIdField.Value;
            set => RoomIdField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<SdmObjectReference<Asset>> ParentAssetField => FieldHandler.GetOrCreateField(
            nameof(ParentAsset),
            () => new ChangeTrackingField<SdmObjectReference<Asset>>(default));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<long?> HolderNumberField => FieldHandler.GetOrCreateField(
            nameof(HolderNumber),
            () => new ChangeTrackingField<long?>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<SdmObjectReference<Rack>> RackIdField => FieldHandler.GetOrCreateField(
            nameof(RackId),
            () => new ChangeTrackingField<SdmObjectReference<Rack>>(default));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<long?> RackPositionField => FieldHandler.GetOrCreateField(
            nameof(RackPosition),
            () => new ChangeTrackingField<long?>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<SlcAsset_Management.Enums.SideEnum?> SideField => FieldHandler.GetOrCreateField(
            nameof(Side),
            () => new ChangeTrackingField<SlcAsset_Management.Enums.SideEnum?>(default));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<Guid> DeskIdField => FieldHandler.GetOrCreateField(
            nameof(DeskId),
            () => new ChangeTrackingField<Guid>(Guid.Empty));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<SdmObjectReference<Facility>> ContainerIdField => FieldHandler.GetOrCreateField(
            nameof(ContainerId),
            () => new ChangeTrackingField<SdmObjectReference<Facility>>(default));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<SdmObjectReference<FacilityManagement.Models.Room>> RoomIdField => FieldHandler.GetOrCreateField(
            nameof(RoomId),
            () => new ChangeTrackingField<SdmObjectReference<FacilityManagement.Models.Room>>(default));

        public static bool operator ==(AssetLocation left, AssetLocation right)
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

        public static bool operator !=(AssetLocation left, AssetLocation right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as AssetLocation);
        }

        public bool Equals(AssetLocation other)
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
                ParentAsset == other.ParentAsset &&
                HolderNumber == other.HolderNumber &&
                RackId == other.RackId &&
                RackPosition == other.RackPosition &&
                Side == other.Side &&
                DeskId.Equals(other.DeskId) &&
                ContainerId == other.ContainerId &&
                RoomId == other.RoomId;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + (ParentAsset != null ? ParentAsset.GetHashCode() : 0);
                hash = (hash * 23) + HolderNumber.GetHashCode();
                hash = (hash * 23) + (RackId != null ? RackId.GetHashCode() : 0);
                hash = (hash * 23) + RackPosition.GetHashCode();
                hash = (hash * 23) + Side.GetHashCode();
                hash = (hash * 23) + DeskId.GetHashCode();
                hash = (hash * 23) + (ContainerId != null ? ContainerId.GetHashCode() : 0);
                hash = (hash * 23) + (RoomId != null ? RoomId.GetHashCode() : 0);
                return hash;
            }
        }
    }
}