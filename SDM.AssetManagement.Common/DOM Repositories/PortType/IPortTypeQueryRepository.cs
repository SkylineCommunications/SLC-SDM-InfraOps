namespace Skyline.DataMiner.SDM.AssetManagement.Repositories
{
    using System.Collections.Generic;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.AssetManagement.Models;

    public interface IPortTypeQueryRepository
    {
        long Count(FilterElement<PortType> filter);
        IEnumerable<PortType> Read(FilterElement<PortType> filter);
    }
}