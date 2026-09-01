namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using Newtonsoft.Json;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    // [GenerateExposers]
    // [SdmDomStorage("(slc)asset_management")]
    public sealed class PowerPort : SdmObject<PowerPort>, IEquatable<PowerPort>, IEntityTracking, IPort
	{
        [JsonIgnore]
        private PowerPortInfo _powerPortInfo;
        [JsonIgnore]
        private AssetRelation _assetFk;
        [JsonIgnore]
        private bool _isNew = true;

        public PowerPort()
        {
        }

        #region Properties

        public PowerPortInfo PowerPortInfo => _powerPortInfo ?? (_powerPortInfo = new PowerPortInfo());

        IPortInfo IPort.PortInfo => PowerPortInfo;

        public SdmObjectReference<Asset> Asset
        {
            get => (_assetFk ?? (_assetFk = new AssetRelation())).Asset;
            set => (_assetFk ?? (_assetFk = new AssetRelation())).Asset = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        public bool Changed => _powerPortInfo?.Changed == true ||
        _assetFk?.Changed == true;

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

        public bool Equals(PowerPort other)
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
                Equals(PowerPortInfo, other.PowerPortInfo) &&
                Equals(Asset, other.Asset);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PowerPort);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (2 << 12) - 1;
                hash = (hash * 23) + (PowerPortInfo != null ? PowerPortInfo.GetHashCode() : 0);
                hash = (hash * 23) + (Asset != null ? Asset.GetHashCode() : 0);
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
            _powerPortInfo?.ResetChangeTracking();
            _assetFk?.ResetChangeTracking();
        }

        #region Section Tracking

        [JsonIgnore]
        [SdmIgnore]
        internal Guid? AssetFkSectionId { get; set; }

        #endregion

        #region Section Tracking

        [JsonIgnore]
        [SdmIgnore]
        internal Guid? AssetRelationPropertiesSectionId { get; set; }

        #endregion
	}
}