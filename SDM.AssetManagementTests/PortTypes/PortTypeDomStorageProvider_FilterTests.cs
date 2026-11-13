namespace SDM.AssetManagement.Tests.PortTypes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using FluentAssertions.Execution;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SDM.AssetManagement.Tests.Setup;
    using SharedMappers.DomIds;
    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Extensions;
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
                    Name = "Port Type With Other Cable FK (Data)",
                    CategoryLinks = { Categories = new List<SlcAsset_Management.Enums.CategoriesEnum> { SlcAsset_Management.Enums.CategoriesEnum.Data } },
                    CableFKs = { CableTypeFks = new List<SdmObjectReference<CableType>> { otherCableTypeFk } },
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
                CableFKs =
                {
                    CableTypeFks = Helper.CreateCableTypeReferences("Unrelated Cable Type"),
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
            var otherCableTypeFk = new SdmObjectReference<CableType>(Helper.CreateCableType("Other Present Cable Type").Identifier);
            var portTypes = new[]
            {
                new PortType
                {
                    Identifier = Guid.NewGuid().ToString(),
                    Name = "Port Type Other Cable FK",
                    CategoryLinks = { Categories = new List<SlcAsset_Management.Enums.CategoriesEnum> { SlcAsset_Management.Enums.CategoriesEnum.Data } },
                    CableFKs = { CableTypeFks = new List<SdmObjectReference<CableType>> { otherCableTypeFk } },
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

        #region Data/Power Port Type Filters

        [TestMethod]
        public void PortTypeDomStorageProvider_ReadDataPortTypes_MatchesIsDataPortType()
        {
            // Arrange
            var portTypes = CreateCategoryMatrixPortTypes();

            // Act
            var results = Helper.AssetManagement.PortTypes.ReadDataPortTypes().ToList();
            var expected = Helper.AssetManagement.PortTypes.Read(new TRUEFilterElement<PortType>()).Where(pt => pt.IsDataPortType()).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().BeEquivalentTo(expected);
                results.Select(pt => pt.Identifier).Should().BeEquivalentTo(new[]
                {
                    portTypes["Networking"].Identifier,
                    portTypes["NoCategories"].Identifier,
                    portTypes["Data"].Identifier,
                });
                results.Should().NotContain(pt => pt.Identifier == portTypes["Power"].Identifier, "Power-only port types are not data port types");
                results.Should().NotContain(pt => pt.Identifier == portTypes["Power+Data"].Identifier, "port types with the Power category are not data port types");
            }
        }

        [TestMethod]
        public void PortTypeDomStorageProvider_ReadDataPortTypes_DemoData_MatchesIsDataPortType()
        {
            // Arrange
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.PortTypes);

            // Act
            var results = Helper.AssetManagement.PortTypes.ReadDataPortTypes().ToList();
            var expected = Helper.AssetManagement.PortTypes.Read(new TRUEFilterElement<PortType>()).Where(pt => pt.IsDataPortType()).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().NotBeEmpty();
                results.Should().BeEquivalentTo(expected);
            }
        }

        [TestMethod]
        public void PortTypeDomStorageProvider_ReadPowerPortTypes_MatchesIsPowerPortType()
        {
            // Arrange
            var portTypes = CreateCategoryMatrixPortTypes();

            // Act
            var results = Helper.AssetManagement.PortTypes.ReadPowerPortTypes().ToList();
            var expected = Helper.AssetManagement.PortTypes.Read(new TRUEFilterElement<PortType>()).Where(pt => pt.IsPowerPortType()).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().BeEquivalentTo(expected);
                results.Select(pt => pt.Identifier).Should().BeEquivalentTo(new[]
                {
                    portTypes["Power"].Identifier,
                    portTypes["Power+Data"].Identifier,
                });
            }
        }

        [TestMethod]
        public void PortTypeDomStorageProvider_ReadFilter_WithoutCategories()
        {
            // Arrange
            var portTypes = CreateCategoryMatrixPortTypes();

            // Act
            var results = Helper.AssetManagement.PortTypes.Read(PortTypeFilters.WithoutCategories()).ToList();

            // Assert
            results.Select(pt => pt.Identifier).Should().BeEquivalentTo(new[] { portTypes["NoCategories"].Identifier });
        }

        [TestMethod]
        public void PortTypeDomStorageProvider_ReadFilter_DataPortTypes_CombinedWithOtherFilter()
        {
            // Arrange
            var portTypes = CreateCategoryMatrixPortTypes();
            var filter = PortTypeFilters.DataPortTypes().AND(PortTypeExposers.Name.Equal(portTypes["Networking"].Name));

            // Act
            var results = Helper.AssetManagement.PortTypes.Read(filter).ToList();

            // Assert
            results.Select(pt => pt.Identifier).Should().BeEquivalentTo(new[] { portTypes["Networking"].Identifier });
        }

        private Dictionary<string, PortType> CreateCategoryMatrixPortTypes()
        {
            var cableTypeFks = Helper.CreateCableTypeReferences("Category Matrix Cable Type");
            var portTypes = new Dictionary<string, PortType>
            {
                ["Power"] = NewPortType("Category Matrix Power", cableTypeFks, SlcAsset_Management.Enums.CategoriesEnum.Power),
                ["Power+Data"] = NewPortType("Category Matrix Power+Data", cableTypeFks, SlcAsset_Management.Enums.CategoriesEnum.Power, SlcAsset_Management.Enums.CategoriesEnum.Data),
                ["Networking"] = NewPortType("Category Matrix Networking", cableTypeFks, SlcAsset_Management.Enums.CategoriesEnum.Networking),
                ["NoCategories"] = NewPortType("Category Matrix No Categories", cableTypeFks, SlcAsset_Management.Enums.CategoriesEnum.Data),
                ["Data"] = NewPortType("Category Matrix Data", cableTypeFks, SlcAsset_Management.Enums.CategoriesEnum.Data),
            };

            Helper.AssetManagement.PortTypes.Create(portTypes.Values);

            // Validation requires at least one category, so legacy port types without categories are simulated by stripping the section in DOM.
            RemoveCategoriesInDom(portTypes["NoCategories"].Identifier);

            return portTypes;
        }

        private void RemoveCategoriesInDom(string portTypeId)
        {
            var dom = new DomHelper(((TestApiHelper)Helper).Connection.HandleMessages, SlcAsset_Management.ModuleId);
            var instance = dom.DomInstances.Read(DomInstanceExposers.Id.Equal(Guid.Parse(portTypeId))).Single();
            instance.Sections.RemoveAll(section => section.SectionDefinitionID.Equals(SlcAsset_Management.Sections.CategoryLink.Id));
            dom.DomInstances.Update(instance);

            Helper.AssetManagement.PortTypes.Read(PortTypeExposers.Identifier.Equal(portTypeId)).Single()
                .CategoryLinks.Categories.Should().BeEmpty("the categories should have been removed directly in DOM");
        }

        private static PortType NewPortType(string name, List<SdmObjectReference<CableType>> cableTypeFks, params SlcAsset_Management.Enums.CategoriesEnum[] categories)
        {
            return new PortType
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = name,
                CategoryLinks = { Categories = categories.ToList() },
                CableFKs = { CableTypeFks = cableTypeFks.ToList() },
            };
        }

        #endregion
    }
}