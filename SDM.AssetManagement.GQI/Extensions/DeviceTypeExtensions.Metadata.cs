namespace Skyline.DataMiner.SDM.AssetManagement.Common.Extensions
{
    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Extensions;

    public static partial class DeviceTypeExtensions
    {
        public static ObjectRefMetadata GetObjectRefMetadata(this SdmObject<DeviceType> obj)
        {
            return new ObjectRefMetadata
            {
                Object = new DomInstanceId(obj.GetIdentifierAsGuid())
                {
                    ModuleId = SharedMappers.DomIds.SlcAsset_Management.ModuleId
                }
            };
        }
    }
}