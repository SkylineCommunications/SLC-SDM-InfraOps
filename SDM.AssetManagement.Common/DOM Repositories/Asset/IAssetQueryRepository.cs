namespace Skyline.DataMiner.SDM.AssetManagement.Repositories
{
    using System.Collections.Generic;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.AssetManagement.Models;

    public interface IAssetQueryRepository
    {
        long Count(FilterElement<Asset> filter);
        IEnumerable<Asset> Read(FilterElement<Asset> filter);
    }
}