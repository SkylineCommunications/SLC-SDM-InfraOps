namespace SDM.AssetManagement.Tests.Assets
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using FluentAssertions.Execution;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SDM.AssetManagement.Tests.Setup;
    using SharedMappers.DomIds;
    using Moq;
    using Skyline.DataMiner.Net;
    using Skyline.DataMiner.Net.Messages;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.Net.ManagerStore;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences;
    using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

    /// <summary>
    /// CRUD tests for Asset repository operations.
    /// </summary>
    [TestClass]
    public class AssetDomStorageProvider_CRUDTests : BaseRepositoryTest
    {
        private Asset referenceAsset = null!;

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
                InstallationUserId = new PnoObjectReference<Person>(Guid.NewGuid()),
                ModificationDate = DateTime.UtcNow,
                ModificationUserId = new PnoObjectReference<Person>(Guid.NewGuid()),
                EndOfLifeDate = DateTime.UtcNow.AddYears(5),
                Ownership =
                {
                    Organization = new PnoObjectReference<Organization>(Guid.NewGuid()),
                    ContactPerson = new PnoObjectReference<Person>(Guid.NewGuid()),
                    ContactPersonRole = new PnoObjectReference<Role>(Guid.NewGuid()),
                    Team = new PnoObjectReference<Team>(Guid.NewGuid()),
                },
                Custody =
                {
                    From = DateTime.UtcNow.AddMonths(-6),
                    Till = DateTime.UtcNow.AddMonths(6),
                    ContactPerson = new PnoObjectReference<Person>(Guid.NewGuid()),
                    Team = new PnoObjectReference<Team>(Guid.NewGuid()),
                    Organization = new PnoObjectReference<Organization>(Guid.NewGuid()),
                    ContactPersonRole = new PnoObjectReference<Role>(Guid.NewGuid()),
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
            AssertCreated();
        }

        [TestMethod]
        public void CreateOrUpdate_WithNewAsset_ShouldCreate()
        {
            // Arrange
            PrepareReferenceAssetWithAssetClass();

            // Act
            Helper.AssetManagement.Assets.CreateOrUpdate([referenceAsset]);

            // Assert
            AssertCreated();
        }

        [TestMethod]
        public void CreateOrUpdate_WithExistingAsset_ShouldUpdate()
        {
            // Arrange
            PrepareReferenceAssetWithAssetClass();
            var created = Helper.AssetManagement.Assets.Create(referenceAsset);

            var updatedAsset = new Asset
            {
                AssetClassId = created.AssetClassId,
                Identifier = created.Identifier,
                AssetID = created.AssetID,
                Name = "Updated Asset Name",
                Description = "Updated description",
                HardwareVersion = "HW2.0",
                MacAddress = null,
                PurchaseDate = DateTime.UtcNow.AddYears(-1),
                FirstUseDate = DateTime.UtcNow.AddMonths(-11),
                EndOfWarrantyDate = DateTime.UtcNow.AddYears(1),
                InstallationDate = DateTime.UtcNow.AddMonths(-10),
                InstallationUserId = new PnoObjectReference<Person>(Guid.NewGuid()),
                ModificationDate = DateTime.UtcNow,
                ModificationUserId = new PnoObjectReference<Person>(Guid.NewGuid()),
                EndOfLifeDate = DateTime.UtcNow.AddYears(5),
                Ownership =
                {
                    Organization = new PnoObjectReference<Organization>(Guid.NewGuid()),
                },
                Custody =
                {
                    From = DateTime.UtcNow.AddMonths(-6),
                    Till = DateTime.UtcNow.AddMonths(6),
                    ContactPerson = new PnoObjectReference<Person>(Guid.NewGuid()),
                    Team = new PnoObjectReference<Team>(Guid.NewGuid()),
                    Organization = new PnoObjectReference<Organization>(Guid.NewGuid()),
                    ContactPersonRole = new PnoObjectReference<Role>(Guid.NewGuid()),
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
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.Assets);

            var initialCount = Helper.TestData.Assets.Count;
            var assetToDelete = Helper.AssetManagement.Assets
                .Read(AssetExposers.AssetName.Equal("Test Asset 3"))
                .First();
            assetToDelete.State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable;

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
            assetsToDelete.ForEach(asset => asset.State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable);
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

        #region Regression Tests

        [TestMethod]
        public void Create_WithRoomOnlyLocation_ShouldPersistAndReadBackCorrectly()
        {
            // Arrange — asset with a Room location but no Rack or Container (Facility).
            // Regression: AssetDomRepository.FromInstance was reading _locationcontainer.Value
            // instead of _locationroom.Value, causing NullReferenceException when ContainerId was absent.
            PrepareReferenceAssetWithAssetClass();
            var roomId = Guid.NewGuid();
            var room = Helper.FacilityManagement.Rooms.Create(Helper.FacilityManagement.AttachFloor(new Room
            {
                Identifier = roomId.ToString(),
                RoomId = $"ROOM-{roomId}",
                Name = "Asset Location Room",
            }));
            referenceAsset.Location.RoomId = new SdmObjectReference<Room>(room.Identifier);

            // Act
            Helper.AssetManagement.Assets.Create(referenceAsset);
            var readBack = Helper.AssetManagement.Assets
                .Read(AssetExposers.AssetName.Equal(referenceAsset.Name))
                .Single();

            // Assert
            using (new AssertionScope())
            {
                readBack.Should().NotBeNull();
                readBack.Location.Should().NotBeNull();
                readBack.Location.RoomId.Should().NotBeNull();
                readBack.Location.RoomId.Identifier.Should().Be(room.Identifier);
                readBack.Location.RackId.HasValue().Should().BeFalse();
                readBack.Location.ContainerId.HasValue().Should().BeFalse();
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
                updated.Location.IsEmpty.Should().BeTrue();

                // Ownership changes
                updated.Ownership.Should().NotBeNull();
                updated.Ownership.Organization.Should().NotBe(original.Ownership.Organization);
                updated.Ownership.ContactPerson.Should().Be(new PnoObjectReference<Person>(Guid.Empty));
                updated.Ownership.ContactPersonRole.Should().Be(new PnoObjectReference<Role>(Guid.Empty));
                updated.Ownership.Team.Should().Be(new PnoObjectReference<Team>(Guid.Empty));

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

        #region Ported From Shared Tests

        private Asset NewMinimalAsset(string suffix)
        {
            Helper.PopulateWithDemoData(DemoDataLayer.AssetClasses);
            return new Asset
            {
                AssetID = $"PORTED-{suffix}",
                Name = $"Ported Asset {suffix}",
                AssetClassId = new SdmObjectReference<AssetClass>(Helper.TestData.AssetClasses.First().Identifier),
            };
        }

        private Asset ReloadAsset(string identifier)
        {
            return Helper.AssetManagement.Assets.Read(new TRUEFilterElement<Asset>()).Single(a => a.Identifier == identifier);
        }

        private Asset CopyForUpdate(Asset created)
        {
            return new Asset
            {
                Identifier = created.Identifier,
                AssetID = created.AssetID,
                Name = created.Name,
                AssetClassId = created.AssetClassId,
            };
        }

        [TestMethod]
        public void OperationalFlags_OnNewAsset_ShouldBeEmpty()
        {
            var asset = new Asset();

            asset.OperationalFlags.Should().NotBeNull();
            asset.OperationalFlags.Should().BeEmpty();
        }

        [TestMethod]
        public void OperationalFlags_WithEmptyFlags_ShouldStayEmptyAfterSaveAndReload()
        {
            var asset = NewMinimalAsset("FLAG-2");

            var created = Helper.AssetManagement.Assets.Create(asset);
            var reloaded = ReloadAsset(created.Identifier);

            reloaded.OperationalFlags.Should().BeEmpty();
        }

        [TestMethod]
        public void OperationalFlags_SaveAndReload_FlagSurvives()
        {
            var asset = NewMinimalAsset("FLAG-3");
            asset.OperationalFlags.Add(SlcAsset_Management.Enums.Operationalflagsenum.Faulty);

            var created = Helper.AssetManagement.Assets.Create(asset);
            var reloaded = ReloadAsset(created.Identifier);

            reloaded.Should().NotBeNull();
            reloaded.OperationalFlags.Should().Contain(SlcAsset_Management.Enums.Operationalflagsenum.Faulty);
        }

        [TestMethod]
        public void OperationalFlags_AddFlagToExistingAsset_StoresInt32ListMatchingFieldDefinition()
        {
            var created = Helper.AssetManagement.Assets.Create(NewMinimalAsset("FLAG-5"));

            var toUpdate = ReloadAsset(created.Identifier);
            toUpdate.OperationalFlags.Add(SlcAsset_Management.Enums.Operationalflagsenum.Faulty);

            // The in-memory DOM mock normalizes list values on read, so inspect what the repository sends.
            // DomHelper and the in-memory DOM normalize list values, so inspect the instance the repository builds.
            var toInstance = typeof(AssetDomRepository).GetMethod("ToInstance", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var instance = (DomInstance)toInstance.Invoke(new AssetDomRepository(Helper.Connection), new object[] { toUpdate });

            // The operational flags field is an Int32 generic enum; the DOM rejects any other element type.
            instance.Sections.SelectMany(s => s.FieldValues)
                .Single(f => f.FieldDescriptorID.Equals(SlcAsset_Management.Sections.AssetInformation.OperationalFlags))
                .Value.Type.Should().Be(typeof(List<int>));
        }

        [TestMethod]
        public void OperationalFlags_SaveWithFlag_ClearAndSaveAgain_FlagGone()
        {
            var asset = NewMinimalAsset("FLAG-4");
            asset.OperationalFlags.Add(SlcAsset_Management.Enums.Operationalflagsenum.Faulty);
            var created = Helper.AssetManagement.Assets.Create(asset);

            var reloaded = ReloadAsset(created.Identifier);
            reloaded.OperationalFlags.Clear();
            Helper.AssetManagement.Assets.CreateOrUpdate([reloaded]);

            ReloadAsset(created.Identifier).OperationalFlags.Should().BeEmpty();
        }

        [TestMethod]
        public void SerialNumber_HardwareVersion_MacAddress_ShouldPersistAcrossSaveReloadAndUpdate()
        {
            var asset = NewMinimalAsset("NET-1");
            asset.SerialNumber = "SN-ROUNDTRIP-1";
            asset.HardwareVersion = "HW-1.0";
            asset.MacAddress = "AA-BB-CC-DD-EE-01";

            var created = Helper.AssetManagement.Assets.Create(asset);
            var reloaded = ReloadAsset(created.Identifier);

            using (new AssertionScope())
            {
                reloaded.SerialNumber.Should().Be("SN-ROUNDTRIP-1");
                reloaded.HardwareVersion.Should().Be("HW-1.0");
                reloaded.MacAddress.Should().Be("AA-BB-CC-DD-EE-01");
            }

            var update = CopyForUpdate(created);
            update.SerialNumber = "SN-ROUNDTRIP-2";
            update.HardwareVersion = "HW-2.0";
            update.MacAddress = "AA-BB-CC-DD-EE-02";
            Helper.AssetManagement.Assets.CreateOrUpdate([update]);
            var updated = ReloadAsset(created.Identifier);

            using (new AssertionScope())
            {
                updated.SerialNumber.Should().Be("SN-ROUNDTRIP-2");
                updated.HardwareVersion.Should().Be("HW-2.0");
                updated.MacAddress.Should().Be("AA-BB-CC-DD-EE-02");
            }
        }

        #endregion

        private void AssertCreated()
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

                created.EndOfWarrantyDate.Should().NotBe(null);
                created.InstallationDate.Should().NotBe(null);
                created.EndOfLifeDate.Should().NotBe(null);
                created.FirstUseDate.Should().NotBe(null);
                created.PurchaseDate.Should().BeBefore(created.EndOfWarrantyDate.Value);
                created.PurchaseDate.Should().BeBefore(created.InstallationDate.Value);
                created.FirstUseDate.Should().BeBefore(created.EndOfLifeDate.Value);
                created.EndOfLifeDate.Should().BeAfter(created.FirstUseDate.Value);

                // Ownership
                created.Ownership.Should().NotBeNull();
                created.Ownership.Organization.Should().NotBe(Guid.Empty);

                // Custody
                created.Custody.Should().NotBeNull();
                created.Custody.Till.Should().NotBeNull();
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