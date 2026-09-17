namespace Skyline.DataMiner.SDM.Extensions
{
    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.SDM;

    public static class SdmObjectMetadataExtensions
    {
        public static ObjectRefMetadata GetObjectRefMetadata<T>(
            this SdmObject<T> obj,
            string moduleId)
            where T : SdmObject<T>
        {
            return new ObjectRefMetadata
            {
                Object = obj.GetObjectRefDomInstanceId(moduleId),
            };
        }
    }
}
