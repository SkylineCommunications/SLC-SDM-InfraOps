using System.Collections.Generic;
namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    using System;
    using System.Runtime.Serialization;

    using Newtonsoft.Json;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    // [GenerateExposers]
    [SdmDomStorage("(slc)facility_management")]
    public class Rack : SdmObject<Rack>
    {
        [JsonIgnore]
        private ChangeTrackingFieldHandler _fieldHandler;

        public Rack()
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
            ResetChangeTracking();
        }

        // PUBLIC API: Simple properties
        public string Name
        {
            get => NameField.Value;
            set => NameField.Value = value;
        }

        public string Model
        {
            get => ModelField.Value;
            set => ModelField.Value = value;
        }

        public SlcFacility_Management.Enums.RackpositionenumEnum Position
        {
            get => PositionField.Value;
            set => PositionField.Value = value;
        }

        public double Width
        {
            get => WidthField.Value;
            set => WidthField.Value = value;
        }

        public double Height
        {
            get => HeightField.Value;
            set => HeightField.Value = value;
        }

        public double Depth
        {
            get => DepthField.Value;
            set => DepthField.Value = value;
        }

        public string Description
        {
            get => DescriptionField.Value;
            set => DescriptionField.Value = value;
        }

        public bool Bookable
        {
            get => BookableField.Value;
            set => BookableField.Value = value;
        }

        public SlcFacility_Management.Enums.CoolingflowenumEnum CoolingFlow
        {
            get => CoolingFlowField.Value;
            set => CoolingFlowField.Value = value;
        }

        public double XPosition
        {
            get => XPositionField.Value;
            set => XPositionField.Value = value;
        }

        public double YPosition
        {
            get => YPositionField.Value;
            set => YPositionField.Value = value;
        }

        public string Label
        {
            get => LabelField.Value;
            set => LabelField.Value = value;
        }

        public SlcFacility_Management.Enums.Placementorientationenum Orientation
        {
            get => OrientationField.Value;
            set => OrientationField.Value = value;
        }

        public string RackId
        {
            get => RackIdField.Value;
            set => RackIdField.Value = value;
        }

        public RackCapacity Capacity { get; set; }

        public RowRelation RowFk { get; set; }

        public ZoneRelation ZoneFk { get; set; }

        public ResourceLink Resource { get; set; }

        public List<ImageInfo> ImageDetails { get; set; }

        // INTERNAL: Change tracking fields (validation handler uses these)
        [JsonIgnore]
        internal IChangeTrackingField<string> NameField => FieldHandler.GetOrCreateField(
            nameof(Name),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        internal IChangeTrackingField<string> ModelField => FieldHandler.GetOrCreateField(
            nameof(Model),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        internal IChangeTrackingField<SlcFacility_Management.Enums.RackpositionenumEnum> PositionField => FieldHandler.GetOrCreateField(
            nameof(Position),
            () => new ChangeTrackingField<SlcFacility_Management.Enums.RackpositionenumEnum>(default));

        [JsonIgnore]
        internal IChangeTrackingField<double> WidthField => FieldHandler.GetOrCreateField(
            nameof(Width),
            () => new ChangeTrackingField<double>(0));

        [JsonIgnore]
        internal IChangeTrackingField<double> HeightField => FieldHandler.GetOrCreateField(
            nameof(Height),
            () => new ChangeTrackingField<double>(0));

        [JsonIgnore]
        internal IChangeTrackingField<double> DepthField => FieldHandler.GetOrCreateField(
            nameof(Depth),
            () => new ChangeTrackingField<double>(0));

        [JsonIgnore]
        internal IChangeTrackingField<string> DescriptionField => FieldHandler.GetOrCreateField(
            nameof(Description),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        internal IChangeTrackingField<bool> BookableField => FieldHandler.GetOrCreateField(
            nameof(Bookable),
            () => new ChangeTrackingField<bool>(false));

        [JsonIgnore]
        internal IChangeTrackingField<SlcFacility_Management.Enums.CoolingflowenumEnum> CoolingFlowField => FieldHandler.GetOrCreateField(
            nameof(CoolingFlow),
            () => new ChangeTrackingField<SlcFacility_Management.Enums.CoolingflowenumEnum>(default));

        [JsonIgnore]
        internal IChangeTrackingField<double> XPositionField => FieldHandler.GetOrCreateField(
            nameof(XPosition),
            () => new ChangeTrackingField<double>(0));

        [JsonIgnore]
        internal IChangeTrackingField<double> YPositionField => FieldHandler.GetOrCreateField(
            nameof(YPosition),
            () => new ChangeTrackingField<double>(0));

        [JsonIgnore]
        internal IChangeTrackingField<string> LabelField => FieldHandler.GetOrCreateField(
            nameof(Label),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        internal IChangeTrackingField<SlcFacility_Management.Enums.Placementorientationenum> OrientationField => FieldHandler.GetOrCreateField(
            nameof(Orientation),
            () => new ChangeTrackingField<SlcFacility_Management.Enums.Placementorientationenum>(default));

        [JsonIgnore]
        internal IChangeTrackingField<string> RackIdField => FieldHandler.GetOrCreateField(
            nameof(RackId),
            () => new ChangeTrackingStringField(null));

        // Reset change tracking after deserialization or save
        public void ResetChangeTracking()
        {
            FieldHandler.ApplyChanges();
        }
    }
}