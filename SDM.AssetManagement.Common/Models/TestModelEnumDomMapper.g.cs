using System;
using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
using Skyline.DataMiner.Net.Sections;
using Skyline.DataMiner.SDM;

namespace Skyline.DataMiner.SDM.AssetManagement.Common.Models
{
    [SdmDomMapper]
    public static class TestModelEnumDomMapper
    {
        internal const string ModuleId = "(slc)asset_management";
        private static readonly DomDefinitionId TestModelEnumDomDefinitionId = new DomDefinitionId(new Guid("91aaf782-9a91-4f7f-8589-400226e8b8c4"))
        {ModuleId = ModuleId};
        internal static DomDefinitionId DomDefinitionId => TestModelEnumDomDefinitionId;
        public static class TestModelEnumProperties
        {
            private static readonly SectionDefinitionID sectionDefinitionId = new SectionDefinitionID(new Guid("a85bbf74-d19a-4559-bd1e-3025cda9de48"))
            {ModuleId = ModuleId};
            internal static SectionDefinitionID SectionDefinitionId => sectionDefinitionId;
            public static readonly FieldDescriptorID ThisIsAnEnum = new FieldDescriptorID(new Guid("8054d1c3-ed46-439c-a83d-d4707bd4da73"));
        }
    }
}