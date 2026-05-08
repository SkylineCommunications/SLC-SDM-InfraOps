namespace SDM.AssetManagement.Tests
{
    using FluentAssertions;
    using FluentAssertions.Execution;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SDM.AssetManagement.Tests.Setup;

    using SharedMappers.DomIds;
    //using Skyline.DataMiner.Analytics.GenericInterface.JoinFilter;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement;
    using Skyline.DataMiner.SDM.AssetManagement.Helpers;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;

    [TestClass]
    public partial class AssetDomStorageProviderTests
    {
        private Asset referenceAsset;

        [TestInitialize]
        public void Init()
        {
            Guid assetId = Guid.NewGuid();
            referenceAsset = new Asset
            {
                Identifier = assetId.ToString(),
                AssetID = assetId.ToString(),
                Name = "Test Asset",
                AssetClassId = null, // Set if you have a valid AssetClass reference
                Description = "Sample asset for unit test",
                FW_OS = "FW1.0",
                SerialNumber = "SN123456",
                HardwareVersion = "HW1.0",
                MacAddress = "00-14-22-01-23-45",
                Location = new AssetLocation
                {
                    // Set ParentAsset if needed
                    ParentAsset = new SdmObjectReference<Asset>(assetId.ToString()),
                    RoomId = new SdmObjectReference<Room>(Convert.ToString(Guid.NewGuid())),
                    RackId = new SdmObjectReference<Rack>(Convert.ToString(Guid.NewGuid())),
                    RackPosition = 6,
                    ContainerId = new SdmObjectReference<Facility>(Convert.ToString(Guid.NewGuid())),
                    DeskId = Guid.NewGuid(),
                    Side = SlcAsset_Management.Enums.SideEnum.Back,
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
                Holders =
                [
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
                ],
                ElementLinks =
                [
                    new ElementLink
                    {
                        ElementID = "123/456",
                        IsPrimary = false,
                    },
                    new ElementLink
                    {
                        ElementID = "1845/2",
                    },
                ],
            };
        }

        [TestMethod]
        public void AssetDomStorageProvider_EmptyDOM_Create()
        {
            var helper = RepositoryInitialize.InitializeEmptyRepositories();

            helper.Assets.Create(referenceAsset);

            AssertCreated(helper);
        }

        [TestMethod]
        public void AssetDomStorageProvider_EmptyDOM_CreateOrUpdate_Create()
        {
            var helper = RepositoryInitialize.InitializeEmptyRepositories();
            helper.Assets.CreateOrUpdate([referenceAsset]);

            AssertCreated(helper);
        }

        [TestMethod]
        public void AssetDomStorageProvider_EmptyDOM_CreateOrUpdate_Update()
        {
            var helper = RepositoryInitialize.InitializeEmptyRepositories();
            helper.Assets.Create(referenceAsset);

            var updatedAsset = new Asset
            {
                Identifier = referenceAsset.Identifier,
                AssetID = referenceAsset.AssetID, // using the same ID
                Name = "Updated Asset Name",
                Description = "Updated description",
                HardwareVersion = "HW2.0",
                MacAddress = null, // MAC Address is removed
                Location = new AssetLocation // most of the properties were changed here
                {
                    ParentAsset = referenceAsset.Location.ParentAsset,
                    RoomId = referenceAsset.Location.RoomId,
                    RackId = referenceAsset.Location.RackId,
                    RackPosition = 12,
                    ContainerId = referenceAsset.Location.ContainerId,
                    DeskId = referenceAsset.Location.DeskId,
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
                ElementLinks =
                [
                    new ElementLink
                    {
                       ElementID = "100546/34",
                    },
                ],
            };

            helper.Assets.CreateOrUpdate([updatedAsset]);

            AssertAssetUpdateDifferences(referenceAsset, updatedAsset);
        }

        [TestMethod]
        public void AssetDomStorageProvider_ReadPaged()
        {
            const int pageCount = 2;
            var helper = RepositoryInitialize.InitializeEmptyRepositories();
            helper.PopulateAssets();

            FilterElement<Asset> allFilter = new TRUEFilterElement<Asset>();
            var pagedResult = helper.Assets.ReadPaged(allFilter, pageCount);
            var assetCount = helper.Assets.Count(allFilter);

            using (new AssertionScope())
            {
                pagedResult.Should().NotBeNull();
                pagedResult.Should().HaveCount((int)(assetCount / pageCount));
                pagedResult.Should().AllSatisfy(page => page.Should().HaveCount(pageCount));
            }
        }

        [TestMethod]
        public void AssetDomStorageProvider_DeleteBulk()
        {
            var helper = RepositoryInitialize.InitializeEmptyRepositories();
            helper.PopulateAssets();

            var filter = new ORFilterElement<Asset>(
                AssetExposers.AssetName.Equal("Test Asset 3"),
                AssetExposers.AssetDescription.Equal("Sample asset 7"));
            var assetToDelete = helper.Assets.Read(filter);

            helper.Assets.Delete(assetToDelete);

            using (new AssertionScope())
            {
                helper.Assets.Count(new TRUEFilterElement<Asset>()).Should().Be(DemoData.Assets.Count - 2); // 8 records
                helper.Assets.Count(AssetExposers.AssetName.Equal("Test Asset 3")).Should().Be(0);
                helper.Assets.Count(AssetExposers.AssetDescription.Equal("Sample asset 7")).Should().Be(0);
            }
        }

        [TestMethod]
        public void AssetDomStorageProvider_EmptyDOM_DeleteSingle()
        {
            var helper = RepositoryInitialize.InitializeEmptyRepositories();
            helper.PopulateAssets();

            var assetToDelete = helper.Assets.Read(AssetExposers.AssetName.Equal("Test Asset 3")).First();

            helper.Assets.Delete(assetToDelete);

            helper.Assets.Count(new TRUEFilterElement<Asset>()).Should().Be(DemoData.Assets.Count - 1);
            helper.Assets.Count(AssetExposers.AssetId.Equal(assetToDelete.AssetID)).Should().Be(0);
        }

        private static void AssertAssetUpdateDifferences(Asset original, Asset updated)
        {
            using (new AssertionScope())
            {
                updated.AssetID.Should().BeEquivalentTo(original.AssetID);

                // AssetName
                updated.Name.Should().NotBe(original.Name);
                updated.Name.Should().Be("Updated Asset Name");

                // AssetDescription
                updated.Description.Should().NotBe(original.Description);
                updated.Description.Should().Be("Updated description");

                // HardwareVersion
                updated.HardwareVersion.Should().NotBe(original.HardwareVersion);
                updated.HardwareVersion.Should().Be("HW2.0");

                // NetworkDetails
                updated.MacAddress.Should().BeNullOrEmpty();

                // Location
                updated.Location.Should().NotBeNull();
                updated.Location.ParentAsset.Should().Be(original.Location.ParentAsset);
                updated.Location.RoomId.Should().Be(original.Location.RoomId);
                updated.Location.RackId.Should().Be(original.Location.RackId);
                updated.Location.RackPosition.Should().Be(12);
                updated.Location.ContainerId.Should().Be(original.Location.ContainerId);
                updated.Location.DeskId.Should().Be(original.Location.DeskId);
                updated.Location.Side.Should().Be(SlcAsset_Management.Enums.SideEnum.Front);

                // Ownership
                updated.Ownership.Should().NotBeNull();
                updated.Ownership.Organization.Should().NotBe(original.Ownership.Organization);
                updated.Ownership.ContactPerson.Should().Be(Guid.Empty);
                updated.Ownership.ContactPersonRole.Should().Be(Guid.Empty);
                updated.Ownership.Team.Should().Be(Guid.Empty);

                // Custody
                updated.Custody.Should().NotBeNull();
                updated.Custody.ContactPerson.Should().NotBe(original.Custody.ContactPerson);
                updated.Custody.Team.Should().NotBe(original.Custody.Team);
                updated.Custody.Organization.Should().NotBe(original.Custody.Organization);
                updated.Custody.ContactPersonRole.Should().NotBe(original.Custody.ContactPersonRole);

                // Holders
                updated.Holders.Should().BeEmpty();

                updated.ElementLinks.Should().HaveCount(1);
                updated.ElementLinks[0].ElementID.Should().Be("100546/34");
            }
        }

        private void AssertCreated(IAssetManagementApiHelper helper)
        {
            using (new AssertionScope())
            {
                helper.Assets.Count(new TRUEFilterElement<Asset>()).Should().Be(1);

                var createdAsset = helper.Assets.Read(new TRUEFilterElement<Asset>()).First();
                createdAsset.Should().NotBeNull();
                createdAsset.Name.Should().Be("Test Asset");
                createdAsset.Description.Should().Be("Sample asset for unit test");

                createdAsset.HardwareVersion.Should().Be("HW1.0");
                createdAsset.MacAddress.Should().NotBeNull();

                createdAsset.Location.Should().NotBeNull();
                createdAsset.Location.ParentAsset.Should().BeAssignableTo<SdmObjectReference<Asset>>();
                createdAsset.Location.Side.Should().Be(SlcAsset_Management.Enums.SideEnum.Back);

                createdAsset.Should().NotBeNull();
                createdAsset.PurchaseDate.Should().BeBefore(createdAsset.EndOfWarrantyDate.Value);
                createdAsset.PurchaseDate.Should().BeBefore(createdAsset.InstallationDate.Value);
                createdAsset.FirstUseDate.Should().BeBefore(createdAsset.EndOfLifeDate.Value);
                createdAsset.EndOfLifeDate.Should().BeAfter(createdAsset.FirstUseDate.Value);

                createdAsset.Ownership.Should().NotBeNull();
                createdAsset.Ownership.Organization.Should().NotBe(Guid.Empty);

                createdAsset.Custody.Should().NotBeNull();
                createdAsset.Custody.From.Should().BeBefore(createdAsset.Custody.Till.Value);

                createdAsset.Holders.Should().NotBeNull();
                createdAsset.Holders.Should().NotBeEmpty();

                createdAsset.Holders[0].HierarchyRole.Should().Be(SlcAsset_Management.Enums.HierarchyRoleEnum.Chassis);
                createdAsset.Holders[0].SlotNumber.Should().Be(4);

                createdAsset.Holders[1].HierarchyRole.Should().Be(SlcAsset_Management.Enums.HierarchyRoleEnum.Card);
                createdAsset.Holders[1].SlotNumber.Should().Be(1);

                createdAsset.Holders[2].HierarchyRole.Should().Be(SlcAsset_Management.Enums.HierarchyRoleEnum.Fan);
                createdAsset.Holders[2].SlotNumber.Should().Be(3);

                createdAsset.ElementLinks.Should().HaveCount(2);
                createdAsset.ElementLinks[0].ElementID.Should().Be("123/456");
                createdAsset.ElementLinks[0].IsPrimary.Should().BeFalse();

                createdAsset.ElementLinks[1].ElementID.Should().Be("1845/2");
                createdAsset.ElementLinks[1].IsPrimary.Should().BeFalse();
            }
        }
    }
}