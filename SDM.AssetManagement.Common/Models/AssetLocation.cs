namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;
    using System.Security.Cryptography;

    using Newtonsoft.Json;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    using static SharedMappers.DomIds.SlcFacility_Management.Sections;

    public class AssetLocation
    {
        [JsonIgnore]
        private ChangeTrackingFieldHandler _fieldHandler;

        public AssetLocation()
        {
            _fieldHandler = new ChangeTrackingFieldHandler();
        }

        [JsonIgnore]
        private ChangeTrackingFieldHandler FieldHandler
        {
            get
            {
                if (_fieldHandler == null)
                {
                    _fieldHandler = new ChangeTrackingFieldHandler();
                }
                return _fieldHandler;
            }
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            ResetChangeTracking();
        }

        #region Public Properties

        public SdmObjectReference<Asset> ParentAsset
        {
            get => ParentAssetField.Value;
            set => ParentAssetField.Value = value;
        }

        public long HolderNumber
        {
            get => HolderNumberField.Value;
            set => HolderNumberField.Value = value;
        }

        public Guid RackId
        {
            get => RackIdField.Value;
            set => RackIdField.Value = value;
        }

        public long RackPosition
        {
            get => RackPositionField.Value;
            set => RackPositionField.Value = value;
        }

        public SlcAsset_Management.Enums.SideEnum Side
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

        public Guid RoomId
        {
            get => RoomIdField.Value;
            set => RoomIdField.Value = value;
        }

        public long PowerSupplyRackPosition
        {
            get => PowerSupplyRackPositionField.Value;
            set => PowerSupplyRackPositionField.Value = value;
        }

        #endregion

        #region Internal Tracking Fields

        [JsonIgnore]
        internal IChangeTrackingField<SdmObjectReference<Asset>> ParentAssetField => FieldHandler.GetOrCreateField(
            nameof(ParentAsset),
            () => new ChangeTrackingField<SdmObjectReference<Asset>>(default));

        [JsonIgnore]
        internal IChangeTrackingField<long> HolderNumberField => FieldHandler.GetOrCreateField(
            nameof(HolderNumber),
            () => new ChangeTrackingField<long>(0));

        [JsonIgnore]
        internal IChangeTrackingField<Guid> RackIdField => FieldHandler.GetOrCreateField(
            nameof(RackId),
            () => new ChangeTrackingField<Guid>(default));

        [JsonIgnore]
        internal IChangeTrackingField<long> RackPositionField => FieldHandler.GetOrCreateField(
            nameof(RackPosition),
            () => new ChangeTrackingField<long>(0));

        [JsonIgnore]
        internal IChangeTrackingField<SlcAsset_Management.Enums.SideEnum> SideField => FieldHandler.GetOrCreateField(
            nameof(Side),
            () => new ChangeTrackingField<SlcAsset_Management.Enums.SideEnum>(default));

        [JsonIgnore]
        internal IChangeTrackingField<Guid> DeskIdField => FieldHandler.GetOrCreateField(
            nameof(DeskId),
            () => new ChangeTrackingField<Guid>(default));

        [JsonIgnore]
        internal IChangeTrackingField<SdmObjectReference<FacilityManagement.Models.Facility>> ContainerIdField => FieldHandler.GetOrCreateField(
            nameof(ContainerId),
            () => new ChangeTrackingField<SdmObjectReference<FacilityManagement.Models.Facility>>(default));

        [JsonIgnore]
        internal IChangeTrackingField<Guid> RoomIdField => FieldHandler.GetOrCreateField(
            nameof(RoomId),
            () => new ChangeTrackingField<Guid>(default));

        [JsonIgnore]
        internal IChangeTrackingField<long> PowerSupplyRackPositionField => FieldHandler.GetOrCreateField(
        nameof(PowerSupplyRackPosition),
        () => new ChangeTrackingField<long>(0));

        #endregion

        public void ResetChangeTracking()
        {
            FieldHandler?.ApplyChanges();
        }
    }
}