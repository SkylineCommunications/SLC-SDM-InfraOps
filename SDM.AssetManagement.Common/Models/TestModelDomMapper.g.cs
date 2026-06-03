using System;
using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
using Skyline.DataMiner.Net.Sections;
using Skyline.DataMiner.SDM;

namespace Skyline.DataMiner.SDM.AssetManagement.Common.Models
{
    [SdmDomMapper]
    public static class TestModelDomMapper
    {
        internal const string ModuleId = "dod";
        private static readonly DomDefinitionId TestModelDomDefinitionId = new DomDefinitionId(new Guid("0fa9776f-b9c2-44b3-a8a9-f199b095960f"))
        {ModuleId = ModuleId};
        internal static DomDefinitionId DomDefinitionId => TestModelDomDefinitionId;
        public static class TestModelProperties
        {
            private static readonly SectionDefinitionID sectionDefinitionId = new SectionDefinitionID(new Guid("17585aa5-20ab-486e-b9d8-f4b995509621"))
            {ModuleId = ModuleId};
            internal static SectionDefinitionID SectionDefinitionId => sectionDefinitionId;
            public static readonly FieldDescriptorID MyInteger = new FieldDescriptorID(new Guid("89489249-3401-4994-a64e-4e9d647a95e6"));
            public static readonly FieldDescriptorID MyEnum = new FieldDescriptorID(new Guid("dc04b2aa-2b6a-486c-8cb7-eb71baa63d75"));
            public static readonly FieldDescriptorID MyGuid = new FieldDescriptorID(new Guid("0ee92017-2a1e-479e-9cf5-147b0ef8c2a8"));
            public static readonly FieldDescriptorID MyDateTime = new FieldDescriptorID(new Guid("55ff3199-ffd6-4f2c-b2e4-a6390b06e74b"));
        }
    }
}