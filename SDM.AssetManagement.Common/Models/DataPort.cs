namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;

    using Newtonsoft.Json;

    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    //[GenerateExposers]
    //[SdmDomStorage("(slc)asset_management")]
    public sealed class DataPort : SdmObject<DataPort>, IEquatable<DataPort>, IEntityTracking
    {
        [JsonIgnore]
        private DataPortInfo _dataPortInfo;
        [JsonIgnore]
        private AssetRelation _assetFk;
        [JsonIgnore]
        private AddressInfo _addressInfo;
        [JsonIgnore]
        private PrimaryPortRelation _primaryPortRelation;
        [JsonIgnore]
        private bool _isNew = true;

        public DataPort()
        { 
        }

        #region Properties

        public DataPortInfo DataPortInfo
        {
            get => _dataPortInfo ?? (_dataPortInfo = new DataPortInfo());
            set => _dataPortInfo = value ?? new DataPortInfo();
        }

        public SdmObjectReference<Asset> Asset
        {
            get => (_assetFk ?? (_assetFk = new AssetRelation())).Asset;
            set => (_assetFk ?? (_assetFk = new AssetRelation())).Asset = value;
        }

        public AddressInfo AddressInfo
        {
            get => _addressInfo ?? (_addressInfo = new AddressInfo());
            set => _addressInfo = value ?? new AddressInfo();
        }

        public PrimaryPortRelation PrimaryPortRelation
        {
            get => _primaryPortRelation ?? (_primaryPortRelation = new PrimaryPortRelation());
            set => _primaryPortRelation = value ?? new PrimaryPortRelation();
        }

        public bool Changed => _dataPortInfo?.Changed == true ||
        _assetFk?.Changed == true ||
        _addressInfo?.Changed == true ||
        _primaryPortRelation?.Changed == true;

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<SdmObjectReference<Asset>> AssetField =>
            (_assetFk ?? (_assetFk = new AssetRelation())).AssetField;

        [JsonIgnore]
        [SdmIgnore]
        public bool IsNew => _isNew;


        [JsonIgnore]
        [SdmIgnore]
        internal bool IsNewInternal
        {
            get => _isNew;
            set => _isNew = value;
        }

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
                Equals(Asset, other.Asset) &&
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
                hash = (hash * 23) + (Asset != null ? Asset.GetHashCode() : 0);
                hash = (hash * 23) + (AddressInfo != null ? AddressInfo.GetHashCode() : 0);
                hash = (hash * 23) + (PrimaryPortRelation != null ? PrimaryPortRelation.GetHashCode() : 0);
                return hash;
            }
        }

        #endregion

        /// <summary>
        /// Resets the change tracking state for all related properties to indicate that no changes have been made since
        /// the last reset.
        /// </summary>
        /// <remarks>Call this method after persisting or discarding changes to clear the modified state
        /// of the object and its tracked properties. This is typically used in scenarios where change tracking is
        /// required to detect modifications for persistence or synchronization purposes.</remarks>
        public void ResetChangeTracking()
        {
            _dataPortInfo?.ResetChangeTracking();
            _assetFk?.ResetChangeTracking();
            _addressInfo?.ResetChangeTracking();
            _primaryPortRelation?.ResetChangeTracking();
        }
    }
}