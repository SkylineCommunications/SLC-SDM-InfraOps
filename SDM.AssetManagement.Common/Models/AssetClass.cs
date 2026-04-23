namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    using Newtonsoft.Json;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    //[GenerateExposers]
    //[SdmDomStorage("(slc)asset_management")]
    public class AssetClass : SdmObject<AssetClass>
    {
        [JsonIgnore]
        private ChangeTrackingFieldHandler _fieldHandler;

        public AssetClass()
        {
            _fieldHandler = new ChangeTrackingFieldHandler();
        }

        // Ensure _fieldHandler is always initialized (handles JSON deserialization without constructor)
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

        // Called after JSON deserialization to reset change tracking
        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            // Apply all current values as "original" values to reset change tracking
            ResetChangeTracking();
        }

        [JsonIgnore]
        public Guid Id { get; set; }

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

        public double Depth
        {
            get => DepthField.Value;
            set => DepthField.Value = value;
        }

        public double Height
        {
            get => HeightField.Value;
            set => HeightField.Value = value;
        }

        public double Width
        {
            get => WidthField.Value;
            set => WidthField.Value = value;
        }

        public double HeightU
        {
            get => HeightUField.Value;
            set => HeightUField.Value = value;
        }

        public double Weight
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

        public double TypicalPowerConsumption
        {
            get => TypicalPowerConsumptionField.Value;
            set => TypicalPowerConsumptionField.Value = value;
        }

        public double MaximumPowerConsumption
        {
            get => MaximumPowerConsumptionField.Value;
            set => MaximumPowerConsumptionField.Value = value;
        }

        public SlcAsset_Management.Enums.PowerSupplyEnum PowerSupply
        {
            get => PowerSupplyField.Value;
            set => PowerSupplyField.Value = value;
        }

        public AssetClassLifecycle Lifecycle
        {
            get => LifecycleField.Value ?? new AssetClassLifecycle();
            set => LifecycleField.Value = value;
        }

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

        // INTERNAL: Change tracking fields (validation handler uses these)
        [JsonIgnore]
        internal IChangeTrackingField<string> NameField => FieldHandler.GetOrCreateField(
            nameof(Name),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        internal IChangeTrackingField<SdmObjectReference<DeviceType>> DeviceTypeIdField => FieldHandler.GetOrCreateField(
            nameof(DeviceTypeId),
            () => new ChangeTrackingField<SdmObjectReference<DeviceType>>(default));

        [JsonIgnore]
        internal IChangeTrackingField<string> DescriptionField => FieldHandler.GetOrCreateField(
            nameof(Description),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        internal IChangeTrackingField<Guid> ManufacturerField => FieldHandler.GetOrCreateField(
            nameof(Manufacturer),
            () => new ChangeTrackingField<Guid>(Guid.Empty));

        [JsonIgnore]
        internal IChangeTrackingField<double> DepthField => FieldHandler.GetOrCreateField(
            nameof(Depth),
            () => new ChangeTrackingField<double>(0));

        [JsonIgnore]
        internal IChangeTrackingField<double> HeightField => FieldHandler.GetOrCreateField(
            nameof(Height),
            () => new ChangeTrackingField<double>(0));

        [JsonIgnore]
        internal IChangeTrackingField<double> WidthField => FieldHandler.GetOrCreateField(
            nameof(Width),
            () => new ChangeTrackingField<double>(0));

        [JsonIgnore]
        internal IChangeTrackingField<double> HeightUField => FieldHandler.GetOrCreateField(
            nameof(HeightU),
            () => new ChangeTrackingField<double>(0));

        [JsonIgnore]
        internal IChangeTrackingField<double> WeightField => FieldHandler.GetOrCreateField(
            nameof(Weight),
            () => new ChangeTrackingField<double>(0));

        [JsonIgnore]
        internal IChangeTrackingField<string> PlanField => FieldHandler.GetOrCreateField(
           nameof(FrontImage),
           () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        internal IChangeTrackingField<string> FrontImageField => FieldHandler.GetOrCreateField(
            nameof(FrontImage),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        internal IChangeTrackingField<string> BackImageField => FieldHandler.GetOrCreateField(
            nameof(BackImage),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        internal IChangeTrackingField<double> TypicalPowerConsumptionField => FieldHandler.GetOrCreateField(
            nameof(TypicalPowerConsumption),
            () => new ChangeTrackingField<double>(0));

        [JsonIgnore]
        internal IChangeTrackingField<double> MaximumPowerConsumptionField => FieldHandler.GetOrCreateField(
            nameof(MaximumPowerConsumption),
            () => new ChangeTrackingField<double>(0));

        [JsonIgnore]
        internal IChangeTrackingField<SlcAsset_Management.Enums.PowerSupplyEnum> PowerSupplyField => FieldHandler.GetOrCreateField(
            nameof(PowerSupply),
            () => new ChangeTrackingField<SlcAsset_Management.Enums.PowerSupplyEnum>(default));

        [JsonIgnore]
        internal IChangeTrackingField<AssetClassLifecycle> LifecycleField => FieldHandler.GetOrCreateField(
            nameof(Lifecycle),
            () => new ChangeTrackingField<AssetClassLifecycle>(new AssetClassLifecycle()));

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

        public void ResetChangeTracking()
        {
            FieldHandler?.ApplyChanges();
        }
    }
}