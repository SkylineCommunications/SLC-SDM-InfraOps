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
    using Skyline.DataMiner.SDM.Extensions;

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
                CategoryLinks =
                {
                    Categories = new List<SlcAsset_Management.Enums.CategoriesEnum> { SlcAsset_Management.Enums.CategoriesEnum.Data },
                },
                CableFKs =
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

        [TestMethod]
        public void PortTypeDomStorageProvider_NestedReadFilter_CableTypeFks_NotContains()
        {
            var targetCableType = Helper.AssetManagement.CableTypes.Create(new CableType
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = "Target Cable Type",
                CategoryLinks = new CategoryRelation
                {
                    Categories = new List<SlcAsset_Management.Enums.CategoriesEnum> { SlcAsset_Management.Enums.CategoriesEnum.Data },
                },
            });
            var otherCableType = Helper.AssetManagement.CableTypes.Create(new CableType
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = "Other Cable Type",
                CategoryLinks = new CategoryRelation
                {
                    Categories = new List<SlcAsset_Management.Enums.CategoriesEnum> { SlcAsset_Management.Enums.CategoriesEnum.Power },
                },
            });

            var targetCableTypeFk = new SdmObjectReference<CableType>(targetCableType.Identifier);
            var otherCableTypeFk = new SdmObjectReference<CableType>(otherCableType.Identifier);
            var portTypes = new[]
            {
                new PortType
                {
                    Identifier = Guid.NewGuid().ToString(),
                    Name = "Port Type Without Cable FK",
                    CategoryLinks = { Categories = new List<SlcAsset_Management.Enums.CategoriesEnum> { SlcAsset_Management.Enums.CategoriesEnum.Data } },
                    CableFKs = { CableTypeFks = new List<SdmObjectReference<CableType>>() },
                },
                new PortType
                {
                    Identifier = Guid.NewGuid().ToString(),
                    Name = "Port Type With Target Cable FK",
                    CategoryLinks = { Categories = new List<SlcAsset_Management.Enums.CategoriesEnum> { SlcAsset_Management.Enums.CategoriesEnum.Data } },
                    CableFKs = { CableTypeFks = new List<SdmObjectReference<CableType>> { targetCableTypeFk } },
                },
                new PortType
                {
                    Identifier = Guid.NewGuid().ToString(),
                    Name = "Port Type With Other Cable FK",
                    CategoryLinks = { Categories = new List<SlcAsset_Management.Enums.CategoriesEnum> { SlcAsset_Management.Enums.CategoriesEnum.Power } },
                    CableFKs = { CableTypeFks = new List<SdmObjectReference<CableType>> { otherCableTypeFk } },
                },
            };

            Helper.AssetManagement.PortTypes.Create(portTypes);

            var filter = PortTypeExposers.CableFKs.CableTypeFks.NotContains(targetCableTypeFk);
            var expected = portTypes.Where(pt => pt.CableFKs.CableTypeFks.Any() && !pt.CableFKs.CableTypeFks.Any(fk => fk != null && fk.HasValue() && fk.Identifier == targetCableTypeFk.Identifier)).ToArray();

            var results = Helper.AssetManagement.PortTypes.Read(filter).ToList();

            using (new AssertionScope())
            {
                results.Should().NotBeNull();
                results.Should().HaveCount(expected.Length);
                results.Should().BeEquivalentTo(expected);
            }
        }

        [TestMethod]
        public void PortTypeDomStorageProvider_NestedReadFilter_CableTypeFks_NonExistent()
        {
            Helper.AssetManagement.PortTypes.Create(new PortType
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = "Port Type Without Matching Cable Type",
                CategoryLinks =
                {
                    Categories = new List<SlcAsset_Management.Enums.CategoriesEnum> { SlcAsset_Management.Enums.CategoriesEnum.Data },
                },
            });

            var filter = PortTypeExposers.CableFKs.CableTypeFks.Contains(new SdmObjectReference<CableType>(Guid.NewGuid().ToString()));

            var results = Helper.AssetManagement.PortTypes.Read(filter).ToList();

            using (new AssertionScope())
            {
                results.Should().NotBeNull();
                results.Should().BeEmpty();
                results.Should().HaveCount(0);
            }
        }

        [TestMethod]
        public void PortTypeDomStorageProvider_NestedReadFilter_CableTypeFks_WithValue()
        {
            var targetCableType = Helper.AssetManagement.CableTypes.Create(new CableType
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = "Present Cable Type",
                CategoryLinks = new CategoryRelation
                {
                    Categories = new List<SlcAsset_Management.Enums.CategoriesEnum> { SlcAsset_Management.Enums.CategoriesEnum.Data },
                },
            });

            var targetCableTypeFk = new SdmObjectReference<CableType>(targetCableType.Identifier);
            var portTypes = new[]
            {
                new PortType
                {
                    Identifier = Guid.NewGuid().ToString(),
                    Name = "Port Type Empty Cable FKs",
                    CategoryLinks = { Categories = new List<SlcAsset_Management.Enums.CategoriesEnum> { SlcAsset_Management.Enums.CategoriesEnum.Data } },
                    CableFKs = { CableTypeFks = new List<SdmObjectReference<CableType>>() },
                },
                new PortType
                {
                    Identifier = Guid.NewGuid().ToString(),
                    Name = "Port Type Matching Cable FK",
                    CategoryLinks = { Categories = new List<SlcAsset_Management.Enums.CategoriesEnum> { SlcAsset_Management.Enums.CategoriesEnum.Data } },
                    CableFKs = { CableTypeFks = new List<SdmObjectReference<CableType>> { targetCableTypeFk } },
                },
            };

            Helper.AssetManagement.PortTypes.Create(portTypes);

            var filter = PortTypeExposers.CableFKs.CableTypeFks.Contains(targetCableTypeFk);
            var expected = portTypes.Where(pt => pt.CableFKs.CableTypeFks.Any(fk => fk != null && fk.HasValue() && fk.Identifier == targetCableTypeFk.Identifier)).ToArray();

            var results = Helper.AssetManagement.PortTypes.Read(filter).ToList();

            using (new AssertionScope())
            {
                results.Should().NotBeNull();
                results.Should().HaveCount(expected.Length);
                results.Should().BeEquivalentTo(expected);
            }
        }

        #endregion
    }
}