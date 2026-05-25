namespace Skyline.DataMiner.Utils.InfraOps.Common.Fields
{
    /// <summary>
    /// Interface for entities that track both changes and persistence state.
    /// Use this for SDM objects that are independently persisted to the database.
    /// </summary>
    public interface IEntityTracking : IChangeTracking
    {
        /// <summary>
        /// Gets whether the entity has not yet been persisted to the database.
        /// </summary>
        bool IsNew { get; }
    }
}
