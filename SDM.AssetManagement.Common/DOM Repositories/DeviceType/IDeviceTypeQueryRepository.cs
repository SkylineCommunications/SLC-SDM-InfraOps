namespace Skyline.DataMiner.SDM.AssetManagement.Repositories
{
    using System.Collections.Generic;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.AssetManagement.Models;

    public interface IDeviceTypeQueryRepository
    {
        long Count(FilterElement<DeviceType> filter);
        IEnumerable<DeviceType> Read(FilterElement<DeviceType> filter);
    }
}