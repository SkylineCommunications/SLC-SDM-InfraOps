namespace Skyline.DataMiner.SDM.FacilityManagement.Common.Extensions
{
    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM.Extensions;

    public static partial class FacilityExtensions
    {
        public static ObjectRefMetadata GetObjectRefMetadata(this SdmObject<Facility> obj)
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