namespace SDM.AssetManagement.Tests
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
    using Skyline.DataMiner.SDM.AssetManagement.Helpers;
    using Skyline.DataMiner.SDM.AssetManagement.Models;

    /// <summary>
    /// CRUD tests for Asset repository operations.
    /// </summary>
    [TestClass]
    public class AssetDomStorageProvider_CRUDTests : BaseRepositoryTest
    {
        private Asset referenceAsset;

        [TestInitialize]
        public void TestInitialize()
        {
            referenceAsset = new Asset
            {
                AssetID = Guid.NewGuid().ToString(),
                Name = "Test Asset",
                AssetClassId = null, // Will be set in tests
                Description = "Sample asset for unit test",
                FW_OS = "FW1.0",
                SerialNumber = "SN123456",
                HardwareVersion = "HW1.0",
                MacAddress = "00-14-22-01-23-45",
                PurchaseDate = DateTime.UtcNow.AddYears(-1),
                FirstUseDate = DateTime.UtcNow.AddMonths(-11),
                EndOfWarrantyDate = DateTime.UtcNow.AddYears(1),
                InstallationDate = DateTime.UtcNow.AddMonths(-10),
                InstallationUserId = Guid.NewGuid(),
                ModificationDate = DateTime.UtcNow,
                ModificationUserId = Guid.NewGuid(),
                EndOfLifeDate = DateTime.UtcNow.AddYears(5),
                Ownership = new AssetOwnership
                {
                    Organization = Guid.NewGuid(),
                    ContactPerson = Guid.NewGuid(),
                    ContactPersonRole = Guid.NewGuid(),
                    Team = Guid.NewGuid(),
                },
                Custody = new AssetCustody
                {
                    From = DateTime.UtcNow.AddMonths(-6),
                    Till = DateTime.UtcNow.AddMonths(6),
                    ContactPerson = Guid.NewGuid(),
                    Team = Guid.NewGuid(),
                    Organization = Guid.NewGuid(),
                    ContactPersonRole = Guid.NewGuid(),
                },
                Holders = new List<AssetHolder>
                {
                    new AssetHolder
                    {
                        SlotNumber = 4,
                        HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Chassis,
                    },
                    new AssetHolder
                    {
                        SlotNumber = 1,
                        HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Card,
                    },
                    new AssetHolder
                    {
                        SlotNumber = 3,
                        HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Fan,
                    },
                },
                ElementLinks = new List<ElementLink>
                {
                    new ElementLink
                    {
                        ElementID = "123/456",
                        IsPrimary = false,
                    },
                    new ElementLink
                    {
                        ElementID = "1845/2",
                    },
                },
            };
        }

        /// <summary>
        /// Ensures AssetClasses are populated and assigns the first one to the reference asset.
        /// </summary>
        private void PrepareReferenceAssetWithAssetClass()
        {
            Helper.PopulateWithDemoData(DemoDataLayer.AssetClasses);
            var assetClass = Helper.TestData.AssetClasses.First();
            referenceAsset.AssetClassId = new SdmObjectReference<AssetClass>(assetClass.Identifier);
        }

        #region Create Tests

        [TestMethod]
        public void Create_WithValidData_ShouldPersistAsset()
        {
            // Arrange
            PrepareReferenceAssetWithAssetClass();

            // Act
            Helper.AssetManagement.Assets.Create(referenceAsset);

            // Assert
            AssertCreated(Helper.AssetManagement);
        }

        [TestMethod]
        public void CreateOrUpdate_WithNewAsset_ShouldCreate()
        {
            // Arrange
            PrepareReferenceAssetWithAssetClass();

            // Act
            Helper.AssetManagement.Assets.CreateOrUpdate([referenceAsset]);

            // Assert
            AssertCreated(Helper.AssetManagement);
        }

        [TestMethod]
        public void CreateOrUpdate_WithExistingAsset_ShouldUpdate()
        {
            // Arrange
            PrepareReferenceAssetWithAssetClass();
            var created = Helper.AssetManagement.Assets.Create(referenceAsset);

            var updatedAsset = new Asset
            {
                Identifier = created.Identifier,
                AssetID = created.AssetID,
                Name = "Updated Asset Name",
                Description = "Updated description",
                HardwareVersion = "HW2.0",
                MacAddress = null, // MAC Address removed
                Location = new AssetLocation
                {
                    ParentAsset = created.Location.ParentAsset,
                    RoomId = created.Location.RoomId,
                    RackId = created.Location.RackId,
                    RackPosition = 12,
                    ContainerId = created.Location.ContainerId,
                    DeskId = created.Location.DeskId,
                    Side = SlcAsset_Management.Enums.SideEnum.Front,
                },
                PurchaseDate = DateTime.UtcNow.AddYears(-1),
                FirstUseDate = DateTime.UtcNow.AddMonths(-11),
                EndOfWarrantyDate = DateTime.UtcNow.AddYears(1),
                InstallationDate = DateTime.UtcNow.AddMonths(-10),
                InstallationUserId = Guid.NewGuid(),
                ModificationDate = DateTime.UtcNow,
                ModificationUserId = Guid.NewGuid(),
                EndOfLifeDate = DateTime.UtcNow.AddYears(5),
                Ownership = new AssetOwnership
                {
                    Organization = Guid.NewGuid(),
                },
                Custody = new AssetCustody
                {
                    From = DateTime.UtcNow.AddMonths(-6),
                    Till = DateTime.UtcNow.AddMonths(6),
                    ContactPerson = Guid.NewGuid(),
                    Team = Guid.NewGuid(),
                    Organization = Guid.NewGuid(),
                    ContactPersonRole = Guid.NewGuid(),
                },
                Holders = new List<AssetHolder>(),
                ElementLinks = new List<ElementLink>
                {
                    new ElementLink
                    {
                        ElementID = "100546/34",
                    },
                },
            };

            // Act
            Helper.AssetManagement.Assets.CreateOrUpdate([updatedAsset]);

            // Assert
            var persisted = Helper.AssetManagement.Assets.Read(new TRUEFilterElement<Asset>()).First();
            AssertAssetUpdateDifferences(referenceAsset, persisted);
        }

        #endregion

        #region Read Tests

        [TestMethod]
        public void ReadPaged_WithValidFilter_ShouldReturnPages()
        {
            // Arrange
            const int pageSize = 2;
            ;
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.Assets);

            var allFilter = new TRUEFilterElement<Asset>();
            var totalCount = Helper.TestData.Assets.Count;

            // Act
            var pagedResult = Helper.AssetManagement.Assets.ReadPaged(allFilter, pageSize);

            // Assert
            using (new AssertionScope())
            {
                pagedResult.Should().NotBeNull();
                pagedResult.Should().HaveCount((int)(totalCount / pageSize), "should have correct number of pages");
                pagedResult.Should().AllSatisfy(page => page.Should().HaveCount(pageSize), "each page should have correct size");
            }
        }

        #endregion

        #region Delete Tests

        [TestMethod]
        public void Delete_Single_ShouldRemoveAsset()
        {
            // Arrange
            ;
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.Assets);

            var initialCount = Helper.TestData.Assets.Count;
            var assetToDelete = Helper.AssetManagement.Assets
                .Read(AssetExposers.AssetName.Equal("Test Asset 3"))
                .First();

            // Act
            Helper.AssetManagement.Assets.Delete(assetToDelete);

            // Assert
            using (new AssertionScope())
            {
                Helper.AssetManagement.Assets.Count(new TRUEFilterElement<Asset>())
                    .Should().Be(initialCount - 1, "one asset should be deleted");

                Helper.AssetManagement.Assets.Count(AssetExposers.AssetId.Equal(assetToDelete.AssetID))
                    .Should().Be(0, "deleted asset should not exist");
            }
        }

        [TestMethod]
        public void Delete_Bulk_ShouldRemoveMultipleAssets()
        {
            // Arrange
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.Assets);

            var initialCount = Helper.TestData.Assets.Count;

            var filter = new ORFilterElement<Asset>(
                AssetExposers.AssetName.Equal("Test Asset 3"),
                AssetExposers.AssetDescription.Equal("Sample asset 7"));

            var assetsToDelete = Helper.AssetManagement.Assets.Read(filter).ToList();
            var deleteCount = assetsToDelete.Count;

            // Act
            Helper.AssetManagement.Assets.Delete(assetsToDelete);

            // Assert
            using (new AssertionScope())
            {
                Helper.AssetManagement.Assets.Count(new TRUEFilterElement<Asset>())
                    .Should().Be(initialCount - deleteCount, $"{deleteCount} assets should be deleted");

                Helper.AssetManagement.Assets.Count(AssetExposers.AssetName.Equal("Test Asset 3"))
                    .Should().Be(0, "Test Asset 3 should be deleted");

                Helper.AssetManagement.Assets.Count(AssetExposers.AssetDescription.Equal("Sample asset 7"))
                    .Should().Be(0, "asset with description 'Sample asset 7' should be deleted");
            }
        }

        #endregion

        #region Assertion Helpers

        private static void AssertAssetUpdateDifferences(Asset original, Asset updated)
        {
            using (new AssertionScope())
            {
                // Identifiers remain the same
                updated.AssetID.Should().BeEquivalentTo(original.AssetID);

                // Updated fields
                updated.Name.Should().Be("Updated Asset Name");
                updated.Description.Should().Be("Updated description");
                updated.HardwareVersion.Should().Be("HW2.0");
                updated.MacAddress.Should().BeNullOrEmpty();

                // Location changes
                updated.Location.Should().NotBeNull();
                updated.Location.ParentAsset.Should().Be(original.Location.ParentAsset);
                updated.Location.RoomId.Should().Be(original.Location.RoomId);
                updated.Location.RackId.Should().Be(original.Location.RackId);
                updated.Location.RackPosition.Should().Be(12);
                updated.Location.ContainerId.Should().Be(original.Location.ContainerId);
                updated.Location.DeskId.Should().Be(original.Location.DeskId);
                updated.Location.Side.Should().Be(SlcAsset_Management.Enums.SideEnum.Front);

                // Ownership changes
                updated.Ownership.Should().NotBeNull();
                updated.Ownership.Organization.Should().NotBe(original.Ownership.Organization);
                updated.Ownership.ContactPerson.Should().Be(Guid.Empty);
                updated.Ownership.ContactPersonRole.Should().Be(Guid.Empty);
                updated.Ownership.Team.Should().Be(Guid.Empty);

                // Custody changes
                updated.Custody.Should().NotBeNull();
                updated.Custody.ContactPerson.Should().NotBe(original.Custody.ContactPerson);
                updated.Custody.Team.Should().NotBe(original.Custody.Team);
                updated.Custody.Organization.Should().NotBe(original.Custody.Organization);
                updated.Custody.ContactPersonRole.Should().NotBe(original.Custody.ContactPersonRole);

                // Collections
                updated.Holders.Should().BeEmpty();
                updated.ElementLinks.Should().HaveCount(1);
                updated.ElementLinks[0].ElementID.Should().Be("100546/34");
            }
        }

        private void AssertCreated(IAssetManagementApiHelper helper)
        {
            using (new AssertionScope())
            {
                Helper.AssetManagement.Assets.Count(new TRUEFilterElement<Asset>()).Should().Be(1);

                var created = Helper.AssetManagement.Assets.Read(new TRUEFilterElement<Asset>()).First();

                // Basic properties
                created.Should().NotBeNull();
                created.Name.Should().Be("Test Asset");
                created.Description.Should().Be("Sample asset for unit test");
                created.HardwareVersion.Should().Be("HW1.0");
                created.MacAddress.Should().NotBeNull();

                // Lifecycle dates
                created.PurchaseDate.Should().BeBefore(created.EndOfWarrantyDate.Value);
                created.PurchaseDate.Should().BeBefore(created.InstallationDate.Value);
                created.FirstUseDate.Should().BeBefore(created.EndOfLifeDate.Value);
                created.EndOfLifeDate.Should().BeAfter(created.FirstUseDate.Value);

                // Ownership
                created.Ownership.Should().NotBeNull();
                created.Ownership.Organization.Should().NotBe(Guid.Empty);

                // Custody
                created.Custody.Should().NotBeNull();
                created.Custody.From.Should().BeBefore(created.Custody.Till.Value);

                // Holders
                created.Holders.Should().NotBeNull();
                created.Holders.Should().HaveCount(3);
                created.Holders[0].HierarchyRole.Should().Be(SlcAsset_Management.Enums.HierarchyRoleEnum.Chassis);
                created.Holders[0].SlotNumber.Should().Be(4);
                created.Holders[1].HierarchyRole.Should().Be(SlcAsset_Management.Enums.HierarchyRoleEnum.Card);
                created.Holders[1].SlotNumber.Should().Be(1);
                created.Holders[2].HierarchyRole.Should().Be(SlcAsset_Management.Enums.HierarchyRoleEnum.Fan);
                created.Holders[2].SlotNumber.Should().Be(3);

                // Element Links
                created.ElementLinks.Should().HaveCount(2);
                created.ElementLinks[0].ElementID.Should().Be("123/456");
                created.ElementLinks[0].IsPrimary.Should().BeFalse();
                created.ElementLinks[1].ElementID.Should().Be("1845/2");
                created.ElementLinks[1].IsPrimary.Should().BeFalse();
            }
        }

        #endregion
    }
}