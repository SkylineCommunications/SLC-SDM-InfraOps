namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Newtonsoft.Json;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    //[GenerateExposers]
    //[SdmDomStorage("(slc)asset_management")]
    public class Asset : SdmObject<Asset>, IEntityTracking
    {
        [JsonIgnore]
        private ChangeTrackingFieldHandler _fieldHandler;
        [JsonIgnore]
        private bool _isNew = true;

        public Asset()
        {
            _fieldHandler = new ChangeTrackingFieldHandler();
        }

        [JsonIgnore]
        [SdmIgnore]
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

        [JsonIgnore]
        [SdmIgnore]
        public Guid Id { get; set; }

        [JsonIgnore]
        [SdmIgnore]
        public bool Changed =>
            FieldHandler.HasChanges ||
            Location?.Changed == true ||
            DestinationLocation?.Changed == true ||
            Ownership?.Changed == true ||
            Custody?.Changed == true ||
            HoldersField?.Changed == true ||
            ElementsField?.Changed == true ||
            StateField?.Changed == true;

        /// <summary>
        /// Gets a value indicating whether the current object has not been assigned an identifier.
        /// </summary>
        [JsonIgnore]
        [SdmIgnore]
        public bool IsNew => _isNew;

        /// <summary>
        /// Sets the IsNew flag. Used internally when loading from database.
        /// </summary>
        [JsonIgnore]
        [SdmIgnore]
        internal bool IsNewInternal
        {
            get => _isNew;
            set => _isNew = value;
        }

        /// <summary>
        /// Gets or sets the current status of the asset.
        /// </summary>
        public SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum State
        {
            get => StateField.Value; internal set => StateField.Value = value;
        }
        

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

        public string FW_OS
        {
            get => FwOSField.Value;
            set => FwOSField.Value = value;
        }

        public string HardwareVersion
        {
            get => HardwareVersionField.Value;
            set => HardwareVersionField.Value = value;
        }

        public long OperationalFlags
        {
            get => OperationalFlagsField.Value;
            set => OperationalFlagsField.Value = value;
        }

        #endregion

        #region Network Properties

        public string MacAddress
        {
            get => MacAddressField.Value;
            set => MacAddressField.Value = value;
        }

        #endregion

        #region Location Properties

        public AssetLocation Location { get; set; }

        public AssetLocation DestinationLocation { get; set; }

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

        public DateTime? FirstUseDate
        {
            get => FirstUseDateField.Value;
            set => FirstUseDateField.Value = value;
        }

        public DateTime? PurchaseDate
        {
            get => PurchaseDateField.Value;
            set => PurchaseDateField.Value = value;
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

        public DateTime? EndOfLifeDate
        {
            get => EndOfLifeDateField.Value;
            set => EndOfLifeDateField.Value = value;
        }

        public DateTime? EndOfWarrantyDate
        {
            get => EndOfWarrantyDateField.Value;
            set => EndOfWarrantyDateField.Value = value;
        }

        #endregion

        #region Ownership Properties

        public AssetOwnership Ownership { get; set; }

        public AssetCustody Custody { get; set; }

        #endregion

        #region Collection Properties

        public List<AssetHolder> Holders
        {
            get => HoldersField.Value ?? new List<AssetHolder>();
            set => HoldersField.Value = value;
        }

        public List<ElementLink> ElementLinks
        {
            get => ElementsField.Value ?? new List<ElementLink>();
            set => ElementsField.Value = value;
        }

        #endregion

        #region Info Tracking Fields

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> NameField => FieldHandler.GetOrCreateField(
            nameof(Name),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> AssetIDField => FieldHandler.GetOrCreateField(
            nameof(AssetID),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<SdmObjectReference<AssetClass>> AssetClassIdField => FieldHandler.GetOrCreateField(
            nameof(AssetClassId),
            () => new ChangeTrackingField<SdmObjectReference<AssetClass>>(default));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> SerialNumberField => FieldHandler.GetOrCreateField(
            nameof(SerialNumber),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> DescriptionField => FieldHandler.GetOrCreateField(
            nameof(Description),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> FwOSField => FieldHandler.GetOrCreateField(
            nameof(FW_OS),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> HardwareVersionField => FieldHandler.GetOrCreateField(
           nameof(HardwareVersion),
           () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<long> OperationalFlagsField => FieldHandler.GetOrCreateField(
            nameof(OperationalFlags),
            () => new ChangeTrackingField<long>(0));

        #endregion

        #region Network Tracking Fields

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> MacAddressField => FieldHandler.GetOrCreateField(
            nameof(MacAddress),
            () => new ChangeTrackingStringField(null));

        #endregion

        #region Lifecycle Tracking Fields

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<Guid> InstallationUserIdField => FieldHandler.GetOrCreateField(
            nameof(InstallationUserId),
            () => new ChangeTrackingField<Guid>(Guid.Empty));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<DateTime?> InstallationDateField => FieldHandler.GetOrCreateField(
            nameof(InstallationDate),
            () => new ChangeTrackingField<DateTime?>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<DateTime?> FirstUseDateField => FieldHandler.GetOrCreateField(
            nameof(FirstUseDate),
            () => new ChangeTrackingField<DateTime?>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<DateTime?> PurchaseDateField => FieldHandler.GetOrCreateField(
            nameof(PurchaseDate),
            () => new ChangeTrackingField<DateTime?>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<Guid> ModificationUserIdField => FieldHandler.GetOrCreateField(
            nameof(ModificationUserId),
            () => new ChangeTrackingField<Guid>(Guid.Empty));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<DateTime?> ModificationDateField => FieldHandler.GetOrCreateField(
            nameof(ModificationDate),
            () => new ChangeTrackingField<DateTime?>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<DateTime?> EndOfLifeDateField => FieldHandler.GetOrCreateField(
            nameof(EndOfLifeDate),
            () => new ChangeTrackingField<DateTime?>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<DateTime?> EndOfWarrantyDateField => FieldHandler.GetOrCreateField(
          nameof(EndOfWarrantyDate),
          () => new ChangeTrackingField<DateTime?>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum> StateField => FieldHandler.GetOrCreateField(
            nameof(State),
            () => new ChangeTrackingField<SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum>(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable));

        #endregion

        #region Collection Tracking Fields

        [JsonIgnore]
        [SdmIgnore]
        internal ChangeTrackingArrayField<AssetHolder> HoldersField => FieldHandler.GetOrCreateArrayField(
            nameof(Holders),
            () => new ChangeTrackingArrayField<AssetHolder>(new List<AssetHolder>()));

        [JsonIgnore]
        [SdmIgnore]
        internal ChangeTrackingArrayField<ElementLink> ElementsField => FieldHandler.GetOrCreateArrayField(
            nameof(ElementLinks),
            () => new ChangeTrackingArrayField<ElementLink>(new List<ElementLink>()));

        #endregion

        public void ResetChangeTracking()
        {
            FieldHandler?.ApplyChanges();
            Location?.ResetChangeTracking();
            DestinationLocation?.ResetChangeTracking();
            Ownership?.ResetChangeTracking();
            Custody?.ResetChangeTracking();

            // Cascade to list items if they implement IChangeTracking
            if (Holders != null)
            {
                foreach (var holder in Holders.OfType<IChangeTracking>())
                {
                    holder?.ResetChangeTracking();
                }
            }

            if (ElementLinks != null)
            {
                foreach (var link in ElementLinks.OfType<IChangeTracking>())
                {
                    link?.ResetChangeTracking();
                }
            }
        }
    }
}