namespace SDM.AssetManagement.Tests.CableTypes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SDM.AssetManagement.Tests.Setup;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.AssetManagement.Models;

    [TestClass]
    public class CableTypeDomStorageProvider_CategoryTests : BaseRepositoryTest
    {
        [TestMethod]
        public void CableTypeDomStorageProvider_Create_PersistsCategories()
        {
            var cableType = new CableType
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = "Cable With Categories",
                CategoryLinks =
                {
                    Categories = new List<SlcAsset_Management.Enums.CategoriesEnum>
                    {
                        SlcAsset_Management.Enums.CategoriesEnum.Networking,
                        SlcAsset_Management.Enums.CategoriesEnum.Data,
                    },
                },
            };

            Helper.AssetManagement.CableTypes.Create(cableType);

            var persisted = Helper.AssetManagement.CableTypes.Read(new TRUEFilterElement<CableType>()).Single();
            persisted.CategoryLinks.Categories.Should().BeEquivalentTo(cableType.CategoryLinks.Categories);
        }

        [TestMethod]
        public void CableTypeDomStorageProvider_Update_PersistsCategories()
        {
            var cableType = Helper.AssetManagement.CableTypes.Create(new CableType
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = "Cable To Update",
                CategoryLinks =
                {
                    Categories = new List<SlcAsset_Management.Enums.CategoriesEnum> { SlcAsset_Management.Enums.CategoriesEnum.Data },
                },
            });

            cableType.CategoryLinks.Categories = new List<SlcAsset_Management.Enums.CategoriesEnum> { SlcAsset_Management.Enums.CategoriesEnum.Power };
            Helper.AssetManagement.CableTypes.Update(cableType);

            var persisted = Helper.AssetManagement.CableTypes.Read(new TRUEFilterElement<CableType>()).Single();
            persisted.CategoryLinks.Categories.Should().BeEquivalentTo(new[] { SlcAsset_Management.Enums.CategoriesEnum.Power });
        }
    }
}
