namespace Skyline.DataMiner.Utils.InfraOps.Common.Fields
{
    using Newtonsoft.Json;

    /// <summary>
    /// Provides a base class for objects that support field-level change tracking.
    /// </summary>
    /// <remarks>Derive from this class to enable automatic tracking of changes to fields within an object.
    /// The class implements the IChangeTracking interface and supplies core functionality for monitoring and resetting
    /// change state. Change tracking is reset after deserialization to ensure accurate state management.</remarks>
    public abstract class ChangeTrackingBase : IChangeTracking
    {
        [JsonIgnore]
        private ChangeTrackingFieldHandler _fieldHandler;

        /// <summary>
        /// Initializes a new instance of the ChangeTrackingBase class.
        /// </summary>
        /// <remarks>This protected constructor is intended to be called by derived classes to initialize
        /// change tracking functionality. It sets up the internal field handler required for tracking changes to
        /// fields.</remarks>
        protected ChangeTrackingBase()
        {
            _fieldHandler = new ChangeTrackingFieldHandler();
        }

        /// <summary>
        /// Gets the handler responsible for tracking changes to fields in the containing object.
        /// </summary>
        [JsonIgnore]
        protected ChangeTrackingFieldHandler FieldHandler
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
        /// Gets or sets a value indicating whether the entity has not yet been persisted or saved.
        /// </summary>
        [JsonIgnore]
        public bool IsNew { get; set; } = true;

        /// <summary>
        /// Gets a value indicating whether any fields have been modified since the last save or initialization.
        /// </summary>
        [JsonIgnore]
        public bool Changed => FieldHandler.HasChanges;

        /// <summary>
        /// Resets the change tracking state for the current object, applying any pending changes.
        /// </summary>
        /// <remarks>Call this method to commit tracked changes and clear the change tracking state. After
        /// calling this method, subsequent change tracking will start from the current state.</remarks>
        public virtual void ResetChangeTracking()
        {
            FieldHandler?.ApplyChanges();
        }
    }
}