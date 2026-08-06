namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using System;
    using System.Collections.Generic;

    public interface IAssetManagementExternalReferenceChecker
    {
        IReadOnlyCollection<Guid> GetExistingIdentifiers(
            AssetManagementExternalReferenceType entityType,
            IReadOnlyCollection<Guid> identifiers);
    }
}
