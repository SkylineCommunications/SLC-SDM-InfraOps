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

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;

    /// <summary>
    /// CRUD tests for PortType repository operations.
    /// </summary>
    [TestClass]
    public partial class PortTypeDomStorageProviderTests : BaseRepositoryTest
    {
        private PortType referencePortType = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            referencePortType = new PortType
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = "Test PortType",
                Description = "Test Description",
                CategoryLinks = new CategoryRelation
                {
                    Categories = new List<SlcAsset_Management.Enums.CategoriesEnum>
                    {
                        SlcAsset_Management.Enums.CategoriesEnum.Networking,
                        SlcAsset_Management.Enums.CategoriesEnum.Data,
                    },
                },
                CableFKs = new CableRelation
                {
                    CableTypeFks = new List<SdmObjectReference<CableType>>
                    {
                        new SdmObjectReference<CableType>(Guid.NewGuid().ToString()),
                    },
                },
            };
        }

        #region Create Tests

        [TestMethod]
        public void PortTypeDomStorageProvider_EmptyDOM_Create()
        {
            // Act
            Helper.AssetManagement.PortTypes.Create(referencePortType);

            // Assert
            AssertCreated();
        }

        [TestMethod]
        public void PortTypeDomStorageProvider_EmptyDOM_CreateOrUpdate_Create()
        {
            // Act
            Helper.AssetManagement.PortTypes.CreateOrUpdate([referencePortType]);

            // Assert
            AssertCreated();
        }

        [TestMethod]
        public void PortTypeDomStorageProvider_EmptyDOM_CreateOrUpdate_Update()
        {
            // Arrange
            Helper.AssetManagement.PortTypes.Create(referencePortType);

            var updatedPortType = new PortType
            {
                Identifier = referencePortType.Identifier,
                Name = "Updated PortType Name",
                Description = "Updated Description",
                CategoryLinks = new CategoryRelation
                {
                    Categories = new List<SlcAsset_Management.Enums.CategoriesEnum>
                    {
                        SlcAsset_Management.Enums.CategoriesEnum.Power,
                        SlcAsset_Management.Enums.CategoriesEnum.Video,
                    },
                },
                CableFKs = new CableRelation
                {
                    CableTypeFks = new List<SdmObjectReference<CableType>>
                    {
                        new SdmObjectReference<CableType>(Guid.NewGuid().ToString()),
                        new SdmObjectReference<CableType>(Guid.NewGuid().ToString()),
                    },
                },
            };

            // Act
            Helper.AssetManagement.PortTypes.CreateOrUpdate([updatedPortType]);

            // Assert
            var persisted = Helper.AssetManagement.PortTypes.Read(new TRUEFilterElement<PortType>()).First();
            AssertPortTypeUpdateDifferences(referencePortType, persisted);
        }

        #endregion

        #region Read Tests

        [TestMethod]
        public void PortTypeDomStorageProvider_ReadPaged()
        {
            // Arrange
            const int pageSize = 3;

            Helper.PopulateWithDemoData(upTo: DemoDataLayer.PortTypes);

            var allFilter = new TRUEFilterElement<PortType>();
            var totalCount = Helper.TestData.PortTypes.Count;

            // Act
            var pagedResult = Helper.AssetManagement.PortTypes.ReadPaged(allFilter, pageSize);

            // Assert
            using (new AssertionScope())
            {
                pagedResult.Should().NotBeNull();
                pagedResult.Should().HaveCountGreaterOrEqualTo((int)(totalCount / pageSize),
                    "should have at least the expected number of pages");
                pagedResult.Should().AllSatisfy(page =>
                    page.Should().HaveCountLessOrEqualTo(pageSize),
                    "each page should not exceed page size");
            }
        }

        #endregion

        #region Delete Tests

        [TestMethod]
        public void PortTypeDomStorageProvider_DeleteBulk()
        {
            // Arrange
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.PortTypes);

            var initialCount = Helper.TestData.PortTypes.Count;

            var filter = new ORFilterElement<PortType>(
                PortTypeExposers.Name.Equal("Port Type 3"),
                PortTypeExposers.Name.Equal("Port Type 7"));

            var portTypesToDelete = Helper.AssetManagement.PortTypes.Read(filter).ToList();
            var deleteCount = portTypesToDelete.Count;

            // Act
            Helper.AssetManagement.PortTypes.Delete(portTypesToDelete);

            // Assert
            using (new AssertionScope())
            {
                Helper.AssetManagement.PortTypes.Count(new TRUEFilterElement<PortType>())
                    .Should().Be(initialCount - deleteCount, $"{deleteCount} port types should be deleted");

                Helper.AssetManagement.PortTypes.Count(PortTypeExposers.Name.Equal("Port Type 3"))
                    .Should().Be(0, "Port Type 3 should be deleted");

                Helper.AssetManagement.PortTypes.Count(PortTypeExposers.Name.Equal("Port Type 7"))
                    .Should().Be(0, "Port Type 7 should be deleted");
            }
        }

        [TestMethod]
        public void PortTypeDomStorageProvider_EmptyDOM_DeleteSingle()
        {
            // Arrange
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.PortTypes);

            var initialCount = Helper.TestData.PortTypes.Count;
            var portTypeToDelete = Helper.AssetManagement.PortTypes
                .Read(PortTypeExposers.Name.Equal("Port Type 3"))
                .First();

            // Act
            Helper.AssetManagement.PortTypes.Delete(portTypeToDelete);

            // Assert
            using (new AssertionScope())
            {
                Helper.AssetManagement.PortTypes.Count(new TRUEFilterElement<PortType>())
                    .Should().Be(initialCount - 1, "one port type should be deleted");

                Helper.AssetManagement.PortTypes.Count(PortTypeExposers.Identifier.Equal(portTypeToDelete.Identifier))
                    .Should().Be(0, "deleted port type should not exist");
            }
        }

        #endregion

        #region Assertion Helpers

        private static void AssertPortTypeUpdateDifferences(PortType original, PortType updated)
        {
            using (new AssertionScope())
            {
                // Identifiers remain the same
                updated.Identifier.Should().Be(original.Identifier);

                // Updated fields
                updated.Name.Should().Be("Updated PortType Name");
                updated.Description.Should().Be("Updated Description");

                // CategoryLinks changes
                updated.CategoryLinks.Categories.Should().NotBeEquivalentTo(original.CategoryLinks.Categories);
                updated.CategoryLinks.Categories.Should().BeEquivalentTo(new List<SlcAsset_Management.Enums.CategoriesEnum>
                {
                    SlcAsset_Management.Enums.CategoriesEnum.Power,
                    SlcAsset_Management.Enums.CategoriesEnum.Video,
                });

                // CableFKs changes
                updated.CableFKs.CableTypeFks.Should().HaveCount(2);
            }
        }

        private void AssertCreated()
        {
            using (new AssertionScope())
            {
                Helper.AssetManagement.PortTypes.Count(new TRUEFilterElement<PortType>()).Should().Be(1);

                var created = Helper.AssetManagement.PortTypes.Read(new TRUEFilterElement<PortType>()).First();

                // Basic properties
                created.Should().NotBeNull();
                created.Name.Should().Be(referencePortType.Name);
                created.Description.Should().Be(referencePortType.Description);

                // CategoryLinks
                created.CategoryLinks.Should().NotBeNull();
                created.CategoryLinks.Categories.Should().BeEquivalentTo(referencePortType.CategoryLinks.Categories);

                // CableFKs
                created.CableFKs.Should().NotBeNull();
                created.CableFKs.CableTypeFks.Should().HaveCount(referencePortType.CableFKs.CableTypeFks.Count);
            }
        }

        #endregion
    }
}
