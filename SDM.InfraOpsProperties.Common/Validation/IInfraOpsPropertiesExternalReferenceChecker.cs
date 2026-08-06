namespace Skyline.DataMiner.SDM.InfraOpsProperties.Validation
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Optional cross-module reference checker for InfraOps Properties validation.
    /// </summary>
    public interface IInfraOpsPropertiesExternalReferenceChecker
    {
        /// <summary>
        /// Gets the linked object references that exist. Return <c>null</c> to skip this check.
        /// </summary>
        IReadOnlyCollection<(Guid LinkedObjectID, string Scope)> GetExistingLinkedObjects(IReadOnlyCollection<(Guid LinkedObjectID, string Scope)> linkedObjects);
    }
}
