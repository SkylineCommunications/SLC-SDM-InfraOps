namespace Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations
{
    using System.Collections.Generic;

    /// <summary>
    /// Contract for entity validators that can be used with <see cref="Middleware.ValidationMiddleware{T}"/>.
    /// </summary>
    internal interface IValidator<T>
        where T : class
    {
        /// <summary>Validates a single entity and returns the result without throwing.</summary>
        ValidationResult Validate(T entity);

        /// <summary>Validates a batch of entities in one pass, returning one result per item.</summary>
        List<ValidationResult> ValidateBulk(List<T> entities);
    }
}
