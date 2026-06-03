
namespace Skyline.DataMiner.SDM.AssetManagement.Common.Models
{
    using System;

    [GenerateExposers]
    [SdmDomStorage("dod")]
    public class TestModel : SdmObject<TestModel>
    {
        public int? MyInteger { get; set; }

        public GuardStatus? MyEnum { get; set; }

        public Guid? MyGuid { get; set; }

        public DateTime? MyDateTime { get; set; }
    }

    public enum GuardStatus
    {
        TODO,
        DONE,
        SKIPPED,
    }
}
