namespace Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations
{
    /// <summary>
    /// Identifies the write operation that triggered a validation call.
    /// Passed by <see cref="Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Middleware.ValidationMiddleware{T}"/>
    /// so validators can apply action-specific rules (e.g. delete-in-use checks).
    /// </summary>
    public enum RepositoryAction
    {
        Create,
        Update,
        Delete,
        CreateOrUpdate,
    }
}
