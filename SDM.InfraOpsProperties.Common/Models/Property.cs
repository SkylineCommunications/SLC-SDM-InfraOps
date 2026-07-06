namespace Skyline.DataMiner.SDM.InfraOpsProperties.Models
{
    using Newtonsoft.Json;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    //[GenerateExposers]
    //[SdmDomStorage("(infraops)properties")]
    public class Property : SdmObject<Property>, IEntityTracking
    {
        [JsonIgnore]
        private ChangeTrackingFieldHandler _fieldHandler;
        [JsonIgnore]
        private bool _isNew = true;

        public Property()
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
        public bool Changed => FieldHandler.HasChanges;

        #region PropertyInfo

        public string Name
        {
            get => NameField.Value;
            set => NameField.Value = value;
        }

        public InfraopsProperties.Enums.PropertyTypeEnum PropertyType
        {
            get => PropertyTypeField.Value;
            set => PropertyTypeField.Value = value;
        }

        public string Scope
        {
            get => ScopeField.Value;
            set => ScopeField.Value = value;
        }

        public string Default
        {
            get => DefaultField.Value;
            set => DefaultField.Value = value;
        }

        public long? StringSizeLimit
        {
            get => StringSizeLimitField.Value;
            set => StringSizeLimitField.Value = value;
        }

        public bool IsMultiLineString
        {
            get => IsMultiLineStringField.Value;
            set => IsMultiLineStringField.Value = value;
        }

        #endregion

        #region Layout

        public string SectionName
        {
            get => SectionNameField.Value;
            set => SectionNameField.Value = value;
        }

        public long? Order
        {
            get => OrderField.Value;
            set => OrderField.Value = value;
        }

        #endregion

        #region Discrete

        public System.Collections.Generic.List<string> Options
        {
            get => OptionsField.Value ?? new System.Collections.Generic.List<string>();
            set => OptionsField.Value = value;
        }

        #endregion

        #region PropertyInfo Tracking Fields

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> NameField => FieldHandler.GetOrCreateField(
            nameof(Name),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<InfraopsProperties.Enums.PropertyTypeEnum> PropertyTypeField => FieldHandler.GetOrCreateField(
            nameof(PropertyType),
            () => new ChangeTrackingField<InfraopsProperties.Enums.PropertyTypeEnum>(InfraopsProperties.Enums.PropertyTypeEnum.String));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> ScopeField => FieldHandler.GetOrCreateField(
            nameof(Scope),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> DefaultField => FieldHandler.GetOrCreateField(
            nameof(Default),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<long?> StringSizeLimitField => FieldHandler.GetOrCreateField(
            nameof(StringSizeLimit),
            () => new ChangeTrackingField<long?>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<bool> IsMultiLineStringField => FieldHandler.GetOrCreateField(
            nameof(IsMultiLineString),
            () => new ChangeTrackingField<bool>(false));

        #endregion

        #region Layout Tracking Fields

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> SectionNameField => FieldHandler.GetOrCreateField(
            nameof(SectionName),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<long?> OrderField => FieldHandler.GetOrCreateField(
            nameof(Order),
            () => new ChangeTrackingField<long?>(null));

        #endregion

        #region Discrete Tracking Fields

        [JsonIgnore]
        [SdmIgnore]
        internal ChangeTrackingArrayField<string> OptionsField => FieldHandler.GetOrCreateArrayField(
            nameof(Options),
            () => new ChangeTrackingArrayField<string>(new System.Collections.Generic.List<string>()));

        #endregion

        public void ResetChangeTracking()
        {
            FieldHandler?.ApplyChanges();
        }
    }
}
