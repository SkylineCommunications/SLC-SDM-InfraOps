namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    using System.Runtime.Serialization;

    using Newtonsoft.Json;

    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public class RackCapacity
    {
        [JsonIgnore]
        private ChangeTrackingFieldHandler _fieldHandler;

        public RackCapacity()
        {
            _fieldHandler = new ChangeTrackingFieldHandler();
        }

        // Ensure _fieldHandler is always initialized
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

        // PUBLIC API
        public double MaximumRackCapacity
        {
            get => RackUnitsField.Value;
            set => RackUnitsField.Value = value;
        }

        public double MaximumPowerCapacity
        {
            get => PowerCapacityField.Value;
            set => PowerCapacityField.Value = value;
        }

        // INTERNAL: Change tracking fields
        [JsonIgnore]
        internal IChangeTrackingField<double> RackUnitsField => FieldHandler.GetOrCreateField(
            nameof(MaximumRackCapacity),
            () => new ChangeTrackingField<double>(0));

        [JsonIgnore]
        internal IChangeTrackingField<double> PowerCapacityField => FieldHandler.GetOrCreateField(
            nameof(MaximumPowerCapacity),
            () => new ChangeTrackingField<double>(0));

        public void ResetChangeTracking()
        {
            FieldHandler.ApplyChanges();
        }
    }
}