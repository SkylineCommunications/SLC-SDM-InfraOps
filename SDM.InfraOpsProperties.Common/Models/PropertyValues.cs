namespace Skyline.DataMiner.SDM.InfraOpsProperties.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Newtonsoft.Json;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    //[GenerateExposers]
    //[SdmDomStorage("(infraops)properties")]
    public class PropertyValues : SdmObject<PropertyValues>, IEntityTracking
    {
        [JsonIgnore]
        private ChangeTrackingFieldHandler _fieldHandler;
        [JsonIgnore]
        private bool _isNew = true;

        public PropertyValues()
        {
            _fieldHandler = new ChangeTrackingFieldHandler();
        }

        [JsonIgnore]
        [SdmIgnore]
        internal ChangeTrackingFieldHandler FieldHandler
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

        /// <summary>
        /// Gets a value indicating whether the entity has not yet been persisted.
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

        [JsonIgnore]
        [SdmIgnore]
        public bool Changed =>
            FieldHandler.HasChanges ||
            ValuesField?.Changed == true;

        #region PropertyValueInfo

        public Guid LinkedObjectID
        {
            get => LinkedObjectIDField.Value;
            set => LinkedObjectIDField.Value = value;
        }

        public string Scope
        {
            get => ScopeField.Value;
            set => ScopeField.Value = value;
        }

        public string SubID
        {
            get => SubIDField.Value;
            set => SubIDField.Value = value;
        }

        #endregion

        #region PropertyValue

        public List<PropertyValue> Values
        {
            get => ValuesField.Value ?? new List<PropertyValue>();
            set => ValuesField.Value = value;
        }

        #endregion

        #region Section Tracking

        /// <summary>
        /// Gets or sets the DOM Section ID of the PropertyValuesProperties section, captured on read so it can be reused on update.
        /// </summary>
        [JsonIgnore]
        [SdmIgnore]
        internal Guid? PropertyValuesPropertiesSectionId { get; set; }

        #endregion

        #region PropertyValueInfo Tracking Fields

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<Guid> LinkedObjectIDField => FieldHandler.GetOrCreateField(
            nameof(LinkedObjectID),
            () => new ChangeTrackingField<Guid>(Guid.Empty));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> ScopeField => FieldHandler.GetOrCreateField(
            nameof(Scope),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> SubIDField => FieldHandler.GetOrCreateField(
            nameof(SubID),
            () => new ChangeTrackingStringField(null));

        #endregion

        #region Collection Tracking Fields

        [JsonIgnore]
        [SdmIgnore]
        internal ChangeTrackingArrayField<PropertyValue> ValuesField => FieldHandler.GetOrCreateArrayField(
            nameof(Values),
            () => new ChangeTrackingArrayField<PropertyValue>(new List<PropertyValue>()));

        #endregion

        public void ResetChangeTracking()
        {
            FieldHandler?.ApplyChanges();

            // Cascade to list items if they implement IChangeTracking
            if (Values != null)
            {
                foreach (var value in Values.OfType<IChangeTracking>())
                {
                    value?.ResetChangeTracking();
                }
            }
        }
    }
}
