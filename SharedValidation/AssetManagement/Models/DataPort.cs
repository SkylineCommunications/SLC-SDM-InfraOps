namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using System.Runtime.Serialization;

    using Newtonsoft.Json;

    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    //[GenerateExposers]
    [SdmDomStorage("(slc)asset_management")]
    public sealed class DataPort : SdmObject<DataPort>, IEquatable<DataPort>
    {
        [JsonIgnore]
        private ChangeTrackingFieldHandler _fieldHandler;

        public DataPort()
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

        #region Properties

        public DataPortInfo DataPortInfo
        {
            get => DataPortInfoField.Value ?? new DataPortInfo();
            set => DataPortInfoField.Value = value;
        }

        public AssetRelation AssetFk
        {
            get => AssetFkField.Value;
            set => AssetFkField.Value = value;
        }

        public AddressInfo AddressInfo
        {
            get => AddressInfoField.Value ?? new AddressInfo();
            set => AddressInfoField.Value = value;
        }

        public PrimaryPortRelation PrimaryPortRelation
        {
            get => PrimaryPortRelationField.Value ?? new PrimaryPortRelation();
            set => PrimaryPortRelationField.Value = value;
        }

        #endregion

        #region Change Tracking Fields

        [JsonIgnore]
        internal IChangeTrackingField<DataPortInfo> DataPortInfoField => FieldHandler.GetOrCreateField(
            nameof(DataPortInfo),
            () => new ChangeTrackingField<DataPortInfo>(new DataPortInfo()));

        [JsonIgnore]
        internal IChangeTrackingField<AssetRelation> AssetFkField => FieldHandler.GetOrCreateField(
            nameof(AssetFk),
            () => new ChangeTrackingField<AssetRelation>(null));

        [JsonIgnore]
        internal IChangeTrackingField<AddressInfo> AddressInfoField => FieldHandler.GetOrCreateField(
            nameof(AddressInfo),
            () => new ChangeTrackingField<AddressInfo>(new AddressInfo()));

        [JsonIgnore]
        internal IChangeTrackingField<PrimaryPortRelation> PrimaryPortRelationField => FieldHandler.GetOrCreateField(
            nameof(PrimaryPortRelation),
            () => new ChangeTrackingField<PrimaryPortRelation>(new PrimaryPortRelation()));

        #endregion

        #region Equality

        public bool Equals(DataPort other)
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
                Equals(DataPortInfo, other.DataPortInfo) &&
                Equals(AssetFk, other.AssetFk) &&
                Equals(AddressInfo, other.AddressInfo) &&
                Equals(PrimaryPortRelation, other.PrimaryPortRelation);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DataPort);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (2 << 12) - 1;
                hash = (hash * 23) + (DataPortInfo != null ? DataPortInfo.GetHashCode() : 0);
                hash = (hash * 23) + (AssetFk != null ? AssetFk.GetHashCode() : 0);
                hash = (hash * 23) + (AddressInfo != null ? AddressInfo.GetHashCode() : 0);
                hash = (hash * 23) + (PrimaryPortRelation != null ? PrimaryPortRelation.GetHashCode() : 0);
                return hash;
            }
        }

        #endregion

        public void ResetChangeTracking()
        {
            FieldHandler?.ApplyChanges();
        }
    }
}