namespace Skyline.DataMiner.SDM.PlanAndBuild.Common.Extensions
{
    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.SDM.PlanAndBuild.Models;
    using Skyline.DataMiner.SDM.Extensions;

    public static partial class JobTypeExtensions
    {
        public static ObjectRefMetadata GetObjectRefMetadata(this SdmObject<JobType> obj)
        {
            return new ObjectRefMetadata
            {
                Object = new DomInstanceId(obj.GetIdentifierAsGuid())
                {
                    ModuleId = SharedMappers.DomIds.SlcPlan_And_Build.ModuleId
                }
            };
        }
    }
}