namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using Newtonsoft.Json;
    using SharedMappers.DomIds;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public class AssetLocation : ChangeTrackingBase, ISectionTrackable, ISectionEmptyState
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }
        [JsonIgnore]
        [SdmIgnore]
        public bool IsEmpty => ParentAsset == default &&
            HolderNumber == default &&
            RackId == default &&
            RackPosition == default &&
            Side == default &&
            DeskId == default &&
            ContainerId == default &&
            RoomId == default;

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
    }
}