namespace SDM.AssetManagement.Tests.Setup
{
    using System;
    using System.Collections.Generic;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;

    /// <summary>
    /// Port Types require at least one existing Cable Type. These helpers create one and return a ready-to-use reference list.
    /// </summary>
    public static class PortTypeCableHelper
    {
        public static CableType CreateCableType(this ITestApiHelper helper, string name = "Test Cable Type")
        {
            if (helper == null)
            {
                throw new ArgumentNullException(nameof(helper));
            }

            return helper.AssetManagement.CableTypes.Create(CreateCableTypeModel(name));
        }

        public static List<SdmObjectReference<CableType>> CreateCableTypeReferences(this ITestApiHelper helper, string name = "Test Cable Type")
        {
            var cableType = helper.CreateCableType(name);

            return new List<SdmObjectReference<CableType>> { new SdmObjectReference<CableType>(cableType.Identifier) };
        }

        internal static CableType CreateCableTypeModel(string name, string? identifier = null)
        {
            return new CableType
            {
                Identifier = identifier ?? Guid.NewGuid().ToString(),
                Name = name,
                CategoryLinks = new CategoryRelation
                {
                    Categories = new List<SlcAsset_Management.Enums.CategoriesEnum> { SlcAsset_Management.Enums.CategoriesEnum.Data },
                },
            };
        }
    }
}
