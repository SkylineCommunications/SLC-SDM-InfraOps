namespace Skyline.DataMiner.SDM.InfraOpsProperties.Common.Extensions
{
    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.SDM.InfraOpsProperties.Models;
    using Skyline.DataMiner.SDM.Extensions;

    public static partial class PropertyExtensions
    {
        public static ObjectRefMetadata GetObjectRefMetadata(this SdmObject<Property> obj)
        {
            return new ObjectRefMetadata
            {
                Object = new DomInstanceId(obj.GetIdentifierAsGuid())
                {
                    ModuleId = SharedMappers.DomIds.InfraopsProperties.ModuleId
                }
            };
        }
    }
}