namespace Skyline.DataMiner.SDM.FacilityManagement.Common.Extensions
{
    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM.Extensions;

    public static partial class ZoneExtensions
    {
        public static ObjectRefMetadata GetObjectRefMetadata(this SdmObject<Zone> obj)
        {
            return new ObjectRefMetadata
            {
                Object = new DomInstanceId(obj.GetIdentifierAsGuid())
                {
                    ModuleId = SharedMappers.DomIds.SlcFacility_Management.ModuleId
                }
            };
        }
    }
}