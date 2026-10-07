namespace Skyline.DataMiner.Utils.InfraOps.Common.Fields
{
    using System.Collections.Generic;

    internal interface IChangeTracking
    {
        /// <summary>
        /// Gets a value indicating whether the object has been modified since it was last saved or loaded.
        /// </summary>
        bool Changed { get; }

        /// <summary>
        /// Gets a collection of changes made to the tracked fields, including the field name, old value, and new value.
        /// </summary>
        /// <returns>A collection of TrackingFieldValueDifference objects representing the changes.</returns>
        IEnumerable<TrackingFieldValueDifference> GetChanges();

        /// <summary>
        /// Resets the change tracking state, marking all tracked changes as unchanged.
        /// </summary>
        /// <remarks>Call this method to clear the record of changes, typically after saving or accepting
        /// modifications. Subsequent change tracking will start from the current state.</remarks>
        void ResetChangeTracking();
    }
}