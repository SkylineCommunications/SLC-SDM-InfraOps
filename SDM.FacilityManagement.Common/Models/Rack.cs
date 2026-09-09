namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Newtonsoft.Json;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    // [GenerateExposers]
    //[SdmDomStorage("(slc)facility_management")]
    public sealed class Rack : SdmObject<Rack>, IEquatable<Rack>, IEntityTracking
    {
        [JsonIgnore]
        private ChangeTrackingFieldHandler _fieldHandler;
        [JsonIgnore]
        private bool _isNew = true;

        public Rack()
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

        [JsonIgnore]
        [SdmIgnore]
        public bool Changed =>
         FieldHandler.HasChanges ||
         NameField?.Changed == true ||
         ModelField?.Changed == true ||
         PositionField?.Changed == true ||
         WidthField?.Changed == true ||
         HeightField?.Changed == true ||
         DepthField?.Changed == true ||
         DescriptionField?.Changed == true ||
         CoolingFlowField?.Changed == true ||
         BookableField?.Changed == true ||
         XPositionField?.Changed == true ||
         YPositionField?.Changed == true ||
         LabelField?.Changed == true ||
         OrientationField?.Changed == true ||
         RackIdField?.Changed == true ||
         Capacity?.Changed == true ||
         RowFk?.Changed == true ||
         ZoneFk?.Changed == true ||
         Resource?.Changed == true ||
         ImageDetailsField?.Changed == true;

        [JsonIgnore]
        [SdmIgnore]
        internal bool IsNewInternal
        {
            get => _isNew;
            set => _isNew = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        public bool IsNew => _isNew;

        #region Section Tracking

        [JsonIgnore]
        [SdmIgnore]
        internal Guid? RackPropertiesSectionId { get; set; }

        #endregion

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

        public SlcFacility_Management.Enums.RackpositionenumEnum? Position
        {
            get => PositionField.Value;
            set => PositionField.Value = value;
        }

        public double? Width
        {
            get => WidthField.Value;
            set => WidthField.Value = value;
        }

        public double? Height
        {
            get => HeightField.Value;
            set => HeightField.Value = value;
        }

        public double? Depth
        {
            get => DepthField.Value;
            set => DepthField.Value = value;
        }

        public string Description
        {
            get => DescriptionField.Value;
            set => DescriptionField.Value = value;
        }

        public bool? Bookable
        {
            get => BookableField.Value;
            set => BookableField.Value = value;
        }

        public SlcFacility_Management.Enums.CoolingflowenumEnum? CoolingFlow
        {
            get => CoolingFlowField.Value;
            set => CoolingFlowField.Value = value;
        }

        public double? XPosition
        {
            get => XPositionField.Value;
            set => XPositionField.Value = value;
        }

        public double? YPosition
        {
            get => YPositionField.Value;
            set => YPositionField.Value = value;
        }

        public string Label
        {
            get => LabelField.Value;
            set => LabelField.Value = value;
        }

        public SlcFacility_Management.Enums.Placementorientationenum? Orientation
        {
            get => OrientationField.Value;
            set => OrientationField.Value = value;
        }

        public string RackId
        {
            get => RackIdField.Value;
            set => RackIdField.Value = value;
        }

        private RackCapacity _capacity;

        public RackCapacity Capacity => _capacity ?? (_capacity = new RackCapacity());

        private RowRelation _rowFk;

        public RowRelation RowFk => _rowFk ?? (_rowFk = new RowRelation());

        private ZoneRelation _zoneFk;

        public ZoneRelation ZoneFk => _zoneFk ?? (_zoneFk = new ZoneRelation());

        private ResourceLink _resource;

        public ResourceLink Resource => _resource ?? (_resource = new ResourceLink());

        public List<ImageInfo> ImageDetails
        {
            get => ImageDetailsField.Value ?? new List<ImageInfo>();
            set => ImageDetailsField.Value = value;
        }

        [SdmIgnore]
        public SlcFacility_Management.Behaviors.Rack_Behaviour.StatusesEnum State { get; internal set; }

        // INTERNAL: Change tracking fields (validation handler uses these)
        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> NameField => FieldHandler.GetOrCreateField(
            nameof(Name),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> ModelField => FieldHandler.GetOrCreateField(
            nameof(Model),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        internal IChangeTrackingField<SlcFacility_Management.Enums.RackpositionenumEnum?> PositionField => FieldHandler.GetOrCreateField(
            nameof(Position),
            () => new ChangeTrackingField<SlcFacility_Management.Enums.RackpositionenumEnum?>(null));

        [JsonIgnore]
        internal IChangeTrackingField<double?> WidthField => FieldHandler.GetOrCreateField(
            nameof(Width),
            () => new ChangeTrackingField<double?>(null));

        [JsonIgnore]
        internal IChangeTrackingField<double?> HeightField => FieldHandler.GetOrCreateField(
            nameof(Height),
            () => new ChangeTrackingField<double?>(null));

        [JsonIgnore]
        internal IChangeTrackingField<double?> DepthField => FieldHandler.GetOrCreateField(
            nameof(Depth),
            () => new ChangeTrackingField<double?>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> DescriptionField => FieldHandler.GetOrCreateField(
            nameof(Description),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        internal IChangeTrackingField<bool?> BookableField => FieldHandler.GetOrCreateField(
            nameof(Bookable),
            () => new ChangeTrackingField<bool?>(null));

        [JsonIgnore]
        internal IChangeTrackingField<SlcFacility_Management.Enums.CoolingflowenumEnum?> CoolingFlowField => FieldHandler.GetOrCreateField(
            nameof(CoolingFlow),
            () => new ChangeTrackingField<SlcFacility_Management.Enums.CoolingflowenumEnum?>(null));

        [JsonIgnore]
        internal IChangeTrackingField<double?> XPositionField => FieldHandler.GetOrCreateField(
            nameof(XPosition),
            () => new ChangeTrackingField<double?>(null));

        [JsonIgnore]
        internal IChangeTrackingField<double?> YPositionField => FieldHandler.GetOrCreateField(
            nameof(YPosition),
            () => new ChangeTrackingField<double?>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> LabelField => FieldHandler.GetOrCreateField(
            nameof(Label),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        internal IChangeTrackingField<SlcFacility_Management.Enums.Placementorientationenum?> OrientationField => FieldHandler.GetOrCreateField(
            nameof(Orientation),
            () => new ChangeTrackingField<SlcFacility_Management.Enums.Placementorientationenum?>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> RackIdField => FieldHandler.GetOrCreateField(
            nameof(RackId),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal ChangeTrackingArrayField<ImageInfo> ImageDetailsField => FieldHandler.GetOrCreateArrayField(
            nameof(ImageDetails),
            () => new ChangeTrackingArrayField<ImageInfo>(new List<ImageInfo>()));

        // Reset change tracking after deserialization or save
        public void ResetChangeTracking()
        {
            FieldHandler.ApplyChanges();
            Capacity?.ResetChangeTracking();
            RowFk?.ResetChangeTracking();
            ZoneFk?.ResetChangeTracking();
            Resource?.ResetChangeTracking();

            // Cascade to list items if they implement IChangeTracking
            if (ImageDetails != null)
            {
                foreach (var image in ImageDetails.OfType<IChangeTracking>())
                {
                    image?.ResetChangeTracking();
                }
            }
        }

        #region Equality

        public static bool operator ==(Rack left, Rack right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left is null || right is null)
            {
                return false;
            }

            return left.Equals(right);
        }

        public static bool operator !=(Rack left, Rack right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Rack);
        }

        public bool Equals(Rack other)
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
                string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(Model, other.Model, StringComparison.OrdinalIgnoreCase) &&
                Position == other.Position &&
                Width == other.Width &&
                Height == other.Height &&
                Depth == other.Depth &&
                string.Equals(Description, other.Description, StringComparison.OrdinalIgnoreCase) &&
                Bookable == other.Bookable &&
                CoolingFlow == other.CoolingFlow &&
                XPosition == other.XPosition &&
                YPosition == other.YPosition &&
                string.Equals(Label, other.Label, StringComparison.OrdinalIgnoreCase) &&
                Orientation == other.Orientation &&
                string.Equals(RackId, other.RackId, StringComparison.OrdinalIgnoreCase) &&
                Equals(Capacity, other.Capacity) &&
                Equals(RowFk, other.RowFk) &&
                Equals(ZoneFk, other.ZoneFk) &&
                Equals(Resource, other.Resource) &&
                ListsEqual(ImageDetails, other.ImageDetails) &&
                State == other.State;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + (Name != null ? Name.GetHashCode() : 0);
                hash = (hash * 23) + (Model != null ? Model.GetHashCode() : 0);
                hash = (hash * 23) + (Description != null ? Description.GetHashCode() : 0);
                hash = (hash * 23) + (RackId != null ? RackId.GetHashCode() : 0);
                hash = (hash * 23) + (Capacity != null ? Capacity.GetHashCode() : 0);
                hash = (hash * 23) + (RowFk != null ? RowFk.GetHashCode() : 0);
                hash = (hash * 23) + (ZoneFk != null ? ZoneFk.GetHashCode() : 0);
                hash = (hash * 23) + State.GetHashCode();
                return hash;
            }
        }

        private static bool ListsEqual<T>(List<T> left, List<T> right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left is null || right is null)
            {
                return false;
            }

            return left.SequenceEqual(right);
        }

        #endregion
    }
}