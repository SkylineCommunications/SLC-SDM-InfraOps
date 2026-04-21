namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Runtime.Serialization;

    using Newtonsoft.Json;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Authentication.UserIdUtil;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public class Asset : SdmObject<Asset>
    {
        [JsonIgnore]
        private ChangeTrackingFieldHandler _fieldHandler;

        public Asset()
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

        [JsonIgnore]
        public Guid Id { get; set; }

        #region Info Properties

        public string Name
        {
            get => NameField.Value;
            set => NameField.Value = value;
        }

        public string AssetID
        {
            get => AssetIDField.Value;
            set => AssetIDField.Value = value;
        }

        public SdmObjectReference<AssetClass> AssetClassId
        {
            get => AssetClassIdField.Value;
            set => AssetClassIdField.Value = value;
        }

        public string SerialNumber
        {
            get => SerialNumberField.Value;
            set => SerialNumberField.Value = value;
        }

        public string Description
        {
            get => DescriptionField.Value;
            set => DescriptionField.Value = value;
        }

        #endregion

        #region Network Properties

        public string PrimaryIPv4Address
        {
            get => PrimaryIPv4AddressField.Value;
            set => PrimaryIPv4AddressField.Value = value;
        }

        public string PrimaryIPv6Address
        {
            get => PrimaryIPv6AddressField.Value;
            set => PrimaryIPv6AddressField.Value = value;
        }

        public string PrimaryMacAddress
        {
            get => PrimaryMacAddressField.Value;
            set => PrimaryMacAddressField.Value = value;
        }

        #endregion

        #region Location Properties

        public AssetLocation Location
        {
            get => LocationField.Value ?? new AssetLocation();
            set => LocationField.Value = value;
        }

        public AssetLocation DestinationLocation
        {
            get => DestinationLocationField.Value ?? new AssetLocation();
            set => DestinationLocationField.Value = value;
        }

        #endregion

        #region Lifecycle Properties

        public Guid InstallationUserId
        {
            get => InstallationUserIdField.Value;
            set => InstallationUserIdField.Value = value;
        }

        public DateTime? InstallationDate
        {
            get => InstallationDateField.Value;
            set => InstallationDateField.Value = value;
        }

        public Guid ModificationUserId
        {
            get => ModificationUserIdField.Value;
            set => ModificationUserIdField.Value = value;
        }

        public DateTime? ModificationDate
        {
            get => ModificationDateField.Value;
            set => ModificationDateField.Value = value;
        }

        public SlcAsset_Management.Enums.AssetStateEnum State
        {
            get => StateField.Value;
            set => StateField.Value = value;
        }

        #endregion

        #region Ownership Properties

        public SdmObjectReference<ContactPerson> OwnerContactPersonId
        {
            get => OwnerContactPersonIdField.Value;
            set => OwnerContactPersonIdField.Value = value;
        }

        public SdmObjectReference<Role> OwnerContactPersonRoleId
        {
            get => OwnerContactPersonRoleIdField.Value;
            set => OwnerContactPersonRoleIdField.Value = value;
        }

        public SdmObjectReference<ContactPerson> CustodyContactPersonId
        {
            get => CustodyContactPersonIdField.Value;
            set => CustodyContactPersonIdField.Value = value;
        }

        public SdmObjectReference<Role> CustodyContactPersonRoleId
        {
            get => CustodyContactPersonRoleIdField.Value;
            set => CustodyContactPersonRoleIdField.Value = value;
        }

        #endregion

        #region Collection Properties

        public List<DataPortInfo> DataPorts
        {
            get => DataPortsField.Value ?? new List<DataPortInfo>();
            set => DataPortsField.Value = value;
        }

        public List<PowerPortInfo> PowerPorts
        {
            get => PowerPortsField.Value ?? new List<PowerPortInfo>();
            set => PowerPortsField.Value = value;
        }

        public List<AssetHolder> Holders
        {
            get => HoldersField.Value ?? new List<AssetHolder>();
            set => HoldersField.Value = value;
        }

        public List<AssetElement> Elements
        {
            get => ElementsField.Value ?? new List<AssetElement>();
            set => ElementsField.Value = value;
        }

        #endregion

        #region Info Tracking Fields

        [JsonIgnore]
        internal IChangeTrackingField<string> NameField => FieldHandler.GetOrCreateField(
            nameof(Name),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        internal IChangeTrackingField<string> AssetIDField => FieldHandler.GetOrCreateField(
            nameof(AssetID),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        internal IChangeTrackingField<SdmObjectReference<AssetClass>> AssetClassIdField => FieldHandler.GetOrCreateField(
            nameof(AssetClassId),
            () => new ChangeTrackingField<SdmObjectReference<AssetClass>>(default));

        [JsonIgnore]
        internal IChangeTrackingField<string> SerialNumberField => FieldHandler.GetOrCreateField(
            nameof(SerialNumber),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        internal IChangeTrackingField<string> DescriptionField => FieldHandler.GetOrCreateField(
            nameof(Description),
            () => new ChangeTrackingStringField(null));

        #endregion

        #region Network Tracking Fields

        [JsonIgnore]
        internal IChangeTrackingField<string> PrimaryIPv4AddressField => FieldHandler.GetOrCreateField(
            nameof(PrimaryIPv4Address),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        internal IChangeTrackingField<string> PrimaryIPv6AddressField => FieldHandler.GetOrCreateField(
            nameof(PrimaryIPv6Address),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        internal IChangeTrackingField<string> PrimaryMacAddressField => FieldHandler.GetOrCreateField(
            nameof(PrimaryMacAddress),
            () => new ChangeTrackingStringField(null));

        #endregion

        #region Location Tracking Fields

        [JsonIgnore]
        internal IChangeTrackingField<AssetLocation> LocationField => FieldHandler.GetOrCreateField(
            nameof(Location),
            () => new ChangeTrackingField<AssetLocation>(new AssetLocation()));

        [JsonIgnore]
        internal IChangeTrackingField<AssetLocation> DestinationLocationField => FieldHandler.GetOrCreateField(
            nameof(DestinationLocation),
            () => new ChangeTrackingField<AssetLocation>(new AssetLocation()));

        #endregion

        #region Lifecycle Tracking Fields

        [JsonIgnore]
        internal IChangeTrackingField<Guid> InstallationUserIdField => FieldHandler.GetOrCreateField(
            nameof(InstallationUserId),
            () => new ChangeTrackingField<Guid>(default));

        [JsonIgnore]
        internal IChangeTrackingField<DateTime?> InstallationDateField => FieldHandler.GetOrCreateField(
            nameof(InstallationDate),
            () => new ChangeTrackingField<DateTime?>(null));

        [JsonIgnore]
        internal IChangeTrackingField<Guid> ModificationUserIdField => FieldHandler.GetOrCreateField(
            nameof(ModificationUserId),
            () => new ChangeTrackingField<Guid>(default));

        [JsonIgnore]
        internal IChangeTrackingField<DateTime?> ModificationDateField => FieldHandler.GetOrCreateField(
            nameof(ModificationDate),
            () => new ChangeTrackingField<DateTime?>(null));

        [JsonIgnore]
        internal IChangeTrackingField<SlcAsset_Management.Enums.AssetStateEnum> StateField => FieldHandler.GetOrCreateField(
            nameof(State),
            () => new ChangeTrackingField<SlcAsset_Management.Enums.AssetStateEnum>(default));

        #endregion

        #region Ownership Tracking Fields

        [JsonIgnore]
        internal IChangeTrackingField<SdmObjectReference<ContactPerson>> OwnerContactPersonIdField => FieldHandler.GetOrCreateField(
            nameof(OwnerContactPersonId),
            () => new ChangeTrackingField<SdmObjectReference<ContactPerson>>(default));

        [JsonIgnore]
        internal IChangeTrackingField<SdmObjectReference<Role>> OwnerContactPersonRoleIdField => FieldHandler.GetOrCreateField(
            nameof(OwnerContactPersonRoleId),
            () => new ChangeTrackingField<SdmObjectReference<Role>>(default));

        [JsonIgnore]
        internal IChangeTrackingField<SdmObjectReference<ContactPerson>> CustodyContactPersonIdField => FieldHandler.GetOrCreateField(
            nameof(CustodyContactPersonId),
            () => new ChangeTrackingField<SdmObjectReference<ContactPerson>>(default));

        [JsonIgnore]
        internal IChangeTrackingField<SdmObjectReference<Role>> CustodyContactPersonRoleIdField => FieldHandler.GetOrCreateField(
            nameof(CustodyContactPersonRoleId),
            () => new ChangeTrackingField<SdmObjectReference<Role>>(default));

        #endregion

        #region Collection Tracking Fields

        [JsonIgnore]
        internal ChangeTrackingArrayField<DataPortInfo> DataPortsField => FieldHandler.GetOrCreateArrayField(
            nameof(DataPorts),
            () => new ChangeTrackingArrayField<DataPortInfo>(new List<DataPortInfo>()));

        [JsonIgnore]
        internal ChangeTrackingArrayField<PowerPortInfo> PowerPortsField => FieldHandler.GetOrCreateArrayField(
            nameof(PowerPorts),
            () => new ChangeTrackingArrayField<PowerPortInfo>(new List<PowerPortInfo>()));

        [JsonIgnore]
        internal ChangeTrackingArrayField<AssetHolder> HoldersField => FieldHandler.GetOrCreateArrayField(
            nameof(Holders),
            () => new ChangeTrackingArrayField<AssetHolder>(new List<AssetHolder>()));

        [JsonIgnore]
        internal ChangeTrackingArrayField<AssetElement> ElementsField => FieldHandler.GetOrCreateArrayField(
            nameof(Elements),
            () => new ChangeTrackingArrayField<AssetElement>(new List<AssetElement>()));

        #endregion

        public void ResetChangeTracking()
        {
            FieldHandler?.ApplyChanges();
        }
    }
}