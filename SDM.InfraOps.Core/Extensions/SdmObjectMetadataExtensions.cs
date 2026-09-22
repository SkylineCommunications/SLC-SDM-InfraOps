namespace Skyline.DataMiner.SDM.Extensions
{
    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public static class SdmObjectMetadataExtensions
    {
        public static ObjectRefMetadata GetObjectRefMetadata<T>(
            this T obj)
            where T : SdmObject<T>, IReadOnlyModuleIdReferencer
        {
            return new ObjectRefMetadata
            {
                Object = new DomInstanceId(obj.GetIdentifierAsGuid())
                {
                    ModuleId = obj.ModuleId
                }
            };
        }
    }
}
