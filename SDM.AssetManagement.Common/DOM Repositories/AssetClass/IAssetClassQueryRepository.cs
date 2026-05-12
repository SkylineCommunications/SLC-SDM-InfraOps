namespace Skyline.DataMiner.SDM.AssetManagement.Repositories
{
    using System.Collections.Generic;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.AssetManagement.Models;

    public interface IAssetClassQueryRepository
    {
        long Count(FilterElement<AssetClass> filter);
        IEnumerable<AssetClass> Read(FilterElement<AssetClass> filter);
    }
}
