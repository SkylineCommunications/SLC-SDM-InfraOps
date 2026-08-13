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
    public class AssetClass : SdmObject<AssetClass>, IEntityTracking
    {
        [JsonIgnore]
        private ChangeTrackingFieldHandler _fieldHandler;
        [JsonIgnore]
        private AssetClassLifecycle _lifecycle;
        [JsonIgnore]
        private ProtocolLink _protocolLink;
        [JsonIgnore]
        private bool _isNew = true;

        public AssetClass()
        {
            _fieldHandler = new ChangeTrackingFieldHandler();
        }

        // Ensure _fieldHandler is always initialized (handles JSON deserialization without constructor)
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

        // PUBLIC API: Simple types (consumers see these)
        public string Name
        {
            get => NameField.Value;
            set => NameField.Value = value;
        }

        public SdmObjectReference<DeviceType> DeviceTypeId
        {
            get => DeviceTypeIdField.Value;
            set => DeviceTypeIdField.Value = value;
        }

        public string Description
        {
            get => DescriptionField.Value;
            set => DescriptionField.Value = value;
        }

        public Guid Manufacturer
        {
            get => ManufacturerField.Value;
            set => ManufacturerField.Value = value;
        }

        public double? Depth
        {
            get => DepthField.Value;
            set => DepthField.Value = value;
        }

        public double? Height
        {
            get => HeightField.Value;
            set => HeightField.Value = value;
        }

        public double? Width
        {
            get => WidthField.Value;
            set => WidthField.Value = value;
        }

        public double? HeightU
        {
            get => HeightUField.Value;
            set => HeightUField.Value = value;
        }

        public double? Weight
        {
            get => WeightField.Value;
            set => WeightField.Value = value;
        }

        public string Plan
        {
            get => PlanField.Value;
            set => PlanField.Value = value;
        }

        public string FrontImage
        {
            get => FrontImageField.Value;
            set => FrontImageField.Value = value;
        }

        public string BackImage
        {
            get => BackImageField.Value;
            set => BackImageField.Value = value;
        }

        public double? TypicalPowerConsumption
        {
            get => TypicalPowerConsumptionField.Value;
            set => TypicalPowerConsumptionField.Value = value;
        }

        public double? MaximumPowerConsumption
        {
            get => MaximumPowerConsumptionField.Value;
            set => MaximumPowerConsumptionField.Value = value;
        }

        public SlcAsset_Management.Enums.PowerSupplyEnum? PowerSupply
        {
            get => PowerSupplyField.Value;
            set => PowerSupplyField.Value = value;
        }

        public AssetClassLifecycle Lifecycle => _lifecycle ?? (_lifecycle = new AssetClassLifecycle());

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

        public bool IsBookable
        {
            get => IsBookableField.Value;
            set => IsBookableField.Value = value;
        }

        public ProtocolLink ProtocolLink
        {
            get => _protocolLink ?? (_protocolLink = new ProtocolLink());
            set => _protocolLink = value ?? new ProtocolLink();
        }

        /// <summary>
        /// Gets or sets a value indicating whether the entity has not yet been persisted or saved.
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

        // INTERNAL: Change tracking fields (validation handler uses these)
        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> NameField => FieldHandler.GetOrCreateField(
            nameof(Name),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<SdmObjectReference<DeviceType>> DeviceTypeIdField => FieldHandler.GetOrCreateField(
            nameof(DeviceTypeId),
            () => new ChangeTrackingField<SdmObjectReference<DeviceType>>(default));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> DescriptionField => FieldHandler.GetOrCreateField(
            nameof(Description),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<Guid> ManufacturerField => FieldHandler.GetOrCreateField(
            nameof(Manufacturer),
            () => new ChangeTrackingField<Guid>(Guid.Empty));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<double?> DepthField => FieldHandler.GetOrCreateField(
            nameof(Depth),
            () => new ChangeTrackingField<double?>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<double?> HeightField => FieldHandler.GetOrCreateField(
            nameof(Height),
            () => new ChangeTrackingField<double?>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<double?> WidthField => FieldHandler.GetOrCreateField(
            nameof(Width),
            () => new ChangeTrackingField<double?>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<double?> HeightUField => FieldHandler.GetOrCreateField(
            nameof(HeightU),
            () => new ChangeTrackingField<double?>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<double?> WeightField => FieldHandler.GetOrCreateField(
            nameof(Weight),
            () => new ChangeTrackingField<double?>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> PlanField => FieldHandler.GetOrCreateField(
           nameof(Plan),
           () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> FrontImageField => FieldHandler.GetOrCreateField(
            nameof(FrontImage),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> BackImageField => FieldHandler.GetOrCreateField(
            nameof(BackImage),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<double?> TypicalPowerConsumptionField => FieldHandler.GetOrCreateField(
            nameof(TypicalPowerConsumption),
            () => new ChangeTrackingField<double?>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<double?> MaximumPowerConsumptionField => FieldHandler.GetOrCreateField(
            nameof(MaximumPowerConsumption),
            () => new ChangeTrackingField<double?>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<SlcAsset_Management.Enums.PowerSupplyEnum?> PowerSupplyField => FieldHandler.GetOrCreateField(
            nameof(PowerSupply),
            () => new ChangeTrackingField<SlcAsset_Management.Enums.PowerSupplyEnum?>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal ChangeTrackingArrayField<DataPortInfo> DataPortsField => FieldHandler.GetOrCreateArrayField(
            nameof(DataPorts),
            () => new ChangeTrackingArrayField<DataPortInfo>(new List<DataPortInfo>()));

        [JsonIgnore]
        [SdmIgnore]
        internal ChangeTrackingArrayField<PowerPortInfo> PowerPortsField => FieldHandler.GetOrCreateArrayField(
            nameof(PowerPorts),
            () => new ChangeTrackingArrayField<PowerPortInfo>(new List<PowerPortInfo>()));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum> StateField => FieldHandler.GetOrCreateField(
            nameof(State),
            () => new ChangeTrackingField<SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum>(SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Draft));

        [JsonIgnore]
        [SdmIgnore]
        internal ChangeTrackingArrayField<AssetHolder> HoldersField => FieldHandler.GetOrCreateArrayField(
            nameof(Holders),
            () => new ChangeTrackingArrayField<AssetHolder>(new List<AssetHolder>()));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<bool> IsBookableField => FieldHandler.GetOrCreateField(
            nameof(IsBookable),
            () => new ChangeTrackingField<bool>(false));

        /// <summary>
        /// Gets the current status of the asset class.
        /// </summary>
        [SdmIgnore]
        public SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum State
        {
            get => StateField.Value; internal set => StateField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        public bool Changed => FieldHandler.HasChanges ||
            _lifecycle?.Changed == true ||
            StateField?.Changed == true ||
            (DataPorts?.Any(p => p?.Changed == true) == true);

        public void ResetChangeTracking()
        {
            FieldHandler?.ApplyChanges();
            _lifecycle?.ResetChangeTracking();

            // Cascade to list items
            if (DataPorts != null)
            {
                foreach (var port in DataPorts)
                {
                    port?.ResetChangeTracking();
                }
            }
        }

        #region Section Tracking

        [JsonIgnore]
        [SdmIgnore]
        internal Guid? AssetClassPropertiesSectionId { get; set; }

        #endregion

    }
}