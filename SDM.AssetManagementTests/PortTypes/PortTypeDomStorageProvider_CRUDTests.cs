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
                CategoryLinks =
                {
                    Categories = new List<SlcAsset_Management.Enums.CategoriesEnum>
                    {
                        SlcAsset_Management.Enums.CategoriesEnum.Networking,
                        SlcAsset_Management.Enums.CategoriesEnum.Data,
                    },
                },
                CableFKs =
                {
                    CableTypeFks = Helper.CreateCableTypeReferences("Reference Cable Type"),
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
        public void PortTypeDomStorageProvider_Create_WithNonExistingCableType_ShouldFail()
        {
            var missingId = Guid.NewGuid().ToString();
            referencePortType.CableFKs.CableTypeFks.Add(new SdmObjectReference<CableType>(missingId));

            Action act = () => Helper.AssetManagement.PortTypes.Create(referencePortType);

            act.Should().Throw<Exception>()
                .WithMessage($"*Referenced Cable Type '{missingId}' does not exist*");
        }

        [TestMethod]
        public void PortTypeDomStorageProvider_Create_WithExistingCableType_ShouldPass()
        {
            var cableType = Helper.AssetManagement.CableTypes.Create(new CableType
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = "Existing Port Type Cable",
                CategoryLinks = new CategoryRelation
                {
                    Categories = new List<SlcAsset_Management.Enums.CategoriesEnum> { SlcAsset_Management.Enums.CategoriesEnum.Data },
                },
            });
            referencePortType.CableFKs.CableTypeFks.Add(new SdmObjectReference<CableType>(cableType.Identifier));

            Helper.AssetManagement.PortTypes.Create(referencePortType);

            AssertCreated();
        }

        [TestMethod]
        public void PortTypeDomStorageProvider_Create_WithoutCables_ShouldFail()
        {
            var portType = new PortType
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = "Port Type Without Cables",
                CategoryLinks =
                {
                    Categories = new List<SlcAsset_Management.Enums.CategoriesEnum> { SlcAsset_Management.Enums.CategoriesEnum.Data },
                },
            };

            Action act = () => Helper.AssetManagement.PortTypes.Create(portType);

            act.Should().Throw<Exception>().WithMessage("*Port Type must have at least one cable type*");
            Helper.AssetManagement.PortTypes.Count(new TRUEFilterElement<PortType>()).Should().Be(0);
        }

        [TestMethod]
        public void PortTypeDomStorageProvider_Create_WithEmptyCableTypeList_ShouldFail()
        {
            referencePortType.CableFKs.CableTypeFks = new List<SdmObjectReference<CableType>>();

            Action act = () => Helper.AssetManagement.PortTypes.Create(referencePortType);

            act.Should().Throw<Exception>().WithMessage("*Port Type must have at least one cable type*");
        }

        [TestMethod]
        public void PortTypeDomStorageProvider_Create_WithoutCablesAndCategories_ShouldReportBoth()
        {
            var portType = new PortType
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = "Port Type Without Cables And Categories",
            };

            Action act = () => Helper.AssetManagement.PortTypes.Create(portType);

            act.Should().Throw<Exception>()
                .Where(e => e.Message.Contains("Port Type must have at least one category") && e.Message.Contains("Port Type must have at least one cable type"));
        }

        [TestMethod]
        public void PortTypeDomStorageProvider_Update_RemovingAllCableTypes_ShouldFail()
        {
            Helper.AssetManagement.PortTypes.Create(referencePortType);
            var persisted = Helper.AssetManagement.PortTypes.Read(PortTypeExposers.Identifier.Equal(referencePortType.Identifier)).Single();
            persisted.CableFKs.CableTypeFks = new List<SdmObjectReference<CableType>>();

            Action act = () => Helper.AssetManagement.PortTypes.Update(persisted);

            act.Should().Throw<Exception>().WithMessage("*Port Type must have at least one cable type*");
        }

        [TestMethod]
        public void PortTypeDomStorageProvider_Create_WithoutCategories_ShouldFail()
        {
            var portType = new PortType
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = "Port Type Without Categories",
            };

            Action act = () => Helper.AssetManagement.PortTypes.Create(portType);

            act.Should().Throw<Exception>().WithMessage("*Port Type must have at least one category*");
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
                CategoryLinks =
                {
                    Categories = new List<SlcAsset_Management.Enums.CategoriesEnum>
                    {
                        SlcAsset_Management.Enums.CategoriesEnum.Power,
                        SlcAsset_Management.Enums.CategoriesEnum.Video,
                    },
                },
                CableFKs =
                {
                    CableTypeFks = Helper.CreateCableTypeReferences("Updated Cable Type"),
                },
            };

            // Act
            Helper.AssetManagement.PortTypes.CreateOrUpdate([updatedPortType]);

            // Assert
            var persisted = Helper.AssetManagement.PortTypes.Read(new TRUEFilterElement<PortType>()).First();
            AssertPortTypeUpdateDifferences(referencePortType, persisted, updatedPortType);
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
            Helper.AssetManagement.DataPorts.Delete(Helper.AssetManagement.DataPorts.Read(new TRUEFilterElement<DataPort>()).ToList());
            Helper.AssetManagement.PowerPorts.Delete(Helper.AssetManagement.PowerPorts.Read(new TRUEFilterElement<PowerPort>()).ToList());

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
            Helper.AssetManagement.DataPorts.Delete(Helper.AssetManagement.DataPorts.Read(new TRUEFilterElement<DataPort>()).ToList());
            Helper.AssetManagement.PowerPorts.Delete(Helper.AssetManagement.PowerPorts.Read(new TRUEFilterElement<PowerPort>()).ToList());

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

        private static void AssertPortTypeUpdateDifferences(PortType original, PortType updated, PortType expected)
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
                updated.CableFKs.CableTypeFks.Select(fk => fk.Identifier).Should()
                    .NotBeEquivalentTo(original.CableFKs.CableTypeFks.Select(fk => fk.Identifier));
                updated.CableFKs.CableTypeFks.Select(fk => fk.Identifier).Should()
                    .BeEquivalentTo(expected.CableFKs.CableTypeFks.Select(fk => fk.Identifier));
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
                created.CableFKs.CableTypeFks.Select(fk => fk.Identifier).Should()
                    .BeEquivalentTo(referencePortType.CableFKs.CableTypeFks.Select(fk => fk.Identifier));
            }
        }

        #endregion
    }
}
