namespace SDM.AssetManagement.Tests.PortTypes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;

    /// <summary>
    /// Filter and query tests for PortType repository operations.
    /// </summary>
    public partial class PortTypeDomStorageProviderTests
    {
        #region Basic Field Filters

        [TestMethod]
        public void PortTypeDomStorageProvider_ReadFilter_Name_Equal()
        {
            // Arrange
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.PortTypes);

            var targetPortType = Helper.TestData.PortTypes.Skip(3).First();
            var filter = PortTypeExposers.Name.Equal(targetPortType.Name);

            // Act
            var results = Helper.AssetManagement.PortTypes.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().HaveCount(1, $"should find port type with name '{targetPortType.Name}'");
                var portType = results.First();
                portType.Name.Should().Be(targetPortType.Name);
                portType.Identifier.Should().Be(targetPortType.Identifier);
            }
        }

        [TestMethod]
        public void PortTypeDomStorageProvider_ReadFilter_Name_Contains()
        {
            // Arrange
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.PortTypes);

            const string namePattern = "Port Type";
            var filter = PortTypeExposers.Name.Contains(namePattern);

            // Act
            var results = Helper.AssetManagement.PortTypes.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().NotBeEmpty($"should find port types with '{namePattern}' in name");
                results.Should().OnlyContain(pt => pt.Name.Contains(namePattern),
                    "all results should contain 'Port Type' in name");
            }
        }

        [TestMethod]
        public void PortTypeDomStorageProvider_ReadFilter_Description_Contains()
        {
            // Arrange
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.PortTypes);

            const string descriptionPattern = "port type";
            var filter = PortTypeExposers.Description.Contains(descriptionPattern);

            // Act
            var results = Helper.AssetManagement.PortTypes.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().NotBeEmpty($"should find port types with '{descriptionPattern}' in description");
                results.Should().OnlyContain(pt => pt.Description.Contains(descriptionPattern));
            }
        }

        #endregion

        #region Nested Object Filters

        [TestMethod]
        public void PortTypeDomStorageProvider_NestedReadFilter_Categories_Contains()
        {
            // Arrange
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.PortTypes);

            var category = SlcAsset_Management.Enums.CategoriesEnum.Networking;
            var filter = PortTypeExposers.CategoryLinks.Categories.Contains(category);

            // Act
            var results = Helper.AssetManagement.PortTypes.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().NotBeEmpty($"should find port types with category '{category}'");
                results.Should().OnlyContain(pt => pt.CategoryLinks.Categories.Contains(category));
            }
        }

        [TestMethod]
        public void PortTypeDomStorageProvider_NestedReadFilter_Categories_NotContains()
        {
            // Arrange
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.PortTypes);

            var excludedCategory = SlcAsset_Management.Enums.CategoriesEnum.Broadcast;
            var filter = PortTypeExposers.CategoryLinks.Categories.NotContains(excludedCategory);

            // Act
            var results = Helper.AssetManagement.PortTypes.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().NotBeEmpty($"should find port types without category '{excludedCategory}'");
                results.Should().OnlyContain(pt => !pt.CategoryLinks.Categories.Contains(excludedCategory));
            }
        }

        [TestMethod]
        public void PortTypeDomStorageProvider_NestedReadFilter_CableTypeFks_Contains()
        {
            // Arrange
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.PortTypes);

            var cableType = Helper.AssetManagement.CableTypes.Create(new CableType
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = "Filter Cable Type",
                CategoryLinks = new CategoryRelation
                {
                    Categories = new List<SlcAsset_Management.Enums.CategoriesEnum> { SlcAsset_Management.Enums.CategoriesEnum.Data },
                },
            });
            var targetPortType = Helper.AssetManagement.PortTypes.Create(new PortType
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = "Port Type With Cable FK",
                CategoryLinks = new CategoryRelation
                {
                    Categories = new List<SlcAsset_Management.Enums.CategoriesEnum> { SlcAsset_Management.Enums.CategoriesEnum.Data },
                },
                CableFKs = new CableRelation
                {
                    CableTypeFks = new List<SdmObjectReference<CableType>> { new SdmObjectReference<CableType>(cableType.Identifier) },
                },
            });
            var targetCableTypeFk = targetPortType.CableFKs.CableTypeFks.First();
            var filter = PortTypeExposers.CableFKs.CableTypeFks.Contains(targetCableTypeFk);

            // Act
            var results = Helper.AssetManagement.PortTypes.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().NotBeEmpty("should find port types with the specified cable type FK");
                results.Should().OnlyContain(pt => pt.CableFKs.CableTypeFks.Contains(targetCableTypeFk));
            }
        }

        #endregion
    }
}
